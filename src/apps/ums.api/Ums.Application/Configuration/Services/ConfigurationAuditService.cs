namespace Ums.Application.Configuration.Services;

using Ums.Application.Common.Aop;

public sealed class ConfigurationAuditService
{
    private readonly IAuditTrailSink _auditTrailSink;

    public ConfigurationAuditService(IAuditTrailSink auditTrailSink)
    {
        _auditTrailSink = auditTrailSink;
    }

    public void RecordConfigurationChange(
        Guid userId,
        string parameterCode,
        Guid? tenantId,
        string? previousValue,
        string? newValue,
        string operationType,
        Guid rootTenantId)
    {
        var entry = new AuditTrailEntry(
            WhoActed: userId,
            SubjectType: "AppConfiguration",
            WhatChanged: $"Parameter '{parameterCode}' {operationType}",
            EventType: operationType,
            AuditResult: string.IsNullOrEmpty(newValue) ? "DELETED" : "MODIFIED",
            AffectedEntityId: Guid.Empty,
            AffectedEntityType: "AppConfiguration",
            RootTenantId: rootTenantId,
            Metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                ParameterCode = parameterCode,
                TenantId = tenantId,
                PreviousValue = previousValue,
                NewValue = newValue,
                Operation = operationType
            }));

        _auditTrailSink.TryWrite(entry);
    }

    public void RecordParameterOverride(
        Guid userId,
        string parameterCode,
        Guid tenantId,
        string? previousValue,
        string? newValue,
        Guid rootTenantId)
    {
        RecordConfigurationChange(
            userId,
            parameterCode,
            tenantId,
            previousValue,
            newValue,
            "OVERRIDE",
            rootTenantId);
    }
}