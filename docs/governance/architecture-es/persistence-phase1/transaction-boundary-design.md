# Diseno de Limites Transaccionales

> Espejo en espanol de [transaction-boundary-design.md](../../architecture/persistence-phase1/transaction-boundary-design.md).

## Baseline

Los limites transaccionales de UMS deben permanecer explicitos por caso de uso, repositorio y bounded context. PostgreSQL es el proveedor persistente autoritativo segun ADR-0082.

## Reglas

- Un comando debe declarar claramente su limite transaccional.
- Los cambios de multiples agregados deben usar unidad de trabajo o coordinacion explicita.
- Eventos de integracion se publican via outbox, no por efectos laterales fuera de transaccion.
- El filtrado por tenant en aplicacion permanece obligatorio dentro del limite transaccional.
