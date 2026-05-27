namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Enums;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;
using ModuleEntity = Ums.Domain.Authorization.SystemSuite.Module.Module;
using MenuEntity = Ums.Domain.Authorization.SystemSuite.Menu.Menu;
using SubMenuEntity = Ums.Domain.Authorization.SystemSuite.SubMenu.SubMenu;
using OptionEntity = Ums.Domain.Authorization.SystemSuite.Option.Option;
using ActionEntity = Ums.Domain.Authorization.SystemSuite.Action.Action;

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

        var profiles = BuildSeedProfiles(ransaTenantId, suites, actor);
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
            Description.Create("Core operations, identity, and access management"),
            actor);

        var wmsResult = SystemSuiteAggregate.Create(
            tenantId,
            Code.Create("WMS"),
            Name.Create("Warehouse Management"),
            Description.Create("Warehouse inventory and promotions management"),
            actor);

        if (coreResult.IsSuccess)
        {
            var core = coreResult.Value;
            PopulateLogisticsCoreSuite(core, tenantId, actor);
            suites.Add(core);
        }

        if (wmsResult.IsSuccess)
        {
            var wms = wmsResult.Value;
            PopulateWmsSuite(wms, tenantId, actor);
            suites.Add(wms);
        }

        return suites;
    }

    private static void PopulateLogisticsCoreSuite(SystemSuiteAggregate suite, TenantId tenantId, ActorId actor)
    {
        RegisterSuiteActions(suite, actor);

        var identityModule = AddModule(suite, "IDENTITY", "Identity & Access", "Tenant, branch, brand, and user management", 1, actor);
        var authModule = AddModule(suite, "AUTH", "Authorization", "Profiles, roles, permissions, and templates", 2, actor);
        var configModule = AddModule(suite, "CONFIG", "Configuration", "Feature flags and application settings", 3, actor);
        var approvalModule = AddModule(suite, "APPROVALS", "Approvals", "Approval requests and workflows", 4, actor);

        PopulateIdentityModule(suite, identityModule, actor);
        PopulateAuthModule(suite, authModule, actor);
        PopulateConfigModule(suite, configModule, actor);
        PopulateApprovalModule(suite, approvalModule, actor);
    }

    private static void PopulateWmsSuite(SystemSuiteAggregate suite, TenantId tenantId, ActorId actor)
    {
        RegisterWmsActions(suite, actor);

        var inventoryModule = AddModule(suite, "INVENTORY", "Inventory", "Stock, movements, and warehouse operations", 1, actor);
        var promotionModule = AddModule(suite, "PROMOTIONS", "Promotions", "Promotion campaigns and rules", 2, actor);

        PopulateInventoryModule(suite, inventoryModule, actor);
        PopulatePromotionModule(suite, promotionModule, actor);
    }

    private static void RegisterSuiteActions(SystemSuiteAggregate suite, ActorId actor)
    {
        var crudActions = new (string code, string name)[]
        {
            ("CREATE", "Create"),
            ("READ", "Read"),
            ("SEARCH", "Search"),
            ("UPDATE", "Update"),
            ("DELETE", "Delete"),
            ("DEACTIVATE", "Deactivate"),
            ("ACTIVATE", "Activate"),
            ("EXPORT", "Export"),
            ("IMPORT", "Import"),
            ("ASSIGN", "Assign"),
            ("REVOKE", "Revoke"),
            ("APPROVE", "Approve"),
            ("REJECT", "Reject"),
            ("PUBLISH", "Publish"),
            ("DEPRECATE", "Deprecate"),
        };

        foreach (var (code, name) in crudActions)
        {
            suite.RegisterAction(ActionCode.Create(code), Name.Create(name), actor);
        }
    }

    private static void RegisterWmsActions(SystemSuiteAggregate suite, ActorId actor)
    {
        var crudActions = new (string code, string name)[]
        {
            ("CREATE", "Create"),
            ("READ", "Read"),
            ("SEARCH", "Search"),
            ("UPDATE", "Update"),
            ("DELETE", "Delete"),
            ("DEACTIVATE", "Deactivate"),
            ("ACTIVATE", "Activate"),
            ("EXPORT", "Export"),
            ("IMPORT", "Import"),
            ("ADJUST_STOCK", "Adjust Stock"),
            ("TRANSFER_STOCK", "Transfer Stock"),
            ("COUNT_INVENTORY", "Count Inventory"),
            ("APPLY_PROMOTION", "Apply Promotion"),
            ("SCHEDULE_PROMOTION", "Schedule Promotion"),
            ("CANCEL_PROMOTION", "Cancel Promotion"),
        };

        foreach (var (code, name) in crudActions)
        {
            suite.RegisterAction(ActionCode.Create(code), Name.Create(name), actor);
        }
    }

    private static ModuleEntity AddModule(SystemSuiteAggregate suite, string code, string name, string description, int sortOrder, ActorId actor)
    {
        suite.AddModule(Code.Create(code), Name.Create(name), Description.Create(description), sortOrder, actor);
        return suite.Modules.First(m => m.Code.GetValue() == code);
    }

    private static void PopulateIdentityModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var tenantMenu = AddMenu(suite, module, "TENANTS", "Tenants", "Multi-tenant configuration", 1, actor);
        var tenantSub = AddSubMenu(suite, tenantMenu, "TENANT_LIST", "Tenant List", "View and manage tenants", 1, actor);
        AddOption(suite, tenantSub, "TENANT_VIEW", "View Tenants", "List and view tenant details", "READ", 1, actor);
        AddOption(suite, tenantSub, "TENANT_CREATE", "Create Tenant", "Register new tenant", "CREATE", 2, actor);
        AddOption(suite, tenantSub, "TENANT_EDIT", "Edit Tenant", "Modify tenant configuration", "UPDATE", 3, actor);
        AddOption(suite, tenantSub, "TENANT_DEACTIVATE", "Deactivate Tenant", "Disable tenant access", "DEACTIVATE", 4, actor);

        var branchMenu = AddMenu(suite, module, "BRANCHES", "Branches", "Branch/office management", 2, actor);
        var branchSub = AddSubMenu(suite, branchMenu, "BRANCH_LIST", "Branch List", "View and manage branches", 1, actor);
        AddOption(suite, branchSub, "BRANCH_VIEW", "View Branches", "List and view branch details", "READ", 1, actor);
        AddOption(suite, branchSub, "BRANCH_CREATE", "Create Branch", "Register new branch", "CREATE", 2, actor);
        AddOption(suite, branchSub, "BRANCH_EDIT", "Edit Branch", "Modify branch details", "UPDATE", 3, actor);
        AddOption(suite, branchSub, "BRANCH_DELETE", "Delete Branch", "Remove branch", "DELETE", 4, actor);

        var brandMenu = AddMenu(suite, module, "BRANDS", "Brands", "Brand management", 3, actor);
        var brandSub = AddSubMenu(suite, brandMenu, "BRAND_LIST", "Brand List", "View and manage brands", 1, actor);
        AddOption(suite, brandSub, "BRAND_VIEW", "View Brands", "List and view brand details", "READ", 1, actor);
        AddOption(suite, brandSub, "BRAND_CREATE", "Create Brand", "Register new brand", "CREATE", 2, actor);
        AddOption(suite, brandSub, "BRAND_EDIT", "Edit Brand", "Modify brand details", "UPDATE", 3, actor);

        var userMenu = AddMenu(suite, module, "USERS", "User Accounts", "User account management", 4, actor);
        var userSub = AddSubMenu(suite, userMenu, "USER_LIST", "User List", "View and manage user accounts", 1, actor);
        AddOption(suite, userSub, "USER_VIEW", "View Users", "List and view user accounts", "READ", 1, actor);
        AddOption(suite, userSub, "USER_CREATE", "Create User", "Register new user account", "CREATE", 2, actor);
        AddOption(suite, userSub, "USER_EDIT", "Edit User", "Modify user account", "UPDATE", 3, actor);
        AddOption(suite, userSub, "USER_DEACTIVATE", "Deactivate User", "Disable user account", "DEACTIVATE", 4, actor);
        AddOption(suite, userSub, "USER_ASSIGN_ROLE", "Assign Role", "Assign role to user", "ASSIGN", 5, actor);
        AddOption(suite, userSub, "USER_REVOKE_ROLE", "Revoke Role", "Remove role from user", "REVOKE", 6, actor);
    }

    private static void PopulateAuthModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var profileMenu = AddMenu(suite, module, "PROFILES", "Profiles", "User profile management", 1, actor);
        var profileSub = AddSubMenu(suite, profileMenu, "PROFILE_LIST", "Profile List", "View and manage profiles", 1, actor);
        AddOption(suite, profileSub, "PROFILE_VIEW", "View Profiles", "List and view profiles", "READ", 1, actor);
        AddOption(suite, profileSub, "PROFILE_CREATE", "Create Profile", "Create new profile", "CREATE", 2, actor);
        AddOption(suite, profileSub, "PROFILE_EDIT", "Edit Profile", "Modify profile", "UPDATE", 3, actor);
        AddOption(suite, profileSub, "PROFILE_DEACTIVATE", "Deactivate Profile", "Disable profile", "DEACTIVATE", 4, actor);

        var roleMenu = AddMenu(suite, module, "ROLES", "Roles", "Role definition and management", 2, actor);
        var roleSub = AddSubMenu(suite, roleMenu, "ROLE_LIST", "Role List", "View and manage roles", 1, actor);
        AddOption(suite, roleSub, "ROLE_VIEW", "View Roles", "List and view roles", "READ", 1, actor);
        AddOption(suite, roleSub, "ROLE_CREATE", "Create Role", "Create new role", "CREATE", 2, actor);
        AddOption(suite, roleSub, "ROLE_EDIT", "Edit Role", "Modify role definition", "UPDATE", 3, actor);
        AddOption(suite, roleSub, "ROLE_DELETE", "Delete Role", "Remove role", "DELETE", 4, actor);

        var templateMenu = AddMenu(suite, module, "TEMPLATES", "Permission Templates", "Permission template management", 3, actor);
        var templateSub = AddSubMenu(suite, templateMenu, "TEMPLATE_LIST", "Template List", "View and manage permission templates", 1, actor);
        AddOption(suite, templateSub, "TEMPLATE_VIEW", "View Templates", "List and view templates", "READ", 1, actor);
        AddOption(suite, templateSub, "TEMPLATE_CREATE", "Create Template", "Create new permission template", "CREATE", 2, actor);
        AddOption(suite, templateSub, "TEMPLATE_EDIT", "Edit Template", "Modify template permissions", "UPDATE", 3, actor);
        AddOption(suite, templateSub, "TEMPLATE_PUBLISH", "Publish Template", "Publish template for use", "PUBLISH", 4, actor);
        AddOption(suite, templateSub, "TEMPLATE_DEPRECATE", "Deprecate Template", "Deprecate template", "DEPRECATE", 5, actor);
        AddOption(suite, templateSub, "TEMPLATE_ASSIGN", "Assign Template", "Assign template to profile", "ASSIGN", 6, actor);

        var permMenu = AddMenu(suite, module, "PERMISSIONS", "Profile Permissions", "Profile permission overrides", 4, actor);
        var permSub = AddSubMenu(suite, permMenu, "PERM_LIST", "Permission List", "View and manage profile permissions", 1, actor);
        AddOption(suite, permSub, "PERM_VIEW", "View Permissions", "List and view permissions", "READ", 1, actor);
        AddOption(suite, permSub, "PERM_OVERRIDE", "Override Permission", "Override template permission", "UPDATE", 2, actor);
        AddOption(suite, permSub, "PERM_REVOKE", "Revoke Permission", "Revoke permission override", "REVOKE", 3, actor);
    }

    private static void PopulateConfigModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var flagMenu = AddMenu(suite, module, "FEATURE_FLAGS", "Feature Flags", "Feature flag management", 1, actor);
        var flagSub = AddSubMenu(suite, flagMenu, "FLAG_LIST", "Flag List", "View and manage feature flags", 1, actor);
        AddOption(suite, flagSub, "FLAG_VIEW", "View Flags", "List and view feature flags", "READ", 1, actor);
        AddOption(suite, flagSub, "FLAG_CREATE", "Create Flag", "Create new feature flag", "CREATE", 2, actor);
        AddOption(suite, flagSub, "FLAG_EDIT", "Edit Flag", "Modify feature flag", "UPDATE", 3, actor);
        AddOption(suite, flagSub, "FLAG_TOGGLE", "Toggle Flag", "Enable/disable feature flag", "ACTIVATE", 4, actor);

        var appConfigMenu = AddMenu(suite, module, "APP_CONFIG", "App Configuration", "Application configuration management", 2, actor);
        var appConfigSub = AddSubMenu(suite, appConfigMenu, "CONFIG_LIST", "Configuration List", "View and manage app settings", 1, actor);
        AddOption(suite, appConfigSub, "CONFIG_VIEW", "View Config", "List and view configuration", "READ", 1, actor);
        AddOption(suite, appConfigSub, "CONFIG_EDIT", "Edit Config", "Modify configuration values", "UPDATE", 2, actor);
        AddOption(suite, appConfigSub, "CONFIG_EXPORT", "Export Config", "Export configuration", "EXPORT", 3, actor);
    }

    private static void PopulateApprovalModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var requestMenu = AddMenu(suite, module, "APPROVAL_REQUESTS", "Approval Requests", "Approval request management", 1, actor);
        var requestSub = AddSubMenu(suite, requestMenu, "REQUEST_LIST", "Request List", "View and manage approval requests", 1, actor);
        AddOption(suite, requestSub, "REQUEST_VIEW", "View Requests", "List and view approval requests", "READ", 1, actor);
        AddOption(suite, requestSub, "REQUEST_CREATE", "Create Request", "Submit new approval request", "CREATE", 2, actor);
        AddOption(suite, requestSub, "REQUEST_APPROVE", "Approve Request", "Approve pending request", "APPROVE", 3, actor);
        AddOption(suite, requestSub, "REQUEST_REJECT", "Reject Request", "Reject pending request", "REJECT", 4, actor);
        AddOption(suite, requestSub, "REQUEST_EXPORT", "Export Requests", "Export approval history", "EXPORT", 5, actor);
    }

    private static void PopulateInventoryModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var stockMenu = AddMenu(suite, module, "STOCK", "Stock Management", "Inventory stock operations", 1, actor);
        var stockSub = AddSubMenu(suite, stockMenu, "STOCK_LIST", "Stock List", "View and manage stock levels", 1, actor);
        AddOption(suite, stockSub, "STOCK_VIEW", "View Stock", "List and view stock levels", "READ", 1, actor);
        AddOption(suite, stockSub, "STOCK_ADJUST", "Adjust Stock", "Manually adjust stock quantity", "ADJUST_STOCK", 2, actor);
        AddOption(suite, stockSub, "STOCK_TRANSFER", "Transfer Stock", "Transfer stock between locations", "TRANSFER_STOCK", 3, actor);
        AddOption(suite, stockSub, "STOCK_COUNT", "Count Inventory", "Perform inventory count", "COUNT_INVENTORY", 4, actor);
        AddOption(suite, stockSub, "STOCK_EXPORT", "Export Stock", "Export stock report", "EXPORT", 5, actor);

        var movementMenu = AddMenu(suite, module, "MOVEMENTS", "Stock Movements", "Stock movement tracking", 2, actor);
        var movementSub = AddSubMenu(suite, movementMenu, "MOVEMENT_LIST", "Movement List", "View stock movement history", 1, actor);
        AddOption(suite, movementSub, "MOVEMENT_VIEW", "View Movements", "List and view movements", "READ", 1, actor);
        AddOption(suite, movementSub, "MOVEMENT_CREATE", "Record Movement", "Record stock movement", "CREATE", 2, actor);
        AddOption(suite, movementSub, "MOVEMENT_EXPORT", "Export Movements", "Export movement history", "EXPORT", 3, actor);
    }

    private static void PopulatePromotionModule(SystemSuiteAggregate suite, ModuleEntity module, ActorId actor)
    {
        suite.ActivateModule(module.Id, actor);

        var promoMenu = AddMenu(suite, module, "PROMOTIONS", "Promotions", "Promotion campaign management", 1, actor);
        var promoSub = AddSubMenu(suite, promoMenu, "PROMO_LIST", "Promotion List", "View and manage promotions", 1, actor);
        AddOption(suite, promoSub, "PROMO_VIEW", "View Promotions", "List and view promotions", "READ", 1, actor);
        AddOption(suite, promoSub, "PROMO_CREATE", "Create Promotion", "Create new promotion", "CREATE", 2, actor);
        AddOption(suite, promoSub, "PROMO_EDIT", "Edit Promotion", "Modify promotion details", "UPDATE", 3, actor);
        AddOption(suite, promoSub, "PROMO_SCHEDULE", "Schedule Promotion", "Schedule promotion activation", "SCHEDULE_PROMOTION", 4, actor);
        AddOption(suite, promoSub, "PROMO_APPLY", "Apply Promotion", "Manually apply promotion", "APPLY_PROMOTION", 5, actor);
        AddOption(suite, promoSub, "PROMO_CANCEL", "Cancel Promotion", "Cancel active promotion", "CANCEL_PROMOTION", 6, actor);
        AddOption(suite, promoSub, "PROMO_DELETE", "Delete Promotion", "Remove promotion", "DELETE", 7, actor);
    }

    private static MenuEntity AddMenu(SystemSuiteAggregate suite, ModuleEntity module, string code, string label, string description, int sortOrder, ActorId actor)
    {
        module.AddMenu(Code.Create(code), Name.Create(label), Description.Create(description), sortOrder, actor);
        return module.Menus.First(m => m.Code.GetValue() == code);
    }

    private static SubMenuEntity AddSubMenu(SystemSuiteAggregate suite, MenuEntity menu, string code, string label, string description, int sortOrder, ActorId actor)
    {
        menu.AddSubMenu(Code.Create(code), Name.Create(label), Description.Create(description), sortOrder, actor);
        return menu.SubMenus.First(sm => sm.Code.GetValue() == code);
    }

    private static void AddOption(SystemSuiteAggregate suite, SubMenuEntity subMenu, string code, string label, string description, string actionCode, int sortOrder, ActorId actor)
    {
        subMenu.AddOption(Code.Create(code), Name.Create(label), Description.Create(description), ActionCode.Create(actionCode), sortOrder, actor);
    }

    private static ActionId GetActionId(SystemSuiteAggregate suite, string actionCode)
    {
        var action = suite.Actions.FirstOrDefault(a => a.Code.GetValue() == actionCode);
        return action?.GetId() ?? ActionId.Create();
    }

    private static IReadOnlyList<PermissionTemplateAggregate> BuildSeedPermissionTemplates(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, ActorId actor)
    {
        var templates = new List<PermissionTemplateAggregate>();
        if (suites.Count == 0) return templates;

        var coreSuite = suites[0];
        var wmsSuite = suites.Count > 1 ? suites[1] : null;

        var adminTemplate = BuildAdminTemplate(tenantId, coreSuite, actor);
        if (adminTemplate is not null) templates.Add(adminTemplate);

        var operatorTemplate = BuildOperatorTemplate(tenantId, coreSuite, actor);
        if (operatorTemplate is not null) templates.Add(operatorTemplate);

        var viewerTemplate = BuildViewerTemplate(tenantId, coreSuite, actor);
        if (viewerTemplate is not null) templates.Add(viewerTemplate);

        var deprecatedTemplate = BuildDeprecatedTemplate(tenantId, coreSuite, actor);
        if (deprecatedTemplate is not null) templates.Add(deprecatedTemplate);

        var conflictTemplate = BuildConflictScenarioTemplate(tenantId, coreSuite, actor);
        if (conflictTemplate is not null) templates.Add(conflictTemplate);

        if (wmsSuite is not null)
        {
            var wmsAdminTemplate = BuildWmsAdminTemplate(tenantId, wmsSuite, actor);
            if (wmsAdminTemplate is not null) templates.Add(wmsAdminTemplate);

            var wmsReadOnlyTemplate = BuildWmsReadOnlyTemplate(tenantId, wmsSuite, actor);
            if (wmsReadOnlyTemplate is not null) templates.Add(wmsReadOnlyTemplate);
        }

        return templates;
    }

    private static PermissionTemplateAggregate? BuildAdminTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId)),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var identityModule = suite.Modules.First(m => m.Code.GetValue() == "IDENTITY");
        var authModule = suite.Modules.First(m => m.Code.GetValue() == "AUTH");
        var configModule = suite.Modules.First(m => m.Code.GetValue() == "CONFIG");
        var approvalModule = suite.Modules.First(m => m.Code.GetValue() == "APPROVALS");

        template.AddItem(ExclusiveArcTarget.SystemSuite, suite.GetId(), GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.SystemSuite, suite.GetId(), GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.SystemSuite, suite.GetId(), GetActionId(suite, "EXPORT"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "DELETE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "DEACTIVATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "ASSIGN"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "REVOKE"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "DELETE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "PUBLISH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "DEPRECATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "ASSIGN"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "ACTIVATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "EXPORT"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "APPROVE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "REJECT"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "EXPORT"), true, false, actor);

        var tenantSub = identityModule.Menus.First(m => m.Code.GetValue() == "TENANTS").SubMenus.First(sm => sm.Code.GetValue() == "TENANT_LIST");
        template.AddItem(ExclusiveArcTarget.Submodule, tenantSub.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, tenantSub.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, tenantSub.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, tenantSub.Id, GetActionId(suite, "DEACTIVATE"), true, false, actor);

        template.Publish(actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildOperatorTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminProfileId)),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var identityModule = suite.Modules.First(m => m.Code.GetValue() == "IDENTITY");
        var approvalModule = suite.Modules.First(m => m.Code.GetValue() == "APPROVALS");

        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "EXPORT"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "DELETE"), false, true, actor);

        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "APPROVE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, approvalModule.Id, GetActionId(suite, "REJECT"), false, true, actor);

        var userSub = identityModule.Menus.First(m => m.Code.GetValue() == "USERS").SubMenus.First(sm => sm.Code.GetValue() == "USER_LIST");
        template.AddItem(ExclusiveArcTarget.Submodule, userSub.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, userSub.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, userSub.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, userSub.Id, GetActionId(suite, "ASSIGN"), false, true, actor);

        template.Publish(actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildViewerTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.NewGuid().ToString()),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var identityModule = suite.Modules.First(m => m.Code.GetValue() == "IDENTITY");
        var authModule = suite.Modules.First(m => m.Code.GetValue() == "AUTH");

        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "DELETE"), false, true, actor);

        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);

        var tenantMenu = identityModule.Menus.First(m => m.Code.GetValue() == "TENANTS");
        var tenantSub = tenantMenu.SubMenus.First(sm => sm.Code.GetValue() == "TENANT_LIST");
        template.AddItem(ExclusiveArcTarget.Submodule, tenantSub.Id, GetActionId(suite, "READ"), true, false, actor);

        var tenantViewOption = tenantSub.Options.First(o => o.Code.GetValue() == "TENANT_VIEW");
        template.AddItem(ExclusiveArcTarget.Option, tenantViewOption.Id, GetActionId(suite, "READ"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "DEACTIVATE"), false, true, actor);

        var deactivateActionId = GetActionId(suite, "DEACTIVATE");
        var inactiveItem = template.Items.First(i => i.ActionId.GetValue() == deactivateActionId.GetValue() && i.TargetType == ExclusiveArcTarget.Module);
        template.DeactivateItem(inactiveItem.Id, actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildDeprecatedTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.NewGuid().ToString()),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var configModule = suite.Modules.First(m => m.Code.GetValue() == "CONFIG");

        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, configModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);

        template.Publish(actor);
        template.Deprecate(actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildConflictScenarioTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.NewGuid().ToString()),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var identityModule = suite.Modules.First(m => m.Code.GetValue() == "IDENTITY");
        var authModule = suite.Modules.First(m => m.Code.GetValue() == "AUTH");

        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, identityModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, authModule.Id, GetActionId(suite, "DELETE"), false, true, actor);

        var roleMenu = authModule.Menus.First(m => m.Code.GetValue() == "ROLES");
        var roleSub = roleMenu.SubMenus.First(sm => sm.Code.GetValue() == "ROLE_LIST");
        template.AddItem(ExclusiveArcTarget.Submodule, roleSub.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, roleSub.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, roleSub.Id, GetActionId(suite, "UPDATE"), false, true, actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildWmsAdminTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var adminRoleId = Guid.NewGuid().ToString();
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(adminRoleId),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var inventoryModule = suite.Modules.First(m => m.Code.GetValue() == "INVENTORY");
        var promotionModule = suite.Modules.First(m => m.Code.GetValue() == "PROMOTIONS");

        template.AddItem(ExclusiveArcTarget.SystemSuite, suite.GetId(), GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.SystemSuite, suite.GetId(), GetActionId(suite, "SEARCH"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "DELETE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "ADJUST_STOCK"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "TRANSFER_STOCK"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "COUNT_INVENTORY"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "EXPORT"), true, false, actor);

        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "CREATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "UPDATE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "DELETE"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "APPLY_PROMOTION"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "SCHEDULE_PROMOTION"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "CANCEL_PROMOTION"), true, false, actor);

        var stockSub = inventoryModule.Menus.First(m => m.Code.GetValue() == "STOCK").SubMenus.First(sm => sm.Code.GetValue() == "STOCK_LIST");
        template.AddItem(ExclusiveArcTarget.Submodule, stockSub.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, stockSub.Id, GetActionId(suite, "ADJUST_STOCK"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Submodule, stockSub.Id, GetActionId(suite, "TRANSFER_STOCK"), true, false, actor);

        template.Publish(actor);

        return template;
    }

    private static PermissionTemplateAggregate? BuildWmsReadOnlyTemplate(TenantId tenantId, SystemSuiteAggregate suite, ActorId actor)
    {
        var result = PermissionTemplateAggregate.Create(
            tenantId,
            RoleId.Load(Guid.NewGuid().ToString()),
            suite.GetId(),
            actor);

        if (result.IsFailure) return null;

        var template = result.Value;

        var inventoryModule = suite.Modules.First(m => m.Code.GetValue() == "INVENTORY");
        var promotionModule = suite.Modules.First(m => m.Code.GetValue() == "PROMOTIONS");

        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "EXPORT"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "DELETE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "ADJUST_STOCK"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, inventoryModule.Id, GetActionId(suite, "TRANSFER_STOCK"), false, true, actor);

        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "READ"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "SEARCH"), true, false, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "CREATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "UPDATE"), false, true, actor);
        template.AddItem(ExclusiveArcTarget.Module, promotionModule.Id, GetActionId(suite, "APPLY_PROMOTION"), false, true, actor);

        template.Publish(actor);

        return template;
    }

    private static IReadOnlyList<ProfileAggregate> BuildSeedProfiles(TenantId tenantId, IReadOnlyList<SystemSuiteAggregate> suites, ActorId actor)
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
