using System.Text.Json;
using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Validators;
using LiteDB;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Handles saving, loading, deleting, exporting, and importing game state data using LiteDB.
/// </summary>
public class GameDataService : IGameDataService
{
    private readonly ILiteDatabase _db;
    private readonly GameStateValidator _gameStateValidator = new();
    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        WriteIndented = true,
    };
    private readonly IAppLogger _logger;

    public GameDataService(ILiteDatabase db, IAppLogger logger)
    {
        Guard.IsNotNull(db, nameof(db));
        Guard.IsNotNull(logger, nameof(logger));
        _db = db;
        _logger = logger;
        _logger.Information("GameDataService initialized with LiteDB instance.");
    }

    /// <summary>
    /// Asynchronously saves the current game state to the database.
    /// </summary>
    public async Task SaveGameAsync(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        try
        {
            await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                collection.Upsert(gameState);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error saving game");
            throw new ApplicationException("Failed to save game data.", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves all saved games, ordered by most recently played.
    /// </summary>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        Guard.IsNotNull(_db, nameof(_db));
        try
        {
            return await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                return collection.FindAll().OrderByDescending(g => g.LastPlayed).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error loading saved games");
            return [];
        }
    }

    /// <summary>
    /// Asynchronously loads a saved game by its unique ID.
    /// </summary>
    public async Task<GameState?> LoadGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        Guard.IsNotNull(_db, nameof(_db));
        try
        {
            return await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                return collection.FindById(gameId);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error loading game");
            return null;
        }
    }

    /// <summary>
    /// Asynchronously deletes a saved game by its unique ID.
    /// </summary>
    public async Task DeleteGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        Guard.IsNotNull(_db, nameof(_db));
        try
        {
            await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                collection.Delete(gameId);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error deleting game");
        }
    }

    /// <summary>
    /// Export a game state as a JSON string.
    /// </summary>
    public string ExportGameState(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        var validation = _gameStateValidator.Validate(gameState);
        Guard.IsTrue(
            validation.IsValid,
            $"Invalid game state for export: {string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))}"
        );

        try
        {
            return System.Text.Json.JsonSerializer.Serialize(gameState, CachedJsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.Error(ex, "[GameDataService] Error exporting game state");
            throw new ApplicationException("Failed to export game data.", ex);
        }
    }

    /// <summary>
    /// Import a game state from a JSON string.
    /// </summary>
    public GameState? ImportGameState(string json)
    {
        Guard.IsNotNullOrWhiteSpace(json, nameof(json));
        try
        {
            var gameState = System.Text.Json.JsonSerializer.Deserialize<GameState>(
                json,
                CachedJsonOptions
            );
            if (gameState == null)
                return null;

            var validation = _gameStateValidator.Validate(gameState);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                _logger.Warning(
                    "[GameDataService] Invalid game state from import: {Errors}",
                    errors
                );
                return null;
            }
            return gameState;
        }
        catch (JsonException ex)
        {
            _logger.Error(ex, "[GameDataService] Error importing game state");
            return null;
        }
    }
}
