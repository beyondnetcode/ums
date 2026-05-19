namespace Ums.Application.Identity.Tenant.ActivateIdentityProvider;

using FluentValidation;

public sealed class ActivateIdentityProviderCommandValidator : AbstractValidator<ActivateIdentityProviderCommand>
{
    public ActivateIdentityProviderCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();

        RuleFor(command => command.IdentityProviderId)
            .NotEmpty();
    }
}
