using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Queries;

public sealed record GetAccessEnforcementPolicyByIdQuery(Guid AccessEnforcementPolicyId) : IQuery<AccessEnforcementPolicyDto>;
