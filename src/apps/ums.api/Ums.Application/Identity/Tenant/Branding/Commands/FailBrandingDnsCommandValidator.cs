namespace Ums.Application.Identity.Tenant.Branding.Commands;

using FluentValidation;

public sealed class FailBrandingDnsCommandValidator : AbstractValidator<FailBrandingDnsCommand>
{
    public FailBrandingDnsCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
