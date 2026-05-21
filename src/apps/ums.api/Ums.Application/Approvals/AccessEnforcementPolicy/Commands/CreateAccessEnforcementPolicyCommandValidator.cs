namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;

using FluentValidation;

public sealed class CreateAccessEnforcementPolicyCommandValidator : AbstractValidator<CreateAccessEnforcementPolicyCommand>
{
    public CreateAccessEnforcementPolicyCommandValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.EnforcementAction).NotEmpty();
        RuleFor(c => c).Must(c => c.ProfileId.HasValue || c.RoleId.HasValue)
            .WithMessage("Either ProfileId or RoleId must be provided.");
    }
}
