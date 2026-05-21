using Ums.Application.IGA.RoleMaturityStatus.DTOs;

namespace Ums.Application.IGA.RoleMaturityStatus.Queries;

public sealed record GetRoleMaturityStatusByIdQuery(Guid RoleMaturityStatusId) : IQuery<RoleMaturityStatusDto>;
