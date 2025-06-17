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
        /// <param name="name">The name of the faction.</param>
        /// <param name="type">The <see cref="FactionType"/> of the faction, determining its characteristics.</param>
        /// <param name="isPlayer">A boolean indicating whether this faction is controlled by the player. Defaults to false.</param>
        /// <returns>The newly created <see cref="Faction"/> object.</returns>
        Faction CreateFaction(string name, FactionType type, bool isPlayer = false);
    }
}
