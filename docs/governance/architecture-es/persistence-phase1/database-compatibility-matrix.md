# Matriz de Compatibilidad de Base de Datos

> Espejo en espanol de [database-compatibility-matrix.md](../../architecture/persistence-phase1/database-compatibility-matrix.md).

## Estado

La compatibilidad productiva actual se evalua contra PostgreSQL. SQL Server solo debe aparecer como contexto historico, comparacion externa o nota de migracion.

| Capacidad | Baseline UMS | Criterio de aceptacion |
|---|---|---|
| Migraciones | PostgreSQL con Npgsql | Migraciones EF Core aplicables sin pasos manuales |
| Esquemas | Schema por bounded context | Cada modulo mantiene propiedad clara de tablas |
| Locks distribuidos | Advisory locks PostgreSQL | Interfaz de lock no filtra detalles del proveedor |
| Outbox | EF Core sobre PostgreSQL | Publicacion confiable y transaccional |
| Tenancy | Filtro de aplicacion primario | RLS y politicas DB solo como resguardo |
