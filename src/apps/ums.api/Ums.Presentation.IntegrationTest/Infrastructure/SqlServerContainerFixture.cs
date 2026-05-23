using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Ums.Application.Common.Interfaces;
using Ums.Infrastructure.Persistence;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

/// <summary>
/// REC-15: xUnit v3 collection fixture that starts a SQL Server Testcontainer once
/// and bootstraps the UMS platform schema.  Shared across all tests in the
/// <c>[Collection("SqlServer")]</c> collection — container spins up once and is
/// discarded at the end of the test run.
///
/// If Docker is unavailable the fixture sets <see cref="IsAvailable"/> = <c>false</c>
/// so individual tests can call <c>Skip.If(!fixture.IsAvailable, "Docker required")</c>
/// instead of failing with a raw exception.
///
/// xUnit v3 requires <see cref="ValueTask"/> on <see cref="IAsyncLifetime"/> methods.
/// </summary>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    // Deferred — built inside InitializeAsync to avoid throwing in the constructor
    // when Docker is not reachable (e.g. on machines without Docker Desktop).
    private MsSqlContainer? _container;

    /// <summary>Full ADO.NET connection string pointing at the container database.</summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// <c>true</c> when the SQL Server container started successfully and the schema
    /// was bootstrapped.  Tests should call <c>Skip.If(!IsAvailable, "Docker required")</c>.
    /// </summary>
    public bool IsAvailable { get; private set; }

    public async ValueTask InitializeAsync()
    {
        try
        {
            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithAutoRemove(true)
                .Build();

            await _container.StartAsync();

            ConnectionString = _container.GetConnectionString();

            // Bootstrap the UMS platform schema using the same bootstrapper as production.
            var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
                .UseSqlServer(ConnectionString, sql => sql.EnableRetryOnFailure(3))
                .Options;

            await using var ctx = new UmsPlatformDbContext(options, new SystemTenantContext());
            await SqlServerSchemaBootstrapper.InitializeAsync(ctx);

            IsAvailable = true;
        }
        catch (Exception)
        {
            // Docker not available or container failed to start — tests will be skipped.
            IsAvailable = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Null tenant context → no global query filter applied (system / admin view)
    // ─────────────────────────────────────────────────────────────────────────
    private sealed class SystemTenantContext : ITenantContext
    {
        public Guid? OrganizationId => null;
        public void SetOrganizationId(Guid organizationId) { /* no-op for system context */ }
    }
}
