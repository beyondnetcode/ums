using Ums.Application.Identity.Tenant.Branding.DTOs;
using Ums.Domain.Identity.Tenant;

namespace Ums.Application.Identity.Tenant.Branding.Queries;

public sealed class GetBrandingByTenantIdQueryHandler : IQueryHandler<GetBrandingByTenantIdQuery, BrandingDto>
{
    private readonly ITenantRepository _tenantRepository;

    public GetBrandingByTenantIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<BrandingDto>> Handle(
        GetBrandingByTenantIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result<BrandingDto>.Failure("Tenant not found.");
        }

        if (tenant.Branding is null)
        {
            return Result<BrandingDto>.Success(null!);
        }

        var branding = tenant.Branding;

        return Result<BrandingDto>.Success(new BrandingDto(
            branding.Logo.GetValue(),
            branding.LogoFormat.ToString(),
            branding.PrimaryColor.GetValue(),
            branding.BackgroundStyle.ToString(),
            branding.HeadlineText.GetValue(),
            branding.SecondaryText.GetValue(),
            branding.PrimaryButtonLabel.GetValue(),
            branding.FooterText.GetValue(),
            branding.CustomDomain?.GetValue(),
            branding.MagicLinkFallbackEnabled,
            branding.DnsVerificationStatus?.ToString()));
    }
}
