# BC-I — Compliance Context

**Schema:** `[ums_compliance]` | **Owner:** UMS Core API .NET 8  
**Mision:** Hacer cumplir politicas de acceso basadas en documentos. Gestionar ciclo de vida documental, evaluar expiracion, despachar notificaciones y ejecutar enforcement automatizado.  
**FS cubiertos:** FS-11, FS-15, FS-16  
**Version:** 2.0 | **Fecha:** 2026-05-15

---

## Agregados

| Agregado | Raiz | Descripcion |
|---------|------|-------------|
| [DocumentType](#aggregate-documenttype) | `DocumentType` | Catalogo de tipos de documento con reglas y politicas |
| [UserDocument](#aggregate-userdocument) | `UserDocument` | Instancia de documento de un usuario con su estado |

---

## Aggregate: DocumentType

**Aggregate Root:** `DocumentType`  
**FS:** FS-11, FS-15, FS-16

### Entidades

| Entidad | Descripcion |
|---------|-------------|
| `DocumentType` (AR) | Catalogo de tipos de documento; define criticidad y reglas |
| `NotificationRule` | Alerta N-step pre-expiracion configurable por canal |
| `AccessEnforcementPolicy` | Accion automatica ejecutada al vencer el documento |

### Value Objects

| Value Object | Tipo | Regla |
|-------------|------|-------|
| `DocumentCriticity` | enum | `LOW / MEDIUM / HIGH / CRITICAL` |
| `NotificationChannel` | enum | `EMAIL / SMS / IN_APP / WEBHOOK` |
| `EnforcementAction` | enum | `BLOCK_ACCESS / NOTIFY_ONLY / DOWNGRADE_ROLE / SUSPEND` |
| `DaysBefore` | int | > 0; dias antes del vencimiento para notificar |
| `GracePeriodDays` | int? | Dias de gracia post-expiracion antes de enforcement |
| `RenewalPeriodDays` | int? | Dias de anticipacion para renovacion |

### Invariantes

| ID | Regla | Fuente |
|----|-------|--------|
| INV-DT1 | `CRITICAL` debe tener al menos una `AccessEnforcementPolicy` configurada | ADR-0045, FS-16 |
| INV-DT2 | `NotificationRule.DaysBefore` valores unicos y descendentes por tipo | ADR-0045, FS-15 |
| INV-DT3 | Solo una `AccessEnforcementPolicy` activa por `(DocumentTypeId, TenantId)` | FS-16 |
| INV-DT4 | Documentos no-criticos no pueden tener politica `BLOCK_ACCESS` o `DOWNGRADE_ROLE` | FS-16 |
| INV-DT5 | `NotificationRule` requiere `Code, Value (DaysBefore), Description` obligatorios | FS-15, database-design-er.md Regla 9 |

### Comandos

| Comando | Descripcion |
|---------|-------------|
| `RegisterDocumentTypeCommand` | Registra tipo de documento con criticidad |
| `ConfigureNotificationRuleCommand` | Agrega regla de notificacion (FS-15) |
| `RemoveNotificationRuleCommand` | Elimina regla de notificacion |
| `DefineEnforcementPolicyCommand` | Define politica de enforcement (FS-16) |
| `UpdateEnforcementPolicyCommand` | Actualiza la politica existente |

### Eventos de Dominio

```
DocumentTypeRegisteredEvent     { documentTypeId, criticity, tenantId }
NotificationRuleConfiguredEvent { ruleId, documentTypeId, daysBefore, channels }
EnforcementPolicyDefinedEvent   { policyId, documentTypeId, actionOnExpiration }
```

---

## Aggregate: UserDocument

**Aggregate Root:** `UserDocument`  
**FS:** FS-11, FS-15, FS-16

### Entidades

| Entidad | Descripcion |
|---------|-------------|
| `UserDocument` (AR) | Documento especifico de un usuario con su estado de validez |
| `AccessNotification` | Registro de cada notificacion despachada para este documento |

### Value Objects

| Value Object | Tipo | Regla |
|-------------|------|-------|
| `DocumentStatus` | enum | `PENDING_REVIEW / VALID / EXPIRED / REJECTED` |
| `IssueDate` | DateOnly | Fecha de emision del documento |
| `ExpirationDate` | DateOnly | Debe ser > `IssueDate` |
| `FileStoragePath` | string | URI valida al almacenamiento de objetos |
| `FileChecksum` | string | Hash de integridad del archivo |
| `NotificationStep` | int | Ultimo paso de notificacion ejecutado (0 = ninguno) |

### Invariantes

| ID | Regla | Fuente |
|----|-------|--------|
| INV-UD1 | `ExpirationDate > IssueDate` | FS-11 |
| INV-UD2 | `REJECTED` no puede transicionar a `VALID` directamente; requiere nuevo upload | ADR-0045 |
| INV-UD3 | Un documento `VALID` que supera `ExpirationDate` transiciona a `EXPIRED` por Background Worker | ADR-0045 |
| INV-UD4 | Solo un documento `VALID` activo por `(UserId, DocumentTypeId)` | ADR-0045 |
| INV-UD5 | `FileStoragePath` debe ser URI valida accesible via `IDocumentStoragePort` | tecnico |

### Maquina de Estado: UserDocument

> **Visualizacion:** [interactive-ddd-viewer.html](./interactive-ddd-viewer.html) — seccion "UserDocument"

```mermaid
stateDiagram-v2
    [*] --> PENDING_REVIEW : UploadDocument
    PENDING_REVIEW --> VALID : ValidateDocument (reviewer)
    PENDING_REVIEW --> REJECTED : RejectDocument (reviewer)
    VALID --> EXPIRED : ExpirationDate superada (Background Worker)
    EXPIRED --> PENDING_REVIEW : UploadDocument (nuevo)
    REJECTED --> PENDING_REVIEW : UploadDocument (nuevo)
```

### Comandos

| Comando | Descripcion |
|---------|-------------|
| `UploadDocumentCommand` | Carga un nuevo documento con fechas y ubicacion de archivo |
| `ValidateDocumentCommand` | El reviewer valida el documento -> VALID |
| `RejectDocumentCommand` | El reviewer rechaza el documento con razon |
| `ExpireDocumentCommand` | Background Worker vence documentos con ExpirationDate pasada |
| `RecordNotificationSentCommand` | Registra que se envio la notificacion N-step |

### Eventos de Dominio

```
DocumentUploadedEvent       { documentId, userId, documentTypeId, expirationDate }
DocumentValidatedEvent      { documentId, userId, validatedBy }
DocumentRejectedEvent       { documentId, userId, rejectionReason }
DocumentNearExpirationEvent { documentId, userId, documentTypeId, daysRemaining, step }
DocumentExpiredEvent        { documentId, userId, documentTypeId, criticity, enforcementAction }
EnforcementExecutedEvent    { documentId, userId, action, executedAt }
```

---

**[Anterior: IGA Context](./08-iga-context.md)** | **[Indice DDD](./index.md)** | **[Siguiente: Cross-Context Flows](./10-cross-context-flows.md)**
