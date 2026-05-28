namespace Ums.Application.Authorization.Profile.DTOs;

public sealed record ProfilePermissionDto(
    Guid PermissionId,
    Guid ProfileId,
    Guid TemplateId,
    string TargetType,
    Guid TargetId,
    string TargetName,
    Guid ActionId,
    string ActionName,
    bool IsAllowed,
    bool IsDenied,
    bool IsActive,
    bool IsOverride);
