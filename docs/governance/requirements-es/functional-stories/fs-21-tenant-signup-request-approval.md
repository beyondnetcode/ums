# FS-21: Solicitud y Aprobacion de Alta de Empresa

## 1. Proposito de Negocio

Las nuevas empresas necesitan una forma controlada de solicitar acceso a UMS e iniciar su onboarding sin que un equipo operativo cree el tenant manualmente. UMS debe capturar la solicitud, mantenerla pendiente hasta que un administrador global la revise y crear el tenant solo despues de la aprobacion.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| System Admin | Revisa las solicitudes de alta de empresa y decide si una nueva compania ingresa a UMS. |
| Contacto de la Empresa | Envia los datos de la compania y recibe el resultado del onboarding. |
| Usuario Administrador del Tenant | Recibe la primera cuenta administrativa creada para la compania aprobada. |

## 3. Precondiciones de Negocio

- La compania aun no esta registrada como tenant activo en UMS.
- El solicitante cuenta con nombre de empresa, codigo de referencia, nombre de contacto y correo de contacto validos.
- El System Admin tiene acceso al area de revision de onboarding.

## 4. Flujo Funcional Principal

1. El representante de la compania abre la opcion de registro desde la experiencia de login.
2. El representante ingresa el nombre de la compania, el codigo de referencia, el nombre de contacto y el correo de contacto.
3. El sistema registra la solicitud como pendiente y muestra un mensaje de confirmacion.
4. La solicitud aparece en la bandeja global de onboarding para System Admins.
5. Un System Admin revisa la solicitud y decide si la compania debe ser admitida.
6. Si la solicitud se aprueba, el sistema crea el tenant y el primer usuario administrativo de ese tenant.
7. El sistema genera una contrasena temporal para el primer usuario administrativo y notifica al contacto de la empresa.
8. La solicitud de onboarding permanece trazable con su resultado final.

## 5. Flujos Alternativos y Excepciones

### A. Codigo de Empresa Duplicado

Si el codigo de referencia ya pertenece a un tenant existente, el sistema no crea un tenant duplicado y la solicitud no se aprueba.

### B. Solicitud Rechazada

Si el System Admin rechaza la solicitud, la empresa no recibe un tenant y la solicitud queda marcada como rechazada.

### C. Verificacion de Pago Futura

Si mas adelante el negocio requiere verificacion de pago antes de admitir la empresa, el flujo de onboarding debe soportar estados de revision adicionales sin cambiar el punto de entrada publico.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Las solicitudes de alta de empresa pertenecen al alcance global y solo las revisan System Admins. |
| BR-02 | Un tenant solo se crea despues de aprobar la solicitud. |
| BR-03 | El tenant aprobado debe recibir su primer usuario administrativo como parte del mismo resultado de aprobacion. |
| BR-04 | La solicitud de onboarding debe permanecer auditada desde su envio hasta la decision final. |
| BR-05 | Se pueden agregar estados futuros de revision para verificacion de pago, pero el flujo publico debe mantenerse estable. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Una empresa puede enviar una solicitud de alta con los datos requeridos de compania y contacto. |
| 2 | La solicitud aparece como pendiente en la bandeja global de onboarding. |
| 3 | Solo los System Admins pueden revisar y aprobar o rechazar la solicitud. |
| 4 | La aprobacion crea el tenant y la primera cuenta administradora del tenant. |
| 5 | El contacto de la empresa recibe el resultado del onboarding y las credenciales temporales despues de la aprobacion. |
| 6 | Las solicitudes rechazadas no crean tenants ni usuarios administrativos. |

## 8. Requisitos Tecnicos

- Persistir las solicitudes de onboarding en el agregado `TenantSignupRequest` con estados `Pending`, `Approved` y `Rejected`.
- Mantener anonimo y sin contexto de tenant el punto de entrada publico de la solicitud.
- Usar un read model compuesto de bandeja de aprobacion en la UI en lugar de una tabla generica adicional de inbox.
- Crear el tenant y el primer usuario administrativo como parte del comando de aprobacion.
- Enviar la notificacion de aprobacion con la contrasena temporal generada y los datos de la cuenta.
- Reservar espacio para futuros estados de verificacion de pago en el modelo de estados y la documentacion.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Historias Funcionales | FS-22 |
| Entidades de Dominio | `TenantSignupRequest`, `Tenant`, `UserAccount` |
| Notificaciones | `TenantSignupRequestReceived`, `TenantSignupApproved` |
| ADRs | ADR-0075 |

