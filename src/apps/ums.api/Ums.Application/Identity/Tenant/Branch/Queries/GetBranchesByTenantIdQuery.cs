using Ums.Application.Identity.Tenant.Branch.DTOs;

namespace Ums.Application.Identity.Tenant.Branch.Queries;

public sealed record GetBranchesByTenantIdQuery(Guid TenantId) : IQuery<IReadOnlyList<BranchDto>>;
