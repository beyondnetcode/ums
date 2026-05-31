# UMS NestJS SDK

> **Idioma:** [English](../../sdk/nestjs/README.md) | Español

Distribución: npm · Paquete: `@ums/sdk-nestjs` · Target: NestJS 10+

Esta es la **distribución NestJS** del UMS SDK. Es un adaptador delgado sobre [`@ums/sdk-authorization`](../typescript/README.md) — **no** reimplementa el validador ni ninguna regla. Expone los mismos cuatro primitivos como Decorators idiomáticos de NestJS respaldados por un único `UmsAuthGuard`.

Para integración en 5 minutos, salta a [quickstart.md](./quickstart.md). Ver el [README TypeScript](../typescript/README.md) para la semántica subyacente del validador y accessor.

---

## 1. Por qué NestJS tiene su propio paquete

NestJS tiene conceptos de primera clase que mapean limpiamente a la autorización UMS:

| Concepto NestJS | Mapeo UMS SDK |
|---|---|
| **Decorator** (`@RequiresScope`, etc.) | Adjunta metadata vía `Reflector` |
| **Guard** (`CanActivate`) | Lee metadata, consulta `AuthorizationValidator`, retorna true/false |
| **Module** (`UmsSdkModule`) | Configura accessor, validator, mode, y los exporta |
| **Exception filter** | Mapea `AuthorizationDeniedError` a `ForbiddenException` (HTTP 403) |
| **Request scope** | `AsyncLocalAuthGraphAccessor` se integra naturalmente con el ciclo de vida de request de Nest |

Un consumidor NestJS escribe `@RequiresScope` en un route handler y el resto se conecta automáticamente.

---

## 2. Contenido del Paquete

```
@ums/sdk-nestjs
├── UmsSdkModule          ← entrada de configuración .forRoot() / .forRootAsync()
├── UmsAuthGuard          ← CanActivate — guard único para los cuatro primitivos
├── decorators/
│   ├── @RequiresScope
│   ├── @RequiresMenuOption
│   ├── @RequiresDomainAccess
│   └── @RequiresFeatureFlag
├── filters/
│   └── AuthorizationDeniedFilter   ← mapea a ForbiddenException (HTTP 403)
└── middleware/
    └── AuthGraphMiddleware         ← parsea body del JWT, pobla accessor
```

Dependencias declaradas: `@ums/sdk-authorization`, `@ums/sdk-contracts`, `@nestjs/common`, `@nestjs/core`.

---

## 3. Flujo Conceptual

```
HTTP request
   │
   ▼
AuthGraphMiddleware           ← parsea JWT, pobla AsyncLocalAuthGraphAccessor
   │
   ▼
Ruta del controller NestJS
   │
   ▼
@UseGuards(UmsAuthGuard)
@RequiresScope("X.Y")
   │
   ▼
UmsAuthGuard.canActivate()    ← lee metadata Reflector, llama a validator.requireScope()
   │
   ├── granted → corre handler del controller
   └── denied → lanza ForbiddenException (HTTP 403)
                o retorna false (si el guard está configurado así)
                o solo loguea (modo audit-only)
```

El guard es **request-scoped** — lee el accessor poblado por el middleware para el request actual. Múltiples decorators en el mismo handler se evalúan en orden; la primera denegación short-circuit.

---

## 4. Configuración del Módulo

### 4.1 Síncrona

```ts
import { Module } from "@nestjs/common";
import { UmsSdkModule } from "@ums/sdk-nestjs";

@Module({
  imports: [
    UmsSdkModule.forRoot({
      umsBaseUrl: "https://ums.example.com",
      mode: "enforce",                  // o "audit-only"
      schemaCompatibility: ">=1.0.0 <2.0.0",
    }),
  ],
})
export class AppModule {}
```

### 4.2 Asíncrona (desde ConfigService)

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

### 4.3 Aplicar el middleware globalmente

```ts
import { MiddlewareConsumer, NestModule } from "@nestjs/common";
import { AuthGraphMiddleware } from "@ums/sdk-nestjs";

export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer) {
    consumer.apply(AuthGraphMiddleware).forRoutes("*");
  }
}
```

### 4.4 Aplicar el guard globalmente (o por controller)

Global:

```ts
{
  provide: APP_GUARD,
  useClass: UmsAuthGuard,
}
```

Por controller:

```ts
@Controller("orders")
@UseGuards(UmsAuthGuard)
export class OrdersController { ... }
```

Cuando se aplica globalmente, handlers sin decorator pasan sin modificación — el guard solo enforce cuando un decorator `@Requires*` está presente.

---

## 5. Referencia de Decorators

Los cuatro decorators tienen la misma forma: un identificador de target + objeto opcional de options.

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

### Combinando decorators

```ts
@Post(":id/approve-and-pick")
@RequiresScope("PURCHASE_ORDER.APPROVE")
@RequiresFeatureFlag("WMS_NEW_PICKING_UI")
async approveAndPick(@Param("id") id: string): Promise<void> { ... }
```

Ambos deben pasar. La primera denegación short-circuit.

### Options

```ts
@RequiresScope("X.Y", { auditOnly: true })   // sobreescribe modo global para este handler
@RequiresScope("X.Y", { onDenied: "ignore" }) // logea pero nunca bloquea, incluso en modo enforce
```

---

## 6. Manejo Custom de Denegación

Comportamiento default: el guard lanza `AuthorizationDeniedError`, que el `AuthorizationDeniedFilter` (auto-registrado por `UmsSdkModule.forRoot`) mapea a una `ForbiddenException` de Nest (HTTP 403) con un body de error estructurado:

```jsonc
{
  "statusCode": 403,
  "error": "Forbidden",
  "code": "AUTH_101",
  "message": "Scope 'PURCHASE_ORDER.APPROVE' no concedido",
  "primitive": "RequiresScope",
  "target": "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "uuid"
}
```

Para customizar, provee tu propio filter:

```ts
@Catch(AuthorizationDeniedError)
export class MyDeniedFilter implements ExceptionFilter {
  catch(err: AuthorizationDeniedError, host: ArgumentsHost) {
    // mapeo custom — ej. redirect a /unauthorized para requests HTML
  }
}
```

---

## 7. Testing

`@ums/sdk-testing` se reutiliza — no se necesitan utilidades de test específicas para NestJS.

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
  it("deniega approve sin scope", async () => {
    const module = await Test.createTestingModule({
      imports: [UmsSdkModule.forRoot({ mode: "enforce" })],
      controllers: [OrdersController],
    }).compile();

    const app = module.createNestApplication();
    await app.init();

    // Inyectar un grafo para el request de test vía un middleware solo-test
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .build();   // sin scopes

    // Usando supertest:
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

## 8. Relación con `@ums/sdk-authorization`

Todo en `@ums/sdk-nestjs` delega a `@ums/sdk-authorization`:

- `UmsAuthGuard` llama a `validator.requireScope(...)` (o el método primitivo equivalente) desde `@ums/sdk-authorization`.
- `@RequiresScope` y similares usan `Reflector` de NestJS para adjuntar metadata; el guard la lee e invoca la misma regla equivalente a HOF.
- `AuthGraphMiddleware` usa el mismo `AsyncLocalAuthGraphAccessor` desde `@ums/sdk-authorization`.

Esto es intencional: un consumidor NestJS y un consumidor TS plano de Express toman decisiones de autorización idénticas porque comparten el validador literalmente.

---

## 9. Referencias

- [Quickstart](./quickstart.md)
- [README SDK TypeScript](../typescript/README.md) — validador y accessor subyacentes
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
