namespace Ums.Application.Identity.Tenant.Branding.Commands;

using FluentValidation;

public sealed class VerifyBrandingDnsCommandValidator : AbstractValidator<VerifyBrandingDnsCommand>
{
    public VerifyBrandingDnsCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
