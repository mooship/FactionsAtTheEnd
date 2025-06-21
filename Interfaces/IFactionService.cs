using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that manages faction creation and potentially other faction-related logic.
    /// </summary>
    public interface IFactionService
    {
        /// <summary>
        /// Creates a new faction with specified attributes.
        /// </summary>
        Faction CreateFaction(string name, FactionType type, bool isPlayer = false);

        /// <summary>
        /// Rehydrates the static fields of the given faction.
        /// </summary>
        void RehydrateStaticFields(Faction faction);
    }
}
