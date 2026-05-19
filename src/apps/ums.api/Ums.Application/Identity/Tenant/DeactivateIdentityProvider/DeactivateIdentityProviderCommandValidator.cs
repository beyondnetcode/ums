namespace Ums.Application.Identity.Tenant.DeactivateIdentityProvider;

using FluentValidation;

public sealed class DeactivateIdentityProviderCommandValidator : AbstractValidator<DeactivateIdentityProviderCommand>
{
    public DeactivateIdentityProviderCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();

        RuleFor(command => command.IdentityProviderId)
            .NotEmpty();
    }
}
