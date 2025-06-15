using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    public interface IGameDataService
    {
        Task SaveGameAsync(GameState gameState);
        Task<List<GameState>> GetSavedGamesAsync();
        Task<GameState?> LoadGameAsync(string gameId);
        Task DeleteGameAsync(string gameId);
    }
}
