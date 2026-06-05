using System.Text.Json.Serialization;

namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// Identity and location context of the authenticated principal — who they are
/// and in which tenant / system / role / branch they are operating.
/// </summary>
public sealed record GraphContext(
    GraphUser        User,
    GraphTenant      Tenant,
    GraphSystemSuite SystemSuite,
    GraphRole        Role,
    GraphProfile     Profile,
    GraphBranch?     Branch);       // null when Scope == OrgWide

public sealed record GraphUser(
    [property: JsonIgnore] Guid Id,
    string Email,
    string Username,
    [property: JsonPropertyName("value")] string DisplayName,
    string Status);

public sealed record GraphTenant(
    [property: JsonIgnore] Guid Id,
    string Code,
    [property: JsonPropertyName("value")] string Name,
    string Status,
    bool   IsManagementOwner);

public sealed record GraphSystemSuite(
    [property: JsonIgnore] Guid Id,
    string Code,
    [property: JsonPropertyName("value")] string Name,
    string Status);

public sealed record GraphRole(
    [property: JsonIgnore] Guid Id,
    string Code,
    [property: JsonPropertyName("value")] string Name,
    int    HierarchyLevel,
    [property: JsonIgnore]
    Guid?  ParentRoleId);

public sealed record GraphProfile(
    [property: JsonIgnore] Guid Id,
    string Scope,       // "OrgWide" | "BranchScoped"
    bool   IsActive);

public sealed record GraphBranch(
    [property: JsonIgnore] Guid Id,
    string Code,
    [property: JsonPropertyName("value")] string Name);
