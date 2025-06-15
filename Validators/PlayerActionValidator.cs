using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates player actions for type and faction ID constraints.
/// </summary>
public class PlayerActionValidator : AbstractValidator<PlayerAction>
{
    public PlayerActionValidator()
    {
        RuleFor(a => a.ActionType).Must(a => Enum.IsDefined(a)).WithMessage("Invalid action type.");

        RuleFor(a => a.FactionId).NotEmpty().WithMessage("Faction ID is required.");
    }
}
