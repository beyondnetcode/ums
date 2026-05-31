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
    Guid   Id,
    string Email,
    string Username,
    string DisplayName,
    string Status);

public sealed record GraphTenant(
    Guid   Id,
    string Code,
    string Name,
    string Status);

public sealed record GraphSystemSuite(
    Guid   Id,
    string Code,
    string Name,
    string Status);

public sealed record GraphRole(
    Guid   Id,
    string Code,
    string Name,
    int    HierarchyLevel,
    Guid?  ParentRoleId);

public sealed record GraphProfile(
    Guid   Id,
    string Scope,       // "OrgWide" | "BranchScoped"
    bool   IsActive);

public sealed record GraphBranch(
    Guid   Id,
    string Code,
    string Name);
