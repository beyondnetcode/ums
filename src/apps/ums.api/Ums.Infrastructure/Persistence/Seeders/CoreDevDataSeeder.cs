namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;

public static class CoreDevDataSeeder
{
    public const string SystemActorId = "00000000-0000-0000-0000-000000000001";

    // ── Internal Admin Tenant (global administration) ─────────────────────────
    public const string InternalAdminTenantId = "11111111-1111-1111-1111-111111111111";
    public const string InternalAdminTenantCode = "INTERNAL_ADMIN";
    public const string InternalAdminTenantName = "Internal Admin Tenant";

    // ── SuperAdmin User (global admin) ─────────────────────────────────────────
    public const string SuperAdminUserId = "22222222-2222-2222-2222-222222222222";
    public const string SuperAdminUsername = "admin";
    public const string SuperAdminPassword = "root"; // Default password for INTERNAL admin (change in production)
    public const string InternalAdminPendingUserId = "11111103-1111-1111-1111-111111111111";

    // ── GlobalAdmin Role & Profile ─────────────────────────────────────────────
    public const string GlobalAdminRoleId = "33333333-3333-3333-3333-333333333333";
    public const string GlobalAdminProfileId = "44444444-4444-4444-4444-444444444444";
    public const string GlobalAdminTemplateId = "55555555-5555-5555-5555-555555555555";

    // ── Ransa Tenant (existing commercial tenant) ─────────────────────────────
    public const string RansaTenantId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    // Derived from RansaTenantId bytes: bytes[0] replaced by index (little-endian Data1)
    public const string RansaAdminUserId = "3fa85f01-5717-4562-b3fc-2c963f66afa6";   // gerente.operaciones — DeriveGuid(1)
    public const string RansaAnalystUserId = "3fa85f02-5717-4562-b3fc-2c963f66afa6"; // analista.inventario — DeriveGuid(2)

    // Fixed placeholder GUIDs for cross-context dev references
    public const string DemoAdminRoleId = "aaaa0001-0000-0000-0000-000000000001";
    public const string DemoOperatorRoleId = "aaaa0002-0000-0000-0000-000000000001";
    public const string DemoAdminProfileId = "bbbb0001-0000-0000-0000-000000000001";
    public const string RansaEntraIdProviderId = "cccc0001-0000-0000-0000-000000000001";
    public const string DemoSystemSuiteId = "dddd0001-0000-0000-0000-000000000001";
    public const string InternalAdminInboxWorkflowId = "88888888-3333-3333-3333-333333333333";

    public static async Task SeedAllAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await RunSeederAsync(serviceProvider, "Identity", IdentityDevDataSeeder.SeedAsync, cancellationToken);
        await RunSeederAsync(serviceProvider, "Authorization", AuthorizationDevDataSeeder.SeedAsync, cancellationToken);
        await RunSeederAsync(serviceProvider, "Configuration", ConfigurationDevDataSeeder.SeedAsync, cancellationToken);
        await RunSeederAsync(serviceProvider, "Parameter catalog", ParameterCatalogSeeder.SeedAsync, cancellationToken);
        await RunSeederAsync(serviceProvider, "Approvals", ApprovalsDevDataSeeder.SeedAsync, cancellationToken);
        await RunSeederAsync(serviceProvider, "Audit", AuditDevDataSeeder.SeedAsync, cancellationToken);
    }

    private static async Task RunSeederAsync(
        IServiceProvider serviceProvider,
        string seederName,
        Func<IServiceProvider, CancellationToken, Task> seedAsync,
        CancellationToken cancellationToken)
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        try
        {
            await seedAsync(scope.ServiceProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{seederName} seeder failed: {ex.Message}");
        }
    }
}
