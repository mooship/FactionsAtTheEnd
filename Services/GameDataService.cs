using System.Text.Json;
using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Validators;
using LiteDB;
using Serilog;

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
    private static readonly ILogger _serilog = Log.Logger;
    private readonly IFactionService _factionService;

    public GameDataService(ILiteDatabase db, IAppLogger logger, IFactionService factionService)
    {
        Guard.IsNotNull(db, nameof(db));
        Guard.IsNotNull(logger, nameof(logger));
        Guard.IsNotNull(factionService, nameof(factionService));
        _db = db;
        _logger = logger;
        _factionService = factionService;
        _logger.Information("GameDataService initialized with LiteDB instance.");
        _serilog.Debug("GameDataService constructed.");
    }

    /// <summary>
    /// Asynchronously saves the current game state to the database.
    /// </summary>
    public async Task SaveGameAsync(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        _serilog.Debug("Saving game: {SaveName}", gameState.SaveName);
        try
        {
            await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                collection.Upsert(gameState);
            });
            _serilog.Information("Game saved: {SaveName}", gameState.SaveName);
        }
        catch (Exception ex)
        {
            _serilog.Error(
                ex,
                "[GameDataService] Error saving game: {SaveName}",
                gameState.SaveName
            );
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
        _serilog.Debug("Loading all saved games.");
        try
        {
            var games = await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                return collection.FindAll().OrderByDescending(g => g.LastPlayed).ToList();
            });
            _serilog.Information("Loaded {Count} saved games.", games.Count);
            return games;
        }
        catch (Exception ex)
        {
            _serilog.Error(ex, "[GameDataService] Error loading saved games");
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
        _serilog.Debug("Loading game with ID: {GameId}", gameId);
        try
        {
            var game = await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                return collection.FindById(gameId);
            });
            if (game != null)
            {
                _factionService.RehydrateStaticFields(game.PlayerFaction);
                _serilog.Information("Game loaded: {SaveName}", game.SaveName);
            }
            else
                _serilog.Warning("No game found with ID: {GameId}", gameId);
            return game;
        }
        catch (Exception ex)
        {
            _serilog.Error(ex, "[GameDataService] Error loading game: {GameId}", gameId);
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
        _serilog.Debug("Deleting game with ID: {GameId}", gameId);
        try
        {
            await Task.Run(() =>
            {
                var collection = _db.GetCollection<GameState>("games");
                collection.Delete(gameId);
            });
            _serilog.Information("Game deleted: {GameId}", gameId);
        }
        catch (Exception ex)
        {
            _serilog.Error(ex, "[GameDataService] Error deleting game: {GameId}", gameId);
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
        _serilog.Debug("Exporting game state: {SaveName}", gameState.SaveName);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(gameState, CachedJsonOptions);
            _serilog.Information("Game state exported: {SaveName}", gameState.SaveName);
            return json;
        }
        catch (JsonException ex)
        {
            _serilog.Error(
                ex,
                "[GameDataService] Error exporting game state: {SaveName}",
                gameState.SaveName
            );
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
        _serilog.Debug("Importing game state from JSON");
        try
        {
            var gameState = System.Text.Json.JsonSerializer.Deserialize<GameState>(
                json,
                CachedJsonOptions
            );
            if (gameState == null)
            {
                _serilog.Warning("Deserialized game state is null");
                return null;
            }
            var validation = _gameStateValidator.Validate(gameState);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                _serilog.Warning("Invalid game state from import: {Errors}", errors);
                _logger.Warning(
                    "[GameDataService] Invalid game state from import: {Errors}",
                    errors
                );
                return null;
            }
            _serilog.Information(
                "Game state imported successfully: {SaveName}",
                gameState.SaveName
            );
            return gameState;
        }
        catch (JsonException ex)
        {
            _serilog.Error(ex, "[GameDataService] Error importing game state");
            _logger.Error(ex, "[GameDataService] Error importing game state");
            return null;
        }
    }
}
