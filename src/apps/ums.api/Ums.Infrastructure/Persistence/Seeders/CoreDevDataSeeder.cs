namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;

public static class CoreDevDataSeeder
{
    public const string SystemActorId = "00000000-0000-0000-0000-000000000001";

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

    public static async Task SeedAllAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        // 1. Identity
        await IdentityDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);

        // 2. Authorization
        await AuthorizationDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);

        // 3. Approvals
        await ApprovalsDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);

        // 4. Configuration
        await ConfigurationDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);

        // 5. IGA
        await IgaDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);

        // 6. Audit
        await AuditDevDataSeeder.SeedAsync(serviceProvider, cancellationToken);
    }
}
