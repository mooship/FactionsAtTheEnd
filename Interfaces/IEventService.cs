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
        Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState);

        /// <summary>
        /// Generates a list of galactic news headlines based on the current game state and recent events.
        /// </summary>
        List<string> GenerateGalacticNews(GameState gameState, List<GameEvent> recentEvents);
    }
}
