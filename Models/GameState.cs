namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents the full state of a game session, including world and player data.
/// </summary>
public class GameState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SaveName { get; set; } = "New Game";
    public int CurrentCycle { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    // World state
    public int GalacticStability { get; set; } = 40; // 0-100, starts deteriorating
    public int GateNetworkIntegrity { get; set; } = 60; // 0-100, affects travel/trade
    public int AncientTechDiscovery { get; set; } = 10; // 0-100, unlocks events

    // All factions in the game
    public List<Faction> Factions { get; set; } = [];

    // Player's faction ID
    public string PlayerFactionId { get; set; } = string.Empty;

    // Recent events and history
    public List<GameEvent> RecentEvents { get; set; } = [];
    public List<string> WorldHistory { get; set; } = [];

    // Actions blocked for the next turn due to events
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    // Track how many times each action was used in the last 3 turns (anti-spam)
    public Dictionary<PlayerActionType, int> RecentActionCounts { get; set; } = [];
}

/// <summary>
/// Represents a narrative or mechanical event that affects the game state.
/// </summary>
public class GameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public int Cycle { get; set; }
    public List<string> AffectedFactions { get; set; } = [];
    public Dictionary<string, object> Parameters { get; set; } = [];

    // Effects: resource/stat changes (e.g., { StatKey.Military, -5 })
    public Dictionary<UI.StatKey, int> Effects { get; set; } = [];

    // Blocked actions for the next turn (optional)
    public List<PlayerActionType> BlockedActions { get; set; } = [];
}

/// <summary>
/// The type/category of a game event.
/// </summary>
public enum EventType
{
    Military,
    Economic,
    Technological,
    Crisis,
    Discovery,
    Natural,
}
