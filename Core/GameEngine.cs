using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;
using FluentValidation;

namespace FactionsAtTheEnd.Core;

/// <summary>
/// Main game engine for Factions at the End.
/// Handles game state, turn processing, and win/lose conditions.
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
    /// Gets the current game state.
    /// This is null if no game is loaded or before a new game starts.
    /// </summary>
    public GameState? CurrentGame { get; private set; }

    /// <summary>
    /// Creates a new game session.
    /// Initializes a player faction and sets up the initial game state, including a starting crisis event.
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

        // Add initial crisis event for narrative context
        var initialEvents = new List<GameEvent>
        {
            new()
            {
                Title = "Collapse",
                Description =
                    "The imperial government has fallen. You lead the last organized group in your region. Survival is up to you.",
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
        Guard.IsNotNull(loaded, nameof(loaded)); // Ensure the loaded game state is not null.
        CurrentGame = loaded;
    }

    /// <summary>
    /// Processes a full turn in the game.
    /// This includes validating player actions, updating the world state, generating new events,
    /// applying event effects, saving the game, checking win/loss conditions, and advancing the game cycle.
    /// </summary>
    /// <param name="playerActions">A list of actions taken by the player during this turn.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ProcessTurnAsync(List<PlayerAction> playerActions)
    {
        Guard.IsNotNull(playerActions, nameof(playerActions));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        var (validActions, actionCounts) = ValidateAndCountActions(playerActions);
        UpdateActionCounts(actionCounts); // Update counts of how often each action type has been recently used.
        await ApplyPlayerActionsAsync(validActions); // Apply the effects of the player's actions.
        await UpdateWorldStateAsync(); // Update global game parameters like stability and tech discovery.
        var newEvents = (await _eventService.GenerateRandomEventsAsync(CurrentGame)).ToList(); // Generate random events for the turn.
        CurrentGame.RecentEvents.AddRange(newEvents);

        // Generate and add galactic news headlines based on recent events.
        var newNews = _eventService.GenerateGalacticNews(CurrentGame, newEvents);
        if (newNews.Count != 0)
        {
            CurrentGame.GalacticNews.AddRange(newNews);
            // Keep only the most recent 15 news items to prevent the list from growing indefinitely.
            CurrentGame.GalacticNews =
            [
                .. CurrentGame.GalacticNews.Skip(Math.Max(0, CurrentGame.GalacticNews.Count - 15)),
            ];
        }
        ApplyEventEffects(newEvents); // Apply the statistical and gameplay effects of the new events.
        await _gameDataService.SaveGameAsync(CurrentGame); // Persist the current game state.
        CheckWinLoseConditions(); // Determine if any win or loss conditions have been met.
        CurrentGame.CurrentCycle++; // Advance to the next game cycle.
    }

    /// <summary>
    /// Validates a list of player actions and counts the occurrences of each action type.
    /// Invalid actions are filtered out.
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
    /// Updates the rolling counts of recently performed actions.
    /// This involves adding the counts from the current turn and then decaying the counts from previous turns.
    /// This mechanic can be used to influence event generation or impose temporary restrictions.
    /// </summary>
    /// <param name="actionCounts">A dictionary containing the counts of each action type performed in the current turn.</param>
    private void UpdateActionCounts(Dictionary<PlayerActionType, int> actionCounts)
    {
        Guard.IsNotNull(actionCounts, nameof(actionCounts));
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        // Add current turn's action counts to the persistent recent action counts.
        foreach (var key in actionCounts.Keys)
        {
            if (!CurrentGame.RecentActionCounts.ContainsKey(key))
            {
                CurrentGame.RecentActionCounts[key] = 0;
            }
            CurrentGame.RecentActionCounts[key] += actionCounts[key];
        }

        // Decay all recent action counts by 1, ensuring they don't go below 0.
        // This simulates the diminishing relevance of past actions.
        var keys = CurrentGame.RecentActionCounts.Keys.ToList(); // ToList() is used to avoid modification during iteration issues.
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
        CurrentGame.BlockedActions?.Clear(); // Clear any actions blocked from previous turns.

        foreach (var gameEvent in newEvents)
        {
            Guard.IsNotNull(player, nameof(player)); // Ensure player faction exists.
            foreach (var effect in gameEvent.Effects)
            {
                // Apply stat changes based on the event effect.
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
                        // Note: This currently modifies GalacticStability, not player faction's stability.
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += effect.Value;
                        break;
                    case StatKey.Reputation:
                        player.Reputation += effect.Value;
                        // Clamp reputation within defined min/max bounds.
                        player.Reputation = Math.Max(
                            GameConstants.MinReputation,
                            Math.Min(player.Reputation, GameConstants.MaxReputation)
                        );
                        break;
                }
            }

            // Add any actions that this event blocks for the next turn.
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

        // Example Win Condition: Survive 20 cycles or achieve 100 Technology.
        if (player != null && (CurrentGame.CurrentCycle > 20 || player.Technology >= 100))
        {
            CurrentGame.SaveName = "WINNER"; // Mark the game as won.
        }
        // TODO: Implement Lose Conditions (e.g., player stability at 0, critical resource depletion).
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

        // Apply action effects within the ApplyToFactionAsync to ensure resource clamping.
        await ApplyToFactionAsync(
            player.Id,
            async f =>
            {
                switch (action.ActionType)
                {
                    case PlayerActionType.BuildDefenses:
                        f.Military += 5; // Increase military strength.
                        f.Stability += 2; // Increase faction stability.
                        break;
                    case PlayerActionType.RecruitTroops:
                        f.Military += 7; // Increase military strength.
                        f.Resources -= 3; // Decrease resources (cost of recruitment).
                        break;
                    case PlayerActionType.DevelopInfrastructure:
                        f.Resources += 5; // Increase resource generation/capacity.
                        f.Stability += 2; // Increase faction stability.
                        break;
                    case PlayerActionType.ExploitResources:
                        f.Resources += 8; // Increase resources.
                        f.Stability -= 1; // Slightly decrease stability (potential unrest or environmental impact).
                        break;
                    case PlayerActionType.MilitaryTech:
                        f.Technology += 4; // Increase technology level.
                        f.Military += 2; // Increase military strength due to tech.
                        break;
                    case PlayerActionType.EconomicTech:
                        f.Technology += 4; // Increase technology level.
                        f.Resources += 2; // Increase resources due to tech.
                        break;
                    case PlayerActionType.AncientStudies:
                        f.Technology += 2; // Increase technology level.
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.AncientTechDiscovery += 5; // Increase discovery of ancient technology.
                        break;
                    case PlayerActionType.GateNetworkResearch:
                        f.Technology += 2; // Increase technology level.
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GateNetworkIntegrity += 3; // Improve gate network integrity.
                        break;
                    case PlayerActionType.Diplomacy:
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += 3; // Increase overall galactic stability.
                        player.Influence += 2; // Increase player faction's influence.
                        player.Reputation += 5; // Increase player faction's reputation.
                        // Clamp reputation within defined min/max bounds.
                        player.Reputation = Math.Max(
                            GameConstants.MinReputation,
                            Math.Min(player.Reputation, GameConstants.MaxReputation)
                        );
                        break;
                    case PlayerActionType.Espionage:
                        // Successful espionage might yield technology and resources.
                        player.Technology += 1;
                        player.Resources += 2;
                        break;
                    case PlayerActionType.Sabotage:
                        // Sabotage actions might slightly increase galactic stability if targeting destabilizing elements,
                        // or decrease it if targeting stable factions. This implementation assumes the former.
                        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));
                        CurrentGame.GalacticStability += 1;
                        break;
                    default:
                        // No operation for undefined or unhandled action types.
                        break;
                }
                await Task.CompletedTask; // Mark the inner async lambda as complete.
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

        // TODO: This method currently only targets the PlayerFaction. It should be generalized if actions can affect other factions.
        var faction = CurrentGame.PlayerFaction;
        Guard.IsNotNull(faction, nameof(faction)); // Ensure the faction to be updated exists.

        await update(faction); // Execute the provided update logic on the faction.
        faction.ClampResources(); // Ensure faction resources and stats are within valid game limits after the update.
    }

    /// <summary>
    /// Updates the global world state at the end of each turn.
    /// This can include gradual decay of galactic infrastructure, random discoveries, or other background changes.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateWorldStateAsync()
    {
        Guard.IsNotNull(CurrentGame, nameof(CurrentGame));

        // Simulate gradual decay of galactic stability and gate network integrity.
        // Values are reduced by a small random amount each turn, with a minimum of 0.
        await Task.Run(() => // Offload to a background thread if potentially long-running, though current logic is quick.
        {
            CurrentGame.GalacticStability = Math.Max(
                0, // Prevent stability from going below 0.
                CurrentGame.GalacticStability - Random.Shared.Next(0, 3) // Subtract a random value between 0 and 2.
            );
            CurrentGame.GateNetworkIntegrity = Math.Max(
                0, // Prevent integrity from going below 0.
                CurrentGame.GateNetworkIntegrity - Random.Shared.Next(0, 2) // Subtract a random value between 0 and 1.
            );

            // There's a 15% chance each turn to make progress in Ancient Tech Discovery.
            if (Random.Shared.Next(1, 101) <= 15) // Random.Shared.Next(1, 101) gives a range of 1 to 100 inclusive.
            {
                CurrentGame.AncientTechDiscovery = Math.Min(
                    100, // Prevent ancient tech discovery from exceeding 100.
                    CurrentGame.AncientTechDiscovery + Random.Shared.Next(1, 5) // Add a random value between 1 and 4.
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
