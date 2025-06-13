namespace FactionsAtTheEnd.Models;

public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public string FactionId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = [];
}
