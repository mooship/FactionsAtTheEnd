using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.UI;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a choice within a game event, allowing player decisions.
/// </summary>
public class EventChoice
{
    public string Description { get; set; } = string.Empty;
    public Dictionary<StatKey, int> Effects { get; set; } = [];
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="EventChoice"/> class with the specified description, effects, and blocked actions.
    /// </summary>
    /// <param name="description">The description of the event choice.</param>
    /// <param name="effects">The effects of the event choice on game stats.</param>
    /// <param name="blockedActions">The actions blocked by this event choice.</param>
    public EventChoice(
        string description,
        Dictionary<StatKey, int>? effects = null,
        List<PlayerActionType>? blockedActions = null
    )
    {
        Description = description;
        Effects = effects ?? new Dictionary<StatKey, int>();
        BlockedActions = blockedActions ?? new List<PlayerActionType>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventChoice"/> class with default values.
    /// </summary>
    public EventChoice() { }
}
