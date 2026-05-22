using System.Reflection;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.Profile.ProfilePermission;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Shell.Ddd;
using Ums.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;

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
}
