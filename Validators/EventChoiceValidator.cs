using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates EventChoice objects for required fields and constraints.
/// </summary>
public class EventChoiceValidator : AbstractValidator<EventChoice>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventChoiceValidator"/> class.
    /// </summary>
    public EventChoiceValidator()
    {
        RuleFor(c => c.Description)
            .NotEmpty()
            .WithMessage("Choice description is required.")
            .MaximumLength(256)
            .WithMessage("Description must be 256 characters or fewer.");

        RuleForEach(c => c.Effects.Keys).IsInEnum().WithMessage("Invalid stat key in effects.");

        RuleForEach(c => c.BlockedActions).IsInEnum().WithMessage("Invalid blocked action type.");
    }
}
