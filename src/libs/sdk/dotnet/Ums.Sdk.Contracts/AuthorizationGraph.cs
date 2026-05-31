using System.Text.Json.Serialization;

namespace Ums.Sdk.Contracts;

/// <summary>
/// Self-contained, immutable snapshot of a user's authorization universe at authentication time.
/// Schema is defined in <c>src/libs/sdk/contracts/auth-graph.schema.json</c> (v1.0.0).
/// See ADR-0071 for the model and ADR-0074 for the versioning policy.
/// </summary>
public sealed record AuthorizationGraph(
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("context")] PrincipalContext Context,
    [property: JsonPropertyName("authentication")] AuthenticationMetadata Authentication,
    [property: JsonPropertyName("actions")] IReadOnlyList<ActionRef> Actions,
    [property: JsonPropertyName("menuAccess")] IReadOnlyList<MenuModule> MenuAccess,
    [property: JsonPropertyName("domainPermissions")] IReadOnlyList<DomainResourcePermissions> DomainPermissions,
    [property: JsonPropertyName("featureFlags")] IReadOnlyList<FeatureFlagState> FeatureFlags,
    [property: JsonPropertyName("effectiveConfig")] EffectiveConfig EffectiveConfig,
    [property: JsonPropertyName("scopes")] IReadOnlyList<string> Scopes,
    [property: JsonPropertyName("generatedAt")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("validUntil")] DateTimeOffset ValidUntil);

public sealed record PrincipalContext(
    [property: JsonPropertyName("user")] UserSummary User,
    [property: JsonPropertyName("tenant")] TenantSummary Tenant,
    [property: JsonPropertyName("systemSuite")] SystemSuiteSummary SystemSuite,
    [property: JsonPropertyName("role")] RoleSummary Role,
    [property: JsonPropertyName("profile")] ProfileSummary Profile,
    [property: JsonPropertyName("branch")] BranchSummary? Branch);

public sealed record UserSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("status")] string Status);

public sealed record TenantSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status);

public sealed record SystemSuiteSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status);

public sealed record RoleSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("hierarchyLevel")] int HierarchyLevel,
    [property: JsonPropertyName("parentRoleId")] Guid? ParentRoleId);

public sealed record ProfileSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("isActive")] bool IsActive);

public sealed record BranchSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name);

public sealed record AuthenticationMetadata(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("provider")] IdpProviderRef? Provider,
    [property: JsonPropertyName("mfaRequired")] bool MfaRequired,
    [property: JsonPropertyName("issuedAt")] DateTimeOffset IssuedAt,
    [property: JsonPropertyName("sessionExpiresAt")] DateTimeOffset SessionExpiresAt);

public sealed record IdpProviderRef(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("strategy")] string Strategy);

public sealed record ActionRef(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name);

public sealed record MenuModule(
    [property: JsonPropertyName("module")] ModuleSummary Module,
    [property: JsonPropertyName("menus")] IReadOnlyList<Menu> Menus);

public sealed record ModuleSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sortOrder")] int SortOrder,
    [property: JsonPropertyName("status")] string Status);

public sealed record Menu(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("sortOrder")] int SortOrder,
    [property: JsonPropertyName("subMenus")] IReadOnlyList<SubMenu> SubMenus);

public sealed record SubMenu(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("sortOrder")] int SortOrder,
    [property: JsonPropertyName("options")] IReadOnlyList<MenuOption> Options);

public sealed record MenuOption(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("actionCode")] string ActionCode,
    [property: JsonPropertyName("effect")] AccessEffect Effect,
    [property: JsonPropertyName("source")] PermissionSource Source);

public sealed record DomainResourcePermissions(
    [property: JsonPropertyName("resource")] DomainResource Resource,
    [property: JsonPropertyName("actions")] IReadOnlyList<DomainActionResolution> Actions);

public sealed record DomainResource(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("moduleId")] Guid? ModuleId);

public sealed record DomainActionResolution(
    [property: JsonPropertyName("actionId")] Guid ActionId,
    [property: JsonPropertyName("actionCode")] string ActionCode,
    [property: JsonPropertyName("actionName")] string ActionName,
    [property: JsonPropertyName("effect")] AccessEffect Effect,
    [property: JsonPropertyName("source")] PermissionSource Source);

public sealed record FeatureFlagState(
    [property: JsonPropertyName("flagCode")] string FlagCode,
    [property: JsonPropertyName("systemSuiteId")] Guid SystemSuiteId,
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("matchedCriteriaType")] string? MatchedCriteriaType);

public sealed record EffectiveConfig(
    [property: JsonPropertyName("sessionTimeoutMinutes")] int SessionTimeoutMinutes,
    [property: JsonPropertyName("maxLoginAttempts")] int MaxLoginAttempts,
    [property: JsonPropertyName("minPasswordLength")] int MinPasswordLength,
    [property: JsonPropertyName("mfaRequiredForAdmin")] bool MfaRequiredForAdmin,
    [property: JsonPropertyName("accessTokenDurationMs")] long AccessTokenDurationMs,
    [property: JsonPropertyName("authUseExternalIdp")] bool AuthUseExternalIdp);
