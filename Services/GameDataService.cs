using System.Text.Json;
using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Validators;
using LiteDB;

namespace FactionsAtTheEnd.Services;

public class GameDataService(ILiteDatabase db) : IGameDataService
{
    private readonly ILiteDatabase _db = db;
    private readonly GameStateValidator _gameStateValidator = new();

    /// <summary>
    /// Save or update a game state in the database.
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
    /// Get all saved games, ordered by last played (most recent first).
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
    /// Load a saved game by its unique ID.
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
    /// Delete a saved game by its unique ID.
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
        if (!validation.IsValid)
        {
            throw new ApplicationException(
                $"Cannot export: GameState is invalid.\n{string.Join("\n", validation.Errors.Select(e => e.ErrorMessage))}"
            );
        }
        var options = new JsonSerializerOptions { WriteIndented = true };
        return System.Text.Json.JsonSerializer.Serialize(gameState, options);
    }

    /// <summary>
    /// Import a game state from a JSON string.
    /// </summary>
    public GameState ImportGameState(string json)
    {
        Guard.IsNotNullOrWhiteSpace(json);
        var gameState =
            System.Text.Json.JsonSerializer.Deserialize<GameState>(json)
            ?? throw new ApplicationException("Failed to import game state from JSON.");
        var validation = _gameStateValidator.Validate(gameState);
        if (!validation.IsValid)
        {
            throw new ApplicationException(
                $"Imported GameState is invalid.\n{string.Join("\n", validation.Errors.Select(e => e.ErrorMessage))}"
            );
        }
        return gameState;
    }
}
