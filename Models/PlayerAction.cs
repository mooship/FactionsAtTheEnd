namespace FactionsAtTheEnd.Models;

// <summary>
// Represents a single action taken by a player in a turn.
// </summary>
public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public string FactionId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = [];
}
