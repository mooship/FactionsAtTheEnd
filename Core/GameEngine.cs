using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;
using FluentValidation;
using Spectre.Console;

namespace FactionsAtTheEnd.Core;

/// <summary>
/// Main game engine for Factions at the End. Coordinates game state, turn logic, event processing, and achievement tracking.
/// </summary>
public class GameEngine(
    IEventService eventService,
    IFactionService factionService,
    IGameDataService gameDataService,
    IValidator<PlayerAction> playerActionValidator,
    IValidator<GameEvent> gameEventValidator,
    IValidator<EventChoice> eventChoiceValidator,
    IGlobalAchievementService globalAchievementService,
    IAppLogger logger
)
{
    private readonly IEventService _eventService = eventService;
    private readonly IFactionService _factionService = factionService;
    private readonly IGameDataService _gameDataService = gameDataService;
    private readonly IValidator<PlayerAction> _playerActionValidator = playerActionValidator;
    private readonly IValidator<GameEvent> _gameEventValidator = gameEventValidator;
    private readonly IValidator<EventChoice> _eventChoiceValidator = eventChoiceValidator;
    private readonly IGlobalAchievementService _globalAchievementService = globalAchievementService;
    private readonly IAppLogger _logger = logger;

    /// <summary>
    /// The current game state, or null if no game is loaded.
    /// </summary>
    public GameState? CurrentGame { get; private set; }

    /// <summary>
    /// Starts a new game session with the specified player faction and initializes the first crisis event.
    /// </summary>
    /// <param name="playerFactionName">The name of the player's faction.</param>
    /// <param name="playerFactionType">The type of the player's faction.</param>
    /// <returns>The initialized <see cref="GameState"/>.</returns>
    public async Task<GameState> CreateNewGameAsync(
        string playerFactionName,
        FactionType playerFactionType
    )
    {
        Guard.IsNotNullOrWhiteSpace(playerFactionName, nameof(playerFactionName));
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), playerFactionType),
            nameof(playerFactionType) + " must be a valid FactionType."
        );
        _logger.Debug(
            "Starting CreateNewGameAsync for {FactionName} ({FactionType})",
            playerFactionName,
            playerFactionType
        );
        try
        {
            _logger.Information(
                "Creating new game for faction: {FactionName} ({FactionType})",
                playerFactionName,
                playerFactionType
            );
            try
            {
                Guard.IsNotNullOrWhiteSpace(playerFactionName, nameof(playerFactionName));
                Guard.IsTrue(
                    Enum.IsDefined(typeof(FactionType), playerFactionType),
                    nameof(playerFactionType)
                );

                var playerFaction = _factionService.CreateFaction(
                    playerFactionName,
                    playerFactionType,
                    true
                );
                Guard.IsNotNull(playerFaction, nameof(playerFaction));
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                var safeFactionName = string.Join(
                    "",
                    playerFactionName.Split(Path.GetInvalidFileNameChars())
                );
                var gameState = new GameState
                {
                    PlayerFaction = playerFaction,
                    SaveName = $"{safeFactionName}_{timestamp}",
                    CurrentCycle = 1,
                };
                Guard.IsNotNull(gameState, nameof(gameState));

                var initialEvents = new List<GameEvent>
                {
                    new()
                    {
                        Title = EventTemplates.InitialCrisisTitle,
                        Description = EventTemplates.InitialCrisisDescription,
                        Type = EventType.Crisis,
                        Cycle = gameState.CurrentCycle,
                    },
                };
                foreach (var gameEvent in initialEvents)
                {
                    Guard.IsNotNull(gameEvent, nameof(gameEvent));
                    var validationResult = _gameEventValidator.Validate(gameEvent);
                    if (!validationResult.IsValid)
                    {
                        _logger.Error(
                            "Invalid initial event: {Errors}",
                            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                        );
                        throw new ArgumentException(
                            $"Invalid initial event: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}"
                        );
                    }
                    if (gameEvent.Choices != null)
                    {
                        foreach (var choice in gameEvent.Choices)
                        {
                            Guard.IsNotNull(choice, nameof(choice));
                            var choiceValidation = _eventChoiceValidator.Validate(choice);
                            if (!choiceValidation.IsValid)
                            {
                                _logger.Error(
                                    "Invalid event choice: {Errors}",
                                    string.Join(
                                        ", ",
                                        choiceValidation.Errors.Select(e => e.ErrorMessage)
                                    )
                                );
                                throw new ArgumentException(
                                    $"Invalid event choice: {string.Join(", ", choiceValidation.Errors.Select(e => e.ErrorMessage))}"
                                );
                            }
                        }
                    }
                }
                gameState.RecentEvents.AddRange(initialEvents);
                _logger.Debug(
                    "Initial events created for new game: {EventCount}",
                    initialEvents.Count
                );

                CurrentGame = gameState;
                Guard.IsNotNull(gameState, nameof(gameState));
                await _gameDataService.SaveGameAsync(gameState);
                _logger.Information("New game created and saved: {SaveName}", gameState.SaveName);
                return gameState;
            }
            catch (ArgumentException argEx)
            {
                _logger.Warning(
                    "Validation failed during new game creation for {FactionName}: {Error}",
                    playerFactionName,
                    argEx.Message
                );
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Failed to create new game for faction: {FactionName}",
                    playerFactionName
                );
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Failed to create new game for faction: {FactionName}",
                playerFactionName
            );
            throw;
        }
    }

    /// <summary>
    /// Retrieves all saved games from persistent storage, ordered by most recently played.
    /// </summary>
    /// <returns>A list of saved <see cref="GameState"/> objects.</returns>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        _logger.Debug("Retrieving all saved games.");
        Guard.IsNotNull(_gameDataService, nameof(_gameDataService));
        try
        {
            var games = await _gameDataService.GetSavedGamesAsync();
            Guard.IsNotNull(games, nameof(games));
            _logger.Information("Retrieved {Count} saved games.", games.Count);
            return games;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameEngine] Error retrieving saved games");
            return [];
        }
    }

    /// <summary>
    /// Loads a saved game by its unique ID and sets it as the current session.
    /// </summary>
    /// <param name="gameId">The ID of the game to load.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadGameAsync(string gameId)
    {
        _logger.Information("Loading game with ID: {GameId}", gameId);
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        try
        {
            var game = await _gameDataService.LoadGameAsync(gameId);
            if (game == null)
            {
                _logger.Warning("No game found with ID: {GameId}", gameId);
                throw new ArgumentException($"No saved game found with ID: {gameId}");
            }
            CurrentGame = game;
            _logger.Information("Game loaded successfully: {SaveName}", game.SaveName);
        }
        catch (ArgumentException argEx)
        {
            _logger.Warning(
                "User error loading game with ID: {GameId}: {Error}",
                gameId,
                argEx.Message
            );
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameEngine] Error loading game with ID: {GameId}", gameId);
            throw;
        }
    }

    /// <summary>
    /// Processes a full turn: validates actions, updates the world, generates and applies events, saves progress, checks win/loss, and advances the cycle.
    /// </summary>
    /// <param name="playerActions">The actions taken by the player this turn.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessTurnAsync(List<PlayerAction> playerActions)
    {
        _logger.Debug(
            "Processing turn with {ActionCount} player actions.",
            playerActions?.Count ?? 0
        );
        Guard.IsNotNull(playerActions, nameof(playerActions));
        Guard.IsTrue(playerActions.Count > 0, "At least one player action must be provided.");
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        try
        {
            var (validActions, actionCounts) = ValidateAndCountActions(playerActions);
            if (validActions.Count == 0)
            {
                _logger.Warning("No valid player actions provided for this turn.");
            }
            UpdateActionCounts(actionCounts);
            await ApplyPlayerActionsAsync(validActions);
            await UpdateWorldStateAsync();
            var newEvents = (await _eventService.GenerateRandomEventsAsync(CurrentGame)).ToList();
            CurrentGame.RecentEvents.AddRange(newEvents);
            _logger.Information("{EventCount} new events generated this turn.", newEvents.Count);
            var newNews = _eventService.GenerateGalacticNews(CurrentGame, newEvents);
            if (newNews.Count != 0)
            {
                CurrentGame.GalacticNews.AddRange(newNews);
                CurrentGame.GalacticNews =
                [
                    .. CurrentGame.GalacticNews.Skip(
                        Math.Max(0, CurrentGame.GalacticNews.Count - 15)
                    ),
                ];
                _logger.Debug("Added {NewsCount} new galactic news items.", newNews.Count);
            }
            if (newEvents.Count > 0)
            {
                ApplyEventEffects(newEvents);
            }
            await _gameDataService.SaveGameAsync(CurrentGame);
            _logger.Information(
                "Turn processed and game saved. Cycle: {Cycle}",
                CurrentGame.CurrentCycle
            );
            CheckWinLoseConditions();
            CurrentGame.CurrentCycle++;
        }
        catch (ArgumentException argEx)
        {
            _logger.Warning("Validation failed during turn processing: {Error}", argEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameEngine] Error processing turn");
            throw;
        }
    }

    /// <summary>
    /// Validates and counts player actions, returning only valid actions and a count by action type.
    /// </summary>
    /// <param name="playerActions">The actions to validate and count.</param>
    /// <returns>A tuple containing valid actions and their type counts.</returns>
    private (
        List<PlayerAction> validActions,
        Dictionary<PlayerActionType, int> actionCounts
    ) ValidateAndCountActions(List<PlayerAction> playerActions)
    {
        Guard.IsNotNull(playerActions, nameof(playerActions));
        Guard.IsTrue(playerActions.Count > 0, "At least one player action must be provided.");
        var validActions = new List<PlayerAction>();
        var actionCounts = new Dictionary<PlayerActionType, int>();
        foreach (var action in playerActions)
        {
            var validationResult = _playerActionValidator.Validate(action);
            if (!validationResult.IsValid)
            {
                _logger.Warning(
                    "Invalid player action: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                );
                continue;
            }
            validActions.Add(action);
            if (!actionCounts.TryGetValue(action.ActionType, out int value))
            {
                value = 0;
                actionCounts[action.ActionType] = value;
            }
            actionCounts[action.ActionType] = ++value;
        }
        return (validActions, actionCounts);
    }

    /// <summary>
    /// Updates rolling counts for recently performed actions, decaying unused ones over time.
    /// </summary>
    /// <param name="actionCounts">The counts of each action type performed this turn.</param>
    private void UpdateActionCounts(Dictionary<PlayerActionType, int> actionCounts)
    {
        Guard.IsNotNull(actionCounts, nameof(actionCounts));
        Guard.IsTrue(actionCounts.Count > 0, "At least one action count must be provided.");
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        foreach (var key in actionCounts.Keys)
        {
            if (!CurrentGame.RecentActionCounts.ContainsKey(key))
            {
                CurrentGame.RecentActionCounts[key] = 0;
            }
            CurrentGame.RecentActionCounts[key] += actionCounts[key];
        }

        var keys = CurrentGame.RecentActionCounts.Keys.ToList();
        foreach (var key in keys)
        {
            if (!actionCounts.ContainsKey(key))
            {
                CurrentGame.RecentActionCounts[key] = Math.Max(
                    0,
                    CurrentGame.RecentActionCounts[key] - 1
                );
            }
        }
    }

    /// <summary>
    /// Applies all valid player actions to the player's faction, updating stats and state.
    /// </summary>
    /// <param name="validActions">The validated actions to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ApplyPlayerActionsAsync(List<PlayerAction> validActions)
    {
        Guard.IsNotNull(validActions, nameof(validActions));
        foreach (var action in validActions)
        {
            await ProcessPlayerActionAsync(action);
        }
    }

    /// <summary>
    /// Applies effects from new game events and updates blocked actions for the next turn.
    /// </summary>
    /// <param name="newEvents">The events that occurred this turn.</param>
    private void ApplyEventEffects(List<GameEvent> newEvents)
    {
        Guard.IsNotNull(newEvents, nameof(newEvents));
        Guard.IsTrue(newEvents.Count > 0, "At least one event must be provided.");
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        var player = CurrentGame.PlayerFaction;
        CurrentGame.BlockedActions?.Clear();
        foreach (var gameEvent in newEvents)
        {
            var validationResult = _gameEventValidator.Validate(gameEvent);
            if (!validationResult.IsValid)
            {
                _logger.Error(
                    "Invalid event during ApplyEventEffects: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                );
                throw new ValidationException(validationResult.Errors);
            }
            if (gameEvent.Choices != null)
            {
                foreach (var choice in gameEvent.Choices)
                {
                    var choiceResult = _eventChoiceValidator.Validate(choice);
                    if (!choiceResult.IsValid)
                    {
                        _logger.Error(
                            "Invalid event choice during ApplyEventEffects: {Errors}",
                            string.Join(", ", choiceResult.Errors.Select(e => e.ErrorMessage))
                        );
                        throw new ValidationException(choiceResult.Errors);
                    }
                }
            }

            Guard.IsNotNull(player, nameof(player));
            foreach (var effect in gameEvent.Effects)
            {
                switch (effect.Key)
                {
                    case StatKey.Population:
                        player.Population += effect.Value;
                        break;
                    case StatKey.Military:
                        player.Military += effect.Value;
                        break;
                    case StatKey.Technology:
                        player.Technology += effect.Value;
                        break;
                    case StatKey.Influence:
                        player.Influence += effect.Value;
                        break;
                    case StatKey.Resources:
                        player.Resources += effect.Value;
                        break;
                    case StatKey.Stability:
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += effect.Value;
                        break;
                    case StatKey.Reputation:
                        player.Reputation += effect.Value;
                        break;
                }
            }

            player.UpdateStatus();

            if (gameEvent.BlockedActions != null && CurrentGame.BlockedActions != null)
            {
                foreach (var blocked in gameEvent.BlockedActions)
                {
                    if (!CurrentGame.BlockedActions.Contains(blocked))
                    {
                        CurrentGame.BlockedActions.Add(blocked);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks and updates win/lose conditions, unlocking achievements as needed.
    /// </summary>
    private void CheckWinLoseConditions()
    {
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        var player = CurrentGame.PlayerFaction;
        Guard.IsNotNull(player);

        if (CurrentGame.CurrentCycle > 20 || player.Technology >= 100)
        {
            CurrentGame.HasWon = true;
            if (!CurrentGame.Achievements.Contains(AchievementTemplates.Names.Victory))
            {
                CurrentGame.Achievements.Add(AchievementTemplates.Names.Victory);
                _globalAchievementService.UnlockAchievement(
                    AchievementTemplates.Names.Victory,
                    AchievementTemplates.Descriptions.Victory
                );
            }
            if (
                !_globalAchievementService.IsAchievementUnlocked(
                    AchievementTemplates.Names.FirstWin
                )
            )
            {
                _globalAchievementService.UnlockAchievement(
                    AchievementTemplates.Names.FirstWin,
                    AchievementTemplates.Descriptions.FirstWin
                );
            }
        }

        if (
            CurrentGame.CurrentCycle >= 20
            && !CurrentGame.Achievements.Contains(AchievementTemplates.Names.Survivor)
        )
        {
            CurrentGame.Achievements.Add(AchievementTemplates.Names.Survivor);
            _globalAchievementService.UnlockAchievement(
                AchievementTemplates.Names.Survivor,
                AchievementTemplates.Descriptions.Survivor
            );
        }

        if (
            player.Technology >= 100
            && !CurrentGame.Achievements.Contains(AchievementTemplates.Names.TechMaster)
        )
        {
            CurrentGame.Achievements.Add(AchievementTemplates.Names.TechMaster);
            _globalAchievementService.UnlockAchievement(
                AchievementTemplates.Names.TechMaster,
                AchievementTemplates.Descriptions.TechMaster
            );
        }

        if (
            player.Technology >= 100
            && !CurrentGame.Achievements.Contains(AchievementTemplates.Names.TechAscendant)
        )
        {
            CurrentGame.Achievements.Add(AchievementTemplates.Names.TechAscendant);
            _globalAchievementService.UnlockAchievement(
                AchievementTemplates.Names.TechAscendant,
                AchievementTemplates.Descriptions.TechAscendant
            );
        }

        if (
            CurrentGame.GalacticStability <= 0
            || player.Population <= 0
            || player.Resources <= 0
            || player.Stability <= 0
        )
        {
            CurrentGame.HasLost = true;
            if (!CurrentGame.Achievements.Contains(AchievementTemplates.Names.Defeat))
            {
                CurrentGame.Achievements.Add(AchievementTemplates.Names.Defeat);
                _globalAchievementService.UnlockAchievement(
                    AchievementTemplates.Names.Defeat,
                    AchievementTemplates.Descriptions.Defeat
                );
            }
        }

        if (
            player.Reputation >= 100
            && !CurrentGame.Achievements.Contains(AchievementTemplates.Names.LegendaryReputation)
        )
        {
            CurrentGame.Achievements.Add(AchievementTemplates.Names.LegendaryReputation);
            _globalAchievementService.UnlockAchievement(
                AchievementTemplates.Names.LegendaryReputation,
                AchievementTemplates.Descriptions.LegendaryReputation
            );
        }
        if (
            player.Military >= 100
            && !CurrentGame.Achievements.Contains(AchievementTemplates.Names.Warlord)
        )
        {
            CurrentGame.Achievements.Add(AchievementTemplates.Names.Warlord);
            _globalAchievementService.UnlockAchievement(
                AchievementTemplates.Names.Warlord,
                AchievementTemplates.Descriptions.Warlord
            );
        }
    }

    /// <summary>
    /// Applies a single player action to the player's faction, updating stats accordingly.
    /// </summary>
    /// <param name="action">The player action to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessPlayerActionAsync(PlayerAction action)
    {
        Guard.IsNotNull(action, nameof(action));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        Guard.IsNotNull(CurrentGame.PlayerFaction, nameof(CurrentGame.PlayerFaction));
        var player = CurrentGame.PlayerFaction;

        await ApplyToFactionAsync(
            player.Id,
            async f =>
            {
                switch (action.ActionType)
                {
                    case PlayerActionType.BuildDefenses:
                        f.Military += 5;
                        f.Stability += 2;
                        break;
                    case PlayerActionType.RecruitTroops:
                        f.Military += 7;
                        f.Resources -= 3;
                        break;
                    case PlayerActionType.DevelopInfrastructure:
                        f.Resources += 5;
                        f.Stability += 2;
                        break;
                    case PlayerActionType.ExploitResources:
                        f.Resources += 8;
                        f.Stability -= 1;
                        break;
                    case PlayerActionType.MilitaryTech:
                        f.Technology += 4;
                        f.Military += 2;
                        break;
                    case PlayerActionType.EconomicTech:
                        f.Technology += 4;
                        f.Resources += 2;
                        break;
                    case PlayerActionType.AncientStudies:
                        f.Technology += 2;
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.AncientTechDiscovery += 5;
                        break;
                    case PlayerActionType.GateNetworkResearch:
                        f.Technology += 2;
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GateNetworkIntegrity += 3;
                        break;
                    case PlayerActionType.Diplomacy:
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += 3;
                        player.Influence += 2;
                        player.Reputation += 5;
                        break;
                    case PlayerActionType.Espionage:
                        player.Technology += 1;
                        player.Resources += 2;
                        break;
                    default:
                        break;
                }
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Applies an update function to the player's faction and clamps its stats to valid bounds.
    /// </summary>
    /// <param name="factionId">The ID of the faction to update (currently only the player).</param>
    /// <param name="update">An async function to modify the faction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ApplyToFactionAsync(string factionId, Func<Faction, Task> update)
    {
        Guard.IsNotNullOrWhiteSpace(factionId, nameof(factionId));
        Guard.IsNotNull(update, nameof(update));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        var faction = CurrentGame.PlayerFaction;
        Guard.IsNotNull(faction, nameof(faction));

        await update(faction);
        faction.ClampResources();
    }

    /// <summary>
    /// Updates global world state at the end of each turn (e.g., decay, discoveries, world events).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateWorldStateAsync()
    {
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        await Task.Run(() =>
        {
            CurrentGame.GalacticStability = Math.Max(
                0,
                CurrentGame.GalacticStability - Random.Shared.Next(0, 3)
            );
            CurrentGame.GateNetworkIntegrity = Math.Max(
                0,
                CurrentGame.GateNetworkIntegrity - Random.Shared.Next(0, 2)
            );

            if (Random.Shared.Next(1, 101) <= 15)
            {
                CurrentGame.AncientTechDiscovery = Math.Min(
                    100,
                    CurrentGame.AncientTechDiscovery + Random.Shared.Next(1, 5)
                );
            }
        });
    }

    /// <summary>
    /// Provides access to the game data service for save/load operations.
    /// </summary>
    public IGameDataService GameDataService => _gameDataService;

    /// <summary>
    /// Sets the current game state (for testing or custom setup scenarios).
    /// </summary>
    /// <param name="gameState">The <see cref="GameState"/> to set as current.</param>
    public void SetCurrentGame(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        CurrentGame = gameState;
    }

    /// <summary>
    /// Handles the logic for presenting and resolving multi-step event choices.
    /// </summary>
    /// <param name="initialChoice">The root EventChoice to present.</param>
    /// <returns>The final EventChoice selected by the player (leaf node).</returns>
    public static EventChoice? RunMultiStepChoice(EventChoice initialChoice)
    {
        Guard.IsNotNull(initialChoice, nameof(initialChoice));
        var currentChoice = initialChoice;
        while (currentChoice.NextStepChoices != null && currentChoice.NextStepChoices.Count > 0)
        {
            var options = currentChoice.NextStepChoices;
            var descriptions = options.Select((c, i) => $"[{i + 1}] {c.Description}").ToList();
            int selectedIndex = 0;
            do
            {
                AnsiConsole.MarkupLine("\n[bold]Choose:[/]");
                for (int i = 0; i < descriptions.Count; i++)
                    AnsiConsole.MarkupLine(descriptions[i]);
                var input = AnsiConsole.Ask<string>("[yellow]Enter choice number:[/]");
                if (int.TryParse(input, out int idx) && idx > 0 && idx <= options.Count)
                {
                    selectedIndex = idx - 1;
                    break;
                }
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid number.[/]");
            } while (true);
            currentChoice = options[selectedIndex];
        }
        return currentChoice;
    }
}
