namespace Ums.Application.Identity.UserAccount.DTOs;

public sealed record UserAccountDto(
    Guid UserAccountId,
    Guid TenantId,
    Guid? BranchId,
    string Email,
    string Category,
    string Status,
    string? IdentityReference,
    string? IdentityReferenceType);
