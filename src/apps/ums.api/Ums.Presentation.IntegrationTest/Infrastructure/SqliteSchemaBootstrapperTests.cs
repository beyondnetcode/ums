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
        var ct = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=file:ums-bootstrap-test?mode=memory&cache=shared");
        await connection.OpenAsync(ct);

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
            await setup.ExecuteNonQueryAsync(ct);
        }

        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new UmsPlatformDbContext(options, new SystemTenantContext());

        var bootstrapper = typeof(SqliteSchemaBootstrapper)
            .GetMethod("EnsureTenantManagementOwnerColumnAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Bootstrapper helper method was not found.");

        var task = (Task)bootstrapper.Invoke(null, new object[] { context, TestContext.Current.CancellationToken })!;
        await task;

        var columnExists = await ColumnExistsAsync(connection, "Tenants", "IsManagementOwner");
        columnExists.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_OnFreshDatabase_CreatesTenantManagementOwnerColumn()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=file:ums-bootstrap-fresh?mode=memory&cache=shared");
        await connection.OpenAsync(ct);

        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new UmsPlatformDbContext(options, new SystemTenantContext());

        await SqliteSchemaBootstrapper.InitializeAsync(context, ct);

        var columnExists = await ColumnExistsAsync(connection, "Tenants", "IsManagementOwner");
        columnExists.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WhenInternalAdminTenantExistsWithFalseFlag_RepairsItToTrue()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=file:ums-bootstrap-repair?mode=memory&cache=shared");
        await connection.OpenAsync(ct);

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
                    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                    "IsManagementOwner" INTEGER NOT NULL DEFAULT 0
                );
                INSERT INTO "Tenants" (
                    "Id", "Code", "Name", "StatusId", "CreatedBy", "CreatedAtUtc", "AuditTimeSpan", "IsDeleted", "IsManagementOwner"
                ) VALUES (
                    '11111111-1111-1111-1111-111111111111',
                    'INTERNAL_ADMIN',
                    'Internal Admin Tenant',
                    1,
                    '00000000-0000-0000-0000-000000000001',
                    '2026-06-02T00:00:00Z',
                    '0:00:00',
                    0,
                    0
                );
                """;
            await setup.ExecuteNonQueryAsync(ct);
        }

        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new UmsPlatformDbContext(options, new SystemTenantContext());

        await SqliteSchemaBootstrapper.InitializeAsync(context, ct);

        await using var verify = connection.CreateCommand();
        verify.CommandText = """
            SELECT "IsManagementOwner"
            FROM "Tenants"
            WHERE upper("Code") = 'INTERNAL_ADMIN'
            LIMIT 1;
            """;

        var value = await verify.ExecuteScalarAsync(ct);
        value.Should().NotBeNull();
        Convert.ToInt32(value).Should().Be(1);
    }

    private static async Task<bool> ColumnExistsAsync(
        SqliteConnection connection,
        string tableName,
        string columnName)
    {
        var ct = TestContext.Current.CancellationToken;

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT 1
            FROM pragma_table_info('{tableName}')
            WHERE name = '{columnName}'
            LIMIT 1;
            """;

        var result = await command.ExecuteScalarAsync(ct);
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
