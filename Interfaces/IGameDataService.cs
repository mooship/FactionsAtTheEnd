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
        /// <param name="gameState">The game state to save.</param>
        Task SaveGameAsync(GameState gameState);

        /// <summary>
        /// Asynchronously retrieves a list of all saved games.
        /// </summary>
        /// <returns>A list of all saved <see cref="GameState"/> objects.</returns>
        Task<List<GameState>> GetSavedGamesAsync();

        /// <summary>
        /// Asynchronously loads a game state by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game state.</param>
        /// <returns>The loaded <see cref="GameState"/>, or null if not found.</returns>
        Task<GameState?> LoadGameAsync(string gameId);

        /// <summary>
        /// Asynchronously deletes a saved game by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game state to delete.</param>
        Task DeleteGameAsync(string gameId);

        /// <summary>
        /// Exports the given game state to a string format (e.g., JSON).
        /// </summary>
        /// <param name="gameState">The game state to export.</param>
        /// <returns>The exported game state as a string.</returns>
        string ExportGameState(GameState gameState);

        /// <summary>
        /// Imports a game state from a string format (e.g., JSON).
        /// </summary>
        /// <param name="json">The JSON string representing the game state.</param>
        /// <returns>The imported <see cref="GameState"/>, or null if import fails.</returns>
        GameState? ImportGameState(string json);
    }
}
