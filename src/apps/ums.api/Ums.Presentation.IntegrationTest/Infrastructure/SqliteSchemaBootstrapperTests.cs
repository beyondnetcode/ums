using System.Reflection;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ums.Infrastructure.Persistence;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

public sealed class SqliteSchemaBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_WhenTenantsTableMissesManagementOwnerColumn_AddsItWithoutFailing()
    {
        await using var connection = new SqliteConnection("Data Source=file:ums-bootstrap-test?mode=memory&cache=shared");
        await connection.OpenAsync();

        await using (var setup = connection.CreateCommand())
        {
            setup.CommandText = """
                CREATE TABLE IF NOT EXISTS "Tenants" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Tenants" PRIMARY KEY,
                    "Code" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "StatusId" INTEGER NOT NULL,
                    "CreatedBy" TEXT NOT NULL,
                    "CreatedAtUtc" TEXT NOT NULL,
                    "AuditTimeSpan" TEXT NOT NULL,
                    "IsDeleted" INTEGER NOT NULL DEFAULT 0
                );
                """;
            await setup.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new UmsPlatformDbContext(options, new SystemTenantContext());

        var bootstrapper = typeof(SqliteSchemaBootstrapper)
            .GetMethod("EnsureTenantManagementOwnerColumnAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Bootstrapper helper method was not found.");

        var task = (Task)bootstrapper.Invoke(null, new object[] { context, CancellationToken.None })!;
        await task;

        var columnExists = await ColumnExistsAsync(connection, "Tenants", "IsManagementOwner");
        columnExists.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_OnFreshDatabase_CreatesTenantManagementOwnerColumn()
    {
        await using var connection = new SqliteConnection("Data Source=file:ums-bootstrap-fresh?mode=memory&cache=shared");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new UmsPlatformDbContext(options, new SystemTenantContext());

        await SqliteSchemaBootstrapper.InitializeAsync(context);

        var columnExists = await ColumnExistsAsync(connection, "Tenants", "IsManagementOwner");
        columnExists.Should().BeTrue();
    }

    private static async Task<bool> ColumnExistsAsync(
        SqliteConnection connection,
        string tableName,
        string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT 1
            FROM pragma_table_info('{tableName}')
            WHERE name = '{columnName}'
            LIMIT 1;
            """;

        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }

    private sealed class SystemTenantContext : Ums.Application.Common.Interfaces.ITenantContext
    {
        public Guid? OrganizationId => null;
        public Guid? OriginalTenantId => null;
        public bool IsInternalAdmin => true;
        public void Initialize(Guid userTenantId, bool isInternalAdmin) { }
        public void SetOrganizationId(Guid organizationId) { }
        public void EnableCrossTenantAccess() { }
        public void DisableCrossTenantAccess() { }
    }
}
