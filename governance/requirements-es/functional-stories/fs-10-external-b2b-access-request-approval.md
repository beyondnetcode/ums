# 🧪 Functional Story 10: Flujo de Aprobación y Petición de Acceso Externo B2B

Este documento especifica el flujo de transacciones, actores, precondiciones, postcondiciones y manejo de excepciones para patrocinar, aprobar y aprovisionar el acceso a organizaciones B2B externas (clientes, proveedores, socios) y a sus usuarios bajo la **estrategia spec-driven AI BMAD-METHOD**.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Flujo de Aprobación y Petición de Acceso Externo B2B |
| **Actor Principal** | Usuario Patrocinador (Empleado Corporativo Interno) |
| **Actor Secundario** | Administrador PAP (Aprobador), Usuario Externo Objetivo |
| **Precondiciones** | El Usuario Patrocinador está autenticado y pertenece a una organización tipo INTERNAL. Posee el permiso CREATE_EXTERNAL_REQUEST. |
| **Postcondiciones** | Se crea un rastro de auditoría inmutable EXTERNAL_ACCESS_REQUEST. El Usuario y Organización externos son aprovisionados y aislados lógicamente. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal (Happy Path)
1. **Iniciación:** El Usuario Patrocinador navega al módulo de "Gestión de Accesos B2B" y selecciona "Nueva Solicitud Externa".
2. **Ingreso de Datos de Organización:** El Patrocinador especifica la organización objetivo. Si la organización no existe, proporciona el nombre legal, código de referencia ERP y el tipo de organización (`CLIENT` o `SUPPLIER`).
3. **Ingreso de Datos de Usuario:** El Patrocinador ingresa el correo electrónico del usuario externo objetivo y selecciona un Perfil permitido (Ej. "Portal Proveedor - Solo Lectura").
4. **Justificación:** El Patrocinador escribe una justificación de negocio obligatoria y envía el formulario.
5. **Creación de Registro:** El Motor UMS en .NET 8 crea un registro `EXTERNAL_ACCESS_REQUEST` con estado `PENDING_APPROVAL`.
6. **Notificación:** El Motor UMS despacha una notificación interna a los Administradores PAP avisando de una solicitud pendiente.
7. **Evaluación:** Un Administrador PAP revisa la solicitud en la cola de pendientes y hace clic en "Aprobar".
8. **Aprovisionamiento (Basado en Eventos):**
    * El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `APPROVED` junto con el rastro de auditoría `approved_by`.
    * El Motor UMS crea la `ORGANIZATION` externa (si no existía) vinculada al `tenant_id` principal.
    * El Motor UMS pre-registra al `USER` con el `PROFILE` solicitado.
9. **Onboarding:** El Motor UMS envía un Enlace Mágico seguro (Passwordless) o un Correo de Bienvenida al correo del usuario externo para completar el alta en el sistema.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Rechazo por el PAP
* Si el Administrador PAP hace clic en "Rechazar" y proporciona un motivo de rechazo: El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `REJECTED`. El Patrocinador es notificado, y no se aprovisionan entidades externas.

### Flujo Alternativo B: Perfil Solicitado Inválido
* Si el Patrocinador intenta eludir los controles de la interfaz y envía un payload a la API solicitando un Perfil interno altamente privilegiado: El Motor UMS rechaza la petición retornando una excepción `403 Forbidden`, registrando un evento de auditoría de seguridad por intento de escalada de privilegios.

### Flujo Alternativo C: La Organización Externa Ya Existe
* Si el Motor detecta que la organización ya existe durante el aprovisionamiento: El Motor adjunta de forma segura el nuevo usuario a la organización existente en lugar de lanzar un error de conflicto.

---

## 📋 4. Postcondiciones y Auditoría
* Existe un rastro de auditoría inmutable (`EXTERNAL_ACCESS_REQUEST`) que explica exactamente por qué y quién otorgó acceso a la entidad externa.
* El Usuario externo está lógicamente aislado dentro de su propia frontera `ORGANIZATION`, permitiendo la aplicación nativa de Seguridad a Nivel de Fila (RLS) en **SQL Server 2022** mediante `SESSION_CONTEXT` y Security Policies.
