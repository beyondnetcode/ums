namespace Ums.Application.Configuration.Services;

using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

/// <summary>
/// Storage abstraction for in-process configuration cache.
///
/// TD-003: Introducing this interface from day one lets us swap the concrete
/// implementation from <see cref="InMemoryConfigurationCache"/> to a Redis-backed
/// implementation (RedisConfigurationCache) without changing <see cref="IConfigurationProvider"/>
/// or any business logic. The migration becomes a single DI registration change.
///
/// Design contract:
///  - All implementations must be thread-safe.
///  - Reads must be synchronous (no round-trip to an external store at read time).
///  - Writes (population / invalidation) may be asynchronous in future Redis implementations.
///  - The cache is loaded eagerly at startup and invalidated on parameter changes.
/// </summary>
public interface IConfigurationCache
{
    // ── Read ────────────────────────────────────────────────────────────────

    AppConfigurationAggregate? GetGlobal(string code);

    AppConfigurationAggregate? GetForTenant(Guid tenantId, string code);

    /// <summary>
    /// Returns the tenant override if it exists, falling back to the global entry.
    /// Implements the Tenant &gt; Global precedence rule.
    /// </summary>
    AppConfigurationAggregate? GetWithPrecedence(string code, Guid? tenantId);

    IReadOnlyList<AppConfigurationAggregate> GetAllGlobal();

    IReadOnlyList<AppConfigurationAggregate> GetAllForTenant(Guid tenantId);

    bool HasTenantOverride(string code, Guid tenantId);

    // ── Write (population / invalidation) ───────────────────────────────────

    void PopulateGlobal(IEnumerable<AppConfigurationAggregate> configs);

    void PopulateTenant(Guid tenantId, IEnumerable<AppConfigurationAggregate> configs);

    /// <summary>Hot-reload: evicts all entries for a single tenant.</summary>
    void InvalidateTenant(Guid tenantId);

    /// <summary>Full reload: evicts all entries.</summary>
    void InvalidateAll();
}
