namespace Ums.Application.Identity.Tenant.Branch.DTOs;

public sealed record BranchDto(
    Guid BranchId,
    string Code,
    string Name,
    bool IsActive,
    string? GeofencingMetadata);
