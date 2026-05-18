namespace Ums.Application.Tenants.SuspendTenant;

using Ums.Application.Abstractions.Messaging;

public sealed record SuspendTenantCommand(Guid TenantId) : ICommand;
