# UMS NestJS SDK

> **Language:** English | [Español](../../sdk-es/nestjs/README.md)

Distribution: npm · Package: `@ums/sdk-nestjs` · Targets: NestJS 10+

This is the **NestJS distribution** of the UMS SDK. It is a thin adapter on top of [`@ums/sdk-authorization`](../typescript/README.md) — it does **not** reimplement the validator or any rule. It exposes the same four primitives as NestJS-idiomatic Decorators backed by a single `UmsAuthGuard`.

For a 5-minute integration, jump to [quickstart.md](./quickstart.md). See the [TypeScript README](../typescript/README.md) for the underlying validator semantics.

---

## 1. Why NestJS gets its own package

NestJS has first-class concepts that map cleanly to UMS authorization:

| NestJS concept | UMS SDK mapping |
|---|---|
| **Decorator** (`@RequiresScope`, etc.) | Attaches metadata via `Reflector` |
| **Guard** (`CanActivate`) | Reads metadata, queries `AuthorizationValidator`, returns true/false |
| **Module** (`UmsSdkModule`) | Configures accessor, validator, mode, and exports them |
| **Exception filter** | Maps `AuthorizationDeniedError` to `ForbiddenException` (HTTP 403) |
| **Request scope** | `AsyncLocalAuthGraphAccessor` integrates naturally with Nest's request lifecycle |

A NestJS consumer writes `@RequiresScope` on a route handler and the rest is wired up automatically.

---

## 2. Package Contents

```
@ums/sdk-nestjs
├── UmsSdkModule          ← .forRoot() / .forRootAsync() configuration entry
├── UmsAuthGuard          ← CanActivate — single guard for all four primitives
├── decorators/
│   ├── @RequiresScope
│   ├── @RequiresMenuOption
│   ├── @RequiresDomainAccess
│   └── @RequiresFeatureFlag
├── filters/
│   └── AuthorizationDeniedFilter   ← maps to ForbiddenException (HTTP 403)
└── middleware/
    └── AuthGraphMiddleware         ← parses JWT body, populates accessor
```

Dependencies declared: `@ums/sdk-authorization`, `@ums/sdk-contracts`, `@nestjs/common`, `@nestjs/core`.

---

## 3. Conceptual Flow

```
HTTP request
   │
   ▼
AuthGraphMiddleware           ← parses JWT, populates AsyncLocalAuthGraphAccessor
   │
   ▼
NestJS controller route
   │
   ▼
@UseGuards(UmsAuthGuard)
@RequiresScope("X.Y")
   │
   ▼
UmsAuthGuard.canActivate()    ← reads Reflector metadata, calls validator.requireScope()
   │
   ├── granted → controller handler runs
   └── denied → throws ForbiddenException (HTTP 403)
                or returns false (if guard configured that way)
                or logs only (audit-only mode)
```

The guard is **request-scoped** — it reads the accessor populated by the middleware for the current request. Multiple decorators on the same handler are evaluated in order; the first denial short-circuits.

---

## 4. Module Configuration

### 4.1 Synchronous

```ts
import { Module } from "@nestjs/common";
import { UmsSdkModule } from "@ums/sdk-nestjs";

@Module({
  imports: [
    UmsSdkModule.forRoot({
      umsBaseUrl: "https://ums.example.com",
      mode: "enforce",                  // or "audit-only"
      schemaCompatibility: ">=1.0.0 <2.0.0",
    }),
  ],
})
export class AppModule {}
```

### 4.2 Asynchronous (from ConfigService)

```ts
UmsSdkModule.forRootAsync({
  inject: [ConfigService],
  useFactory: (config: ConfigService) => ({
    umsBaseUrl: config.get("UMS_BASE_URL")!,
    mode: config.get("UMS_AUTH_MODE", "enforce"),
    schemaCompatibility: ">=1.0.0 <2.0.0",
  }),
});
```

### 4.3 Apply the middleware globally

```ts
import { MiddlewareConsumer, NestModule } from "@nestjs/common";
import { AuthGraphMiddleware } from "@ums/sdk-nestjs";

export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer) {
    consumer.apply(AuthGraphMiddleware).forRoutes("*");
  }
}
```

### 4.4 Apply the guard globally (or per-controller)

Global:

```ts
{
  provide: APP_GUARD,
  useClass: UmsAuthGuard,
}
```

Per-controller:

```ts
@Controller("orders")
@UseGuards(UmsAuthGuard)
export class OrdersController { ... }
```

When applied globally, decorator-free handlers are allowed through unmodified — the guard only enforces when a `@Requires*` decorator is present.

---

## 5. Decorator Reference

All four decorators carry the same shape: a target identifier + optional options object.

### `@RequiresScope`

```ts
@Post(":id/approve")
@RequiresScope("PURCHASE_ORDER.APPROVE")
async approveOrder(@Param("id") id: string): Promise<void> { ... }
```

### `@RequiresMenuOption`

```ts
@Patch("stock/:id")
@RequiresMenuOption("STOCK_ADJUST")
async adjustStock(@Param("id") id: string, @Body() body: StockAdjustment): Promise<void> { ... }
```

### `@RequiresDomainAccess`

```ts
@Get(":id")
@RequiresDomainAccess("PURCHASE_ORDER", "VIEW")
async getOrder(@Param("id") id: string): Promise<OrderDto> { ... }
```

### `@RequiresFeatureFlag`

```ts
@Get(":id/pick-list")
@RequiresFeatureFlag("WMS_NEW_PICKING_UI")
async getPickList(@Param("id") id: string): Promise<PickListDto> { ... }
```

### Combining decorators

```ts
@Post(":id/approve-and-pick")
@RequiresScope("PURCHASE_ORDER.APPROVE")
@RequiresFeatureFlag("WMS_NEW_PICKING_UI")
async approveAndPick(@Param("id") id: string): Promise<void> { ... }
```

Both must pass. First denial short-circuits.

### Options

```ts
@RequiresScope("X.Y", { auditOnly: true })   // override global mode for this handler
@RequiresScope("X.Y", { onDenied: "ignore" }) // log but never block, even in enforce mode
```

---

## 6. Custom Denial Handling

Default behavior: the guard throws `AuthorizationDeniedError`, which the `AuthorizationDeniedFilter` (auto-registered by `UmsSdkModule.forRoot`) maps to a Nest `ForbiddenException` (HTTP 403) with a structured error body:

```jsonc
{
  "statusCode": 403,
  "error": "Forbidden",
  "code": "AUTH_101",
  "message": "Scope 'PURCHASE_ORDER.APPROVE' not granted",
  "primitive": "RequiresScope",
  "target": "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "uuid"
}
```

To customize, provide your own filter:

```ts
@Catch(AuthorizationDeniedError)
export class MyDeniedFilter implements ExceptionFilter {
  catch(err: AuthorizationDeniedError, host: ArgumentsHost) {
    // custom mapping — e.g., redirect to /unauthorized for HTML requests
  }
}
```

---

## 7. Testing

`@ums/sdk-testing` is reused — no NestJS-specific test utilities are needed.

```ts
import { Test } from "@nestjs/testing";
import { UmsSdkModule, UmsAuthGuard, RequiresScope } from "@ums/sdk-nestjs";
import { AuthGraphBuilder } from "@ums/sdk-testing";

@Controller("orders")
class OrdersController {
  @Post(":id/approve")
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  approve() { return { ok: true }; }
}

describe("OrdersController", () => {
  it("denies approve without scope", async () => {
    const module = await Test.createTestingModule({
      imports: [UmsSdkModule.forRoot({ mode: "enforce" })],
      controllers: [OrdersController],
    }).compile();

    const app = module.createNestApplication();
    await app.init();

    // Inject a graph for the test request via a test-only middleware
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .build();   // no scopes

    // Use supertest:
    return request(app.getHttpServer())
      .post("/orders/abc/approve")
      .set("Authorization", `Bearer ${encodeFakeJwt(graph)}`)
      .expect(403)
      .expect((res) => {
        expect(res.body.code).toBe("AUTH_101");
      });
  });
});
```

---

## 8. Relationship to `@ums/sdk-authorization`

Everything in `@ums/sdk-nestjs` delegates to `@ums/sdk-authorization`:

- `UmsAuthGuard` calls `validator.requireScope(...)` (or the equivalent primitive method) from `@ums/sdk-authorization`.
- `@RequiresScope` and friends use NestJS's `Reflector` to attach metadata; the guard reads it and invokes the same HOF-equivalent rule.
- `AuthGraphMiddleware` uses the same `AsyncLocalAuthGraphAccessor` from `@ums/sdk-authorization`.

This is intentional: a NestJS consumer and a plain-TS Express consumer make identical authorization decisions because they share the validator literally.

---

## 9. References

- [Quickstart](./quickstart.md)
- [TypeScript SDK README](../typescript/README.md) — underlying validator and accessor
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.md)
