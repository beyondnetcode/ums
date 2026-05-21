namespace Ums.Application.Approvals.ApprovalWorkflow.Commands;

using FluentValidation;

public sealed class CreateApprovalWorkflowCommandValidator : AbstractValidator<CreateApprovalWorkflowCommand>
{
    public CreateApprovalWorkflowCommandValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
        RuleFor(c => c.TargetUserCategory).NotEmpty();
    }
}
