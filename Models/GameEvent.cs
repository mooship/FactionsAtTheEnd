using FactionsAtTheEnd.Enums;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a narrative or mechanical event that affects the game state.
/// </summary>
public class GameEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the event.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the title of the event.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    public EventType Type { get; set; }

    /// <summary>
    /// Gets or sets the cycle in which the event occurs.
    /// </summary>
    public int Cycle { get; set; }

    /// <summary>
    /// Gets or sets additional parameters for the event.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets the effects of the event on various stats.
    /// </summary>
    public Dictionary<StatKey, int> Effects { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of player actions blocked by this event.
    /// </summary>
    public List<PlayerActionType> BlockedActions { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of choices available for this event.
    /// </summary>
    public List<EventChoice>? Choices { get; set; }

    /// <summary>
    /// Optional tags or categories for event variety and filtering.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent"/> class with the specified title, description, type, and cycle.
    /// </summary>
    /// <param name="title">The title of the event.</param>
    /// <param name="description">The description of the event.</param>
    /// <param name="type">The type of the event.</param>
    /// <param name="cycle">The cycle in which the event occurs.</param>
    public GameEvent(string title, string description, EventType type, int cycle)
    {
        Id = Guid.NewGuid().ToString();
        Title = title;
        Description = description;
        Type = type;
        Cycle = cycle;
        Parameters = [];
        Effects = [];
        BlockedActions = [];
        Tags = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent"/> class with default values.
    /// </summary>
    public GameEvent()
    {
        Tags = [];
    }
}
