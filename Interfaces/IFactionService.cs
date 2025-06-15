using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    public interface IFactionService
    {
        Faction CreateFaction(string name, FactionType type, bool isPlayer = false);
    }
}
