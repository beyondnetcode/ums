namespace Ums.Application.Authorization.Template.Commands;

public sealed class AddTemplateItemCommandValidator : AbstractValidator<AddTemplateItemCommand>
{
    private static readonly string[] ValidTargetTypes = ["SystemSuite", "Module", "Submodule", "Option"];

    public AddTemplateItemCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .Must(v => ValidTargetTypes.Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage("TargetType must be one of: SystemSuite, Module, Submodule, Option.");
        RuleFor(x => x.TargetId).NotEmpty();
        RuleFor(x => x.ActionId).NotEmpty();
        RuleFor(x => x).Must(x => x.IsAllowed || x.IsDenied || (!x.IsAllowed && !x.IsDenied))
            .WithMessage("IsAllowed and IsDenied cannot both be true simultaneously.")
            .When(x => x.IsAllowed && x.IsDenied);
    }
}
