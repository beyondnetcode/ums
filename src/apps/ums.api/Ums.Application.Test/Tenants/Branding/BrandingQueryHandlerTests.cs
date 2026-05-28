namespace Ums.Application.Test.Tenants.Branding;

using Ums.Application.Identity.Tenant.Branding.Queries;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branding;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Enums;

public class BrandingQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _repo = new();

    private static Tenant MakeTenant()
    {
        return Tenant.Create(
            Code.Create("TEN-001"),
            Name.Create("Test Tenant"),
            OrganizationType.INTERNAL,
            ActorId.Create("user-001"),
            IdpStrategy.InternalBcrypt,
            null,
            null).Value;
    }

    // =========================================================================
    #region GetBrandingByTenantIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetBranding_WhenTenantHasBranding_ReturnsBranding()
    {
        var tenant = MakeTenant();
        tenant.SetBranding(
            BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("logo.png"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#FFFFFF"), BackgroundStyle.SolidColor)
                .WithTexts(
                    LoginText.Create("Welcome"),
                    LoginText.Create("Sign in"),
                    LoginText.Create("Sign In"),
                    LoginText.Create("Footer"))
                .Build(),
            ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var query = new GetBrandingByTenantIdQuery(tenant.Props.Id.GetValue());
        var handler = new GetBrandingByTenantIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("logo.png", result.Value.Logo);
        Assert.Equal("Png", result.Value.LogoFormat);
        Assert.Equal("#FFFFFF", result.Value.PrimaryColor);
        Assert.Equal("Welcome", result.Value.HeadlineText);
    }

    [Fact]
    public async Task GetBranding_WhenTenantHasNoBranding_ReturnsNull()
    {
        var tenant = MakeTenant();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var query = new GetBrandingByTenantIdQuery(tenant.Props.Id.GetValue());
        var handler = new GetBrandingByTenantIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetBranding_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var query = new GetBrandingByTenantIdQuery(Guid.NewGuid());
        var handler = new GetBrandingByTenantIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant not found", result.Error);
    }

    [Fact]
    public async Task GetBranding_WithCustomDomain_ReturnsCustomDomain()
    {
        var tenant = MakeTenant();
        tenant.SetBranding(
            BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("logo.png"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#FFFFFF"), BackgroundStyle.SolidColor)
                .WithTexts(
                    LoginText.Create("Welcome"),
                    LoginText.Create("Sign in"),
                    LoginText.Create("Sign In"),
                    LoginText.Create("Footer"))
                .WithCustomDomain(CustomDomain.Create("login.example.com"))
                .Build(),
            ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var query = new GetBrandingByTenantIdQuery(tenant.Props.Id.GetValue());
        var handler = new GetBrandingByTenantIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("login.example.com", result.Value.CustomDomain);
    }

    #endregion
}
