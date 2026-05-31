using Ums.Application.Configuration.Services;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Graph;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Auth;
using SystemSuiteAggregate  = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using ProfileAggregate       = Ums.Domain.Authorization.Profile.Profile;
using RoleAggregate          = Ums.Domain.Authorization.Role.Role;
using BranchEntity           = Ums.Domain.Identity.Tenant.Branch.Branch;
using UserAccountAggregate   = Ums.Domain.Identity.UserAccount.UserAccount;
using FeatureFlagAggregate   = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;

namespace Ums.Application.Authorization.Graph;

/// <summary>
/// Builds the complete AuthorizationGraph for an authenticated user.
///
/// Cross-aggregate enrichment — identical pattern to GetProfileByIdQueryHandler.
/// Resolves: Profile → Role → SystemSuite → PermissionTemplate → Branch (optional)
/// → FeatureFlags → EffectiveConfig.
///
/// Permission resolution (deny-wins, override-takes-precedence):
///   1. Find ProfilePermission for (TargetType, TargetId, ActionId) where IsActive
///   2. IsOverride = true  → use PP.IsAllowed / PP.IsDenied   (source = Override)
///   3. IsOverride = false → use TemplateItem values           (source = Template)
///   4. IsDenied == true → Effect = Deny (always wins over Allow)
///   5. IsAllowed == true → Effect = Allow
///   6. No matching entry → Effect = NotGranted (implicit deny)
/// </summary>
public sealed class AuthorizationGraphBuilderService : IAuthorizationGraphBuilder
{
    private readonly IProfileRepository            _profileRepo;
    private readonly IRoleRepository               _roleRepo;
    private readonly ISystemSuiteRepository        _suiteRepo;
    private readonly IPermissionTemplateRepository _templateRepo;
    private readonly ITenantRepository             _tenantRepo;
    private readonly IFeatureFlagRepository        _featureFlagRepo;
    private readonly IFeatureFlagEvaluator         _flagEvaluator;
    private readonly IConfigurationProvider        _configProvider;

    public AuthorizationGraphBuilderService(
        IProfileRepository            profileRepo,
        IRoleRepository               roleRepo,
        ISystemSuiteRepository        suiteRepo,
        IPermissionTemplateRepository templateRepo,
        ITenantRepository             tenantRepo,
        IFeatureFlagRepository        featureFlagRepo,
        IFeatureFlagEvaluator         flagEvaluator,
        IConfigurationProvider        configProvider)
    {
        _profileRepo     = profileRepo;
        _roleRepo        = roleRepo;
        _suiteRepo       = suiteRepo;
        _templateRepo    = templateRepo;
        _tenantRepo      = tenantRepo;
        _featureFlagRepo = featureFlagRepo;
        _flagEvaluator   = flagEvaluator;
        _configProvider  = configProvider;
    }

    public async Task<Result<AuthorizationGraph>> BuildAsync(
        UserAccountAggregate userAccount,
        Guid                 tenantId,
        AuthMethod           authMethod,
        CancellationToken    cancellationToken = default)
    {
        var userId = userAccount.Props.Id.GetValue();

        // ── 1. Tenant ─────────────────────────────────────────────────────────
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            return Result<AuthorizationGraph>.Failure($"Tenant {tenantId} not found.");

        // ── 2. Active profile for this user + tenant ───────────────────────────
        var profiles = await _profileRepo.GetByUserIdAsync(userId, cancellationToken);
        var profile  = profiles.FirstOrDefault(p =>
            p.Props.TenantId.GetValue() == tenantId && p.IsActive);

        if (profile is null)
            return Result<AuthorizationGraph>.Failure(
                "No active profile found for user in this tenant.");

        // ── 3. Role ───────────────────────────────────────────────────────────
        var role = await _roleRepo.GetByIdAsync(profile.Props.RoleId.GetValue(), cancellationToken);
        if (role is null)
            return Result<AuthorizationGraph>.Failure("Role not found for profile.");

        // ── 4. SystemSuite (with full hierarchy) ──────────────────────────────
        var suite = await _suiteRepo.GetByIdAsync(role.Props.SystemSuiteId.GetValue(), cancellationToken);
        if (suite is null)
            return Result<AuthorizationGraph>.Failure("SystemSuite not found for role.");

        // ── 5. Published PermissionTemplate for this role+tenant ──────────────
        var templates = await _templateRepo.GetByTenantIdAsync(tenantId, cancellationToken);
        var template  = templates
            .Where(t => t.Props.RoleId.GetValue() == role.Props.Id.GetValue()
                     && t.Status == TemplateStatus.Published)
            .OrderByDescending(t => t.Props.Audit.GetValue().CreatedAt)
            .FirstOrDefault();

        // ── 6. Branch (if BranchScoped) ────────────────────────────────────────
        BranchEntity? branch = profile.Props.BranchId is not null
            ? tenant.Branches.FirstOrDefault(b =>
                b.Props.Id.GetValue() == profile.Props.BranchId.GetValue())
            : null;

        // ── 7. Build action lookup: ActionId → (Code, Name) ───────────────────
        var actionLookup = suite.Actions.ToDictionary(
            a => a.GetId().GetValue(),
            a => (Code: a.Props.Code.GetValue(), Name: a.Props.Name.GetValue()));

        // ── 8. Resolve effective permission map ────────────────────────────────
        var permMap = BuildPermissionMap(profile, actionLookup);

        // ── 9. Assemble graph sections ─────────────────────────────────────────
        var actions           = BuildActions(suite);
        var menuAccess        = BuildMenuAccess(suite, permMap);
        var domainPermissions = BuildDomainPermissions(suite, permMap, actionLookup);
        var featureFlags      = await EvaluateFeatureFlagsAsync(suite, profile, role, branch, cancellationToken);
        var effectiveConfig   = BuildEffectiveConfig(tenantId);
        var scopes            = DeriveScopes(menuAccess, domainPermissions);

        // ── 10. Context node ──────────────────────────────────────────────────
        var displayName = userAccount.Props.IdentityReference?.GetValue()
                          ?? userAccount.Props.Email.GetValue();

        var context = new GraphContext(
            User: new GraphUser(
                userId,
                userAccount.Props.Email.GetValue(),
                userAccount.Props.IdentityReference?.GetValue() ?? userAccount.Props.Email.GetValue(),
                displayName,
                userAccount.Props.Status.ToString()),
            Tenant: new GraphTenant(
                tenant.Props.Id.GetValue(),
                tenant.Props.Code.GetValue(),
                tenant.Props.Name.GetValue(),
                tenant.Props.Status.ToString()),
            SystemSuite: new GraphSystemSuite(
                suite.Props.Id.GetValue(),
                suite.Props.Code.GetValue(),
                suite.Props.Name.GetValue(),
                suite.Props.Status.ToString()),
            Role: new GraphRole(
                role.Props.Id.GetValue(),
                role.Props.Code.GetValue(),
                role.Props.Value.GetValue(),
                role.Props.HierarchyLevel,
                role.Props.ParentRoleId?.GetValue()),
            Profile: new GraphProfile(profile.Props.Id.GetValue(), profile.Scope.Name, profile.IsActive),
            Branch: branch is null ? null
                : new GraphBranch(
                    branch.Props.Id.GetValue(),
                    branch.Props.Code.GetValue(),
                    branch.Props.Name.GetValue()));

        // ── 11. Authentication node ────────────────────────────────────────────
        var mfaRequired  = _configProvider.GetValueAs<bool>("MFA_REQUIRED_FOR_ADMIN", tenantId, false);
        GraphIdpProvider? idpProvider = authMethod.Provider is not null
            ? new GraphIdpProvider(
                authMethod.Provider.GetId().GetValue(),
                authMethod.Provider.Props.Name.GetValue(),
                authMethod.Provider.Props.Code.GetValue(),
                authMethod.Provider.Props.Strategy.Name)
            : null;

        var now             = DateTime.UtcNow;
        var sessionMinutes  = effectiveConfig.SessionTimeoutMinutes;
        var authentication  = new GraphAuthentication(
            Method:           authMethod.Type.ToString(),
            Provider:         idpProvider,
            MfaRequired:      mfaRequired,
            IssuedAt:         now,
            SessionExpiresAt: now.AddMinutes(sessionMinutes));

        var graph = AuthorizationGraph.Build(
            context, authentication, actions, menuAccess,
            domainPermissions, featureFlags, effectiveConfig, scopes, now);

        return Result<AuthorizationGraph>.Success(graph);
    }

    // ── Permission Resolution ────────────────────────────────────────────────

    /// <summary>
    /// Builds (TargetId, ActionId) → (Effect, Source) map from profile permissions.
    /// Deny-wins: if any permission for a key is Deny, it stays Deny.
    /// </summary>
    private static Dictionary<(Guid TargetId, Guid ActionId), (AccessEffect Effect, PermissionSource Source)>
        BuildPermissionMap(
            ProfileAggregate profile,
            Dictionary<Guid, (string Code, string Name)> actionLookup)
    {
        var map = new Dictionary<(Guid TargetId, Guid ActionId), (AccessEffect Effect, PermissionSource Source)>();

        foreach (var pp in profile.Permissions.Where(p => p.Props.IsActive))
        {
            var key = (pp.Props.TargetId.GetValue(), pp.Props.ActionId.GetValue());

            var effect = ResolveEffect(pp.Props.IsAllowed, pp.Props.IsDenied);
            if (effect == AccessEffect.NotGranted) continue;

            var source = pp.Props.IsOverride ? PermissionSource.Override : PermissionSource.Template;

            if (map.TryGetValue(key, out var existing))
            {
                // Deny always wins
                if (existing.Effect == AccessEffect.Deny) continue;
                if (effect == AccessEffect.Deny) { map[key] = (effect, source); continue; }
                // Override wins over Template for Allow
                if (source == PermissionSource.Override) map[key] = (effect, source);
            }
            else
            {
                map[key] = (effect, source);
            }
        }

        return map;
    }

    private static AccessEffect ResolveEffect(bool isAllowed, bool isDenied)
    {
        if (isDenied)  return AccessEffect.Deny;
        if (isAllowed) return AccessEffect.Allow;
        return AccessEffect.NotGranted;
    }

    // ── Section Builders ─────────────────────────────────────────────────────

    private static IReadOnlyList<GraphAction> BuildActions(SystemSuiteAggregate suite)
        => suite.Actions
            .Select(a => new GraphAction(
                a.GetId().GetValue(),
                a.Props.Code.GetValue(),
                a.Props.Name.GetValue()))
            .OrderBy(a => a.Code)
            .ToList();

    private static IReadOnlyList<GraphMenuModule> BuildMenuAccess(
        SystemSuiteAggregate suite,
        Dictionary<(Guid, Guid), (AccessEffect Effect, PermissionSource Source)> permMap)
    {
        var modules = new List<GraphMenuModule>();

        foreach (var module in suite.Modules.OrderBy(m => m.Props.SortOrder))
        {
            var menus = new List<GraphMenu>();

            foreach (var menu in module.Menus.OrderBy(m => m.Props.SortOrder))
            {
                var subMenus = new List<GraphSubMenu>();

                foreach (var sub in menu.SubMenus.OrderBy(s => s.Props.SortOrder))
                {
                    var options = new List<GraphMenuOption>();

                    foreach (var opt in sub.Options.OrderBy(o => o.Props.SortOrder))
                    {
                        // Match action by ActionCode string (not by ActionId FK — Options reference by code)
                        var action = suite.Actions.FirstOrDefault(a =>
                            a.Props.Code.GetValue() == opt.Props.ActionCode.GetValue());

                        var effect = AccessEffect.NotGranted;
                        var source = PermissionSource.Template;

                        if (action is not null)
                        {
                            var key = (opt.Props.Id.GetValue(), action.GetId().GetValue());
                            if (permMap.TryGetValue(key, out var perm))
                                (effect, source) = perm;
                        }

                        options.Add(new GraphMenuOption(
                            opt.Props.Id.GetValue(),
                            opt.Props.Code.GetValue(),
                            opt.Props.Label.GetValue(),
                            opt.Props.ActionCode.GetValue(),
                            effect,
                            source));
                    }

                    subMenus.Add(new GraphSubMenu(
                        sub.Props.Id.GetValue(),
                        sub.Props.Code.GetValue(),
                        sub.Props.Label.GetValue(),
                        sub.Props.SortOrder,
                        options));
                }

                menus.Add(new GraphMenu(
                    menu.Props.Id.GetValue(),
                    menu.Props.Code.GetValue(),
                    menu.Props.Label.GetValue(),
                    menu.Props.SortOrder,
                    subMenus));
            }

            modules.Add(new GraphMenuModule(
                module.Props.Id.GetValue(),
                module.Props.Code.GetValue(),
                module.Props.Name.GetValue(),
                module.Props.SortOrder,
                module.Props.Status?.Name ?? "Active",
                menus));
        }

        return modules;
    }

    private static IReadOnlyList<GraphDomainPermission> BuildDomainPermissions(
        SystemSuiteAggregate suite,
        Dictionary<(Guid, Guid), (AccessEffect Effect, PermissionSource Source)> permMap,
        Dictionary<Guid, (string Code, string Name)> actionLookup)
    {
        var result = new List<GraphDomainPermission>();

        foreach (var resource in suite.DomainResources.OrderBy(r => r.Props.Code.GetValue()))
        {
            var domainActions = new List<GraphDomainAction>();

            foreach (var (actionId, (actionCode, actionName)) in actionLookup.OrderBy(kv => kv.Value.Code))
            {
                var key = (resource.Props.Id.GetValue(), actionId);
                permMap.TryGetValue(key, out var perm);

                domainActions.Add(new GraphDomainAction(
                    actionId,
                    actionCode,
                    actionName,
                    perm.Effect,
                    perm.Source));
            }

            result.Add(new GraphDomainPermission(
                resource.Props.Id.GetValue(),
                resource.Props.Type.Name,
                resource.Props.Code.GetValue(),
                resource.Props.Name.GetValue(),
                resource.Props.ModuleId?.GetValue(),
                domainActions));
        }

        return result;
    }

    private async Task<IReadOnlyList<GraphFeatureFlag>> EvaluateFeatureFlagsAsync(
        SystemSuiteAggregate suite,
        ProfileAggregate     profile,
        RoleAggregate        role,
        BranchEntity?        branch,
        CancellationToken    cancellationToken)
    {
        var flags = await _featureFlagRepo.GetBySystemSuiteIdAsync(
            suite.Props.Id.GetValue(), cancellationToken);

        var ctx = new EvaluationContext(
            TenantId:  profile.Props.TenantId.GetValue(),
            BranchId:  branch?.Props.Id.GetValue(),
            ProfileId: profile.Props.Id.GetValue(),
            RoleCode:  role.Props.Code.GetValue(),
            Environment: "Production");

        return flags
            .Where(f => f.Props.Status == FlagStatus.Active)
            .Select(f =>
            {
                var r = _flagEvaluator.Evaluate(f, ctx);
                return new GraphFeatureFlag(
                    f.Props.FlagCode,
                    f.Props.SystemSuiteId.GetValue(),
                    r.IsEnabled,
                    r.MatchedCriteriaType);
            })
            .ToList();
    }

    private GraphEffectiveConfig BuildEffectiveConfig(Guid tenantId)
        => new(
            SessionTimeoutMinutes: _configProvider.GetValueAs<int>("SESSION_TIMEOUT_MINUTES",  tenantId, 30),
            MaxLoginAttempts:      _configProvider.GetValueAs<int>("MAX_LOGIN_ATTEMPTS",        tenantId, 5),
            MinPasswordLength:     _configProvider.GetValueAs<int>("MIN_PASSWORD_LENGTH",       tenantId, 12),
            MfaRequiredForAdmin:   _configProvider.GetValueAs<bool>("MFA_REQUIRED_FOR_ADMIN",   tenantId, false),
            AccessTokenDurationMs: _configProvider.GetValueAs<int>("ACCESS_TOKEN_DURATION_MS", tenantId, 3600000),
            AuthUseExternalIdp:    _configProvider.GetValueAs<bool>("AUTH_USE_EXTERNAL_IDP",   tenantId, false));

    private static IReadOnlyList<string> DeriveScopes(
        IReadOnlyList<GraphMenuModule>       menuAccess,
        IReadOnlyList<GraphDomainPermission> domainPerms)
    {
        var scopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in menuAccess)
        foreach (var menu   in module.Menus)
        foreach (var sub    in menu.SubMenus)
        foreach (var opt    in sub.Options.Where(o => o.Effect == AccessEffect.Allow))
            scopes.Add($"{opt.Code.ToLowerInvariant()}.{opt.ActionCode.ToLowerInvariant()}");

        foreach (var res in domainPerms)
        foreach (var act in res.Actions.Where(a => a.Effect == AccessEffect.Allow))
            scopes.Add($"{res.ResourceCode.ToLowerInvariant()}.{act.ActionCode.ToLowerInvariant()}");

        return scopes.OrderBy(s => s).ToList();
    }
}
