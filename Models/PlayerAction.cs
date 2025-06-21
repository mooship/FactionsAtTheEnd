namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a single action taken by a player in a turn.
/// </summary>
public class PlayerAction
{
    /// <summary>
    /// Gets or sets the type of action taken by the player.
    /// </summary>
    public PlayerActionType ActionType { get; set; }

    /// <summary>
    /// Gets or sets the parameters associated with the action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerAction"/> class with the specified action type and parameters.
    /// </summary>
    /// <param name="actionType">The type of action taken.</param>
    /// <param name="parameters">The parameters for the action.</param>
    public PlayerAction(PlayerActionType actionType, Dictionary<string, object>? parameters = null)
    {
        ActionType = actionType;
        Parameters = parameters ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerAction"/> class with default values.
    /// </summary>
    public PlayerAction() { }
}
