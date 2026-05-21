namespace Ums.Application.Approvals.ApprovalRequest.DTOs;

public sealed record ApprovalRequestDto(
    Guid ApprovalRequestId,
    Guid WorkflowId,
    Guid TargetUserId,
    Guid? TargetProfileId,
    string Status);
