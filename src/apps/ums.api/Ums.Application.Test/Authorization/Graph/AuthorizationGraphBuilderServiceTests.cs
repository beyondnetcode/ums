namespace Ums.Application.Test.Authorization.Graph;

using Moq;
using Xunit;
using Ums.Application.Authorization.Graph;
using Ums.Application.Configuration.Services;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Graph;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Auth;
using ProfileAggregate     = Ums.Domain.Authorization.Profile.Profile;
using RoleAggregate        = Ums.Domain.Authorization.Role.Role;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;

/// <summary>
/// Unit tests for AuthorizationGraphBuilderService.
/// All dependencies are mocked — no DB access.
/// </summary>
public class AuthorizationGraphBuilderServiceTests
{
    private readonly Mock<IProfileRepository>            _profileRepo    = new();
    private readonly Mock<IRoleRepository>               _roleRepo       = new();
    private readonly Mock<ISystemSuiteRepository>        _suiteRepo      = new();
    private readonly Mock<IPermissionTemplateRepository> _templateRepo   = new();
    private readonly Mock<ITenantRepository>             _tenantRepo     = new();
    private readonly Mock<IFeatureFlagRepository>        _flagRepo       = new();
    private readonly Mock<IFeatureFlagEvaluator>         _flagEvaluator  = new();
    private readonly Mock<IConfigurationProvider>        _configProvider = new();

    private AuthorizationGraphBuilderService CreateSut() => new(
        _profileRepo.Object, _roleRepo.Object, _suiteRepo.Object,
        _templateRepo.Object, _tenantRepo.Object, _flagRepo.Object,
        _flagEvaluator.Object, _configProvider.Object);

    private static readonly Guid TenantGuid = Guid.NewGuid();
    private static readonly Guid UserGuid   = Guid.NewGuid();

    public AuthorizationGraphBuilderServiceTests()
    {
        _configProvider.Setup(c => c.GetValueAs<int>(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                       .Returns<string, Guid?, int>((_, __, def) => def);
        _configProvider.Setup(c => c.GetValueAs<bool>(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                       .Returns<string, Guid?, bool>((_, __, def) => def);
        _configProvider.Setup(c => c.GetValue(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>()))
                       .Returns((string _, Guid? __, string? defaultValue) => defaultValue ?? string.Empty);

        _flagRepo.Setup(r => r.GetBySystemSuiteIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<FeatureFlagAggregate>());

        _templateRepo.Setup(r => r.GetByTenantIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PermissionTemplate>());
    }

    // ── Error paths ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuildAsync_TenantNotFound_ReturnsFailure()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync(TenantGuid, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Ums.Domain.Identity.Tenant.Tenant?)null);

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsFailure);
        Assert.Contains("Tenant", result.Error);
    }

    [Fact]
    public async Task BuildAsync_NoActiveProfileForUser_ReturnsFailure()
    {
        SetupValidTenant();
        _profileRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<ProfileAggregate>());

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsFailure);
        Assert.Contains("profile", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BuildAsync_RoleNotFound_ReturnsFailure()
    {
        SetupValidTenant();
        _profileRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<ProfileAggregate> { MakeProfile() });
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((RoleAggregate?)null);

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsFailure);
        Assert.Contains("Role", result.Error);
    }

    // ── Success paths ─────────────────────────────────────────────────────────

    [Fact]
    public async Task BuildAsync_UserWithNoPermissions_ReturnsGraphWithEmptyScopes()
    {
        SetupFullChain();

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Scopes);
    }

    [Fact]
    public async Task BuildAsync_ReturnsGraphWithCorrectTenantContext()
    {
        SetupFullChain();

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantGuid, result.Value.Context.Tenant.Id);
        Assert.Equal("Local", result.Value.Authentication.Method);
        Assert.Null(result.Value.Authentication.Provider);
    }

    [Fact]
    public async Task BuildAsync_ValidUntil_IsAfterGeneratedAt()
    {
        SetupFullChain();

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.ValidUntil > result.Value.GeneratedAt);
    }

    [Fact]
    public async Task BuildAsync_FeatureFlags_EvaluatedWithUserContext()
    {
        SetupFullChain();
        var suiteId = Guid.NewGuid();

        var flag = FeatureFlagAggregate.Create(
            IdValueObject.Load(suiteId), null, "TEST_FLAG",
            FlagType.Boolean, "*", null, null, null,
            ActorId.Create("test")).Value;
        flag.Activate(ActorId.Create("test"));
        flag.DomainEvents.MarkChangesAsCommitted();

        _flagRepo.Setup(r => r.GetBySystemSuiteIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<FeatureFlagAggregate> { flag });
        _flagEvaluator.Setup(e => e.Evaluate(It.IsAny<FeatureFlagAggregate>(), It.IsAny<EvaluationContext>()))
                      .Returns(new FlagEvaluationResult(true, null, "no criteria"));

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.FeatureFlags);
        Assert.True(result.Value.FeatureFlags[0].IsEnabled);
        Assert.Equal("TEST_FLAG", result.Value.FeatureFlags[0].FlagCode);
    }

    [Fact]
    public async Task BuildAsync_EffectiveConfig_ReadsFromConfigProvider()
    {
        SetupFullChain();
        _configProvider.Setup(c => c.GetValueAs<int>("SESSION_TIMEOUT_MINUTES", TenantGuid, 30))
                       .Returns(45);

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Equal(45, result.Value.EffectiveConfig.SessionTimeoutMinutes);
    }

    [Fact]
    public async Task BuildAsync_Actions_IncludeAllSuiteActions()
    {
        SetupFullChain();

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Actions);
        Assert.Contains(result.Value.Actions, a => a.Code == "VIEW");
    }

    [Fact]
    public async Task BuildForProfileAsync_WhenProfileIsInactive_ReturnsFailure()
    {
        SetupValidTenant();
        var profile = MakeProfile();
        profile.Deactivate(ActorId.Create("test"));
        _profileRepo.Setup(r => r.GetByIdAsync(profile.GetId().GetValue(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await CreateSut().BuildForProfileAsync(
            MakeUser(),
            TenantGuid,
            profile.GetId().GetValue(),
            AuthMethod.Local());

        Assert.True(result.IsFailure);
        Assert.Contains("inactive", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BuildForProfileAsync_UsesRequestedProfile()
    {
        SetupValidTenant();

        var suiteId = Guid.NewGuid();
        var requestedRoleId = Guid.NewGuid();
        var requestedProfile = ProfileAggregate.Create(
            TenantId.Load(TenantGuid), UserId.Load(UserGuid),
            RoleId.Load(requestedRoleId), null, ActorId.Create("test")).Value;
        requestedProfile.DomainEvents.MarkChangesAsCommitted();

        _profileRepo.Setup(r => r.GetByIdAsync(requestedProfile.GetId().GetValue(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestedProfile);

        var role = RoleAggregate.Create(
            TenantId.Load(TenantGuid),
            SystemSuiteId.Load(suiteId),
            Code.Create("ADMIN"),
            Name.Create("Administrator"),
            Description.Create(""),
            null, 0, 1,
            ActorId.Create("test")).Value;
        role.DomainEvents.MarkChangesAsCommitted();
        _roleRepo.Setup(r => r.GetByIdAsync(requestedRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var suite = BuildMinimalSuite(suiteId);
        _suiteRepo.Setup(r => r.GetByIdAsync(suiteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(suite);

        var result = await CreateSut().BuildForProfileAsync(
            MakeUser(),
            TenantGuid,
            requestedProfile.GetId().GetValue(),
            AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Equal(requestedProfile.GetId().GetValue(), result.Value.Context.Profile.Id);
    }

    [Fact]
    public async Task BuildAsync_OrgWide_NoBranchInContext()
    {
        SetupFullChain();

        var result = await CreateSut().BuildAsync(MakeUser(), TenantGuid, AuthMethod.Local());

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Context.Branch);
        Assert.Equal("OrgWide", result.Value.Context.Profile.Scope);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static UserAccountAggregate MakeUser()
    {
        var actor = ActorId.Create("test");
        var user  = UserAccountAggregate.Create(
            TenantId.Load(TenantGuid),
            Email.Create("user@test.com"),
            Ums.Domain.Enums.UserCategory.Internal,
            null, null,
            actor,
            null,
            UserAccountId.Load(UserGuid)).Value;
        user.DomainEvents.MarkChangesAsCommitted();
        return user;
    }

    private void SetupValidTenant()
    {
        var actor  = ActorId.Create("test");
        var tenant = Ums.Domain.Identity.Tenant.Tenant.Create(
            Code.Create("TEST"),
            Name.Create("Test Tenant"),
            Ums.Domain.Enums.OrganizationType.INTERNAL,
            actor,
            Ums.Domain.Enums.IdpStrategy.AzureAd,
            tenantId: TenantId.Load(TenantGuid)).Value;
        tenant.DomainEvents.MarkChangesAsCommitted();
        _tenantRepo.Setup(r => r.GetByIdAsync(TenantGuid, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tenant);
    }

    private static ProfileAggregate MakeProfile()
    {
        var profile = ProfileAggregate.Create(
            TenantId.Load(TenantGuid),
            UserId.Load(UserGuid),
            RoleId.Load(Guid.NewGuid()),
            null,
            ActorId.Create("test")).Value;
        profile.DomainEvents.MarkChangesAsCommitted();
        return profile;
    }

    private void SetupFullChain()
    {
        SetupValidTenant();

        var suiteId = Guid.NewGuid();
        var roleId  = Guid.NewGuid();

        var profile = ProfileAggregate.Create(
            TenantId.Load(TenantGuid), UserId.Load(UserGuid),
            RoleId.Load(roleId), null, ActorId.Create("test")).Value;
        profile.DomainEvents.MarkChangesAsCommitted();
        _profileRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<ProfileAggregate> { profile });

        var role = RoleAggregate.Create(
            TenantId.Load(TenantGuid),
            SystemSuiteId.Load(suiteId),
            Code.Create("ADMIN"),
            Name.Create("Administrator"),
            Description.Create(""),
            null, 0, 1,
            ActorId.Create("test")).Value;
        role.DomainEvents.MarkChangesAsCommitted();
        _roleRepo.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(role);

        var suite = BuildMinimalSuite(suiteId);
        _suiteRepo.Setup(r => r.GetByIdAsync(suiteId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(suite);
    }

    private static SystemSuiteAggregate BuildMinimalSuite(Guid suiteId)
    {
        var actor = ActorId.Create("test");
        var suite = SystemSuiteAggregate.Create(
            TenantId.Load(Guid.NewGuid()),
            Code.Create("CORE"),
            Name.Create("Core System"),
            Description.Create(""),
            actor).Value;

        // Register a VIEW action using ActionCode VO
        suite.RegisterAction(ActionCode.Create("VIEW"), Name.Create("View Records"), actor);
        suite.DomainEvents.MarkChangesAsCommitted();
        return suite;
    }
}
