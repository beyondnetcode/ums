# ADR-0063: Middleware de Clave de Idempotencia (FIX-06 / RISK-05)

**Estado:** Aceptado  
**Fecha:** 2026-05-24  
**Responsable:** Arquitectura  
**Disposición Evolith:** Propuesto para adopción en Evolith — la deduplicación de requests a nivel HTTP es neutra en cuanto al runtime; aplicable a cualquier satélite ASP.NET Core  
**Relacionados:**
- [ADR-0051: Event Bus — Puerto Injectable](./0051-event-bus-injectable-port.md)
- [CP-07: Middleware de Clave de Idempotencia](../artifacts/canonical-patterns/cp-07-idempotency-middleware.es.md)

---

## Contexto

Los command handlers de UMS son operaciones que mutan estado: crean agregados, actualizan estado, publican domain events al outbox e interactúan con proveedores de identidad externos. Un reintento de red, un doble toque desde un cliente móvil, o un paso de compensación de saga que re-emite un comando puede ejecutar la misma operación dos veces — creando tenants duplicados, cargos dobles, o estado en conflicto.

La capa de dominio hace cumplir las invariantes de negocio (p.ej., "el código de tenant ya existe"), pero devolver un error de dominio en la segunda llamada idéntica no siempre es el comportamiento correcto. El cliente puede esperar la misma respuesta exitosa que en la primera llamada.

### Por qué middleware en vez de un behavior de MediatR

Un `IPipelineBehavior` de MediatR se ejecuta después de la deserialización y validación — requiere un store de idempotencia persistente por tipo de handler. El middleware a nivel HTTP:

1. Intercepta antes de la deserialización — la caché de respuestas puede devolverse sin ejecutar el pipeline completo
2. Es agnóstico al handler — un único registro cubre todos los endpoints
3. Opera sobre la respuesta HTTP como buffer de bytes — la respuesta es idéntica a la original, incluyendo headers y código de estado

---

## Decisión

**Implementar la deduplicación de requests como un middleware ASP.NET Core que lee el header `Idempotency-Key`, cachea la primera respuesta y la reproduce literalmente para requests duplicados.**

### Matriz de comportamiento

| Escenario | Respuesta |
|-----------|-----------|
| Sin header `Idempotency-Key` | Pasa — la clave es opcional |
| Clave nueva, primer request | Ejecuta el pipeline, cachea la respuesta (TTL: 24h), devuelve resultado |
| Clave conocida, request completado | Devuelve la respuesta cacheada inmediatamente — handler NO invocado |
| Clave conocida, request en vuelo | Devuelve HTTP 409 "request already in progress" |
| Método no mutante (GET, DELETE) | Pasa — idempotente por naturaleza |

### Métodos cubiertos

Solo `POST`, `PUT`, `PATCH`. `GET` y `DELETE` pasan incondicionalmente.

### Backend de caché

`IMemoryCache` (por defecto nodo único). Para despliegues multi-réplica, reemplazar con `IDistributedCache` (Redis o SQL Server) para compartir estado entre pods.

### Formato de clave

UUID v4 generado por el cliente, p.ej. `550e8400-e29b-41d4-a716-446655440000`. El middleware no genera claves — el cliente es responsable de generarlas y reintentar con la misma clave.

### TTL

24 horas (configurable vía `IdempotencyOptions`). Tras la expiración del TTL, una clave re-enviada se trata como un nuevo request.

### Registro en DI

```csharp
// Program.cs / DependencyInjection
services.AddMemoryCache();      // requerido para IdempotencyStore de nodo único
app.UseIdempotency();           // debe ir después de UseCorrelationId, antes del routing
```

### Posición en el pipeline de middleware

```
UseCorrelationId
  → UseSessionTracking
    → UseGlobalExceptionHandler
      → UseIdempotency          ← aquí
        → UseRateLimiter
          → Routes
```

La posición después de `UseGlobalExceptionHandler` garantiza que las excepciones durante la ejecución del pipeline sean capturadas y no cacheadas. La posición antes del routing garantiza que la reproducción ocurra antes de la selección del endpoint.

---

## Consecuencias

### Positivas

- Los requests HTTP duplicados devuelven respuestas idénticas — transparente para los clientes
- La lógica de negocio del handler se ejecuta exactamente una vez por operación lógica independientemente de los reintentos de red
- Sin boilerplate por handler — un middleware cubre todos los endpoints mutantes
- Se combina naturalmente con el patrón outbox: si el handler se ejecutó y confirmó el mensaje de outbox, la respuesta cacheada se devuelve; el domain event no se re-publica

### Compromisos

- La caché en memoria no se comparte entre réplicas del pod — un reintento que llega a un pod diferente se re-ejecutará. Mitigar con `IDistributedCache` respaldado por Redis en producción
- El cuerpo de la respuesta se cachea como array de bytes — las respuestas grandes consumen memoria proporcionalmente al número de claves activas únicas
- El middleware cachea solo respuestas `2xx` — las respuestas de error no se cachean (el cliente debe reintentar en caso de fallo con la misma clave)
- `Idempotency-Key` es un concepto a nivel de request; no puede prevenir eventos duplicados si el mismo comando es emitido con claves diferentes por un cliente con mal comportamiento

---

## Checklist de Extracción Evolith

- [ ] `IdempotencyMiddleware` — sin import específico de UMS; depende solo de `IMemoryCache` y abstracciones de ASP.NET Core
- [ ] `IdempotencyOptions` — POCO simple con TTL y flag habilitado/deshabilitado
- [ ] Método de extensión `UseIdempotency()`

---

**[Registro ADR](./index.md)** | **[CP-07 Idempotencia](../artifacts/canonical-patterns/cp-07-idempotency-middleware.es.md)**
