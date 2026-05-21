using Ums.Application.Approvals.ApprovalWorkflow.DTOs;

namespace Ums.Application.Approvals.ApprovalWorkflow.Queries;

public sealed record GetApprovalWorkflowByIdQuery(Guid ApprovalWorkflowId) : IQuery<ApprovalWorkflowDto>;
