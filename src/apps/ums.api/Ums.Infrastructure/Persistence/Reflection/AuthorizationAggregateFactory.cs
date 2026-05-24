using System.Reflection;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.Profile.ProfilePermission;
using Ums.Domain.Authorization.SystemSuite.AppSetting;
using Ums.Domain.Authorization.SystemSuite.Module;
using Ums.Domain.Authorization.SystemSuite.Menu;
using Ums.Domain.Authorization.SystemSuite.SubMenu;
using Ums.Domain.Authorization.SystemSuite.Option;
using Ums.Domain.Authorization.SystemSuite.Action;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization.Template.PermissionTemplateItem;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Shell.Ddd;
using Ums.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using ModuleEntity = Ums.Domain.Authorization.SystemSuite.Module.Module;
using MenuEntity = Ums.Domain.Authorization.SystemSuite.Menu.Menu;
using SubMenuEntity = Ums.Domain.Authorization.SystemSuite.SubMenu.SubMenu;
using OptionEntity = Ums.Domain.Authorization.SystemSuite.Option.Option;
using ActionEntity = Ums.Domain.Authorization.SystemSuite.Action.Action;
using PermissionTemplateItemEntity = Ums.Domain.Authorization.Template.PermissionTemplateItem.PermissionTemplateItem;

internal static class AuthorizationAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static ProfileAggregate RehydrateProfile(
        ProfileRecord profileRecord,
        IReadOnlyCollection<ProfilePermissionRecord> permissionRecords)
    {
        var props = new ProfileProps(
            ProfileId.Load(profileRecord.Id),
            TenantId.Load(profileRecord.TenantId),
            UserId.Load(profileRecord.UserId),
            RoleId.Load(profileRecord.RoleId),
            profileRecord.BranchId.HasValue ? BranchId.Load(profileRecord.BranchId.Value) : null,
            DomainEnumerationMapper.FromValue<ProfileScope>(profileRecord.ScopeId),
            ActorId.Create(profileRecord.CreatedBy));

        props.IsActive = profileRecord.IsActive;
        SetAudit(props, profileRecord.CreatedBy, profileRecord.CreatedAtUtc, profileRecord.UpdatedBy, profileRecord.UpdatedAtUtc, profileRecord.AuditTimeSpan);

        var profile = Construct<ProfileAggregate, ProfileProps>(props);
        var permissions = permissionRecords.Select(RehydratePermission).ToList();

        SetField(profile, "_permissions", permissions);
        profile.DomainEvents.MarkChangesAsCommitted();
        profile.BrokenRules.Clear();

        return profile;
    }

    private static ProfilePermission RehydratePermission(ProfilePermissionRecord record)
    {
        var props = new ProfilePermissionProps(
            ProfilePermissionId.Load(record.Id),
            ProfileId.Load(record.ProfileId),
            TemplateId.Load(record.TemplateId),
            DomainEnumerationMapper.FromValue<ExclusiveArcTarget>(record.TargetTypeId),
            IdValueObject.Load(record.TargetId),
            ActionId.Load(record.ActionId),
            record.IsAllowed,
            record.IsDenied,
            record.IsActive,
            record.IsOverride,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        return Construct<ProfilePermission, ProfilePermissionProps>(props);
    }

    public static SystemSuiteAggregate RehydrateSystemSuite(
        SystemSuiteRecord systemSuiteRecord,
        IReadOnlyCollection<SystemSuiteModuleRecord> moduleRecords,
        IReadOnlyCollection<SystemSuiteAppSettingRecord> appSettingRecords,
        IReadOnlyCollection<SystemSuiteActionRecord> actionRecords)
    {
        var props = new Ums.Domain.Authorization.SystemSuite.SystemSuiteProps(
            IdValueObject.Load(systemSuiteRecord.Id),
            TenantId.Load(systemSuiteRecord.TenantId),
            Code.Create(systemSuiteRecord.Code),
            Name.Create(systemSuiteRecord.Name),
            Description.Create(systemSuiteRecord.Description),
            DomainEnumerationMapper.FromValue<SystemStatus>(systemSuiteRecord.StatusId),
            ActorId.Create(systemSuiteRecord.CreatedBy));

        SetAudit(props, systemSuiteRecord.CreatedBy, systemSuiteRecord.CreatedAtUtc, systemSuiteRecord.UpdatedBy, systemSuiteRecord.UpdatedAtUtc, systemSuiteRecord.AuditTimeSpan);

        var aggregate = Construct<SystemSuiteAggregate, Ums.Domain.Authorization.SystemSuite.SystemSuiteProps>(props);
        var modules = moduleRecords.OrderBy(x => x.SortOrder).Select(RehydrateModule).ToList();
        var appSettings = appSettingRecords.Select(RehydrateAppSetting).ToList();
        var actions = actionRecords.Select(RehydrateAction).ToList();

        SetField(aggregate, "_modules", modules);
        SetField(aggregate, "_appSettings", appSettings);
        SetField(aggregate, "_actions", actions);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();

        return aggregate;
    }

    public static PermissionTemplateAggregate RehydratePermissionTemplate(
        PermissionTemplateRecord templateRecord,
        IReadOnlyCollection<PermissionTemplateItemRecord> itemRecords)
    {
        var props = new PermissionTemplateProps(
            IdValueObject.Load(templateRecord.Id),
            TenantId.Load(templateRecord.TenantId),
            RoleId.Load(templateRecord.RoleId),
            SystemSuiteId.Load(templateRecord.SystemSuiteId),
            ConstructStringValueObject<TemplateVersion>(templateRecord.Version),
            DomainEnumerationMapper.FromValue<TemplateStatus>(templateRecord.StatusId),
            ActorId.Create(templateRecord.CreatedBy));

        SetAudit(props, templateRecord.CreatedBy, templateRecord.CreatedAtUtc, templateRecord.UpdatedBy, templateRecord.UpdatedAtUtc, templateRecord.AuditTimeSpan);

        var aggregate = Construct<PermissionTemplateAggregate, PermissionTemplateProps>(props);
        var items = itemRecords.Select(RehydrateTemplateItem).ToList();

        SetField(aggregate, "_items", items);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();

        return aggregate;
    }

    private static ModuleEntity RehydrateModule(SystemSuiteModuleRecord record)
    {
        var props = new ModuleProps(
            IdValueObject.Load(record.Id),
            SystemId.Load(record.SystemSuiteId),
            Code.Create(record.Code),
            Name.Create(record.Name),
            Description.Create(record.Description),
            DomainEnumerationMapper.FromValue<ModuleStatus>(record.StatusId),
            record.SortOrder,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var module = Construct<ModuleEntity, ModuleProps>(props);
        var menus = record.Menus.OrderBy(x => x.SortOrder).Select(RehydrateMenu).ToList();
        SetField(module, "_menus", menus);
        module.BrokenRules.Clear();
        return module;
    }

    private static MenuEntity RehydrateMenu(SystemSuiteMenuRecord record)
    {
        var props = new MenuProps(
            IdValueObject.Load(record.Id),
            ModuleId.Load(record.ModuleId),
            Code.Create(record.Code),
            Name.Create(record.Label),
            Description.Create(record.Description),
            record.SortOrder,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var menu = Construct<MenuEntity, MenuProps>(props);
        var subMenus = record.SubMenus.OrderBy(x => x.SortOrder).Select(RehydrateSubMenu).ToList();
        SetField(menu, "_subMenus", subMenus);
        menu.BrokenRules.Clear();
        return menu;
    }

    private static SubMenuEntity RehydrateSubMenu(SystemSuiteSubMenuRecord record)
    {
        var props = new SubMenuProps(
            IdValueObject.Load(record.Id),
            MenuId.Load(record.MenuId),
            Code.Create(record.Code),
            Name.Create(record.Label),
            Description.Create(record.Description),
            record.SortOrder,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var subMenu = Construct<SubMenuEntity, SubMenuProps>(props);
        var options = record.Options.OrderBy(x => x.SortOrder).Select(RehydrateOption).ToList();
        SetField(subMenu, "_options", options);
        subMenu.BrokenRules.Clear();
        return subMenu;
    }

    private static OptionEntity RehydrateOption(SystemSuiteOptionRecord record)
    {
        var props = new OptionProps(
            IdValueObject.Load(record.Id),
            SubMenuId.Load(record.SubMenuId),
            Code.Create(record.Code),
            Name.Create(record.Label),
            Description.Create(record.Description),
            ActionCode.Create(record.ActionCode),
            record.SortOrder,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var option = Construct<OptionEntity, OptionProps>(props);
        option.BrokenRules.Clear();
        return option;
    }

    private static ActionEntity RehydrateAction(SystemSuiteActionRecord record)
    {
        var props = new ActionProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            SystemSuiteId.Load(record.SystemSuiteId),
            record.ModuleId.HasValue ? ModuleId.Load(record.ModuleId.Value) : null,
            ActionCode.Create(record.Code),
            Name.Create(record.Name),
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var action = Construct<ActionEntity, ActionProps>(props);
        action.BrokenRules.Clear();
        return action;
    }

    private static AppSetting RehydrateAppSetting(SystemSuiteAppSettingRecord record)
    {
        var method = typeof(AppSetting).GetMethod(nameof(AppSetting.Create), BindingFlags.Static | BindingFlags.Public)
            ?? throw new InvalidOperationException("AppSetting.Create factory not found.");

        var result = (Result<AppSetting>)method.Invoke(null,
            [ConfigurationKey.Create(record.ConfigKey), ConfigurationValue.Create(record.ConfigValue), DomainEnumerationMapper.FromValue<ConfigurationScope>(record.ScopeId)])!;

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unable to rehydrate app setting {record.ConfigKey}: {result.Error}");
        }

        return result.Value;
    }

    private static PermissionTemplateItemEntity RehydrateTemplateItem(PermissionTemplateItemRecord record)
    {
        var props = new PermissionTemplateItemProps(
            IdValueObject.Load(record.Id),
            TemplateId.Load(record.TemplateId),
            DomainEnumerationMapper.FromValue<ExclusiveArcTarget>(record.TargetTypeId),
            IdValueObject.Load(record.TargetId),
            ActionId.Load(record.ActionId),
            record.IsAllowed,
            record.IsDenied,
            record.IsActive,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        var item = Construct<PermissionTemplateItemEntity, PermissionTemplateItemProps>(props);
        item.BrokenRules.Clear();
        return item;
    }

    private static TEntity Construct<TEntity, TProps>(TProps props)
        where TEntity : class
        where TProps : class
    {
        var ctor = typeof(TEntity).GetConstructor(InstanceFlags, null, [typeof(TProps)], null)
            ?? throw new InvalidOperationException($"Constructor for {typeof(TEntity).Name} not found.");

        return (TEntity)ctor.Invoke([props]);
    }

    private static void SetField<TTarget>(object target, string fieldName, TTarget value)
    {
        var field = target.GetType().GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field {fieldName} not found on {target.GetType().Name}.");

        field.SetValue(target, value);
    }

    private static void SetAudit(object props, string createdBy, DateTime createdAtUtc, string? updatedBy, DateTime? updatedAtUtc, string auditTimeSpan)
    {
        var property = props.GetType().GetProperty("Audit", InstanceFlags)
            ?? throw new InvalidOperationException($"Audit property not found on {props.GetType().Name}.");

        property.SetValue(props, AuditValueObject.Load(new AuditProps
        {
            CreatedBy = createdBy,
            CreatedAt = createdAtUtc,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAtUtc,
            TimeSpan = auditTimeSpan,
        }));
    }

    private static TValueObject ConstructStringValueObject<TValueObject>(string value)
        where TValueObject : class
    {
        var ctor = typeof(TValueObject).GetConstructor(InstanceFlags, null, [typeof(string)], null)
            ?? throw new InvalidOperationException($"String constructor for {typeof(TValueObject).Name} not found.");

        return (TValueObject)ctor.Invoke([value]);
    }
}
