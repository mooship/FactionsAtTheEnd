using FactionsAtTheEnd.Models;
using LiteDB;

namespace FactionsAtTheEnd.Services;

public class GameDataService
{
    private const string DatabasePath = "factionsattheend.db";

    public static async Task SaveGameAsync(GameState gameState)
    {
        try
        {
            await Task.Run(() =>
            {
                using var db = new LiteDatabase(DatabasePath);
                var collection = db.GetCollection<GameState>("games");
                collection.Upsert(gameState);
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GameDataService] Error saving game: {ex.Message}");
            throw new ApplicationException("Failed to save game data.", ex);
        }
    }

    public static async Task<List<GameState>> GetSavedGamesAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(DatabasePath);
                var collection = db.GetCollection<GameState>("games");
                return collection.FindAll().OrderByDescending(g => g.LastPlayed).ToList();
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GameDataService] Error loading saved games: {ex.Message}");
            return [];
        }
    }

    public static async Task<GameState?> LoadGameAsync(string gameId)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(DatabasePath);
                var collection = db.GetCollection<GameState>("games");
                return collection.FindById(gameId);
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GameDataService] Error loading game: {ex.Message}");
            return null;
        }
    }

    public static async Task DeleteGameAsync(string gameId)
    {
        try
        {
            await Task.Run(() =>
            {
                using var db = new LiteDatabase(DatabasePath);
                var collection = db.GetCollection<GameState>("games");
                collection.Delete(gameId);
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GameDataService] Error deleting game: {ex.Message}");
        }
    }
}
