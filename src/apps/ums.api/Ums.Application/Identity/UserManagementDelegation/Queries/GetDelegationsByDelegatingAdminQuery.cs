using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed record GetDelegationsByDelegatingAdminQuery(Guid DelegatingAdminId, Guid TenantId) : IQuery<IReadOnlyList<DelegationDto>>;
