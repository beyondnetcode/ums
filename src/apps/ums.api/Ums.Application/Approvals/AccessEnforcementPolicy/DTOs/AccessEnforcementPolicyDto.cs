namespace Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

public sealed record AccessEnforcementPolicyDto(
    Guid AccessEnforcementPolicyId,
    Guid TenantId,
    Guid? ProfileId,
    Guid? RoleId,
    string EnforcementAction,
    bool IsActive);
