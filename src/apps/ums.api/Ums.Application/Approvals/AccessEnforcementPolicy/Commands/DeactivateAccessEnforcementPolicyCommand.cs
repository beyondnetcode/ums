using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;

namespace Ums.Application.Approvals.AccessEnforcementPolicy.Commands;

public sealed record DeactivateAccessEnforcementPolicyCommand(Guid AccessEnforcementPolicyId) : ICommand;
