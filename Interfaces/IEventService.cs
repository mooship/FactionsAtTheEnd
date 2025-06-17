using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that handles the generation of game events and galactic news.
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Asynchronously generates a list of random game events based on the current game state.
        /// </summary>
        /// <param name="gameState">The current <see cref="GameState"/>, used to influence event generation probabilities and types.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a list of <see cref="GameEvent"/> objects.
        /// </returns>
        Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState);

        /// <summary>
        /// Generates a list of galactic news headlines based on the current game state and recent events.
        /// </summary>
        /// <param name="gameState">The current <see cref="GameState"/>.</param>
        /// <param name="recentEvents">A list of <see cref="GameEvent"/> that have recently occurred, which might influence news generation.</param>
        /// <returns>A list of strings, where each string is a news headline.</returns>
        List<string> GenerateGalacticNews(GameState gameState, List<GameEvent> recentEvents);
    }
}
