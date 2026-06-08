# Estrategia de Pruebas de Base de Datos y Mensajeria

> Espejo en espanol de [database-and-messaging-testing-strategy.md](../../architecture/persistence-phase1/database-and-messaging-testing-strategy.md).

## Estado

ADR-0082 establece PostgreSQL como baseline autoritativa de persistencia para UMS. Las pruebas de base de datos y mensajeria deben validar PostgreSQL mediante EF Core y Npgsql.

## Reglas

- Usar contenedores o instancias PostgreSQL para pruebas de integracion persistentes.
- Validar migraciones, outbox, advisory locks, esquemas por bounded context y politicas de aislamiento.
- Mantener el filtrado por tenant en aplicacion como control primario.
- Tratar referencias SQL Server como contexto legado o comparacion externa.
