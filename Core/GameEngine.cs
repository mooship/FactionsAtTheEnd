using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;
using FluentValidation;

namespace FactionsAtTheEnd.Core;

/// <summary>
/// Main game engine for Factions at the End. Handles game state, turn processing, and win/lose conditions.
/// </summary>
public class GameEngine(
    IEventService eventService,
    IFactionService factionService,
    IGameDataService gameDataService,
    IValidator<PlayerAction> playerActionValidator
)
{
    private readonly IEventService _eventService = eventService;
    private readonly IFactionService _factionService = factionService;
    private readonly IGameDataService _gameDataService = gameDataService;
    private readonly IValidator<PlayerAction> _playerActionValidator = playerActionValidator;

    /// <summary>
    /// Gets the current game state. This is null if no game is loaded or before a new game starts.
    /// </summary>
    public GameState? CurrentGame { get; private set; }

    /// <summary>
    /// Creates a new game session. Initializes a player faction and sets up the initial game state, including a starting crisis event.
    /// </summary>
    /// <param name="playerFactionName">The name of the player's faction.</param>
    /// <param name="playerFactionType">The type of the player's faction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created <see cref="GameState"/>.</returns>
    public async Task<GameState> CreateNewGameAsync(
        string playerFactionName,
        FactionType playerFactionType
    )
    {
        var playerFaction = _factionService.CreateFaction(
            playerFactionName,
            playerFactionType,
            true
        );
        var gameState = new GameState
        {
            PlayerFaction = playerFaction,
            SaveName = $"Game Started {DateTime.Now:yyyy-MM-dd HH:mm}",
            CurrentCycle = 1,
        };

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
        gameState.RecentEvents.AddRange(initialEvents);

        CurrentGame = gameState;
        await _gameDataService.SaveGameAsync(gameState);

        return gameState;
    }

    /// <summary>
    /// Retrieves all saved games from persistent storage, ordered with the most recently played first.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="GameState"/> objects.</returns>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        return await _gameDataService.GetSavedGamesAsync();
    }

    /// <summary>
    /// Loads a saved game into the current game session using its unique ID.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to load.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadGameAsync(string gameId)
    {
        var loaded = await _gameDataService.LoadGameAsync(gameId);
        Guard.IsNotNull(loaded, nameof(loaded));
        CurrentGame = loaded;
    }

    /// <summary>
    /// Processes a full turn in the game. This includes validating player actions, updating the world state, generating new events,
    /// applying event effects, saving the game, checking win/loss conditions, and advancing the game cycle.
    /// </summary>
    /// <param name="playerActions">A list of actions taken by the player during this turn.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ProcessTurnAsync(List<PlayerAction> playerActions)
    {
        Guard.IsNotNull(playerActions, nameof(playerActions));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        var (validActions, actionCounts) = ValidateAndCountActions(playerActions);
        UpdateActionCounts(actionCounts);
        await ApplyPlayerActionsAsync(validActions);
        await UpdateWorldStateAsync();
        var newEvents = (await _eventService.GenerateRandomEventsAsync(CurrentGame)).ToList();
        CurrentGame.RecentEvents.AddRange(newEvents);

        var newNews = _eventService.GenerateGalacticNews(CurrentGame, newEvents);
        if (newNews.Count != 0)
        {
            CurrentGame.GalacticNews.AddRange(newNews);
            CurrentGame.GalacticNews =
            [
                .. CurrentGame.GalacticNews.Skip(Math.Max(0, CurrentGame.GalacticNews.Count - 15)),
            ];
        }
        ApplyEventEffects(newEvents);
        await _gameDataService.SaveGameAsync(CurrentGame);
        CheckWinLoseConditions();
        CurrentGame.CurrentCycle++;
    }

    /// <summary>
    /// Validates a list of player actions and counts the occurrences of each action type. Invalid actions are filtered out.
    /// This count can be used for game mechanics like preventing action spam or triggering specific events.
    /// </summary>
    /// <param name="playerActions">The list of player actions to validate and count.</param>
    /// <returns>A tuple containing a list of valid actions and a dictionary mapping action types to their counts.</returns>
    private (
        List<PlayerAction> validActions,
        Dictionary<PlayerActionType, int> actionCounts
    ) ValidateAndCountActions(List<PlayerAction> playerActions)
    {
        Guard.IsNotNull(playerActions);
        var validActions = new List<PlayerAction>();
        var actionCounts = new Dictionary<PlayerActionType, int>();
        foreach (var action in playerActions)
        {
            var validationResult = _playerActionValidator.Validate(action);
            if (!validationResult.IsValid)
            {
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
    /// Updates the rolling counts of recently performed actions. This involves adding the counts from the current turn and then decaying the counts from previous turns.
    /// This mechanic can be used to influence event generation or impose temporary restrictions.
    /// </summary>
    /// <param name="actionCounts">A dictionary containing the counts of each action type performed in the current turn.</param>
    private void UpdateActionCounts(Dictionary<PlayerActionType, int> actionCounts)
    {
        Guard.IsNotNull(actionCounts, nameof(actionCounts));
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
            CurrentGame.RecentActionCounts[key] = Math.Max(
                0,
                CurrentGame.RecentActionCounts[key] - 1
            );
        }
    }

    /// <summary>
    /// Applies the effects of all valid player actions to the player's faction.
    /// </summary>
    /// <param name="validActions">A list of validated player actions to be applied.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ApplyPlayerActionsAsync(List<PlayerAction> validActions)
    {
        foreach (var action in validActions)
        {
            await ProcessPlayerActionAsync(action);
        }
    }

    /// <summary>
    /// Applies the effects of new game events to the player's faction and updates the list of blocked actions for the next turn.
    /// Effects can include changes to faction stats or global game parameters.
    /// </summary>
    /// <param name="newEvents">A list of new game events that occurred this turn.</param>
    private void ApplyEventEffects(List<GameEvent> newEvents)
    {
        Guard.IsNotNull(newEvents, nameof(newEvents));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        var player = CurrentGame.PlayerFaction;
        CurrentGame.BlockedActions?.Clear();

        foreach (var gameEvent in newEvents)
        {
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
    /// Checks if any win or lose conditions have been met by the player.
    /// Updates the game state (e.g., SaveName) if a condition is met.
    /// </summary>
    private void CheckWinLoseConditions()
    {
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
        var player = CurrentGame.PlayerFaction;
        Guard.IsNotNull(player);

        // Win condition
        if (CurrentGame.CurrentCycle > 20 || player.Technology >= 100)
        {
            CurrentGame.SaveName = "WINNER";
        }

        // Lose conditions
        if (
            CurrentGame.GalacticStability <= 0
            || player.Population <= 0
            || player.Resources <= 0
            || player.Stability <= 0
        )
        {
            CurrentGame.SaveName = "LOSER";
        }
    }

    /// <summary>
    /// Applies a single player action to the player's faction, updating its statistics accordingly.
    /// Each action type has a predefined set of effects on faction stats or global game parameters.
    /// </summary>
    /// <param name="action">The player action to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ProcessPlayerActionAsync(PlayerAction action)
    {
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
                        player.Reputation = Math.Max(
                            GameConstants.MinReputation,
                            Math.Min(player.Reputation, GameConstants.MaxReputation)
                        );
                        break;
                    case PlayerActionType.Espionage:
                        player.Technology += 1;
                        player.Resources += 2;
                        break;
                    case PlayerActionType.Sabotage:
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += 1;
                        break;
                    default:
                        break;
                }
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Applies an update function to a specified faction and then clamps its resources to ensure they stay within valid game bounds.
    /// This is a utility method to ensure that faction stats like resources, population, etc., do not go below zero or exceed defined maximums after an update.
    /// </summary>
    /// <param name="factionId">The ID of the faction to update. Currently, this method is hardcoded to update the player's faction.</param>
    /// <param name="update">An asynchronous function that takes a <see cref="Faction"/> object and applies modifications to it.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    /// Updates the global world state at the end of each turn.
    /// This can include gradual decay of galactic infrastructure, random discoveries, or other background changes.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    /// Provides access to the game data service for operations like saving or loading games.
    /// </summary>
    public IGameDataService GameDataService => _gameDataService;

    /// <summary>
    /// Sets the current game state. Primarily used for testing or specific game setup scenarios.
    /// </summary>
    /// <param name="gameState">The <see cref="GameState"/> to set as the current game.</param>
    public void SetCurrentGame(GameState gameState) => CurrentGame = gameState;
}
