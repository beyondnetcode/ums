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
using Ums.Domain.Enums;

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

        if (coreResult.IsSuccess)
        {
            var suite = coreResult.Value;

            // Register standard actions
            suite.RegisterAction(ActionCode.Create("VIEW"), Name.Create("View Logistics Core"), actor);
            suite.RegisterAction(ActionCode.Create("MANAGE"), Name.Create("Manage Logistics Core"), actor);
            suite.RegisterAction(ActionCode.Create("APPROVE"), Name.Create("Approve Operations"), actor);

            // Add app settings
            suite.AddAppSetting(
                ConfigurationKey.Create("SessionTimeout"),
                ConfigurationValue.Create("30"),
                ConfigurationScope.Global,
                actor);
            suite.AddAppSetting(
                ConfigurationKey.Create("MaxRetries"),
                ConfigurationValue.Create("5"),
                ConfigurationScope.Global,
                actor);

            // Add security module
            var modSec = suite.AddModule(Code.Create("SEC"), Name.Create("Security & Auditing"), Description.Create("Security, user management, and audit trailing modules"), 1, actor);
            if (modSec.IsSuccess)
            {
                var module = suite.Modules.First(m => m.Code.GetValue() == "SEC");
                suite.ActivateModule(module.Id, actor);

                // Menu 1: Users
                module.AddMenu(Code.Create("USERS"), Name.Create("Users Administration"), Description.Create("Manage user accounts and details"), 1, actor);
                var menuUsers = module.Menus.First(m => m.Code.GetValue() == "USERS");

                menuUsers.AddSubMenu(Code.Create("LIST"), Name.Create("User Directory"), Description.Create("View and search all user accounts"), 1, actor);
                var subMenuList = menuUsers.SubMenus.First(sm => sm.Code.GetValue() == "LIST");
                subMenuList.AddOption(Code.Create("VIEW_USERS"), Name.Create("View Users List"), Description.Create("Permission to view the users list"), ActionCode.Create("VIEW"), 1, actor);
                subMenuList.AddOption(Code.Create("EDIT_USERS"), Name.Create("Edit User Profiles"), Description.Create("Permission to edit and modify user profiles"), ActionCode.Create("MANAGE"), 2, actor);

                menuUsers.AddSubMenu(Code.Create("ROLES"), Name.Create("Roles & Permissions"), Description.Create("Manage access control roles and templates"), 2, actor);
                var subMenuRoles = menuUsers.SubMenus.First(sm => sm.Code.GetValue() == "ROLES");
                subMenuRoles.AddOption(Code.Create("VIEW_ROLES"), Name.Create("View Security Roles"), Description.Create("Permission to view roles in system"), ActionCode.Create("VIEW"), 1, actor);
                subMenuRoles.AddOption(Code.Create("MANAGE_ROLES"), Name.Create("Configure Permissions"), Description.Create("Permission to edit access rights"), ActionCode.Create("MANAGE"), 2, actor);

                // Menu 2: Audit Logs
                module.AddMenu(Code.Create("AUDIT"), Name.Create("Audit Trails"), Description.Create("System operations logging and analysis"), 2, actor);
                var menuAudit = module.Menus.First(m => m.Code.GetValue() == "AUDIT");

                menuAudit.AddSubMenu(Code.Create("LOGS"), Name.Create("System Logs"), Description.Create("View system telemetry and user transactions"), 1, actor);
                var subMenuLogs = menuAudit.SubMenus.First(sm => sm.Code.GetValue() == "LOGS");
                subMenuLogs.AddOption(Code.Create("VIEW_LOGS"), Name.Create("Search Audit Trail"), Description.Create("Permission to query audit logs"), ActionCode.Create("VIEW"), 1, actor);
                subMenuLogs.AddOption(Code.Create("PURGE_LOGS"), Name.Create("Purge Historical Data"), Description.Create("Permission to clear obsolete log records"), ActionCode.Create("APPROVE"), 2, actor);
            }

            // Add config module
            var modConfig = suite.AddModule(Code.Create("CONFIG"), Name.Create("System Configuration"), Description.Create("Global properties, settings and email setup"), 2, actor);
            if (modConfig.IsSuccess)
            {
                var module = suite.Modules.First(m => m.Code.GetValue() == "CONFIG");
                suite.ActivateModule(module.Id, actor);

                // Menu 1: System Settings
                module.AddMenu(Code.Create("SETTINGS"), Name.Create("Global Setup"), Description.Create("Configure global system variables"), 1, actor);
                var menuSettings = module.Menus.First(m => m.Code.GetValue() == "SETTINGS");

                menuSettings.AddSubMenu(Code.Create("PARAMS"), Name.Create("App Parameters"), Description.Create("Configure timeouts, thresholds and limits"), 1, actor);
                var subMenuParams = menuSettings.SubMenus.First(sm => sm.Code.GetValue() == "PARAMS");
                subMenuParams.AddOption(Code.Create("VIEW_PARAMS"), Name.Create("View Parameters"), Description.Create("Permission to view system options"), ActionCode.Create("VIEW"), 1, actor);
                subMenuParams.AddOption(Code.Create("EDIT_PARAMS"), Name.Create("Update Global Config"), Description.Create("Permission to edit critical global values"), ActionCode.Create("MANAGE"), 2, actor);

                menuSettings.AddSubMenu(Code.Create("SMTP"), Name.Create("SMTP Server Setup"), Description.Create("Email gateway and server connection"), 2, actor);
                var subMenuSmtp = menuSettings.SubMenus.First(sm => sm.Code.GetValue() == "SMTP");
                subMenuSmtp.AddOption(Code.Create("TEST_SMTP"), Name.Create("Test SMTP Gateway"), Description.Create("Permission to trigger email delivery test"), ActionCode.Create("APPROVE"), 1, actor);
            }

            suites.Add(suite);
        }

        var wmsResult = SystemSuiteAggregate.Create(
            tenantId,
            Code.Create("WMS"),
            Name.Create("Warehouse Management"),
            Description.Create("Warehouse inventory management"),
            actor);

        if (wmsResult.IsSuccess)
        {
            var suite = wmsResult.Value;

            suite.RegisterAction(ActionCode.Create("INVENTORY_VIEW"), Name.Create("View Inventory"), actor);
            suite.RegisterAction(ActionCode.Create("INVENTORY_EDIT"), Name.Create("Edit Inventory"), actor);

            suite.AddAppSetting(
                ConfigurationKey.Create("AllowNegativeStock"),
                ConfigurationValue.Create("false"),
                ConfigurationScope.Global,
                actor);

            var modInv = suite.AddModule(Code.Create("INV"), Name.Create("Inventory Control"), Description.Create("Inventory management and levels"), 1, actor);
            if (modInv.IsSuccess)
            {
                var module = suite.Modules.First(m => m.Code.GetValue() == "INV");
                suite.ActivateModule(module.Id, actor);

                // Menu 1: Stock levels
                module.AddMenu(Code.Create("STOCK"), Name.Create("Stock Administration"), Description.Create("Stock levels and status"), 1, actor);
                var menuStock = module.Menus.First(m => m.Code.GetValue() == "STOCK");

                menuStock.AddSubMenu(Code.Create("LEVELS"), Name.Create("Real-time Levels"), Description.Create("Current physical stock status"), 1, actor);
                var subMenuLevels = menuStock.SubMenus.First(sm => sm.Code.GetValue() == "LEVELS");
                subMenuLevels.AddOption(Code.Create("VIEW_STOCK"), Name.Create("View Stock Levels"), Description.Create("Permission to view real-time inventory counts"), ActionCode.Create("INVENTORY_VIEW"), 1, actor);
                subMenuLevels.AddOption(Code.Create("ADJUST_STOCK"), Name.Create("Adjust Inventory Counts"), Description.Create("Permission to perform physical inventory adjustments"), ActionCode.Create("INVENTORY_EDIT"), 2, actor);

                // Menu 2: Operations
                module.AddMenu(Code.Create("OPS"), Name.Create("Warehouse Operations"), Description.Create("Stock movements and transfers"), 2, actor);
                var menuOps = module.Menus.First(m => m.Code.GetValue() == "OPS");

                menuOps.AddSubMenu(Code.Create("TRANSFERS"), Name.Create("Warehouse Transfers"), Description.Create("Move stock between physical locations"), 1, actor);
                var subMenuTransfers = menuOps.SubMenus.First(sm => sm.Code.GetValue() == "TRANSFERS");
                subMenuTransfers.AddOption(Code.Create("INITIATE_TRANSFER"), Name.Create("Initiate Stock Transfer"), Description.Create("Permission to draft and start a transfer request"), ActionCode.Create("INVENTORY_EDIT"), 1, actor);
                subMenuTransfers.AddOption(Code.Create("APPROVE_TRANSFER"), Name.Create("Approve Location Transfer"), Description.Create("Permission to authorize inventory relocation"), ActionCode.Create("INVENTORY_EDIT"), 2, actor);
            }

            suites.Add(suite);
        }

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
