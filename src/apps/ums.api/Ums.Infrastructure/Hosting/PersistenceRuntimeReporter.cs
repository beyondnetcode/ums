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
            "UMS persistence configured with provider {Provider}, aggregate store mode {AggregateStoreMode}, identity PostgreSQL stores {UsePostgreSqlIdentityStores}, authorization PostgreSQL stores {UsePostgreSqlAuthorizationStores}, outbox enabled {EnableOutbox}.",
            options.Provider,
            options.AggregateStoreMode,
            options.UsePostgreSqlIdentityStores,
            options.UsePostgreSqlAuthorizationStores,
            options.EnableOutbox);

        if (options.Provider == PersistenceProvider.PostgreSql
            && options.AggregateStoreMode == AggregateStoreMode.InMemory
            && !options.UsePostgreSqlIdentityStores)
        {
            logger.LogWarning(
                "PostgreSQL is configured as the platform provider, but aggregate repositories still run in-memory. This is a valid transitional modular-monolith mode, not the final production persistence model.");
        }

        if (options.Provider == PersistenceProvider.PostgreSql && options.UsePostgreSqlIdentityStores)
        {
            logger.LogInformation("Identity aggregates are configured to run on PostgreSQL repositories while the remaining contexts stay in transitional mode.");
        }

        if (options.Provider == PersistenceProvider.PostgreSql && options.UsePostgreSqlAuthorizationStores)
        {
            logger.LogInformation("Authorization profile aggregates are configured to run on PostgreSQL repositories.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
