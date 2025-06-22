using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces;

/// <summary>
/// Unified repository interface for all data persistence operations.
/// Combines game state and achievement management with proper resource disposal.
/// </summary>
public interface IDataRepository : IDisposable
{
    void UpsertGameState(GameState gameState);
    List<GameState> GetAllGameStates();
    GameState? FindGameStateById(string gameId);
    void DeleteGameState(string gameId);
    void UnlockAchievement(string name, string description);
    bool IsAchievementUnlocked(string name);
    List<GlobalAchievement> GetAllAchievements();
}
