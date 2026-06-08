# Estrategia de Proveedor de Base de Datos

> Espejo en espanol de [database-provider-strategy.md](../../architecture/persistence-phase1/database-provider-strategy.md).

## Decision Vigente

PostgreSQL es el proveedor autoritativo para UMS segun ADR-0082. EF Core debe configurarse mediante Npgsql y las decisiones especificas del motor deben quedar encapsuladas en infraestructura.

## Principios

1. Dominio y aplicacion permanecen agnosticos del proveedor.
2. Infraestructura contiene las decisiones PostgreSQL.
3. Configuracion runtime no debe promover SQL Server como default.
4. Documentacion y pruebas deben usar PostgreSQL como baseline.
