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
| Implementado / utilizable | 22 | [FS-01](../requirements-es/functional-stories/fs-01-user-authentication.md), [FS-02](../requirements-es/functional-stories/fs-02-create-authorization-template.md), [FS-03](../requirements-es/functional-stories/fs-03-register-organization.md), [FS-04](../requirements-es/functional-stories/fs-04-register-system-topology.md), [FS-05](../requirements-es/functional-stories/fs-05-create-profile-manual-template.md), [FS-06](../requirements-es/functional-stories/fs-06-auto-assign-template.md), [FS-07](../requirements-es/functional-stories/fs-07-visual-graph-resolver.md), [FS-08](../requirements-es/functional-stories/fs-08-hosted-login-redirection.md), [FS-09](../requirements-es/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md), [FS-10](../requirements-es/functional-stories/fs-10-external-b2b-access-request-approval.md), [FS-11](../requirements-es/functional-stories/fs-11-user-document-upload.md), [FS-15](../requirements-es/functional-stories/fs-15-notification-rules.md), [FS-16](../requirements-es/functional-stories/fs-16-access-enforcement-policy.md), [FS-17](../requirements-es/functional-stories/fs-17-maintain-system-roles.md), [FS-18](../requirements-es/functional-stories/fs-18-manage-local-user-password.md), [FS-19](../requirements-es/functional-stories/fs-19-admin-password-reset-validity-management.md), [FS-20](../requirements-es/functional-stories/fs-20-system-parameter-management.md), [FS-21](../requirements-es/functional-stories/fs-21-tenant-signup-request-approval.md), [FS-22](../requirements-es/functional-stories/fs-22-user-signup-request-approval.md), [FS-25](../requirements/functional-stories/fs-25-ddd-domain-resource-hierarchy.es.md), [FS-26](../requirements/functional-stories/fs-26-auth-graph-preview-from-profile.es.md), [FS-27](../requirements/functional-stories/fs-27-state-change-consistency-broken-rules.es.md) |
| Parcial | 5 | [FS-12](../requirements-es/functional-stories/fs-12-role-promotion-process.md), [FS-13](../requirements-es/functional-stories/fs-13-hierarchical-config.md), [FS-14](../requirements-es/functional-stories/fs-14-delegated-management.md), [FS-23](../requirements-es/functional-stories/fs-23-profile-access-request.md), [FS-24](../requirements-es/functional-stories/fs-24-profile-request-approval.md) |
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
| [FS-13](../requirements-es/functional-stories/fs-13-hierarchical-config.md) | Configure Hierarchical System Parameters | Amber | P1 | Alta | Alta | Plataforma / Configuracion | TBD | Abierto | La parametrizacion existe, pero el contexto formal de Configuration sigue sin su superficie API completa. | Implementar de punta a punta las APIs de `AppConfiguration`, `FeatureFlag` e `IdpConfiguration`. |
| [FS-14](../requirements-es/functional-stories/fs-14-delegated-management.md) | Delegate User Management Between Administrators | Amber | P2 | Media | Media | Identity | TBD | Abierto | La delegacion existe como modelo, pero el alcance de punta a punta y el flujo de auditoria aun necesitan validacion final. | Cerrar la cobertura de acciones delegadas y verificar la ruta de aceptacion. |
| [FS-23](../requirements-es/functional-stories/fs-23-profile-access-request.md) | Profile Access Request from Lobby User | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El modelo de solicitud aun necesita el rol pedido y la fidelidad de auditoria esperada por el diseno. | Extender el contrato de la solicitud y el seguimiento de su ciclo de vida. |
| [FS-24](../requirements-es/functional-stories/fs-24-profile-request-approval.md) | Profile Request Approval and Manual Assignment | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El registro de decision aun necesita rol solicitado, rol otorgado, razon y resultado de notificacion. | Extender el payload del resultado de aprobacion y el modelo de persistencia. |

## Cadencia de Revision

- Actualizar este tracker cada vez que una historia cambie en el backlog o el tracker de implementacion cambie.
- Re-auditar las brechas abiertas despues de cualquier cambio de dominio, API o documentacion que afecte a las historias listadas.
- Mantener sincronizadas las versiones en ingles y espanol en estructura y contenido.

## Ultima Revision

2026-06-04 (FS-25)
