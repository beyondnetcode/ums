# Plan de Implementacion de Persistencia Fase 1

> Espejo en espanol de [phase-1-persistence-implementation-plan.md](../../architecture/persistence-phase1/phase-1-persistence-implementation-plan.md).

## Objetivo

Alinear persistencia Fase 1 con PostgreSQL, EF Core y Npgsql, manteniendo dominio y aplicacion libres de detalles del proveedor.

## Checklist

- Configuracion runtime por defecto en PostgreSQL.
- Repositorios productivos usando Npgsql.
- Migraciones y bootstrap PostgreSQL validados.
- Outbox habilitado con persistencia PostgreSQL.
- Pruebas de integracion ejecutadas contra PostgreSQL.
- Documentacion bilingue actualizada.
