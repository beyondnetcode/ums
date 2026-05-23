# ADR Registry — UMS Architecture Decision Records

> **Parent standard:** [arc32 Progressive Monolith — ADR Registry](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/reference/architecture/adrs/README.md)

All architectural decisions for the User Management System (UMS) are recorded here. Decisions inherit the mandatory baseline from the arc32 reference architecture and extend or specialize it for the UMS product context.

UMS is a satellite repository of `arc32_progresive_monolith`. The parent repository defines the corporate architecture baseline; UMS ADRs must either adopt it by reference or document an explicit SQL Server / .NET adaptation where the parent example is runtime- or database-specific.

> **Note:** ADRs 0001–0049 are inherited from the arc32 parent architecture baseline. Their governance and canonical text lives in the [arc32 ADR Registry](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/reference/architecture/adrs/README.md). Only UMS-specific decisions (ADR-0050 onward) are maintained as physical files in this repository.

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
| [ADR-0050](./0050-naming-taxonomy-standard.md) | Naming & Taxonomy Standard — Adoption of arc32 ADR-0056 | Accepted |
| [ADR-0051](./0051-event-bus-injectable-port.md) | Event Bus — Injectable Port Strategy (.NET / MassTransit) | Accepted |
| [ADR-0052](./0052-immutable-audit-trail-enforcement.md) | Immutable Audit Trail — SQL Server Enforcement Strategy | Accepted |
| [ADR-0053](./0053-opentelemetry-observability.md) | OpenTelemetry Observability Strategy | Accepted |
| [ADR-0054](./0054-shell-library-isolation.md) | Shell Library Isolation for DDD and Factory Patterns | Accepted |
| [ADR-0055](./0055-graphql-rest-hybrid-api.md) | GraphQL/REST Hybrid API Pattern | Accepted |
| [ADR-0056](./0056-clean-architecture-frontend.md) | Clean Architecture Layer Boundaries (Frontend) | Accepted |
| [ADR-0057](./0057-zustand-tanstack-query-state.md) | Zustand + TanStack Query State Management | Accepted |
| [ADR-0058](./0058-api-gateway-yarp-evolution.md) | API Gateway Evolution — YARP for Multi-Client SaaS | Proposed |

---

**[Architecture Portal](../index.md)** | **[Master Index](../../MASTER_INDEX.md)**
