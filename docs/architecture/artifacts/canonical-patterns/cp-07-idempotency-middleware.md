# CP-07: Idempotency Key Middleware

**Type:** Canonical Pattern  
**Status:** Accepted  
**arc32 disposition:** Proposed for arc32 — no product-specific dependencies; portable to any ASP.NET Core satellite  
**Related ADR:** [ADR-0063: Idempotency Middleware](../../adrs/0063-idempotency-middleware.md)

---

## Problem

Mutating HTTP endpoints (`POST`, `PUT`, `PATCH`) may be called more than once for the same logical operation due to network retries, client bugs, or saga compensation steps. Re-executing the handler creates duplicate aggregates or inconsistent state.

---

## Pattern

An ASP.NET Core middleware reads a client-supplied `Idempotency-Key` header, executes the pipeline on first call, caches the response, and replays it verbatim for subsequent calls with the same key — without re-invoking any handler.

```
Client                  Middleware                  Pipeline
──────                  ──────────                  ────────
POST /tenants           │
  Idempotency-Key: abc  │
                  ──►  │  Key "abc" known? No
                        │  Mark "abc" as in-flight
                        │  ──────────────────────►  Handler executes
                        │  ◄──────────────────────  Result
                        │  Cache response (24h)
                  ◄──   200 { tenantId: "..." }

POST /tenants (retry)   │
  Idempotency-Key: abc  │
                  ──►  │  Key "abc" known? Yes (completed)
                        │  Return cached response (no handler invoked)
                  ◄──   200 { tenantId: "..." }

POST /tenants (parallel)│
  Idempotency-Key: abc  │
                  ──►  │  Key "abc" known? Yes (in-flight)
                  ◄──   409 "request already in progress"
```

---

## Implementation

```csharp
public sealed class IdempotencyMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    ILogger<IdempotencyMiddleware> logger)
{
    private const string Header       = "Idempotency-Key";
    private const string InFlight     = ":inflight";
    private static readonly HashSet<string> Methods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };

    public async Task InvokeAsync(HttpContext context)
    {
        if (!Methods.Contains(context.Request.Method)
            || !context.Request.Headers.TryGetValue(Header, out var keyValues)
            || string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            await next(context); return;
        }

        var key = keyValues.First()!;

        // Parallel duplicate check
        if (cache.TryGetValue(key + InFlight, out _))
        {
            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(
                new { error = "request already in progress", idempotencyKey = key });
            return;
        }

        // Replay completed request
        if (cache.TryGetValue(key, out CachedResponse? cached))
        {
            context.Response.StatusCode  = cached!.StatusCode;
            context.Response.ContentType = cached.ContentType;
            await context.Response.Body.WriteAsync(cached.Body);
            return;
        }

        // First call — execute and cache
        cache.Set(key + InFlight, true, TimeSpan.FromMinutes(5));
        try
        {
            var original = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await next(context);

            buffer.Position = 0;
            var body = buffer.ToArray();

            if (context.Response.StatusCode is >= 200 and < 300)
                cache.Set(key, new CachedResponse(
                    context.Response.StatusCode,
                    context.Response.ContentType ?? "application/json",
                    body), TimeSpan.FromHours(24));

            context.Response.Body = original;
            await original.WriteAsync(body);
        }
        finally
        {
            cache.Remove(key + InFlight);
        }
    }

    private record CachedResponse(int StatusCode, string ContentType, byte[] Body);
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
        => app.UseMiddleware<IdempotencyMiddleware>();
}
```

---

## DI / Pipeline Registration

```csharp
// DependencyInjection.cs
services.AddMemoryCache(); // or AddStackExchangeRedisCache for multi-pod

// Program.cs — after UseGlobalExceptionHandler, before routing
app.UseIdempotency();
```

## Multi-Replica Upgrade Path

Replace `IMemoryCache` with `IDistributedCache`:

```csharp
// services.AddStackExchangeRedisCache(o => o.Configuration = "redis:6379");
// Then inject IDistributedCache instead of IMemoryCache in the middleware
```

---

## Behaviour Reference

| Scenario | HTTP Method | Key present | Status | Handler invoked |
|----------|-------------|-------------|--------|-----------------|
| First call | POST/PUT/PATCH | Yes | 2xx (from handler) | ✅ |
| Retry, completed | POST/PUT/PATCH | Yes (cached) | 2xx (replayed) | ❌ |
| Parallel duplicate | POST/PUT/PATCH | Yes (in-flight) | 409 | ❌ |
| No key | POST/PUT/PATCH | No | pass-through | ✅ |
| Safe method | GET/DELETE | Any | pass-through | ✅ |
| Handler error | POST/PUT/PATCH | Yes | 4xx/5xx (not cached) | ✅ |

---

## Related Patterns

- [ADR-0063](../../adrs/0063-idempotency-middleware.md)
- [CP-04: Multi-Tenant Repository with RLS](./cp-04-multitenant-repository-rls.md) — the outbox pattern pairs with idempotency for exactly-once domain event delivery
