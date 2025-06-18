namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a single action taken by a player in a turn.
/// </summary>
public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerAction"/> class with the specified action type and parameters.
    /// </summary>
    public PlayerAction(PlayerActionType actionType, Dictionary<string, object>? parameters = null)
    {
        ActionType = actionType;
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerAction"/> class with default values.
    /// </summary>
    public PlayerAction() { }
}
