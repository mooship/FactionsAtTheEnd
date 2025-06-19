using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using LiteDB;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Service for managing global achievements that persist across all games for a player.
/// </summary>
public class GlobalAchievementService : IGlobalAchievementService
{
    private readonly ILiteDatabase _db;
    private readonly ILiteCollection<GlobalAchievement> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalAchievementService"/> class.
    /// </summary>
    /// <param name="db">The LiteDB database instance.</param>
    public GlobalAchievementService(ILiteDatabase db)
    {
        Guard.IsNotNull(db, nameof(db));
        _db = db;
        _collection = _db.GetCollection<GlobalAchievement>("global_achievements");
        _collection.EnsureIndex(x => x.Name, true);
    }

    /// <summary>
    /// Unlocks a global achievement if it is not already unlocked.
    /// </summary>
    /// <param name="name">The achievement name.</param>
    /// <param name="description">The achievement description.</param>
    public void UnlockAchievement(string name, string description)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Guard.IsNotNullOrWhiteSpace(description, nameof(description));
        if (!IsAchievementUnlocked(name))
        {
            var achievement = new GlobalAchievement
            {
                Name = name,
                Description = description,
                UnlockedAt = DateTime.UtcNow,
            };
            _collection.Insert(achievement);
        }
    }

    /// <summary>
    /// Checks if a global achievement is already unlocked.
    /// </summary>
    /// <param name="name">The achievement name.</param>
    /// <returns>True if the achievement is unlocked; otherwise, false.</returns>
    public bool IsAchievementUnlocked(string name)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        return _collection.Exists(x => x.Name == name);
    }

    /// <summary>
    /// Gets all unlocked global achievements.
    /// </summary>
    /// <returns>List of unlocked <see cref="GlobalAchievement"/> objects.</returns>
    public List<GlobalAchievement> GetAllAchievements()
    {
        Guard.IsNotNull(_collection, nameof(_collection));
        return [.. _collection.FindAll()];
    }
}
