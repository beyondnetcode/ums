namespace Ums.Application.Authorization.SystemSuite.Commands;

using FluentValidation;

public sealed class AddModuleCommandValidator : AbstractValidator<AddModuleCommand>
{
    public AddModuleCommandValidator()
    {
        RuleFor(x => x.SystemSuiteId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Za-z0-9_]+$")
            .WithMessage("Code must contain only letters, digits, and underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        // Description is optional — the domain allows empty strings.
        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.SortOrder)
            .GreaterThan(0)
            .WithMessage("SortOrder must be a positive integer.");
    }
}
