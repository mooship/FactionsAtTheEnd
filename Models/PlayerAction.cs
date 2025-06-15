namespace FactionsAtTheEnd.Models;

// PlayerAction: Represents a single action taken by a player in a turn.
public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public string FactionId { get; set; } = string.Empty;
    public string? TargetId { get; set; } // Target for actions like Espionage or Sabotage
    public Dictionary<string, object> Parameters { get; set; } = [];
}
