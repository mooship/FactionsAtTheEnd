namespace FactionsAtTheEnd.Models;

/// <summary>
/// Holds all persistent and transient data for a single game session.
/// </summary>
public class GameState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SaveName { get; set; } = "New Game";
    public int CurrentCycle { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    // World state variables (affect events, win/lose, and narrative)
    public int GalacticStability { get; set; } = 40; // 0-100, starts deteriorating
    public int GateNetworkIntegrity { get; set; } = 60; // 0-100, affects travel/trade
    public int AncientTechDiscovery { get; set; } = 10; // 0-100, unlocks events

    public Faction PlayerFaction { get; set; } = new Faction();

    // Recent events (for event log and turn feedback)
    public List<GameEvent> RecentEvents { get; set; } = [];
    public List<string> GalacticHistory { get; set; } = [];

    // Actions blocked for the next turn due to events
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    // Rolling action counts for anti-spam and event logic
    public Dictionary<PlayerActionType, int> RecentActionCounts { get; set; } = [];

    // Emergent galactic news for narrative flavor
    public List<string> GalacticNews { get; set; } = [];
}
