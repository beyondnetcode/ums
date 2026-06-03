# Reglas de Consistencia - BC IGA

> **Contexto Delimitado:** `Ums.Domain.IGA`

## PromotionRequest

| Operación | Regla | Broken Rule |
|---|---|---|
| `Submit()` | Solo en Draft | `iga.promotion_not_in_draft` |
| `ManagerApprove()` | Solo en estado pendiente de manager | `iga.promotion_not_pending_manager` |
| `SecurityReview()` | Solo en estado pendiente de security | `iga.promotion_not_pending_security` |

