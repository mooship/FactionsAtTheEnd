using System.Text.RegularExpressions;
using FactionsAtTheEnd.Models;
using FluentValidation;

namespace FactionsAtTheEnd.Validators;

/// <summary>
/// Validates Faction objects for required fields, valid types, and stat bounds.
/// </summary>
public partial class FactionValidator : AbstractValidator<Faction>
{
    // Compiled static regex for valid faction names: letters, numbers, spaces, and select punctuation.
    private static readonly Regex NameRegex = MyRegex();

    public FactionValidator()
    {
        RuleFor(f => f.Name)
            .NotEmpty()
            .WithMessage("Faction name is required.")
            .MaximumLength(32)
            .WithMessage("Faction name must be 32 characters or fewer.")
            .Must(NameRegex.IsMatch)
            .WithMessage("Faction name contains invalid characters.");

        RuleFor(f => f.Type).IsInEnum().WithMessage("Invalid faction type.");

        RuleFor(f => f.Description)
            .MaximumLength(256)
            .WithMessage("Description must be 256 characters or fewer.");

        // Ensure all numeric stats are non-negative
        RuleFor(f => f.Population)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Population cannot be negative.");

        RuleFor(f => f.Military)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Military cannot be negative.");

        RuleFor(f => f.Technology)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Technology cannot be negative.");

        RuleFor(f => f.Influence)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Influence cannot be negative.");

        RuleFor(f => f.Resources)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Resources cannot be negative.");

        RuleFor(f => f.Stability)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stability cannot be negative.");

        RuleFor(f => f.Reputation)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reputation cannot be negative.")
            .LessThanOrEqualTo(100)
            .WithMessage("Reputation cannot exceed 100.");
    }

    [GeneratedRegex("^[a-zA-Z0-9 .'-]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
