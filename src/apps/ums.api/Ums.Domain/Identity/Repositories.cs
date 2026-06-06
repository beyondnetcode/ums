namespace Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Identity.UserManagementDelegation;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;
using UserManagementDelegationAggregate = Ums.Domain.Identity.UserManagementDelegation.UserManagementDelegation;

public interface ITenantRepository : IAggregateRepository<TenantAggregate>
{
    Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// REC-12: Server-side paginated query. SQL implementations use Skip/Take at the DB level.
    /// InMemory implementations call GetAllAsync then apply in-memory pagination.
    /// Returns (items in the current page, total matching items).
    /// When tenantId is provided, only returns the matching tenant and its direct children (ParentTenantId == tenantId).
    /// Null tenantId means cross-tenant access (internal admins only).
    /// </summary>
    Task<(IReadOnlyList<TenantAggregate> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? status, string sortBy, string sortOrder,
        Guid? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// REC-16: Soft-delete a tenant by ID. Marks IsDeleted=true and records who deleted it.
    /// Returns false if the tenant was not found.
    /// </summary>
    Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository : IAggregateRepository<UserAccountAggregate>
{
    Task<UserAccountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAccountAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<UserAccountAggregate?> GetByTenantAndEmailAsync(Guid tenantId, Email email, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccountAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccountAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    // ── Dependency guard queries ────────────────────────────────────────────
    /// <summary>Returns the number of non-deleted users in the given tenant.</summary>
    Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// REC-12: Server-side paginated query. SQL implementations use Skip/Take at the DB level.
    /// </summary>
    Task<(IReadOnlyList<UserAccountAggregate> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? status, string sortBy, string sortOrder,
        Guid? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// REC-16: Soft-delete a user account by ID. For SQL repos: marks IsDeleted=true and
    /// anonymizes PII (email → gdpr_del_{sha256}@anonymized.invalid, IdentityReference → null).
    /// For InMemory repos: removes the record from the store.
    /// Returns false if not found.
    /// </summary>
    Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
}

public interface IUserManagementDelegationRepository : IAggregateRepository<UserManagementDelegationAggregate>
{
    Task<UserManagementDelegationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetByDelegatedAdminAsync(Guid delegatedAdminId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetByDelegatingAdminAsync(Guid delegatingAdminId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetExpiredActiveAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
}
