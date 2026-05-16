# Architecture Portal

Welcome to the **Architecture Portal** for the User Management System (UMS).

## Core Architecture Assets

This section focuses on the structural design and database models of the system.

### [Blueprints](./blueprints/index.md)
Detailed engineering blueprints focusing on:
- **[Database Design ER](./blueprints/database-design-er.md)**: The authoritative entity-relationship model.
- **[ER Export Formats](./blueprints/er-export-formats.md)**: SQL, Mermaid, and image exports of the schema.
- **[Interactive ER Viewer](./blueprints/interactive-er-viewer.html)**: Browser-based tool to explore the database structure.
- **[Service Entity Map](./blueprints/service-entity-map.md)**: Logical mapping between system services and database entities.
- **[Shell Library Architecture](./blueprints/shell-library-architecture.md)**: UMS-owned shell layer for inherited DDD and Factory patterns.

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

**[Back to Master Index](../MASTER_INDEX.md)** | **[Back to Root README](../../README.md)**
