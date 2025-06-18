using FactionsAtTheEnd.Enums;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a narrative or mechanical event that affects the game state.
/// </summary>
public class GameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public int Cycle { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public Dictionary<StatKey, int> Effects { get; set; } = [];
    public List<PlayerActionType> BlockedActions { get; set; } = [];
    public List<EventChoice>? Choices { get; set; }

    /// <summary>
    /// Optional tags or categories for event variety and filtering.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent"/> class with the specified title, description, type, and cycle.
    /// </summary>
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
