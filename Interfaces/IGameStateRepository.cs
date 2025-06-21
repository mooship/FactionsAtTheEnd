using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces;

public interface IGameStateRepository
{
    void Upsert(GameState gameState);
    List<GameState> GetAll();
    GameState? FindById(string gameId);
    void Delete(string gameId);
}
