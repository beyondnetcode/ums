# ADR-0063: Idempotency Key Middleware (FIX-06 / RISK-05)

**Status:** Accepted  
**Date:** 2026-05-24  
**Decision Owner:** Architecture  
**arc32 disposition:** Proposed for arc32 adoption — HTTP-level request deduplication is runtime-neutral; applicable to any ASP.NET Core satellite  
**Related:**
- [ADR-0051: Event Bus Injectable Port](./0051-event-bus-injectable-port.md)
- [CP-07: Idempotency Key Middleware](../artifacts/canonical-patterns/cp-07-idempotency-middleware.md)

---

## Context

UMS command handlers are state-mutating operations: they create aggregates, update status, publish domain events to the outbox, and interact with external identity providers. A network retry, a double-tap from a mobile client, or a saga compensation step re-issuing a command can execute the same operation twice — creating duplicate tenants, double-charged fees, or conflicting state.

The domain layer enforces business invariants (e.g., "tenant code already exists"), but returning a domain error to the second identical call is not always the right behaviour. The client may expect the same successful response as the first call.

### Why middleware over a MediatR behavior

A MediatR `IPipelineBehavior` runs after deserialization and validation — it requires a persistent idempotency store per handler type. HTTP-level middleware:

1. Intercepts before deserialization — the response cache can be returned without running the full pipeline
2. Is handler-agnostic — one registration covers all endpoints
3. Operates on the HTTP response as a byte buffer — the reply is identical to the original, including headers and status code

---

## Decision

**Implement request deduplication as an ASP.NET Core middleware reading the `Idempotency-Key` header, caching the first response, and replaying it verbatim for duplicate requests.**

### Behaviour matrix

| Scenario | Response |
|----------|----------|
| No `Idempotency-Key` header | Pass through — key is optional |
| New key, first request | Execute pipeline, cache response (TTL: 24h), return result |
| Known key, request completed | Return cached response immediately — handler NOT invoked |
| Known key, request in-flight | Return HTTP 409 "request already in progress" |
| Non-mutating method (GET, DELETE) | Pass through — naturally idempotent |

### Covered methods

`POST`, `PUT`, `PATCH` only. `GET` and `DELETE` pass through unconditionally.

### Cache backend

`IMemoryCache` (single-node default). For multi-replica deployments, replace with `IDistributedCache` (Redis or SQL Server) to share state across pods.

### Key format

Client-generated UUID (v4), e.g. `550e8400-e29b-41d4-a716-446655440000`. The middleware does not generate keys — the client is responsible for generating and retrying with the same key.

### TTL

24 hours (configurable via `IdempotencyOptions`). After TTL expiry, a re-submitted key is treated as a new request.

### DI registration

```csharp
// Program.cs / DependencyInjection
services.AddMemoryCache();      // required for single-node IdempotencyStore
app.UseIdempotency();           // must come after UseCorrelationId, before routing
```

### Middleware pipeline position

```
UseCorrelationId
  → UseSessionTracking
    → UseGlobalExceptionHandler
      → UseIdempotency          ← here
        → UseRateLimiter
          → Routes
```

Position after `UseGlobalExceptionHandler` ensures that exceptions during pipeline execution are caught and not cached. Position before routing ensures the replay occurs before endpoint selection.

---

## Consequences

### Positive

- Duplicate HTTP requests return identical responses — transparent to clients
- Handler business logic executes exactly once per logical operation regardless of network retries
- No per-handler boilerplate — one middleware covers all mutating endpoints
- Pairs naturally with the outbox pattern: if the handler ran and committed the outbox message, the cached response is returned; the domain event is not re-published

### Trade-offs

- In-memory cache is not shared across pod replicas — a retry hitting a different pod will re-execute. Mitigate with Redis-backed `IDistributedCache` in production
- Response body is cached as a byte array — large responses consume memory proportionally to unique active keys
- The middleware caches `2xx` responses only — error responses are not cached (client should retry on failure with the same key)
- `Idempotency-Key` is a request-level concept; it cannot prevent duplicate events if the same command is issued with different keys by a misbehaving client

---

## arc32 Extraction Checklist

- [ ] `IdempotencyMiddleware` — no UMS-specific import; depends only on `IMemoryCache` and ASP.NET Core abstractions
- [ ] `IdempotencyOptions` — simple POCO with TTL and enabled/disabled flag
- [ ] `UseIdempotency()` extension method

---

**[ADR Registry](./index.md)** | **[CP-07 Idempotency](../artifacts/canonical-patterns/cp-07-idempotency-middleware.md)**
