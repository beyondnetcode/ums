namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using FluentValidation;

public sealed class CreateApprovalRequestCommandValidator : AbstractValidator<CreateApprovalRequestCommand>
{
    public CreateApprovalRequestCommandValidator()
    {
        RuleFor(c => c.WorkflowId).NotEmpty();
        RuleFor(c => c.TargetUserId).NotEmpty();
    }
}
