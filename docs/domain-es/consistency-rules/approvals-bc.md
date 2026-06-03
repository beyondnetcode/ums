# Reglas de Consistencia - BC Approvals

> **Contexto Delimitado:** `Ums.Domain.Approvals`

## ApprovalWorkflow

| Operación | Regla | Broken Rule |
|---|---|---|
| `Create()` | Si `RequiresApproval=true`, debe existir al menos un required document | `approvals.requires_documents_if_approval_required` |
| `RemoveRequiredDocument()` | Si `RequiresApproval=true`, debe quedar al menos un documento | `approvals.requires_documents_if_approval_required` |

