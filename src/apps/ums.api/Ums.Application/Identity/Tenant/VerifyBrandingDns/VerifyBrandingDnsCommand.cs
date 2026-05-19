namespace Ums.Application.Identity.Tenant.VerifyBrandingDns;


public sealed record VerifyBrandingDnsCommand(Guid TenantId) : ICommand<VerifyBrandingDnsResponse>;
