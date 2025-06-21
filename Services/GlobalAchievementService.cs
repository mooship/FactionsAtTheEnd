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
    private readonly IAppLogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalAchievementService"/> class.
    /// </summary>
    /// <param name="db">The LiteDB database instance.</param>
    /// <param name="logger">The application logger instance.</param>
    public GlobalAchievementService(ILiteDatabase db, IAppLogger logger)
    {
        Guard.IsNotNull(db, nameof(db));
        Guard.IsNotNull(logger, nameof(logger));
        _db = db;
        _logger = logger;
        _collection = _db.GetCollection<GlobalAchievement>("global_achievements");
        _collection.EnsureIndex(x => x.Name, true);
        _logger.Debug("GlobalAchievementService initialized.");
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
        _logger.Debug("Unlocking achievement: {Name}", name);
        try
        {
            if (!IsAchievementUnlocked(name))
            {
                var achievement = new GlobalAchievement
                {
                    Name = name,
                    Description = description,
                    UnlockedAt = DateTime.UtcNow,
                };
                _collection.Insert(achievement);
                _logger.Information("Achievement unlocked: {Name}", name);
            }
            else
            {
                _logger.Debug("Achievement already unlocked: {Name}", name);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "[GlobalAchievementService] Failed to unlock achievement: {Name}",
                name
            );
            throw new ApplicationException($"Failed to unlock achievement: {ex.Message}", ex);
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
        _logger.Debug("Checking if achievement is unlocked: {Name}", name);
        try
        {
            return _collection.Exists(x => x.Name == name);
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "[GlobalAchievementService] Failed to check achievement: {Name}",
                name
            );
            throw new ApplicationException($"Failed to check achievement: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all unlocked global achievements.
    /// </summary>
    /// <returns>List of unlocked <see cref="GlobalAchievement"/> objects.</returns>
    public List<GlobalAchievement> GetAllAchievements()
    {
        Guard.IsNotNull(_collection, nameof(_collection));
        _logger.Debug("Retrieving all global achievements.");
        try
        {
            var achievements = _collection.FindAll().ToList();
            _logger.Information("Retrieved {Count} global achievements.", achievements.Count);
            return achievements;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GlobalAchievementService] Failed to get all achievements");
            throw new ApplicationException($"Failed to get achievements: {ex.Message}", ex);
        }
    }
}
