# UMS TypeScript SDK

> **Idioma:** [English](../../sdk/typescript/README.md) | Español

Distribución: npm · Scope: `@ums/*` · Targets: Node 20+ y browsers modernos

Esta es la **distribución TypeScript** del UMS SDK. Es agnóstica de framework — usable desde Express, Fastify, servicios Node planos, SPAs browser y cualquier otro contexto TypeScript/JavaScript. Para NestJS, ver [`@ums/sdk-nestjs`](../nestjs/README.md), que agrega Guards y Decorators encima de esta distribución.

Consumidores JavaScript usan estos paquetes directamente (publican `.js` + `.d.ts`).

Para integración en 5 minutos, salta a [quickstart.md](./quickstart.md).

---

## 1. Familia de Paquetes

| Paquete | Propósito | Depende de |
|---|---|---|
| `@ums/sdk-contracts` | Tipos TypeScript generados desde `auth-graph.schema.json`, constantes de códigos de error | — |
| `@ums/sdk-authorization` | Validador puro, abstracción `AuthGraphAccessor`, accessors específicos por entorno | `@ums/sdk-contracts` |
| `@ums/sdk-testing` | `AuthGraphBuilder` para tests | `@ums/sdk-authorization` |

Dos accessors específicos por entorno vienen en `@ums/sdk-authorization`:

| Accessor | Entorno | Mecanismo subyacente |
|---|---|---|
| `AsyncLocalAuthGraphAccessor` | Node 20+ | `AsyncLocalStorage` |
| `MemoryAuthGraphAccessor` | Browser / SPA | Holder en memoria, scopeado manualmente por sesión |

Integraciones de framework (middleware Express, hook Fastify, Guard NestJS) son paquetes separados y viven fuera de Fase 1 — excepto NestJS, que es entregable de Fase 1.

---

## 2. Capas Conceptuales

```
tu código
   │
   ▼
requireScope("X.Y", handler)         ← HOF envolviendo un handler
o
@RequiresScope("X.Y")                ← decorator stage-3 en un método
   │
   ▼
AuthorizationAspect / Decorator      ← lee desde AuthGraphAccessor
   │
   ▼
AuthorizationValidator               ← reglas puras (deny-wins, override, expiry)
   │
   ▼
AuthorizationDecision                ← { granted, denied, expired, errorCode? }
   │
   ├── modo throw     → AuthorizationDeniedError
   └── modo return    → Result.failure("AUTH_xxx")
```

El validador es puro: `(graph, probe) => decision`. Igual que .NET — reusable desde cualquier parte.

---

## 3. `AuthGraphAccessor` — Cómo el SDK Encuentra el Grafo

```ts
export interface AuthGraphAccessor {
  current(): AuthorizationGraph | null;
}
```

### Node (ciclos de vida de request server-side)

```ts
import { AsyncLocalAuthGraphAccessor } from "@ums/sdk-authorization";

const accessor = new AsyncLocalAuthGraphAccessor();

// En tu handler / middleware de request:
accessor.run(loadedGraph, async () => {
  await someBusinessLogic(); // dentro de este scope, accessor.current() retorna loadedGraph
});
```

`AsyncLocalAuthGraphAccessor` usa `node:async_hooks.AsyncLocalStorage` — el mismo primitivo del que Express, Fastify y NestJS dependen para contexto scoped a request.

### Browser / SPA

```ts
import { MemoryAuthGraphAccessor } from "@ums/sdk-authorization";

const accessor = new MemoryAuthGraphAccessor();
accessor.set(loadedGraph);   // llamar después del login
// ... usar a través de la sesión
accessor.clear();            // al logout o expiración
```

Holder único en memoria. Adecuado para SPAs que operan en una sesión de usuario a la vez.

---

## 4. API de Autorización

Dos estilos equivalentes por primitivo — elige el que se ajuste a tu codebase.

### 4.1 Estilo Higher-Order Function (recomendado, funciona en todas partes)

```ts
import { requireScope, requireMenuOption, requireDomainAccess, requireFeatureFlag }
  from "@ums/sdk-authorization";

const approveOrder = requireScope(
  "PURCHASE_ORDER.APPROVE",
  async (orderId: string): Promise<Result> => {
    // lógica de negocio
    return Result.success();
  }
);
```

### 4.2 Estilo Decorator (decorators stage-3, métodos en clases)

```ts
import { RequiresScope } from "@ums/sdk-authorization";

class OrderService {
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  async approveOrder(orderId: string): Promise<Result> {
    return Result.success();
  }
}
```

Los decorators stage-3 son nativos en TS 5.0+ y soportados en NestJS vía el sistema de decorators existente. Para TS más viejo (< 5.0) o proyectos que deshabilitan decorators, usa el estilo HOF.

### 4.3 Cuatro primitivos

| HOF | Decorator | Sección del grafo |
|---|---|---|
| `requireScope("RESOURCE.ACTION", handler)` | `@RequiresScope("RESOURCE.ACTION")` | `scopes[]` |
| `requireMenuOption("CODE", handler)` | `@RequiresMenuOption("CODE")` | `menuAccess[].…options[]` |
| `requireDomainAccess("RESOURCE", "ACTION", handler)` | `@RequiresDomainAccess("RESOURCE", "ACTION")` | `domainPermissions[]` |
| `requireFeatureFlag("FLAG_CODE", handler)` | `@RequiresFeatureFlag("FLAG_CODE")` | `featureFlags[]` |

Todos aceptan un argumento opcional `options`:

```ts
requireScope("X.Y", handler, { onDenied: "returnFailure" });
@RequiresScope("X.Y", { onDenied: "throw" })
```

---

## 5. Wiring (ejemplo Node con Express)

```ts
import express from "express";
import {
  AsyncLocalAuthGraphAccessor,
  AuthorizationValidator,
  parseAuthorizationGraph,
} from "@ums/sdk-authorization";

const accessor = new AsyncLocalAuthGraphAccessor();
const validator = new AuthorizationValidator();

const app = express();

// Middleware que decodifica el body del JWT, parsea el grafo, y corre el request dentro del scope del accessor
app.use(async (req, res, next) => {
  const graph = parseAuthorizationGraph(req.headers.authorization!);
  accessor.run(graph, () => next());
});

// Configurar globals para las APIs HOF/decorator (o pasar explícitamente por llamada)
import { configureAuthorization } from "@ums/sdk-authorization";
configureAuthorization({ accessor, validator });

app.post("/orders/:id/approve", requireScope("PURCHASE_ORDER.APPROVE", async (req, res) => {
  // lógica de negocio
  res.json({ ok: true });
}));
```

---

## 6. Integración con el Patrón Result

`@ums/sdk-authorization` publica una unión discriminada `Result<T>` mínima compatible con librerías Result comunes de TS:

```ts
type Result<T = void, E = AuthorizationError> =
  | { ok: true; value: T }
  | { ok: false; error: E };
```

Cuando `onDenied: "returnFailure"`, el handler envuelto retorna `{ ok: false, error: { code: "AUTH_101", ... } }` en lugar de lanzar.

---

## 7. Modo Audit-Only

```ts
configureAuthorization({ accessor, validator, mode: "audit-only" });
```

Todas las denegaciones se emiten vía el logger que proveas (`pino`, `winston`, o un adapter custom). Los handlers corren sin bloqueo. Cambia a `mode: "enforce"` una vez que las denegaciones están limpias.

---

## 8. Probando Tu Código Consumidor

```ts
import { AuthGraphBuilder } from "@ums/sdk-testing";

test("approveOrder falla sin scope", async () => {
  const graph = AuthGraphBuilder
    .forTenant("LOGISTICS_CORE")
    .withUser("ana.flores@example.com")
    .withScope("PURCHASE_ORDER.VIEW")
    .build();

  const accessor = new MemoryAuthGraphAccessor();
  accessor.set(graph);
  configureAuthorization({ accessor, validator: new AuthorizationValidator() });

  await expect(approveOrder("order-id")).rejects.toThrow(AuthorizationDeniedError);
});
```

El builder es fluido, produce una instancia `AuthorizationGraph` completamente válida, sin parsing JSON, sin UMS.

---

## 9. Referencias

- [Quickstart](./quickstart.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
- [Política de Versionado](../contracts/versioning.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
- [README SDK NestJS](../nestjs/README.md)
