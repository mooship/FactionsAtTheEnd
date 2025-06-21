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
        /// <param name="name">The name of the achievement.</param>
        /// <param name="description">The description of the achievement.</param>
        void UnlockAchievement(string name, string description);

        /// <summary>
        /// Checks if a global achievement is already unlocked.
        /// </summary>
        /// <param name="name">The name of the achievement to check.</param>
        /// <returns>True if the achievement is unlocked; otherwise, false.</returns>
        bool IsAchievementUnlocked(string name);

        /// <summary>
        /// Gets all unlocked global achievements.
        /// </summary>
        /// <returns>A list of all unlocked <see cref="GlobalAchievement"/> objects.</returns>
        List<GlobalAchievement> GetAllAchievements();
    }
}
