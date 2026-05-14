# Functional Story 16: Definir Política de Acceso por Vencimiento

## 1. Propósito de Negocio

Los equipos de seguridad y cumplimiento necesitan definir qué debe ocurrir cuando un documento crítico de usuario vence. UMS debe aplicar consecuencias de acceso predecibles manteniendo visible y auditable la razón.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Arquitecto de Seguridad** | Define el impacto de acceso por documentos críticos vencidos. |
| **Administrador Global** | Publica o actualiza políticas de cumplimiento. |
| **Usuario Afectado** | Recibe restricciones o advertencias según la política. |

---

## 3. Precondiciones de Negocio

- La validación de cumplimiento documental está habilitada.
- El tipo de documento está marcado como relevante para control de acceso.
- El actor tiene permiso para gestionar políticas de enforcement.

---

## 4. Flujo Funcional Principal

1. El actor selecciona un tipo de documento que puede afectar el acceso.
2. El actor elige la consecuencia de negocio que debe aplicarse tras el vencimiento.
3. El actor define si aplica un periodo de gracia.
4. El actor proporciona una descripción clara del propósito e impacto de la política.
5. El sistema guarda y activa la política.
6. Cuando vence el documento de un usuario, el sistema aplica la consecuencia configurada.
7. El usuario y los administradores pueden ver por qué se aplicó la restricción o advertencia.

---

## 5. Flujos Alternativos y Excepciones

### A. Reactivación Después de Renovación

Cuando el usuario entrega un documento renovado y aprobado, el sistema retira la restricción según la política configurada.

### B. Documento No Crítico

Si el tipo de documento seleccionado no es crítico para acceso, el sistema impide publicar una política de bloqueo y sugiere una regla solo informativa.

---

## 6. Reglas de Negocio

1. El vencimiento de un documento crítico puede bloquear acceso, restringir perfiles o solo generar advertencia de auditoría.
2. Las políticas deben incluir `code`, `value` y `description`.
3. El usuario debe poder entender por qué se restringió el acceso.
4. La renovación debe permitir restaurar acceso cuando se cumplan las condiciones de la política.

---

## 7. Criterios de Aceptación

1. Un administrador puede configurar una consecuencia para un tipo de documento crítico.
2. El sistema impide políticas de bloqueo sobre documentos no críticos.
3. Las restricciones quedan trazables y visibles para administradores.
4. Renovar un documento válido puede restaurar acceso según la política.

---

## 8. Requisitos Técnicos

- Persistir políticas en `ACCESS_ENFORCEMENT_POLICY`.
- Campos obligatorios: `Code`, `Value`, `Description`.
- Aplicar unicidad por `Code`, alcance de tenant y `DocumentTypeId`.
- Acciones soportadas: `BLOCK_USER`, `RESTRICT_PROFILE` y `LOG_ONLY`.
- Emitir eventos de auditoría cuando se aplican o revierten restricciones.

---

## 9. Trazabilidad

- Entidades: `ACCESS_ENFORCEMENT_POLICY`, `DOCUMENT_TYPE`, `USER_ACCOUNT`, `PROFILE`
- ADRs: ADR-0045, ADR-0035
- Historias relacionadas: FS-11, FS-15
