# Decision Outbox e Inbox

> Espejo en espanol de [outbox-inbox-decision.md](../../architecture/persistence-phase1/outbox-inbox-decision.md).

## Baseline

UMS usa el patron Outbox para preservar confiabilidad de integracion dentro del monolito modular. Con ADR-0082, la implementacion productiva debe alinearse a PostgreSQL.

## Reglas

- El outbox debe persistirse en PostgreSQL dentro del limite transaccional correspondiente.
- El inbox debe ser idempotente y auditable cuando se incorpore procesamiento entrante.
- El schema del outbox debe respetar propiedad de infraestructura o bounded context documentada.
