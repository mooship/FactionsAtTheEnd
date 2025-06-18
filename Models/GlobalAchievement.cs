namespace FactionsAtTheEnd.Models
{
    /// <summary>
    /// Represents a global achievement unlocked by the player, persisting across all games.
    /// </summary>
    public class GlobalAchievement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
