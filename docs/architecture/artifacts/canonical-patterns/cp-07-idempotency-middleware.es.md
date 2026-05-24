# CP-07: Middleware de Clave de Idempotencia

**Tipo:** Patrón Canónico  
**Estado:** Aceptado  
**Disposición Evolith:** Propuesto para Evolith — sin dependencias específicas del producto; portable a cualquier satélite ASP.NET Core  
**ADR relacionado:** [ADR-0063: Middleware de Idempotencia](../../adrs/0063-idempotency-middleware.md)

---

## Problema

Los endpoints HTTP mutantes (`POST`, `PUT`, `PATCH`) pueden ser llamados más de una vez por la misma operación lógica debido a reintentos de red, bugs del cliente o pasos de compensación de saga. Re-ejecutar el handler crea agregados duplicados o estado inconsistente.

---

## Patrón

Un middleware ASP.NET Core lee un header `Idempotency-Key` provisto por el cliente, ejecuta el pipeline en la primera llamada, cachea la respuesta y la reproduce literalmente en llamadas posteriores con la misma clave — sin re-invocar ningún handler.

```
Cliente                 Middleware                  Pipeline
──────                  ──────────                  ────────
POST /tenants           │
  Idempotency-Key: abc  │
                  ──►  │  ¿Clave "abc" conocida? No
                        │  Marcar "abc" como en vuelo
                        │  ──────────────────────►  Handler se ejecuta
                        │  ◄──────────────────────  Resultado
                        │  Cachear respuesta (24h)
                  ◄──   200 { tenantId: "..." }

POST /tenants (reintento)│
  Idempotency-Key: abc  │
                  ──►  │  ¿Clave "abc" conocida? Sí (completada)
                        │  Devolver respuesta cacheada (sin invocar handler)
                  ◄──   200 { tenantId: "..." }

POST /tenants (paralelo)│
  Idempotency-Key: abc  │
                  ──►  │  ¿Clave "abc" conocida? Sí (en vuelo)
                  ◄──   409 "request already in progress"
```

---

## Implementación

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

        // Verificación de duplicado paralelo
        if (cache.TryGetValue(key + InFlight, out _))
        {
            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(
                new { error = "request already in progress", idempotencyKey = key });
            return;
        }

        // Reproducir request completado
        if (cache.TryGetValue(key, out CachedResponse? cached))
        {
            context.Response.StatusCode  = cached!.StatusCode;
            context.Response.ContentType = cached.ContentType;
            await context.Response.Body.WriteAsync(cached.Body);
            return;
        }

        // Primera llamada — ejecutar y cachear
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

## Registro en DI / Pipeline

```csharp
// DependencyInjection.cs
services.AddMemoryCache(); // o AddStackExchangeRedisCache para multi-pod

// Program.cs — después de UseGlobalExceptionHandler, antes del routing
app.UseIdempotency();
```

## Ruta de Actualización Multi-Réplica

Reemplazar `IMemoryCache` con `IDistributedCache`:

```csharp
// services.AddStackExchangeRedisCache(o => o.Configuration = "redis:6379");
// Luego inyectar IDistributedCache en lugar de IMemoryCache en el middleware
```

---

## Referencia de Comportamiento

| Escenario | Método HTTP | Clave presente | Estado | Handler invocado |
|-----------|-------------|----------------|--------|-----------------|
| Primera llamada | POST/PUT/PATCH | Sí | 2xx (del handler) | ✅ |
| Reintento, completado | POST/PUT/PATCH | Sí (cacheada) | 2xx (reproducida) | ❌ |
| Duplicado paralelo | POST/PUT/PATCH | Sí (en vuelo) | 409 | ❌ |
| Sin clave | POST/PUT/PATCH | No | pasa | ✅ |
| Método seguro | GET/DELETE | Cualquiera | pasa | ✅ |
| Error del handler | POST/PUT/PATCH | Sí | 4xx/5xx (no cacheado) | ✅ |

---

## Patrones Relacionados

- [ADR-0063](../../adrs/0063-idempotency-middleware.md)
- [CP-04: Repositorio Multi-Tenant con RLS](./cp-04-multitenant-repository-rls.md) — el patrón outbox se combina con idempotencia para entrega exactly-once de domain events
