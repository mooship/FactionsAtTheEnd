using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using LiteDB;

namespace FactionsAtTheEnd.Repositories;

/// <summary>
/// Unified LiteDB repository that handles all data persistence operations.
/// Manages database lifecycle and provides operations for both game states and achievements.
/// </summary>
public class LiteDbDataRepository : IDataRepository
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<GameState> _gameStateCollection;
    private readonly ILiteCollection<GlobalAchievement> _achievementCollection;
    private readonly IAppLogger _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbDataRepository"/> class.
    /// </summary>
    /// <param name="connectionString">The database connection string/path.</param>
    /// <param name="logger">The application logger.</param>
    public LiteDbDataRepository(string connectionString, IAppLogger logger)
    {
        Guard.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
        Guard.IsNotNull(logger, nameof(logger));

        _logger = logger;
        _database = new LiteDatabase(connectionString);
        _gameStateCollection = _database.GetCollection<GameState>("games");
        _achievementCollection = _database.GetCollection<GlobalAchievement>("global_achievements");
        _achievementCollection.EnsureIndex(x => x.Name, true);

        _logger.Information(
            "Unified repository initialized with connection: {ConnectionString}",
            connectionString
        );
    }

    /// <summary>
    /// Inserts or updates a game state in the database.
    /// </summary>
    /// <param name="gameState">The game state to upsert.</param>
    public void UpsertGameState(GameState gameState)
    {
        ThrowIfDisposed();
        Guard.IsNotNull(gameState, nameof(gameState));

        _logger.Debug("Upserting game state: {SaveName}", gameState.SaveName);
        _gameStateCollection.Upsert(gameState);
        _logger.Debug("Game state upserted successfully: {SaveName}", gameState.SaveName);
    }

    /// <summary>
    /// Retrieves all game states from the database.
    /// </summary>
    /// <returns>A list of all saved game states.</returns>
    public List<GameState> GetAllGameStates()
    {
        ThrowIfDisposed();
        _logger.Debug("Retrieving all game states.");
        var gameStates = _gameStateCollection.FindAll().ToList();
        _logger.Debug("Retrieved {Count} game states.", gameStates.Count);
        return gameStates;
    }

    /// <summary>
    /// Finds a game state by its unique ID.
    /// </summary>
    /// <param name="gameId">The unique ID of the game state.</param>
    /// <returns>The matching <see cref="GameState"/>, or null if not found.</returns>
    public GameState? FindGameStateById(string gameId)
    {
        ThrowIfDisposed();
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));

        _logger.Debug("Finding game state by ID: {GameId}", gameId);
        var gameState = _gameStateCollection.FindById(gameId);
        _logger.Debug("Game state found: {Found}", gameState != null);
        return gameState;
    }

    /// <summary>
    /// Deletes a game state by its unique ID.
    /// </summary>
    /// <param name="gameId">The unique ID of the game state to delete.</param>
    public void DeleteGameState(string gameId)
    {
        ThrowIfDisposed();
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));

        _logger.Debug("Deleting game state with ID: {GameId}", gameId);
        var deleted = _gameStateCollection.Delete(gameId);
        _logger.Debug("Game state deletion result: {Deleted}", deleted);
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

        _logger.Debug("Unlocking achievement: {Name}", name);

        if (!IsAchievementUnlocked(name))
        {
            var achievement = new GlobalAchievement
            {
                Name = name,
                Description = description,
                UnlockedAt = DateTime.UtcNow,
            };
            _achievementCollection.Insert(achievement);
            _logger.Information("Achievement unlocked: {Name}", name);
        }
        else
        {
            _logger.Debug("Achievement already unlocked: {Name}", name);
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

        _logger.Debug("Checking if achievement is unlocked: {Name}", name);
        var exists = _achievementCollection.Exists(x => x.Name == name);
        _logger.Debug("Achievement exists: {Exists}", exists);
        return exists;
    }

    /// <summary>
    /// Gets all unlocked global achievements.
    /// </summary>
    /// <returns>List of unlocked <see cref="GlobalAchievement"/> objects.</returns>
    public List<GlobalAchievement> GetAllAchievements()
    {
        ThrowIfDisposed();
        _logger.Debug("Retrieving all global achievements.");
        var achievements = _achievementCollection.FindAll().ToList();
        _logger.Information("Retrieved {Count} global achievements.", achievements.Count);
        return achievements;
    }

    /// <summary>
    /// Disposes the repository and closes the database connection.
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
            try
            {
                _database?.Dispose();
                _logger.Information("Database connection closed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while closing database connection.");
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the repository has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
