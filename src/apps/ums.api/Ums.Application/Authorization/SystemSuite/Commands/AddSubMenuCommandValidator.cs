namespace Ums.Application.Authorization.SystemSuite.Commands;

using FluentValidation;

public sealed class AddSubMenuCommandValidator : AbstractValidator<AddSubMenuCommand>
{
    public AddSubMenuCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SystemSuiteId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("SubMenu code is required.")
            .MaximumLength(50).WithMessage("SubMenu code must not exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9_]+$").WithMessage("SubMenu code may only contain letters, digits, and underscores.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("SubMenu label is required.")
            .MaximumLength(150).WithMessage("SubMenu label must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.SortOrder)
            .GreaterThan(0).WithMessage("Sort order must be greater than zero.");
    }
}
