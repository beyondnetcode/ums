namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Enums;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branding;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Identity.UserManagementDelegation;
using Ums.Domain.Kernel.ValueObjects;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;
using UserManagementDelegationAggregate = Ums.Domain.Identity.UserManagementDelegation.UserManagementDelegation;

public static class IdentityDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var tenantRepository = serviceProvider.GetService<ITenantRepository>();
        var inMemoryTenantRepository = serviceProvider.GetService<InMemoryTenantRepository>();
        
        var userAccountRepository = serviceProvider.GetService<IUserAccountRepository>();
        var inMemoryUserAccountRepository = serviceProvider.GetService<InMemoryUserAccountRepository>();
        
        var delegationRepository = serviceProvider.GetService<IUserManagementDelegationRepository>();
        var inMemoryDelegationRepository = serviceProvider.GetService<InMemoryUserManagementDelegationRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);

        // Seed Tenants
        var tenants = BuildSeedTenants(actor);
        if (inMemoryTenantRepository is null && tenantRepository is not null)
        {
            var alreadySeeded = await tenantRepository.GetByCodeAsync("RANSA_PERU", cancellationToken);
            if (alreadySeeded is null)
            {
                foreach (var tenant in tenants)
                {
                    await tenantRepository.AddAsync(tenant, cancellationToken);
                }
                await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
        else if (inMemoryTenantRepository is not null)
        {
            foreach (var tenant in tenants)
            {
                inMemoryTenantRepository.Seed(tenant);
            }
        }

        // Seed User Accounts
        var userAccounts = BuildSeedUserAccounts(actor);
        if (inMemoryUserAccountRepository is null && userAccountRepository is not null)
        {
            var alreadySeeded = await userAccountRepository.GetByTenantIdAsync(tenants[0].Props.Id.GetValue(), cancellationToken);
            if (alreadySeeded.Count == 0)
            {
                foreach (var userAccount in userAccounts)
                {
                    await userAccountRepository.AddAsync(userAccount, cancellationToken);
                }
                await userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
        else if (inMemoryUserAccountRepository is not null)
        {
            foreach (var userAccount in userAccounts)
            {
                inMemoryUserAccountRepository.Seed(userAccount);
            }
        }

        // Seed Delegations
        var delegations = BuildSeedDelegations(actor);
        if (inMemoryDelegationRepository is null && delegationRepository is not null)
        {
            var alreadySeeded = await delegationRepository.GetAllAsync(cancellationToken);
            if (alreadySeeded.Count == 0)
            {
                foreach (var delegation in delegations)
                {
                    await delegationRepository.AddAsync(delegation, cancellationToken);
                }
                await delegationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
        else if (inMemoryDelegationRepository is not null)
        {
            foreach (var delegation in delegations)
            {
                inMemoryDelegationRepository.Seed(delegation);
            }
        }
    }

    private static IReadOnlyList<TenantAggregate> BuildSeedTenants(ActorId actor)
    {
        var ransa = BuildTenant(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "RANSA_PERU", "Ransa Comercial S.A.", "RUC-20101024645", OrganizationType.INTERNAL, null, actor,
            [("RANSA_LIM_NORTE", "Almacén Lima Norte — Independencia"), ("RANSA_LIM_SUR", "Almacén Lima Sur — Lurín"), ("RANSA_CALLAO_HQ", "Sede Principal Callao")]);
        
        var neptunia = BuildTenant(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"), "NEPTUNIA", "Neptunia S.A. — Callao", "RUC-20330791684", OrganizationType.CLIENT, Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), actor,
            [("NEP_CALLAO_DP1", "Depósito Portuario 1 — Av. Néstor Gambetta"), ("NEP_CALLAO_DP2", "Depósito Portuario 2 — Zona Industrial")]);
        
        var apm = BuildTenant(Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"), "APM_CALLAO", "APM Terminals Callao S.A.", "RUC-20516357498", OrganizationType.CLIENT, Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), actor,
            [("APM_MUELLE_N", "Muelle Norte — Terminal Contenedores")]);
        
        var paita = BuildTenant(Guid.Parse("9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe"), "PAITA_PORT", "Terminal Portuario de Paita S.A.", "RUC-20512180098", OrganizationType.CLIENT, null, actor,
            [("PAITA_MUELLE", "Muelle de Transferencia — Puerto Paita"), ("PAITA_ALMACEN", "Almacén General Paita")]);
        
        var unimar = BuildTenant(Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654"), "UNIMAR", "Unimar S.A. — Lima", "RUC-20101523381", OrganizationType.SUPPLIER, null, actor,
            [("UNI_MIRAFLORES", "Oficina Miraflores — Av. Larco"), ("UNI_CALLAO_OP", "Operaciones Callao — Jr. Colón")]);
        
        var intradevco = BuildTenant(Guid.Parse("f3e2d1c0-b9a8-7f6e-5d4c-321098765432"), "INTRADEVCO", "Intradevco Industrial S.A.", "RUC-20101041268", OrganizationType.SUPPLIER, null, actor,
            [("INTRA_SJL", "Planta San Juan de Lurigancho"), ("INTRA_ATE", "Almacén Ate Vitarte — Carretera Central")]);

        return [ransa, neptunia, apm, paita, unimar, intradevco];
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

        if (code == "RANSA_PERU")
        {
            tenant.RegisterIdentityProvider(Code.Create("ENTRA_ID"), Name.Create("Azure AD Corporativo"), Description.Create("Directorio principal Ransa"), IdpStrategy.AzureAd, actor);
            tenant.ActivateIdentityProvider(tenant.IdentityProviders.First().GetId(), actor);
            
            var branding = BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("base64_ransa_logo_data"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#006400"), BackgroundStyle.SolidColor)
                .WithTexts(LoginText.Create("Bienvenido a Ransa"), LoginText.Create("Ingresa tus credenciales"), LoginText.Create("Iniciar sesión"), LoginText.Create("© 2026 Ransa Comercial"))
                .WithCustomDomain(CustomDomain.Create("login.ransa.pe"))
                .WithMagicLinkFallback(true)
                .Build();
            tenant.SetBranding(branding, actor);
        }
        else if (code == "NEPTUNIA")
        {
            tenant.RegisterIdentityProvider(Code.Create("OKTA_CORP"), Name.Create("Okta Neptunia"), Description.Create("Directorio subsidiarias"), IdpStrategy.Okta, actor);
            tenant.ActivateIdentityProvider(tenant.IdentityProviders.First().GetId(), actor);
            
            var branding = BrandingSettings.CreateBuilder()
                .WithLogo(Logo.Create("base64_neptunia_logo_data"), LogoFormat.Png)
                .WithTheme(HexColor.Create("#00008B"), BackgroundStyle.Gradient)
                .WithTexts(LoginText.Create("Portal Neptunia"), LoginText.Create("Accesos a operaciones portuarias"), LoginText.Create("Entrar"), LoginText.Create("© 2026 Neptunia"))
                .WithCustomDomain(CustomDomain.Create("acceso.neptunia.pe"))
                .WithMagicLinkFallback(false)
                .Build();
            tenant.SetBranding(branding, actor);
        }

        return tenant;
    }

    private static IReadOnlyList<UserAccountAggregate> BuildSeedUserAccounts(ActorId actor)
    {
        var ransaTenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        var neptuniaTenantId = TenantId.Load(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"));
        var apmTenantId = TenantId.Load(Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc"));
        var unimarTenantId = TenantId.Load(Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654"));

        return
        [
            ..BuildSeedUserAccountsForTenant(ransaTenantId, actor),
            ..BuildSeedUserAccountsForTenant(neptuniaTenantId, actor),
            ..BuildSeedUserAccountsForTenant(apmTenantId, actor),
            ..BuildSeedUserAccountsForTenant(unimarTenantId, actor)
        ];
    }

    private static IReadOnlyList<UserAccountAggregate> BuildSeedUserAccountsForTenant(TenantId tenantId, ActorId actor)
    {
        var baseGuidBytes = tenantId.GetValue().ToByteArray();
        Guid DeriveGuid(byte index)
        {
            var bytes = (byte[])baseGuidBytes.Clone();
            bytes[0] = index;
            return new Guid(bytes);
        }

        var domain = tenantId.GetValue().ToString().StartsWith("3fa8") ? "ransa.pe" :
                     tenantId.GetValue().ToString().StartsWith("c9b7") ? "neptunia.pe" :
                     tenantId.GetValue().ToString().StartsWith("a3f5") ? "apmterminals.com" :
                     tenantId.GetValue().ToString().StartsWith("5f4e") ? "unimar.com.pe" :
                     "logistics.pe";

        var admin = BuildUserAccount(DeriveGuid(1), tenantId, $"gerente.operaciones@{domain}", UserCategory.Internal, actor, "EMP-001");
        admin.Activate(actor);

        var analyst = BuildUserAccount(DeriveGuid(2), tenantId, $"analista.inventario@{domain}", UserCategory.Internal, actor, "EMP-002");
        analyst.Activate(actor);

        var pending = BuildUserAccount(DeriveGuid(3), tenantId, $"coordinador.flota@{domain}", UserCategory.External, actor, "EXT-101");

        var blocked = BuildUserAccount(DeriveGuid(4), tenantId, $"ex.empleado@{domain}", UserCategory.External, actor, "EXT-102");
        blocked.Activate(actor);
        blocked.Block(Reason.Create("Desvinculación laboral"), actor);

        var partner = BuildUserAccount(DeriveGuid(5), tenantId, $"auditor.externo@aduanas.gob.pe", UserCategory.Partner, actor, "DNI-44556677");
        partner.Activate(actor);

        return [admin, analyst, pending, blocked, partner];
    }

    private static UserAccountAggregate BuildUserAccount(
        Guid id,
        TenantId tenantId,
        string email,
        UserCategory category,
        ActorId actor,
        string identityReferenceStr)
    {
        var emailVo = Email.Create(email);
        var identityReference = IdentityReference.Create(identityReferenceStr);
        var identityReferenceType = IdentityReferenceType.HrId;

        var result = UserAccountAggregate.Create(
            tenantId,
            emailVo,
            category,
            identityReference,
            identityReferenceType,
            actor,
            userAccountId: UserAccountId.Load(id));

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unable to build dev user account seed {email}: {result.Error}");
        }

        return result.Value;
    }

    private static IReadOnlyList<UserManagementDelegationAggregate> BuildSeedDelegations(ActorId actor)
    {
        var ransaTenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        var ransaBaseGuidBytes = ransaTenantId.GetValue().ToByteArray();
        Guid DeriveRansaGuid(byte index)
        {
            var bytes = (byte[])ransaBaseGuidBytes.Clone();
            bytes[0] = index;
            return new Guid(bytes);
        }

        var neptuniaTenantId = TenantId.Load(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542"));
        var neptuniaBaseBytes = neptuniaTenantId.GetValue().ToByteArray();
        var neptuniaDelegatedAdminId = new Guid(neptuniaBaseBytes.Select((b, i) => i == 0 ? (byte)2 : b).ToArray());

        var result = UserManagementDelegationAggregate.Create(
            ransaTenantId,
            UserAccountId.Load(DeriveRansaGuid(1)), // gerente operaciones ransa
            UserAccountId.Load(neptuniaDelegatedAdminId), // analista inventario neptunia
            DelegationScopeType.Tenant,
            null,
            new[] { DelegatedAction.CreateUser, DelegatedAction.BlockUser },
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30),
            90, // maxDurationDays
            false,
            actor);
            
        var delegation = result.Value;
        delegation.Activate(actor);

        return new[] { delegation };
    }
}
