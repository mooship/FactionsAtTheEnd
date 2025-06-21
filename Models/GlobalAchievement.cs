namespace FactionsAtTheEnd.Models
{
    /// <summary>
    /// Represents a global achievement unlocked by the player, persisting across all games.
    /// </summary>
    public class GlobalAchievement
    {
        /// <summary>
        /// Gets or sets the unique identifier for the achievement.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the name of the achievement.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the achievement.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the achievement was unlocked.
        /// </summary>
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
