using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Services;

public class FactionService
{
    public static Faction CreateFaction(string name, FactionType type, bool isPlayer = false)
    {
        var faction = new Faction
        {
            Name = name,
            Type = type,
            IsPlayer = isPlayer,
            Description = GenerateFactionDescription(type),
            Traits = GenerateFactionTraits(type),
        };

        // Set starting resources based on faction type
        SetStartingResources(faction);

        return faction;
    }

    private static string GenerateFactionDescription(FactionType type)
    {
        return type switch
        {
            FactionType.MilitaryJunta =>
                "A ruthless military organization maintaining order through force.",
            FactionType.CorporateCouncil =>
                "Mega-corporations united in pursuit of profit above all else.",
            FactionType.ReligiousOrder =>
                "Zealous believers seeking to spread their faith across the stars.",
            FactionType.PirateAlliance => "Raiders and smugglers operating outside galactic law.",
            FactionType.TechnocraticUnion =>
                "Scientists and engineers believing technology will save civilization.",
            FactionType.RebellionCell => "Freedom fighters opposing tyranny wherever they find it.",
            FactionType.ImperialRemnant => "Loyalists clinging to the glory of the fallen empire.",
            FactionType.AncientAwakened =>
                "Mysterious beings from a bygone era, recently stirred to action.",
            _ => "A faction struggling for survival in a dying galaxy.",
        };
    }

    private static List<string> GenerateFactionTraits(FactionType type)
    {
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
        // Base starting resources
        faction.Population = Random.Shared.Next(40, 71);
        faction.Military = Random.Shared.Next(30, 61);
        faction.Technology = Random.Shared.Next(25, 56);
        faction.Influence = Random.Shared.Next(20, 51);
        faction.Resources = Random.Shared.Next(35, 66);

        // Faction type bonuses
        switch (faction.Type)
        {
            case FactionType.MilitaryJunta:
                faction.Military += 20;
                break;
            case FactionType.CorporateCouncil:
                faction.Resources += 20;
                break;
            case FactionType.TechnocraticUnion:
                faction.Technology += 20;
                break;
            case FactionType.ReligiousOrder:
                faction.Influence += 15;
                faction.Population += 10;
                break;
            case FactionType.ImperialRemnant:
                faction.Influence += 20;
                break;
            case FactionType.AncientAwakened:
                faction.Technology += 25;
                faction.Population -= 15;
                break;
            // Note: Other FactionTypes like PirateAlliance, RebellionCell might need cases here
            // if they have specific starting resource bonuses.
        }

        // Player factions get a slight bonus
        if (faction.IsPlayer)
        {
            faction.Population += 10;
            faction.Resources += 10;
        }
    }
}
