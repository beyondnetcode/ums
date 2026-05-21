using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

public sealed record ApproveRequestCommand(Guid ApprovalRequestId) : ICommand;
