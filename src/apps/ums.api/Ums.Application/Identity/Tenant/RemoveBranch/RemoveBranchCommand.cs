namespace Ums.Application.Identity.Tenant.RemoveBranch;


public sealed record RemoveBranchCommand(
    Guid TenantId,
    Guid BranchId) : ICommand<RemoveBranchResponse>;
