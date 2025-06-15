using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    public interface IEventService
    {
        Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState);
        List<string> GenerateGalacticNews(GameState gameState, List<GameEvent> recentEvents);
    }
}
