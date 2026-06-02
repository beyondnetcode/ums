using Microsoft.EntityFrameworkCore;

namespace Ums.Infrastructure.Persistence;

using Ums.Infrastructure.Persistence.Seeders;

public static class SqliteSchemaBootstrapper
{
    public static async Task InitializeAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureTenantManagementOwnerColumnAsync(dbContext, cancellationToken);
        await EnsureInternalAdminTenantManagementOwnerAsync(dbContext, cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "Roles" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "SystemSuiteId" TEXT NOT NULL,
                "ParentRoleId" TEXT NULL,
                "Code" TEXT NOT NULL,
                "Value" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "HierarchyLevel" INTEGER NOT NULL,
                "PromotionOrder" INTEGER NOT NULL,
                "IsActive" INTEGER NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedBy" TEXT NULL,
                "UpdatedAtUtc" TEXT NULL,
                "AuditTimeSpan" TEXT NOT NULL,
                "RowVersion" BLOB NOT NULL,
                CONSTRAINT "FK_Roles_SystemSuites_SystemSuiteId"
                    FOREIGN KEY ("SystemSuiteId") REFERENCES "SystemSuites" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_Roles_Roles_ParentRoleId"
                    FOREIGN KEY ("ParentRoleId") REFERENCES "Roles" ("Id") ON DELETE RESTRICT
            );
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Roles_TenantId" ON "Roles" ("TenantId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Roles_ParentRoleId" ON "Roles" ("ParentRoleId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Roles_SystemSuiteId" ON "Roles" ("SystemSuiteId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Roles_SystemSuiteId_Code" ON "Roles" ("SystemSuiteId", "Code");""",
            cancellationToken);
    }

    private static async Task EnsureTenantManagementOwnerColumnAsync(
        UmsPlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT 1
                FROM pragma_table_info('Tenants')
                WHERE name = 'IsManagementOwner'
                LIMIT 1;
                """;

            var exists = await command.ExecuteScalarAsync(cancellationToken);
            if (exists is not null)
            {
                return;
            }

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE "Tenants"
                ADD COLUMN "IsManagementOwner" INTEGER NOT NULL DEFAULT 0;
                """,
                cancellationToken);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task EnsureInternalAdminTenantManagementOwnerAsync(
        UmsPlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            $"""
            UPDATE "Tenants"
            SET "IsManagementOwner" = 1
            WHERE upper("Code") = '{CoreDevDataSeeder.InternalAdminTenantCode}'
              AND COALESCE("IsManagementOwner", 0) = 0;
            """,
            cancellationToken);
    }
}
