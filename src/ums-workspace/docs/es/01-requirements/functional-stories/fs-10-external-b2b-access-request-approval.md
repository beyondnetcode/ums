# ðŸ§ª Functional Story 10: Flujo de AprobaciÃ³n y PeticiÃ³n de Acceso Externo B2B

Este documento especifica el flujo de transacciones, actores, precondiciones, postcondiciones y manejo de excepciones para patrocinar, aprobar y aprovisionar el acceso a organizaciones B2B externas (clientes, proveedores, socios) y a sus usuarios bajo la **estrategia spec-driven AI BMAD-METHOD**.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Flujo de AprobaciÃ³n y PeticiÃ³n de Acceso Externo B2B |
| **Actor Principal** | Usuario Patrocinador (Empleado Corporativo Interno) |
| **Actor Secundario** | Administrador PAP (Aprobador), Usuario Externo Objetivo |
| **Precondiciones** | El Usuario Patrocinador estÃ¡ autenticado y pertenece a una organizaciÃ³n tipo INTERNAL. Posee el permiso CREATE_EXTERNAL_REQUEST. |
| **Postcondiciones** | Se crea un rastro de auditorÃ­a inmutable EXTERNAL_ACCESS_REQUEST. El Usuario y OrganizaciÃ³n externos son aprovisionados y aislados lÃ³gicamente. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

### A. Flujo Principal (Happy Path)
1. **IniciaciÃ³n:** El Usuario Patrocinador navega al mÃ³dulo de "GestiÃ³n de Accesos B2B" y selecciona "Nueva Solicitud Externa".
2. **Ingreso de Datos de OrganizaciÃ³n:** El Patrocinador especifica la organizaciÃ³n objetivo. Si la organizaciÃ³n no existe, proporciona el nombre legal, cÃ³digo de referencia ERP y el tipo de organizaciÃ³n (`CLIENT` o `SUPPLIER`).
3. **Ingreso de Datos de Usuario:** El Patrocinador ingresa el correo electrÃ³nico del usuario externo objetivo y selecciona un Perfil permitido (Ej. "Portal Proveedor - Solo Lectura").
4. **JustificaciÃ³n:** El Patrocinador escribe una justificaciÃ³n de negocio obligatoria y envÃ­a el formulario.
5. **CreaciÃ³n de Registro:** El Motor UMS en .NET 8 crea un registro `EXTERNAL_ACCESS_REQUEST` con estado `PENDING_APPROVAL`.
6. **NotificaciÃ³n:** El Motor UMS despacha una notificaciÃ³n interna a los Administradores PAP avisando de una solicitud pendiente.
7. **EvaluaciÃ³n:** Un Administrador PAP revisa la solicitud en la cola de pendientes y hace clic en "Aprobar".
8. **Aprovisionamiento (Basado en Eventos):**
    * El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `APPROVED` junto con el rastro de auditorÃ­a `approved_by`.
    * El Motor UMS crea la `ORGANIZATION` externa (si no existÃ­a) vinculada al `tenant_id` principal.
    * El Motor UMS pre-registra al `USER` con el `PROFILE` solicitado.
9. **Onboarding:** El Motor UMS envÃ­a un Enlace MÃ¡gico seguro (Passwordless) o un Correo de Bienvenida al correo del usuario externo para completar el alta en el sistema.

---

## ðŸ›¡ï¸ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Rechazo por el PAP
* Si el Administrador PAP hace clic en "Rechazar" y proporciona un motivo de rechazo: El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `REJECTED`. El Patrocinador es notificado, y no se aprovisionan entidades externas.

### Flujo Alternativo B: Perfil Solicitado InvÃ¡lido
* Si el Patrocinador intenta eludir los controles de la interfaz y envÃ­a un payload a la API solicitando un Perfil interno altamente privilegiado: El Motor UMS rechaza la peticiÃ³n retornando una excepciÃ³n `403 Forbidden`, registrando un evento de auditorÃ­a de seguridad por intento de escalada de privilegios.

### Flujo Alternativo C: La OrganizaciÃ³n Externa Ya Existe
* Si el Motor detecta que la organizaciÃ³n ya existe durante el aprovisionamiento: El Motor adjunta de forma segura el nuevo usuario a la organizaciÃ³n existente en lugar de lanzar un error de conflicto.

---

## ðŸ“‹ 4. Postcondiciones y AuditorÃ­a
* Existe un rastro de auditorÃ­a inmutable (`EXTERNAL_ACCESS_REQUEST`) que explica exactamente por quÃ© y quiÃ©n otorgÃ³ acceso a la entidad externa.
* El Usuario externo estÃ¡ lÃ³gicamente aislado dentro de su propia frontera `ORGANIZATION`, permitiendo la aplicaciÃ³n nativa de Seguridad a Nivel de Fila (RLS) en PostgreSQL.
