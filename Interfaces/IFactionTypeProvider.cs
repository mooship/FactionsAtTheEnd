using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.Interfaces;

/// <summary>
/// Provides an abstraction for retrieving faction-type-specific data and logic.
/// </summary>
public interface IFactionTypeProvider
{
    /// <summary>
    /// Gets the description for a given faction type.
    /// </summary>
    /// <param name="type">The faction type.</param>
    /// <returns>The description string for the faction type.</returns>
    string GetDescription(FactionType type);

    /// <summary>
    /// Gets the list of traits for a given faction type.
    /// </summary>
    /// <param name="type">The faction type.</param>
    /// <returns>A list of trait strings for the faction type.</returns>
    List<string> GetTraits(FactionType type);

    /// <summary>
    /// Sets the starting resources for a faction based on its type.
    /// </summary>
    /// <param name="faction">The faction to initialize.</param>
    void SetStartingResources(Faction faction);
}
