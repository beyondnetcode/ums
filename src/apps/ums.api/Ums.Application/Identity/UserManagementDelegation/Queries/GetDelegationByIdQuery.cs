using Ums.Application.Identity.UserManagementDelegation.DTOs;

namespace Ums.Application.Identity.UserManagementDelegation.Queries;

public sealed record GetDelegationByIdQuery(Guid DelegationId) : IQuery<DelegationDto>;
