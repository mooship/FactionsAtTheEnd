namespace FactionsAtTheEnd.Models;

using CommunityToolkit.Diagnostics;

/// <summary>
/// Represents a single action taken by a player in a turn.
/// </summary>
public class PlayerAction
{
    public PlayerActionType ActionType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];

    public PlayerAction()
    {
        Guard.IsTrue(Enum.IsDefined(typeof(PlayerActionType), ActionType), nameof(ActionType));
        Guard.IsNotNull(Parameters, nameof(Parameters));
    }
}
