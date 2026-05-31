# UMS NestJS SDK — Quickstart

> **Language:** English | [Español](../../sdk-es/nestjs/quickstart.md)

Get a NestJS route protected by UMS authorization in **five minutes**. For full reference, see [README.md](./README.md).

---

## Step 1 — Install

```bash
npm install @ums/sdk-nestjs @ums/sdk-authorization @ums/sdk-contracts
npm install --save-dev @ums/sdk-testing
```

---

## Step 2 — Import the module

```ts
// app.module.ts
import { Module, MiddlewareConsumer, NestModule } from "@nestjs/common";
import { APP_GUARD, APP_FILTER } from "@nestjs/core";
import { UmsSdkModule, UmsAuthGuard, AuthGraphMiddleware,
         AuthorizationDeniedFilter } from "@ums/sdk-nestjs";

@Module({
  imports: [
    UmsSdkModule.forRoot({
      umsBaseUrl: process.env.UMS_BASE_URL!,
      mode: "enforce",                       // or "audit-only" while rolling out
      schemaCompatibility: ">=1.0.0 <2.0.0",
    }),
  ],
  providers: [
    { provide: APP_GUARD,  useClass: UmsAuthGuard },
    { provide: APP_FILTER, useClass: AuthorizationDeniedFilter },
  ],
})
export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer) {
    consumer.apply(AuthGraphMiddleware).forRoutes("*");
  }
}
```

That's the entire setup. From now on, any handler with a `@Requires*` decorator is protected.

---

## Step 3 — Protect a route

```ts
// orders.controller.ts
import { Controller, Post, Param } from "@nestjs/common";
import { RequiresScope } from "@ums/sdk-nestjs";

@Controller("orders")
export class OrdersController {
  @Post(":id/approve")
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  async approveOrder(@Param("id") id: string): Promise<{ ok: true }> {
    // business logic — runs only if authorized
    return { ok: true };
  }
}
```

If the authenticated user lacks `PURCHASE_ORDER.APPROVE`, the response is `403 Forbidden` with body:

```json
{
  "statusCode": 403,
  "error": "Forbidden",
  "code": "AUTH_101",
  "message": "Scope 'PURCHASE_ORDER.APPROVE' not granted",
  "primitive": "RequiresScope",
  "target": "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "..."
}
```

---

## Step 4 — Test it

```ts
import { Test } from "@nestjs/testing";
import { INestApplication } from "@nestjs/common";
import request from "supertest";
import { UmsSdkModule, AuthorizationDeniedFilter } from "@ums/sdk-nestjs";
import { AuthGraphBuilder } from "@ums/sdk-testing";
import { encodeFakeBearer } from "@ums/sdk-testing/nestjs";
import { OrdersController } from "./orders.controller";

describe("OrdersController (e2e)", () => {
  let app: INestApplication;

  beforeAll(async () => {
    const module = await Test.createTestingModule({
      imports: [UmsSdkModule.forRoot({ mode: "enforce" })],
      controllers: [OrdersController],
    }).compile();

    app = module.createNestApplication();
    await app.init();
  });

  it("denies approve without scope", async () => {
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .build();  // no scopes

    return request(app.getHttpServer())
      .post("/orders/abc/approve")
      .set("Authorization", `Bearer ${encodeFakeBearer(graph)}`)
      .expect(403)
      .expect((res) => expect(res.body.code).toBe("AUTH_101"));
  });

  it("allows approve with scope", async () => {
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .withScope("PURCHASE_ORDER.APPROVE")
      .build();

    return request(app.getHttpServer())
      .post("/orders/abc/approve")
      .set("Authorization", `Bearer ${encodeFakeBearer(graph)}`)
      .expect(201);
  });
});
```

---

## Common adjustments

### Combine multiple checks

```ts
@Post(":id/pick")
@RequiresScope("PURCHASE_ORDER.APPROVE")
@RequiresFeatureFlag("WMS_NEW_PICKING_UI")
async pickOrder(@Param("id") id: string) { ... }
```

Both must pass. First denial short-circuits.

### Audit-only rollout

```ts
UmsSdkModule.forRoot({ mode: "audit-only" });
```

Denials logged as `AuthorizationDeniedEvent`, requests pass through. Flip to `enforce` after cleanup.

### Different denial behavior per handler

```ts
@RequiresScope("X.Y", { auditOnly: true })  // never blocks this handler even in enforce mode
```

### Custom error mapping

Replace `AuthorizationDeniedFilter` with your own `@Catch(AuthorizationDeniedError)` filter.

---

## Next Steps

- [Full Reference](./README.md)
- [TypeScript SDK (underlying)](../typescript/README.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
