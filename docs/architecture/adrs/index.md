# ADR Registry — UMS Architecture Decision Records

> **Parent standard:** [Evolith — ADR Registry](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/README.md)

All architectural decisions for the User Management System (UMS) are recorded here. Decisions inherit the mandatory baseline from the Evolith reference architecture and extend or specialize it for the UMS product context.

UMS is a satellite repository of `evolith_arch32`. The parent repository defines the corporate architecture baseline; UMS ADRs must either adopt it by reference or document an explicit SQL Server / .NET adaptation where the parent example is runtime- or database-specific.

> **Note:** ADRs 0001–0049 are inherited from the Evolith parent architecture baseline. Their governance and canonical text lives in the [Evolith ADR Registry](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/README.md). Only UMS-specific decisions (ADR-0050 onward) are maintained as physical files in this repository.

---

## Foundation & Cross-Cutting (ADR-0001 – ADR-0029)

> Referenced in governance documents. Physical files pending backfill.

| ADR | Title | Status |
|-----|-------|--------|
| ADR-0001 – ADR-0028 | Foundation, infrastructure, runtime, CI/CD | Referenced — files pending |
| ADR-0029 | C# Native DDD Primitives | Clarified by ADR-0054 |

## DDD & Domain Design (ADR-0030 – ADR-0049)

| ADR | Title | Status |
|-----|-------|--------|
| ADR-0030 – ADR-0048 | DDD bounded contexts, multi-tenancy, authorization, compliance | Referenced — files pending |
| ADR-0048 | Closure Table for Hierarchical Multi-Tenancy | Accepted |
| ADR-0049 | Table Partitioning Strategy | Accepted |

## Phase 04 — Construction Standards (ADR-0050 – )

| ADR | Title | Status |
|-----|-------|--------|
| [ADR-0050](./0050-naming-taxonomy-standard.md) | Naming & Taxonomy Standard — Adoption of Evolith ADR-0056 | Accepted |
| [ADR-0051](./0051-event-bus-injectable-port.md) | Event Bus — Injectable Port Strategy (.NET / MassTransit) | Accepted |
| [ADR-0052](./0052-immutable-audit-trail-enforcement.md) | Immutable Audit Trail — SQL Server Enforcement Strategy | Accepted |
| [ADR-0053](./0053-opentelemetry-observability.md) | OpenTelemetry Observability Strategy | Accepted |
| [ADR-0054](./0054-shell-library-isolation.md) | Shell Library Isolation — DDD, Factory, AOP, Bootstrapper | Accepted · Amended 2026-05-24 |
| [ADR-0055](./0055-graphql-rest-hybrid-api.md) | GraphQL/REST Hybrid API Pattern | Accepted |
| [ADR-0056](./0056-clean-architecture-frontend.md) | Clean Architecture Layer Boundaries (Frontend) | Accepted |
| [ADR-0057](./0057-zustand-tanstack-query-state.md) | Zustand + TanStack Query State Management | Accepted |
| [ADR-0058](./0058-api-gateway-yarp-evolution.md) | API Gateway Evolution — YARP for Multi-Client SaaS | Proposed |
| [ADR-0059](./0059-single-api-tier-decision.md) | Single API Tier Decision — co-location over split tiers | Accepted |
| [ADR-0060](./0060-aop-cross-cutting-concern-strategy.md) | AOP Cross-Cutting Concern Strategy — DispatchProxy over MediatR Behaviors | Accepted |
| [ADR-0061](./0061-execution-context-accessor.md) | Execution Context Accessor Pattern | Accepted, Evolith candidate |
| [ADR-0062](./0062-pii-safe-serilog-configuration.md) | PII-Safe Serilog Configuration (HARDENING-04) | Accepted, Evolith candidate |
| [ADR-0063](./0063-idempotency-middleware.md) | Idempotency Key Middleware (FIX-06 / RISK-05) | Accepted, Evolith candidate |
| [ADR-0064](./0064-lean-root-repository-taxonomy.md) | Lean Root Repository Taxonomy | Accepted |
| [ADR-0065](./0065-no-raw-guids-in-ui.md) | Prohibition of Raw GUIDs in User Interfaces (UX / DDD) | Accepted, Evolith candidate |
| [ADR-0066](./0066-actionable-user-error-contract.md) | Actionable User Error Contract and Correlated Diagnostics | Accepted, Evolith candidate |

> **Evolith candidate** - ADR has zero UMS-specific dependencies and is proposed for extraction to the Evolith parent architecture baseline.

---

**[Architecture Portal](../index.md)** | **[Master Index](../../MASTER_INDEX.md)**
