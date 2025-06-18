using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that manages global achievements persisting across all games.
    /// </summary>
    public interface IGlobalAchievementService
    {
        /// <summary>
        /// Adds a global achievement if not already unlocked.
        /// </summary>
        void UnlockAchievement(string name, string description);

        /// <summary>
        /// Checks if a global achievement is already unlocked.
        /// </summary>
        bool IsAchievementUnlocked(string name);

        /// <summary>
        /// Gets all unlocked global achievements.
        /// </summary>
        List<GlobalAchievement> GetAllAchievements();
    }
}
