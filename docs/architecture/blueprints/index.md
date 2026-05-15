# Architecture Blueprints

Detailed engineering specifications for the User Management System (UMS), with a primary focus on the database design and entity relationships.

## Key Documents

- **[Database Design ER](./database-design-er.md)**: The definitive entity-relationship diagram and documentation.
- **[ER Export Formats](./er-export-formats.md)**: Different representations (SQL, Mermaid, PNG) of the data model.
- **[Interactive ER Viewer](./interactive-er-viewer.html)**: Interactive tool for database exploration.
- **[Service Entity Map](./service-entity-map.md)**: Mapping between services and their database entities.

## Technical Enablers

- **[TE-04: Transactional Outbox Pattern](./technical-enablers/te-04-transactional-outbox.md)**: At-least-once event delivery via outbox table + relay worker.
- **[TE-05: Distributed Saga with Dapr](./technical-enablers/te-05-distributed-saga-dapr.md)**: Choreography-based saga for long-running cross-context workflows.
- **[TE-06: CQRS Projection Rebuild](./technical-enablers/te-06-cqrs-projection-rebuild.md)**: Shadow store + atomic swap for zero-downtime projection rebuilds.

---

**[Back to Architecture Portal](../index.md)** | **[Back to Master Index](../../MASTER_INDEX.md)**
