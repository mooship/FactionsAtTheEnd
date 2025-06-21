using System.Text.Json;
using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FluentValidation;
using LiteDB;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Handles saving, loading, deleting, exporting, and importing game state data using LiteDB.
/// </summary>
public class GameDataService : IGameDataService
{
    private readonly IGameStateRepository _repository;
    private readonly IValidator<GameState> _gameStateValidator;
    private readonly IAppLogger _logger;
    private readonly IFactionService _factionService;

    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        WriteIndented = true,
    };

    public GameDataService(
        IGameStateRepository repository,
        IAppLogger logger,
        IFactionService factionService,
        IValidator<GameState> gameStateValidator
    )
    {
        Guard.IsNotNull(repository, nameof(repository));
        Guard.IsNotNull(logger, nameof(logger));
        Guard.IsNotNull(factionService, nameof(factionService));
        Guard.IsNotNull(gameStateValidator, nameof(gameStateValidator));
        _repository = repository;
        _logger = logger;
        _factionService = factionService;
        _gameStateValidator = gameStateValidator;
        _logger.Information("GameDataService initialized with repository instance.");
    }

    /// <summary>
    /// Asynchronously saves the current game state to the database.
    /// </summary>
    public async Task SaveGameAsync(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        _logger.Debug("Saving game: {SaveName}", gameState.SaveName);
        try
        {
            await Task.Run(() =>
            {
                _repository.Upsert(gameState);
            });
            _logger.Information("Game saved: {SaveName}", gameState.SaveName);
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "[GameDataService] Error saving game: {SaveName}",
                gameState.SaveName
            );
            throw new ApplicationException("Failed to save game data.", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves all saved games, ordered by most recently played.
    /// </summary>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
        _logger.Debug("Loading all saved games.");
        try
        {
            var games = await Task.Run(() =>
            {
                return _repository.GetAll().OrderByDescending(g => g.LastPlayed).ToList();
            });
            _logger.Information("Loaded {Count} saved games.", games.Count);
            return games;
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
        _logger.Debug("Loading game with ID: {GameId}", gameId);
        try
        {
            var game = await Task.Run(() =>
            {
                return _repository.FindById(gameId);
            });
            if (game != null)
            {
                _factionService.RehydrateStaticFields(game.PlayerFaction);
                _logger.Information("Game loaded: {SaveName}", game.SaveName);
            }
            else
                _logger.Warning("No game found with ID: {GameId}", gameId);
            return game;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error loading game: {GameId}", gameId);
            return null;
        }
    }

    /// <summary>
    /// Asynchronously deletes a saved game by its unique ID.
    /// </summary>
    public async Task DeleteGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        _logger.Debug("Deleting game with ID: {GameId}", gameId);
        try
        {
            await Task.Run(() =>
            {
                _repository.Delete(gameId);
            });
            _logger.Information("Game deleted: {GameId}", gameId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GameDataService] Error deleting game: {GameId}", gameId);
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
        _logger.Debug("Exporting game state: {SaveName}", gameState.SaveName);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(gameState, CachedJsonOptions);
            _logger.Information("Game state exported: {SaveName}", gameState.SaveName);
            return json;
        }
        catch (JsonException ex)
        {
            _logger.Error(
                ex,
                "[GameDataService] Error exporting game state: {SaveName}",
                gameState.SaveName
            );
            throw new ApplicationException("Failed to export game data.", ex);
        }
    }

    /// <summary>
    /// Import a game state from a JSON string.
    /// </summary>
    public GameState? ImportGameState(string json)
    {
        Guard.IsNotNullOrWhiteSpace(json, nameof(json));
        _logger.Debug("Importing game state from JSON");
        try
        {
            var gameState = System.Text.Json.JsonSerializer.Deserialize<GameState>(
                json,
                CachedJsonOptions
            );
            if (gameState == null)
            {
                _logger.Warning("Deserialized game state is null");
                return null;
            }
            var validation = _gameStateValidator.Validate(gameState);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                _logger.Warning("Invalid game state from import: {Errors}", errors);
                return null;
            }
            _logger.Information("Game state imported successfully: {SaveName}", gameState.SaveName);
            return gameState;
        }
        catch (JsonException ex)
        {
            _logger.Error(ex, "[GameDataService] Error importing game state");
            return null;
        }
    }
}
