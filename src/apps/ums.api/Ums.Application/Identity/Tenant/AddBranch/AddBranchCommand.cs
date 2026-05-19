namespace Ums.Application.Identity.Tenant.AddBranch;


public sealed record AddBranchCommand(
    Guid TenantId,
    string Code,
    string Name,
    string? GeofencingMetadata) : ICommand<AddBranchResponse>;
