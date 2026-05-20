namespace Ums.Infrastructure.Persistence;

using Ums.Domain.Enums;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Kernel.ValueObjects;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

/// <summary>
/// Seeds the in-memory tenant repository with well-known dev GUIDs so that
/// frontend prototype requests do not return 404 during local development.
/// This seeder is infrastructure-only and must never run in production.
/// </summary>
public static class DevDataSeeder
{
    private const string SystemActorId = "00000000-0000-0000-0000-000000000001";

    public static void Seed(InMemoryTenantRepository repository)
    {
        var actor = ActorId.Create(SystemActorId);

        SeedTenant(
            repository,
            id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            code: "DEV_ROOT",
            name: "Default Developer Tenant (Root)",
            companyRef: "REF_ROOT_001",
            parentId: null,
            actor: actor,
            branches:
            [
                ("HQ_MAIN", "Headquarters Main"),
                ("DEV_LAB", "Developer Lab Branch"),
            ]);

        SeedTenant(
            repository,
            id: Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"),
            code: "ACME_INT",
            name: "Acme Global Logistics",
            companyRef: "ACME-902",
            parentId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            actor: actor,
            branches:
            [
                ("ACME_WEST", "Acme West Region"),
                ("ACME_EAST", "Acme East Region"),
            ]);

        SeedTenant(
            repository,
            id: Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"),
            code: "CONTOSO_EMEA",
            name: "Contoso Retail EMEA",
            companyRef: "CONT-EMEA-88",
            parentId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            actor: actor,
            branches:
            [
                ("CONT_UK", "Contoso UK Office"),
            ]);

        SeedTenant(
            repository,
            id: Guid.Parse("9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe"),
            code: "NWD_IND",
            name: "Northwind Industrial",
            companyRef: "NWD-CORE-10",
            parentId: null,
            actor: actor,
            branches:
            [
                ("NWD_PLANT1", "Northwind Plant Alpha"),
                ("NWD_PLANT2", "Northwind Plant Beta"),
            ]);

        SeedTenant(
            repository,
            id: Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654"),
            code: "VNG_HLTH",
            name: "Vanguard Health Systems",
            companyRef: "VNG-HEALTH-5",
            parentId: null,
            actor: actor,
            branches:
            [
                ("VNG_CLINIC", "Vanguard Main Clinic"),
            ]);

        SeedTenant(
            repository,
            id: Guid.Parse("f3e2d1c0-b9a8-7f6e-5d4c-321098765432"),
            code: "APX_FIN",
            name: "Apex Financial Group",
            companyRef: "APX-GLOBAL-7",
            parentId: null,
            actor: actor,
            branches:
            [
                ("APX_NYC", "Apex New York Office"),
                ("APX_LON", "Apex London Office"),
            ]);
    }

    private static void SeedTenant(
        InMemoryTenantRepository repository,
        Guid id,
        string code,
        string name,
        string companyRef,
        Guid? parentId,
        ActorId actor,
        (string Code, string Name)[] branches)
    {
        var companyReference = CompanyReference.Create(companyRef);
        var parentTenantId = parentId.HasValue ? TenantId.Load(parentId.Value) : null;

        var result = TenantAggregate.Create(
            Code.Create(code),
            Name.Create(name),
            OrganizationType.INTERNAL,
            actor,
            IdpStrategy.InternalBcrypt,
            companyReference,
            parentTenantId);

        if (result.IsFailure) return;

        var tenant = result.Value;

        foreach (var (branchCode, branchName) in branches)
        {
            tenant.AddBranch(
                Code.Create(branchCode),
                Name.Create(branchName),
                actor);
        }

        // Store under the well-known dev GUID (overrides the auto-generated one)
        repository.SeedWithKnownId(id, tenant);
    }
}
