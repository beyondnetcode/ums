using Ums.Application.Identity.Tenant.DTOs;


namespace Ums.Application.Identity.Tenant.Commands;


public sealed record SuspendTenantCommand(Guid TenantId) : ICommand;
