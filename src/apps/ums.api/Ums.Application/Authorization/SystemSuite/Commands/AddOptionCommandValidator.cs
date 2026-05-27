namespace Ums.Application.Authorization.SystemSuite.Commands;

using FluentValidation;

public sealed class AddOptionCommandValidator : AbstractValidator<AddOptionCommand>
{
    public AddOptionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SystemSuiteId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.SubMenuId).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Option code is required.")
            .MaximumLength(50).WithMessage("Option code must not exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9_]+$").WithMessage("Option code may only contain letters, digits, and underscores.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Option label is required.")
            .MaximumLength(150).WithMessage("Option label must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.ActionCode)
            .NotEmpty().WithMessage("Action code is required.")
            .MaximumLength(50).WithMessage("Action code must not exceed 50 characters.");

        RuleFor(x => x.SortOrder)
            .GreaterThan(0).WithMessage("Sort order must be greater than zero.");
    }
}
