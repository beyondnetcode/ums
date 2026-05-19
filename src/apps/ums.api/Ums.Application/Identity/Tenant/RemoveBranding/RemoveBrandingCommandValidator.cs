namespace Ums.Application.Identity.Tenant.RemoveBranding;

using FluentValidation;

public sealed class RemoveBrandingCommandValidator : AbstractValidator<RemoveBrandingCommand>
{
    public RemoveBrandingCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
