using Ums.Application.Identity.Tenant.Branch.DTOs;



namespace Ums.Application.Identity.Tenant.Branch.Commands;


public sealed record AddBranchCommand(
    Guid TenantId,
    string Code,
    string Name,
    string? GeofencingMetadata) : ICommand<AddBranchResponse>;
