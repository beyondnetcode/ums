# Seguimiento de Brechas de Historias Funcionales

> **Idioma:** Español | [Read in English](../project/functional-story-gap-tracker.md)

Seguimiento vivo de la brecha entre el catalogo de historias funcionales y la evidencia actual de implementacion en UMS.

## Proposito

Este documento mantiene una vista dinamica de lo que ya esta implementado, lo que sigue parcial y lo que aun permanece diferido. Debe actualizarse cada vez que cambie el backlog, el diseno de dominio o el tracker de implementacion de la API.

## Fuente de Verdad

- [Directorio de Historias Funcionales](../requirements-es/functional-stories/index.md)
- [Backlog del Producto MVP](./mvp-product-backlog.md)
- [Seguimiento de Implementacion de Agregados en la API](./api-aggregate-implementation-tracker.md)
- [Portal DDD](../construction/ddd-design/index.md)
- [Matriz de Trazabilidad](../../architecture/traceability-matrix.md)

## Resumen de Cobertura

| Estado | Cantidad | IDs de historia |
|---|---:|---|
| Implementado / utilizable | 23 | [FS-01](../requirements-es/functional-stories/fs-01-user-authentication.md), [FS-02](../requirements-es/functional-stories/fs-02-create-authorization-template.md), [FS-03](../requirements-es/functional-stories/fs-03-register-organization.md), [FS-04](../requirements-es/functional-stories/fs-04-register-system-topology.md), [FS-05](../requirements-es/functional-stories/fs-05-create-profile-manual-template.md), [FS-06](../requirements-es/functional-stories/fs-06-auto-assign-template.md), [FS-07](../requirements-es/functional-stories/fs-07-visual-graph-resolver.md), [FS-08](../requirements-es/functional-stories/fs-08-hosted-login-redirection.md), [FS-09](../requirements-es/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md), [FS-10](../requirements-es/functional-stories/fs-10-external-b2b-access-request-approval.md), [FS-11](../requirements-es/functional-stories/fs-11-user-document-upload.md), [FS-13](../requirements-es/functional-stories/fs-13-hierarchical-config.md), [FS-15](../requirements-es/functional-stories/fs-15-notification-rules.md), [FS-16](../requirements-es/functional-stories/fs-16-access-enforcement-policy.md), [FS-17](../requirements-es/functional-stories/fs-17-maintain-system-roles.md), [FS-18](../requirements-es/functional-stories/fs-18-manage-local-user-password.md), [FS-19](../requirements-es/functional-stories/fs-19-admin-password-reset-validity-management.md), [FS-20](../requirements-es/functional-stories/fs-20-system-parameter-management.md), [FS-21](../requirements-es/functional-stories/fs-21-tenant-signup-request-approval.md), [FS-22](../requirements-es/functional-stories/fs-22-user-signup-request-approval.md), [FS-25](../requirements/functional-stories/fs-25-ddd-domain-resource-hierarchy.es.md), [FS-26](../requirements/functional-stories/fs-26-auth-graph-preview-from-profile.es.md), [FS-27](../requirements/functional-stories/fs-27-state-change-consistency-broken-rules.es.md) |
| Parcial | 3 | [FS-12](../requirements-es/functional-stories/fs-12-role-promotion-process.md), [FS-14](../requirements-es/functional-stories/fs-14-delegated-management.md), [FS-24](../requirements-es/functional-stories/fs-24-profile-request-approval.es.md) |
| Diferido | 0 | — |

## Leyenda de Seguimiento

| Campo | Significado |
|---|---|
| Senal | `Green` = implementado / utilizable, `Amber` = parcial, `Red` = diferido o faltante |
| Prioridad | `P1` = maxima prioridad de seguimiento, `P2` = seguimiento importante, `P3` = diferible |
| Criticidad | `Alta`, `Media`, `Baja` |
| Complejidad | `Alta`, `Media`, `Baja` |
| Objetivo | Objetivo de revision para el siguiente ciclo; usar `TBD` hasta definir una fecha |
| Orden | Numero de `FS` ascendente, luego `Prioridad` descendente, luego `Criticidad` descendente, luego `Complejidad` descendente |

## Registro de Brechas Abiertas

| FS | Historia | Senal | Prioridad | Criticidad | Complejidad | Responsable | Objetivo | Estado | Brecha principal | Siguiente accion |
|---|---|---|---|---|---|---|---|---|---|---|
| [FS-12](../requirements-es/functional-stories/fs-12-role-promotion-process.md) | Execute Role Promotion Process | Amber | P1 | Alta | Alta | IGA | TBD | Abierto | El flujo de promocion aun necesita la revision completa de manager/seguridad, ejecucion, verificacion y cierre del analisis de impacto. | Terminar la maquina de estados de promocion y alinear los pasos de aprobacion con el contrato de dominio. |
| [FS-14](../requirements-es/functional-stories/fs-14-delegated-management.md) | Delegate User Management Between Administrators | Amber | P2 | Media | Media | Identity | TBD | Abierto | La delegacion existe como modelo, pero el alcance de punta a punta y el flujo de auditoria aun necesitan validacion final. | Cerrar la cobertura de acciones delegadas y verificar la ruta de aceptacion. |
| [FS-24](../requirements-es/functional-stories/fs-24-profile-request-approval.es.md) | Profile Request Approval and Manual Assignment | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El registro de decision aun necesita rol solicitado, rol otorgado, razon y resultado de notificacion. | Extender el payload del resultado de aprobacion y el modelo de persistencia. |

---

## Desglose de Brechas FS-13

> Brechas ordenadas por prioridad y criticidad. Trabajar de arriba hacia abajo. Cada brecha referencia el criterio de aceptacion o regla de negocio que bloquea.

| # | Brecha | Severidad | Bloquea | Que implementar | Archivos clave |
|---|---|---|---|---|---|
| 1 | ~~**Resolver jerárquico de 4 niveles**~~ **Hecho** — `GetWithPrecedence(code, tenantId, suiteId, moduleId)` agregado. Cascada Module → Suite → Tenant → Global implementada. 10 nuevos tests cubriendo todos los caminos de fallback. | Cerrado | CA-3, RN-1 | — | `Ums.Infrastructure/Configuration/InMemoryConfigurationCache.cs`, `ConfigurationProvider.cs` |
| 2 | ~~**Flag no-sobrescribible**~~ **Hecho** — `IsNonOverridable` agregado al dominio, record, DTO y comando. Guard en `CreateAppConfigurationCommandHandler` rechaza sobreescrituras cuando cualquier scope padre tiene `IsNonOverridable=true`. Migración `Fs13AddIsNonOverridableToAppConfiguration` generada. 4 nuevos tests del guard. | Cerrado | CA-2, RN-2 | — | `AppConfigurationProps.cs`, `AppConfigurationRecord.cs`, `CreateAppConfigurationCommandHandler.cs` |
| 3 | ~~**Alcances Suite y Module no procesados**~~ **Hecho** — `ReloadTenantAsync` invalida correctamente los buckets Suite/Module antes de repoblar. Helper `BucketTenantConfigs` filtra solo Published y distribuye por scope. Configs Draft/Archived ya no entran al cache de resolución. 3 nuevos tests para evicción de entradas obsoletas y semántica Published-only. | Cerrado | CA-1, CA-3 | — | `ConfigurationProvider.cs` |
| 4 | ~~**Protección de valores sensibles**~~ **Hecho** — Cifrado AES-256-CBC integrado de punta a punta: cifra en Create/Update (handler), descifra al cargar el cache (ConfigurationProvider), redacta `"***"` en DTOs para llamantes no admin. `IValueEncryptionService` + `AesValueEncryptionService` agregados. Clave desde `AppConfiguration:EncryptionKey` (fallback de dev usa clave cero). 9 tests: round-trip, idempotencia, detección de prefijo, fallback dev, guard de longitud de clave. | Cerrado | RN-5 | — | `AesValueEncryptionService.cs`, `CreateAppConfigurationCommandHandler.cs`, `ConfigurationProvider.cs`, query handlers |
| 5 | ~~**Endpoint `/resolve` faltante**~~ **Hecho** — `GET /app-configurations/resolve?code=X&tenantId=Y&suiteId=Z&moduleId=W` agregado. `ResolveAppConfigurationQuery/Handler` conectado a `IConfigurationProvider.GetWithPrecedence`. Retorna `ResolvedAppConfigurationDto` con `ResolvedScope`, `SourceConfigId`, flag `Found`. Valores cifrados se redactan para no-admin. 5 nuevos tests. | Cerrado | CA-3 | — | `ResolveAppConfigurationQueryHandler.cs`, `AppConfigurationQueryEndpoints.cs` |
| 6 | ~~**Sin endpoints REST para Parameters**~~ **Hecho (backend)** — Repositorios, commands y endpoints REST agregados para ParameterDefinition (Create/Update/Archive), ParameterGlobalValue (Create/Update/Publish/Archive) y ParameterTenantValue (Create/Update). Rehydration agregada al ConfigurationAggregateFactory. Implementaciones InMemory para dev/tests. Panel frontend permanece diferido. | Cerrado | CA-1 | — | `SqlServerParameterRepositories.cs`, `ParameterDefinitionCommands.cs`, `ParameterValueCommands.cs`, `ParameterEndpoints.cs` |
| 7 | ~~**Inconsistencia del modelo de alcance**~~ **Hecho** — `ParameterScope` extendido con `SuiteLevel(4)` y `ModuleLevel(5)` que coinciden con los IDs de `ConfigurationScope`. Helpers `SupportsGlobal/Tenant/Suite/Module` agregados. `AllScopes` actualizado. No requiere migración (valores son aditivos). | Cerrado | RN-1 | — | `Ums.Domain/Configuration/Parameter/ValueObjects/ParameterScope.cs` |
| 8 | ~~**`IdpConfiguration` sin ciclo de vida Archive**~~ **Hecho** — `IdpConfigStatus.Archived(4)` agregado. Método de dominio `IdpConfiguration.Archive()` agregado (bloquea si está Active o ya Archived). `ArchiveIdpConfigurationCommand` + handler + endpoint `POST /idp-configurations/{id}/archive` agregados. Update ahora bloquea en estado Archived. | Cerrado | Completitud del ciclo de vida | — | `IdpConfigStatus.cs`, `IdpConfiguration.cs`, `ArchiveIdpConfigurationCommand*.cs`, `IdpConfigurationEndpoints.cs` |

---

## Cadencia de Revision

- Actualizar este tracker cada vez que una historia cambie en el backlog o el tracker de implementacion cambie.
- Re-auditar las brechas abiertas despues de cualquier cambio de dominio, API o documentacion que afecte a las historias listadas.
- Mantener sincronizadas las versiones en ingles y espanol en estructura y contenido.

## Ultima Revision

2026-06-04 (FS-13 completamente implementado; pendientes parciales: FS-12, FS-14, FS-24)
