namespace Ums.Application.Identity.Tenant.DeactivateBranch;


public sealed record DeactivateBranchCommand(
    Guid TenantId,
    Guid BranchId) : ICommand<DeactivateBranchResponse>;
