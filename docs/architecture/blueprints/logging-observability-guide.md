# Logging and Observability Guide

## Purpose

This guide defines how UMS emits logs automatically, how observability data is propagated, and when to decorate application handlers with AOP logging.

## Automatic Logging Modes

UMS emits logs in two complementary ways:

1. **HTTP request logging**
   - Enabled globally in the API pipeline through `UseSerilogRequestLogging(...)`.
   - Captures request host, correlation data, session tracking data, trace identifiers, response status, and elapsed time.

2. **AOP handler logging**
   - Enabled per method with `[LoggerAspect(...)]`.
   - Intended for application handlers and important cross-cutting execution points.
   - Produces structured entry/exit/exception logs compatible with Serilog, OpenTelemetry, Console, Loki, and other sinks.

## Recommended Logger Types

### `IMelLogger`

Use when the goal is lightweight local debugging with Microsoft.Extensions.Logging semantics.

Recommended for:
- low-noise technical debugging
- local-only support cases
- internal diagnostics where observability enrichment is not required

### `IUmsLogger`

Use when the goal is operational/business observability.

Recommended for:
- command handlers
- mutation flows
- business operations with audit value
- important orchestration points

`IUmsLogger` enriches every emitted log with:
- `TenantId`
- `CorrelationId`
- `SessionTrackingId`
- `TraceId`
- `SpanId`
- `BoundedContext`

## Header Contract

### `X-Correlation-Id`

Optional on input.  
If omitted, the API generates one.

### `X-Session-Tracking-Id`

Strongly recommended on every client request.  
If omitted, the API generates one and echoes it back in the response.

This identifier is the cross-request session correlation key for:
- logs
- traces
- business flows
- orchestration chains
- downstream request correlation

## Environment Strategy

### Development

Goal: keep observability as local and readable as possible.

- Console text format
- Debug minimum level
- No OTLP exporter unless explicitly configured
- Human-readable output with correlation and session identifiers

### UAT

Goal: behave close to production while remaining easy to inspect.

- Compact JSON console output
- Information minimum level
- OTLP endpoint configurable through configuration
- No developer-oriented noisy formatting

### Production

Goal: ship structured logs and traces to standard observability pipelines.

- Compact JSON console output
- Information minimum level
- OTLP endpoint configurable through configuration
- PII masking remains enabled

## Configuration

The active configuration is read from:

- `appsettings.Development.json`
- `appsettings.UAT.json`
- `appsettings.Production.json`

Relevant settings:

```json
{
  "Observability": {
    "Logging": {
      "MinimumLevel": "Debug",
      "ConsoleFormat": "Text",
      "OutputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SessionTrackingId} {SourceContext} {Message:lj}{NewLine}{Exception}"
    },
    "Tracing": {
      "OtlpEndpoint": "",
      "ServiceVersion": "1.0.0-dev"
    }
  }
}
```

`ConsoleFormat` values:
- `Text`
- `CompactJson`

## Example Usage

### Command Handler

```csharp
[LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateFeatureFlagResponse>> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
{
    ...
}
```

### Lightweight Debug Handler

```csharp
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result> Handle(SomeInternalCommand request, CancellationToken cancellationToken)
{
    ...
}
```

## Architectural Rule for Decorators

### Decorate with `IUmsLogger`

- command handlers
- mutation workflows
- lifecycle state transitions
- approval and governance flows
- operations whose failures or duration matter operationally

### Usually do not decorate with `IUmsLogger`

- simple read-only queries returning paged or lookup data
- trivial projections where request logging already gives enough operational signal

For queries, prefer adding `IUmsLogger` only when:
- the query is expensive
- the query is cross-bounded-context
- the query has sensitive latency or reliability requirements
- the query participates in a business-critical support scenario

## Notes

- `SessionTrackingId` is intentionally **not** emitted as a metric dimension due to high cardinality.
- `TenantId` enrichment is performed by the UMS logger, not by request middleware.
- AOP proxy registration for MediatR handlers is enabled centrally in Infrastructure.
