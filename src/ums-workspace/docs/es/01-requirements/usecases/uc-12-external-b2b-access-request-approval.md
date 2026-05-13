# ðŸ“– UC-12: Flujo de AprobaciÃ³n y PeticiÃ³n de Acceso Externo B2B

Este documento especifica el flujo de transacciones, actores, precondiciones, postcondiciones y manejo de excepciones para patrocinar, aprobar y aprovisionar el acceso a organizaciones B2B externas (clientes, proveedores, socios) y a sus usuarios bajo la **estrategia spec-driven AI BMAD-METHOD**.

---

## ðŸŽ¯ 1. Objetivo
Proporcionar un flujo de trabajo seguro, auditable y federado que permita a los usuarios corporativos internos (Patrocinadores) solicitar acceso para organizaciones de terceros y sus empleados, garantizando una aprobaciÃ³n explÃ­cita desde un Punto de AdministraciÃ³n de PolÃ­ticas (PAP) antes de que cualquier identidad externa sea aprovisionada o se le otorgue acceso al UMS.

---

## ðŸŽ­ 2. Actores
*   **Usuario Patrocinador (Interno):** Un empleado corporativo (Ej. Ejecutivo Comercial, Encargado de Compras) que inicia la solicitud de acceso para una organizaciÃ³n de terceros.
*   **Administrador PAP / Propietario:** Un usuario autorizado con privilegios de gobernanza que evalÃºa y aprueba o rechaza la solicitud de acceso externo.
*   **Usuario Externo (Objetivo):** El empleado de terceros (Cliente/Proveedor) que recibirÃ¡ la invitaciÃ³n e ingresarÃ¡ al sistema una vez aprobado.
*   **Motor UMS:** El procesador en segundo plano que maneja el despacho de eventos, la creaciÃ³n de organizaciones, el pre-registro de usuarios y la entrega de correos electrÃ³nicos.

---

## ðŸš¦ 3. Precondiciones
1.  El Usuario Patrocinador debe tener una sesiÃ³n activa y pertenecer a una organizaciÃ³n interna (`ORGANIZATION.type = INTERNAL`).
2.  El Usuario Patrocinador debe poseer el permiso `CREATE_EXTERNAL_REQUEST` en su grafo de autorizaciÃ³n compilado.
3.  El Perfil sugerido para el usuario externo debe estar explÃ­citamente marcado como seguro para uso externo (Ej. no puede ser un perfil de Administrador interno).

---

## ðŸš€ 4. Escenario Principal de Ã‰xito (Happy Path)

1.  **IniciaciÃ³n:** El Usuario Patrocinador navega al mÃ³dulo de "GestiÃ³n de Accesos B2B" y selecciona "Nueva Solicitud Externa".
2.  **Ingreso de Datos de OrganizaciÃ³n:** El Patrocinador especifica la organizaciÃ³n objetivo. Si la organizaciÃ³n no existe, proporciona el nombre legal, cÃ³digo de referencia ERP y el tipo de organizaciÃ³n (`CLIENT` o `SUPPLIER`).
3.  **Ingreso de Datos de Usuario:** El Patrocinador ingresa el correo electrÃ³nico del usuario externo objetivo y selecciona un Perfil permitido (Ej. "Portal Proveedor - Solo Lectura").
4.  **JustificaciÃ³n:** El Patrocinador escribe una justificaciÃ³n de negocio obligatoria y envÃ­a el formulario.
5.  **CreaciÃ³n de Registro:** El Motor UMS crea un registro `EXTERNAL_ACCESS_REQUEST` con estado `PENDING_APPROVAL`.
6.  **NotificaciÃ³n:** El Motor UMS despacha una notificaciÃ³n interna (o correo electrÃ³nico) a los Administradores PAP avisando de una solicitud pendiente.
7.  **EvaluaciÃ³n:** Un Administrador PAP revisa la solicitud en la cola de pendientes y hace clic en "Aprobar".
8.  **Aprovisionamiento (Basado en Eventos):**
    *   El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `APPROVED` con el rastro de auditorÃ­a `approved_by`.
    *   El Motor UMS crea la `ORGANIZATION` externa (si no existÃ­a) vinculada al `tenant_id` principal.
    *   El Motor UMS pre-registra el `USER` con el `PROFILE` solicitado.
9.  **Onboarding:** El Motor UMS envÃ­a un Enlace MÃ¡gico seguro (Passwordless) o un Correo de Bienvenida al correo del usuario externo para completar el alta en el sistema.

---

## ðŸ›‘ 5. Rutas de ExcepciÃ³n

*   **5a. Rechazo por el PAP:**
    *   *Alternativa al Paso 7:* El Administrador PAP hace clic en "Rechazar" y proporciona un motivo de rechazo.
    *   *ResoluciÃ³n:* El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `REJECTED`. El Patrocinador es notificado, y no se aprovisionan entidades externas.
*   **5b. Perfil Solicitado InvÃ¡lido:**
    *   *Alternativa al Paso 3:* El Patrocinador intenta eludir los controles de la interfaz y envÃ­a un payload API solicitando un Perfil interno altamente privilegiado.
    *   *ResoluciÃ³n:* El Motor UMS rechaza el payload retornando una excepciÃ³n `403 Forbidden`, registrando un evento de auditorÃ­a de seguridad por intento de escalada de privilegios.
*   **5c. La OrganizaciÃ³n Externa Ya Existe:**
    *   *Alternativa al Paso 8:* El Motor detecta que la organizaciÃ³n ya existe durante el aprovisionamiento.
    *   *ResoluciÃ³n:* El Motor adjunta de forma segura el nuevo usuario a la organizaciÃ³n existente en lugar de lanzar un error de conflicto.

---

## ðŸ 6. Postcondiciones
*   Existe un rastro de auditorÃ­a inmutable (`EXTERNAL_ACCESS_REQUEST`) que explica exactamente por quÃ© y quiÃ©n otorgÃ³ acceso a la entidad externa.
*   El Usuario externo estÃ¡ lÃ³gicamente aislado dentro de su propia frontera `ORGANIZATION`, permitiendo la aplicaciÃ³n nativa de Seguridad a Nivel de Fila (RLS).
