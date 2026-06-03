namespace Ums.Application.Authorization.AssignmentRule.Queries;

using Ums.Application.Authorization.AssignmentRule.DTOs;

public sealed record GetAssignmentRulesByTenantQuery(Guid TenantId) : IQuery<IReadOnlyList<AssignmentRuleDto>>;
