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
    /// The current game state. Null if no game is loaded or before a new game starts.
    /// </summary>
    public GameState? CurrentGame { get; private set; }

    /// <summary>
    /// Create a new game session with a single player faction and initial event.
    /// </summary>
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
    /// Get all saved games from persistent storage, most recent first.
    /// </summary>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        return await _gameDataService.GetSavedGamesAsync();
    }

    /// <summary>
    /// Load a saved game by its unique ID.
    /// </summary>
    public async Task LoadGameAsync(string gameId)
    {
        var loaded = await _gameDataService.LoadGameAsync(gameId);
        Guard.IsNotNull(loaded);
        CurrentGame = loaded;
    }

    /// <summary>
    /// Process a full turn: validate actions, update world, generate events, and check win/lose.
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
        // Generate and add galactic news headlines
        var newNews = _eventService.GenerateGalacticNews(CurrentGame, newEvents);
        if (newNews.Count != 0)
        {
            CurrentGame.GalacticNews.AddRange(newNews);
            // Keep only the most recent 15 news items
            CurrentGame.GalacticNews =
            [
                .. CurrentGame.GalacticNews.Skip(CurrentGame.GalacticNews.Count - 15),
            ];
        }
        ApplyEventEffects(newEvents);
        await _gameDataService.SaveGameAsync(CurrentGame);
        CheckWinLoseConditions();
        CurrentGame.CurrentCycle++;
    }

    /// <summary>
    /// Validate player actions and count each action type for anti-spam and event logic.
    /// </summary>
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
    /// Update rolling action counts for anti-spam and event triggers. Decays old counts.
    /// </summary>
    private void UpdateActionCounts(Dictionary<PlayerActionType, int> actionCounts)
    {
        Guard.IsNotNull(actionCounts);
        Guard.IsNotNull(CurrentGame);
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
    /// Apply all valid player actions to the player's faction.
    /// </summary>
    private async Task ApplyPlayerActionsAsync(List<PlayerAction> validActions)
    {
        foreach (var action in validActions)
        {
            await ProcessPlayerActionAsync(action);
        }
    }

    /// <summary>
    /// Apply effects from new events and update blocked actions for the next turn.
    /// </summary>
    private void ApplyEventEffects(List<GameEvent> newEvents)
    {
        Guard.IsNotNull(newEvents);
        Guard.IsNotNull(CurrentGame);
        var player = CurrentGame.PlayerFaction;
        CurrentGame.BlockedActions?.Clear();
        foreach (var gameEvent in newEvents)
        {
            Guard.IsNotNull(player);
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
                        Guard.IsNotNull(CurrentGame);
                        CurrentGame.GalacticStability += effect.Value;
                        break;
                    case StatKey.Reputation:
                        player.Reputation += effect.Value;
                        player.Reputation = Math.Max(
                            GameConstants.MinReputation,
                            Math.Min(player.Reputation, GameConstants.MaxReputation)
                        );
                        break;
                }
            }
            if (gameEvent.BlockedActions != null && CurrentGame?.BlockedActions != null)
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
    /// Check for win or lose conditions and update game state if met.
    /// </summary>
    private void CheckWinLoseConditions()
    {
        Guard.IsNotNull(CurrentGame);
        var player = CurrentGame.PlayerFaction;
        if (player != null && (CurrentGame.CurrentCycle > 20 || player.Technology >= 100))
        {
            CurrentGame.SaveName = "WINNER";
        }
    }

    /// <summary>
    /// Apply a single player action to the player's faction, updating stats.
    /// </summary>
    private async Task ProcessPlayerActionAsync(PlayerAction action)
    {
        Guard.IsNotNull(CurrentGame);
        Guard.IsNotNull(CurrentGame.PlayerFaction);
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
                        Guard.IsNotNull(CurrentGame);
                        CurrentGame.AncientTechDiscovery += 5;
                        break;
                    case PlayerActionType.GateNetworkResearch:
                        f.Technology += 2;
                        Guard.IsNotNull(CurrentGame);
                        CurrentGame.GateNetworkIntegrity += 3;
                        break;
                    case PlayerActionType.Diplomacy:
                        // Handle as a world/self action: increase GalacticStability or Reputation
                        CurrentGame.GalacticStability += 3;
                        player.Influence += 2;
                        player.Reputation += 5;
                        player.Reputation = Math.Max(
                            GameConstants.MinReputation,
                            Math.Min(player.Reputation, GameConstants.MaxReputation)
                        );
                        break;
                    case PlayerActionType.Espionage:
                        // Handle as a world/self action: reveal upcoming events or grant minor stat/resource bonuses
                        player.Technology += 1;
                        player.Resources += 2;
                        break;
                    case PlayerActionType.Sabotage:
                        // Handle as a world/self action: reduce negative event impact or remove internal obstacles
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
    /// Apply an update function to a faction and clamp its resources to valid bounds.
    /// </summary>
    private async Task ApplyToFactionAsync(string factionId, Func<Faction, Task> update)
    {
        Guard.IsNotNullOrWhiteSpace(factionId);
        Guard.IsNotNull(update);
        Guard.IsNotNull(CurrentGame);

        var faction = CurrentGame.PlayerFaction;
        Guard.IsNotNull(faction);
        await update(faction);
        faction.ClampResources();
    }

    /// <summary>
    /// Update world state at the end of each turn (e.g., decay, discoveries).
    /// </summary>
    private async Task UpdateWorldStateAsync()
    {
        Guard.IsNotNull(CurrentGame);
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

    public IGameDataService GameDataService => _gameDataService;

    public void SetCurrentGame(GameState gameState) => CurrentGame = gameState;
}
