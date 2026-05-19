namespace Ums.Application.Identity.Tenant.FailBrandingDns;

using FluentValidation;

public sealed class FailBrandingDnsCommandValidator : AbstractValidator<FailBrandingDnsCommand>
{
    public FailBrandingDnsCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
