using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Core;

/// <summary>
/// Main game engine for Factions at the End (single-player, single-faction MVP).
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
    /// The current game state. Null if no game is loaded.
    /// </summary>
    public GameState? CurrentGame { get; private set; }

    /// <summary>
    /// Creates a new game with a single player faction.
    /// </summary>
    public async Task<GameState> CreateNewGameAsync(
        string playerFactionName,
        FactionType playerFactionType
    )
    {
        Guard.IsNotNullOrWhiteSpace(playerFactionName);
        Guard.IsTrue(
            Enum.IsDefined(playerFactionType),
            nameof(playerFactionType) + " must be a valid FactionType."
        );

        var gameState = new GameState
        {
            SaveName = $"Game Started {DateTime.Now:yyyy-MM-dd HH:mm}",
            CurrentCycle = 1,
        };

        var playerFaction = _factionService.CreateFaction(
            playerFactionName,
            playerFactionType,
            isPlayer: true
        );
        gameState.Factions.Add(playerFaction);
        gameState.PlayerFactionId = playerFaction.Id;

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
                AffectedFactions = [playerFaction.Id],
            },
        };
        gameState.RecentEvents.AddRange(initialEvents);

        CurrentGame = gameState;
        await _gameDataService.SaveGameAsync(gameState);

        return gameState;
    }

    /// <summary>
    /// Returns a list of all saved games.
    /// </summary>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        Guard.IsNotNull(_gameDataService);

        return await _gameDataService.GetSavedGamesAsync();
    }

    /// <summary>
    /// Loads a saved game by ID.
    /// </summary>
    public async Task LoadGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId);
        Guard.IsNotNull(_gameDataService);

        CurrentGame = await _gameDataService.LoadGameAsync(gameId);
    }

    /// <summary>
    /// Processes a turn: validates and applies player actions, updates world state, generates events, and checks win/lose conditions.
    /// </summary>
    public async Task ProcessTurnAsync(List<PlayerAction> playerActions)
    {
        Guard.IsNotNull(playerActions);
        Guard.IsNotNull(CurrentGame);

        var (validActions, actionCounts) = ValidateAndCountActions(playerActions);
        UpdateActionCounts(actionCounts);
        await ApplyPlayerActionsAsync(validActions);
        await UpdateWorldStateAsync();
        var newEvents = (await _eventService.GenerateRandomEventsAsync(CurrentGame)).ToList();
        CurrentGame.RecentEvents.AddRange(newEvents);
        ApplyEventEffects(newEvents);
        await _gameDataService.SaveGameAsync(CurrentGame);
        CheckWinLoseConditions();
    }

    /// <summary>
    /// Validates player actions and counts their types.
    /// </summary>
    private (
        List<PlayerAction> validActions,
        Dictionary<PlayerActionType, int> actionCounts
    ) ValidateAndCountActions(List<PlayerAction> playerActions)
    {
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
    /// Updates rolling action counts and decays old counts.
    /// </summary>
    private void UpdateActionCounts(Dictionary<PlayerActionType, int> actionCounts)
    {
        foreach (var key in actionCounts.Keys)
        {
            if (!CurrentGame!.RecentActionCounts.ContainsKey(key))
            {
                CurrentGame!.RecentActionCounts[key] = 0;
            }
            CurrentGame!.RecentActionCounts[key] += actionCounts[key];
        }
        var keys = CurrentGame!.RecentActionCounts.Keys.ToList();
        foreach (var key in keys)
        {
            CurrentGame!.RecentActionCounts[key] = Math.Max(
                0,
                CurrentGame!.RecentActionCounts[key] - 1
            );
        }
    }

    /// <summary>
    /// Applies all valid player actions asynchronously.
    /// </summary>
    private async Task ApplyPlayerActionsAsync(List<PlayerAction> validActions)
    {
        foreach (var action in validActions)
        {
            await ProcessPlayerActionAsync(action);
        }
    }

    /// <summary>
    /// Applies event effects and updates blocked actions.
    /// </summary>
    private void ApplyEventEffects(List<GameEvent> newEvents)
    {
        var player = CurrentGame!.Factions.FirstOrDefault(f =>
            f.Id == CurrentGame!.PlayerFactionId
        );
        CurrentGame!.BlockedActions.Clear();
        foreach (var gameEvent in newEvents)
        {
            if (player != null)
            {
                foreach (var effect in gameEvent.Effects)
                {
                    switch (effect.Key)
                    {
                        case UI.StatKey.Population:
                            player.Population += effect.Value;
                            break;
                        case UI.StatKey.Military:
                            player.Military += effect.Value;
                            break;
                        case UI.StatKey.Technology:
                            player.Technology += effect.Value;
                            break;
                        case UI.StatKey.Influence:
                            player.Influence += effect.Value;
                            break;
                        case UI.StatKey.Resources:
                            player.Resources += effect.Value;
                            break;
                        case UI.StatKey.Stability:
                            CurrentGame!.GalacticStability += effect.Value;
                            break;
                    }
                }
                if (gameEvent.BlockedActions != null)
                {
                    foreach (var blocked in gameEvent.BlockedActions)
                    {
                        if (!CurrentGame!.BlockedActions.Contains(blocked))
                        {
                            CurrentGame!.BlockedActions.Add(blocked);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks win/lose conditions and updates game state accordingly.
    /// </summary>
    private void CheckWinLoseConditions()
    {
        var player = CurrentGame!.Factions.FirstOrDefault(f =>
            f.Id == CurrentGame!.PlayerFactionId
        );
        if (player != null && (CurrentGame!.CurrentCycle > 20 || player.Technology >= 100))
        {
            CurrentGame!.SaveName = "WINNER";
        }
    }

    /// <summary>
    /// Applies a single player action to the player's faction.
    /// </summary>
    private async Task ProcessPlayerActionAsync(PlayerAction action)
    {
        Guard.IsNotNull(action);

        switch (action.ActionType)
        {
            case PlayerActionType.Build_Defenses:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Military += 5;
                        f.Stability = Math.Min(100, f.Stability + 2);
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Recruit_Troops:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Military += 8;
                        f.Resources = Math.Max(0, f.Resources - 5);
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Develop_Infrastructure:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Resources += 7;
                        f.Stability = Math.Min(100, f.Stability + 1);
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Exploit_Resources:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Resources += 10;
                        f.Stability = Math.Max(0, f.Stability - 2);
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Military_Tech:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Technology += 5;
                        f.Military += 2;
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Economic_Tech:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Technology += 5;
                        f.Resources += 3;
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Ancient_Studies:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Technology += 3;
                        await Task.CompletedTask;
                    }
                );
                break;
            case PlayerActionType.Gate_Network_Research:
                await ApplyToFactionAsync(
                    action.FactionId,
                    async f =>
                    {
                        f.Technology += 2;
                        await Task.CompletedTask;
                    }
                );
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Applies an update function to the specified faction and clamps resources.
    /// </summary>
    private async Task ApplyToFactionAsync(string factionId, Func<Faction, Task> update)
    {
        Guard.IsNotNullOrWhiteSpace(factionId);
        Guard.IsNotNull(update);
        Guard.IsNotNull(CurrentGame);

        if (CurrentGame == null)
        {
            return;
        }
        var faction = CurrentGame.Factions.FirstOrDefault(f => f.Id == factionId);
        if (faction != null)
        {
            await update(faction);
            faction.ClampResources();
        }
    }

    /// <summary>
    /// Updates the world state at the end of each turn (e.g., global stability, tech discovery).
    /// </summary>
    private async Task UpdateWorldStateAsync()
    {
        Guard.IsNotNull(CurrentGame);

        if (CurrentGame == null)
        {
            return;
        }
        // Gradual decay of galactic infrastructure
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
}
