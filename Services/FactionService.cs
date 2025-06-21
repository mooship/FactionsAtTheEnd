using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Constants;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;
using Serilog;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Service for creating and managing Faction entities.
/// </summary>
public class FactionService : IFactionService
{
    /// <summary>
    /// Application logger for diagnostic and informational messages.
    /// </summary>
    private readonly IAppLogger _logger;

    /// <summary>
    /// Provider for faction-type-specific data and logic.
    /// </summary>
    private readonly IFactionTypeProvider _typeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactionService"/> class.
    /// </summary>
    /// <param name="logger">The application logger.</param>
    /// <param name="typeProvider">The provider for faction type data and logic.</param>
    public FactionService(IAppLogger logger, IFactionTypeProvider typeProvider)
    {
        _logger = logger;
        _typeProvider = typeProvider;
    }

    /// <summary>
    /// Creates a new faction with the specified name, type, and player status.
    /// </summary>
    /// <param name="name">The name of the faction.</param>
    /// <param name="type">The type of the faction.</param>
    /// <param name="isPlayer">Whether the faction is controlled by the player.</param>
    /// <returns>The created <see cref="Faction"/> instance.</returns>
    /// <exception cref="ApplicationException">Thrown if faction creation fails.</exception>
    public Faction CreateFaction(string name, FactionType type, bool isPlayer = false)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Guard.IsTrue(
            Enum.IsDefined(typeof(FactionType), type),
            nameof(type) + " must be a valid FactionType."
        );
        _logger.Debug($"Creating faction: {name} ({type})");
        try
        {
            var faction = new Faction
            {
                Name = name,
                Type = type,
                IsPlayer = isPlayer,
                Description = _typeProvider.GetDescription(type),
                Traits = _typeProvider.GetTraits(type),
                Reputation = 25,
            };
            Guard.IsNotNull(faction.Description, nameof(faction.Description));
            Guard.IsNotNull(faction.Traits, nameof(faction.Traits));
            _typeProvider.SetStartingResources(faction);
            _logger.Information($"Faction created: {name}");
            switch (type)
            {
                case FactionType.MilitaryJunta:
                    faction.Military += 5;
                    break;
                case FactionType.CorporateCouncil:
                    faction.Resources += 5;
                    break;
                case FactionType.ReligiousOrder:
                    faction.Stability += 5;
                    break;
                case FactionType.PirateAlliance:
                    faction.Influence += 5;
                    break;
                case FactionType.TechnocraticUnion:
                    faction.Technology += 5;
                    break;
                case FactionType.RebellionCell:
                    faction.Stability += 3;
                    faction.Influence += 2;
                    break;
                case FactionType.ImperialRemnant:
                    faction.Population += 4;
                    faction.Reputation += 1;
                    break;
                case FactionType.AncientAwakened:
                    faction.Technology += 3;
                    faction.Stability += 2;
                    break;
            }
            return faction;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to create faction: {name}");
            throw new ApplicationException($"Failed to create faction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rehydrates static/derived fields (Description, Traits) for a Faction after deserialization.
    /// </summary>
    /// <param name="faction">The faction to rehydrate.</param>
    public void RehydrateStaticFields(Faction faction)
    {
        Guard.IsNotNull(faction, nameof(faction));
        faction.Description = _typeProvider.GetDescription(faction.Type);
        faction.Traits = _typeProvider.GetTraits(faction.Type);
    }
}
