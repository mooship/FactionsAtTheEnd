using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that handles game data persistence.
    /// This includes saving, loading, deleting, exporting, and importing game states.
    /// </summary>
    public interface IGameDataService
    {
        /// <summary>
        /// Asynchronously saves the current game state.
        /// </summary>
        /// <param name="gameState">The <see cref="GameState"/> to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveGameAsync(GameState gameState);

        /// <summary>
        /// Asynchronously retrieves a list of all saved games.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a list of <see cref="GameState"/> objects, typically ordered by last played.
        /// </returns>
        Task<List<GameState>> GetSavedGamesAsync();

        /// <summary>
        /// Asynchronously loads a game state by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game to load.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the loaded <see cref="GameState"/>, or null if not found.
        /// </returns>
        Task<GameState?> LoadGameAsync(string gameId);

        /// <summary>
        /// Asynchronously deletes a saved game by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteGameAsync(string gameId);

        /// <summary>
        /// Exports the given game state to a string format (e.g., JSON).
        /// </summary>
        /// <param name="gameState">The <see cref="GameState"/> to export.</param>
        /// <returns>A string representation of the game state.</returns>
        string ExportGameState(GameState gameState);

        /// <summary>
        /// Imports a game state from a string format (e.g., JSON).
        /// </summary>
        /// <param name="json">The string representation of the game state to import.</param>
        /// <returns>The imported <see cref="GameState"/>.</returns>
        GameState ImportGameState(string json);
    }
}
