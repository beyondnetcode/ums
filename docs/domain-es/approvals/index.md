# Contexto de Aprobaciones (Approvals BC) — Arquitectura de Agregados

> **Idioma:** [English](../../domain/approvals/index.md) | [Español](./index.md)

**Contexto Delimitado:** Aprobaciones (`Ums.Domain.Approvals`)  
**Raíces de Agregado (Aggregate Roots):** `ApprovalWorkflow`, `ApprovalRequest`, `DocumentType`, `UserDocument`, `AccessEnforcementPolicy`

---

### Modelo de Flujos de Trabajo y Solicitudes
Los elementos centrales de los flujos de trabajo gobiernan el enrutamiento dinámico de aprobaciones y los eventos de verificación:
- [ApprovalWorkflow](./approval-workflow.md) (Raíz de Agregado) — Define los pasos secuenciales o paralelos de verificación requeridos para acciones sensibles.
- [ApprovalRequiredDocument](./approval-required-document.md) (Entidad Propia) — Asignación de tipos de documentos específicos necesarios para autorizar un flujo de trabajo.
- [ApprovalRequest](./approval-request.md) (Raíz de Agregado) — Una solicitud de ejecución en tiempo de ejecución concreta que contiene el estado de verificación y las firmas.

### Clasificación y Políticas de Documentos
- [DocumentType](./document-type.md) (Raíz de Agregado) — Clasificación de documentos de verificación (ej., Identificación, Comprobante de Domicilio).
- [NotificationRule](./notification-rule.md) (Entidad Propia) — Define los días previos a la expiración, los canales (correo electrónico, SMS) y las frecuencias para activar alertas de notificación.
- [UserDocument](./user-document.md) (Raíz de Agregado) — El registro físico del archivo subido que pertenece a un usuario, almacenando su estado de verificación.
- [AccessNotification](./access-notification.md) (Entidad Propia) — Historial de alertas enviadas para el cumplimiento de documentos.
- [AccessEnforcementPolicy](./access-enforcement-policy.md) (Raíz de Agregado) — Define los bloqueos automáticos de cuentas o rebajas de perfiles de seguridad ante el incumplimiento de documentos.

---

**[Volver al Índice de Dominio](../index.md)**
