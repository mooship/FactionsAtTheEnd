using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates GlobalAchievement objects for required fields and constraints.
/// </summary>
public class GlobalAchievementValidator : AbstractValidator<GlobalAchievement>
{
    public GlobalAchievementValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty()
            .WithMessage("Achievement name is required.")
            .MaximumLength(64)
            .WithMessage("Achievement name must be 64 characters or fewer.");

        RuleFor(a => a.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(256)
            .WithMessage("Description must be 256 characters or fewer.");

        RuleFor(a => a.UnlockedAt).NotEmpty().WithMessage("Unlock date is required.");
    }
}
