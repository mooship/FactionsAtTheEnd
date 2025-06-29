using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Constants;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;

namespace FactionsAtTheEnd.Providers;

/// <summary>
/// Provides faction-type-specific data and logic such as descriptions, traits, and starting resources.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FactionTypeProvider"/> class.
/// </remarks>
/// <param name="random">The random provider to use for resource generation.</param>
public class FactionTypeProvider(IRandomProvider random) : IFactionTypeProvider
{
    private readonly IRandomProvider _random = random;

    /// <summary>
    /// Gets the description for a given faction type.
    /// </summary>
    /// <param name="type">The faction type.</param>
    /// <returns>The description string for the faction type.</returns>
    public string GetDescription(FactionType type)
    {
        Guard.IsNotNull(type, nameof(type));
        return type switch
        {
            FactionType.MilitaryJunta => FactionDescriptions.MilitaryJunta,
            FactionType.CorporateCouncil => FactionDescriptions.CorporateCouncil,
            FactionType.ReligiousOrder => FactionDescriptions.ReligiousOrder,
            FactionType.PirateAlliance => FactionDescriptions.PirateAlliance,
            FactionType.TechnocraticUnion => FactionDescriptions.TechnocraticUnion,
            FactionType.RebellionCell => FactionDescriptions.RebellionCell,
            FactionType.ImperialRemnant => FactionDescriptions.ImperialRemnant,
            FactionType.AncientAwakened => FactionDescriptions.AncientAwakened,
            _ => FactionDescriptions.Default,
        };
    }

    /// <summary>
    /// Gets the list of traits for a given faction type.
    /// </summary>
    /// <param name="type">The faction type.</param>
    /// <returns>A list of trait strings for the faction type.</returns>
    public List<string> GetTraits(FactionType type)
    {
        Guard.IsNotNull(type, nameof(type));
        return type switch
        {
            FactionType.MilitaryJunta => ["Disciplined", "Aggressive", "Organized"],
            FactionType.CorporateCouncil => ["Wealthy", "Calculating", "Opportunistic"],
            FactionType.ReligiousOrder => ["Fanatical", "United", "Missionary"],
            FactionType.PirateAlliance => ["Mobile", "Unpredictable", "Resourceful"],
            FactionType.TechnocraticUnion => ["Innovative", "Logical", "Progressive"],
            FactionType.RebellionCell => ["Idealistic", "Guerrilla", "Inspiring"],
            FactionType.ImperialRemnant => ["Traditional", "Proud", "Declining"],
            FactionType.AncientAwakened => ["Mysterious", "Powerful", "Alien"],
            _ => ["Determined", "Adaptive"],
        };
    }

    /// <summary>
    /// Sets the starting resources for a faction based on its type.
    /// </summary>
    /// <param name="faction">The faction to initialize.</param>
    public void SetStartingResources(Faction faction)
    {
        Guard.IsNotNull(faction, nameof(faction));
        faction.Population = _random.Next(
            GameConstants.StartingPopulationMin,
            GameConstants.StartingPopulationMax + 1
        );
        faction.Military = _random.Next(
            GameConstants.StartingMilitaryMin,
            GameConstants.StartingMilitaryMax + 1
        );
        faction.Technology = _random.Next(
            GameConstants.StartingTechnologyMin,
            GameConstants.StartingTechnologyMax + 1
        );
        faction.Influence = _random.Next(
            GameConstants.StartingInfluenceMin,
            GameConstants.StartingInfluenceMax + 1
        );
        faction.Resources = _random.Next(
            GameConstants.StartingResourcesMin,
            GameConstants.StartingResourcesMax + 1
        );
        switch (faction.Type)
        {
            case FactionType.MilitaryJunta:
                faction.Military += 15;
                faction.Stability -= 10;
                break;
            case FactionType.CorporateCouncil:
                faction.Resources += 15;
                faction.Reputation -= 10;
                break;
            case FactionType.TechnocraticUnion:
                faction.Technology += 15;
                faction.Influence -= 10;
                break;
            case FactionType.ReligiousOrder:
                faction.Population += 10;
                faction.Influence += 10;
                faction.Technology -= 10;
                break;
            case FactionType.ImperialRemnant:
                faction.Influence += 10;
                faction.Military += 10;
                faction.Reputation += 10;
                faction.Resources -= 10;
                break;
            case FactionType.AncientAwakened:
                faction.Technology += 20;
                faction.Population -= 20;
                break;
            case FactionType.PirateAlliance:
                faction.Military += 10;
                faction.Resources += 10;
                faction.Stability -= 10;
                break;
            case FactionType.RebellionCell:
                faction.Stability += 10;
                faction.Influence += 10;
                faction.Resources -= 10;
                break;
        }
        if (faction.IsPlayer)
        {
            faction.Population += 10;
            faction.Resources += 10;
        }
    }
}
