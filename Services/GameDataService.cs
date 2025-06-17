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
public class GameDataService(ILiteDatabase db) : IGameDataService
{
    private readonly ILiteDatabase _db = db;
    private readonly GameStateValidator _gameStateValidator = new();
    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Asynchronously saves the current game state to the database.
    /// </summary>
    public async Task SaveGameAsync(GameState gameState)
    {
        Guard.IsNotNull(gameState);
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
            Console.Error.WriteLine($"[GameDataService] Error saving game: {ex.Message}");
            throw new ApplicationException("Failed to save game data.", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves all saved games, ordered by most recently played.
    /// </summary>
    public async Task<List<GameState>> GetSavedGamesAsync()
    {
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
            Console.Error.WriteLine($"[GameDataService] Error loading saved games: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Asynchronously loads a saved game by its unique ID.
    /// </summary>
    public async Task<GameState?> LoadGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId);
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
            Console.Error.WriteLine($"[GameDataService] Error loading game: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Asynchronously deletes a saved game by its unique ID.
    /// </summary>
    public async Task DeleteGameAsync(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId);
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
            Console.Error.WriteLine($"[GameDataService] Error deleting game: {ex.Message}");
        }
    }

    /// <summary>
    /// Export a game state as a JSON string.
    /// </summary>
    public string ExportGameState(GameState gameState)
    {
        Guard.IsNotNull(gameState);
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
            Console.Error.WriteLine($"[GameDataService] Error exporting game state: {ex.Message}");
            throw new ApplicationException("Failed to export game data.", ex);
        }
    }

    /// <summary>
    /// Import a game state from a JSON string.
    /// </summary>
    public GameState? ImportGameState(string json)
    {
        Guard.IsNotNullOrWhiteSpace(json);
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
                Console.Error.WriteLine(
                    $"[GameDataService] Invalid game state from import: {errors}"
                );
                return null;
            }
            return gameState;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[GameDataService] Error importing game state: {ex.Message}");
            return null;
        }
    }
}
