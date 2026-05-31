# UMS NestJS SDK — Quickstart

> **Idioma:** [English](../../sdk/nestjs/quickstart.md) | Español

Obtén una ruta NestJS protegida por autorización UMS en **cinco minutos**. Para referencia completa, ver [README.md](./README.md).

---

## Paso 1 — Instalar

```bash
npm install @ums/sdk-nestjs @ums/sdk-authorization @ums/sdk-contracts
npm install --save-dev @ums/sdk-testing
```

---

## Paso 2 — Importar el módulo

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
      mode: "enforce",                       // o "audit-only" mientras se hace roll-out
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

Esa es toda la configuración. A partir de ahora, cualquier handler con un decorator `@Requires*` está protegido.

---

## Paso 3 — Proteger una ruta

```ts
// orders.controller.ts
import { Controller, Post, Param } from "@nestjs/common";
import { RequiresScope } from "@ums/sdk-nestjs";

@Controller("orders")
export class OrdersController {
  @Post(":id/approve")
  @RequiresScope("PURCHASE_ORDER.APPROVE")
  async approveOrder(@Param("id") id: string): Promise<{ ok: true }> {
    // lógica de negocio — corre solo si está autorizado
    return { ok: true };
  }
}
```

Si el usuario autenticado no tiene `PURCHASE_ORDER.APPROVE`, la respuesta es `403 Forbidden` con body:

```json
{
  "statusCode": 403,
  "error": "Forbidden",
  "code": "AUTH_101",
  "message": "Scope 'PURCHASE_ORDER.APPROVE' no concedido",
  "primitive": "RequiresScope",
  "target": "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "..."
}
```

---

## Paso 4 — Testearlo

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

  it("deniega approve sin scope", async () => {
    const graph = AuthGraphBuilder
      .forTenant("LOGISTICS_CORE")
      .withUser("ana.flores@example.com")
      .build();  // sin scopes

    return request(app.getHttpServer())
      .post("/orders/abc/approve")
      .set("Authorization", `Bearer ${encodeFakeBearer(graph)}`)
      .expect(403)
      .expect((res) => expect(res.body.code).toBe("AUTH_101"));
  });

  it("permite approve con scope", async () => {
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

## Ajustes comunes

### Combinar múltiples checks

```ts
@Post(":id/pick")
@RequiresScope("PURCHASE_ORDER.APPROVE")
@RequiresFeatureFlag("WMS_NEW_PICKING_UI")
async pickOrder(@Param("id") id: string) { ... }
```

Ambos deben pasar. La primera denegación short-circuit.

### Roll-out audit-only

```ts
UmsSdkModule.forRoot({ mode: "audit-only" });
```

Denegaciones logueadas como `AuthorizationDeniedEvent`, requests pasan. Cambia a `enforce` después de la limpieza.

### Comportamiento de denegación diferente por handler

```ts
@RequiresScope("X.Y", { auditOnly: true })  // nunca bloquea este handler ni en modo enforce
```

### Mapeo custom de errores

Reemplaza `AuthorizationDeniedFilter` con tu propio filter `@Catch(AuthorizationDeniedError)`.

---

## Próximos Pasos

- [Referencia Completa](./README.md)
- [SDK TypeScript (subyacente)](../typescript/README.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
