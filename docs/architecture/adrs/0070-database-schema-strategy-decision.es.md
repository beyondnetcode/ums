# ADR-0070: Estrategia de Esquema de Base de Datos Por Módulo

**Estado:** Aceptado
**Fecha:** 2026-05-27
**Responsable de Decisión:** Arquitectura
**Padre:** [ADR-0067 — Modular Monolith Schema Per Domain](./0067-modular-monolith-schema-per-domain.md)

---

## Contexto

UMS es un modular monolith construido sobre .NET 10 y SQL Server, implementando múltiples bounded contexts (Identity, Authorization, Configuration, Audit, Approvals, IGA). La arquitectura base de Evolith (ADR-0067) establece que cada bounded context debe poseer su propio esquema de base de datos dentro de una única base de datos física, aplicando separación lógica y propiedad de módulo.

Antes de esta decisión, la convención de nombres de esquemas era inconsistente entre la documentación y el código, y algunas configuraciones de entidades dependían del fallback default schema del DbContext en lugar de declarar explícitamente su esquema. Esto creó riesgos de:

- Colocación silenciosa de tablas en el esquema incorrecto si el default cambia.
- Confusión del desarrollador cuando los nombres de esquema en la documentación no coincidían con la base de datos real.
- Propiedad de módulo no clara a nivel de base de datos.

## Decisión

**UMS usa una única base de datos física SQL Server con esquemas dedicados por bounded context. Cada módulo posee su esquema y sus tablas. El acceso cruzado entre módulos de base de datos está prohibido a nivel de repositorio.**

### Registro de Esquemas

| Bounded Context | Código | Nombre de Esquema | Módulo Propietario |
|-----------------|--------|-------------------|-------------------|
| Identity | BC-A | `ums_identity` | `Ums.Infrastructure/Persistence/Identity/` |
| Authorization | BC-B | `ums_authorization` | `Ums.Infrastructure/Persistence/Authorization/` |
| Configuration | BC-C | `ums_configuration` | `Ums.Infrastructure/Persistence/Configuration/` |
| Audit | BC-D | `audit` | `Ums.Infrastructure/Persistence/Audit/` |
| Approvals | BC-F | `approvals` | `Ums.Infrastructure/Persistence/Approvals/` |
| IGA | BC-H | `iga` | `Ums.Infrastructure/Persistence/IGA/` |
| Platform/Outbox | Infra | `ums_platform` | `Ums.Infrastructure/Persistence/Outbox/` |

### Reglas

1. **Única base de datos física.** Todos los esquemas coexisten en una instancia de SQL Server.
2. **Esquema por módulo.** Cada bounded context tiene exactamente un esquema. Las tablas pertenecen a su contexto se crean solo en su esquema.
3. **Esquema explícito en EF Core.** Cada `IEntityTypeConfiguration` debe llamar `ToTable(tableName, schema)` con la constante del esquema del módulo. Ninguna entidad puede depender del fallback `HasDefaultSchema()`.
4. **Constantes de esquema centralizadas.** Cada módulo declara su nombre de esquema en un archivo `*PersistenceConstants.cs` bajo su carpeta de persistencia.
5. **Propiedad de módulo.** Los repositorios de un módulo solo pueden consultar DbSets que pertenezcan a su propio bounded context. El acceso cruzado debe usar application services, domain services, read models, o integration events (Outbox pattern).
6. **Sin uso de `dbo`.** Ninguna tabla puede crearse en el esquema default `dbo`.
7. **Propiedad de migraciones.** Cada archivo de migración SQL crea o modifica tablas dentro del esquema de un solo módulo. Las migraciones cruzadas entre esquemas están prohibidas.
8. **Comunicación cruzada de módulos.** Los módulos se comunican vía el patrón Outbox (integration events), no vía joins directos de base de datos entre esquemas.

### Configuración de DbContext

`UmsPlatformDbContext` sirve todos los bounded contexts pero respeta los límites de esquema mediante `ToTable(tableName, schema)` explícito en cada configuración de entidad:

```csharp
// Ejemplo: Configuración de entidad de Authorization
builder.ToTable("SystemSuites", AuthorizationPersistenceConstants.Schema);
```

El esquema default (`ums_platform`) está configurado como fallback para entidades de infraestructura únicamente (Outbox), pero incluso estas deben declarar su esquema explícitamente.

## Consecuencias

### Positivas

- **Propiedad de módulo clara.** Cada esquema es propiedad de exactamente un bounded context.
- **Aislamiento a nivel de base de datos.** Los módulos no pueden modificar accidentalmente las tablas de otro módulo.
- **Auditabilidad.** Los límites de esquema hacen fácil rastrear qué módulo posee qué datos.
- **Seguridad de migraciones.** Las migraciones están acotadas a un único esquema, reduciendo el riesgo de conflictos.
- **Alineación de documentación.** Los nombres de esquema en código y docs son idénticos.

### Trade-offs

- **Acoplamiento de DbContext único.** Todos los bounded contexts comparten un `UmsPlatformDbContext`, creando acoplamiento en tiempo de compilación. Esto es aceptado para un modular monolith pero requeriría refactoring si UMS evoluciona a arquitectura distribuida.
- **Convención de nombres de esquema.** No todos los esquemas usan el prefijo `ums_` (`audit`, `approvals`, `iga`). Esta es una elección pragmática para mantener nombres concisos; el prefijo `ums_` está reservado para contextos de negocio core.

## Cumplimiento

- Las configuraciones de entidades de EF Core deben usar `ToTable(tableName, schema)` con una constante de `*PersistenceConstants.cs`.
- Las migraciones SQL deben crear esquemas explícitamente (`CREATE SCHEMA`) y todas las tablas dentro de ese esquema.
- El índice ADR y el bounded context map deben reflejar los nombres canónicos de esquema listados en este documento.
- Los tests de guard arquitectónicos (NetArchTest o equivalente) aplican que los repositorios solo acceden a sus propios DbSets del módulo.

---

**[Índice ADR](./index.md)** | **[ADR Padre-0067](./0067-modular-monolith-schema-per-domain.md)**