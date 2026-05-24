using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Ums.Infrastructure.Persistence;

public static partial class SqlServerSchemaBootstrapper
{
    private static readonly string[] ScriptOrder =
    [
        "20260521_sqlserver_platform_outbox.sql",
        "20260521_sqlserver_identity_aggregates.sql",
        "20260521_sqlserver_authorization_profiles.sql",
        "20260522_sqlserver_identity_delegations.sql",
        "20260524_sqlserver_authorization_advanced.sql",
        "20260523_sqlserver_configuration_aggregates.sql",
        "20260523_sqlserver_audit_records.sql",
        "20260523_sqlserver_approvals.sql",
        "20260523_soft_delete_gdpr.sql",                   // REC-16
        "20260523_outbox_dispatch_lease.sql",              // HARDENING-01
    ];

    /// <summary>
    /// HARDENING-05: Runs all pending migration scripts under a SQL Server distributed lock
    /// (sp_getapplock) so that concurrent pod startups do not run migrations simultaneously.
    ///
    /// Lock semantics:
    /// - Mode = Exclusive → only one session holds the lock at a time.
    /// - Timeout = 60 000 ms → pods wait up to 60 s for the lock; they do not crash on startup.
    /// - The lock is released automatically when the connection is returned to the pool (end of scope).
    /// - All migration SQL scripts are idempotent (IF NOT EXISTS guards) so a second pod that
    ///   acquires the lock after the first finishes will simply execute no-ops.
    /// </summary>
    public static async Task InitializeAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Acquire distributed lock before running migrations.
        await dbContext.Database.ExecuteSqlRawAsync(
            "EXEC sp_getapplock @Resource = N'ums_schema_migrations', @LockMode = N'Exclusive', " +
            "@LockOwner = N'Session', @LockTimeout = 60000;",
            cancellationToken);

        try
        {
            var assembly = typeof(SqlServerSchemaBootstrapper).Assembly;

            foreach (var scriptName in ScriptOrder)
            {
                var resourceName = assembly
                    .GetManifestResourceNames()
                    .SingleOrDefault(name => name.EndsWith(scriptName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Embedded SQL script '{scriptName}' was not found.");

                await using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Embedded SQL script '{resourceName}' could not be opened.");

                using var reader = new StreamReader(stream);
                var sql = await reader.ReadToEndAsync(cancellationToken);

                foreach (var batch in SplitBatches(sql))
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    await dbContext.Database.ExecuteSqlRawAsync(batch, cancellationToken);
                }
            }
        }
        finally
        {
            // Release the lock regardless of success or failure so other pods are not blocked.
            await dbContext.Database.ExecuteSqlRawAsync(
                "EXEC sp_releaseapplock @Resource = N'ums_schema_migrations', @LockOwner = N'Session';",
                cancellationToken);
        }
    }

    private static IEnumerable<string> SplitBatches(string sql)
    {
        return GoBatchRegex()
            .Split(sql)
            .Select(batch => batch.Trim())
            .Where(batch => !string.IsNullOrWhiteSpace(batch));
    }

    [GeneratedRegex(@"^\s*GO\s*$(\r?\n)?", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoBatchRegex();
}
