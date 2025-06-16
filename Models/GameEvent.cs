using FactionsAtTheEnd.UI;

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
}
