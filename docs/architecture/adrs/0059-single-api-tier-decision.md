# ADR-0059 — Single API Tier Decision

> **Language:** English | *Español no disponible*

| Field | Value |
|---|---|
| **ID** | ADR-0059 |
| **Status** | ACCEPTED |
| **Date** | 2026-05-15 |
| **Deciders** | Architecture Team |
| **Evolith Relationship** | Override — UMS diverges from Evolith multi-tier API baseline |

---

## Context

Evolith permits splitting query and command surfaces into separate API tiers when scale or team ownership justifies it (e.g., a dedicated GraphQL read-tier and a separate REST command-tier deployed independently). At current UMS maturity this split was evaluated and explicitly rejected.

## Decision

UMS will co-locate GraphQL (query) and REST (command) surfaces in a **single `ums.api` deployment unit** using HotChocolate Minimal APIs on .NET 10.

CQRS separation is maintained at the **protocol level** (GraphQL for reads, REST for writes) but not at the deployment/infrastructure level.

## Rationale

| Factor | Analysis |
|---|---|
| **Scale** | MVP and early production load does not justify the operational overhead of two independently deployed API surfaces |
| **CQRS already enforced** | Protocol-level CQRS (GraphQL vs REST) provides the read/write isolation that Evolith intends, without infrastructure cost |
| **Team size** | A split tier requires separate CI/CD pipelines, separate health checks, and separate scaling policies — not cost-effective at current headcount |
| **Multi-tenant risk** | Complex query risk is mitigated by gateway rate limiting (TE-07), timeout policies, and complexity limits on the GraphQL schema — not by tier isolation |

## Consequences

- All API endpoints (GraphQL + REST) are served from a single process and container.
- YARP gateway (ADR-0058) routes traffic to this single tier.
- Horizontal scaling applies to the whole tier, not read vs. write separately.

## Trigger to Revisit

This decision should be revisited if **any** of the following occurs:

1. Read query latency exceeds SLA under production multi-tenant load and profiling confirms API-tier contention (not DB contention).
2. Team ownership grows to the point where separate teams own query vs. command surfaces.
3. A specific tenant's GraphQL complexity starves REST commands under load testing at scale.

---

**[ADR Registry](./index.md)** | **[Architecture Portal](../index.md)**
