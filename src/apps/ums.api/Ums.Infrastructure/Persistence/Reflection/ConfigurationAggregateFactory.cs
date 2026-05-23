using System.Reflection;
using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.FeatureFlag.FlagEvaluationLog;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Shell.Ddd;
using Ums.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;
using FeatureFlagEvaluationLogEntity = Ums.Domain.Configuration.FeatureFlag.FlagEvaluationLog.FlagEvaluationLog;

internal static class ConfigurationAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static readonly Type AppConfigurationIdType = Type.GetType("Ums.Domain.Kernel.ValueObjects.AppConfigurationId, Ums.Domain")!;
    private static readonly Type FeatureFlagIdType = Type.GetType("Ums.Domain.Kernel.ValueObjects.FeatureFlagId, Ums.Domain")!;

    public static AppConfigurationAggregate RehydrateAppConfiguration(AppConfigurationRecord record)
    {
        var props = new AppConfigurationProps(
            LoadTypedId(AppConfigurationIdType, record.Id),
            record.TenantId.HasValue ? TenantId.Load(record.TenantId.Value) : null,
            record.SystemSuiteId.HasValue ? SystemSuiteId.Load(record.SystemSuiteId.Value) : null,
            record.ModuleId.HasValue ? IdValueObject.Load(record.ModuleId.Value) : null,
            Code.Create(record.Code),
            ConfigurationValue.Create(record.Value),
            Description.Create(record.Description),
            record.IsInheritable,
            record.IsEncrypted,
            ActorId.Create(record.CreatedBy));

        props.Scope = DomainEnumerationMapper.FromValue<ConfigurationScope>(record.ScopeId);
        props.Version = record.Version;
        props.Status = DomainEnumerationMapper.FromValue<ConfigStatus>(record.StatusId);
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var aggregate = Construct<AppConfigurationAggregate, AppConfigurationProps>(props);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();
        return aggregate;
    }

    public static FeatureFlagAggregate RehydrateFeatureFlag(
        FeatureFlagRecord record,
        IReadOnlyCollection<FeatureFlagEvaluationLogRecord> evaluationLogRecords)
    {
        var props = new FeatureFlagProps(
            LoadTypedId(FeatureFlagIdType, record.Id),
            record.FlagCode,
            DomainEnumerationMapper.FromValue<FlagType>(record.FlagTypeId),
            record.FlagTargets,
            record.LinkedResourceTypeId.HasValue ? DomainEnumerationMapper.FromValue<LinkedResourceType>(record.LinkedResourceTypeId.Value) : null,
            record.LinkedResourceId.HasValue ? IdValueObject.Load(record.LinkedResourceId.Value) : null,
            record.RolloutPercentage,
            ActorId.Create(record.CreatedBy));

        props.Status = DomainEnumerationMapper.FromValue<FlagStatus>(record.StatusId);
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var aggregate = Construct<FeatureFlagAggregate, FeatureFlagProps>(props);
        var logs = evaluationLogRecords.Select(RehydrateEvaluationLog).ToList();
        SetField(aggregate, "_evaluationLog", logs);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();
        return aggregate;
    }

    private static FeatureFlagEvaluationLogEntity RehydrateEvaluationLog(FeatureFlagEvaluationLogRecord record)
    {
        var props = new FlagEvaluationLogProps(
            IdValueObject.Load(record.Id),
            record.EvaluatedBy,
            record.Result,
            record.Context)
        {
            EvaluatedAt = record.EvaluatedAtUtc,
        };

        return Construct<FeatureFlagEvaluationLogEntity, FlagEvaluationLogProps>(props);
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

    private static IdValueObject LoadTypedId(Type idType, Guid value)
    {
        var method = idType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "Load"
                && candidate.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(Guid))
            ?? throw new InvalidOperationException($"Static Load(Guid) method was not found on {idType.Name}.");

        return (IdValueObject)method.Invoke(null, [value])!;
    }
}
