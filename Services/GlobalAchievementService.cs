using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Service for managing global achievements that persist across all games for a player.
/// Delegates operations to the unified data repository.
/// </summary>
public class GlobalAchievementService : IGlobalAchievementService
{
    private readonly IDataRepository _dataRepository;
    private readonly IAppLogger _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalAchievementService"/> class.
    /// </summary>
    /// <param name="dataRepository">The unified data repository.</param>
    /// <param name="logger">The application logger instance.</param>
    public GlobalAchievementService(IDataRepository dataRepository, IAppLogger logger)
    {
        Guard.IsNotNull(dataRepository, nameof(dataRepository));
        Guard.IsNotNull(logger, nameof(logger));
        _dataRepository = dataRepository;
        _logger = logger;
        _logger.Debug("GlobalAchievementService initialized with unified repository.");
    }

    /// <summary>
    /// Unlocks a global achievement if it is not already unlocked.
    /// </summary>
    /// <param name="name">The achievement name.</param>
    /// <param name="description">The achievement description.</param>
    public void UnlockAchievement(string name, string description)
    {
        ThrowIfDisposed();
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Guard.IsNotNullOrWhiteSpace(description, nameof(description));

        try
        {
            _dataRepository.UnlockAchievement(name, description);
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
        ThrowIfDisposed();
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));

        try
        {
            return _dataRepository.IsAchievementUnlocked(name);
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
        ThrowIfDisposed();

        try
        {
            return _dataRepository.GetAllAchievements();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GlobalAchievementService] Failed to get all achievements");
            throw new ApplicationException($"Failed to get achievements: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disposes the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of the dispose pattern.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Note: We don't dispose the repository here as it's shared
            // The repository disposal is handled by the DI container
            _disposed = true;
        }
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the service has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
