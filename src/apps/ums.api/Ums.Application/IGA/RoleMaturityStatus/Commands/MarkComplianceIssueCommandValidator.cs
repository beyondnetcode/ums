namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed class MarkComplianceIssueCommandValidator : AbstractValidator<MarkComplianceIssueCommand>
{
    public MarkComplianceIssueCommandValidator()
    {
        RuleFor(x => x.RoleMaturityStatusId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
