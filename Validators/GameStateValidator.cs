using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates GameState objects for save/load operations.
/// Ensures all critical fields and nested objects are valid.
/// </summary>
public class GameStateValidator : AbstractValidator<GameState>
{
    public GameStateValidator()
    {
        RuleFor(g => g.Id).NotEmpty();
        RuleFor(g => g.SaveName).NotEmpty();
        RuleFor(g => g.CurrentCycle).GreaterThanOrEqualTo(1);
        RuleFor(g => g.PlayerFaction).NotNull().SetValidator(new FactionValidator());
        RuleFor(g => g.GalacticStability).InclusiveBetween(0, 100);
        RuleFor(g => g.GateNetworkIntegrity).InclusiveBetween(0, 100);
        RuleFor(g => g.AncientTechDiscovery).InclusiveBetween(0, 100);
        RuleForEach(g => g.RecentEvents).NotNull();
        RuleForEach(g => g.GalacticHistory).NotNull();
        RuleForEach(g => g.BlockedActions).IsInEnum();
        RuleForEach(g => g.RecentActionCounts.Keys).IsInEnum();
        RuleForEach(g => g.GalacticNews).NotNull();
        RuleFor(g => g.SaveVersion).GreaterThanOrEqualTo(1);
        RuleForEach(g => g.Achievements).NotNull();
    }
}
