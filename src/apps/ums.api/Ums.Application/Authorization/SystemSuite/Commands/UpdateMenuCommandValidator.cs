namespace Ums.Application.Authorization.SystemSuite.Commands;

using FluentValidation;

public sealed class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.SystemSuiteId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.SortOrder).GreaterThan(0).WithMessage("SortOrder must be a positive integer.");
    }
}
