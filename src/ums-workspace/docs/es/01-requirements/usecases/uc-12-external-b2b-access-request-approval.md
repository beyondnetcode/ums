# 📖 UC-12: Flujo de Aprobación y Petición de Acceso Externo B2B

Este documento especifica el flujo de transacciones, actores, precondiciones, postcondiciones y manejo de excepciones para patrocinar, aprobar y aprovisionar el acceso a organizaciones B2B externas (clientes, proveedores, socios) y a sus usuarios bajo la **estrategia spec-driven AI BMAD-METHOD**.

---

## 🎯 1. Objetivo
Proporcionar un flujo de trabajo seguro, auditable y federado que permita a los usuarios corporativos internos (Patrocinadores) solicitar acceso para organizaciones de terceros y sus empleados, garantizando una aprobación explícita desde un Punto de Administración de Políticas (PAP) antes de que cualquier identidad externa sea aprovisionada o se le otorgue acceso al UMS.

---

## 🎭 2. Actores
*   **Usuario Patrocinador (Interno):** Un empleado corporativo (Ej. Ejecutivo Comercial, Encargado de Compras) que inicia la solicitud de acceso para una organización de terceros.
*   **Administrador PAP / Propietario:** Un usuario autorizado con privilegios de gobernanza que evalúa y aprueba o rechaza la solicitud de acceso externo.
*   **Usuario Externo (Objetivo):** El empleado de terceros (Cliente/Proveedor) que recibirá la invitación e ingresará al sistema una vez aprobado.
*   **Motor UMS:** El procesador en segundo plano que maneja el despacho de eventos, la creación de organizaciones, el pre-registro de usuarios y la entrega de correos electrónicos.

---

## 🚦 3. Precondiciones
1.  El Usuario Patrocinador debe tener una sesión activa y pertenecer a una organización interna (`ORGANIZATION.type = INTERNAL`).
2.  El Usuario Patrocinador debe poseer el permiso `CREATE_EXTERNAL_REQUEST` en su grafo de autorización compilado.
3.  El Perfil sugerido para el usuario externo debe estar explícitamente marcado como seguro para uso externo (Ej. no puede ser un perfil de Administrador interno).

---

## 🚀 4. Escenario Principal de Éxito (Happy Path)

1.  **Iniciación:** El Usuario Patrocinador navega al módulo de "Gestión de Accesos B2B" y selecciona "Nueva Solicitud Externa".
2.  **Ingreso de Datos de Organización:** El Patrocinador especifica la organización objetivo. Si la organización no existe, proporciona el nombre legal, código de referencia ERP y el tipo de organización (`CLIENT` o `SUPPLIER`).
3.  **Ingreso de Datos de Usuario:** El Patrocinador ingresa el correo electrónico del usuario externo objetivo y selecciona un Perfil permitido (Ej. "Portal Proveedor - Solo Lectura").
4.  **Justificación:** El Patrocinador escribe una justificación de negocio obligatoria y envía el formulario.
5.  **Creación de Registro:** El Motor UMS crea un registro `EXTERNAL_ACCESS_REQUEST` con estado `PENDING_APPROVAL`.
6.  **Notificación:** El Motor UMS despacha una notificación interna (o correo electrónico) a los Administradores PAP avisando de una solicitud pendiente.
7.  **Evaluación:** Un Administrador PAP revisa la solicitud en la cola de pendientes y hace clic en "Aprobar".
8.  **Aprovisionamiento (Basado en Eventos):**
    *   El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `APPROVED` con el rastro de auditoría `approved_by`.
    *   El Motor UMS crea la `ORGANIZATION` externa (si no existía) vinculada al `tenant_id` principal.
    *   El Motor UMS pre-registra el `USER` con el `PROFILE` solicitado.
9.  **Onboarding:** El Motor UMS envía un Enlace Mágico seguro (Passwordless) o un Correo de Bienvenida al correo del usuario externo para completar el alta en el sistema.

---

## 🛑 5. Rutas de Excepción

*   **5a. Rechazo por el PAP:**
    *   *Alternativa al Paso 7:* El Administrador PAP hace clic en "Rechazar" y proporciona un motivo de rechazo.
    *   *Resolución:* El estado del `EXTERNAL_ACCESS_REQUEST` se actualiza a `REJECTED`. El Patrocinador es notificado, y no se aprovisionan entidades externas.
*   **5b. Perfil Solicitado Inválido:**
    *   *Alternativa al Paso 3:* El Patrocinador intenta eludir los controles de la interfaz y envía un payload API solicitando un Perfil interno altamente privilegiado.
    *   *Resolución:* El Motor UMS rechaza el payload retornando una excepción `403 Forbidden`, registrando un evento de auditoría de seguridad por intento de escalada de privilegios.
*   **5c. La Organización Externa Ya Existe:**
    *   *Alternativa al Paso 8:* El Motor detecta que la organización ya existe durante el aprovisionamiento.
    *   *Resolución:* El Motor adjunta de forma segura el nuevo usuario a la organización existente en lugar de lanzar un error de conflicto.

---

## 🏁 6. Postcondiciones
*   Existe un rastro de auditoría inmutable (`EXTERNAL_ACCESS_REQUEST`) que explica exactamente por qué y quién otorgó acceso a la entidad externa.
*   El Usuario externo está lógicamente aislado dentro de su propia frontera `ORGANIZATION`, permitiendo la aplicación nativa de Seguridad a Nivel de Fila (RLS).
