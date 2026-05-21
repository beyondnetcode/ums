namespace Ums.Application.Authorization.Template.DTOs;

public sealed record PermissionTemplateDto(
    Guid TemplateId,
    Guid TenantId,
    Guid RoleId,
    Guid SystemSuiteId,
    string Version,
    string Status);
