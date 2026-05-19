namespace Ums.Application.Identity.Tenant.ActivateTenant;


public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
