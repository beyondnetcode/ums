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
        "20260523_sqlserver_configuration_aggregates.sql",
        "20260523_sqlserver_audit_records.sql",
        "20260523_sqlserver_approvals.sql",
        "20260523_soft_delete_gdpr.sql",                   // REC-16
    ];

    public static async Task InitializeAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

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
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                await dbContext.Database.ExecuteSqlRawAsync(batch, cancellationToken);
            }
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
