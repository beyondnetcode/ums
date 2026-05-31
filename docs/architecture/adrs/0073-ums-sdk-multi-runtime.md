# ADR-0073: UMS SDK — Multi-Runtime Client Integration Surface (.NET / TypeScript / NestJS)

**Status:** Accepted
**Date:** 2026-05-31
**Decision Owner:** Architecture
**Related:**
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md)
- [ADR-0060: AOP Cross-Cutting Concern Strategy](./0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [ADR-0071: Auth Graph Engine](./0071-auth-graph-engine.md)
- [ADR-0072: Dynamic Auth Method Resolution](./0072-dynamic-auth-method-resolution.md)
- [ADR-0074: Auth Graph Schema Versioning Policy](./0074-auth-graph-schema-versioning.md)

---

## Context

ADR-0071 established the `AuthorizationGraph` as the self-contained artifact UMS delivers to client systems after authentication. The graph is rich (context, authentication metadata, menus, domain permissions, feature flags, effective configuration, scopes) and is designed to be cached client-side for the session duration, eliminating per-request authorization queries to UMS.

In practice, every client system must:
1. Call `POST /api/v1/client/authenticate`, parse the response, store the JWT
2. Deserialize the graph and hold it in scoped storage
3. Apply the deny-wins / override-takes-precedence resolution rules every time it checks a permission
4. Re-evaluate `validUntil` before each use and trigger re-authentication when expired
5. Map the four graph sections (`scopes`, `menuAccess`, `domainPermissions`, `featureFlags`) onto their own authorization model

If every client implements this independently, the rules drift, denial semantics get implemented wrong, and the value of centralizing authorization in UMS erodes. The repeated boilerplate is non-trivial — particularly in service methods, where the same `if (graph.scopes.Contains("X.Y"))` pattern appears in hundreds of places.

UMS is consumed by systems written in **.NET (the platform's native runtime), TypeScript (both Node and browser), and NestJS (an opinionated TypeScript server framework with first-class decorator/guard support)**. Each ecosystem has idiomatic conventions for authorization (attributes, decorators, guards) that differ enough to make a single distribution impractical, but conceptually they implement the same model.

A unified Software Development Kit — published as a family of language-specific distributions sharing a single canonical contract — is the natural answer.

---

## Decision

### 1. Establish the UMS SDK as the official client integration surface

UMS will ship a Software Development Kit (SDK) covering **three runtimes** as first-class targets:

| Runtime | Distribution | Registry |
|---|---|---|
| .NET | `Ums.Sdk.*` | NuGet |
| TypeScript (Node + browser) | `@ums/sdk-*` | npm |
| NestJS | `@ums/sdk-nestjs` | npm |

JavaScript (without TypeScript) is covered implicitly: every `@ums/sdk-*` package publishes both `.js` and `.d.ts` artifacts, so a JS consumer imports exactly the same packages but loses compile-time typing. JS is **not** a first-class target with separate idiomatic APIs.

### 2. Contract-first: the AuthorizationGraph JSON Schema is the canonical contract

The contract between server (UMS) and SDKs is **not** any language's interface — it is the JSON Schema of the `AuthorizationGraph` payload defined in [`auth-graph.md` §8](../../domain/identity/auth-graph.md#8-json-response-example).

The schema, the canonical catalog of `AUTH_xxx` error codes, and a library of golden fixture JSONs are produced as language-neutral artifacts that **all three SDKs and the UMS server consume**. They live at:

```
src/libs/sdk/contracts/
├── auth-graph.schema.json     ← JSON Schema 2020-12
├── error-codes.yaml           ← canonical AUTH_xxx catalog
├── fixtures/                  ← golden JSON files
│   ├── local-auth-success.json
│   ├── idp-auth-success.json
│   ├── deny-wins.json
│   ├── override-allow.json
│   ├── empty-permissions.json
│   ├── expired-graph.json
│   ├── feature-flag-matched.json
│   ├── feature-flag-missed-context.json
│   └── multi-tenant-rejection.json
└── SCHEMA_VERSIONING.md       ← summary of ADR-0074
```

The UMS server's `AuthorizationGraphBuilderService` and every SDK's deserializer/validator must round-trip the same fixtures successfully. CI enforces this.

### 3. Live inside the UMS monorepo under `src/libs/sdk/`

The SDK does not get its own repository. It lives in the existing `ums/` monorepo as a sibling of `src/libs/shell/`:

```
src/libs/
├── shell/              ← reusable infrastructure (DDD, Factory, AOP, Bootstrapper)
└── sdk/                ← UMS product integration surface
    ├── contracts/
    ├── dotnet/
    ├── typescript/
    └── nestjs/
```

This co-location guarantees:
- Schema changes in the server and SDK can ship in a single PR — no two-repo drift.
- `Ums.Sdk.Authorization.Aop` references `Shell.Aop` via `<ProjectReference>` during development, eliminating NuGet round-trips while iterating.
- Golden fixtures are consumed by both `ums.api` tests (builder regression) and SDK tests (parser/validator regression) from one source.
- One ADR registry, one MASTER_INDEX, one CI pipeline, one bilingual docs policy.

**Extraction policy:** the SDK stays in the monorepo until a concrete, documented reason justifies splitting it out (e.g., third-party contributors, independent release cadence demanded by external consumers, repository size). Extraction is explicitly **not** anticipated for v1.

### 4. Package layout per runtime

Each runtime ships a small family of focused packages. Inter-package dependencies form a DAG rooted at `Contracts`.

**.NET (NuGet, namespace `Ums.Sdk.*`):**

| Package | Depends on | Purpose |
|---|---|---|
| `Ums.Sdk.Contracts` | — | DTOs mirroring the graph schema, error code constants |
| `Ums.Sdk.Authorization` | Contracts | Pure validator (deny-wins, override, expiry), `IAuthGraphAccessor` port |
| `Ums.Sdk.Authorization.Aop` | Authorization, `Shell.Aop` | Attributes (`[RequiresScope]` etc.) + aspect over DispatchProxy |
| `Ums.Sdk.Authorization.Testing` | Authorization | `AuthGraphBuilder` fluent API for unit tests of consumer code |

**TypeScript (npm, scope `@ums`):**

| Package | Depends on | Purpose |
|---|---|---|
| `@ums/sdk-contracts` | — | Types generated from JSON Schema, error code constants |
| `@ums/sdk-authorization` | sdk-contracts | Pure validator, accessor abstraction (Node `AsyncLocalStorage` and browser-friendly variants) |
| `@ums/sdk-testing` | sdk-authorization | Graph builder for tests |

**NestJS (npm, extends TypeScript):**

| Package | Depends on | Purpose |
|---|---|---|
| `@ums/sdk-nestjs` | `@ums/sdk-authorization`, `@nestjs/common` | `UmsSdkModule`, `UmsAuthGuard` (`CanActivate`), decorators using `Reflector` metadata |

Phase 1 explicitly scopes out: HTTP client packages (`Ums.Sdk.Client`, `@ums/sdk-client`), ASP.NET integration (`Ums.Sdk.Authorization.AspNetCore`), Express middleware. They are valid Phase 2 deliverables.

### 5. Common conceptual model — four authorization attributes / decorators

Every runtime exposes the same four authorization primitives, mapped one-to-one to the four authorization-bearing sections of the graph:

| Primitive | Graph section | Allow rule |
|---|---|---|
| `RequiresScope("RESOURCE.ACTION")` | `scopes[]` | Scope present in `scopes[]` and **not** explicitly Denied |
| `RequiresMenuOption("OPTION_CODE")` | `menuAccess[]…options[]` | Option found with `effect = "Allow"` |
| `RequiresDomainAccess("RESOURCE", "ACTION")` | `domainPermissions[]` | Resource+action found with `effect = "Allow"` |
| `RequiresFeatureFlag("FLAG_CODE")` | `featureFlags[]` | Flag found with `isEnabled = true` |

Universal pre-check (applies to all four): if `graph.validUntil < now`, the decision is `Expired` regardless of content. Deny always beats Allow (Axiom A3 — same rule as the server-side resolution).

### 6. Denial behavior is configurable per attribute

Two modes, selectable per attribute use site:

- **`Throw`** (default) — raises a runtime exception (`UnauthorizedAccessException` in .NET, `ForbiddenException` in NestJS, throws an `AuthorizationDeniedError` in TypeScript). Standard for endpoint guards and high-stakes operations.
- **`ReturnFailure`** — when the method's return type is `Result` (or `Result<T>` / `Promise<Result<T>>`), the aspect/guard returns `Result.Failure("AUTH_403", reason)` instead of throwing. Standard for application-service boundaries that already use the Result pattern.

A global `AuthorizationValidator.Mode = AuditOnly` flag bypasses denial entirely and only logs structured `AuthorizationDeniedEvent` records — used for progressive rollouts.

### 7. Versioning — schema and packages versioned independently

- The graph schema is versioned per ADR-0074 (semver, strict-on-major, permissive-on-minor).
- Each SDK package is versioned independently. The schema version a package targets is declared in package metadata (`schemaCompatibility: ">=1.0.0 <2.0.0"`).
- The graph payload carries `schemaVersion` (e.g., `"1.0.0"`) — SDKs reject incompatible majors and warn on unknown minors.

NuGet uses `Ums.Sdk.<Pkg>` versions; npm uses `@ums/sdk-<pkg>` versions. They do **not** need to share numbers across registries — each ecosystem has its own release cadence.

### 8. Bilingual documentation, mirroring the rest of UMS docs

All SDK documentation under `docs/sdk/` is published in both English and Spanish, following the same pattern as the rest of UMS (`*.md` for EN, `*.es.md` or `docs/sdk-es/` for ES depending on convention chosen in the portal). The MASTER_INDEX gets a new top-level entry **Phase 06 — UMS SDK** alongside Architecture / Domain / Governance / Operations.

---

## Rationale

### Why an SDK and not just published docs

The graph contract is documented today in markdown (auth-graph.md §8) — but markdown is not enforceable. As soon as the server's serializer adds a field or renames one, clients break silently unless they have a tight integration loop. An SDK turns the contract into:

- A machine-readable schema validated by CI.
- Typed DTOs in three runtimes generated from that schema.
- A canonical validator implementation that encodes the deny-wins/override rules once and is reused everywhere.

Docs remain — but they describe the SDK's behavior, not the wire contract directly.

### Why three runtimes and not just .NET

UMS is consumed by the platform's own .NET applications **and** by external NestJS services and TypeScript-based BFFs/SPAs. Restricting the SDK to .NET leaves the larger fraction of integration surface uncovered. Three runtimes is honest scope: .NET (platform-native), TypeScript (broad ecosystem coverage including JS), NestJS (highest-leverage server framework in the TS ecosystem, with decorators/guards that map directly to the conceptual model).

### Why contract-first instead of API-first

API-first means each runtime designs its own ergonomic API, and the union of those APIs implicitly defines the contract. This works at first and drifts inevitably: small differences in error code naming, in the shape of effective config, in nullable handling. Contract-first inverts the dependency: the schema is the source of truth, language APIs are projections of it. CI enforces parity by round-tripping fixtures through every SDK.

### Why monorepo placement under `src/libs/sdk/`

Two failure modes were considered:

1. **Separate repo (`ums-sdk/`):** clean separation but high coordination cost. Schema changes require two PRs and a coordinated release. Drift is a question of when, not if.
2. **Inside `src/libs/sdk/`:** schema, server consumer, and three SDK consumers all reachable by one CI run. Single ADR registry. One bilingual docs policy. Coordination cost is zero for changes that span server and SDK.

Option 2 wins decisively for v1. Extraction stays an option for the future, gated on concrete demand.

### Why NestJS extends TypeScript instead of being independent

NestJS is implemented on TypeScript, uses TypeScript's type system, and consumes TypeScript libraries natively. Treating `@ums/sdk-nestjs` as a thin adapter on top of `@ums/sdk-authorization` (Guards + Decorators that delegate to the framework-agnostic validator) reduces the surface to design and maintain, and guarantees behavioral parity between a "plain TS" consumer and a "NestJS" consumer — they share a validator literally.

### Why JavaScript is implicit, not first-class

A first-class JS API would mean designing decorator-free ergonomics (HOFs, middleware) — a meaningful but ancillary surface. The TS packages already publish `.js` + `.d.ts`, so JS consumers can use them with full functionality, only losing compile-time type checks. The cost/benefit of a separate JS-idiomatic API does not justify Phase 1 inclusion.

---

## Consequences

### Positive

- The graph contract becomes a versioned product artifact, not implicit knowledge.
- Authorization rules (deny-wins, override-precedence, expiry) are implemented once per runtime and reused everywhere — no risk of client code implementing them wrong.
- New client systems can integrate in minutes by following a quickstart, not by reading ADR-0071 and rebuilding the wheel.
- The same fixtures used to validate the server's `AuthorizationGraphBuilderService` validate every SDK's parser — a single round-trip CI job covers the entire contract.
- Future runtimes (Python, Go, Java) can be added incrementally by adopting the same contract; the schema is language-neutral.
- Audit denial events emitted by SDKs can correlate to UMS audit trail through `requestId`, closing the observability loop.

### Trade-offs

- The team takes on a public API responsibility. Breaking changes need deprecation periods, migration guides and major-version bumps — significantly higher engineering rigor than ad-hoc internal libraries.
- The schema becomes the binding contract. Adding fields requires the policy from ADR-0074 to be followed, slowing down server-side experimentation in the graph shape.
- Three runtimes require three build pipelines, three test stacks (xUnit, vitest, jest) and three registries. CI complexity grows.
- Documentation surface grows substantially — every change has to be reflected in 3 runtime guides × 2 languages plus contracts docs.

### Neutral

- Existing `Shell.Aop` does **not** change. `Ums.Sdk.Authorization.Aop` consumes it as-is.
- ADR-0071 and ADR-0072 do not change; this ADR depends on them.
- The MASTER_INDEX gets a new top-level entry but the existing five-pillar structure is preserved.

---

## Implementation

### Phase A — Documentation (precedes any code)

| Deliverable | Path |
|---|---|
| ADR-0073 (EN + ES) | `docs/architecture/adrs/0073-ums-sdk-multi-runtime{.es,}.md` |
| ADR-0074 (EN + ES) | `docs/architecture/adrs/0074-auth-graph-schema-versioning{.es,}.md` |
| SDK portal index | `docs/sdk/index.md` + `docs/sdk/index.es.md` |
| Contract docs | `docs/sdk/contracts/{schema-overview,error-codes,versioning,fixtures}.md` (+ `.es.md`) |
| .NET SDK guide | `docs/sdk/dotnet/README.md` + quickstart (+ `.es.md`) |
| TypeScript SDK guide | `docs/sdk/typescript/README.md` + quickstart (+ `.es.md`) |
| NestJS SDK guide | `docs/sdk/nestjs/README.md` + quickstart (+ `.es.md`) |
| Index updates | `MASTER_INDEX.md`, `MASTER_INDEX.es.md`, `architecture/adrs/index.md` |
| Cross-links | `domain/identity/auth-graph.md` references the SDK portal |

### Phase B — Implementation (after Phase A is reviewed and merged)

| Deliverable | Path |
|---|---|
| Contract artifacts | `src/libs/sdk/contracts/` |
| .NET packages | `src/libs/sdk/dotnet/Ums.Sdk.*/` (4 csproj + tests) |
| TypeScript packages | `src/libs/sdk/typescript/sdk-*/` (3 npm packages + tests) |
| NestJS package | `src/libs/sdk/nestjs/sdk-nestjs/` (1 npm package + tests) |
| CI integration | Workflow validates fixtures round-trip through all SDKs |
| Server alignment | `ums.api` emits `schemaVersion` and references `Ums.Sdk.Contracts` DTOs |

---

**[ADR Registry](./index.md)**
