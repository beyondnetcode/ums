# Transaccionalidad del Bus In-Memory

> Espejo en espanol de [in-memory-service-bus-transactionality.md](../../architecture/persistence-phase1/in-memory-service-bus-transactionality.md).

## Resumen

El bus in-memory puede ser util para desarrollo y pruebas focalizadas, pero no reemplaza el outbox persistente para confiabilidad de integracion.

## Reglas

- Produccion debe depender de outbox persistente sobre PostgreSQL.
- Las pruebas deben diferenciar claramente bus in-memory, mocks y mensajeria persistente.
- La consistencia transaccional debe validarse junto con EF Core y el proveedor Npgsql.
