using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop;
using Ums.Shell.Aop.Aspects;

using MelILoggerFactory  = Microsoft.Extensions.Logging.ILoggerFactory;
using MelILogger         = Microsoft.Extensions.Logging.ILogger;

namespace Ums.Infrastructure.Aop;

/// <summary>
/// Serilog-backed, observability-aware implementation of <see cref="IUmsLogger"/>.
///
/// Enriches every AOP log entry with the full UMS observability envelope:
/// <list type="bullet">
///   <item><description><b>TenantId</b>   — from <see cref="IUserContext"/> (multi-tenant dimension)</description></item>
///   <item><description><b>CorrelationId</b> — from W3C Activity baggage key "correlation.id"
///         (written by <c>CorrelationIdMiddleware</c>)</description></item>
///   <item><description><b>TraceId / SpanId</b> — from <see cref="Activity.Current"/> (W3C trace context);
///         also emitted automatically by the Serilog OTel sink, but made explicit here so
///         non-OTel sinks (Console, Loki, Seq) carry the same fields</description></item>
///   <item><description><b>BoundedContext</b> — inferred from the second namespace segment of the
///         handler type (e.g. <c>Ums.Application.Identity.Tenant.Commands</c> → "Identity")</description></item>
/// </list>
///
/// Log levels:
/// <list type="bullet">
///   <item><description><see cref="LogLevel.Information"/> — method entry and successful exit</description></item>
///   <item><description><see cref="LogLevel.Error"/>       — unhandled exceptions</description></item>
/// </list>
///
/// PII safety: argument <em>values</em> are never emitted — only parameter names and CLR types.
///
/// Registration (Infrastructure DI):
/// <code>
/// services.AddKeyedTransient&lt;AopILogger, UmsSerilogLogger&gt;(typeof(IUmsLogger));
/// </code>
/// </summary>
public sealed class UmsSerilogLogger(
    MelILoggerFactory loggerFactory,
    IUserContext userContext,
    IExecutionContextAccessor executionContextAccessor) : StructuredAopLoggerBase(executionContextAccessor), IUmsLogger
{
    // ── Helpers ───────────────────────────────────────────────────────────────────────────

    private MelILogger Logger(IJoinPoint jp) =>
        loggerFactory.CreateLogger(jp.TargetType);

    /// <summary>Tenant from scoped IUserContext, or "system" when running outside a user request.</summary>
    private string TenantId() =>
        userContext.TenantId ?? "system";

    private static string BoundedContext(Type targetType) => InferBoundedContext(targetType);

    // ── ILogger contract ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();
        var bc            = BoundedContext(joinPoint.TargetType);

        // PII-safe: only names + CLR types, never values.
        var argSummary = arguments is { Length: > 0 }
            ? string.Join(", ", arguments.Select(a => $"{a.Name}:{a.Type}"))
            : string.Empty;

        if (string.IsNullOrEmpty(argSummary))
        {
            log.LogInformation(
                "→ {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
                bc, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
                tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
        }
        else
        {
            log.LogInformation(
                "→ {BoundedContext} {Handler}.{Method} params=[{Params}] | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
                bc, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, argSummary,
                tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
        }
    }

    /// <inheritdoc/>
    public override void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} in {Duration}ms | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            duration, tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
    }

    /// <inheritdoc/>
    public override void OnExit(IJoinPoint joinPoint, string requestId, long duration)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} in {Duration}ms | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            duration, tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
    }

    /// <inheritdoc/>
    public override void OnExit(IJoinPoint joinPoint, Return @return, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
    }

    /// <inheritdoc/>
    public override void OnExit(IJoinPoint joinPoint, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
    }

    /// <inheritdoc/>
    public override void OnException(IJoinPoint joinPoint, string requestId, Exception ex)
    {
        var log           = Logger(joinPoint);
        var executionContext = ResolveExecutionContext(requestId);
        var tenantId      = TenantId();

        log.LogError(
            ex,
            "✗ {BoundedContext} {Handler}.{Method} threw {ExceptionType} | tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            ex.GetType().Name,
            tenantId, executionContext.CorrelationId, executionContext.SessionTrackingId, executionContext.TraceId, executionContext.SpanId);
    }
}
