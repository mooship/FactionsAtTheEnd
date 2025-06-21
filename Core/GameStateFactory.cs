using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Core;

/// <summary>
/// Default implementation of IGameStateFactory for rehydrating GameState objects.
/// </summary>
public class GameStateFactory(IFactionService factionService) : IGameStateFactory
{
    public void Rehydrate(GameState gameState)
    {
        if (gameState?.PlayerFaction != null)
        {
            factionService.RehydrateStaticFields(gameState.PlayerFaction);
        }
    }
}
