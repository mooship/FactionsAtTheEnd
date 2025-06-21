using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates GameEvent objects for required fields and constraints.
/// </summary>
public class GameEventValidator : AbstractValidator<GameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventValidator"/> class.
    /// </summary>
    public GameEventValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty()
            .WithMessage("Event title is required.")
            .MaximumLength(128)
            .WithMessage("Title must be 128 characters or fewer.");

        RuleFor(e => e.Description)
            .NotEmpty()
            .WithMessage("Event description is required.")
            .MaximumLength(1024)
            .WithMessage("Description must be 1024 characters or fewer.");

        RuleFor(e => e.Type).IsInEnum().WithMessage("Invalid event type.");

        RuleFor(e => e.Cycle).GreaterThanOrEqualTo(1).WithMessage("Cycle must be >= 1.");

        RuleForEach(e => e.Effects.Keys).IsInEnum().WithMessage("Invalid stat key in effects.");

        RuleForEach(e => e.BlockedActions).IsInEnum().WithMessage("Invalid blocked action type.");

        RuleForEach(e => e.Tags)
            .MaximumLength(32)
            .WithMessage("Tag must be 32 characters or fewer.");

        RuleForEach(e => e.Choices)
            .SetValidator(new EventChoiceValidator())
            .When(e => e.Choices != null);
    }
}
