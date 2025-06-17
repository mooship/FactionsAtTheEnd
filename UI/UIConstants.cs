namespace FactionsAtTheEnd.UI;

/// <summary>
/// Main menu and in-game menu options for the UI.
/// </summary>
public enum MenuOption
{
    NewGame,
    LoadGame,
    Help,
    Exit,
    TakeAction,
    ViewFactionOverview,
    ViewEventLog,
    ExitToMainMenu,
    FinishTurn,
    ExportSave,
    ImportSave,
}

/// <summary>
/// Used for stat display and event effects.
/// </summary>
public enum StatKey
{
    Population,
    Military,
    Technology,
    Influence,
    Resources,
    Stability,
    Reputation,
}

public static class MenuOptionExtensions
{
    /// <summary>
    /// Get the display name for a MenuOption for UI display.
    /// </summary>
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
            MenuOption.ViewEventLog => "View Event Log",
            MenuOption.ExportSave => "Export Save (JSON)",
            MenuOption.ImportSave => "Import Save (JSON)",
            _ => option.ToString(),
        };
    }
}

public static class StatKeyExtensions
{
    /// <summary>
    /// Get the display name for a StatKey for UI display.
    /// </summary>
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
            StatKey.Reputation => "Reputation",
            _ => key.ToString(),
        };
    }
}

/// <summary>
/// Centralized stat and resource bounds for all factions.
/// </summary>
public static class GameConstants
{
    public const int MaxStat = 100;
    public const int MinStat = 0;
    public const int MinReputation = -100;
    public const int MaxReputation = 100;
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
