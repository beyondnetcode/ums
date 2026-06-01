# FS-24: Aprobacion de Solicitud de Perfil y Asignacion Manual

## 1. Proposito de Negocio

Los administradores de tenant y gerentes de sucursal delegados necesitan revisar solicitudes de perfil antes de que los usuarios reciban acceso operativo. UMS debe soportar aprobacion, modificacion y denegacion para que el perfil asignado corresponda a la necesidad real de negocio, el ciclo de vida de la solicitud quede cerrado y la decision quede auditada.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Tenant Admin | Revisa solicitudes de perfil en todo el tenant y asigna el perfil final. |
| Gerente de Sucursal | Revisa solicitudes de perfil dentro del alcance de sucursal delegado. |
| Usuario en Lobby | Recibe la decision y obtiene acceso solo despues de que se asigna un perfil. |
| Auditor | Revisa quien aprobo el acceso, cuando y que rol fue otorgado. |

## 3. Precondiciones de Negocio

- Existe una solicitud de perfil en estado pendiente de asignacion.
- El aprobador tiene autoridad sobre el tenant o la sucursal.
- El sistema, la sucursal y el rol solicitados siguen activos.

## 4. Flujo Funcional Principal

1. El aprobador abre la bandeja de solicitudes de perfil.
2. El aprobador revisa usuario, sistema, sucursal, rol sugerido y justificacion.
3. El sistema advierte al aprobador sobre conflictos visibles o condiciones de riesgo de rol.
4. El aprobador elige uno de tres resultados: aprobar como fue solicitado, aprobar con un rol diferente o denegar.
5. Si se aprueba, el sistema asigna el perfil final al usuario.
6. El sistema registra quien aprobo la asignacion, cuando ocurrio y que rol fue otorgado.
7. Si se deniega, el sistema cierra la solicitud sin asignar perfil.
8. El usuario recibe una notificacion indicando que la decision del perfil fue completada.

## 5. Flujos Alternativos y Excepciones

### A. Aprobacion con Rol Modificado

El aprobador puede aprobar la solicitud con un rol distinto al solicitado cuando se requiere un nivel de acceso menor o mas adecuado.

### B. Denegacion

El aprobador puede denegar la solicitud. El usuario permanece sin acceso operativo para ese sistema y sucursal.

### C. Advertencia de Conflicto

Si el rol seleccionado entra en conflicto con roles existentes o crea un riesgo de segregacion de funciones, el sistema advierte al aprobador antes de completar la decision.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | La aprobacion debe limitarse al alcance de tenant o sucursal delegada del aprobador. |
| BR-02 | El aprobador puede otorgar el rol solicitado o un rol distinto dentro de su autoridad. |
| BR-03 | La denegacion debe mantener al usuario sin acceso operativo para el alcance solicitado. |
| BR-04 | El rol final, aprobador, fecha de decision y motivo deben quedar auditados. |
| BR-05 | Las advertencias de conflicto de roles y segregacion de funciones deben mostrarse antes de la aprobacion final cuando sean detectables. |
| BR-06 | Toda solicitud de perfil debe llegar a un estado terminal Aprobado o Denegado. |
| BR-07 | La decision final debe disparar una notificacion automatica al solicitante. |
| BR-08 | Las solicitudes no pueden eliminarse u ocultarse como sustituto de una decision final. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un aprobador autorizado puede ver solicitudes de perfil pendientes dentro de su alcance. |
| 2 | El aprobador puede aprobar el rol solicitado. |
| 3 | El aprobador puede aprobar la solicitud con un rol diferente. |
| 4 | El aprobador puede denegar la solicitud. |
| 5 | El sistema registra aprobador, fecha, rol otorgado y resultado de la decision. |
| 6 | El usuario recibe notificacion despues de aprobacion o denegacion. |
| 7 | El sistema advierte sobre conflictos de roles detectables antes de aprobar. |
| 8 | Una solicitud decidida queda cerrada y ya no aparece como pendiente. |

## 8. Requisitos Tecnicos

- Reutilizar o extender el modelo de aprobaciones para decisiones `PROFILE_ASSIGNMENT`.
- Guardar el rol solicitado y el rol otorgado por separado.
- Registrar metadata de decision en registros de auditoria inmutables.
- Invocar la asignacion de perfil solo despues de la aprobacion.
- Invalidar el grafo de autorizacion del usuario despues de asignar el perfil.
- Preparar el modelo de ruteo de aprobaciones para delegacion por sucursal.
- Exponer estados terminales como Aprobado y Denegado en vistas de usuario y auditoria, aunque eventos internos reutilicen terminologia existente de rechazo.
- Emitir eventos o plantillas de notificacion para resultados finales aprobados y denegados.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias Relacionadas | FS-23, FS-05, FS-07, FS-14 |
| Entidades de Dominio | `ApprovalRequest`, `Profile`, `Role`, `UserAccount`, `Branch` |
| Eventos de Dominio | `ProfileAssignedToUserEvent`, `ApprovalRequestApprovedEvent`, `ApprovalRequestDeniedEvent` |
| Notificaciones | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
