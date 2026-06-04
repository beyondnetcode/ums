# FS-24: Aprobación de Solicitud de Perfil y Asignación Manual

## 1. Propósito de Negocio

Los administradores de tenant y los administradores delegados de sucursal necesitan revisar las solicitudes de perfil antes de que los usuarios reciban acceso operativo. UMS debe soportar aprobación, modificación y denegación para que el perfil asignado coincida con la necesidad real del negocio, el ciclo de vida de la solicitud se cierre y la decisión quede auditable.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Tenant | Revisa solicitudes de perfil en todo el tenant y asigna el perfil final. |
| Administrador de Sucursal | Revisa solicitudes de perfil dentro del alcance delegado de la sucursal. |
| Usuario en Lobby | Recibe la decisión y obtiene acceso solo después de que se asigne un perfil. |
| Auditor | Revisa quién aprobó el acceso, cuándo y qué rol se otorgó. |

## 3. Precondiciones de Negocio

- Existe una solicitud de perfil en estado pendiente de asignación.
- El aprobador tiene autoridad sobre el alcance del tenant o de la sucursal.
- El sistema, la sucursal y el rol solicitados siguen activos.

## 4. Flujo Funcional Principal

1. El aprobador abre la bandeja de solicitudes de perfil.
2. El aprobador revisa el usuario, el sistema, la sucursal, el rol sugerido y la justificación.
3. El sistema advierte al aprobador sobre conflictos visibles o condiciones de riesgo de rol.
4. El aprobador elige uno de tres resultados: aprobar tal como se solicitó, aprobar con un rol diferente o denegar.
5. Si se aprueba, el sistema asigna el perfil final al usuario.
6. El sistema registra quién aprobó la asignación, cuándo ocurrió y qué rol se otorgó.
7. Si se deniega, el sistema cierra la solicitud sin asignar un perfil.
8. El usuario es notificado de que la decisión del perfil está completa.

## 5. Flujos Alternativos y Excepciones

### A. Aprobación con rol modificado

El aprobador puede aprobar la solicitud con un rol distinto del solicitado cuando se requiere un nivel de acceso más bajo o más apropiado.

### B. Denegación

El aprobador puede denegar la solicitud. El usuario permanece sin acceso operativo para ese sistema y esa sucursal.

### C. Advertencia de conflicto

Si el rol seleccionado entra en conflicto con roles existentes o crea un riesgo de segregación de funciones, el sistema advierte al aprobador antes de completar la decisión.

## 6. Reglas de Negocio

| Regla | Descripción |
|---|---|
| BR-01 | La aprobación debe limitarse al alcance del tenant o de la sucursal delegada del aprobador. |
| BR-02 | El aprobador puede otorgar el rol solicitado o un rol diferente dentro de su autoridad. |
| BR-03 | La denegación debe mantener al usuario sin acceso operativo para el alcance solicitado. |
| BR-04 | El rol final, el aprobador, la fecha de decisión y el motivo de la decisión deben ser auditables. |
| BR-05 | Las advertencias de conflicto de rol y segregación de funciones deben mostrarse antes de la aprobación final cuando puedan detectarse. |
| BR-06 | Toda solicitud de perfil debe llegar a un estado terminal Aprobado o Denegado. |
| BR-07 | Una decisión final debe activar una notificación automática al solicitante. |
| BR-08 | Las solicitudes no pueden eliminarse ni ocultarse como sustituto de una decisión final. |

## 7. Criterios de Aceptación

| # | Criterio de Aceptación |
|---|---|
| 1 | Un aprobador autorizado puede ver solicitudes de perfil pendientes dentro de su alcance. |
| 2 | El aprobador puede aprobar el rol solicitado. |
| 3 | El aprobador puede aprobar la solicitud con un rol diferente. |
| 4 | El aprobador puede denegar la solicitud. |
| 5 | El sistema registra aprobador, fecha, rol otorgado y resultado de la decisión. |
| 6 | El usuario es notificado después de la aprobación o la denegación. |
| 7 | El sistema advierte sobre conflictos de rol detectables antes de la aprobación. |
| 8 | Una solicitud decidida queda cerrada y ya no aparece como pendiente. |

## 8. Requisitos Técnicos

- Reutilizar o extender el modelo de solicitud de aprobación para decisiones de `PROFILE_ASSIGNMENT`.
- Almacenar por separado el rol solicitado y el rol otorgado.
- Registrar metadatos de decisión en registros de auditoría inmutables.
- Invocar la asignación de perfil solo después de aprobar.
- Invalidar el grafo de autorización del usuario después de la asignación de perfil.
- Preparar el modelo de enrutamiento de aprobación para la delegación con alcance de sucursal.
- Exponer los estados terminales como Aprobado y Denegado en las vistas de usuario y auditoría, aunque los eventos internos reutilicen la terminología de rechazo existente.
- Emitir eventos y plantillas de notificación para ambos resultados finales: aprobado y denegado.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias relacionadas | FS-23, FS-05, FS-07, FS-14 |
| Entidades de dominio | `ApprovalRequest`, `Profile`, `Role`, `UserAccount`, `Branch` |
| Eventos de dominio | `ProfileAssignedToUserEvent`, `ApprovalRequestApprovedEvent`, `ApprovalRequestDeniedEvent` |
| Notificaciones | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
