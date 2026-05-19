namespace Ums.Application.Identity.Tenant.ReactivateBranch;


public sealed record ReactivateBranchCommand(
    Guid TenantId,
    Guid BranchId) : ICommand<ReactivateBranchResponse>;
