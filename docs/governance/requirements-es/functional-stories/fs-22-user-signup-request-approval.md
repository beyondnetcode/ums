# FS-22: Solicitud y Aprobacion de Alta de Usuario

> **Estado:** Implementado

## 1. Proposito de Negocio

Los usuarios que desean unirse a un tenant existente necesitan una ruta controlada de solicitud en lugar de una creacion directa de cuentas. UMS debe permitir que el solicitante elija el tenant, envie la solicitud, la mantenga pendiente y obligar al responsable del tenant a cerrarla con una aprobacion o denegacion explicita desde una bandeja dedicada.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Solicitante | Pide acceso a un tenant existente y envia los datos de identidad requeridos. |
| Tenant Admin | Revisa las solicitudes pendientes de acceso del tenant y aprueba o deniega cuando corresponde. |
| Solicitante | Recibe la notificacion final de onboarding despues de que la solicitud es aprobada o denegada. |

## 3. Precondiciones de Negocio

- El tenant objetivo ya existe en UMS.
- El solicitante tiene nombre, correo electronico y contrasena que cumplen las reglas de alta.
- El Tenant Admin tiene acceso a la bandeja de onboarding del tenant.

## 4. Flujo Funcional Principal

1. El solicitante abre la experiencia de login y elige la opcion de solicitud de acceso.
2. El solicitante selecciona el tenant al que desea pedir acceso.
3. El solicitante ingresa nombre, correo y contrasena.
4. El sistema crea la cuenta de usuario con estado pendiente y muestra un mensaje de confirmacion.
5. La solicitud pendiente aparece en la bandeja de onboarding del tenant.
6. Un Tenant Admin revisa la solicitud y la cierra aprobando o denegando el requerimiento.
7. Si se aprueba, el sistema activa la cuenta de usuario y notifica al usuario que el acceso fue aprobado.
8. Si se deniega, el sistema cierra la solicitud sin activar el acceso y notifica al usuario que el acceso fue denegado.

## 5. Flujos Alternativos y Excepciones

### A. Tenant Invalido

Si el solicitante selecciona un tenant que no existe o no esta disponible para onboarding, la solicitud no se envia.

### B. Correo Ya Registrado

Si el correo ya existe para el mismo tenant, el sistema no crea una cuenta duplicada.

### C. Solicitud Aun Sin Decision

Si el Tenant Admin aun no toma una decision final, la solicitud permanece pendiente y el solicitante no puede iniciar sesion.

### D. Solicitud Denegada

Si el Tenant Admin deniega la solicitud, el requerimiento llega a un estado terminal denegado, el solicitante no puede iniciar sesion y la denegacion se comunica automaticamente.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Las solicitudes de alta de usuario pertenecen solo al tenant objetivo. |
| BR-02 | Los Tenant Admins solo pueden revisar solicitudes de su propio tenant. |
| BR-03 | Una solicitud pendiente no otorga acceso hasta ser aprobada. |
| BR-04 | La aprobacion debe activar la cuenta y notificar al solicitante. |
| BR-05 | Las solicitudes pendientes deben ser visibles en una bandeja dedicada para que no se pierdan dentro de la administracion general de cuentas. |
| BR-06 | El ciclo de vida de cada solicitud debe ser trazable dentro del tenant objetivo desde el envio hasta el cierre final. |
| BR-07 | Los unicos resultados finales de negocio son Aprobado y Denegado. |
| BR-08 | La decision final es obligatoria; la solicitud no puede eliminarse silenciosamente ni quedar sin registro de cierre. |
| BR-09 | La decision final debe disparar una notificacion automatica al solicitante con el resultado de acceso. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un solicitante puede enviar una solicitud de alta completa para un tenant existente. |
| 2 | La solicitud se guarda como pendiente y aparece en la bandeja de onboarding del tenant. |
| 3 | Solo el tenant objetivo puede ver la solicitud. |
| 4 | Un Tenant Admin puede aprobar o denegar la solicitud desde la bandeja. |
| 5 | La aprobacion activa la cuenta de usuario y notifica al solicitante. |
| 6 | La denegacion cierra la solicitud sin activar acceso y notifica al solicitante. |
| 7 | Las solicitudes pendientes no permiten acceso antes de la aprobacion. |
| 8 | El sistema guarda tenant, estado actual, resultado de decision, fecha de decision, aprobador y motivo cuando aplique. |

## 8. Requisitos Tecnicos

- Persistir la solicitud como un `UserAccount` en estado `Pending` en lugar de crear una tabla separada de onboarding.
- Mantener anonimo el endpoint publico de solicitud y usar el cliente publico sin encabezados de tenant.
- Usar scope por tenant para asegurar que las consultas de la bandeja solo devuelvan las cuentas pendientes del tenant actual.
- Reutilizar el flujo de activacion existente para aprobar la solicitud y emitir la notificacion de aprobacion.
- Agregar un comando explicito de denegacion o una operacion equivalente de aplicacion que registre denegacion terminal sin activar la cuenta.
- Persistir metadata de ciclo de vida para envio, decision, aprobador, estado final y motivo opcional de denegacion en forma auditable.
- Mantener la bandeja como una composicion UI sobre el listado de cuentas para que el flujo siga alineado con el modelo central de cuentas.
- Agregar o mapear plantillas de notificacion para resultados finales aprobados y denegados.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias Funcionales | FS-22 |
| Entidades de Dominio | `UserAccount`, `Tenant` |
| Notificaciones | `UserSignupRequestReceived`, `UserSignupApproved`, `UserSignupDenied` |
| ADRs | ADR-0075 |

## 10. Evidencia de Pruebas de Aceptacion

- [`UserAccountOnboardingCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Identity/UserAccount/UserAccountOnboardingCommandHandlerTests.cs) cubre visibilidad de solicitudes pendientes, denegacion, alcance por tenant y manejo de estados finales.
- [`UserAccountCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Identity/UserAccount/UserAccountCommandHandlerTests.cs) cubre la activacion de una cuenta pendiente y la transicion al estado aprobado usada por el flujo de onboarding.
- [`UserAccountEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/UserAccount/UserAccountEndpoints.cs) y [`OnboardingInboxEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Onboarding/OnboardingInboxEndpoints.cs) exponen la bandeja y las rutas de accion terminal usadas por la historia.
