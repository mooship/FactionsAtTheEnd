using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces;

/// <summary>
/// Factory for rehydrating and constructing GameState and related objects.
/// </summary>
public interface IGameStateFactory
{
    /// <summary>
    /// Rehydrates the specified <see cref="GameState"/> object, restoring any necessary references or state.
    /// </summary>
    /// <param name="gameState">The game state to rehydrate.</param>
    void Rehydrate(GameState gameState);
}
