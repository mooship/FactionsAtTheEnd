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

        RuleFor(a => a.FactionId).NotEmpty().WithMessage("Faction ID is required.");

        // TargetId is required for attack or spy actions
        RuleFor(a => a.TargetId)
            .NotEmpty()
            .When(a =>
                a.ActionType == PlayerActionType.Attack || a.ActionType == PlayerActionType.Spy
            )
            .WithMessage("Target ID is required for attack or spy actions.");

        // Parameters dictionary must always be present
        RuleFor(a => a.Parameters)
            .NotNull()
            .WithMessage("Parameters dictionary must be provided.");
    }
}
