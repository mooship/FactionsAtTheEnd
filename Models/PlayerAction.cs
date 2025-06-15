namespace FactionsAtTheEnd.Models;

// PlayerAction: Represents a single action taken by a player in a turn.
public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public string FactionId { get; set; } = string.Empty;

    // TargetId is no longer used in single-faction mode, but retained for future extensibility.
    public Dictionary<string, object> Parameters { get; set; } = [];
}
