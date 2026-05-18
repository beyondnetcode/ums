namespace Ums.Application.Tenants.AddBranch;

using Ums.Application.Abstractions.Messaging;

public sealed record AddBranchCommand(
    Guid TenantId,
    string Code,
    string Name,
    string? GeofencingMetadata) : ICommand<AddBranchResponse>;
