namespace Ums.Application.Authorization.Profile.DTOs;

public sealed record ProfileDto(
    Guid ProfileId,
    Guid TenantId,
    Guid UserId,
    Guid RoleId,
    Guid? BranchId,
    string Scope,
    bool IsActive,
    IReadOnlyList<ProfilePermissionDto> Permissions);

