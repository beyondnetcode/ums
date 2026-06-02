namespace Ums.Application.Test.Tenants.Branding;

using Ums.Application.Identity.Tenant.Branding.Commands;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branding;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Enums;

public class BrandingCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserContext> _ctx = new();
    private readonly Mock<ITenantScopePolicy> _scopePolicy = new();

    public BrandingCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

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

    private static SetBrandingCommand ValidSetCommand(Guid tenantId) => new(
        TenantId: tenantId,
        Logo: "data:image/png;base64,abc123",
        LogoFormat: "Png",
        PrimaryColor: "#FFFFFF",
        BackgroundStyle: "SolidColor",
        HeadlineText: "Welcome",
        SecondaryText: "Sign in to continue",
        PrimaryButtonLabel: "Sign In",
        FooterText: "Powered by UMS",
        CustomDomain: null,
        MagicLinkFallbackEnabled: true);

    // =========================================================================
    #region SetBrandingCommandHandler
    // =========================================================================

    [Fact]
    public async Task SetBranding_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue());
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.NotNull(tenant.Branding);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetBranding_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = ValidSetCommand(Guid.NewGuid());
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task SetBranding_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = ValidSetCommand(Guid.NewGuid());
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task SetBranding_WhenTenantIsNotManagementOwner_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _scopePolicy.Setup(s => s.EnsureManagementOwnerScopeAsync(tenant.Props.Id.GetValue(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("AUTH_015: Tenant is not marked as management owner."));
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue());
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("management owner", result.Error, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetBranding_WithInvalidLogoFormat_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue()) with { LogoFormat = "INVALID" };
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid logo format", result.Error);
    }

    [Fact]
    public async Task SetBranding_WithInvalidBackgroundStyle_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue()) with { BackgroundStyle = "INVALID" };
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid background style", result.Error);
    }

    [Fact]
    public async Task SetBranding_WithCustomDomain_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue()) with { CustomDomain = "login.example.com" };
        var handler = new SetBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
    }

    #endregion

    // =========================================================================
    #region UpdateBrandingCommandHandler
    // =========================================================================

    [Fact]
    public async Task UpdateBranding_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        tenant.SetBranding(
            BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("logo.png"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#000000"), BackgroundStyle.SolidColor)
                .WithTexts(
                    LoginText.Create("Old Headline"),
                    LoginText.Create("Old Secondary"),
                    LoginText.Create("Old Button"),
                    LoginText.Create("Old Footer"))
                .Build(),
            ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = ValidSetCommand(tenant.Props.Id.GetValue()) with
        {
            HeadlineText = "New Headline"
        };
        var updateCmd = new UpdateBrandingCommand(
            TenantId: cmd.TenantId,
            Logo: cmd.Logo,
            LogoFormat: cmd.LogoFormat,
            PrimaryColor: cmd.PrimaryColor,
            BackgroundStyle: cmd.BackgroundStyle,
            HeadlineText: cmd.HeadlineText,
            SecondaryText: cmd.SecondaryText,
            PrimaryButtonLabel: cmd.PrimaryButtonLabel,
            FooterText: cmd.FooterText,
            CustomDomain: cmd.CustomDomain,
            MagicLinkFallbackEnabled: cmd.MagicLinkFallbackEnabled);

        var handler = new UpdateBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(updateCmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBranding_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = new UpdateBrandingCommand(
            TenantId: Guid.NewGuid(),
            Logo: "logo.png",
            LogoFormat: "Png",
            PrimaryColor: "#FFFFFF",
            BackgroundStyle: "SolidColor",
            HeadlineText: "Welcome",
            SecondaryText: "Sign in",
            PrimaryButtonLabel: "Sign In",
            FooterText: "Footer",
            CustomDomain: null,
            MagicLinkFallbackEnabled: true);

        var handler = new UpdateBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task UpdateBranding_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new UpdateBrandingCommand(
            TenantId: Guid.NewGuid(),
            Logo: "logo.png",
            LogoFormat: "Png",
            PrimaryColor: "#FFFFFF",
            BackgroundStyle: "SolidColor",
            HeadlineText: "Welcome",
            SecondaryText: "Sign in",
            PrimaryButtonLabel: "Sign In",
            FooterText: "Footer",
            CustomDomain: null,
            MagicLinkFallbackEnabled: true);

        var handler = new UpdateBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    #endregion

    // =========================================================================
    #region RemoveBrandingCommandHandler
    // =========================================================================

    [Fact]
    public async Task RemoveBranding_WithValidCommand_ReturnsSuccess()
    {
        var tenant = MakeTenant();
        tenant.SetBranding(
            BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("logo.png"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#000000"), BackgroundStyle.SolidColor)
                .WithTexts(
                    LoginText.Create("Headline"),
                    LoginText.Create("Secondary"),
                    LoginText.Create("Button"),
                    LoginText.Create("Footer"))
                .Build(),
            ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new RemoveBrandingCommand(tenant.Props.Id.GetValue());
        var handler = new RemoveBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Null(tenant.Branding);
        _repo.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveBranding_WhenTenantNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tenant?)null);

        var cmd = new RemoveBrandingCommand(Guid.NewGuid());
        var handler = new RemoveBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant was not found", result.Error);
    }

    [Fact]
    public async Task RemoveBranding_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new RemoveBrandingCommand(Guid.NewGuid());
        var handler = new RemoveBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Authenticated user is required", result.Error);
    }

    [Fact]
    public async Task RemoveBranding_WhenNoBrandingExists_ReturnsFailure()
    {
        var tenant = MakeTenant();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(tenant);

        var cmd = new RemoveBrandingCommand(tenant.Props.Id.GetValue());
        var handler = new RemoveBrandingCommandHandler(_repo.Object, _ctx.Object, _scopePolicy.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}
