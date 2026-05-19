using Ums.Application.Identity.Tenant.Branding.DTOs;



namespace Ums.Application.Identity.Tenant.Branding.Commands;


public sealed record RemoveBrandingCommand(Guid TenantId) : ICommand<RemoveBrandingResponse>;
