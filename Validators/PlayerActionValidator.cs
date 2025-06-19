using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates PlayerAction objects for required fields and context-specific constraints.
/// </summary>
public class PlayerActionValidator : AbstractValidator<PlayerAction>
{
    public PlayerActionValidator()
    {
        RuleFor(a => a.ActionType).Must(a => Enum.IsDefined(a)).WithMessage("Invalid action type.");

        RuleFor(a => a.Parameters).NotNull().WithMessage("Parameters dictionary must be provided.");
    }
}
