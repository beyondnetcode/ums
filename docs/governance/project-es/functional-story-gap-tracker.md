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
| Implementado / utilizable | 22 | FS-01, FS-02, FS-03, FS-04, FS-05, FS-06, FS-07, FS-08, FS-09, FS-10, FS-11, FS-15, FS-16, FS-17, FS-18, FS-19, FS-20, FS-21, FS-22, FS-25, FS-26, FS-27 |
| Parcial | 5 | FS-12, FS-13, FS-14, FS-23, FS-24 |
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
| FS-12 | Execute Role Promotion Process | Amber | P1 | Alta | Alta | IGA | TBD | Abierto | El flujo de promocion aun necesita la revision completa de manager/seguridad, ejecucion, verificacion y cierre del analisis de impacto. | Terminar la maquina de estados de promocion y alinear los pasos de aprobacion con el contrato de dominio. |
| FS-13 | Configure Hierarchical System Parameters | Amber | P1 | Alta | Alta | Plataforma / Configuracion | TBD | Abierto | La parametrizacion existe, pero el contexto formal de Configuration sigue sin su superficie API completa. | Implementar de punta a punta las APIs de `AppConfiguration`, `FeatureFlag` e `IdpConfiguration`. |
| FS-14 | Delegate User Management Between Administrators | Amber | P2 | Media | Media | Identity | TBD | Abierto | La delegacion existe como modelo, pero el alcance de punta a punta y el flujo de auditoria aun necesitan validacion final. | Cerrar la cobertura de acciones delegadas y verificar la ruta de aceptacion. |
| FS-23 | Profile Access Request from Lobby User | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El modelo de solicitud aun necesita el rol pedido y la fidelidad de auditoria esperada por el diseno. | Extender el contrato de la solicitud y el seguimiento de su ciclo de vida. |
| FS-24 | Profile Request Approval and Manual Assignment | Amber | P1 | Alta | Alta | Approvals | TBD | Abierto | El registro de decision aun necesita rol solicitado, rol otorgado, razon y resultado de notificacion. | Extender el payload del resultado de aprobacion y el modelo de persistencia. |

## Cadencia de Revision

- Actualizar este tracker cada vez que una historia cambie en el backlog o el tracker de implementacion cambie.
- Re-auditar las brechas abiertas despues de cualquier cambio de dominio, API o documentacion que afecte a las historias listadas.
- Mantener sincronizadas las versiones en ingles y espanol en estructura y contenido.

## Ultima Revision

2026-06-04 (FS-20)
