# Seguimiento de Brechas de Historias Funcionales

> **Idioma:** Espanol | [Read in English](../project/functional-story-gap-tracker.md)

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
| Implementado / utilizable | 14 | FS-01, FS-02, FS-03, FS-04, FS-05, FS-07, FS-08, FS-10, FS-11, FS-18, FS-21, FS-22, FS-26, FS-27 |
| Parcial | 12 | FS-09, FS-12, FS-13, FS-14, FS-15, FS-16, FS-17, FS-19, FS-20, FS-23, FS-24, FS-25 |
| Diferido | 1 | FS-06 |

## Leyenda de Seguimiento

| Campo | Significado |
|---|---|
| Senal | `Green` = implementado / utilizable, `Amber` = parcial, `Red` = diferido o faltante |
| Prioridad | `P1` = maxima prioridad de seguimiento, `P2` = seguimiento importante, `P3` = diferible |
| Criticidad | `Alta`, `Media`, `Baja` |
| Complejidad | `Alta`, `Media`, `Baja` |
| Objetivo | Objetivo de revision para el siguiente ciclo; usar `TBD` hasta definir una fecha |
| Orden | `Prioridad` descendente, luego `Criticidad` descendente, luego `Complejidad` descendente |

## Completados Recientemente

| FS | Historia | Estado | Fecha | Que se hizo |
|---|---|---|---|---|
| FS-22 | Solicitud y Aprobacion de Alta de Usuario | Green — Listo | 2026-06-03 | Se agrego `SignupUserCommandHandlerTests.cs` (8 tests) cubriendo el flujo publico del solicitante: caso feliz, email duplicado, tenant invalido y sin admin activo. Documentacion actualizada con badge de estado, referencia de trazabilidad corregida y seccion 10 con evidencia de pruebas de aceptacion. |
| FS-10 | B2B External Access Request and Approval Flow | Green — Listo | 2026-06-03 | Guard `PROFILE_INTERNAL_ONLY` en `CreateApprovalRequestCommandHandler` (bloquea usuarios externos/B2B de workflows solo para internos con error de escalacion de privilegios). Notificaciones de aprobacion y rechazo en `ApproveRequestCommandHandler` y `RejectRequestCommandHandler`. Nueva constante de error `WorkflowNotAllowedForUserCategory`. 6 tests nuevos. 571 tests pasan. |
| FS-11 | Upload and Validate User Document | Green — Listo | 2026-06-03 | Templates `UserDocumentRejected` y `UserDocumentValidated`. Notificaciones en `RejectUserDocumentCommandHandler` y `ValidateUserDocumentCommandHandler`. `RecordEnforcementExecutedCommand` + handler + endpoint `POST /user-documents/{id}/enforcement`. 8 tests nuevos: notificaciones, guardas de FSM (`WhenNotPendingReview`, `WhenAlreadyExpired`, `WhenDocumentIsValid`), enforcement. 579 tests pasan. |

## Registro de Brechas Abiertas

| FS | Historia | Senal | Prioridad | Criticidad | Complejidad | Responsable | Objetivo | Estado | Brecha principal | Siguiente accion |
|---|---|---|---|---|---|---|---|---|---|---|
| FS-10 | B2B External Access Request and Approval Flow | Green | P1 | Alta | Alta | Approvals | 2026-06-03 | Listo | Guard y notificaciones de ciclo de vida implementados. | — |
| FS-11 | Upload and Validate User Document | Green | P1 | Alta | Alta | Compliance | 2026-06-03 | Listo | Notificaciones, enforcement recording y tests de edge cases implementados. | — |
| FS-13 | Configure Hierarchical System Parameters | Amber | P1 | Alta | Alta | Plataforma / Configuracion | TBD | Abierto | La parametrizacion existe, pero el contexto formal de Configuration sigue sin su superficie API completa. | Implementar de punta a punta las APIs de `AppConfiguration`, `FeatureFlag` e `IdpConfiguration`. |
| FS-20 | Manage System Parameters | Amber | P1 | Alta | Alta | Plataforma / Configuracion | TBD | Abierto | El tracker aun marca al trinomio de configuracion como faltante como contexto completo de API. | Construir repositorios, comandos, REST, GraphQL y persistencia para el contexto de configuracion. |
| FS-23 | Profile Access Request from Lobby User | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El modelo de solicitud aun necesita el rol pedido y la fidelidad de auditoria esperada por el diseno. | Extender el contrato de la solicitud y el seguimiento de su ciclo de vida. |
| FS-24 | Profile Request Approval and Manual Assignment | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El registro de decision aun necesita rol solicitado, rol otorgado, razon y resultado de notificacion. | Extender el payload del resultado de aprobacion y el modelo de persistencia. |
| FS-09 | Adaptive MFA and Passwordless Authentication | Amber | P1 | Alta | Alta | Identity / Security | TBD | Abierto | MFA tiene soporte de dominio, pero passwordless, decisiones adaptativas de riesgo y exposicion activa de endpoints siguen incompletos. | Reactivar las rutas de API y completar el flujo adaptativo/passwordless. |
| FS-12 | Execute Role Promotion Process | Amber | P1 | Alta | Alta | IGA | TBD | Abierto | El flujo de promocion aun necesita la revision completa de manager/seguridad, ejecucion, verificacion y cierre del analisis de impacto. | Terminar la maquina de estados de promocion y alinear los pasos de aprobacion con el contrato de dominio. |
| FS-14 | Delegate User Management Between Administrators | Amber | P2 | Media | Media | Identity | TBD | Abierto | La delegacion existe como modelo, pero el alcance de punta a punta y el flujo de auditoria aun necesitan validacion final. | Cerrar la cobertura de acciones delegadas y verificar la ruta de aceptacion. |
| FS-15 | Configure Expiration Notification Rules | Amber | P2 | Media | Media | Compliance | TBD | Abierto | La gestion de reglas de notificacion no esta completa para actualizaciones y cambios de configuracion. | Agregar endpoints de mutacion y completar el ciclo de vida de la regla de notificacion. |
| FS-16 | Define Access Policy on Expiration | Amber | P2 | Media | Media | Compliance | TBD | Abierto | La politica de enforcement aun carece de cobertura completa de actualizacion y registro de ejecucion. | Implementar la accion de actualizacion y los eventos trazables de enforcement. |
| FS-17 | Maintain Roles for a System Suite | Amber | P2 | Media | Media | Authorization | TBD | Abierto | La cobertura del ciclo de vida de roles aun no esta documentada ni verificada como completa de punta a punta. | Cerrar el flujo de mantenimiento de roles y refrescar la trazabilidad. |
| FS-19 | Manage Admin Password Reset Validity | Amber | P2 | Media | Baja | Identity | TBD | Abierto | El ciclo de vida de validez de reset de password de admin aun no esta formalizado como una funcionalidad completa. | Definir el flujo y alinearlo con el modelo de configuracion. |
| FS-25 | Manage Domain Resources with DDD Hierarchy | Amber | P2 | Baja | Baja | Arquitectura | TBD | Abierto | Esta historia es mas un objetivo de trazabilidad arquitectura-dominio y el mapeo de implementacion aun no es totalmente explicito. | Agregar un mapa concreto de implementacion o reducir el alcance si la historia sigue siendo solo documental. |
| FS-06 | Auto-Assign Template on Profile Creation | Red | P3 | Media | Alta | Authorization | TBD | Diferido | `TemplateAssignmentRule` esta diferida de forma intencional y el motor de automatizacion no forma parte del alcance actual. | Revisar el motor de reglas solo cuando la autoasignacion sea un compromiso formal de producto. |

## Cadencia de Revision

- Actualizar este tracker cada vez que una historia cambie en el backlog o el tracker de implementacion cambie.
- Re-auditar las brechas abiertas despues de cualquier cambio de dominio, API o documentacion que afecte a las historias listadas.
- Mantener sincronizadas las versiones en ingles y espanol en estructura y contenido.

## Ultima Revision

2026-06-03
