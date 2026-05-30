using System;
using System.Reflection;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Audit.Entities;
using BeyondNetCode.Shell.Ddd;

namespace Ums.Infrastructure.Persistence.Reflection;

using AuditRecordAggregate = Ums.Domain.Audit.AuditRecord.AuditRecord;

internal static class AuditAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static AuditRecordAggregate RehydrateAuditRecord(AuditRecordRecord record)
    {
        var props = new AuditRecordProps(
            IdValueObject.Load(record.Id),
            record.WhoActed,
            DomainEnumerationMapper.FromValue<SubjectType>(record.SubjectTypeId),
            record.WhatChanged,
            record.EventType,
            DomainEnumerationMapper.FromValue<AuditResult>(record.AuditResultId),
            record.AffectedEntityId,
            record.AffectedEntityType,
            record.RootTenantId,
            record.Metadata);

        props.WhenOccurred = record.WhenOccurred;

        var aggregate = Construct<AuditRecordAggregate, AuditRecordProps>(props);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();

        return aggregate;
    }

    private static TEntity Construct<TEntity, TProps>(TProps props)
        where TEntity : class
        where TProps : class
    {
        var ctor = typeof(TEntity).GetConstructor(InstanceFlags, null, [typeof(TProps)], null)
            ?? throw new InvalidOperationException($"Constructor for {typeof(TEntity).Name} not found.");

        return (TEntity)ctor.Invoke([props]);
    }
}
