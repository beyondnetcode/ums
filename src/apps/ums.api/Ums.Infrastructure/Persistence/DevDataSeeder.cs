namespace Ums.Infrastructure.Persistence;

using Ums.Domain.Enums;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel.ValueObjects;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

/// <summary>
/// Seeds the in-memory tenant repository with well-known dev GUIDs so that
/// frontend prototype requests do not return 404 during local development.
/// This seeder is infrastructure-only and must never run in production.
/// </summary>
public static class DevDataSeeder
{
    private const string SystemActorId = "00000000-0000-0000-0000-000000000001";

    public static async Task SeedAsync(ITenantRepository repository, CancellationToken cancellationToken = default)
    {
        var alreadySeeded = await repository.GetByCodeAsync("RANSA_PERU", cancellationToken);
        if (alreadySeeded is not null)
        {
            return;
        }

        var actor = ActorId.Create(SystemActorId);

        foreach (var tenant in BuildSeedTenants(actor))
        {
            await repository.AddAsync(tenant, cancellationToken);
        }

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    public static async Task SeedUserAccountsAsync(IUserAccountRepository repository, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var actor = ActorId.Create(SystemActorId);
        var tenantIdVo = TenantId.Load(tenantId);

        foreach (var userAccount in BuildSeedUserAccountsForTenant(tenantIdVo, actor))
        {
            await repository.AddAsync(userAccount, cancellationToken);
        }

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    public static void Seed(InMemoryTenantRepository repository)
    {
        var actor = ActorId.Create(SystemActorId);
        foreach (var tenant in BuildSeedTenants(actor))
        {
            repository.Seed(tenant);
        }
    }

    public static void SeedUserAccounts(InMemoryUserAccountRepository repository)
    {
        var actor = ActorId.Create(SystemActorId);
        foreach (var userAccount in BuildSeedUserAccounts(actor))
        {
            repository.Seed(userAccount);
        }
    }

    private static IReadOnlyList<TenantAggregate> BuildSeedTenants(ActorId actor)
    {
        return
        [
            BuildTenant(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "RANSA_PERU", "Ransa Comercial S.A.", "RUC-20101024645", OrganizationType.INTERNAL, null, actor,
                [("RANSA_LIM_NORTE", "Almacén Lima Norte — Independencia"), ("RANSA_LIM_SUR", "Almacén Lima Sur — Lurín"), ("RANSA_CALLAO_HQ", "Sede Principal Callao")]),
            BuildTenant(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"), "NEPTUNIA", "Neptunia S.A. — Callao", "RUC-20330791684", OrganizationType.CLIENT, Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), actor,
                [("NEP_CALLAO_DP1", "Depósito Portuario 1 — Av. Néstor Gambetta"), ("NEP_CALLAO_DP2", "Depósito Portuario 2 — Zona Industrial")]),
            BuildTenant(Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"), "APM_CALLAO", "APM Terminals Callao S.A.", "RUC-20516357498", OrganizationType.CLIENT, Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), actor,
                [("APM_MUELLE_N", "Muelle Norte — Terminal Contenedores")]),
            BuildTenant(Guid.Parse("9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe"), "PAITA_PORT", "Terminal Portuario de Paita S.A.", "RUC-20512180098", OrganizationType.CLIENT, null, actor,
                [("PAITA_MUELLE", "Muelle de Transferencia — Puerto Paita"), ("PAITA_ALMACEN", "Almacén General Paita")]),
            BuildTenant(Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654"), "UNIMAR", "Unimar S.A. — Lima", "RUC-20101523381", OrganizationType.SUPPLIER, null, actor,
                [("UNI_MIRAFLORES", "Oficina Miraflores — Av. Larco"), ("UNI_CALLAO_OP", "Operaciones Callao — Jr. Colón")]),
            BuildTenant(Guid.Parse("f3e2d1c0-b9a8-7f6e-5d4c-321098765432"), "INTRADEVCO", "Intradevco Industrial S.A.", "RUC-20101041268", OrganizationType.SUPPLIER, null, actor,
                [("INTRA_SJL", "Planta San Juan de Lurigancho"), ("INTRA_ATE", "Almacén Ate Vitarte — Carretera Central")]),
        ];
    }

    private static IReadOnlyList<UserAccountAggregate> BuildSeedUserAccounts(ActorId actor)
    {
        var ransaTenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        var neptuniaTenantId = TenantId.Load(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"));
        var apmTenantId = TenantId.Load(Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"));

        return
        [
            ..BuildSeedUserAccountsForTenant(ransaTenantId, actor),
            ..BuildSeedUserAccountsForTenant(neptuniaTenantId, actor),
            ..BuildSeedUserAccountsForTenant(apmTenantId, actor),
        ];
    }

    private static IReadOnlyList<UserAccountAggregate> BuildSeedUserAccountsForTenant(TenantId tenantId, ActorId actor)
    {
        var tenantStr = tenantId.GetValue().ToString().Replace("-", "")[..8];

        var admin = BuildUserAccount(Guid.NewGuid(), tenantId, $"admin@{tenantStr}.pe", UserCategory.Internal, actor);
        admin.Activate(actor);

        var analyst = BuildUserAccount(Guid.NewGuid(), tenantId, $"analyst@{tenantStr}.pe", UserCategory.Internal, actor);
        analyst.Activate(actor);

        var pending = BuildUserAccount(Guid.NewGuid(), tenantId, $"external.pending@{tenantStr}.pe", UserCategory.External, actor);

        var blocked = BuildUserAccount(Guid.NewGuid(), tenantId, $"blocked@{tenantStr}.pe", UserCategory.External, actor);
        blocked.Activate(actor);
        blocked.Block(Reason.Create("Violación de políticas de seguridad"), actor);

        var partner = BuildUserAccount(Guid.NewGuid(), tenantId, $"partner@{tenantStr}.pe", UserCategory.Partner, actor);
        partner.Activate(actor);

        return [admin, analyst, pending, blocked, partner];
    }


    private static TenantAggregate BuildTenant(
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
            parentTenantId,
            TenantId.Load(id));

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unable to build dev tenant seed {code}: {result.Error}");
        }

        var tenant = result.Value;

        foreach (var (branchCode, branchName) in branches)
        {
            tenant.AddBranch(
                Code.Create(branchCode),
                Name.Create(branchName),
                actor);
        }

        return tenant;
    }

    private static UserAccountAggregate BuildUserAccount(
        Guid id,
        TenantId tenantId,
        string email,
        UserCategory category,
        ActorId actor)
    {
        var emailVo = Email.Create(email);
        var result = UserAccountAggregate.Create(
            tenantId,
            emailVo,
            category,
            identityReference: null,
            identityReferenceType: null,
            actor,
            userAccountId: UserAccountId.Load(id));

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unable to build dev user account seed {email}: {result.Error}");
        }

        return result.Value;
    }
}
