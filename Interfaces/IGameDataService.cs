using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that handles game data persistence, including saving, loading, deleting, exporting, and importing game states.
    /// </summary>
    public interface IGameDataService
    {
        /// <summary>
        /// Asynchronously saves the current game state.
        /// </summary>
        Task SaveGameAsync(GameState gameState);

        /// <summary>
        /// Asynchronously retrieves a list of all saved games.
        /// </summary>
        Task<List<GameState>> GetSavedGamesAsync();

        /// <summary>
        /// Asynchronously loads a game state by its unique identifier.
        /// </summary>
        Task<GameState?> LoadGameAsync(string gameId);

        /// <summary>
        /// Asynchronously deletes a saved game by its unique identifier.
        /// </summary>
        Task DeleteGameAsync(string gameId);

        /// <summary>
        /// Exports the given game state to a string format (e.g., JSON).
        /// </summary>
        string ExportGameState(GameState gameState);

        /// <summary>
        /// Imports a game state from a string format (e.g., JSON).
        /// </summary>
        GameState? ImportGameState(string json);
    }
}
