using Ums.Application.IGA.RoleMaturityStatus.DTOs;

namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record UpdateRoleMaturityLevelCommand(Guid RoleMaturityStatusId, string NewLevel) : ICommand;
