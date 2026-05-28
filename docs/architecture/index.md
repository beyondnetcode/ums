# Architecture Portal

Welcome to the **Architecture Portal** for the User Management System (UMS).

## Governance Model — Evolith Satellite Architecture

UMS is a **satellite product** of the [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32). This relationship defines how architectural decisions are made in this repository.

### How the inheritance works

```
Evolith (parent)                        UMS (satellite)
─────────────────────────────         ──────────────────────────────────
Defines baseline policies      ──►    Inherits by reference (ADR-0050)
Provides canonical patterns    ──►    Adopts or adapts per UMS context
Sets naming conventions        ──►    Conforms + documents exceptions
```

A UMS ADR can do one of three things relative to Evolith:

| Mode | When to use | Example |
|---|---|---|
| **Adopt** | Evolith policy applies as-is | ADR-0050: Naming taxonomy adopted verbatim |
| **Specialize** | Evolith policy applies but UMS adds constraints | ADR-0052: Immutable audit trail with SQL Server specifics |
| **Override** | UMS diverges from Evolith with explicit justification | ADR-0059: Single API tier decision (co-location over split tiers) |

### Why this matters for onboarding

When you encounter a decision that seems to conflict with Evolith, check the relevant UMS ADR first. The override may be intentional and justified. If no ADR exists, the Evolith baseline applies. **Never assume silence means permission to deviate.**

### Worked example — API Tier decision

Evolith permits splitting query and command surfaces into separate API tiers when scale or team ownership justifies it. UMS explicitly decided **not to** do this at current maturity:

- CQRS separation already exists at protocol level (GraphQL queries / REST commands).
- Splitting tiers adds operational cost with no measurable benefit at MVP scale.
- Multi-tenant load risk is mitigated by complexity limits, timeouts, and gateway rate limiting (TE-07).

This decision is recorded in [ADR-0059](./adrs/0059-single-api-tier-decision.md) with explicit triggers for when the decision should be revisited.

> This is the intended pattern: **inherit the baseline, override with evidence, document the trigger to revert.**

---

## Architecture Overview

- **[Architecture Overview](./overview.md)**: Global architecture vision, bounded context map, aggregate catalog, and cross-context integration principles.

---

## Core Architecture Assets

This section focuses on the structural design and database models of the system.

### [Blueprints](./blueprints/index.md)
Detailed engineering blueprints focusing on:
- **[Database Design ER](./blueprints/database-design-er.md)**: The authoritative entity-relationship model.
- **[ER Export Formats](./blueprints/er-export-formats.md)**: SQL, Mermaid, and image exports of the schema.
- **[Interactive ER Viewer](./blueprints/interactive-er-viewer.html)**: Browser-based tool to explore the database structure.
- **[Service Entity Map](./blueprints/service-entity-map.md)**: Logical mapping between system services and database entities.
- **[Shell Library Architecture](./blueprints/shell-library-architecture.md)**: UMS-owned shell layer for inherited DDD and Factory patterns.
- **[Shell Library Developer Guides](./shell-libraries/README.md)**: Comprehensive developer guides for all four shell libraries with independent and combined usage examples.
  - [Ums.Shell.Ddd](./shell-libraries/ddd.md) · [Ums.Shell.Factory](./shell-libraries/factory.md) · [Ums.Shell.Aop](./shell-libraries/aop.md) · [Ums.Shell.Bootstrapper](./shell-libraries/bootstrapper.md) · [Combined Usage](./shell-libraries/combined-usage.md)

### [Web Frontend](./web-frontend/README.md)
Applied React Web reference for UMS. This section maps current source evidence to the Evolith Web Frontend Standard - React while keeping product-specific implementation details local to UMS.

### [Traceability Matrix](./traceability-matrix.md)
Cross-reference of all Functional Stories (FS-01..16) to ADRs and Technical Enablers.

### [Technical Enablers](./blueprints/technical-enablers/index.md)
Engineering blueprints specifying how ADRs are implemented in the UMS context:
- **[TE-04: Transactional Outbox](./blueprints/technical-enablers/te-04-transactional-outbox.md)**
- **[TE-05: Distributed Saga with Dapr](./blueprints/technical-enablers/te-05-distributed-saga-dapr.md)**
- **[TE-06: CQRS Projection Rebuild](./blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md)**

### [Canonical Patterns](./artifacts/canonical-patterns/index.md)
Reference implementations for core architectural patterns:
- **[CP-01: Hexagonal Architecture — Port & Adapter](./artifacts/canonical-patterns/cp-01-hexagonal-port-adapter.md)**
- **[CP-02: Aggregate Root & Domain Event](./artifacts/canonical-patterns/cp-02-aggregate-root-domain-event.md)**
- **[CP-03: Result Pattern](./artifacts/canonical-patterns/cp-03-result-pattern.md)**
- **[CP-04: Multi-Tenant Repository with RLS](./artifacts/canonical-patterns/cp-04-multitenant-repository-rls.md)**

### Related — Domain Layer Design
- **[DDD Design Portal](../governance/construction/ddd-design/index.md)**: Bounded contexts, aggregates, value objects, commands, events, and state machines for the complete product.
- **[DDD Primitives](../governance/construction/ddd-design/11-ddd-primitives.md)**: Domain primitives implemented through the UMS shell library layer.
- **[Interactive DDD Viewer](../governance/construction/ddd-design/interactive-ddd-viewer.html)**: Browser-based tool to explore bounded context map, state machines, and cross-context flows.

---

## Engineering Metrics

- **[Solution Metrics Dashboard](../operations/metrics/index.md)**: Consolidated engineering metrics across all solutions (API, Web, Libs, Tests) organized by category: coding, security, quality, tests, and AI usage.

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[Back to Root README](../../README.md)**
