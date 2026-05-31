# UMS TypeScript SDK — Quickstart

> **Language:** English | [Español](../../sdk-es/typescript/quickstart.md)

Get a Node handler protected by UMS authorization in **five minutes**. For NestJS, see the [NestJS Quickstart](../nestjs/quickstart.md). For the full reference, see [README.md](./README.md).

---

## Step 1 — Install packages

```bash
npm install @ums/sdk-authorization @ums/sdk-contracts
npm install --save-dev @ums/sdk-testing
```

---

## Step 2 — Set up the accessor and validator

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

## Step 3 — Populate the graph per request (Express example)

```ts
import express from "express";
import { parseAuthorizationGraph } from "@ums/sdk-authorization";
import { accessor } from "./auth";

const app = express();

app.use(async (req, _res, next) => {
  const token = req.headers.authorization?.replace(/^Bearer\s+/i, "");
  if (!token) return next();
  const graph = await parseAuthorizationGraph(token);   // calls /api/v1/client/authenticate if no cached
  accessor.run(graph, () => next());
});
```

The helper validates `schemaVersion` against the SDK's compatibility range and throws `AUTH_205` if incompatible — handle it as a 401 in your error middleware.

---

## Step 4 — Protect a handler

```ts
import { requireScope } from "@ums/sdk-authorization";

app.post(
  "/orders/:id/approve",
  requireScope("PURCHASE_ORDER.APPROVE", async (req, res) => {
    // business logic — runs only if authorized
    res.json({ ok: true });
  })
);
```

If the user lacks the scope, the handler throws `AuthorizationDeniedError` before running.

### Decorator style (TS 5.0+)

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

## Step 5 — Test it

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
  it("denies when scope is missing", async () => {
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .withScope("PURCHASE_ORDER.VIEW")   // VIEW only, no APPROVE
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

## Common adjustments

### Return a Result instead of throwing

```ts
const approveOrder = requireScope(
  "PURCHASE_ORDER.APPROVE",
  async (orderId: string): Promise<Result> => {
    return { ok: true, value: undefined };
  },
  { onDenied: "returnFailure" }
);
```

### Audit-only rollout

```ts
configureAuthorization({ accessor, validator, mode: "audit-only" });
```

### Browser/SPA accessor

```ts
import { MemoryAuthGraphAccessor } from "@ums/sdk-authorization";
const accessor = new MemoryAuthGraphAccessor();
accessor.set(graph);   // after login
// ...
accessor.clear();      // on logout
```

---

## Next Steps

- [Full Reference](./README.md)
- [NestJS Quickstart](../nestjs/quickstart.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
