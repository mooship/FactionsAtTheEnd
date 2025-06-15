using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    public interface IEventService
    {
        Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState);
    }
}
