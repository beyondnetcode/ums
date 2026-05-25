namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Kernel.ValueObjects;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;

public static class AuthorizationDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var suiteRepository = serviceProvider.GetService<ISystemSuiteRepository>();
        var inMemorySuiteRepository = serviceProvider.GetService<InMemorySystemSuiteRepository>();

        var templateRepository = serviceProvider.GetService<IPermissionTemplateRepository>();
        var inMemoryTemplateRepository = serviceProvider.GetService<InMemoryPermissionTemplateRepository>();

        var profileRepository = serviceProvider.GetService<IProfileRepository>();
        var inMemoryProfileRepository = serviceProvider.GetService<InMemoryProfileRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);
        var ransaTenantId = TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId));

        // Seed SystemSuites
        var suites = BuildSeedSystemSuites(ransaTenantId, actor);
        if (inMemorySuiteRepository is not null)
        {
            foreach (var suite in suites) inMemorySuiteRepository.Seed(suite);
        }
        else if (suiteRepository is not null)
        {
            var existing = await suiteRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var suite in suites) await suiteRepository.AddAsync(suite, cancellationToken);
                await suiteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Seed PermissionTemplates
        var templates = BuildSeedPermissionTemplates(ransaTenantId, suites, actor);
        if (inMemoryTemplateRepository is not null)
        {
            foreach (var template in templates) inMemoryTemplateRepository.Seed(template);
        }
        else if (templateRepository is not null)
        {
            var existing = await templateRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var template in templates) await templateRepository.AddAsync(template, cancellationToken);
                await templateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Seed Profiles
        var profiles = BuildSeedProfiles(ransaTenantId, actor);
        if (inMemoryProfileRepository is not null)
        {
            foreach (var profile in profiles) inMemoryProfileRepository.Seed(profile);
        }
        else if (profileRepository is not null)
        {
            var existing = await profileRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var profile in profiles) await profileRepository.AddAsync(profile, cancellationToken);
                await profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
    }

    private static IReadOnlyList<SystemSuiteAggregate> BuildSeedSystemSuites(TenantId tenantId, ActorId actor)
    {
        var suites = new List<SystemSuiteAggregate>();

        var coreResult = SystemSuiteAggregate.Create(
            tenantId,
            Code.Create("LOGISTICS_CORE"),
            Name.Create("Logistics Core"),
            Description.Create("Core operations and user management"),
            actor);

        var wmsResult = SystemSuiteAggregate.Create(
            tenantId,
            Code.Create("WMS"),
            Name.Create("Warehouse Management"),
            Description.Create("Warehouse inventory management"),
            actor);

        if (coreResult.IsSuccess) suites.Add(coreResult.Value);
        if (wmsResult.IsSuccess) suites.Add(wmsResult.Value);

        return suites;
    }

    private static IReadOnlyList<PermissionTemplateAggregate> BuildSeedPermissionTemplates(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, ActorId actor)
    {
        var templates = new List<PermissionTemplateAggregate>();
        if (suites.Count == 0) return templates;

        // PermissionTemplate.Create(TenantId, RoleId, SystemSuiteId, ActorId)
        var adminResult = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId)),
            suites[0].GetId(),
            actor);

        if (adminResult.IsSuccess)
        {
            adminResult.Value.AddItem(
                ExclusiveArcTarget.SystemSuite,
                suites[0].GetId(),
                ActionId.Create(),
                true,
                false,
                actor);
            templates.Add(adminResult.Value);
        }

        if (suites.Count > 1)
        {
            var operatorResult = PermissionTemplateAggregate.Create(
                tenantId,
                RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminProfileId)),
                suites[1].GetId(),
                actor);

            if (operatorResult.IsSuccess)
            {
                operatorResult.Value.AddItem(
                    ExclusiveArcTarget.SystemSuite,
                    suites[1].GetId(),
                    ActionId.Create(),
                    true,
                    false,
                    actor);
                templates.Add(operatorResult.Value);
            }
        }

        return templates;
    }

    private static IReadOnlyList<ProfileAggregate> BuildSeedProfiles(TenantId tenantId, ActorId actor)
    {
        var profiles = new List<ProfileAggregate>();

        var adminProfile = ProfileAggregate.Create(
            tenantId,
            UserId.Load(Guid.Parse(CoreDevDataSeeder.RansaAdminUserId)),
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId)),
            null,
            actor);

        if (adminProfile.IsSuccess)
            profiles.Add(adminProfile.Value);

        return profiles;
    }
}
