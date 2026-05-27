namespace Ums.Application.Authorization.SystemSuite.Commands;

using FluentValidation;
using Ums.Globalization.Access;

public sealed class AddModuleCommandValidator : AbstractValidator<AddModuleCommand>
{
    public AddModuleCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SystemSuiteId)
            .NotEmpty()
            .WithMessage(_ => StringLocalizer.T("system_suite.module.suite_required"));

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(_ => StringLocalizer.T("system_suite.module.code_required"))
            .MaximumLength(50).WithMessage(_ => StringLocalizer.T("system_suite.module.code_too_long"))
            .Matches(@"^[A-Za-z0-9_]+$")
            .WithMessage(command => string.Format(
                StringLocalizer.T("system_suite.module.code_invalid_format"),
                command.Code));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_ => StringLocalizer.T("system_suite.module.name_required"))
            .MaximumLength(150).WithMessage(_ => StringLocalizer.T("system_suite.module.name_too_long"));

        // Description is optional; the domain allows empty strings.
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(_ => StringLocalizer.T("system_suite.module.description_too_long"));

        RuleFor(x => x.SortOrder)
            .GreaterThan(0)
            .WithMessage(_ => StringLocalizer.T("system_suite.module.sort_order_invalid"));
    }
}
