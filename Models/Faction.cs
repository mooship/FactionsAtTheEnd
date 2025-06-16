using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FactionsAtTheEnd.UI;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a player or non-player faction, including stats, traits, and status.
/// </summary>
public class Faction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FactionType Type { get; set; }
    public bool IsPlayer { get; set; }

    // Core resources and stats for gameplay
    public int Population { get; set; }
    public int Military { get; set; }
    public int Technology { get; set; }
    public int Influence { get; set; }
    public int Resources { get; set; }
    public int Reputation { get; set; } = 25;

    // Narrative and mechanical status
    public FactionStatus Status { get; set; } = FactionStatus.Stable;
    public int Stability { get; set; } = 50;

    // Unique traits for flavor and event hooks
    public List<string> Traits { get; set; } = [];

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Clamp all resource and stat values to valid min/max bounds.
    /// </summary>
    public void ClampResources()
    {
        Population = Math.Max(GameConstants.MinStat, Math.Min(Population, GameConstants.MaxStat));
        Military = Math.Max(GameConstants.MinStat, Math.Min(Military, GameConstants.MaxStat));
        Technology = Math.Max(GameConstants.MinStat, Math.Min(Technology, GameConstants.MaxStat));
        Influence = Math.Max(GameConstants.MinStat, Math.Min(Influence, GameConstants.MaxStat));
        Resources = Math.Max(GameConstants.MinStat, Math.Min(Resources, GameConstants.MaxStat));
        Stability = Math.Max(GameConstants.MinStat, Math.Min(Stability, GameConstants.MaxStat));
        Reputation = Math.Max(
            GameConstants.MinReputation,
            Math.Min(Reputation, GameConstants.MaxReputation)
        );
    }
}

/// <summary>
/// The type of a faction, which determines its starting bonuses and flavor.
/// </summary>
public enum FactionType
{
    [Display(Name = "Military Junta")]
    MilitaryJunta,

    [Display(Name = "Corporate Council")]
    CorporateCouncil,

    [Display(Name = "Religious Order")]
    ReligiousOrder,

    [Display(Name = "Pirate Alliance")]
    PirateAlliance,

    [Display(Name = "Technocratic Union")]
    TechnocraticUnion,

    [Display(Name = "Rebellion Cell")]
    RebellionCell,

    [Display(Name = "Imperial Remnant")]
    ImperialRemnant,

    [Display(Name = "Ancient Awakened")]
    AncientAwakened,
}

/// <summary>
/// The current status of a faction, used for narrative and event flavor.
/// </summary>
public enum FactionStatus
{
    Thriving,
    Stable,
    Struggling,
    Desperate,
    Collapsing,
}

public static class FactionExtensions
{
    /// <summary>
    /// Get the display name for a FactionType using its Display attribute.
    /// </summary>
    public static string GetDisplayName(this FactionType factionType)
    {
        var displayName = factionType
            .GetType()
            .GetMember(factionType.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()
            ?.Name;

        return displayName ?? factionType.ToString();
    }
}
