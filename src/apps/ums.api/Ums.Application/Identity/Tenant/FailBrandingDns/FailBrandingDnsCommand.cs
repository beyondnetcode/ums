namespace Ums.Application.Identity.Tenant.FailBrandingDns;


public sealed record FailBrandingDnsCommand(Guid TenantId) : ICommand<FailBrandingDnsResponse>;
