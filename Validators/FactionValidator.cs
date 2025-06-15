using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

// Validates Faction objects for name and type constraints.
public class FactionValidator : AbstractValidator<Faction>
{
    public FactionValidator()
    {
        RuleFor(f => f.Name)
            .NotEmpty()
            .WithMessage("Faction name is required.")
            .MaximumLength(32)
            .WithMessage("Faction name must be 32 characters or fewer.");

        RuleFor(f => f.Type).IsInEnum().WithMessage("Invalid faction type.");
    }
}
