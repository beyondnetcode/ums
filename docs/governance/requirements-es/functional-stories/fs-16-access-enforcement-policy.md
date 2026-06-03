# Functional Story 16: Definir Política de Acceso por Vencimiento

> **Estado:** Implementado

## 1. Propósito de Negocio

Los equipos de seguridad y cumplimiento necesitan definir qué debe ocurrir cuando un documento crítico de usuario vence. UMS debe aplicar consecuencias de acceso predecibles manteniendo visible y auditable la razón.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Arquitecto de Seguridad** | Define el impacto de acceso por documentos críticos vencidos. |
| **Administrador Global** | Publica o actualiza políticas de cumplimiento. |
| **Usuario Afectado** | Recibe restricciones o advertencias según la política. |
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
3. Las restricciónes quedan trazables y visibles para administradores.
4. Renovar un documento válido puede restaurar acceso según la política.

---

## 8. Requisitos Técnicos

> [!NOTE]
> En la implementación real de C# (base de código), `AccessEnforcementPolicy` es una Entidad hija encapsulada dentro del Agregado **DocumentType**, bajo el espacio de nombres unificado **Ums.Domain.Approvals**.

- Persistir políticas como parte del Agregado Root `DocumentType`.
- Campos obligatorios: `Code`, `Value` (JSON con acciones de la política), `Description`.
- Aplicar unicidad por `Code`, alcance de tenant y `DocumentTypeId`.
- Acciones soportadas: `BLOCK_USER`, `RESTRICT_PROFILE` y `LOG_ONLY`.
- Permitir actualizaciones de la acción de la política mediante `PUT /access-enforcement-policies/{policyId}/action`.
- Registrar la ejecucion del enforcement a traves del flujo de documento de usuario para que el resultado aplicado siga siendo trazable.
- Emitir eventos de dominio y auditoría cuando se aplican o revierten restricciones.

---

## 9. Trazabilidad

- Entidades: `DocumentType` (AR), `AccessEnforcementPolicy` (Entidad Hija), `UserAccount` (AR), `Profile` (AR)
- ADRs: ADR-0045, ADR-0035
- Historias relacionadas: FS-11, FS-15

## 10. Evidencia de Pruebas de Aceptacion

- [`AccessEnforcementPolicyE2ETests.cs`](../../../../src/apps/ums.api/Ums.Presentation.IntegrationTest/E2E/AccessEnforcementPolicyE2ETests.cs) cubre creacion de la politica, GET por ID, actualizacion de la accion, desactivacion y la validacion cuando no se suministra `ProfileId` ni `RoleId`.
- [`UpdateAccessEnforcementActionCommandValidatorTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Approvals/AccessEnforcementPolicy/Commands/UpdateAccessEnforcementActionCommandValidatorTests.cs) verifica que el comando acepte los nombres de accion del dominio `BlockUser`, `RestrictProfile` y `LogOnly`.
- [`AccessEnforcementPolicyCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Approvals/AccessEnforcementPolicy/AccessEnforcementPolicyCommandHandlerTests.cs) cubre los handlers de crear, desactivar y actualizar.
- [`UserDocumentEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Approvals/UserDocument/UserDocumentEndpoints.cs) expone la ruta de ejecucion de enforcement usada para preservar la trazabilidad cuando se aplica la politica.
