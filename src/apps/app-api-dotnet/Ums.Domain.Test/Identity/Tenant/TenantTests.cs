namespace Ums.Domain.Test.Identity.Tenant;

using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branding;
using Xunit;

public class TenantTests
{
    private static readonly Code ValidCode = Code.Create("TEST-001");
    private static readonly Name ValidName = Name.Create("Test Tenant");
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    private static readonly OrganizationType ValidType = OrganizationType.INTERNAL;
    private static readonly Description ValidDescription = Description.Create("Test IdP");
    private static BrandingSettings ValidBrandingSettings => BrandingSettings.CreateBuilder()
        .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
        .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
        .WithTexts(
            LoginText.Create("Welcome"),
            LoginText.Create("Sign in to continue"),
            LoginText.Create("Sign In"),
            LoginText.Create("Powered by UMS"))
        .WithCustomDomain(null)
        .WithMagicLinkFallback(false)
        .Build();

    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCode, result.Value.Code);
        Assert.Equal(ValidName, result.Value.Name);
        Assert.Equal(ValidType, result.Value.Type);
        Assert.Equal(TenantStatus.Active, result.Value.Status);
        Assert.Empty(result.Value.Branches);
    }

    [Fact]
    public void Create_WithEmptyCode_ReturnsFailure()
    {
        var code = Code.Create("");

        var result = Tenant.Create(code, ValidName, ValidType, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var name = Name.Create("");

        var result = Tenant.Create(ValidCode, name, ValidType, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_RaisesTenantCreatedEvent()
    {
        var result = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor);

        Assert.True(result.IsSuccess);
        var events = result.Value.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Single(events);
        Assert.IsType<TenantCreatedEvent>(events[0]);
    }

    #endregion

    #region AddBranch

    [Fact]
    public void AddBranch_WithValidData_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");

        var result = tenant.AddBranch(branchCode, branchName, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(tenant.Branches);
    }

    [Fact]
    public void AddBranch_WithDuplicateCode_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");

        tenant.AddBranch(branchCode, branchName, ValidActor);
        var result = tenant.AddBranch(branchCode, Name.Create("Branch Two"), ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.BranchCodeNotUnique, result.Error);
    }

    [Fact]
    public void AddBranch_RaisesBranchCreatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");

        tenant.AddBranch(branchCode, branchName, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BranchCreatedEvent);
    }

    #endregion

    #region Branch BrokenRules

    [Fact]
    public void AddBranch_WithEmptyCode_ReturnsFailureWithBrokenRules()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("");
        var branchName = Name.Create("Branch One");

        var result = tenant.AddBranch(branchCode, branchName, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.Required, result.Error);
    }

    [Fact]
    public void AddBranch_WithEmptyName_ReturnsFailureWithBrokenRules()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("");

        var result = tenant.AddBranch(branchCode, branchName, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.ValueObject.PropertyRequired, result.Error);
    }

    #endregion

    #region RemoveBranch

    [Fact]
    public void RemoveBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.RemoveBranch(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.NotFound, result.Error);
    }

    [Fact]
    public void RemoveBranch_WhenBranchIsActive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;

        var result = tenant.RemoveBranch(branchId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.Invalid, result.Error);
    }

    [Fact]
    public void RemoveBranch_WhenBranchIsInactive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;
        tenant.DeactivateBranch(branchId, ValidActor);

        var result = tenant.RemoveBranch(branchId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Empty(tenant.Branches);
    }

    [Fact]
    public void RemoveBranch_RaisesBranchRemovedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;
        tenant.DeactivateBranch(branchId, ValidActor);

        tenant.RemoveBranch(branchId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BranchRemovedEvent);
    }

    #endregion

    #region DeactivateBranch

    [Fact]
    public void DeactivateBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.DeactivateBranch(fakeId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DeactivateBranch_WhenBranchIsActive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;

        var result = tenant.DeactivateBranch(branchId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(tenant.Branches.First().IsActive);
    }

    [Fact]
    public void DeactivateBranch_WhenBranchAlreadyInactive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;
        tenant.DeactivateBranch(branchId, ValidActor);

        var result = tenant.DeactivateBranch(branchId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DeactivateBranch_RaisesBranchDeactivatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;

        tenant.DeactivateBranch(branchId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BranchDeactivatedEvent);
    }

    #endregion

    #region ReactivateBranch

    [Fact]
    public void ReactivateBranch_WhenBranchIsInactive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;
        tenant.DeactivateBranch(branchId, ValidActor);

        var result = tenant.ReactivateBranch(branchId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.True(tenant.Branches.First().IsActive);
    }

    [Fact]
    public void ReactivateBranch_WhenBranchAlreadyActive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;

        var result = tenant.ReactivateBranch(branchId, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ReactivateBranch_RaisesBranchReactivatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var branchCode = Code.Create("BR-001");
        var branchName = Name.Create("Branch One");
        tenant.AddBranch(branchCode, branchName, ValidActor);
        var branchId = tenant.Branches.First().Id;
        tenant.DeactivateBranch(branchId, ValidActor);

        tenant.ReactivateBranch(branchId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BranchReactivatedEvent);
    }

    #endregion

    #region Suspend

    [Fact]
    public void Suspend_WhenActive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        var result = tenant.Suspend(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Suspended, tenant.Status);
    }

    [Fact]
    public void Suspend_RaisesTenantSuspendedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        tenant.Suspend(ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is TenantSuspendedEvent);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenSuspended_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.Suspend(ValidActor);

        var result = tenant.Activate(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Active, tenant.Status);
    }

    [Fact]
    public void Activate_RaisesTenantActivatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.Suspend(ValidActor);

        tenant.Activate(ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is TenantActivatedEvent);
    }

    #endregion

    #region RegisterIdentityProvider

    [Fact]
    public void RegisterIdentityProvider_WithValidData_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var code = Code.Create("IDP-001");
        var name = Name.Create("Azure AD");
        var description = Description.Create("Corporate Azure AD");

        var result = tenant.RegisterIdentityProvider(code, name, description, IdpStrategy.AzureAd, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(tenant.IdentityProviders);
        Assert.False(tenant.IdentityProviders.First().IsActive);
    }

    [Fact]
    public void RegisterIdentityProvider_WithDuplicateCode_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var code = Code.Create("IDP-001");
        var name = Name.Create("Azure AD");
        var description = Description.Create("Corporate Azure AD");

        tenant.RegisterIdentityProvider(code, name, description, IdpStrategy.AzureAd, ValidActor);
        var result = tenant.RegisterIdentityProvider(code, Name.Create("Azure AD Backup"), description, IdpStrategy.AzureAd, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpCodeNotUnique, result.Error);
    }

    [Fact]
    public void RegisterIdentityProvider_WithEmptyCode_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var code = Code.Create("");
        var name = Name.Create("Azure AD");
        var description = Description.Create("Corporate Azure AD");

        var result = tenant.RegisterIdentityProvider(code, name, description, IdpStrategy.AzureAd, ValidActor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void RegisterIdentityProvider_RaisesIdentityProviderRegisteredEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var code = Code.Create("IDP-001");
        var name = Name.Create("Azure AD");
        var description = Description.Create("Corporate Azure AD");

        tenant.RegisterIdentityProvider(code, name, description, IdpStrategy.AzureAd, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is IdentityProviderRegisteredEvent);
    }

    [Fact]
    public void RegisterMultipleIdentityProviders_AllStartInactive()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");

        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        tenant.RegisterIdentityProvider(Code.Create("IDP-002"), Name.Create("Okta"), description, IdpStrategy.Okta, ValidActor);

        Assert.Equal(2, tenant.IdentityProviders.Count);
        Assert.All(tenant.IdentityProviders, ip => Assert.False(ip.IsActive));
        Assert.Null(tenant.GetActiveIdentityProvider());
    }

    #endregion

    #region ActivateIdentityProvider

    [Fact]
    public void ActivateIdentityProvider_WithValidId_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;

        var result = tenant.ActivateIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.True(tenant.IdentityProviders.First().IsActive);
    }

    [Fact]
    public void ActivateIdentityProvider_WhenNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.ActivateIdentityProvider(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpNotFound, result.Error);
    }

    [Fact]
    public void ActivateIdentityProvider_WhenAlreadyActive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;
        tenant.ActivateIdentityProvider(idpId, ValidActor);

        var result = tenant.ActivateIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpAlreadyActive, result.Error);
    }

    [Fact]
    public void ActivateIdentityProvider_DeactivatesAllOthers()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        tenant.RegisterIdentityProvider(Code.Create("IDP-002"), Name.Create("Okta"), description, IdpStrategy.Okta, ValidActor);
        tenant.RegisterIdentityProvider(Code.Create("IDP-003"), Name.Create("Keycloak"), description, IdpStrategy.Keycloak, ValidActor);

        var secondIdpId = tenant.IdentityProviders.Skip(1).First().Id;
        tenant.ActivateIdentityProvider(secondIdpId, ValidActor);

        var activeCount = tenant.IdentityProviders.Count(ip => ip.IsActive);
        Assert.Equal(1, activeCount);
        Assert.True(tenant.IdentityProviders.Skip(1).First().IsActive);
    }

    [Fact]
    public void ActivateIdentityProvider_RaisesIdentityProviderActivatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;

        tenant.ActivateIdentityProvider(idpId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is IdentityProviderActivatedEvent);
    }

    [Fact]
    public void GetActiveIdentityProvider_ReturnsActiveOne()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        tenant.RegisterIdentityProvider(Code.Create("IDP-002"), Name.Create("Okta"), description, IdpStrategy.Okta, ValidActor);

        var secondIdpId = tenant.IdentityProviders.Skip(1).First().Id;
        tenant.ActivateIdentityProvider(secondIdpId, ValidActor);

        var active = tenant.GetActiveIdentityProvider();
        Assert.NotNull(active);
        Assert.Equal(Code.Create("IDP-002"), active.Code);
    }

    #endregion

    #region DeactivateIdentityProvider

    [Fact]
    public void DeactivateIdentityProvider_WhenActive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;
        tenant.ActivateIdentityProvider(idpId, ValidActor);

        var result = tenant.DeactivateIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(tenant.IdentityProviders.First().IsActive);
    }

    [Fact]
    public void DeactivateIdentityProvider_WhenNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.DeactivateIdentityProvider(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpNotFound, result.Error);
    }

    [Fact]
    public void DeactivateIdentityProvider_WhenAlreadyInactive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;

        var result = tenant.DeactivateIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpAlreadyInactive, result.Error);
    }

    [Fact]
    public void DeactivateIdentityProvider_RaisesIdentityProviderDeactivatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;
        tenant.ActivateIdentityProvider(idpId, ValidActor);

        tenant.DeactivateIdentityProvider(idpId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is IdentityProviderDeactivatedEvent);
    }

    #endregion

    #region RemoveIdentityProvider

    [Fact]
    public void RemoveIdentityProvider_WhenInactive_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;

        var result = tenant.RemoveIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Empty(tenant.IdentityProviders);
    }

    [Fact]
    public void RemoveIdentityProvider_WhenNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.RemoveIdentityProvider(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.IdpNotFound, result.Error);
    }

    [Fact]
    public void RemoveIdentityProvider_WhenActive_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;
        tenant.ActivateIdentityProvider(idpId, ValidActor);

        var result = tenant.RemoveIdentityProvider(idpId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.Invalid, result.Error);
    }

    [Fact]
    public void RemoveIdentityProvider_RaisesIdentityProviderRemovedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var description = Description.Create("Test");
        tenant.RegisterIdentityProvider(Code.Create("IDP-001"), Name.Create("Azure AD"), description, IdpStrategy.AzureAd, ValidActor);
        var idpId = tenant.IdentityProviders.First().Id;

        tenant.RemoveIdentityProvider(idpId, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is IdentityProviderRemovedEvent);
    }

    #endregion

    #region SetBranding

    [Fact]
    public void SetBranding_WithValidData_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        var result = tenant.SetBranding(ValidBrandingSettings, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.NotNull(tenant.Branding);
        Assert.Equal(LogoFormat.Png, tenant.Branding.LogoFormat);
        Assert.Equal(BackgroundStyle.SolidColor, tenant.Branding.BackgroundStyle);
        Assert.False(tenant.Branding.MagicLinkFallbackEnabled);
    }

    [Fact]
    public void SetBranding_WhenAlreadyExists_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var result = tenant.SetBranding(ValidBrandingSettings, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.BrandingAlreadyExists, result.Error);
    }

    [Fact]
    public void SetBranding_WithInvalidHexColor_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("INVALID"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(null)
            .WithMagicLinkFallback(false)
            .Build();

        var result = tenant.SetBranding(settings, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Branding.InvalidHexColor, result.Error);
    }

    [Fact]
    public void SetBranding_WithCustomDomain_SetsDnsStatusToPending()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("login.example.com"))
            .WithMagicLinkFallback(false)
            .Build();

        tenant.SetBranding(settings, ValidActor);

        Assert.Equal(DnsVerificationStatus.Pending, tenant.Branding!.DnsVerificationStatus);
        Assert.Equal("edge.platform.io", tenant.Branding.DnsCnameTarget.GetValue());
    }

    [Fact]
    public void SetBranding_RaisesBrandingCreatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BrandingCreatedEvent);
    }

    #endregion

    #region UpdateBranding

    [Fact]
    public void UpdateBranding_WhenBrandingExists_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var updatedSettings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/new-logo.svg"), LogoFormat.Svg)
            .WithTheme(HexColor.Create("#00FF00"), BackgroundStyle.Gradient)
            .WithTexts(
                LoginText.Create("New Welcome"),
                LoginText.Create("New secondary"),
                LoginText.Create("New button"),
                LoginText.Create("New footer"))
            .WithCustomDomain(null)
            .WithMagicLinkFallback(true)
            .Build();

        var result = tenant.UpdateBranding(updatedSettings, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(LogoFormat.Svg, tenant.Branding!.LogoFormat);
        Assert.True(tenant.Branding.MagicLinkFallbackEnabled);
    }

    [Fact]
    public void UpdateBranding_WhenBrandingNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        var result = tenant.UpdateBranding(ValidBrandingSettings, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.BrandingNotFound, result.Error);
    }

    [Fact]
    public void UpdateBranding_WhenCustomDomainChanged_ResetsDnsStatusToPending()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var initialSettings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("old.example.com"))
            .WithMagicLinkFallback(false)
            .Build();
        tenant.SetBranding(initialSettings, ValidActor);
        tenant.VerifyBrandingDns(ValidActor);

        var updatedSettings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("new.example.com"))
            .WithMagicLinkFallback(false)
            .Build();

        tenant.UpdateBranding(updatedSettings, ValidActor);

        Assert.Equal(DnsVerificationStatus.Pending, tenant.Branding!.DnsVerificationStatus);
    }

    [Fact]
    public void UpdateBranding_RaisesBrandingUpdatedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        tenant.UpdateBranding(ValidBrandingSettings, ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BrandingUpdatedEvent);
    }

    #endregion

    #region RemoveBranding

    [Fact]
    public void RemoveBranding_WhenBrandingExists_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var result = tenant.RemoveBranding(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(tenant.Branding);
    }

    [Fact]
    public void RemoveBranding_WhenBrandingNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        var result = tenant.RemoveBranding(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.BrandingNotFound, result.Error);
    }

    [Fact]
    public void RemoveBranding_RaisesBrandingRemovedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        tenant.RemoveBranding(ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BrandingRemovedEvent);
    }

    #endregion

    #region VerifyBrandingDns

    [Fact]
    public void VerifyBrandingDns_WhenCustomDomainExists_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("login.example.com"))
            .WithMagicLinkFallback(false)
            .Build();
        tenant.SetBranding(settings, ValidActor);

        var result = tenant.VerifyBrandingDns(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(DnsVerificationStatus.Verified, tenant.Branding!.DnsVerificationStatus);
    }

    [Fact]
    public void VerifyBrandingDns_WhenNoCustomDomain_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var result = tenant.VerifyBrandingDns(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Branding.DnsVerificationRequired, result.Error);
    }

    [Fact]
    public void VerifyBrandingDns_WhenBrandingNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;

        var result = tenant.VerifyBrandingDns(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.BrandingNotFound, result.Error);
    }

    [Fact]
    public void VerifyBrandingDns_RaisesBrandingDnsVerifiedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("login.example.com"))
            .WithMagicLinkFallback(false)
            .Build();
        tenant.SetBranding(settings, ValidActor);

        tenant.VerifyBrandingDns(ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BrandingDnsVerifiedEvent);
    }

    #endregion

    #region FailBrandingDns

    [Fact]
    public void FailBrandingDns_WhenCustomDomainExists_ReturnsSuccess()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("login.example.com"))
            .WithMagicLinkFallback(false)
            .Build();
        tenant.SetBranding(settings, ValidActor);

        var result = tenant.FailBrandingDns(ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(DnsVerificationStatus.Failed, tenant.Branding!.DnsVerificationStatus);
    }

    [Fact]
    public void FailBrandingDns_WhenNoCustomDomain_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.SetBranding(ValidBrandingSettings, ValidActor);

        var result = tenant.FailBrandingDns(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Branding.DnsVerificationRequired, result.Error);
    }

    [Fact]
    public void FailBrandingDns_RaisesBrandingDnsFailedEvent()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var settings = BrandingSettings.CreateBuilder()
            .WithLogo(Logo.Create("https://cdn.example.com/logo.png"), LogoFormat.Png)
            .WithTheme(HexColor.Create("#FF5733"), BackgroundStyle.SolidColor)
            .WithTexts(
                LoginText.Create("Welcome"),
                LoginText.Create("Sign in"),
                LoginText.Create("Sign In"),
                LoginText.Create("Footer"))
            .WithCustomDomain(CustomDomain.Create("login.example.com"))
            .WithMagicLinkFallback(false)
            .Build();
        tenant.SetBranding(settings, ValidActor);

        tenant.FailBrandingDns(ValidActor);

        var events = tenant.DomainEvents.GetUncommittedChanges().ToList();
        Assert.Contains(events, e => e is BrandingDnsFailedEvent);
    }

    #endregion

    #region Tenant Status Edge Cases

    [Fact]
    public void Suspend_WhenArchived_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.Suspend(ValidActor);

        var propsType = typeof(TenantProps);
        var statusProp = propsType.GetProperty("Status");
        statusProp!.SetValue(tenant.Props, TenantStatus.Archived);

        var result = tenant.Suspend(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.ArchivedCannotSuspend, result.Error);
    }

    [Fact]
    public void Activate_WhenArchived_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        tenant.Suspend(ValidActor);

        var propsType = typeof(TenantProps);
        var statusProp = propsType.GetProperty("Status");
        statusProp!.SetValue(tenant.Props, TenantStatus.Archived);

        var result = tenant.Activate(ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Tenant.ArchivedCannotActivate, result.Error);
    }

    [Fact]
    public void ReactivateBranch_WhenBranchNotFound_ReturnsFailure()
    {
        var tenant = Tenant.Create(ValidCode, ValidName, ValidType, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = tenant.ReactivateBranch(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.NotFound, result.Error);
    }

    #endregion
}
