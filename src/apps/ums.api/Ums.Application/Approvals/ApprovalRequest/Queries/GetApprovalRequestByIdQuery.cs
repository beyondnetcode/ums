using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Queries;

public sealed record GetApprovalRequestByIdQuery(Guid ApprovalRequestId) : IQuery<ApprovalRequestDto>;
