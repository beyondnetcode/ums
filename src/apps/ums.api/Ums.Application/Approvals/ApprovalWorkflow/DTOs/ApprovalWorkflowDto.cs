namespace Ums.Application.Approvals.ApprovalWorkflow.DTOs;

public sealed record ApprovalWorkflowDto(
    Guid ApprovalWorkflowId,
    Guid TenantId,
    Guid? SystemSuiteId,
    string Code,
    string Name,
    string Description,
    string TargetUserCategory,
    bool RequiresApproval);
