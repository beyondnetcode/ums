# ADR-0082: Línea Base Autoritativa de Persistencia PostgreSQL

**Estado:** Aceptado  
**Fecha:** 2026-06-08  
**Responsable de Decisión:** Arquitectura  
**Reemplaza:** La suposición local de SQL Server como persistencia productiva por defecto de UMS  
**Relacionado:** [ADR-0067](./0067-modular-monolith-schema-per-domain.es.md), [ADR-0070](./0070-database-schema-strategy-decision.es.md)

---

## Contexto

UMS ya contiene configuración runtime activa para PostgreSQL, dependencias Npgsql para EF Core, repositorios PostgreSQL, bootstrap de esquemas PostgreSQL y migraciones PostgreSQL. La documentación y las reglas previas del proyecto todavía describían SQL Server como el objetivo autoritativo de persistencia productiva, lo que generaba una contradicción entre la plataforma implementada y la línea base de gobierno.

El Product Owner confirmó que PostgreSQL es el objetivo de persistencia esperado para UMS. Esta decisión actualiza la línea base específica de UMS para que implementación, QA, documentación y agentes evalúen la coherencia de persistencia contra PostgreSQL en lugar de SQL Server.

La resolución de Context7 para esta decisión identificó `Npgsql.EntityFrameworkCore.PostgreSQL` como el proveedor EF Core para PostgreSQL. La documentación del proveedor muestra `UseNpgsql(...)` como mecanismo de configuración del DbContext y soporta operaciones de migración específicas de PostgreSQL como `EnsurePostgresExtension(...)`. La documentación de Microsoft EF Core mantiene migraciones y configuración de DbContext por proveedor como el camino esperado para gestión de esquema.

## Decisión

UMS adopta PostgreSQL como su línea base autoritativa de persistencia productiva.

La línea base backend queda así:

| Aspecto | Línea base |
|---|---|
| Runtime | .NET 10 |
| ORM | Entity Framework Core |
| Base de datos | PostgreSQL |
| Proveedor EF Core | Npgsql.EntityFrameworkCore.PostgreSQL |
| Propiedad de datos por módulo | Esquema por bounded context |
| Control primario de tenancy | Filtrado por tenant en capa de aplicación |
| Resguardo secundario de tenancy | Row-level security, propiedad de esquemas, constraints y políticas PostgreSQL |
| Confiabilidad de integración | Patrón Outbox con persistencia EF Core específica del proveedor |

Las referencias a SQL Server pasan a tratarse como contexto legado, notas de migración o comparación externa, salvo que un ADR futuro reintroduzca explícitamente SQL Server como target runtime soportado activo.

## Reglas

1. La configuración runtime debe usar PostgreSQL por defecto para rutas de ejecución UMS que no sean pruebas.
2. Nuevos mappings EF Core, migraciones, repositorios, bootstrap de esquemas, read models e integration tests deben apuntar a PostgreSQL mediante Npgsql.
3. El filtrado por tenant en capa de aplicación permanece como mecanismo primario de aislamiento.
4. PostgreSQL RLS y las políticas de base de datos son resguardos secundarios y nunca deben reemplazar autorización y scope de consultas en aplicación.
5. La documentación no debe describir SQL Server como el target autoritativo actual de persistencia UMS.
6. Toda mención a SQL Server debe quedar enmarcada explícitamente como contexto legado, historial de migración, comparación externa o contraste con la línea base corporativa.
7. ADR-0070 permanece vigente para propiedad de esquema por módulo, pero su redacción específica de motor queda reemplazada por este ADR.

## Consecuencias

### Positivas

- Alinea documentación, código y configuración runtime alrededor del proveedor de persistencia implementado.
- Elimina ambigüedad para gates QA e instrucciones de agentes.
- Permite diseñar intencionalmente capacidades específicas de PostgreSQL, incluyendo advisory locks, extensiones, esquemas y políticas RLS.
- Mantiene el camino de extracción del monolito modular mediante esquemas explícitos por bounded context y filtrado por tenant en aplicación.

### Trade-offs

- Los documentos orientados a SQL Server deben migrarse o marcarse claramente como legado.
- Runbooks operativos, planes de performance y ejemplos RLS específicos de SQL Server dejan de ser guía lista para producción hasta ser reescritos para PostgreSQL.
- Los ejemplos corporativos de Evolith que asumen SQL Server requieren notas de adaptación específicas de UMS.

## Seguimiento Requerido

1. Actualizar documentos de arquitectura, operaciones, requisitos y dominio que aún presenten SQL Server como estado actual.
2. Actualizar planes de prueba para usar contenedores PostgreSQL y verificación específica de RLS/advisory locks de PostgreSQL.
3. Actualizar documentación de read models y outbox para citar explícitamente comportamiento Npgsql.
4. Preservar paridad bilingüe en cada documento modificado.

---

**[Indice ADR](./index.es.md)** | **[ADR-0070](./0070-database-schema-strategy-decision.es.md)**
