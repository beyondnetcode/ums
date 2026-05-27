namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.SystemSuite.DomainResource;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Kernel.ValueObjects;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;
using RoleAggregate = Ums.Domain.Authorization.Role.Role;
using Ums.Domain.Enums;

public static class AuthorizationDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var suiteRepository = serviceProvider.GetService<ISystemSuiteRepository>();
        var inMemorySuiteRepository = serviceProvider.GetService<InMemorySystemSuiteRepository>();

        var roleRepository = serviceProvider.GetService<IRoleRepository>();
        var inMemoryRoleRepository = serviceProvider.GetService<InMemoryRoleRepository>();

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

        // Seed Roles
        var roles = BuildSeedRoles(ransaTenantId, suites, actor);
        if (inMemoryRoleRepository is not null)
        {
            foreach (var role in roles) inMemoryRoleRepository.Seed(role);
        }
        else if (roleRepository is not null)
        {
            var existing = await roleRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var role in roles) await roleRepository.AddAsync(role, cancellationToken);
                await roleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Seed PermissionTemplates
        var templates = BuildSeedPermissionTemplates(ransaTenantId, suites, roles, actor);
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

    private static IReadOnlyList<RoleAggregate> BuildSeedRoles(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, ActorId actor)
    {
        var roles = new List<RoleAggregate>();
        if (suites.Count == 0) return roles;

        // Core Suite Roles
        var adminRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("ADMIN"), Name.Create("System Administrator"), Description.Create("Full administrative access"), null, 0, 0, actor);
        if (adminRoleResult.IsSuccess)
        {
            var role = adminRoleResult.Value;
            role.Props.Id = RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId));
            roles.Add(role);
        }

        var supervisorRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("SUPERVISOR"), Name.Create("Core Supervisor"), Description.Create("Supervises core operations"), null, 0, 0, actor);
        if (supervisorRoleResult.IsSuccess) roles.Add(supervisorRoleResult.Value);

        var auditorRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("AUDITOR"), Name.Create("Compliance Auditor"), Description.Create("Read-only access for audits"), null, 0, 0, actor);
        if (auditorRoleResult.IsSuccess) roles.Add(auditorRoleResult.Value);

        // WMS Suite Roles
        if (suites.Count > 1)
        {
            var operatorRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("OPERATOR"), Name.Create("Warehouse Operator"), Description.Create("Standard warehouse operations"), null, 0, 0, actor);
            if (operatorRoleResult.IsSuccess)
            {
                var role = operatorRoleResult.Value;
                role.Props.Id = RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoOperatorRoleId));
                roles.Add(role);
            }

            var inspectorRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("INSPECTOR"), Name.Create("Quality Inspector"), Description.Create("Quality control and inspections"), null, 0, 0, actor);
            if (inspectorRoleResult.IsSuccess) roles.Add(inspectorRoleResult.Value);
            
            var managerRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("WMS_MANAGER"), Name.Create("Warehouse Manager"), Description.Create("Manages all warehouse operations"), null, 0, 0, actor);
            if (managerRoleResult.IsSuccess) roles.Add(managerRoleResult.Value);
        }

        return roles;
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

            // Register standard domain actions
            suite.RegisterAction(ActionCode.Create("CREATE"), Name.Create("Create Record"), actor);
            suite.RegisterAction(ActionCode.Create("READ"), Name.Create("Read Record"), actor);
            suite.RegisterAction(ActionCode.Create("UPDATE"), Name.Create("Update Record"), actor);
            suite.RegisterAction(ActionCode.Create("DELETE"), Name.Create("Delete Record"), actor);
            suite.RegisterAction(ActionCode.Create("SEARCH"), Name.Create("Search Records"), actor);

            // Add domain resources
            suite.AddDomainResource(null, DomainResourceType.Aggregate, Code.Create("USERS"), Name.Create("Users Aggregate"), Description.Create("User Management"), actor);
            suite.AddDomainResource(null, DomainResourceType.Aggregate, Code.Create("INVENTORY"), Name.Create("Inventory Aggregate"), Description.Create("Inventory Management"), actor);
            suite.AddDomainResource(null, DomainResourceType.Entity, Code.Create("AUDIT_LOG"), Name.Create("Audit Log Entity"), Description.Create("Audit Logs"), actor);
            suite.AddDomainResource(null, DomainResourceType.Entity, Code.Create("STOCK_LEVEL"), Name.Create("Stock Level Entity"), Description.Create("Stock Levels"), actor);

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
                suite.ActivateModule(module.Props.Id, actor);

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
                suite.ActivateModule(module.Props.Id, actor);

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
                suite.ActivateModule(module.Props.Id, actor);

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

    private static IReadOnlyList<PermissionTemplateAggregate> BuildSeedPermissionTemplates(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, IReadOnlyList<RoleAggregate> roles, ActorId actor)
    {
        var templates = new List<PermissionTemplateAggregate>();
        if (suites.Count == 0 || roles.Count == 0) return templates;

        // Helper to find roles by code
        var adminRole = roles.FirstOrDefault(r => r.Code.GetValue() == "ADMIN");
        var operatorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "OPERATOR");
        var supervisorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "SUPERVISOR");
        var auditorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "AUDITOR");
        var inspectorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "INSPECTOR");
        var managerRole = roles.FirstOrDefault(r => r.Code.GetValue() == "WMS_MANAGER");

        var coreSuite = suites[0];
        var wmsSuite = suites.Count > 1 ? suites[1] : null;

        // 1. ADMIN - Multiple versions and states
        if (adminRole != null)
        {
            // V1 - Deprecated
            var adminV1 = PermissionTemplateAggregate.Create(tenantId, adminRole.GetId(), coreSuite.GetId(), actor).Value;
            adminV1.Props.Version = TemplateVersion.Create(1, 0, 0);
            adminV1.Publish(actor);
            adminV1.Deprecate(actor);
            templates.Add(adminV1);

            // V2 - Published (Current active)
            var adminV2 = PermissionTemplateAggregate.Create(tenantId, adminRole.GetId(), coreSuite.GetId(), actor).Value;
            adminV2.Props.Version = TemplateVersion.Create(2, 0, 0);
            adminV2.AddItem(ExclusiveArcTarget.SystemSuite, coreSuite.GetId(), ActionId.Create(), true, false, actor);
            adminV2.Publish(actor);
            templates.Add(adminV2);

            // V3 - Draft (Massive Graph Test for UI)
            var adminV3 = PermissionTemplateAggregate.Create(tenantId, adminRole.GetId(), coreSuite.GetId(), actor).Value;
            adminV3.Props.Version = TemplateVersion.Create(3, 0, 0);

            // 1. Module Level
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            adminV3.AddItem(ExclusiveArcTarget.Module, secMod.Props.Id, ActionId.Create(), true, false, actor);

            // 2. Menu Level (Mapped to Submodule in TargetType)
            var usersMenu = secMod.Menus.First(m => m.Code.GetValue() == "USERS");
            adminV3.AddItem(ExclusiveArcTarget.Submodule, usersMenu.Props.Id, ActionId.Create(), true, false, actor);
            
            var auditMenu = secMod.Menus.First(m => m.Code.GetValue() == "AUDIT");
            adminV3.AddItem(ExclusiveArcTarget.Submodule, auditMenu.Props.Id, ActionId.Create(), false, true, actor); // Explicit Deny for Menu

            // 3. SubMenu Level
            var listSubMenu = usersMenu.SubMenus.First(sm => sm.Code.GetValue() == "LIST");
            adminV3.AddItem(ExclusiveArcTarget.Option, listSubMenu.Props.Id, ActionId.Create(), true, false, actor); // Allow

            var rolesSubMenu = usersMenu.SubMenus.First(sm => sm.Code.GetValue() == "ROLES");
            adminV3.AddItem(ExclusiveArcTarget.Option, rolesSubMenu.Props.Id, ActionId.Create(), false, true, actor); // Deny

            // 4. Option/Action Level
            var viewUsersOpt = listSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_USERS");
            adminV3.AddItem(ExclusiveArcTarget.Option, viewUsersOpt.Props.Id, ActionId.Create(), true, false, actor); // Allow Action

            var editUsersOpt = listSubMenu.Options.First(o => o.Code.GetValue() == "EDIT_USERS");
            adminV3.AddItem(ExclusiveArcTarget.Option, editUsersOpt.Props.Id, ActionId.Create(), false, true, actor); // Deny Action

            // Config Module (Mix of permissions)
            var configMod = coreSuite.Modules.First(m => m.Code.GetValue() == "CONFIG");
            adminV3.AddItem(ExclusiveArcTarget.Module, configMod.Props.Id, ActionId.Create(), true, false, actor);
            
            var settingsMenu = configMod.Menus.First(m => m.Code.GetValue() == "SETTINGS");
            var paramsSubMenu = settingsMenu.SubMenus.First(sm => sm.Code.GetValue() == "PARAMS");
            var viewParamsOpt = paramsSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_PARAMS");
            adminV3.AddItem(ExclusiveArcTarget.Option, viewParamsOpt.Props.Id, ActionId.Create(), true, false, actor);
            
            // Allow standard actions on global aggregates for Admin
            var usersAgg = coreSuite.DomainResources.First(x => x.Code.GetValue() == "USERS");
            var auditEnt = coreSuite.DomainResources.First(x => x.Code.GetValue() == "AUDIT_LOG");
            adminV3.AddItem(ExclusiveArcTarget.Aggregate, usersAgg.Id, ActionId.Create(), true, false, actor);
            adminV3.AddItem(ExclusiveArcTarget.Entity, auditEnt.Id, ActionId.Create(), true, false, actor);

            adminV3.Publish(actor);

            templates.Add(adminV3);
        }

        // 2. SUPERVISOR - Published
        if (supervisorRole != null)
        {
            var supervisorTpl = PermissionTemplateAggregate.Create(tenantId, supervisorRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            supervisorTpl.AddItem(ExclusiveArcTarget.Module, secMod.Props.Id, ActionId.Create(), true, false, actor);
            supervisorTpl.Publish(actor);
            templates.Add(supervisorTpl);
        }

        // 3. AUDITOR - Draft with multiple items
        if (auditorRole != null)
        {
            var auditorTpl = PermissionTemplateAggregate.Create(tenantId, auditorRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            var auditMenu = secMod.Menus.First(m => m.Code.GetValue() == "AUDIT");
            var logsSubMenu = auditMenu.SubMenus.First(sm => sm.Code.GetValue() == "LOGS");
            var viewLogsOpt = logsSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_LOGS");
            
            auditorTpl.AddItem(ExclusiveArcTarget.Submodule, auditMenu.Props.Id, ActionId.Create(), true, false, actor);
            auditorTpl.AddItem(ExclusiveArcTarget.Option, logsSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            auditorTpl.AddItem(ExclusiveArcTarget.Option, viewLogsOpt.Props.Id, ActionId.Create(), true, false, actor);
            
            templates.Add(auditorTpl);
        }

        // 4. OPERATOR - Draft (Empty, no items)
        if (operatorRole != null && wmsSuite != null)
        {
            var operatorTpl = PermissionTemplateAggregate.Create(tenantId, operatorRole.GetId(), wmsSuite.GetId(), actor).Value;
            templates.Add(operatorTpl);
        }

        // 5. INSPECTOR - Published with Deny rules
        if (inspectorRole != null && wmsSuite != null)
        {
            var inspectorTpl = PermissionTemplateAggregate.Create(tenantId, inspectorRole.GetId(), wmsSuite.GetId(), actor).Value;
            inspectorTpl.AddItem(ExclusiveArcTarget.Module, wmsSuite.Modules.First().Props.Id, ActionId.Create(), true, false, actor);
            inspectorTpl.AddItem(ExclusiveArcTarget.Option, wmsSuite.Modules.First().Menus.First().SubMenus.First().Props.Id, ActionId.Create(), false, true, actor); // Deny rule
            inspectorTpl.Publish(actor);
            templates.Add(inspectorTpl);
        }

        // 6. WMS MANAGER - Deprecated
        if (managerRole != null && wmsSuite != null)
        {
            var managerTpl = PermissionTemplateAggregate.Create(tenantId, managerRole.GetId(), wmsSuite.GetId(), actor).Value;
            managerTpl.AddItem(ExclusiveArcTarget.SystemSuite, wmsSuite.GetId(), ActionId.Create(), true, false, actor);
            managerTpl.Publish(actor);
            managerTpl.Deprecate(actor);
            templates.Add(managerTpl);
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
