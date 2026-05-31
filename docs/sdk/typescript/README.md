# UMS TypeScript SDK

> **Language:** English | [Español](../../sdk-es/typescript/README.md)

Distribution: npm · Scope: `@ums/*` · Targets: Node 20+ and modern browsers

This is the **TypeScript distribution** of the UMS SDK. It is framework-agnostic — usable from Express, Fastify, plain Node services, browser SPAs and any other TypeScript/JavaScript context. For NestJS, see [`@ums/sdk-nestjs`](../nestjs/README.md), which adds Guards and Decorators on top of this distribution.

JavaScript consumers use these packages directly (they ship `.js` + `.d.ts`).

For a 5-minute integration, jump to [quickstart.md](./quickstart.md).

---

## 1. Package Family

| Package | Purpose | Depends on |
|---|---|---|
| `@ums/sdk-contracts` | TypeScript types generated from `auth-graph.schema.json`, error code constants | — |
| `@ums/sdk-authorization` | Pure validator, `AuthGraphAccessor` abstraction, environment-specific accessors | `@ums/sdk-contracts` |
| `@ums/sdk-testing` | `AuthGraphBuilder` for tests | `@ums/sdk-authorization` |

Two environment-specific accessors ship in `@ums/sdk-authorization`:

| Accessor | Environment | Underlying mechanism |
|---|---|---|
| `AsyncLocalAuthGraphAccessor` | Node 20+ | `AsyncLocalStorage` |
| `MemoryAuthGraphAccessor` | Browser / SPA | In-memory holder, manually scoped per session |

Framework integrations (Express middleware, Fastify hook, NestJS Guard) are separate packages and live outside Phase 1 — except NestJS, which is a Phase 1 deliverable.

---

## 2. Conceptual Layers

```
your code
   │
   ▼
requireScope("X.Y", handler)         ← HOF wrapping a handler
or
@RequiresScope("X.Y")                ← stage-3 decorator on a method
   │
   ▼
AuthorizationAspect / Decorator      ← reads from AuthGraphAccessor
   │
   ▼
AuthorizationValidator               ← pure rules (deny-wins, override, expiry)
   │
   ▼
AuthorizationDecision                ← { granted, denied, expired, errorCode? }
   │
   ├── throw mode     → AuthorizationDeniedError
   └── return mode    → Result.failure("AUTH_xxx")
```

The validator is pure: `(graph, probe) => decision`. Same as .NET — reusable from anywhere.

---

## 3. `AuthGraphAccessor` — How the SDK Finds the Graph

```ts
export interface AuthGraphAccessor {
  current(): AuthorizationGraph | null;
}
```

### Node (server-side request lifecycles)

```ts
import { AsyncLocalAuthGraphAccessor } from "@ums/sdk-authorization";

const accessor = new AsyncLocalAuthGraphAccessor();

// In your request handler / middleware:
accessor.run(loadedGraph, async () => {
  await someBusinessLogic(); // inside this scope, accessor.current() returns loadedGraph
});
```

`AsyncLocalAuthGraphAccessor` uses `node:async_hooks.AsyncLocalStorage` — the same primitive Express, Fastify and NestJS rely on for request-scoped context.

### Browser / SPA

```ts
import { MemoryAuthGraphAccessor } from "@ums/sdk-authorization";

const accessor = new MemoryAuthGraphAccessor();
accessor.set(loadedGraph);   // call after login
// ... use throughout the session
accessor.clear();            // on logout or expiry
```

Single in-memory holder. Suitable for SPAs that operate on one user session at a time.

---

## 4. Authorization API

Two equivalent styles per primitive — pick the one that fits your codebase.

### 4.1 Higher-Order Function Style (recommended, works everywhere)

```ts
import { requireScope, requireMenuOption, requireDomainAccess, requireFeatureFlag }
  from "@ums/sdk-authorization";

const approveOrder = requireScope(
  "PURCHASE_ORDER.APPROVE",
  async (orderId: string): Promise<Result> => {
    // business logic
    return Result.success();
  }
);
```

### 4.2 Decorator Style (stage-3 decorators, methods on classes)

```ts
import { RequiresScope } from "@ums/sdk-authorization";

class OrderService {
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  async approveOrder(orderId: string): Promise<Result> {
    return Result.success();
  }
}
```

Stage-3 decorators are native in TS 5.0+ and supported in NestJS via the existing decorator system. For older TS (< 5.0) or projects that disable decorators, use the HOF style.

### 4.3 Four primitives

| HOF | Decorator | Graph section |
|---|---|---|
| `requireScope("RESOURCE.ACTION", handler)` | `@RequiresScope("RESOURCE.ACTION")` | `scopes[]` |
| `requireMenuOption("CODE", handler)` | `@RequiresMenuOption("CODE")` | `menuAccess[].…options[]` |
| `requireDomainAccess("RESOURCE", "ACTION", handler)` | `@RequiresDomainAccess("RESOURCE", "ACTION")` | `domainPermissions[]` |
| `requireFeatureFlag("FLAG_CODE", handler)` | `@RequiresFeatureFlag("FLAG_CODE")` | `featureFlags[]` |

All accept an optional `options` argument:

```ts
requireScope("X.Y", handler, { onDenied: "returnFailure" });
@RequiresScope("X.Y", { onDenied: "throw" })
```

---

## 5. Wiring Up (Node example with Express)

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

// Middleware that decodes the JWT body, parses the graph, and runs the request inside the accessor scope
app.use(async (req, res, next) => {
  const graph = parseAuthorizationGraph(req.headers.authorization!);
  accessor.run(graph, () => next());
});

// Configure globals for the HOF/decorator APIs (or pass explicitly per call)
import { configureAuthorization } from "@ums/sdk-authorization";
configureAuthorization({ accessor, validator });

app.post("/orders/:id/approve", requireScope("PURCHASE_ORDER.APPROVE", async (req, res) => {
  // business logic
  res.json({ ok: true });
}));
```

---

## 6. Result Pattern Integration

`@ums/sdk-authorization` ships a minimal `Result<T>` discriminated union compatible with common TS Result libraries:

```ts
type Result<T = void, E = AuthorizationError> =
  | { ok: true; value: T }
  | { ok: false; error: E };
```

When `onDenied: "returnFailure"`, the wrapped handler returns `{ ok: false, error: { code: "AUTH_101", ... } }` instead of throwing.

---

## 7. Audit-Only Mode

```ts
configureAuthorization({ accessor, validator, mode: "audit-only" });
```

All denials are emitted via the logger you provide (`pino`, `winston`, or a custom adapter). Handlers run unblocked. Switch to `mode: "enforce"` once denials are cleaned up.

---

## 8. Testing Your Consumer Code

```ts
import { AuthGraphBuilder } from "@ums/sdk-testing";

test("approveOrder fails without scope", async () => {
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

Builder is fluent, produces a fully valid `AuthorizationGraph` instance, no JSON parsing, no UMS.

---

## 9. References

- [Quickstart](./quickstart.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
- [Versioning Policy](../contracts/versioning.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.md)
- [NestJS SDK README](../nestjs/README.md)
