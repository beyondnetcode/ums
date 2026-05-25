# Portal de Operaciones

Procedimientos operativos y runbooks para el User Management System (UMS) en produccion.

## Runbooks

Procedimientos de recuperacion paso a paso:

- **[RB-01: Procedimiento de Respuesta a Incidentes](./runbooks/rb-01-incident-response.es.md)** — Clasificacion de severidad, triaje, sala de crisis, plantilla post-mortem
- **[RB-02: Procedimiento de Rollback](./runbooks/rb-02-rollback-procedure.es.md)** — Pasos de rollback de aplicacion y BD
- **[RB-03: Recuperacion de Fallo de Cache](./runbooks/rb-03-cache-failure-recovery.es.md)** — Diagnostico y recuperacion de fallos Redis
- **[RB-04: Failover de Base de Datos](./runbooks/rb-04-database-failover.es.md)** — Failover primario, PITR, verificacion RLS multi-tenant

## Metricas de Ingenieria

- **[Panel de Metricas de Soluciones](./metrics/index.es.md)** — Metricas consolidadas en todas las soluciones (API, Web, Librerias, Pruebas) por categoria: codificacion, seguridad, calidad, pruebas y uso de IA. Actualizado automaticamente despues de cada commit.

---

**[Volver al Indice Maestro](../MASTER_INDEX.es.md)**
