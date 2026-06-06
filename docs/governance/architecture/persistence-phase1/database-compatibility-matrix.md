# Database Compatibility Matrix

| Capacidad              | SQL Server | PostgreSQL | Estado | Evidencia | Brecha | Acción |
| ---------------------- | ---------: | ---------: | ------ | --------- | ------ | ------ |
| Creación de schemas    |      Sí    |      Sí    | IMPLEMENTED | EF Core `ToTable` annotations | Ninguna | Mantener convenciones agnósticas |
| Migraciones            |      Sí    |      Sí    | PARTIALLY_IMPLEMENTED | Proyecto de migraciones actual es 100% MsSql | Falta proyecto `Ums.Infrastructure.PostgreSQL` | Crear proyecto EF Core Migrations para Postgres |
| Unit of Work           |      Sí    |      Sí    | IMPLEMENTED | DbContext SaveChangesAsync | Ninguna | Ninguna |
| Concurrencia           |      Sí    |      Sí    | PARTIALLY_IMPLEMENTED | `RowVersion` en MsSql (byte[]) vs `xmin` en Postgres (uint) | Diferencia en tipos de concurrencia | Abstraer RowVersion o usar un shadow property manejada por EF Core para Postgres |
| Transacciones          |      Sí    |      Sí    | IMPLEMENTED | EF Core `BeginTransactionAsync` | Ninguna | Ninguna |
| Eventos de dominio     |      Sí    |      Sí    | IMPLEMENTED | Interceptores EF Core | Ninguna | Ninguna |
| Service Bus in-memory  |      Sí    |      Sí    | IMPLEMENTED | MediatR | Ninguna | Ninguna |
| Idempotencia           |      Sí    |      Sí    | PARTIALLY_IMPLEMENTED | Handlers validan estado | Falta Inbox pattern genérico | Implementar idempotencia en endpoints vía Redis o DB Idempotency keys |
| Outbox                 |      Sí    |      Sí    | PARTIALLY_IMPLEMENTED | Implementado para MsSql, usa `sp_getapplock` | Lock distribuido es nativo de MsSql | Crear `IDistributedLock` y usar `pg_advisory_lock` en Postgres |
| Aislamiento por schema |      Sí    |      Sí    | IMPLEMENTED | Convenciones de arquitectura | Ninguna | Ninguna |
