namespace Ums.Infrastructure.Persistence.Seeders;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.Role;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.SystemSuite.DomainResource;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Reflection;
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

        var allTenantIds = new[]
        {
            TenantId.Load(Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId)),           // INTERNAL_ADMIN
            TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId)),                  // RANSA_PERU
            TenantId.Load(Guid.Parse("c9b736b4-6a84-48f8-b34d-176bc5a6d542")),           // NEPTUNIA
            TenantId.Load(Guid.Parse("a3f5b9d2-7c3d-4c8e-a9b0-123456789abc")),           // APM_CALLAO
            TenantId.Load(Guid.Parse("9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe")),           // PAITA_PORT
            TenantId.Load(Guid.Parse("5f4e3d2c-1b0a-9f8e-7d6c-543210987654")),           // UNIMAR
            TenantId.Load(Guid.Parse("f3e2d1c0-b9a8-7f6e-5d4c-321098765432")),           // INTRADEVCO
        };

        foreach (var tenantId in allTenantIds)
        {
            // Seed SystemSuites
            var suites = BuildSeedSystemSuites(tenantId, actor);
            if (inMemorySuiteRepository is not null)
            {
                foreach (var suite in suites) inMemorySuiteRepository.Seed(suite);
            }
            else if (suiteRepository is not null)
            {
                var existing = await suiteRepository.GetByTenantIdAsync(tenantId.GetValue(), cancellationToken);
                if (existing.Count == 0)
                {
                    foreach (var suite in suites) await suiteRepository.AddAsync(suite, cancellationToken);
                    await suiteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }
                else
                {
                    await EnsureDomainResourcesAsync(existing, tenantId, actor, suiteRepository, cancellationToken);
                    suites = existing; // use persisted suites so IDs match for roles/templates
                }
            }

            // Seed Roles
            var roles = BuildSeedRoles(tenantId, suites, actor);
            if (inMemoryRoleRepository is not null)
            {
                foreach (var role in roles) inMemoryRoleRepository.Seed(role);
            }
            else if (roleRepository is not null)
            {
                var existing = await roleRepository.GetByTenantIdAsync(tenantId.GetValue(), cancellationToken);
                if (existing.Count == 0)
                {
                    foreach (var role in roles) await roleRepository.AddAsync(role, cancellationToken);
                    await roleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }
                else
                {
                    roles = existing;
                }
            }

            // Seed PermissionTemplates
            var templates = BuildSeedPermissionTemplates(tenantId, suites, roles, actor);
            if (inMemoryTemplateRepository is not null)
            {
                foreach (var template in templates) inMemoryTemplateRepository.Seed(template);
            }
            else if (templateRepository is not null)
            {
                var existing = await templateRepository.GetByTenantIdAsync(tenantId.GetValue(), cancellationToken);
                if (existing.Count == 0)
                {
                    foreach (var template in templates) await templateRepository.AddAsync(template, cancellationToken);
                    await templateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }
                else
                {
                    templates = existing;
                }
            }

            // Seed Profiles
            var profiles = BuildSeedProfiles(tenantId, roles, templates, actor);
            if (inMemoryProfileRepository is not null)
            {
                foreach (var profile in profiles) inMemoryProfileRepository.Seed(profile);
            }
            else if (profileRepository is not null)
            {
                var existing = await profileRepository.GetByTenantIdAsync(tenantId.GetValue(), cancellationToken);
                if (existing.Count == 0)
                {
                    foreach (var profile in profiles) await profileRepository.AddAsync(profile, cancellationToken);
                    await profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }
            }
        }
    }

    private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private static void SetRoleId(RoleAggregate role, Guid id)
    {
        var propsField = typeof(RoleAggregate).GetField("_props", PrivateInstanceFlags);
        var props = propsField?.GetValue(role) as RoleProps;
        var idProperty = typeof(RoleProps).GetProperty("Id", PrivateInstanceFlags);
        idProperty?.SetValue(props, RoleId.Load(id));
    }

    private static IReadOnlyList<RoleAggregate> BuildSeedRoles(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, ActorId actor)
    {
        var roles = new List<RoleAggregate>();
        if (suites.Count == 0) return roles;

        // ── UMS Suite Roles ──────────────────────────────────────────────────────
        var adminRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("ADMIN"), Name.Create("System Administrator"), Description.Create("Full administrative access"), null, 0, 0, actor);
        if (adminRoleResult.IsSuccess)
        {
            var role = adminRoleResult.Value;
            if (tenantId.GetValue() == Guid.Parse(CoreDevDataSeeder.RansaTenantId))
                SetRoleId(role, Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId));
            roles.Add(role);
        }

        var supervisorRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("SUPERVISOR"), Name.Create("Core Supervisor"), Description.Create("Supervises core operations"), null, 0, 0, actor);
        if (supervisorRoleResult.IsSuccess) roles.Add(supervisorRoleResult.Value);

        var auditorRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("AUDITOR"), Name.Create("Compliance Auditor"), Description.Create("Read-only access for audits"), null, 0, 0, actor);
        if (auditorRoleResult.IsSuccess) roles.Add(auditorRoleResult.Value);

        var readonlyRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("READONLY"), Name.Create("Read Only Viewer"), Description.Create("View-only access, no modifications"), null, 0, 0, actor);
        if (readonlyRoleResult.IsSuccess) roles.Add(readonlyRoleResult.Value);

        var dataEntryRoleResult = RoleAggregate.Create(tenantId, suites[0].GetId(), Code.Create("DATA_ENTRY"), Name.Create("Data Entry Clerk"), Description.Create("Create and update records only"), null, 0, 0, actor);
        if (dataEntryRoleResult.IsSuccess) roles.Add(dataEntryRoleResult.Value);

        // ── WMS Suite Roles ──────────────────────────────────────────────
        if (suites.Count > 1)
        {
            var operatorRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("OPERATOR"), Name.Create("Warehouse Operator"), Description.Create("Standard warehouse operations"), null, 0, 0, actor);
            if (operatorRoleResult.IsSuccess)
            {
                var role = operatorRoleResult.Value;
                if (tenantId.GetValue() == Guid.Parse(CoreDevDataSeeder.RansaTenantId))
                    SetRoleId(role, Guid.Parse(CoreDevDataSeeder.DemoOperatorRoleId));
                roles.Add(role);
            }

            var inspectorRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("INSPECTOR"), Name.Create("Quality Inspector"), Description.Create("Quality control and inspections"), null, 0, 0, actor);
            if (inspectorRoleResult.IsSuccess) roles.Add(inspectorRoleResult.Value);
            
            var managerRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("WMS_MANAGER"), Name.Create("Warehouse Manager"), Description.Create("Manages all warehouse operations"), null, 0, 0, actor);
            if (managerRoleResult.IsSuccess) roles.Add(managerRoleResult.Value);

            var dispatcherRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("DISPATCHER"), Name.Create("Dispatch Coordinator"), Description.Create("Manages stock transfers and dispatches"), null, 0, 0, actor);
            if (dispatcherRoleResult.IsSuccess) roles.Add(dispatcherRoleResult.Value);

            var reporterRoleResult = RoleAggregate.Create(tenantId, suites[1].GetId(), Code.Create("REPORTER"), Name.Create("Report Analyst"), Description.Create("Generates and exports warehouse reports"), null, 0, 0, actor);
            if (reporterRoleResult.IsSuccess) roles.Add(reporterRoleResult.Value);
        }

        return roles;
    }

    private static IReadOnlyList<SystemSuiteAggregate> BuildSeedSystemSuites(TenantId tenantId, ActorId actor)
    {
        var suites = new List<SystemSuiteAggregate>();

        var coreResult = SystemSuiteAggregate.Create(
            tenantId,
            Code.Create("UMS"),
            Name.Create("User Management System"),
            Description.Create("Core UMS functionality"),
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

                // GAP-3: Deactivate CONFIG module to test inactive module filter
                suite.DeactivateModule(module.Props.Id, actor);
            }

            // GAP-4: Add domain resources linked to modules
            var secMod = suite.Modules.First(m => m.Code.GetValue() == "SEC");
            var configMod = suite.Modules.First(m => m.Code.GetValue() == "CONFIG");
            suite.AddDomainResource(secMod.GetId(), null, DomainResourceType.Aggregate, Code.Create("USERS"), Name.Create("Users Aggregate"), Description.Create("User Management aggregate root"), actor);
            suite.AddDomainResource(secMod.GetId(), null, DomainResourceType.Aggregate, Code.Create("INVENTORY"), Name.Create("Inventory Aggregate"), Description.Create("Inventory Management aggregate root"), actor);
            suite.AddDomainResource(secMod.GetId(), null, DomainResourceType.Entity, Code.Create("AUDIT_LOG"), Name.Create("Audit Log Entity"), Description.Create("Audit log entity for tracking operations"), actor);
            suite.AddDomainResource(configMod.GetId(), null, DomainResourceType.Entity, Code.Create("STOCK_LEVEL"), Name.Create("Stock Level Entity"), Description.Create("Stock level entity linked to config module"), actor);

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
            suite.RegisterAction(ActionCode.Create("GENERATE_REPORT"), Name.Create("Generate Report"), actor);
            suite.RegisterAction(ActionCode.Create("EXPORT_DATA"), Name.Create("Export Data"), actor);
            suite.RegisterAction(ActionCode.Create("IMPORT_DATA"), Name.Create("Import Data"), actor);

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

            // GAP-7: Add Reports module to WMS
            var modReports = suite.AddModule(Code.Create("REPORTS"), Name.Create("Reports & Analytics"), Description.Create("Warehouse reporting and analytics"), 2, actor);
            if (modReports.IsSuccess)
            {
                var module = suite.Modules.First(m => m.Code.GetValue() == "REPORTS");
                suite.ActivateModule(module.Props.Id, actor);

                // Menu 1: Inventory Reports
                module.AddMenu(Code.Create("INV_REPORTS"), Name.Create("Inventory Reports"), Description.Create("Stock and inventory reports"), 1, actor);
                var menuInvReports = module.Menus.First(m => m.Code.GetValue() == "INV_REPORTS");

                menuInvReports.AddSubMenu(Code.Create("STOCK_SUMMARY"), Name.Create("Stock Summary"), Description.Create("Overall stock summary report"), 1, actor);
                var subMenuStockSummary = menuInvReports.SubMenus.First(sm => sm.Code.GetValue() == "STOCK_SUMMARY");
                subMenuStockSummary.AddOption(Code.Create("VIEW_STOCK_REPORT"), Name.Create("View Stock Report"), Description.Create("Permission to view stock summary"), ActionCode.Create("GENERATE_REPORT"), 1, actor);
                subMenuStockSummary.AddOption(Code.Create("EXPORT_STOCK"), Name.Create("Export Stock Data"), Description.Create("Permission to export stock data"), ActionCode.Create("EXPORT_DATA"), 2, actor);

                menuInvReports.AddSubMenu(Code.Create("MOVEMENT_REPORTS"), Name.Create("Movement Reports"), Description.Create("Stock movement history"), 2, actor);
                var subMenuMovement = menuInvReports.SubMenus.First(sm => sm.Code.GetValue() == "MOVEMENT_REPORTS");
                subMenuMovement.AddOption(Code.Create("VIEW_MOVEMENT"), Name.Create("View Movement Report"), Description.Create("Permission to view movement history"), ActionCode.Create("GENERATE_REPORT"), 1, actor);

                // Menu 2: Import/Export
                module.AddMenu(Code.Create("IO"), Name.Create("Import / Export"), Description.Create("Data import and export operations"), 2, actor);
                var menuIO = module.Menus.First(m => m.Code.GetValue() == "IO");

                menuIO.AddSubMenu(Code.Create("IMPORT"), Name.Create("Data Import"), Description.Create("Import inventory data from external sources"), 1, actor);
                var subMenuImport = menuIO.SubMenus.First(sm => sm.Code.GetValue() == "IMPORT");
                subMenuImport.AddOption(Code.Create("RUN_IMPORT"), Name.Create("Run Data Import"), Description.Create("Permission to execute data import"), ActionCode.Create("IMPORT_DATA"), 1, actor);
            }

            // Add domain resources for WMS
            var invMod = suite.Modules.First(m => m.Code.GetValue() == "INV");
            suite.AddDomainResource(invMod.GetId(), null, DomainResourceType.Aggregate, Code.Create("INVENTORY_WMS"), Name.Create("WMS Inventory Aggregate"), Description.Create("Warehouse Inventory Management"), actor);
            suite.AddDomainResource(invMod.GetId(), null, DomainResourceType.Entity, Code.Create("STOCK_MOVEMENT"), Name.Create("Stock Movement Entity"), Description.Create("Stock Movement Tracking"), actor);
            suite.AddDomainResource(invMod.GetId(), null, DomainResourceType.Entity, Code.Create("TRANSFER_ORDER"), Name.Create("Transfer Order Entity"), Description.Create("Warehouse Transfer Orders"), actor);

            suites.Add(suite);
        }

        return suites;
    }

    private static IReadOnlyList<PermissionTemplateAggregate> BuildSeedPermissionTemplates(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, IReadOnlyList<RoleAggregate> roles, ActorId actor)
    {
        var templates = new List<PermissionTemplateAggregate>();
        if (suites.Count == 0 || roles.Count == 0) return templates;

        var adminRole = roles.FirstOrDefault(r => r.Code.GetValue() == "ADMIN");
        var supervisorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "SUPERVISOR");
        var auditorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "AUDITOR");
        var readonlyRole = roles.FirstOrDefault(r => r.Code.GetValue() == "READONLY");
        var dataEntryRole = roles.FirstOrDefault(r => r.Code.GetValue() == "DATA_ENTRY");
        var operatorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "OPERATOR");
        var inspectorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "INSPECTOR");
        var managerRole = roles.FirstOrDefault(r => r.Code.GetValue() == "WMS_MANAGER");
        var dispatcherRole = roles.FirstOrDefault(r => r.Code.GetValue() == "DISPATCHER");
        var reporterRole = roles.FirstOrDefault(r => r.Code.GetValue() == "REPORTER");

        var coreSuite = suites[0];
        var wmsSuite = suites.Count > 1 ? suites[1] : null;

        // Helper to find a domain resource by code from the core suite
        var usersResource    = coreSuite.DomainResources.FirstOrDefault(r => r.Code.GetValue() == "USERS");
        var inventoryResource= coreSuite.DomainResources.FirstOrDefault(r => r.Code.GetValue() == "INVENTORY");
        var auditLogResource = coreSuite.DomainResources.FirstOrDefault(r => r.Code.GetValue() == "AUDIT_LOG");
        var stockLevelResource=coreSuite.DomainResources.FirstOrDefault(r => r.Code.GetValue() == "STOCK_LEVEL");

        // ── 1. ADMIN V2 — Published, full suite access ────────────────────
        if (adminRole != null)
        {
            var adminV2 = PermissionTemplateAggregate.Create(tenantId, adminRole.GetId(), coreSuite.GetId(), actor).Value;
            adminV2.Props.Version = TemplateVersion.Create(2, 0, 0);
            // Navigation: full suite
            adminV2.AddItem(ExclusiveArcTarget.SystemSuite, coreSuite.GetId(), ActionId.Create(), true, false, actor);
            foreach (var mod in coreSuite.Modules)
            {
                adminV2.AddItem(ExclusiveArcTarget.Module, mod.Props.Id, ActionId.Create(), true, false, actor);
                foreach (var menu in mod.Menus)
                {
                    adminV2.AddItem(ExclusiveArcTarget.Submodule, menu.Props.Id, ActionId.Create(), true, false, actor);
                    foreach (var subMenu in menu.SubMenus)
                    {
                        adminV2.AddItem(ExclusiveArcTarget.Option, subMenu.Props.Id, ActionId.Create(), true, false, actor);
                        foreach (var opt in subMenu.Options)
                        {
                            adminV2.AddItem(ExclusiveArcTarget.Option, opt.Props.Id, ActionId.Create(), true, false, actor);
                        }
                    }
                }
            }
            // Domain resources: full access
            if (usersResource != null)
            {
                adminV2.AddItem(ExclusiveArcTarget.Aggregate, usersResource.Id, ActionId.Create(), true, false, actor);
            }
            if (inventoryResource != null)
            {
                adminV2.AddItem(ExclusiveArcTarget.Aggregate, inventoryResource.Id, ActionId.Create(), true, false, actor);
            }
            if (auditLogResource != null)
            {
                adminV2.AddItem(ExclusiveArcTarget.Entity, auditLogResource.Id, ActionId.Create(), true, false, actor);
            }
            if (stockLevelResource != null)
            {
                adminV2.AddItem(ExclusiveArcTarget.Entity, stockLevelResource.Id, ActionId.Create(), true, false, actor);
            }
            adminV2.Publish(actor);
            templates.Add(adminV2);
        }

        // ── 2. SUPERVISOR — Published, module-level access ────────────────
        if (supervisorRole != null)
        {
            var supervisorTpl = PermissionTemplateAggregate.Create(tenantId, supervisorRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            // Navigation: security module
            supervisorTpl.AddItem(ExclusiveArcTarget.Module, secMod.Props.Id, ActionId.Create(), true, false, actor);
            // Domain resources: read users + audit
            if (usersResource != null)
                supervisorTpl.AddItem(ExclusiveArcTarget.Aggregate, usersResource.Id, ActionId.Create(), true, false, actor);
            if (auditLogResource != null)
                supervisorTpl.AddItem(ExclusiveArcTarget.Entity, auditLogResource.Id, ActionId.Create(), true, false, actor);
            supervisorTpl.Publish(actor);
            templates.Add(supervisorTpl);
        }

        // ── 3. AUDITOR — Published, read-only on audit logs ───────────────
        if (auditorRole != null)
        {
            var auditorTpl = PermissionTemplateAggregate.Create(tenantId, auditorRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            var auditMenu = secMod.Menus.First(m => m.Code.GetValue() == "AUDIT");
            var logsSubMenu = auditMenu.SubMenus.First(sm => sm.Code.GetValue() == "LOGS");
            var viewLogsOpt = logsSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_LOGS");
            // Navigation: audit trail options only
            auditorTpl.AddItem(ExclusiveArcTarget.Submodule, auditMenu.Props.Id, ActionId.Create(), true, false, actor);
            auditorTpl.AddItem(ExclusiveArcTarget.Option, logsSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            auditorTpl.AddItem(ExclusiveArcTarget.Option, viewLogsOpt.Props.Id, ActionId.Create(), true, false, actor);
            // Domain resources: read-only audit log entity
            if (auditLogResource != null)
                auditorTpl.AddItem(ExclusiveArcTarget.Entity, auditLogResource.Id, ActionId.Create(), true, false, actor);
            auditorTpl.Publish(actor);
            templates.Add(auditorTpl);
        }

        // ── 4. READONLY — Published, view-only on users list ──────────────
        if (readonlyRole != null)
        {
            var readonlyTpl = PermissionTemplateAggregate.Create(tenantId, readonlyRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            var usersMenu = secMod.Menus.First(m => m.Code.GetValue() == "USERS");
            var listSubMenu = usersMenu.SubMenus.First(sm => sm.Code.GetValue() == "LIST");
            var viewUsersOpt = listSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_USERS");
            // Navigation: view users option only
            readonlyTpl.AddItem(ExclusiveArcTarget.Option, viewUsersOpt.Props.Id, ActionId.Create(), true, false, actor);
            // Domain resources: read-only users aggregate
            if (usersResource != null)
                readonlyTpl.AddItem(ExclusiveArcTarget.Aggregate, usersResource.Id, ActionId.Create(), true, false, actor);
            readonlyTpl.Publish(actor);
            templates.Add(readonlyTpl);
        }

        // ── 5. DATA_ENTRY — Published, create/update on users ─────────────
        if (dataEntryRole != null)
        {
            var dataEntryTpl = PermissionTemplateAggregate.Create(tenantId, dataEntryRole.GetId(), coreSuite.GetId(), actor).Value;
            var secMod = coreSuite.Modules.First(m => m.Code.GetValue() == "SEC");
            var usersMenu = secMod.Menus.First(m => m.Code.GetValue() == "USERS");
            var listSubMenu = usersMenu.SubMenus.First(sm => sm.Code.GetValue() == "LIST");
            var editUsersOpt = listSubMenu.Options.First(o => o.Code.GetValue() == "EDIT_USERS");
            // Navigation: edit users options
            dataEntryTpl.AddItem(ExclusiveArcTarget.Option, listSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            dataEntryTpl.AddItem(ExclusiveArcTarget.Option, editUsersOpt.Props.Id, ActionId.Create(), true, false, actor);
            // Domain resources: create + update users aggregate
            if (usersResource != null)
                dataEntryTpl.AddItem(ExclusiveArcTarget.Aggregate, usersResource.Id, ActionId.Create(), true, false, actor);
            dataEntryTpl.Publish(actor);
            templates.Add(dataEntryTpl);
        }

        // ── 6. OPERATOR — Published, stock view + deny adjust ─────────────
        if (operatorRole != null && wmsSuite != null)
        {
            var operatorTpl = PermissionTemplateAggregate.Create(tenantId, operatorRole.GetId(), wmsSuite.GetId(), actor).Value;
            var invMod = wmsSuite.Modules.First(m => m.Code.GetValue() == "INV");
            var stockMenu = invMod.Menus.First(m => m.Code.GetValue() == "STOCK");
            var levelsSubMenu = stockMenu.SubMenus.First(sm => sm.Code.GetValue() == "LEVELS");
            var viewStockOpt = levelsSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_STOCK");
            var adjustStockOpt = levelsSubMenu.Options.First(o => o.Code.GetValue() == "ADJUST_STOCK");
            var invWms = wmsSuite.DomainResources.First(x => x.Code.GetValue() == "INVENTORY_WMS");

            operatorTpl.AddItem(ExclusiveArcTarget.Module, invMod.Props.Id, ActionId.Create(), true, false, actor);
            operatorTpl.AddItem(ExclusiveArcTarget.Option, levelsSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            operatorTpl.AddItem(ExclusiveArcTarget.Option, viewStockOpt.Props.Id, ActionId.Create(), true, false, actor);
            operatorTpl.AddItem(ExclusiveArcTarget.Option, adjustStockOpt.Props.Id, ActionId.Create(), false, true, actor);
            operatorTpl.AddItem(ExclusiveArcTarget.Aggregate, invWms.Id, ActionId.Create(), true, false, actor);
            operatorTpl.Publish(actor);
            templates.Add(operatorTpl);
        }

        // ── 7. INSPECTOR — Published, quality control with deny rules ─────
        if (inspectorRole != null && wmsSuite != null)
        {
            var inspectorTpl = PermissionTemplateAggregate.Create(tenantId, inspectorRole.GetId(), wmsSuite.GetId(), actor).Value;
            var invMod = wmsSuite.Modules.First(m => m.Code.GetValue() == "INV");
            var stockMenu = invMod.Menus.First(m => m.Code.GetValue() == "STOCK");
            var levelsSubMenu = stockMenu.SubMenus.First(sm => sm.Code.GetValue() == "LEVELS");
            var viewStockOpt = levelsSubMenu.Options.First(o => o.Code.GetValue() == "VIEW_STOCK");

            inspectorTpl.AddItem(ExclusiveArcTarget.Module, invMod.Props.Id, ActionId.Create(), true, false, actor);
            inspectorTpl.AddItem(ExclusiveArcTarget.Option, levelsSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            inspectorTpl.AddItem(ExclusiveArcTarget.Option, viewStockOpt.Props.Id, ActionId.Create(), true, false, actor);
            inspectorTpl.Publish(actor);
            templates.Add(inspectorTpl);
        }

        // ── 8. WMS_MANAGER — Published, full WMS access ───────────────────
        if (managerRole != null && wmsSuite != null)
        {
            var managerTpl = PermissionTemplateAggregate.Create(tenantId, managerRole.GetId(), wmsSuite.GetId(), actor).Value;
            managerTpl.AddItem(ExclusiveArcTarget.SystemSuite, wmsSuite.GetId(), ActionId.Create(), true, false, actor);
            foreach (var mod in wmsSuite.Modules)
            {
                managerTpl.AddItem(ExclusiveArcTarget.Module, mod.Props.Id, ActionId.Create(), true, false, actor);
                foreach (var menu in mod.Menus)
                {
                    managerTpl.AddItem(ExclusiveArcTarget.Submodule, menu.Props.Id, ActionId.Create(), true, false, actor);
                    foreach (var subMenu in menu.SubMenus)
                    {
                        managerTpl.AddItem(ExclusiveArcTarget.Option, subMenu.Props.Id, ActionId.Create(), true, false, actor);
                        foreach (var opt in subMenu.Options)
                        {
                            managerTpl.AddItem(ExclusiveArcTarget.Option, opt.Props.Id, ActionId.Create(), true, false, actor);
                        }
                    }
                }
            }
            managerTpl.Publish(actor);
            templates.Add(managerTpl);
        }

        // ── 9. DISPATCHER — Published, transfers only ─────────────────────
        if (dispatcherRole != null && wmsSuite != null)
        {
            var dispatcherTpl = PermissionTemplateAggregate.Create(tenantId, dispatcherRole.GetId(), wmsSuite.GetId(), actor).Value;
            var invMod = wmsSuite.Modules.First(m => m.Code.GetValue() == "INV");
            var opsMenu = invMod.Menus.First(m => m.Code.GetValue() == "OPS");
            var transfersSubMenu = opsMenu.SubMenus.First(sm => sm.Code.GetValue() == "TRANSFERS");
            var initiateOpt = transfersSubMenu.Options.First(o => o.Code.GetValue() == "INITIATE_TRANSFER");
            var approveOpt = transfersSubMenu.Options.First(o => o.Code.GetValue() == "APPROVE_TRANSFER");
            var transferOrder = wmsSuite.DomainResources.First(x => x.Code.GetValue() == "TRANSFER_ORDER");

            dispatcherTpl.AddItem(ExclusiveArcTarget.Submodule, opsMenu.Props.Id, ActionId.Create(), true, false, actor);
            dispatcherTpl.AddItem(ExclusiveArcTarget.Option, transfersSubMenu.Props.Id, ActionId.Create(), true, false, actor);
            dispatcherTpl.AddItem(ExclusiveArcTarget.Option, initiateOpt.Props.Id, ActionId.Create(), true, false, actor);
            dispatcherTpl.AddItem(ExclusiveArcTarget.Option, approveOpt.Props.Id, ActionId.Create(), true, false, actor);
            dispatcherTpl.AddItem(ExclusiveArcTarget.Entity, transferOrder.Id, ActionId.Create(), true, false, actor);
            dispatcherTpl.Publish(actor);
            templates.Add(dispatcherTpl);
        }

        // ── 10. REPORTER — Published, reports module only ─────────────────
        if (reporterRole != null && wmsSuite != null)
        {
            var reporterTpl = PermissionTemplateAggregate.Create(tenantId, reporterRole.GetId(), wmsSuite.GetId(), actor).Value;
            var reportsMod = wmsSuite.Modules.First(m => m.Code.GetValue() == "REPORTS");
            var invReportsMenu = reportsMod.Menus.First(m => m.Code.GetValue() == "INV_REPORTS");
            var stockSummarySubMenu = invReportsMenu.SubMenus.First(sm => sm.Code.GetValue() == "STOCK_SUMMARY");
            var viewReportOpt = stockSummarySubMenu.Options.First(o => o.Code.GetValue() == "VIEW_STOCK_REPORT");
            var exportOpt = stockSummarySubMenu.Options.First(o => o.Code.GetValue() == "EXPORT_STOCK");

            reporterTpl.AddItem(ExclusiveArcTarget.Module, reportsMod.Props.Id, ActionId.Create(), true, false, actor);
            reporterTpl.AddItem(ExclusiveArcTarget.Option, stockSummarySubMenu.Props.Id, ActionId.Create(), true, false, actor);
            reporterTpl.AddItem(ExclusiveArcTarget.Option, viewReportOpt.Props.Id, ActionId.Create(), true, false, actor);
            reporterTpl.AddItem(ExclusiveArcTarget.Option, exportOpt.Props.Id, ActionId.Create(), true, false, actor);
            reporterTpl.Publish(actor);
            templates.Add(reporterTpl);
        }

        return templates;
    }

    private static IReadOnlyList<ProfileAggregate> BuildSeedProfiles(TenantId tenantId, IReadOnlyList<RoleAggregate> roles, IReadOnlyList<PermissionTemplateAggregate> templates, ActorId actor)
    {
        var profiles = new List<ProfileAggregate>();

        var baseBytes = tenantId.GetValue().ToByteArray();
        Guid UserGuid(byte idx)
        {
            var b = (byte[])baseBytes.Clone();
            b[0] = idx;
            return new Guid(b);
        }

        var adminRole = roles.FirstOrDefault(r => r.Code.GetValue() == "ADMIN");
        var supervisorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "SUPERVISOR");
        var auditorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "AUDITOR");
        var readonlyRole = roles.FirstOrDefault(r => r.Code.GetValue() == "READONLY");
        var dataEntryRole = roles.FirstOrDefault(r => r.Code.GetValue() == "DATA_ENTRY");
        var operatorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "OPERATOR");
        var inspectorRole = roles.FirstOrDefault(r => r.Code.GetValue() == "INSPECTOR");
        var managerRole = roles.FirstOrDefault(r => r.Code.GetValue() == "WMS_MANAGER");
        var dispatcherRole = roles.FirstOrDefault(r => r.Code.GetValue() == "DISPATCHER");
        var reporterRole = roles.FirstOrDefault(r => r.Code.GetValue() == "REPORTER");

        var adminTpl = templates.FirstOrDefault(t => t.RoleId.Equals(adminRole?.GetId()) && t.Props.Version.GetValue() == "2.0.0" && t.Status == TemplateStatus.Published);
        var supervisorTpl = templates.FirstOrDefault(t => t.RoleId.Equals(supervisorRole?.GetId()) && t.Status == TemplateStatus.Published);
        var auditorTpl = templates.FirstOrDefault(t => t.RoleId.Equals(auditorRole?.GetId()) && t.Status == TemplateStatus.Published);
        var readonlyTpl = templates.FirstOrDefault(t => t.RoleId.Equals(readonlyRole?.GetId()) && t.Status == TemplateStatus.Published);
        var dataEntryTpl = templates.FirstOrDefault(t => t.RoleId.Equals(dataEntryRole?.GetId()) && t.Status == TemplateStatus.Published);
        var operatorTpl = templates.FirstOrDefault(t => t.RoleId.Equals(operatorRole?.GetId()) && t.Status == TemplateStatus.Published);
        var inspectorTpl = templates.FirstOrDefault(t => t.RoleId.Equals(inspectorRole?.GetId()) && t.Status == TemplateStatus.Published);
        var managerTpl = templates.FirstOrDefault(t => t.RoleId.Equals(managerRole?.GetId()) && t.Status == TemplateStatus.Published);
        var dispatcherTpl = templates.FirstOrDefault(t => t.RoleId.Equals(dispatcherRole?.GetId()) && t.Status == TemplateStatus.Published);
        var reporterTpl = templates.FirstOrDefault(t => t.RoleId.Equals(reporterRole?.GetId()) && t.Status == TemplateStatus.Published);

        void AddProfile(Guid userId, RoleAggregate? role, PermissionTemplateAggregate? tpl)
        {
            if (role == null) return;
            var p = ProfileAggregate.Create(tenantId, UserId.Load(userId), role.GetId(), null, actor);
            if (p.IsSuccess)
            {
                if (tpl != null) p.Value.AssignTemplate(tpl, actor);
                profiles.Add(p.Value);
            }
        }

        // SuperAdmin (INTERNAL_ADMIN tenant only) — fixed UserId 22222222-...
        var internalAdminTenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId);
        if (tenantId.GetValue() == internalAdminTenantId)
            AddProfile(Guid.Parse(CoreDevDataSeeder.SuperAdminUserId), adminRole, adminTpl);

        // 1. Admin — full suite access (ADMIN V2)
        AddProfile(UserGuid(1), adminRole, adminTpl);

        // 2. Supervisor — module-level access to Security
        AddProfile(UserGuid(6), supervisorRole, supervisorTpl);

        // 3. Auditor — read-only on audit logs
        AddProfile(UserGuid(11), auditorRole, auditorTpl);

        // 4. Read Only — view users list only
        AddProfile(UserGuid(12), readonlyRole, readonlyTpl);

        // 5. Data Entry — create/update users
        AddProfile(UserGuid(2), dataEntryRole, dataEntryTpl);

        // 6. Warehouse Operator — stock view, deny adjust
        AddProfile(UserGuid(7), operatorRole, operatorTpl);

        // 7. Quality Inspector — stock view only
        AddProfile(UserGuid(9), inspectorRole, inspectorTpl);

        // 8. Warehouse Manager — full WMS access
        AddProfile(UserGuid(10), managerRole, managerTpl);

        // 9. Dispatch Coordinator — transfers only
        AddProfile(UserGuid(8), dispatcherRole, dispatcherTpl);

        // 10. Report Analyst — reports module only
        AddProfile(UserGuid(3), reporterRole, reporterTpl);

        return profiles;
    }

    private static async Task EnsureDomainResourcesAsync(
        IReadOnlyList<SystemSuiteAggregate> existingSuites,
        TenantId tenantId,
        ActorId actor,
        ISystemSuiteRepository repository,
        CancellationToken cancellationToken)
    {
        foreach (var suite in existingSuites)
        {
            var existingResources = suite.DomainResources;
            var codesToAdd = GetDomainResourceCodesForSuite(suite);

            foreach (var codeToAdd in codesToAdd)
            {
                if (!existingResources.Any(dr => dr.Code.GetValue() == codeToAdd.Code))
                {
                    suite.AddDomainResource(
                        null,
                        null,
                        codeToAdd.Type,
                        Code.Create(codeToAdd.Code),
                        Name.Create(codeToAdd.Name),
                        Description.Create(codeToAdd.Description),
                        actor);
                }
            }

            await repository.UpdateAsync(suite, cancellationToken);
        }

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static IReadOnlyList<(string Code, string Name, string Description, DomainResourceType Type)> GetDomainResourceCodesForSuite(SystemSuiteAggregate suite)
    {
        var suiteCode = suite.Code.GetValue();

        return suiteCode switch
        {
            "UMS" => new (string, string, string, DomainResourceType)[]
            {
                ("USERS", "Users Aggregate", "User Management", DomainResourceType.Aggregate),
                ("INVENTORY", "Inventory Aggregate", "Inventory Management", DomainResourceType.Aggregate),
                ("AUDIT_LOG", "Audit Log Entity", "Audit Logs", DomainResourceType.Entity),
                ("STOCK_LEVEL", "Stock Level Entity", "Stock Levels", DomainResourceType.Entity),
            },
            "WMS" => new (string, string, string, DomainResourceType)[]
            {
                ("INVENTORY_WMS", "WMS Inventory Aggregate", "Warehouse Inventory Management", DomainResourceType.Aggregate),
                ("STOCK_MOVEMENT", "Stock Movement Entity", "Stock Movement Tracking", DomainResourceType.Entity),
                ("TRANSFER_ORDER", "Transfer Order Entity", "Warehouse Transfer Orders", DomainResourceType.Entity),
            },
            _ => Array.Empty<(string, string, string, DomainResourceType)>(),
        };
    }
}
