using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed record GetDelegationsByDelegatedAdminQuery(Guid DelegatedAdminId, Guid TenantId) : IQuery<IReadOnlyList<DelegationDto>>;
