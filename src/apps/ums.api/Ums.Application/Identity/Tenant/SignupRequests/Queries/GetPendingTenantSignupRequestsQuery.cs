using Ums.Application.Identity.Tenant.SignupRequests.DTOs;

namespace Ums.Application.Identity.Tenant.SignupRequests.Queries;

public sealed record GetPendingTenantSignupRequestsQuery() : IQuery<IReadOnlyList<TenantSignupRequestDto>>;
