namespace Ums.Application.Identity.Tenant.CreateTenant;

using FluentValidation;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Type)
            .NotEmpty()
            .Must(type => DomainEnumerationParser.FromName<OrganizationType>(type) is not null)
            .WithMessage("Organization type is not supported.");

        RuleFor(command => command.IdpStrategy)
            .Must(strategy => string.IsNullOrWhiteSpace(strategy) || DomainEnumerationParser.FromName<IdpStrategy>(strategy) is not null)
            .WithMessage("Identity provider strategy is not supported.");

        RuleFor(command => command.CompanyReference)
            .MaximumLength(150)
            .When(command => !string.IsNullOrWhiteSpace(command.CompanyReference));

        RuleFor(command => command.ParentTenantId)
            .NotEqual(Guid.Empty)
            .When(command => command.ParentTenantId.HasValue);
    }
}
