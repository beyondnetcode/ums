# Logging Decorator Coverage Inventory

## Summary

- Total application handlers inspected: `103`
- Decorated with `IUmsLogger`: `103`
- Missing decorator: `0`

## Architectural Position

UMS now applies `[LoggerAspect(Type = typeof(IUmsLogger), ...)]` to every application handler.

That means the standard E2E tracing boundary is now consistent across:

- command handlers
- query handlers
- REST requests
- GraphQL requests
- Serilog request logging
- OpenTelemetry traces

## What This Covers

### Handler-level tracing

Every `ICommandHandler<,>` and `IQueryHandler<,>` emits structured application logs through `IUmsLogger`, enriched with:

- `TenantId`
- `CorrelationId`
- `SessionTrackingId`
- `TraceId`
- `SpanId`
- `BoundedContext`

### Request-level tracing

The HTTP pipeline propagates:

- `X-Correlation-Id`
- `X-Session-Tracking-Id`

and writes them into:

- request scope
- `Activity` baggage
- `Activity` tags
- Serilog request logs

## Remaining Architectural Guidance

Decorating all handlers gives us a strong and uniform tracing baseline. We should still be selective outside handler boundaries.

### Recommended additional logger usage

- background services that continue business flows asynchronously
- outbox dispatch and replay operations
- external integration gateways
- expensive domain services coordinating multiple repositories

### Avoid by default

- trivial private helper methods
- low-value internal mappers
- hot-path utility functions where log volume adds noise without operational value

## Current Verdict

The application layer is now fully covered by the structured logger decorator standard. From an architectural perspective, this is an acceptable E2E tracing baseline for UMS.
