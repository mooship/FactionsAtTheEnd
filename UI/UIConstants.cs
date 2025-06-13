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
