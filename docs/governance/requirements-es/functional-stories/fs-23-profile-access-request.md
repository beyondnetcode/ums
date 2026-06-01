# FS-23: Solicitud de Perfil desde Usuario en Lobby

## 1. Proposito de Negocio

Los usuarios admitidos a un tenant aun pueden necesitar un perfil de negocio antes de usar menus operativos. UMS debe permitir que un usuario autenticado sin perfil asignado solicite el acceso necesario para su trabajo, sin otorgar permisos automaticamente, y debe trazar la solicitud hasta que un aprobador autorizado la cierre.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Usuario en Lobby | Usuario autenticado y admitido al tenant, pero sin perfil activo. |
| Tenant Admin | Recibe solicitudes de perfil y valida si el acceso corresponde. |
| Gerente de Sucursal | Puede revisar solicitudes de usuarios de una sucursal especifica cuando exista delegacion del tenant. |

## 3. Precondiciones de Negocio

- La cuenta del usuario esta activa para el tenant.
- El usuario no tiene perfil activo para el sistema o alcance solicitado.
- El tenant tiene al menos un sistema, una sucursal y un rol disponibles para solicitud.

## 4. Flujo Funcional Principal

1. El usuario inicia sesion correctamente y llega a la pantalla de lobby.
2. El lobby explica que el usuario pertenece al tenant pero aun no tiene perfil asignado.
3. El usuario abre el formulario de solicitud de perfil.
4. El usuario selecciona el sistema, luego la sucursal y finalmente el rol sugerido.
5. El usuario puede ingresar una justificacion de negocio.
6. El sistema guarda la solicitud como pendiente de asignacion.
7. La solicitud aparece en la bandeja de solicitudes de perfil del aprobador responsable.
8. El usuario puede ver que la solicitud sigue pendiente hasta que exista una decision final aprobada o denegada.

## 5. Flujos Alternativos y Excepciones

### A. Sin Roles Disponibles

Si no existe un rol disponible para el sistema y la sucursal seleccionados, el usuario no puede enviar la solicitud y se le indica contactar al administrador del tenant.

### B. Perfil Activo Existente

Si el usuario ya tiene un perfil activo para el mismo sistema y sucursal, el sistema evita una solicitud duplicada y muestra el estado de acceso existente.

### C. Solicitud Ya Pendiente

Si el mismo usuario ya tiene una solicitud pendiente para el sistema y sucursal seleccionados, el sistema muestra la solicitud pendiente en vez de crear otra.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | El acceso al tenant y la autorizacion por perfil son fases separadas. |
| BR-02 | Un usuario en lobby puede autenticarse pero no debe ver menus operativos hasta tener un perfil asignado. |
| BR-03 | La solicitud debe capturar sistema, sucursal, rol sugerido y justificacion opcional. |
| BR-04 | La solicitud permanece pendiente hasta que un aprobador autorizado decida. |
| BR-05 | No se permiten solicitudes pendientes duplicadas para el mismo usuario, sistema y sucursal. |
| BR-06 | El ciclo de vida de cada solicitud de perfil debe ser trazable dentro del tenant desde el envio hasta el cierre final. |
| BR-07 | Los resultados finales de negocio son Aprobado y Denegado; la aprobacion puede otorgar el rol solicitado o un rol modificado. |
| BR-08 | El solicitante debe ser notificado automaticamente cuando la solicitud de perfil llegue a un resultado final. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un usuario activo sin perfil aterriza en el lobby despues del login. |
| 2 | El usuario en lobby puede solicitar un perfil seleccionando sistema, sucursal y rol sugerido. |
| 3 | Las opciones de sucursal y rol dependen de las selecciones previas. |
| 4 | El usuario puede incluir una justificacion de negocio. |
| 5 | La solicitud se guarda como pendiente de asignacion. |
| 6 | El usuario no recibe acceso a menus operativos antes de la aprobacion. |
| 7 | El usuario puede ver el estado pendiente mientras la solicitud espera decision. |
| 8 | El usuario recibe notificacion cuando la solicitud es aprobada o denegada. |

## 8. Requisitos Tecnicos

- Introducir un modelo de solicitud de acceso a perfil o reutilizar el modelo existente de aprobaciones con un tipo de solicitud de asignacion de perfil.
- Persistir sistema solicitado, sucursal, rol solicitado, justificacion, solicitante, tenant y estado.
- Persistir metadata de ciclo de vida para envio, estado actual, resultado final, aprobador, fecha de decision y motivo de decision.
- Retornar una respuesta controlada de lobby cuando el grafo de autorizacion no pueda resolver un perfil activo para el usuario autenticado.
- Mantener la ruta de lobby fuera de los menus operativos y disponible solo despues de autenticacion exitosa.
- Aplicar filtrado por tenant en capa de aplicacion como mecanismo primario de aislamiento.
- Agregar o mapear plantillas de notificacion para aprobacion y denegacion final de solicitud de perfil.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias Relacionadas | FS-22, FS-24, FS-05 |
| Entidades de Dominio | `UserAccount`, `Profile`, `Role`, `SystemSuite`, `Branch`, `ApprovalRequest` |
| Notificaciones | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
