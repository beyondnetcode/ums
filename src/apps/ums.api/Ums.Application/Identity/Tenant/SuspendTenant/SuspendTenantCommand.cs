namespace Ums.Application.Identity.Tenant.SuspendTenant;


public sealed record SuspendTenantCommand(Guid TenantId) : ICommand;
