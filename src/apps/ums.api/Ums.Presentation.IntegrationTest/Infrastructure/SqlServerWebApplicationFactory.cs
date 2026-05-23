using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ums.Infrastructure.Persistence.Options;
using Ums.Presentation;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

/// <summary>
/// REC-15: <see cref="WebApplicationFactory{TEntryPoint}"/> that wires the UMS API
/// to the SQL Server Testcontainer started by <see cref="SqlServerContainerFixture"/>.
///
/// All three store groups (Identity, Authorization, Configuration) are switched to
/// SQL Server so the full persistence stack is exercised.  Schema was already
/// bootstrapped by <see cref="SqlServerContainerFixture.InitializeAsync"/>.
/// </summary>
public sealed class SqlServerWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.Sources.Clear();
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Persistence — full SQL Server mode
                ["Persistence:Provider"] = PersistenceProvider.SqlServer.ToString(),
                ["Persistence:AggregateStoreMode"] = AggregateStoreMode.SqlServer.ToString(),
                ["Persistence:UseSqlServerIdentityStores"] = bool.TrueString,
                ["Persistence:UseSqlServerAuthorizationStores"] = bool.TrueString,
                ["Persistence:UseSqlServerConfigurationStores"] = bool.TrueString,
                ["Persistence:SeedDevData"] = bool.FalseString,
                ["Persistence:EnableOutbox"] = bool.FalseString,
                ["Persistence:InitializePlatformStoreOnStartup"] = bool.FalseString,

                // Connection
                ["ConnectionStrings:DefaultConnection"] = connectionString,

                // Misc
                ["Secrets:Source"] = "AppSettings",
                ["AllowedOrigins"] = "https://localhost",
            });
        });
    }
}
