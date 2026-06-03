namespace Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization.Role;
using Ums.Domain.Authorization.AssignmentRule;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;
using RoleAggregate = Ums.Domain.Authorization.Role.Role;
using AssignmentRuleAggregate = Ums.Domain.Authorization.AssignmentRule.TemplateAssignmentRule;

public interface IProfileRepository : IAggregateRepository<ProfileAggregate>
{
    Task<ProfileAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // ── Dependency guard queries (lightweight count-only) ───────────────────
    /// <summary>Returns the number of active profiles assigned to a given role.</summary>
    Task<int> CountActiveByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    /// <summary>Returns the number of active profiles linked to a given template.</summary>
    Task<int> CountActiveByTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    /// <summary>Returns the number of active profiles owned by a given user.</summary>
    Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ISystemSuiteRepository : IAggregateRepository<SystemSuiteAggregate>
{
    Task<SystemSuiteAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SystemSuiteAggregate?> GetByCodeAsync(Code code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSuiteAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSuiteAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IPermissionTemplateRepository : IAggregateRepository<PermissionTemplateAggregate>
{
    Task<PermissionTemplateAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionTemplateAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionTemplateAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // ── Dependency guard queries ────────────────────────────────────────────
    /// <summary>Returns the number of published templates for a given role.</summary>
    Task<int> CountPublishedByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    /// <summary>Returns the number of active template items that reference the given target (domain resource id).</summary>
    Task<int> CountItemsByTargetAsync(Guid targetId, CancellationToken cancellationToken = default);
}

public interface IRoleRepository : IAggregateRepository<RoleAggregate>
{
    Task<RoleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleAggregate?> GetByCodeAsync(Guid systemSuiteId, Code code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAggregate>> GetBySystemSuiteIdAsync(Guid systemSuiteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    // ── Dependency guard queries ────────────────────────────────────────────
    /// <summary>Returns the number of active child roles for a given parent role.</summary>
    Task<int> CountActiveChildRolesAsync(Guid parentRoleId, CancellationToken cancellationToken = default);
}

public interface ITemplateAssignmentRuleRepository : IAggregateRepository<AssignmentRuleAggregate>
{
    Task<AssignmentRuleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssignmentRuleAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns active rules for the given tenant and role, ordered by Priority descending (highest first).</summary>
    Task<IReadOnlyList<AssignmentRuleAggregate>> GetActiveByTenantAndRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Returns whether any active rule for the given tenant already uses the given priority value.</summary>
    Task<bool> ExistsActiveWithPriorityAsync(Guid tenantId, int priority, CancellationToken cancellationToken = default);
}
