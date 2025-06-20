using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Constants;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;

namespace FactionsAtTheEnd.Services;

public class FactionService : IFactionService
{
    public FactionService() { }

    public Faction CreateFaction(string name, FactionType type, bool isPlayer = false)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), type),
            nameof(type) + " must be a valid FactionType."
        );
        try
        {
            var faction = new Faction
            {
                Name = name,
                Type = type,
                IsPlayer = isPlayer,
                Description = GenerateFactionDescription(type),
                Traits = GenerateFactionTraits(type),
                Reputation = 25,
            };
            Guard.IsNotNull(faction.Description, nameof(faction.Description));
            Guard.IsNotNull(faction.Traits, nameof(faction.Traits));
            SetStartingResources(faction);

            switch (type)
            {
                case FactionType.MilitaryJunta:
                    faction.Military += 5;
                    break;
                case FactionType.CorporateCouncil:
                    faction.Resources += 5;
                    break;
                case FactionType.ReligiousOrder:
                    faction.Stability += 5;
                    break;
                case FactionType.PirateAlliance:
                    faction.Influence += 5;
                    break;
                case FactionType.TechnocraticUnion:
                    faction.Technology += 5;
                    break;
                case FactionType.RebellionCell:
                    faction.Stability += 3;
                    faction.Influence += 2;
                    break;
                case FactionType.ImperialRemnant:
                    faction.Population += 4;
                    faction.Reputation += 1;
                    break;
                case FactionType.AncientAwakened:
                    faction.Technology += 3;
                    faction.Stability += 2;
                    break;
            }
            return faction;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to create faction: {ex.Message}", ex);
        }
    }

    private static string GenerateFactionDescription(FactionType type)
    {
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), type),
            nameof(type) + " must be a valid FactionType."
        );
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

    private static List<string> GenerateFactionTraits(FactionType type)
    {
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), type),
            nameof(type) + " must be a valid FactionType."
        );
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

    private static void SetStartingResources(Faction faction)
    {
        Guard.IsNotNull(faction, nameof(faction));
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), faction.Type),
            nameof(faction.Type) + " must be a valid FactionType."
        );
        try
        {
            faction.Population = Random.Shared.Next(
                GameConstants.StartingPopulationMin,
                GameConstants.StartingPopulationMax + 1
            );
            faction.Military = Random.Shared.Next(
                GameConstants.StartingMilitaryMin,
                GameConstants.StartingMilitaryMax + 1
            );
            faction.Technology = Random.Shared.Next(
                GameConstants.StartingTechnologyMin,
                GameConstants.StartingTechnologyMax + 1
            );
            faction.Influence = Random.Shared.Next(
                GameConstants.StartingInfluenceMin,
                GameConstants.StartingInfluenceMax + 1
            );
            faction.Resources = Random.Shared.Next(
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
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to set starting resources: {ex.Message}", ex);
        }
    }
}
