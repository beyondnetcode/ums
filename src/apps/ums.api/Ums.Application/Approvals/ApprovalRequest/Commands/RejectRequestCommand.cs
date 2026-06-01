using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

public sealed record RejectRequestCommand(
    Guid ApprovalRequestId,
    string? DecisionReason = null) : ICommand;
