namespace Ums.Application.Test.Identity.Auth;

using Moq;
using Xunit;
using Ums.Application.Configuration.Services;
using Ums.Application.Identity.Auth;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Auth;

/// <summary>
/// Tests for AuthMethodResolverService.
/// Verifies dynamic resolution from IConfigurationProvider without hitting the DB.
/// </summary>
public class AuthMethodResolverTests
{
    private readonly Mock<IConfigurationProvider> _config     = new();
    private readonly Mock<ITenantRepository>      _tenantRepo = new();
    private readonly Guid                         _tenantId   = Guid.NewGuid();

    private AuthMethodResolverService CreateSut()
        => new(_config.Object, _tenantRepo.Object);

    private void SetupAuthUseExternalIdp(bool value)
        => _config.Setup(c => c.GetValueAs<bool>("AUTH_USE_EXTERNAL_IDP", _tenantId, false))
                  .Returns(value);

    // ── Local mode ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_WhenAuthUseExternalIdpFalse_ReturnsLocalMethod()
    {
        SetupAuthUseExternalIdp(false);

        var result = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthMethodType.Local, result.Value.Type);
        Assert.Null(result.Value.Provider);
    }

    [Fact]
    public async Task ResolveAsync_LocalMode_DoesNotQueryTenantRepository()
    {
        SetupAuthUseExternalIdp(false);

        await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);

        _tenantRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── IDP mode ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_WhenIdpConfigAndActiveIdp_ReturnsIdpMethod()
    {
        SetupAuthUseExternalIdp(true);
        _tenantRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(BuildTenantWithActiveIdp());

        var result = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthMethodType.IDP, result.Value.Type);
        Assert.NotNull(result.Value.Provider);
    }

    [Fact]
    public async Task ResolveAsync_WhenIdpConfigButNoActiveIdp_ReturnsError_AUTH011()
    {
        SetupAuthUseExternalIdp(true);
        _tenantRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(BuildTenantWithNoActiveIdp());

        var result = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);

        Assert.True(result.IsFailure);
        Assert.Contains("AUTH_011", result.Error);
    }

    [Fact]
    public async Task ResolveAsync_UsesConfigProvider_NotHardcoded()
    {
        // The key invariant: the resolver delegates to config, not hardcoded logic.
        // Changing the config value changes the method — no code change needed.
        SetupAuthUseExternalIdp(false);
        var local = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);
        Assert.Equal(AuthMethodType.Local, local.Value.Type);

        SetupAuthUseExternalIdp(true);
        _tenantRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(BuildTenantWithActiveIdp());
        var idp = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.ExternalApi);
        Assert.Equal(AuthMethodType.IDP, idp.Value.Type);
    }

    [Fact]
    public async Task ResolveAsync_PortalManagementAlwaysReturnsLocal()
    {
        SetupAuthUseExternalIdp(true);

        var result = await CreateSut().ResolveAsync(_tenantId, AuthAccessScope.PortalManagement);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthMethodType.Local, result.Value.Type);
        Assert.Null(result.Value.Provider);
        _tenantRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Ums.Domain.Identity.Tenant.Tenant BuildTenantWithActiveIdp()
    {
        var actor  = ActorId.Create("test");
        var tenant = Ums.Domain.Identity.Tenant.Tenant.Create(
            Code.Create("TEST"),
            Name.Create("Test Tenant"),
            Ums.Domain.Enums.OrganizationType.INTERNAL,
            actor,
            Ums.Domain.Enums.IdpStrategy.AzureAd).Value;

        tenant.RegisterIdentityProvider(
            Code.Create("AZURE"),
            Name.Create("Azure AD"),
            Description.Create(""),
            Ums.Domain.Enums.IdpStrategy.AzureAd,
            actor);

        var idp = tenant.IdentityProviders.First();
        tenant.ActivateIdentityProvider(idp.GetId(), actor);
        tenant.DomainEvents.MarkChangesAsCommitted();
        return tenant;
    }

    private static Ums.Domain.Identity.Tenant.Tenant BuildTenantWithNoActiveIdp()
    {
        var actor = ActorId.Create("test");
        var tenant = Ums.Domain.Identity.Tenant.Tenant.Create(
            Code.Create("TEST"),
            Name.Create("Test Tenant"),
            Ums.Domain.Enums.OrganizationType.INTERNAL,
            actor,
            Ums.Domain.Enums.IdpStrategy.AzureAd).Value;
        tenant.DomainEvents.MarkChangesAsCommitted();
        return tenant;
    }
}
