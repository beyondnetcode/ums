namespace Ums.Application.Tenants.CreateTenant;

using Ums.Application.Abstractions.Messaging;

public sealed record CreateTenantCommand(
    string Code,
    string Name,
    string Type,
    string? IdpStrategy,
    string? CompanyReference,
    Guid? ParentTenantId) : ICommand<CreateTenantResponse>;
