using Ums.Application.Identity.Tenant.Branch.DTOs;



namespace Ums.Application.Identity.Tenant.Branch.Commands;


public sealed record RemoveBranchCommand(
    Guid TenantId,
    Guid BranchId) : ICommand<RemoveBranchResponse>;
