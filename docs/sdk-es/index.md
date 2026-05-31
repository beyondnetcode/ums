# UMS SDK — Portal

> **Idioma:** [English](../sdk/index.md) | Español

El **UMS SDK** es la superficie oficial de integración cliente del User Management System. Empaqueta todo lo que un sistema cliente necesita para autenticarse contra UMS, consumir el `AuthorizationGraph`, y enforce decisiones de autorización localmente — a través de tres runtimes que comparten un único contrato canónico.

**Estado:** Fase de documentación (Fase A) — la implementación comienza después de que la documentación sea revisada y mergeada.

---

## Lo que el SDK te da

Después de que un sistema cliente se autentica con UMS, recibe un `AuthorizationGraph` (ver [ADR-0071](../architecture/adrs/0071-auth-graph-engine.es.md) y [`auth-graph.md`](../domain-es/identity/auth-graph.md)). El SDK provee:

- **Deserialización tipada** del grafo en los tipos idiomáticos de tu runtime.
- **Un validador puro** que aplica las reglas deny-wins / override-takes-precedence y el check de expiración `validUntil` — las mismas reglas implementadas idénticamente a través de runtimes.
- **Autorización declarativa** vía atributos (.NET) o decorators (TypeScript / NestJS) para los cuatro primitivos canónicos:
  - `RequiresScope("RESOURCE.ACTION")`
  - `RequiresMenuOption("OPTION_CODE")`
  - `RequiresDomainAccess("RESOURCE", "ACTION")`
  - `RequiresFeatureFlag("FLAG_CODE")`
- **Utilidades de testing** para construir grafos falsos para tests unitarios de código consumidor sin levantar UMS.

---

## Layout

```
docs/sdk-es/                     ← documentación (Español)
├── index.md                     ← este archivo
├── contracts/                   ← contrato canónico (agnóstico al lenguaje)
│   ├── schema-overview.md
│   ├── error-codes.md
│   ├── versioning.md
│   ├── fixtures.md
│   └── compatibility-matrix.md
├── dotnet/                      ← Guía SDK .NET
│   ├── README.md
│   └── quickstart.md
├── typescript/                  ← Guía SDK TypeScript
│   ├── README.md
│   └── quickstart.md
└── nestjs/                      ← Guía SDK NestJS
    ├── README.md
    └── quickstart.md
```

El espejo en inglés vive bajo [`docs/sdk/`](../sdk/index.md). El código fuente vive en `src/libs/sdk/` (separado de la documentación — ver [ADR-0073](../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)).

---

## Distribuciones

| Distribución | Registro | Paquetes | Estado |
|---|---|---|---|
| **.NET** | NuGet | `Ums.Sdk.Contracts`, `Ums.Sdk.Authorization`, `Ums.Sdk.Authorization.Aop`, `Ums.Sdk.Authorization.Testing` | Documentación |
| **TypeScript** | npm | `@ums/sdk-contracts`, `@ums/sdk-authorization`, `@ums/sdk-testing` | Documentación |
| **NestJS** | npm | `@ums/sdk-nestjs` (extiende `@ums/sdk-authorization`) | Documentación |

Consumidores JavaScript (sin TypeScript) usan los paquetes `@ums/*` directamente — publican tanto `.js` como `.d.ts`, así que JS funciona con funcionalidad completa, solo perdiendo typing en compile-time.

---

## Enlaces Rápidos

### Para integradores

- **Empezar aquí:** [Quickstart .NET](./dotnet/quickstart.md) · [Quickstart TypeScript](./typescript/quickstart.md) · [Quickstart NestJS](./nestjs/quickstart.md)
- **Entender el grafo:** [Grafo de Autorización](../domain-es/identity/auth-graph.md) · [Schema Overview](./contracts/schema-overview.md)
- **Códigos de error:** [Catálogo `AUTH_xxx`](./contracts/error-codes.md)
- **Compatibilidad:** [Matriz de Compatibilidad](./contracts/compatibility-matrix.md)

### Para contribuyentes del SDK

- **Arquitectura:** [ADR-0073](../architecture/adrs/0073-ums-sdk-multi-runtime.es.md) · [ADR-0074](../architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- **Contrato:** [Schema Overview](./contracts/schema-overview.md) · [Política de Versionado](./contracts/versioning.md) · [Fixtures](./contracts/fixtures.md)
- **Código fuente:** `src/libs/sdk/`

---

## Modelo Conceptual

Los tres runtimes implementan los mismos cuatro primitivos de autorización, mapeados uno-a-uno a las cuatro secciones del grafo que portan autorización:

| Primitivo | Sección del grafo | Regla de decisión |
|---|---|---|
| `RequiresScope` | `scopes[]` | Scope presente Y no en denies |
| `RequiresMenuOption` | `menuAccess[].…options[]` | Opción resuelve a `effect = "Allow"` |
| `RequiresDomainAccess` | `domainPermissions[]` | Recurso+acción resuelve a `effect = "Allow"` |
| `RequiresFeatureFlag` | `featureFlags[]` | Flag encontrado con `isEnabled = true` |

Pre-check universal: si `graph.validUntil < now`, la decisión es `Expired` independientemente del contenido. Deny siempre gana sobre Allow (Axioma A3).

---

## Referencias

- [ADR-0071: Motor del Grafo de Autorización](../architecture/adrs/0071-auth-graph-engine.es.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](../architecture/adrs/0072-dynamic-auth-method-resolution.es.md)
- [ADR-0073: UMS SDK Multi-Runtime](../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
- [ADR-0074: Política de Versionado de Schema](../architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- [Grafo de Autorización (doc de dominio)](../domain-es/identity/auth-graph.md)
- [Resolución del Método de Autenticación (doc de dominio)](../domain-es/identity/auth-method-resolution.md)
- [Índice Maestro](../MASTER_INDEX.es.md)
