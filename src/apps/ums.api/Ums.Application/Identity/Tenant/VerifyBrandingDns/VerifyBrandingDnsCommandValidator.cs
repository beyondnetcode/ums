namespace Ums.Application.Identity.Tenant.VerifyBrandingDns;

using FluentValidation;

public sealed class VerifyBrandingDnsCommandValidator : AbstractValidator<VerifyBrandingDnsCommand>
{
    public VerifyBrandingDnsCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
