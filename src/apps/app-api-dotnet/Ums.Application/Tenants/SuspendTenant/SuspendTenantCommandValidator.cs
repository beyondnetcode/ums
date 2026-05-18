namespace Ums.Application.Tenants.SuspendTenant;

using FluentValidation;

public sealed class SuspendTenantCommandValidator : AbstractValidator<SuspendTenantCommand>
{
    public SuspendTenantCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();
    }
}
