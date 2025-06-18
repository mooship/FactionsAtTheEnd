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

    /// <summary>
    /// World state variables affecting events, win/lose, and narrative.
    /// </summary>
    public int GalacticStability { get; set; } = 40;
    public int GateNetworkIntegrity { get; set; } = 60;
    public int AncientTechDiscovery { get; set; } = 10;

    public Faction PlayerFaction { get; set; } = new Faction();

    /// <summary>
    /// Recent events for event log and turn feedback.
    /// </summary>
    public List<GameEvent> RecentEvents { get; set; } = [];
    public List<string> GalacticHistory { get; set; } = [];

    /// <summary>
    /// Actions blocked for the next turn due to events.
    /// </summary>
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    /// <summary>
    /// Rolling action counts for anti-spam and event logic.
    /// </summary>
    public Dictionary<PlayerActionType, int> RecentActionCounts { get; set; } = [];

    /// <summary>
    /// Emergent galactic news for narrative flavor.
    /// </summary>
    public List<string> GalacticNews { get; set; } = [];

    public GameState(string id, string saveName, Faction playerFaction)
    {
        Id = id;
        SaveName = saveName;
        PlayerFaction = playerFaction;
        CreatedAt = DateTime.UtcNow;
        LastPlayed = DateTime.UtcNow;
        RecentEvents = [];
        GalacticHistory = [];
        BlockedActions = [];
        RecentActionCounts = [];
        GalacticNews = [];
    }

    public GameState() { }
}
