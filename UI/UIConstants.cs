namespace FactionsAtTheEnd.UI;

public enum MenuOption
{
    NewGame,
    LoadGame,
    Help,
    Exit,
    TakeAction,
    ViewFactionOverview,
    ExitToMainMenu,
    FinishTurn,
}

public enum StatKey
{
    Population,
    Military,
    Technology,
    Influence,
    Resources,
    Stability,
}

public static class MenuOptionExtensions
{
    public static string GetDisplayName(this MenuOption option)
    {
        return option switch
        {
            MenuOption.NewGame => "New Game",
            MenuOption.LoadGame => "Load Game",
            MenuOption.Help => "Help",
            MenuOption.Exit => "Exit",
            MenuOption.TakeAction => "Take Action",
            MenuOption.ViewFactionOverview => "View Faction Overview",
            MenuOption.ExitToMainMenu => "Exit to Main Menu",
            MenuOption.FinishTurn => "Finish Turn",
            _ => option.ToString(),
        };
    }
}

public static class StatKeyExtensions
{
    public static string GetDisplayName(this StatKey key)
    {
        return key switch
        {
            StatKey.Population => "Population",
            StatKey.Military => "Military",
            StatKey.Technology => "Technology",
            StatKey.Influence => "Influence",
            StatKey.Resources => "Resources",
            StatKey.Stability => "Stability",
            _ => key.ToString(),
        };
    }
}

public static class FactionDescriptions
{
    public const string MilitaryJunta =
        "A ruthless military organization maintaining order through force.";
    public const string CorporateCouncil =
        "Mega-corporations united in pursuit of profit above all else.";
    public const string ReligiousOrder =
        "Zealous believers seeking to spread their faith across the stars.";
    public const string PirateAlliance = "Raiders and smugglers operating outside galactic law.";
    public const string TechnocraticUnion =
        "Scientists and engineers believing technology will save civilization.";
    public const string RebellionCell = "Freedom fighters opposing tyranny wherever they find it.";
    public const string ImperialRemnant = "Loyalists clinging to the glory of the fallen empire.";
    public const string AncientAwakened =
        "Mysterious beings from a bygone era, recently stirred to action.";
    public const string Default = "A faction struggling for survival in a dying galaxy.";
}

public static class GameConstants
{
    public const int MaxStat = 100;
    public const int MinStat = 0;
    public const int StartingPopulationMin = 40;
    public const int StartingPopulationMax = 70;
    public const int StartingMilitaryMin = 30;
    public const int StartingMilitaryMax = 60;
    public const int StartingTechnologyMin = 25;
    public const int StartingTechnologyMax = 55;
    public const int StartingInfluenceMin = 20;
    public const int StartingInfluenceMax = 50;
    public const int StartingResourcesMin = 35;
    public const int StartingResourcesMax = 65;
}

public static class ActionDescriptions
{
    public const string BuildDefenses = "Increase your defenses to resist attacks.";
    public const string RecruitTroops = "Recruit new soldiers to boost military.";
    public const string DevelopInfrastructure = "Improve facilities for long-term growth.";
    public const string ExploitResources = "Gather more resources for your faction.";
    public const string MilitaryTech = "Research new military technologies.";
    public const string EconomicTech = "Research economic improvements.";
    public const string AncientStudies = "Study ancient relics for unique benefits.";
    public const string GateNetworkResearch = "Research the lost gate network.";
    public const string Diplomacy = "Engage in diplomatic efforts to influence other factions.";
    public const string Espionage = "Gather intelligence or disrupt rivals through covert means.";
    public const string Sabotage = "Undermine enemy operations or infrastructure.";
    public const string Default = "(No description available)";
}
