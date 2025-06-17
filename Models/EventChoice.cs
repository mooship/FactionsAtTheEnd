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

    public EventChoice()
    {
        Guard.IsNotNullOrWhiteSpace(Description, nameof(Description));
        Guard.IsNotNull(Effects, nameof(Effects));
        Guard.IsNotNull(BlockedActions, nameof(BlockedActions));
    }
}
