namespace Ums.Application.Authorization.SystemSuite.DTOs;

public sealed record SystemSuiteDto(
    Guid SystemSuiteId,
    Guid TenantId,
    string Code,
    string Name,
    string Description,
    string Status);
