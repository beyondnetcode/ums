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

        // Root: Ransa Comercial S.A. — Lima (operador logístico líder del Perú)
        SeedTenant(
            repository,
            id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            code: "RANSA_PERU",
            name: "Ransa Comercial S.A.",
            companyRef: "RUC-20101024645",
            type: OrganizationType.INTERNAL,
            parentId: null,
            actor: actor,
            branches:
            [
                ("RANSA_LIM_NORTE",  "Almacén Lima Norte — Independencia"),
                ("RANSA_LIM_SUR",    "Almacén Lima Sur — Lurín"),
                ("RANSA_CALLAO_HQ",  "Sede Principal Callao"),
            ]);

        // Sub-tenant: Neptunia S.A. — Callao (almacenaje y distribución portuaria)
        SeedTenant(
            repository,
            id: Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"),
            code: "NEPTUNIA",
            name: "Neptunia S.A. — Callao",
            companyRef: "RUC-20330791684",
            type: OrganizationType.CLIENT,
            parentId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            actor: actor,
            branches:
            [
                ("NEP_CALLAO_DP1",   "Depósito Portuario 1 — Av. Néstor Gambetta"),
                ("NEP_CALLAO_DP2",   "Depósito Portuario 2 — Zona Industrial"),
            ]);

        // Sub-tenant: APM Terminals Callao S.A. (terminal de contenedores)
        SeedTenant(
            repository,
            id: Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"),
            code: "APM_CALLAO",
            name: "APM Terminals Callao S.A.",
            companyRef: "RUC-20516357498",
            type: OrganizationType.CLIENT,
            parentId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            actor: actor,
            branches:
            [
                ("APM_MUELLE_N",     "Muelle Norte — Terminal Contenedores"),
            ]);

        // Terminal Portuario de Paita S.A. — Puerto de Paita, Piura
        SeedTenant(
            repository,
            id: Guid.Parse("9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe"),
            code: "PAITA_PORT",
            name: "Terminal Portuario de Paita S.A.",
            companyRef: "RUC-20512180098",
            type: OrganizationType.CLIENT,
            parentId: null,
            actor: actor,
            branches:
            [
                ("PAITA_MUELLE",     "Muelle de Transferencia — Puerto Paita"),
                ("PAITA_ALMACEN",    "Almacén General Paita"),
            ]);

        // Unimar S.A. — agencia marítima, Lima y Callao
        SeedTenant(
            repository,
            id: Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654"),
            code: "UNIMAR",
            name: "Unimar S.A. — Lima",
            companyRef: "RUC-20101523381",
            type: OrganizationType.SUPPLIER,
            parentId: null,
            actor: actor,
            branches:
            [
                ("UNI_MIRAFLORES",   "Oficina Miraflores — Av. Larco"),
                ("UNI_CALLAO_OP",    "Operaciones Callao — Jr. Colón"),
            ]);

        // Intradevco Industrial S.A. — distribución y transporte nacional
        SeedTenant(
            repository,
            id: Guid.Parse("f3e2d1c0-b9a8-7f6e-5d4c-321098765432"),
            code: "INTRADEVCO",
            name: "Intradevco Industrial S.A.",
            companyRef: "RUC-20101041268",
            type: OrganizationType.SUPPLIER,
            parentId: null,
            actor: actor,
            branches:
            [
                ("INTRA_SJL",        "Planta San Juan de Lurigancho"),
                ("INTRA_ATE",        "Almacén Ate Vitarte — Carretera Central"),
            ]);
    }

    private static void SeedTenant(
        InMemoryTenantRepository repository,
        Guid id,
        string code,
        string name,
        string companyRef,
        OrganizationType type,
        Guid? parentId,
        ActorId actor,
        (string Code, string Name)[] branches)
    {
        var companyReference = CompanyReference.Create(companyRef);
        var parentTenantId = parentId.HasValue ? TenantId.Load(parentId.Value) : null;

        var result = TenantAggregate.Create(
            Code.Create(code),
            Name.Create(name),
            type,
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

        repository.SeedWithKnownId(id, tenant);
    }
}
