namespace FactionsAtTheEnd.UI;

/// <summary>
/// Provides templates for global achievement names and descriptions.
/// </summary>
public static class AchievementTemplates
{
    public static class Names
    {
        public const string FirstWin = "First Victory";
        public const string TechMaster = "Master of Technology";
        public const string Survivor = "Survivor";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";
        public const string LegendaryReputation = "Legendary Reputation";
        public const string Warlord = "Warlord";
        public const string TechAscendant = "Tech Ascendant";
    }

    public static class Descriptions
    {
        public const string FirstWin = "Win your first game.";
        public const string TechMaster = "Reach 100 Technology in a single game.";
        public const string Survivor = "Survive 20 cycles in a single game.";
        public const string Victory =
            "Win a game by surviving 20 turns or reaching 100 Technology.";
        public const string Defeat = "Lose a game by running out of a critical resource.";
        public const string LegendaryReputation = "Reach 100 Reputation in a single game.";
        public const string Warlord = "Reach 100 Military in a single game.";
        public const string TechAscendant = "Reach 100 Technology in a single game.";
    }
}
