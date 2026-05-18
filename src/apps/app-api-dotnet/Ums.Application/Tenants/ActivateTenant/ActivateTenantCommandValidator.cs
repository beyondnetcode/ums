namespace Ums.Application.Tenants.ActivateTenant;

using FluentValidation;

public sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
