namespace Ums.Application.Identity.Tenant.Branding.Commands;

using FluentValidation;

public sealed class RemoveBrandingCommandValidator : AbstractValidator<RemoveBrandingCommand>
{
    public RemoveBrandingCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
