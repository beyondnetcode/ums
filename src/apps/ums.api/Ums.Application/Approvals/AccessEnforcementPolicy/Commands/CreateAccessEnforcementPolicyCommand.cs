using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;

public sealed record CreateAccessEnforcementPolicyCommand(
    Guid TenantId, Guid? ProfileId, Guid? RoleId, string EnforcementAction) : ICommand<CreateAccessEnforcementPolicyResponse>;
