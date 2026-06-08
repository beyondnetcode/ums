# Propiedad de Esquemas de Base de Datos

> Espejo en espanol de [database-schema-ownership.md](../../architecture/persistence-phase1/database-schema-ownership.md).

## Baseline

UMS mantiene schema por bounded context dentro de PostgreSQL. ADR-0070 conserva la regla de propiedad por modulo y ADR-0082 reemplaza la redaccion especifica de motor.

## Reglas

- Cada bounded context posee su esquema.
- Los repositorios no realizan joins directos entre esquemas de otros contextos.
- La comunicacion entre contextos usa contratos, eventos, read models u outbox.
- Los nombres de esquema deben estar documentados y reflejados en EF Core.
