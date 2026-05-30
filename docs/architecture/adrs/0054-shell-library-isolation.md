# ADR-0054: Shell Library Isolation for DDD, Factory, AOP, and Bootstrapper Patterns

**Status:** Accepted  
**Date:** 2026-05-15  
**Amended:** 2026-05-24 — dependency graph corrected; scope extended to include `BeyondNetCode.Shell.Aop` and `BeyondNetCode.Shell.Bootstrapper`  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0060: AOP Cross-Cutting Concern Strategy](./0060-aop-cross-cutting-concern-strategy.md)
- [Shell Library Developer Guides](../shell-libraries/README.md)

---

## Context

UMS includes reusable infrastructure libraries under `src/libs/shell`. These libraries originated from external or reference sources, but UMS must not expose upstream namespaces or repository conventions in application code.

The previous ADR-0029 described "C# Native DDD Primitives (no external library)". That wording is no longer precise: the correct position is that UMS **owns its domain dependency surface** through `BeyondNetCode.Shell.*` shell libraries. Application layers consume the UMS shell abstraction, not the upstream source identity.

As of 2026-05-24, the shell layer includes four library groups:

| Group | Projects | Consumer layers |
|---|---|---|
| `BeyondNetCode.Shell.Ddd` | `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects` | Domain (direct) |
| `BeyondNetCode.Shell.Factory` | `BeyondNetCode.Shell.Factory` · `BeyondNetCode.Shell.DI` | Domain (transitive via Ddd) · Infrastructure (direct) |
| `BeyondNetCode.Shell.Aop` | `BeyondNetCode.Shell.Aop` · `BeyondNetCode.Shell.DispatchProxy` · `BeyondNetCode.Shell.Aspects` · `BeyondNetCode.Shell.Logger.Serilog` · `BeyondNetCode.Shell.DI` | Application (attribute contract) · Infrastructure (DI wiring + adapters) |
| `BeyondNetCode.Shell.Bootstrapper` | `BeyondNetCode.Shell.Bootstrapper` · `BeyondNetCode.Shell.DI` · `BeyondNetCode.Shell.AutoMapper` · `BeyondNetCode.Shell.Observability` | Infrastructure · Presentation (startup) |

---

## Decision

UMS adopts a **Shell Library Isolation** strategy for all shared infrastructure patterns:

1. All shell assemblies use the `BeyondNetCode.Shell.*` namespace and project naming convention.
2. Application layers must not reference upstream namespaces such as `BeyondNet.*` or source repository names such as `csdevlib.*`.
3. Shell libraries must compile cross-platform and target the current stable .NET baseline used by the API (`net10.0`).
4. Dependency direction is strictly enforced as described below.

### Authorised reference graph

```
Ums.Presentation
  ├── Ums.Application
  └── Ums.Infrastructure

Ums.Infrastructure
  ├── Ums.Application
  ├── Ums.Domain
  ├── BeyondNetCode.Shell.DI
  │     └── BeyondNetCode.Shell.Aspects (transitive)
  │           └── BeyondNetCode.Shell.Aop (transitive)
  │                 └── BeyondNetCode.Shell.DispatchProxy (transitive)
  └── BeyondNetCode.Shell.Logger.Serilog (transitive via installer)

Ums.Application
  ├── Ums.Domain
  └── BeyondNetCode.Shell.Aspects    ← attribute contract only (no DI, no runtime proxy)

Ums.Domain
  ├── BeyondNetCode.Shell.Ddd
  │     └── BeyondNetCode.Shell.Factory (transitive via Ddd)
  └── BeyondNetCode.Shell.Ddd.ValueObjects
        └── BeyondNetCode.Shell.Ddd (transitive)
```

> **Correction from original ADR (2026-05-15):** `Ums.Domain.csproj` previously listed a direct `<ProjectReference>` to `BeyondNetCode.Shell.Factory`. This was a redundant reference — `BeyondNetCode.Shell.Ddd` already depends on `BeyondNetCode.Shell.Factory`, making it available transitively. The direct reference was removed on 2026-05-24. Domain code must access factory abstractions only through the DDD shell layer, not by directly importing the Factory namespace.

### Rules per layer

| Layer | May reference | May NOT reference |
|---|---|---|
| `Ums.Domain` | `BeyondNetCode.Shell.Ddd`, `BeyondNetCode.Shell.Ddd.ValueObjects` | Any `BeyondNetCode.Shell.Aop.*`, `BeyondNetCode.Shell.Factory` (direct), `BeyondNetCode.Shell.Bootstrapper.*` |
| `Ums.Application` | `Ums.Domain`, `BeyondNetCode.Shell.Aspects` (attr contract) | `BeyondNetCode.Shell.DispatchProxy`, `BeyondNetCode.Shell.Aop.*.Installer`, `BeyondNetCode.Shell.Factory`, `BeyondNetCode.Shell.Bootstrapper.*` |
| `Ums.Infrastructure` | All of the above + AOP installer + Bootstrapper | — |
| `Ums.Presentation` | All layers + Bootstrapper for startup | — |

---

## Consequences

### Positive

- UMS has a stable internal dependency surface for tactical DDD, Factory, AOP, and Bootstrapper patterns.
- Domain code is pure of all infrastructure concerns — no AOP, no DI, no logging imports.
- `Ums.Application` references only the AOP **attribute contract** (`BeyondNetCode.Shell.Aspects`) — handlers declare cross-cutting intent via attributes without coupling to the proxy infrastructure.
- Upstream source changes can be absorbed inside `src/libs/shell` without touching application layers.
- The `IMelLogger` pattern (marker interface in Application, concrete adapter in Infrastructure) demonstrates how to apply the same isolation principle to cross-cutting adapters.

### Trade-offs

- The shell layer is a real architectural dependency and must be versioned and reviewed accordingly.
- Security and package warnings from shell dependencies (e.g., `OpenTelemetry.Api` CVE in Bootstrapper) affect the UMS build health.
- `BeyondNetCode.Shell.Ddd` depends on `BeyondNetCode.Shell.Factory` — this coupling is intentional (DDD construction can use factory abstractions internally) but means that adding a Factory dependency to Domain is sufficient to pull in Factory everywhere Domain is referenced.

---

## Compliance

The following checks are mandatory after any change to the shell library references:

```bash
# 1. Build the full solution
dotnet build src/apps/ums.api/Ums.sln

# 2. Run all shell library test suites
dotnet test src/libs/shell/aop/src/BeyondNetCode.Shell.Aop.Tests/BeyondNetCode.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/BeyondNetCode.Shell.Factory.Test/BeyondNetCode.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verify Domain purity (no AOP refs in Domain)
grep -r "BeyondNetCode.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Expected: no output

# 4. Verify no direct Factory ref in Domain (must be transitive only)
grep "BeyondNetCode.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Expected: no output
```

---

## Supersedes / Clarifies

This ADR clarifies ADR-0029. The implementation standard is:

> UMS domain code must not depend directly on unmanaged external pattern libraries. It may depend on UMS-owned shell libraries that encapsulate and normalize those patterns. Each shell library has a defined consumer-layer contract; see the table above.

---

**[ADR Registry](./index.md)** | **[Shell Libraries Overview](../shell-libraries/README.md)** | **[ADR-0060 AOP Strategy](./0060-aop-cross-cutting-concern-strategy.md)**
