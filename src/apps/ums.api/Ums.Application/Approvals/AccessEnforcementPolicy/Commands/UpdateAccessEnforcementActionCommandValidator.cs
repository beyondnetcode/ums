namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;
public sealed class UpdateAccessEnforcementActionCommandValidator : AbstractValidator<UpdateAccessEnforcementActionCommand>
{
    private static readonly string[] ValidActions = ["AuditOnly", "SoftBlock", "HardBlock"];
    public UpdateAccessEnforcementActionCommandValidator()
    {
        RuleFor(x => x.PolicyId).NotEmpty();
        RuleFor(x => x.NewAction).NotEmpty().Must(v => ValidActions.Contains(v, StringComparer.OrdinalIgnoreCase)).WithMessage("NewAction must be one of: AuditOnly, SoftBlock, HardBlock.");
    }
}
