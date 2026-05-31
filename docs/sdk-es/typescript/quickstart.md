# UMS TypeScript SDK — Quickstart

> **Idioma:** [English](../../sdk/typescript/quickstart.md) | Español

Obtén un handler Node protegido por autorización UMS en **cinco minutos**. Para NestJS, ver el [Quickstart NestJS](../nestjs/quickstart.md). Para referencia completa, ver [README.md](./README.md).

---

## Paso 1 — Instalar paquetes

```bash
npm install @ums/sdk-authorization @ums/sdk-contracts
npm install --save-dev @ums/sdk-testing
```

---

## Paso 2 — Configurar el accessor y validator

```ts
import {
  AsyncLocalAuthGraphAccessor,
  AuthorizationValidator,
  configureAuthorization,
} from "@ums/sdk-authorization";

const accessor = new AsyncLocalAuthGraphAccessor();
const validator = new AuthorizationValidator();

configureAuthorization({ accessor, validator });

export { accessor };
```

---

## Paso 3 — Poblar el grafo por request (ejemplo Express)

```ts
import express from "express";
import { parseAuthorizationGraph } from "@ums/sdk-authorization";
import { accessor } from "./auth";

const app = express();

app.use(async (req, _res, next) => {
  const token = req.headers.authorization?.replace(/^Bearer\s+/i, "");
  if (!token) return next();
  const graph = await parseAuthorizationGraph(token);   // llama a /api/v1/client/authenticate si no hay cacheado
  accessor.run(graph, () => next());
});
```

El helper valida `schemaVersion` contra el rango de compatibilidad del SDK y lanza `AUTH_205` si es incompatible — manéjalo como un 401 en tu middleware de errores.

---

## Paso 4 — Proteger un handler

```ts
import { requireScope } from "@ums/sdk-authorization";

app.post(
  "/orders/:id/approve",
  requireScope("PURCHASE_ORDER.APPROVE", async (req, res) => {
    // lógica de negocio — corre solo si está autorizado
    res.json({ ok: true });
  })
);
```

Si el usuario no tiene el scope, el handler lanza `AuthorizationDeniedError` antes de correr.

### Estilo decorator (TS 5.0+)

```ts
import { RequiresScope } from "@ums/sdk-authorization";

class OrderService {
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  async approveOrder(orderId: string): Promise<void> {
    // ...
  }
}
```

---

## Paso 5 — Testearlo

```ts
import { describe, it, expect } from "vitest";
import { AuthGraphBuilder } from "@ums/sdk-testing";
import {
  MemoryAuthGraphAccessor,
  AuthorizationValidator,
  AuthorizationDeniedError,
  configureAuthorization,
} from "@ums/sdk-authorization";

describe("approveOrder", () => {
  it("deniega cuando falta el scope", async () => {
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .withScope("PURCHASE_ORDER.VIEW")   // solo VIEW, sin APPROVE
      .build();

    const accessor = new MemoryAuthGraphAccessor();
    accessor.set(graph);
    configureAuthorization({ accessor, validator: new AuthorizationValidator() });

    await expect(approveOrder("order-id"))
      .rejects.toThrow(AuthorizationDeniedError);
  });
});
```

---

## Ajustes comunes

### Retornar un Result en lugar de lanzar

```ts
const approveOrder = requireScope(
  "PURCHASE_ORDER.APPROVE",
  async (orderId: string): Promise<Result> => {
    return { ok: true, value: undefined };
  },
  { onDenied: "returnFailure" }
);
```

### Roll-out audit-only

```ts
configureAuthorization({ accessor, validator, mode: "audit-only" });
```

### Accessor para browser/SPA

```ts
import { MemoryAuthGraphAccessor } from "@ums/sdk-authorization";
const accessor = new MemoryAuthGraphAccessor();
accessor.set(graph);   // después del login
// ...
accessor.clear();      // al logout
```

---

## Próximos Pasos

- [Referencia Completa](./README.md)
- [Quickstart NestJS](../nestjs/quickstart.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
