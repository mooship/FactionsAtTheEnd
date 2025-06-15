using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using LiteDB;

namespace FactionsAtTheEnd.Services;

public class GameDataService(ILiteDatabase db) : IGameDataService
{
    private readonly ILiteDatabase _db = db;

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
}
