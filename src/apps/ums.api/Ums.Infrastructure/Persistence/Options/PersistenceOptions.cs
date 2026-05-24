namespace Ums.Infrastructure.Persistence.Options;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public PersistenceProvider Provider { get; init; } = PersistenceProvider.SqlServer;

    public AggregateStoreMode AggregateStoreMode { get; init; } = AggregateStoreMode.InMemory;

    public bool UseSqlServerIdentityStores { get; init; } = false;

    public bool UseSqlServerAuthorizationStores { get; init; } = false;

    public bool UseSqlServerConfigurationStores { get; init; } = false;

    public bool UseSqlServerApprovalsStores { get; init; } = false;

    public bool UseSqlServerIgaStores { get; init; } = false;

    public bool SeedDevData { get; init; } = true;

    public bool EnableOutbox { get; init; } = true;

    public bool InitializePlatformStoreOnStartup { get; init; } = false;
}
