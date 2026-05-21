using Ums.Application.IGA.RoleMaturityStatus.DTOs;

namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed record CreateRoleMaturityStatusCommand(
    Guid TenantId, Guid UserId, Guid RoleId, string CurrentMaturityLevel) : ICommand<CreateRoleMaturityStatusResponse>;
