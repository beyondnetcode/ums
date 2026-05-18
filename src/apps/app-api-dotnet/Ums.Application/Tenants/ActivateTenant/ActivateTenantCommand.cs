namespace Ums.Application.Tenants.ActivateTenant;

using Ums.Application.Abstractions.Messaging;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
