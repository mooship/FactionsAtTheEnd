namespace FactionsAtTheEnd.Models;

/// <summary>
/// Holds all persistent and transient data for a single game session.
/// </summary>
public class GameState
{
    /// <summary>
    /// Gets or sets the unique identifier for the game state.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the save name for the game state.
    /// </summary>
    public string SaveName { get; set; } = "New Game";

    /// <summary>
    /// Gets or sets the current cycle of the game.
    /// </summary>
    public int CurrentCycle { get; set; } = 1;

    /// <summary>
    /// Gets or sets the creation date of the game state.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last played date of the game state.
    /// </summary>
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// World state variables affecting events, win/lose, and narrative.
    /// </summary>
    public int GalacticStability { get; set; } = 40;

    /// <summary>
    /// Gets or sets the integrity of the gate network.
    /// </summary>
    public int GateNetworkIntegrity { get; set; } = 60;

    /// <summary>
    /// Gets or sets the level of ancient technology discovery.
    /// </summary>
    public int AncientTechDiscovery { get; set; } = 10;

    /// <summary>
    /// Gets or sets the player's faction.
    /// </summary>
    public Faction PlayerFaction { get; set; } = new Faction();

    /// <summary>
    /// Recent events for event log and turn feedback.
    /// </summary>
    public List<GameEvent> RecentEvents { get; set; } = [];

    /// <summary>
    /// Gets or sets the galactic history log.
    /// </summary>
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

    /// <summary>
    /// Indicates if the player has won the game.
    /// </summary>
    public bool HasWon { get; set; } = false;

    /// <summary>
    /// Indicates if the player has lost the game.
    /// </summary>
    public bool HasLost { get; set; } = false;

    /// <summary>
    /// Gets or sets the save version.
    /// </summary>
    public int SaveVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the list of achievements earned in this game state.
    /// </summary>
    public List<string> Achievements { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GameState"/> class with the specified id, save name, and player faction.
    /// </summary>
    public GameState(string id, string saveName, Faction playerFaction)
    {
        Id = id;
        SaveName = saveName;
        PlayerFaction = playerFaction;
        CreatedAt = DateTime.UtcNow;
        LastPlayed = DateTime.UtcNow;
        SaveVersion = 1;
        Achievements = [];
        RecentEvents = [];
        GalacticHistory = [];
        BlockedActions = [];
        RecentActionCounts = [];
        GalacticNews = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameState"/> class with default values.
    /// </summary>
    public GameState()
    {
        SaveVersion = 1;
        Achievements = [];
    }
}
