# ADR-0075: Bandeja de Aprobacion de Onboarding y Autorizacion por Alcance

**Estado:** Accepted  
**Fecha:** 2026-06-01  
**Contexto:** UMS Identity, Onboarding de Tenants, Onboarding de Usuarios

## Contexto

UMS ahora soporta dos flujos de onboarding de identidad distintos y debe separarlos de la asignacion de entitlements:

| Flujo | Fuente de verdad | Alcance de aprobacion |
|---|---|---|
| Onboarding de tenant | Agregado `TenantSignupRequest` | Global, solo System Admin |
| Onboarding de usuario | `UserAccount` en estado `Pending` | Por tenant, solo Tenant Admin |

El producto necesita un lugar unico y facil de encontrar para revisar elementos pendientes de onboarding, pero los objetos de negocio subyacentes no son iguales. Las solicitudes de alta de empresa representan un agregado dedicado, mientras que las solicitudes de alta de usuario ya estan modeladas por el ciclo de vida de una cuenta pendiente.

La arquitectura tambien debe evitar un atajo riesgoso: aprobar el ingreso de un usuario a un tenant no debe otorgar automaticamente permisos operativos. Un usuario puede estar activo en el tenant y aun permanecer en estado de lobby hasta que solicite y se apruebe un perfil.

El producto ahora requiere que el ciclo de vida completo de las solicitudes de alta de usuario y de perfil sea trazable por tenant. Los administradores deben cerrar cada requerimiento con una decision terminal de negocio, Aprobado o Denegado, y el solicitante debe recibir una notificacion automatica cuando se alcance ese resultado final.

## Decision

UMS expondrá una **Bandeja de Aprobacion de Onboarding** como una superficie compuesta de UI y consulta, no como un nuevo agregado o tabla generica de inbox.

| Area de Decision | Eleccion |
|---|---|
| Ubicacion UI | Agregar una nueva opcion de menu a nivel Identity para aprobaciones de onboarding. |
| Fuente de datos de onboarding de tenant | Consultar registros `TenantSignupRequest` pendientes. |
| Fuente de datos de onboarding de usuario | Consultar registros `UserAccount` pendientes dentro del scope del tenant actual. |
| Onboarding de entitlements | Enrutar solicitudes de perfil por el modelo de aprobaciones antes de asignar un `Profile`. |
| Estado de lobby | Usuarios autenticados sin perfil activo reciben una experiencia controlada de lobby en vez de menus operativos. |
| Autorizacion de aprobacion | Requerir capacidad explicita por alcance, no solo visibilidad de pantalla. |
| Cierre de ciclo de vida | Requerir resultados terminales Aprobado o Denegado para solicitudes de alta de usuario y de perfil. |
| Modelo de persistencia | Reutilizar los agregados existentes; no crear una tabla generica de inbox. |
| Preparacion para pago futuro | Mantener extensible el modelo de estados del onboarding de empresa para verificacion de pago. |

## Por Que Esta Decision

1. Mantiene honesto el modelo de negocio. El alta de empresa y el alta de usuario estan relacionadas, pero no son el mismo objeto.
2. Evita duplicacion innecesaria. Una tabla generica de inbox duplicaria estado que ya pertenece a los agregados.
3. Preserva el aislamiento por tenant. Las aprobaciones de alta de usuario no deben cruzar de tenant.
4. Mejora la usabilidad. Los aprobadores necesitan un lugar unico y descubible para trabajar, aunque los registros origen sean distintos.
5. Evita que la admision al tenant se convierta en autorizacion implicita.
6. Permite evolucion futura. Estados adicionales, como verificacion de pago, pueden agregarse sin cambiar el punto de entrada del operador.

## Modelo de Autorizacion

El acceso a aprobaciones debe depender de una capacidad explicita, no solo de la visibilidad del menu.

| Capacidad | Alcance | Accion permitida |
|---|---|---|
| `ApproveTenantSignup` | Global | Ver y aprobar o rechazar solicitudes de alta de empresa. |
| `ApproveUserSignup` | Tenant actual | Ver y aprobar o denegar solicitudes de alta de usuario solo del tenant activo. |
| `RequestProfileAccess` | Tenant actual | Enviar una solicitud de perfil desde el lobby. |
| `ApproveProfileRequest` | Tenant o sucursal delegada | Aprobar, modificar o denegar solicitudes de perfil. |
| `ViewOnboardingInbox` | Global o por tenant | Abrir la bandeja e inspeccionar items pendientes. |

La primera version puede aplicar esto mediante verificaciones de rol y contexto de tenant, pero la expectativa arquitectonica es que la accion siga siendo gobernada por autorizacion y totalmente auditable.

## Modelo de Datos y Estados

### Onboarding de empresa

| Estado | Significado |
|---|---|
| Pending | La solicitud fue enviada y espera una decision global. |
| Approved | El tenant fue creado y se aprovisiono la primera cuenta admin. |
| Rejected | La solicitud se cerro sin crear el tenant. |

### Onboarding de usuario

| Estado | Significado |
|---|---|
| Pending | La cuenta fue creada, pero aun no fue activada. |
| ActiveWithoutProfile | El Tenant Admin aprobo la solicitud, el usuario puede autenticarse, pero no tiene perfil operativo asignado. |
| Active | El usuario tiene al menos un perfil activo y puede recibir menus operativos segun el grafo de autorizacion. |
| Denied | La solicitud se cerro sin activar acceso al tenant. |

### Onboarding de solicitud de perfil

| Estado | Significado |
|---|---|
| PendingAssignment | El usuario solicito sistema, sucursal y rol sugerido. |
| Approved | La solicitud se cerro con un rol asignado. El rol otorgado puede coincidir con el solicitado o ser modificado por el aprobador. |
| Denied | La solicitud se cerro sin asignar perfil para el alcance solicitado. |

Las etiquetas de estado terminal son terminos de negocio. Los eventos internos o valores de persistencia existentes que usen terminologia de rechazo deben traducirse consistentemente en los boundaries de aplicacion para que usuarios, administradores y auditores vean Aprobado o Denegado como resultados finales.

## Alternativas Consideradas

| Alternativa | Decision | Motivo |
|---|---|---|
| Crear una tabla generica de inbox de onboarding | Rechazada | Duplica estado de los agregados e introduce acoplamiento entre flujos no identicos. |
| Meter el onboarding de empresa dentro del listado de usuarios | Rechazada | Oculta un proceso global dentro de una pantalla con scope de tenant. |
| Depender solo de ocultar pantallas por rol | Rechazada | Ocultar UI no es un modelo de autorizacion y es debil para aprobaciones. |
| Crear inbox separados con opciones de menu separadas | Rechazada | Fragmenta la experiencia del operador y hace menos descubrible la aprobacion. |
| Activar usuario y asignar perfil por defecto inmediatamente | Rechazada | Colapsa admision de identidad y aprobacion de entitlements, aumentando el riesgo de acceso indebido. |

## Consecuencias

### Positivas

- La experiencia del operador es mas simple.
- El modelo de dominio se mantiene limpio y explicito.
- El aislamiento por tenant sigue siendo verificable en el boundary de consulta.
- Los usuarios pueden ser admitidos al tenant sin recibir entitlements operativos.
- Los estados futuros de verificacion pueden introducirse sin redisenar la bandeja.

### Compromisos

- La bandeja es un read model compuesto, por lo que la UI debe combinar dos fuentes de verdad.
- Los permisos de aprobacion deben documentarse y aplicarse de forma consistente en endpoints y pantallas.
- El login debe manejar el caso sin perfil como un estado de lobby de primera clase.
- Los caminos de denegacion de alta de usuario y solicitud de perfil requieren comandos explicitos, metadata de auditoria y plantillas de notificacion.

## Notas de Implementacion

| Area | Guia |
|---|---|
| Navegacion | Ubicar la bandeja dentro del modulo Identity, no dentro de Authorization. |
| Consultas | Componer la bandeja con solicitudes de tenant pendientes, cuentas de usuario pendientes y solicitudes pendientes de asignacion de perfil. |
| Comandos | Mantener separados el comando de aprobacion de alta de tenant, la activacion de cuenta, la denegacion de alta de usuario, la aprobacion de asignacion de perfil y la denegacion de solicitud de perfil. |
| Lobby | Enrutar usuarios autenticados sin perfil activo a una pantalla de lobby con formulario de solicitud de perfil. |
| Notificaciones | Continuar usando notificaciones simuladas en UI y plantillas de notificacion en backend; toda aprobacion o denegacion terminal debe notificar al solicitante. |
| Politica futura | Si se introduce verificacion de pago, modelarla como un estado o politica adicional de onboarding, no como un flujo de usuario independiente. |
