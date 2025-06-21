using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Constants;
using FactionsAtTheEnd.Enums;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// Represents a player or non-player faction, including stats, traits, and status.
/// </summary>
public class Faction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public string Description { get; set; } = string.Empty;
    public FactionType Type { get; set; }
    public bool IsPlayer { get; set; }
    public int Population { get; set; }
    public int Military { get; set; }
    public int Technology { get; set; }
    public int Influence { get; set; }
    public int Resources { get; set; }
    public int Reputation { get; set; } = 25;

    [JsonIgnore]
    public FactionStatus Status { get; set; } = FactionStatus.Stable;
    public int Stability { get; set; } = 50;

    [JsonIgnore]
    public List<string> Traits { get; set; } = [];

    [JsonIgnore]
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public Faction(string name, string description, FactionType type, bool isPlayer = false)
    {
        Name = name;
        Description = description;
        Type = type;
        IsPlayer = isPlayer;
    }

    public Faction() { }

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

        Guard.IsTrue(
            Population >= GameConstants.MinStat && Population <= GameConstants.MaxStat,
            nameof(Population)
        );
        Guard.IsTrue(
            Military >= GameConstants.MinStat && Military <= GameConstants.MaxStat,
            nameof(Military)
        );
        Guard.IsTrue(
            Technology >= GameConstants.MinStat && Technology <= GameConstants.MaxStat,
            nameof(Technology)
        );
        Guard.IsTrue(
            Influence >= GameConstants.MinStat && Influence <= GameConstants.MaxStat,
            nameof(Influence)
        );
        Guard.IsTrue(
            Resources >= GameConstants.MinStat && Resources <= GameConstants.MaxStat,
            nameof(Resources)
        );
        Guard.IsTrue(
            Stability >= GameConstants.MinStat && Stability <= GameConstants.MaxStat,
            nameof(Stability)
        );
        Guard.IsTrue(
            Reputation >= GameConstants.MinReputation && Reputation <= GameConstants.MaxReputation,
            nameof(Reputation)
        );

        UpdateStatus();
    }

    /// <summary>
    /// Updates the FactionStatus based on key stats (Stability, Population, Resources).
    /// </summary>
    public void UpdateStatus()
    {
        if (Stability <= 10 || Population <= 10 || Resources <= 10)
        {
            Status =
                (Stability <= 0 || Population <= 0 || Resources <= 0)
                    ? FactionStatus.Collapsing
                    : FactionStatus.Desperate;
        }
        else if (Stability <= 25 || Population <= 25 || Resources <= 25)
        {
            Status = FactionStatus.Struggling;
        }
        else if (Stability >= 80 && Population >= 80 && Resources >= 80)
        {
            Status = FactionStatus.Thriving;
        }
        else
        {
            Status = FactionStatus.Stable;
        }
    }
}
