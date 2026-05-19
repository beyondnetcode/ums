namespace Ums.Application.Identity.Tenant.RemoveBranding;


public sealed record RemoveBrandingCommand(Guid TenantId) : ICommand<RemoveBrandingResponse>;
