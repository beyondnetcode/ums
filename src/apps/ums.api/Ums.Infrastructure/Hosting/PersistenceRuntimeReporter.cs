using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ums.Infrastructure.Persistence.Options;

namespace Ums.Infrastructure.Hosting;

public sealed class PersistenceRuntimeReporter(
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<PersistenceRuntimeReporter> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = persistenceOptions.Value;

        logger.LogInformation(
            "UMS persistence configured with provider {Provider}, aggregate store mode {AggregateStoreMode}, identity SQL stores {UseSqlServerIdentityStores}, authorization SQL stores {UseSqlServerAuthorizationStores}, outbox enabled {EnableOutbox}.",
            options.Provider,
            options.AggregateStoreMode,
            options.UseSqlServerIdentityStores,
            options.UseSqlServerAuthorizationStores,
            options.EnableOutbox);

        if (options.Provider == PersistenceProvider.SqlServer
            && options.AggregateStoreMode == AggregateStoreMode.InMemory
            && !options.UseSqlServerIdentityStores)
        {
            logger.LogWarning(
                "SQL Server is configured as the platform provider, but aggregate repositories still run in-memory. This is a valid transitional modular-monolith mode, not the final production persistence model.");
        }

        if (options.Provider == PersistenceProvider.SqlServer && options.UseSqlServerIdentityStores)
        {
            logger.LogInformation("Identity aggregates are configured to run on SQL Server repositories while the remaining contexts stay in transitional mode.");
        }

        if (options.Provider == PersistenceProvider.SqlServer && options.UseSqlServerAuthorizationStores)
        {
            logger.LogInformation("Authorization profile aggregates are configured to run on SQL Server repositories.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
