using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using LiteDB;

namespace FactionsAtTheEnd.Repositories;

/// <summary>
/// LiteDB implementation of the IGameStateRepository interface for managing game state persistence.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LiteDbGameStateRepository"/> class.
/// </remarks>
/// <param name="db">The LiteDB database instance.</param>
public class LiteDbGameStateRepository(ILiteDatabase db) : IGameStateRepository
{
    private readonly ILiteDatabase _db = db;

    /// <summary>
    /// Inserts or updates a game state in the database.
    /// </summary>
    /// <param name="gameState">The game state to upsert.</param>
    public void Upsert(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        Guard.IsNotNull(_db, nameof(_db));
        var collection = _db.GetCollection<GameState>("games");
        collection.Upsert(gameState);
    }

    /// <summary>
    /// Retrieves all game states from the database.
    /// </summary>
    /// <returns>A list of all saved game states.</returns>
    public List<GameState> GetAll()
    {
        var collection = _db.GetCollection<GameState>("games");
        return [.. collection.FindAll()];
    }

    /// <summary>
    /// Finds a game state by its unique ID.
    /// </summary>
    /// <param name="gameId">The unique ID of the game state.</param>
    /// <returns>The matching <see cref="GameState"/>, or null if not found.</returns>
    public GameState? FindById(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        var collection = _db.GetCollection<GameState>("games");
        return collection.FindById(gameId);
    }

    /// <summary>
    /// Deletes a game state by its unique ID.
    /// </summary>
    /// <param name="gameId">The unique ID of the game state to delete.</param>
    public void Delete(string gameId)
    {
        Guard.IsNotNullOrWhiteSpace(gameId, nameof(gameId));
        var collection = _db.GetCollection<GameState>("games");
        collection.Delete(gameId);
    }
}
