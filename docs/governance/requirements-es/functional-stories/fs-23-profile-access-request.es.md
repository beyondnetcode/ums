# FS-23: Solicitud de Acceso a Perfil desde Usuario en Lobby

## 1. Propósito de Negocio

Los usuarios admitidos a un tenant todavía pueden necesitar el perfil de negocio correcto antes de usar los menús operativos. UMS debe permitir que un usuario autenticado sin perfil asignado solicite el acceso que necesita para su trabajo, sin conceder permisos automáticamente, y debe rastrear la solicitud hasta que un aprobador autorizado la cierre.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Usuario en Lobby | Usuario autenticado admitido al tenant pero sin un perfil activo. |
| Administrador de Tenant | Recibe solicitudes de perfil y valida si el acceso es apropiado. |
| Administrador de Sucursal | Puede revisar solicitudes para usuarios que trabajan en una sucursal específica cuando el tenant lo delega. |

## 3. Precondiciones de Negocio

- La cuenta del usuario está activa para el tenant.
- El usuario no tiene un perfil activo para el sistema o alcance solicitado.
- El tenant tiene al menos un sistema, una sucursal y un rol disponibles para solicitar.

## 4. Flujo Funcional Principal

1. El usuario inicia sesión correctamente y llega a la pantalla de lobby.
2. El lobby explica que el usuario pertenece al tenant, pero aún no tiene un perfil asignado.
3. El usuario abre el formulario de solicitud de perfil.
4. El usuario selecciona el sistema, luego la sucursal y luego el rol sugerido.
5. El usuario puede ingresar una justificación de negocio.
6. El sistema almacena la solicitud como pendiente de asignación.
7. La solicitud aparece en la bandeja de solicitudes de perfil del aprobador responsable.
8. El usuario puede ver que la solicitud está pendiente hasta que se tome una decisión final aprobada o denegada.

## 5. Flujos Alternativos y Excepciones

### A. No hay roles disponibles

Si no hay ningún rol disponible para el sistema y la sucursal seleccionados, el usuario no puede enviar la solicitud y se le indica que contacte al administrador del tenant.

### B. Perfil activo existente

Si el usuario ya tiene un perfil activo para el mismo sistema y la misma sucursal, el sistema impide una solicitud duplicada y muestra el estado de acceso existente.

### C. Solicitud ya pendiente

Si el mismo usuario ya tiene una solicitud pendiente para el sistema y la sucursal seleccionados, el sistema muestra la solicitud pendiente en lugar de crear otra.

## 6. Reglas de Negocio

| Regla | Descripción |
|---|---|
| BR-01 | El acceso al tenant y la autorización de perfil son fases separadas. |
| BR-02 | Un usuario en lobby puede autenticarse, pero no debe ver menús operativos hasta que se asigne un perfil. |
| BR-03 | La solicitud debe capturar sistema, sucursal, rol sugerido y justificación opcional. |
| BR-04 | La solicitud permanece pendiente hasta que un aprobador autorizado decida. |
| BR-05 | No se permiten solicitudes pendientes duplicadas para el mismo usuario, sistema y sucursal. |
| BR-06 | Cada ciclo de vida de la solicitud de perfil debe ser trazable dentro del tenant desde el envío hasta el cierre final. |
| BR-07 | Los resultados finales de negocio son Aprobado y Denegado; la aprobación puede otorgar el rol solicitado o un rol modificado. |
| BR-08 | El solicitante debe ser notificado automáticamente cuando la solicitud de perfil alcance un resultado final. |

## 7. Criterios de Aceptación

| # | Criterio de Aceptación |
|---|---|
| 1 | Un usuario activo sin perfil aterriza en el lobby después de iniciar sesión. |
| 2 | El usuario en lobby puede solicitar un perfil seleccionando sistema, sucursal y rol sugerido. |
| 3 | Las opciones de sucursal y rol dependen de las selecciones previas. |
| 4 | El usuario puede incluir una justificación de negocio. |
| 5 | La solicitud se almacena como asignación pendiente. |
| 6 | El usuario no recibe acceso a menús operativos antes de la aprobación. |
| 7 | El usuario puede ver el estado pendiente mientras la solicitud espera una decisión. |
| 8 | El usuario es notificado cuando la solicitud es aprobada o denegada. |

## 8. Requisitos Técnicos

- Introducir un modelo de solicitud de acceso a perfil o reutilizar el modelo de solicitud de aprobación existente con un tipo de solicitud de asignación de perfil.
- Persistir sistema solicitado, sucursal solicitada, rol solicitado, justificación, solicitante, tenant y estado.
- Persistir metadatos del ciclo de vida para envío, estado actual, resultado final, aprobador, fecha de decisión y motivo de decisión.
- Devolver una respuesta controlada del lobby cuando el grafo de autorización no pueda resolver un perfil activo para el usuario autenticado.
- Mantener la ruta del lobby fuera de los menús operativos y disponible solo después de autenticación correcta.
- Aplicar el filtrado de tenant en la capa de aplicación como mecanismo primario de aislamiento.
- Agregar o mapear plantillas de notificación para la aprobación y la denegación final de la solicitud de perfil.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias relacionadas | FS-22, FS-24, FS-05 |
| Entidades de dominio | `UserAccount`, `Profile`, `Role`, `SystemSuite`, `Branch`, `ApprovalRequest` |
| Notificaciones | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
