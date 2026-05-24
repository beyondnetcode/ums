using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ums.Application.Common.Aop;
using Ums.Application.Common.Interfaces;
using Ums.Shell.Aop;
using Ums.Shell.Aop.Aspects;

using AopILogger         = Ums.Shell.Aop.Aspects.ILogger;
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
    IUserContext      userContext) : IUmsLogger
{
    // ── Helpers ───────────────────────────────────────────────────────────────────────────

    private MelILogger Logger(IJoinPoint jp) =>
        loggerFactory.CreateLogger(jp.TargetType);

    /// <summary>Tenant from scoped IUserContext, or "system" when running outside a user request.</summary>
    private string TenantId() =>
        userContext.TenantId ?? "system";

    /// <summary>
    /// CorrelationId resolution order:
    ///   1. W3C Activity baggage "correlation.id"  (set by CorrelationIdMiddleware via SetBaggage)
    ///   2. requestId passed by the AOP framework (comes from the [LoggerAspect] attribute)
    ///   3. Activity TraceId as fallback (already a global identifier)
    ///   4. Empty string when no HTTP context exists (background services, tests)
    /// </summary>
    private static string CorrelationId(string requestId)
    {
        var activity = Activity.Current;

        var fromBaggage = activity?.GetBaggageItem("correlation.id");
        if (!string.IsNullOrWhiteSpace(fromBaggage)) return fromBaggage!;

        if (!string.IsNullOrWhiteSpace(requestId)) return requestId;

        var fromTrace = activity?.TraceId.ToString();
        return string.IsNullOrWhiteSpace(fromTrace) ? string.Empty : fromTrace!;
    }

    /// <summary>
    /// Infers the bounded context from the handler type's namespace.
    /// <c>Ums.Application.Identity.Tenant.Commands.CreateTenantCommandHandler</c> → "Identity"
    /// Falls back to the simple class name when the namespace does not match expectations.
    /// </summary>
    private static string BoundedContext(Type targetType)
    {
        // Namespace pattern: Ums.(Application|Domain).{BoundedContext}.*
        var parts = targetType.Namespace?.Split('.') ?? [];
        return parts.Length >= 3 ? parts[2] : targetType.Name;
    }

    /// <summary>Returns the current W3C TraceId as a hex string, or empty.</summary>
    private static string TraceId() =>
        Activity.Current?.TraceId.ToString() ?? string.Empty;

    /// <summary>Returns the current W3C SpanId as a hex string, or empty.</summary>
    private static string SpanId() =>
        Activity.Current?.SpanId.ToString() ?? string.Empty;

    // ── ILogger contract ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);
        var traceId       = TraceId();
        var spanId        = SpanId();
        var bc            = BoundedContext(joinPoint.TargetType);

        // PII-safe: only names + CLR types, never values.
        var argSummary = arguments is { Length: > 0 }
            ? string.Join(", ", arguments.Select(a => $"{a.Name}:{a.Type}"))
            : string.Empty;

        if (string.IsNullOrEmpty(argSummary))
        {
            log.LogInformation(
                "→ {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId} trace={TraceId} span={SpanId}",
                bc, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
                tenantId, correlationId, traceId, spanId);
        }
        else
        {
            log.LogInformation(
                "→ {BoundedContext} {Handler}.{Method} params=[{Params}] | tenant={TenantId} cid={CorrelationId} trace={TraceId} span={SpanId}",
                bc, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, argSummary,
                tenantId, correlationId, traceId, spanId);
        }
    }

    /// <inheritdoc/>
    public void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} in {Duration}ms | tenant={TenantId} cid={CorrelationId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            duration, tenantId, correlationId);
    }

    /// <inheritdoc/>
    public void OnExit(IJoinPoint joinPoint, string requestId, long duration)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} in {Duration}ms | tenant={TenantId} cid={CorrelationId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            duration, tenantId, correlationId);
    }

    /// <inheritdoc/>
    public void OnExit(IJoinPoint joinPoint, Return @return, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            tenantId, correlationId);
    }

    /// <inheritdoc/>
    public void OnExit(IJoinPoint joinPoint, string requestId)
    {
        var log           = Logger(joinPoint);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);

        log.LogInformation(
            "← {BoundedContext} {Handler}.{Method} | tenant={TenantId} cid={CorrelationId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            tenantId, correlationId);
    }

    /// <inheritdoc/>
    public void OnException(IJoinPoint joinPoint, string requestId, Exception ex)
    {
        var log           = Logger(joinPoint);
        var tenantId      = TenantId();
        var correlationId = CorrelationId(requestId);
        var traceId       = TraceId();
        var spanId        = SpanId();

        log.LogError(
            ex,
            "✗ {BoundedContext} {Handler}.{Method} threw {ExceptionType} | tenant={TenantId} cid={CorrelationId} trace={TraceId} span={SpanId}",
            BoundedContext(joinPoint.TargetType),
            joinPoint.TargetType.Name, joinPoint.MethodInfo.Name,
            ex.GetType().Name,
            tenantId, correlationId, traceId, spanId);
    }
}
