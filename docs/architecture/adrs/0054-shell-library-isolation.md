# ADR-0054: Shell Library Isolation for DDD, Factory, AOP, and Bootstrapper Patterns

**Status:** Accepted  
**Date:** 2026-05-15  
**Amended:** 2026-05-24 — dependency graph corrected; scope extended to include `Ums.Shell.Aop` and `Ums.Shell.Bootstrapper`  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0060: AOP Cross-Cutting Concern Strategy](./0060-aop-cross-cutting-concern-strategy.md)
- [Shell Library Developer Guides](../shell-libraries/README.md)

---

## Context

UMS includes reusable infrastructure libraries under `src/libs/shell`. These libraries originated from external or reference sources, but UMS must not expose upstream namespaces or repository conventions in application code.

The previous ADR-0029 described "C# Native DDD Primitives (no external library)". That wording is no longer precise: the correct position is that UMS **owns its domain dependency surface** through `Ums.Shell.*` shell libraries. Application layers consume the UMS shell abstraction, not the upstream source identity.

As of 2026-05-24, the shell layer includes four library groups:

| Group | Projects | Consumer layers |
|---|---|---|
| `Ums.Shell.Ddd` | `Ums.Shell.Ddd` · `Ums.Shell.Ddd.ValueObjects` | Domain (direct) |
| `Ums.Shell.Factory` | `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer` | Domain (transitive via Ddd) · Infrastructure (direct) |
| `Ums.Shell.Aop` | `Ums.Shell.Aop` · `Ums.Shell.Aop.DispatchProxy` · `Ums.Shell.Aop.Aspects` · `Ums.Shell.Aop.Aspects.Logger.Serilog` · `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` | Application (attribute contract) · Infrastructure (DI wiring + adapters) |
| `Ums.Shell.Bootstrapper` | `Ums.Shell.Bootstrapper` · `Ums.Shell.Bootstrapper.DependencyInjection` · `Ums.Shell.Bootstrapper.AutoMapper` · `Ums.Shell.Bootstrapper.Observability` | Infrastructure · Presentation (startup) |

---

## Decision

UMS adopts a **Shell Library Isolation** strategy for all shared infrastructure patterns:

1. All shell assemblies use the `Ums.Shell.*` namespace and project naming convention.
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
  ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer
  │     └── Ums.Shell.Aop.Aspects (transitive)
  │           └── Ums.Shell.Aop (transitive)
  │                 └── Ums.Shell.Aop.DispatchProxy (transitive)
  └── Ums.Shell.Aop.Aspects.Logger.Serilog (transitive via installer)

Ums.Application
  ├── Ums.Domain
  └── Ums.Shell.Aop.Aspects    ← attribute contract only (no DI, no runtime proxy)

Ums.Domain
  ├── Ums.Shell.Ddd
  │     └── Ums.Shell.Factory (transitive via Ddd)
  └── Ums.Shell.Ddd.ValueObjects
        └── Ums.Shell.Ddd (transitive)
```

> **Correction from original ADR (2026-05-15):** `Ums.Domain.csproj` previously listed a direct `<ProjectReference>` to `Ums.Shell.Factory`. This was a redundant reference — `Ums.Shell.Ddd` already depends on `Ums.Shell.Factory`, making it available transitively. The direct reference was removed on 2026-05-24. Domain code must access factory abstractions only through the DDD shell layer, not by directly importing the Factory namespace.

### Rules per layer

| Layer | May reference | May NOT reference |
|---|---|---|
| `Ums.Domain` | `Ums.Shell.Ddd`, `Ums.Shell.Ddd.ValueObjects` | Any `Ums.Shell.Aop.*`, `Ums.Shell.Factory` (direct), `Ums.Shell.Bootstrapper.*` |
| `Ums.Application` | `Ums.Domain`, `Ums.Shell.Aop.Aspects` (attr contract) | `Ums.Shell.Aop.DispatchProxy`, `Ums.Shell.Aop.*.Installer`, `Ums.Shell.Factory`, `Ums.Shell.Bootstrapper.*` |
| `Ums.Infrastructure` | All of the above + AOP installer + Bootstrapper | — |
| `Ums.Presentation` | All layers + Bootstrapper for startup | — |

---

## Consequences

### Positive

- UMS has a stable internal dependency surface for tactical DDD, Factory, AOP, and Bootstrapper patterns.
- Domain code is pure of all infrastructure concerns — no AOP, no DI, no logging imports.
- `Ums.Application` references only the AOP **attribute contract** (`Ums.Shell.Aop.Aspects`) — handlers declare cross-cutting intent via attributes without coupling to the proxy infrastructure.
- Upstream source changes can be absorbed inside `src/libs/shell` without touching application layers.
- The `IMelLogger` pattern (marker interface in Application, concrete adapter in Infrastructure) demonstrates how to apply the same isolation principle to cross-cutting adapters.

### Trade-offs

- The shell layer is a real architectural dependency and must be versioned and reviewed accordingly.
- Security and package warnings from shell dependencies (e.g., `OpenTelemetry.Api` CVE in Bootstrapper) affect the UMS build health.
- `Ums.Shell.Ddd` depends on `Ums.Shell.Factory` — this coupling is intentional (DDD construction can use factory abstractions internally) but means that adding a Factory dependency to Domain is sufficient to pull in Factory everywhere Domain is referenced.

---

## Compliance

The following checks are mandatory after any change to the shell library references:

```bash
# 1. Build the full solution
dotnet build src/apps/ums.api/Ums.sln

# 2. Run all shell library test suites
dotnet test src/libs/shell/aop/src/Ums.Shell.Aop.Tests/Ums.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/Ums.Shell.Factory.Test/Ums.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verify Domain purity (no AOP refs in Domain)
grep -r "Ums.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Expected: no output

# 4. Verify no direct Factory ref in Domain (must be transitive only)
grep "Ums.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Expected: no output
```

---

## Supersedes / Clarifies

This ADR clarifies ADR-0029. The implementation standard is:

> UMS domain code must not depend directly on unmanaged external pattern libraries. It may depend on UMS-owned shell libraries that encapsulate and normalize those patterns. Each shell library has a defined consumer-layer contract; see the table above.

---

**[ADR Registry](./index.md)** | **[Shell Libraries Overview](../shell-libraries/README.md)** | **[ADR-0060 AOP Strategy](./0060-aop-cross-cutting-concern-strategy.md)**
