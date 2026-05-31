# ADR-0073: UMS SDK — Superficie de Integración Cliente Multi-Runtime (.NET / TypeScript / NestJS)

**Estado:** Aceptado
**Fecha:** 2026-05-31
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md)
- [ADR-0060: Estrategia de Concerns Cross-Cutting con AOP](./0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [ADR-0071: Motor del Grafo de Autorización](./0071-auth-graph-engine.es.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](./0072-dynamic-auth-method-resolution.es.md)
- [ADR-0074: Política de Versionado del Schema del Grafo de Autorización](./0074-auth-graph-schema-versioning.es.md)

---

## Contexto

ADR-0071 estableció el `AuthorizationGraph` como el artefacto autocontenido que UMS entrega a los sistemas cliente tras la autenticación. El grafo es rico (contexto, metadata de autenticación, menús, permisos de dominio, feature flags, configuración efectiva, scopes) y está diseñado para ser cacheado en el cliente durante la vigencia de la sesión, eliminando consultas de autorización por request a UMS.

En la práctica, cada sistema cliente debe:
1. Llamar a `POST /api/v1/client/authenticate`, parsear la respuesta y almacenar el JWT
2. Deserializar el grafo y mantenerlo en almacenamiento con scope de sesión
3. Aplicar las reglas de resolución deny-wins / override-takes-precedence cada vez que verifica un permiso
4. Re-evaluar `validUntil` antes de cada uso y disparar re-autenticación al expirar
5. Mapear las cuatro secciones del grafo (`scopes`, `menuAccess`, `domainPermissions`, `featureFlags`) sobre su propio modelo de autorización

Si cada cliente implementa esto de forma independiente, las reglas divergen, la semántica de denegación se implementa mal y el valor de centralizar la autorización en UMS se erosiona. El boilerplate repetido es no trivial — particularmente en métodos de servicios, donde el mismo patrón `if (graph.scopes.Contains("X.Y"))` aparece en cientos de lugares.

UMS es consumido por sistemas escritos en **.NET (el runtime nativo de la plataforma), TypeScript (Node y browser), y NestJS (un framework opinionado de TypeScript con soporte first-class para decorators y guards)**. Cada ecosistema tiene convenciones idiomáticas para autorización (atributos, decorators, guards) que difieren lo suficiente como para hacer impráctica una sola distribución, pero conceptualmente implementan el mismo modelo.

Un Software Development Kit unificado — publicado como una familia de distribuciones específicas por lenguaje compartiendo un contrato canónico único — es la respuesta natural.

---

## Decisión

### 1. Establecer el UMS SDK como la superficie oficial de integración cliente

UMS publicará un Software Development Kit (SDK) cubriendo **tres runtimes** como targets de primera clase:

| Runtime | Distribución | Registro |
|---|---|---|
| .NET | `Ums.Sdk.*` | NuGet |
| TypeScript (Node + browser) | `@ums/sdk-*` | npm |
| NestJS | `@ums/sdk-nestjs` | npm |

JavaScript (sin TypeScript) queda cubierto implícitamente: cada paquete `@ums/sdk-*` publica tanto artefactos `.js` como `.d.ts`, por lo que un consumidor JS importa exactamente los mismos paquetes pero pierde el typing en compile-time. JS **no** es un target de primera clase con APIs idiomáticas separadas.

### 2. Contract-first: el JSON Schema del AuthorizationGraph es el contrato canónico

El contrato entre el servidor (UMS) y los SDKs **no** es la interfaz de ningún lenguaje — es el JSON Schema del payload `AuthorizationGraph` definido en [`auth-graph.md` §8](../../domain-es/identity/auth-graph.md#8-ejemplo-json-de-respuesta).

El schema, el catálogo canónico de códigos de error `AUTH_xxx`, y una librería de JSONs golden fixture se producen como artefactos agnósticos de lenguaje que **los tres SDKs y el servidor UMS consumen**. Viven en:

```
src/libs/sdk/contracts/
├── auth-graph.schema.json     ← JSON Schema 2020-12
├── error-codes.yaml           ← catálogo canónico AUTH_xxx
├── fixtures/                  ← archivos JSON golden
│   ├── local-auth-success.json
│   ├── idp-auth-success.json
│   ├── deny-wins.json
│   ├── override-allow.json
│   ├── empty-permissions.json
│   ├── expired-graph.json
│   ├── feature-flag-matched.json
│   ├── feature-flag-missed-context.json
│   └── multi-tenant-rejection.json
└── SCHEMA_VERSIONING.md       ← resumen de ADR-0074
```

El `AuthorizationGraphBuilderService` del servidor UMS y el deserializador/validador de cada SDK deben hacer round-trip exitoso sobre las mismas fixtures. CI lo enforce.

### 3. Vivir dentro del monorepo UMS bajo `src/libs/sdk/`

El SDK no tiene su propio repositorio. Vive en el monorepo existente `ums/` como hermano de `src/libs/shell/`:

```
src/libs/
├── shell/              ← infraestructura reutilizable (DDD, Factory, AOP, Bootstrapper)
└── sdk/                ← superficie de integración del producto UMS
    ├── contracts/
    ├── dotnet/
    ├── typescript/
    └── nestjs/
```

Esta co-locación garantiza:
- Cambios de schema en el servidor y SDK pueden ir en un solo PR — sin drift entre dos repos.
- `Ums.Sdk.Authorization.Aop` referencia a `Shell.Aop` vía `<ProjectReference>` durante desarrollo, eliminando round-trips por NuGet mientras se itera.
- Las golden fixtures son consumidas tanto por tests de `ums.api` (regresión del builder) como por tests de SDK (regresión de parser/validador) desde una sola fuente.
- Un solo registro ADR, un solo MASTER_INDEX, un solo pipeline CI, una sola política bilingüe de docs.

**Política de extracción:** el SDK permanece en el monorepo hasta que una razón concreta y documentada justifique separarlo (ej. contribuyentes externos, cadencia de release independiente exigida por consumidores externos, tamaño del repositorio). La extracción **no** se anticipa para v1.

### 4. Layout de paquetes por runtime

Cada runtime publica una familia pequeña de paquetes enfocados. Las dependencias entre paquetes forman un DAG con raíz en `Contracts`.

**.NET (NuGet, namespace `Ums.Sdk.*`):**

| Paquete | Depende de | Propósito |
|---|---|---|
| `Ums.Sdk.Contracts` | — | DTOs que reflejan el schema del grafo, constantes de códigos de error |
| `Ums.Sdk.Authorization` | Contracts | Validador puro (deny-wins, override, expiry), puerto `IAuthGraphAccessor` |
| `Ums.Sdk.Authorization.Aop` | Authorization, `Shell.Aop` | Atributos (`[RequiresScope]` etc.) + aspecto sobre DispatchProxy |
| `Ums.Sdk.Authorization.Testing` | Authorization | API fluida `AuthGraphBuilder` para tests unitarios de código consumidor |

**TypeScript (npm, scope `@ums`):**

| Paquete | Depende de | Propósito |
|---|---|---|
| `@ums/sdk-contracts` | — | Tipos generados desde JSON Schema, constantes de códigos de error |
| `@ums/sdk-authorization` | sdk-contracts | Validador puro, abstracción de accessor (Node `AsyncLocalStorage` y variantes browser-friendly) |
| `@ums/sdk-testing` | sdk-authorization | Graph builder para tests |

**NestJS (npm, extiende TypeScript):**

| Paquete | Depende de | Propósito |
|---|---|---|
| `@ums/sdk-nestjs` | `@ums/sdk-authorization`, `@nestjs/common` | `UmsSdkModule`, `UmsAuthGuard` (`CanActivate`), decorators usando metadata `Reflector` |

La Fase 1 explícitamente excluye: paquetes cliente HTTP (`Ums.Sdk.Client`, `@ums/sdk-client`), integración ASP.NET (`Ums.Sdk.Authorization.AspNetCore`), middleware Express. Son entregables válidos de Fase 2.

### 5. Modelo conceptual común — cuatro atributos / decorators de autorización

Cada runtime expone los mismos cuatro primitivos de autorización, mapeados uno-a-uno a las cuatro secciones del grafo que portan autorización:

| Primitivo | Sección del grafo | Regla de Allow |
|---|---|---|
| `RequiresScope("RESOURCE.ACTION")` | `scopes[]` | Scope presente en `scopes[]` y **no** denegado explícitamente |
| `RequiresMenuOption("OPTION_CODE")` | `menuAccess[]…options[]` | Opción encontrada con `effect = "Allow"` |
| `RequiresDomainAccess("RESOURCE", "ACTION")` | `domainPermissions[]` | Recurso+acción encontrado con `effect = "Allow"` |
| `RequiresFeatureFlag("FLAG_CODE")` | `featureFlags[]` | Flag encontrado con `isEnabled = true` |

Pre-check universal (aplica a las cuatro): si `graph.validUntil < now`, la decisión es `Expired` independientemente del contenido. Deny siempre gana sobre Allow (Axioma A3 — misma regla que la resolución server-side).

### 6. El comportamiento ante denegación es configurable por atributo

Dos modos, seleccionables por sitio de uso del atributo:

- **`Throw`** (default) — lanza una excepción de runtime (`UnauthorizedAccessException` en .NET, `ForbiddenException` en NestJS, lanza un `AuthorizationDeniedError` en TypeScript). Standard para guardas de endpoint y operaciones críticas.
- **`ReturnFailure`** — cuando el tipo de retorno del método es `Result` (o `Result<T>` / `Promise<Result<T>>`), el aspecto/guard retorna `Result.Failure("AUTH_403", reason)` en lugar de lanzar. Standard para bordes de application services que ya usan el patrón Result.

Un flag global `AuthorizationValidator.Mode = AuditOnly` salta la denegación por completo y solo registra eventos estructurados `AuthorizationDeniedEvent` — usado para roll-outs progresivos.

### 7. Versionado — schema y paquetes versionados de forma independiente

- El schema del grafo se versiona según ADR-0074 (semver, estricto en major, permisivo en minor).
- Cada paquete del SDK se versiona de forma independiente. La versión del schema que un paquete soporta se declara en la metadata del paquete (`schemaCompatibility: ">=1.0.0 <2.0.0"`).
- El payload del grafo lleva `schemaVersion` (ej. `"1.0.0"`) — los SDKs rechazan majors incompatibles y advierten sobre minors desconocidos.

NuGet usa versiones `Ums.Sdk.<Pkg>`; npm usa versiones `@ums/sdk-<pkg>`. **No** necesitan compartir números entre registros — cada ecosistema tiene su propia cadencia de release.

### 8. Documentación bilingüe, reflejando el resto de docs UMS

Toda la documentación del SDK bajo `docs/sdk/` se publica en inglés y español, siguiendo el mismo patrón que el resto de UMS (`*.md` para EN, `*.es.md` o `docs/sdk-es/` para ES según convención elegida en el portal). El MASTER_INDEX recibe una nueva entrada top-level **Fase 06 — UMS SDK** al lado de Architecture / Domain / Governance / Operations.

---

## Justificación

### Por qué un SDK y no solo docs publicadas

El contrato del grafo está documentado hoy en markdown (auth-graph.md §8) — pero el markdown no es enforceable. En cuanto el serializador del servidor agrega un campo o renombra uno, los clientes rompen silenciosamente a menos que tengan un loop de integración tight. Un SDK convierte el contrato en:

- Un schema machine-readable validado por CI.
- DTOs tipados en tres runtimes generados desde ese schema.
- Una implementación canónica del validador que codifica las reglas deny-wins/override una vez y se reutiliza en todas partes.

Los docs permanecen — pero describen el comportamiento del SDK, no el contrato wire directamente.

### Por qué tres runtimes y no solo .NET

UMS es consumido por las propias aplicaciones .NET de la plataforma **y** por servicios externos NestJS y BFFs/SPAs basados en TypeScript. Restringir el SDK a .NET deja sin cubrir la mayor fracción de superficie de integración. Tres runtimes es alcance honesto: .NET (nativo a la plataforma), TypeScript (cobertura amplia del ecosistema incluyendo JS), NestJS (framework server de mayor apalancamiento en el ecosistema TS, con decorators/guards que mapean directamente al modelo conceptual).

### Por qué contract-first en lugar de API-first

API-first significa que cada runtime diseña su propia API ergonómica, y la unión de esas APIs define implícitamente el contrato. Esto funciona al inicio y diverge inevitablemente: pequeñas diferencias en nombrado de códigos de error, en la forma de effective config, en manejo de nullables. Contract-first invierte la dependencia: el schema es la fuente de verdad, las APIs de lenguaje son proyecciones de ese schema. CI enforce paridad haciendo round-trip de fixtures a través de cada SDK.

### Por qué placement en monorepo bajo `src/libs/sdk/`

Se consideraron dos modos de falla:

1. **Repo separado (`ums-sdk/`):** separación limpia pero costo alto de coordinación. Cambios de schema requieren dos PRs y un release coordinado. El drift es cuestión de cuándo, no de si.
2. **Dentro de `src/libs/sdk/`:** schema, consumidor servidor y tres consumidores SDK alcanzables por un solo run de CI. Un solo registro ADR. Una sola política bilingüe de docs. Costo de coordinación cero para cambios que abarcan servidor y SDK.

Opción 2 gana decisivamente para v1. La extracción permanece como opción a futuro, gobernada por demanda concreta.

### Por qué NestJS extiende TypeScript en lugar de ser independiente

NestJS está implementado sobre TypeScript, usa el sistema de tipos de TypeScript y consume librerías TypeScript nativamente. Tratar a `@ums/sdk-nestjs` como un adaptador delgado sobre `@ums/sdk-authorization` (Guards + Decorators que delegan al validador agnóstico del framework) reduce la superficie a diseñar y mantener, y garantiza paridad de comportamiento entre un consumidor "TS plano" y un consumidor "NestJS" — comparten validador literalmente.

### Por qué JavaScript es implícito, no first-class

Una API JS first-class significaría diseñar ergonomía sin decorators (HOFs, middleware) — una superficie significativa pero ancillary. Los paquetes TS ya publican `.js` + `.d.ts`, así que consumidores JS pueden usarlos con funcionalidad completa, solo perdiendo type checks en compile-time. El costo/beneficio de una API separada idiomática para JS no justifica inclusión en Fase 1.

---

## Consecuencias

### Positivas

- El contrato del grafo se vuelve un artefacto de producto versionado, no conocimiento implícito.
- Las reglas de autorización (deny-wins, override-precedence, expiry) están implementadas una vez por runtime y reutilizadas en todas partes — sin riesgo de que código cliente las implemente mal.
- Sistemas cliente nuevos pueden integrarse en minutos siguiendo un quickstart, no leyendo ADR-0071 y reconstruyendo la rueda.
- Las mismas fixtures que validan el `AuthorizationGraphBuilderService` del servidor validan el parser de cada SDK — un único job CI de round-trip cubre todo el contrato.
- Runtimes futuros (Python, Go, Java) pueden agregarse incrementalmente adoptando el mismo contrato; el schema es agnóstico al lenguaje.
- Eventos de denegación de auditoría emitidos por los SDKs pueden correlacionar al audit trail UMS a través de `requestId`, cerrando el loop de observabilidad.

### Compromisos

- El equipo asume una responsabilidad de API pública. Cambios breaking necesitan períodos de deprecación, guías de migración y bumps de major version — rigor de ingeniería significativamente mayor que librerías internas ad-hoc.
- El schema se vuelve contrato vinculante. Agregar campos requiere seguir la política de ADR-0074, ralentizando experimentación server-side en la forma del grafo.
- Tres runtimes requieren tres pipelines de build, tres stacks de test (xUnit, vitest, jest) y tres registros. La complejidad CI crece.
- La superficie de documentación crece sustancialmente — cada cambio debe reflejarse en 3 guías de runtime × 2 idiomas más docs de contratos.

### Neutrales

- `Shell.Aop` existente **no** cambia. `Ums.Sdk.Authorization.Aop` lo consume tal cual.
- ADR-0071 y ADR-0072 no cambian; este ADR depende de ellos.
- El MASTER_INDEX recibe una nueva entrada top-level pero la estructura existente de cinco pilares se preserva.

---

## Implementación

### Fase A — Documentación (precede a cualquier código)

| Entregable | Ruta |
|---|---|
| ADR-0073 (EN + ES) | `docs/architecture/adrs/0073-ums-sdk-multi-runtime{.es,}.md` |
| ADR-0074 (EN + ES) | `docs/architecture/adrs/0074-auth-graph-schema-versioning{.es,}.md` |
| Portal index SDK | `docs/sdk/index.md` + `docs/sdk/index.es.md` |
| Docs de contratos | `docs/sdk/contracts/{schema-overview,error-codes,versioning,fixtures}.md` (+ `.es.md`) |
| Guía .NET SDK | `docs/sdk/dotnet/README.md` + quickstart (+ `.es.md`) |
| Guía TypeScript SDK | `docs/sdk/typescript/README.md` + quickstart (+ `.es.md`) |
| Guía NestJS SDK | `docs/sdk/nestjs/README.md` + quickstart (+ `.es.md`) |
| Updates de índices | `MASTER_INDEX.md`, `MASTER_INDEX.es.md`, `architecture/adrs/index.md` |
| Cross-links | `domain/identity/auth-graph.md` referencia el portal SDK |

### Fase B — Implementación (después de Fase A revisada y mergeada)

| Entregable | Ruta |
|---|---|
| Artefactos de contrato | `src/libs/sdk/contracts/` |
| Paquetes .NET | `src/libs/sdk/dotnet/Ums.Sdk.*/` (4 csproj + tests) |
| Paquetes TypeScript | `src/libs/sdk/typescript/sdk-*/` (3 paquetes npm + tests) |
| Paquete NestJS | `src/libs/sdk/nestjs/sdk-nestjs/` (1 paquete npm + tests) |
| Integración CI | Workflow valida round-trip de fixtures por todos los SDKs |
| Alineación servidor | `ums.api` emite `schemaVersion` y referencia DTOs `Ums.Sdk.Contracts` |

---

**[Registro ADR](./index.md)**
