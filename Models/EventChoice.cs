using FactionsAtTheEnd.Enums;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a choice within a game event, allowing player decisions. Now supports multi-step choices.
/// </summary>
public class EventChoice
{
    public string Description { get; set; } = string.Empty;
    public Dictionary<StatKey, int> Effects { get; set; } = [];
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    /// <summary>
    /// If set, after this choice is selected, these choices are presented as the next step (multi-step branching).
    /// </summary>
    public List<EventChoice>? NextStepChoices { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventChoice"/> class with the specified description, effects, blocked actions, and optional next step choices.
    /// </summary>
    /// <param name="description">The description of the event choice.</param>
    /// <param name="effects">The effects of the event choice on game stats.</param>
    /// <param name="blockedActions">The actions blocked by this event choice.</param>
    /// <param name="nextStepChoices">The next step choices if this is a multi-step choice.</param>
    public EventChoice(
        string description,
        Dictionary<StatKey, int>? effects = null,
        List<PlayerActionType>? blockedActions = null,
        List<EventChoice>? nextStepChoices = null
    )
    {
        Description = description;
        Effects = effects ?? [];
        BlockedActions = blockedActions ?? [];
        NextStepChoices = nextStepChoices;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventChoice"/> class with default values.
    /// </summary>
    public EventChoice() { }
}
