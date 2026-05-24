using Ums.Application.Identity.Tenant.Branding.DTOs;

namespace Ums.Application.Identity.Tenant.Branding.Commands;

using Ums.Domain.Identity.Tenant.Branding;

public sealed class UpdateBrandingCommandHandler : ICommandHandler<UpdateBrandingCommand, UpdateBrandingResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public UpdateBrandingCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<UpdateBrandingResponse>> Handle(
        UpdateBrandingCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<UpdateBrandingResponse>.Failure("Authenticated user is required to update branding.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<UpdateBrandingResponse>.Failure("Tenant was not found.");
        }

        var logoFormat = DomainEnumerationParser.FromName<LogoFormat>(request.LogoFormat);
        if (logoFormat is null)
        {
            return Result<UpdateBrandingResponse>.Failure($"Invalid logo format: {request.LogoFormat}");
        }

        var backgroundStyle = DomainEnumerationParser.FromName<BackgroundStyle>(request.BackgroundStyle);
        if (backgroundStyle is null)
        {
            return Result<UpdateBrandingResponse>.Failure($"Invalid background style: {request.BackgroundStyle}");
        }

        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create(request.Logo), logoFormat)
            .WithTheme(HexColor.Create(request.PrimaryColor), backgroundStyle)
            .WithTexts(
                LoginText.Create(request.HeadlineText),
                LoginText.Create(request.SecondaryText),
                LoginText.Create(request.PrimaryButtonLabel),
                LoginText.Create(request.FooterText))
            .WithCustomDomain(string.IsNullOrWhiteSpace(request.CustomDomain) ? null : CustomDomain.Create(request.CustomDomain))
            .WithMagicLinkFallback(request.MagicLinkFallbackEnabled)
            .Build();

        var result = tenant.UpdateBranding(settings, ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return Result<UpdateBrandingResponse>.Failure(result.Error);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<UpdateBrandingResponse>.Success(new UpdateBrandingResponse(request.TenantId));
    }
}
