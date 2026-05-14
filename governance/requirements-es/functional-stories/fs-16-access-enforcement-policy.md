# 📘 Functional Story 16: Definir Política de Acceso por Expiración

Este documento especifica el flujo para configurar las acciones automáticas que el sistema debe tomar cuando un documento crítico de un usuario expira.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Definir Política de Acceso por Expiración |
| **Actor Principal** | Arquitecto de Seguridad / Administrador Global |
| **Precondiciones** | El sistema tiene habilitada la validación de cumplimiento documental. |
| **Postcondiciones** | La política queda activa y será ejecutada por el motor de cumplimiento al detectar documentos vencidos. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El actor selecciona un tipo de documento configurado como "Crítico para el Acceso" (`IsAccessCritical = TRUE`).
2.  Define la acción a ejecutar tras la expiración:
    - **BLOCK_USER**: Desactiva el acceso total del usuario al sistema.
    - **RESTRICT_PROFILE**: Bloquea solo perfiles específicos vinculados al documento (ej. licencia de conducir expirada bloquea perfil de conductor).
    - **LOG_ONLY**: Solo genera una alerta de auditoría sin restringir el acceso.
3.  Persiste la configuración en `ACCESS_ENFORCEMENT_POLICY`.
4.  El motor de cumplimiento (Worker) evalúa diariamente la vigencia y ejecuta la acción definida de forma inmediata al detectar el vencimiento.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Re-activación tras Renovación
*   Una vez que el usuario carga un nuevo documento válido (FS-11) y este es aprobado, el sistema debe revertir automáticamente la restricción de acceso impuesta por la política.

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `ACCESS_ENFORCEMENT_POLICY`
- `DOCUMENT_TYPE`
- `USER_ACCOUNT`
- `PROFILE`

### Criterios de Aceptación
1.  La acción `BLOCK_USER` debe invalidar todas las sesiones activas del usuario de forma inmediata.
2.  Debe existir un rastro de auditoría claro que explique que el bloqueo fue "Automático por Expiración Documental".
3.  El sistema debe permitir configurar periodos de gracia antes de ejecutar el bloqueo definitivo.
