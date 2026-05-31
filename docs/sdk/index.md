# UMS SDK — Portal

> **Language:** English | [Español](../sdk-es/index.md)

The **UMS SDK** is the official client integration surface for the User Management System. It packages everything a client system needs to authenticate against UMS, consume the `AuthorizationGraph`, and enforce authorization decisions locally — across three runtimes that share a single canonical contract.

**Status:** Documentation phase (Phase A) — implementation begins after documentation is reviewed and merged.

---

## What the SDK gives you

After a client system authenticates with UMS, it receives an `AuthorizationGraph` (see [ADR-0071](../architecture/adrs/0071-auth-graph-engine.md) and [`auth-graph.md`](../domain/identity/auth-graph.md)). The SDK provides:

- **Typed deserialization** of the graph in your runtime's idiomatic types.
- **A pure validator** that applies the deny-wins / override-takes-precedence rules and `validUntil` expiry check — the same rules implemented identically across runtimes.
- **Declarative authorization** via attributes (.NET) or decorators (TypeScript / NestJS) for the four canonical primitives:
  - `RequiresScope("RESOURCE.ACTION")`
  - `RequiresMenuOption("OPTION_CODE")`
  - `RequiresDomainAccess("RESOURCE", "ACTION")`
  - `RequiresFeatureFlag("FLAG_CODE")`
- **Testing utilities** to build fake graphs for unit tests of consumer code without spinning up UMS.

---

## Layout

```
docs/sdk/                        ← documentation (English)
├── index.md                     ← this file
├── contracts/                   ← canonical contract (language-neutral)
│   ├── schema-overview.md
│   ├── error-codes.md
│   ├── versioning.md
│   ├── fixtures.md
│   └── compatibility-matrix.md
├── dotnet/                      ← .NET SDK guide
│   ├── README.md
│   └── quickstart.md
├── typescript/                  ← TypeScript SDK guide
│   ├── README.md
│   └── quickstart.md
└── nestjs/                      ← NestJS SDK guide
    ├── README.md
    └── quickstart.md
```

Spanish mirror lives under [`docs/sdk-es/`](../sdk-es/index.md). Source code lives at `src/libs/sdk/` (separate from documentation — see [ADR-0073](../architecture/adrs/0073-ums-sdk-multi-runtime.md)).

---

## Distributions

| Distribution | Registry | Packages | Status |
|---|---|---|---|
| **.NET** | NuGet | `Ums.Sdk.Contracts`, `Ums.Sdk.Authorization`, `Ums.Sdk.Authorization.Aop`, `Ums.Sdk.Authorization.Testing` | Documentation |
| **TypeScript** | npm | `@ums/sdk-contracts`, `@ums/sdk-authorization`, `@ums/sdk-testing` | Documentation |
| **NestJS** | npm | `@ums/sdk-nestjs` (extends `@ums/sdk-authorization`) | Documentation |

JavaScript consumers (no TypeScript) use the `@ums/*` packages directly — they ship both `.js` and `.d.ts`, so JS works with full functionality, only losing compile-time typing.

---

## Quick Links

### For integrators

- **Start here:** [.NET Quickstart](./dotnet/quickstart.md) · [TypeScript Quickstart](./typescript/quickstart.md) · [NestJS Quickstart](./nestjs/quickstart.md)
- **Understand the graph:** [Authorization Graph](../domain/identity/auth-graph.md) · [Schema Overview](./contracts/schema-overview.md)
- **Error codes:** [`AUTH_xxx` catalog](./contracts/error-codes.md)
- **Compatibility:** [Compatibility Matrix](./contracts/compatibility-matrix.md)

### For SDK contributors

- **Architecture:** [ADR-0073](../architecture/adrs/0073-ums-sdk-multi-runtime.md) · [ADR-0074](../architecture/adrs/0074-auth-graph-schema-versioning.md)
- **Contract:** [Schema Overview](./contracts/schema-overview.md) · [Versioning Policy](./contracts/versioning.md) · [Fixtures](./contracts/fixtures.md)
- **Source:** `src/libs/sdk/`

---

## Conceptual Model

All three runtimes implement the same four authorization primitives, mapped one-to-one to the four authorization-bearing sections of the graph:

| Primitive | Graph section | Decision rule |
|---|---|---|
| `RequiresScope` | `scopes[]` | Scope present AND not in denies |
| `RequiresMenuOption` | `menuAccess[].…options[]` | Option resolves to `effect = "Allow"` |
| `RequiresDomainAccess` | `domainPermissions[]` | Resource+action resolves to `effect = "Allow"` |
| `RequiresFeatureFlag` | `featureFlags[]` | Flag found with `isEnabled = true` |

Universal pre-check: if `graph.validUntil < now`, decision is `Expired` regardless of content. Deny always wins over Allow (Axiom A3).

---

## References

- [ADR-0071: Auth Graph Engine](../architecture/adrs/0071-auth-graph-engine.md)
- [ADR-0072: Dynamic Auth Method Resolution](../architecture/adrs/0072-dynamic-auth-method-resolution.md)
- [ADR-0073: UMS SDK Multi-Runtime](../architecture/adrs/0073-ums-sdk-multi-runtime.md)
- [ADR-0074: Schema Versioning Policy](../architecture/adrs/0074-auth-graph-schema-versioning.md)
- [Authorization Graph (domain doc)](../domain/identity/auth-graph.md)
- [Auth Method Resolution (domain doc)](../domain/identity/auth-method-resolution.md)
- [Master Index](../MASTER_INDEX.md)
