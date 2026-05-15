# Flujos de Eventos Cross-Contexto

**Tipo:** DDD — Inter-Context Event Flows  
**Version:** 2.0 | **Fecha:** 2026-05-15  

> **Visualizacion interactiva:** [interactive-ddd-viewer.html](./interactive-ddd-viewer.html) — seccion "Flujos Cross-Contexto"

---

## Flujo 1: Onboarding de Usuario Externo B2B (FS-10)

```mermaid
sequenceDiagram
    participant Sponsor as Sponsor User
    participant Approvals as BC-F Approvals
    participant Identity as BC-A Identity
    participant Auth as BC-B Authorization
    participant Audit as BC-D Audit

    Sponsor->>Approvals: SubmitApprovalRequest (ONBOARDING, B2B)
    Approvals->>Audit: ApprovalRequestSubmittedEvent
    Approvals->>Approvals: Notifica al rol aprobador
    Note over Approvals: Estado: PENDING

    Approvals->>Approvals: ApproveRequest (PAP Admin)
    Approvals->>Identity: ApprovalRequestApprovedEvent
    Approvals->>Audit: ApprovalRequestApprovedEvent

    Identity->>Identity: ActivateUser (PENDING -> ACTIVE)
    Identity->>Auth: UserActivatedEvent
    Identity->>Audit: UserActivatedEvent

    Auth->>Auth: AssignProfileToUser
    Auth->>Audit: ProfileAssignedToUserEvent
```

---

## Flujo 2: Expiracion de Documento Critico (FS-16)

```mermaid
sequenceDiagram
    participant Worker as Background Worker
    participant Compliance as BC-I Compliance
    participant Identity as BC-A Identity
    participant Audit as BC-D Audit

    Worker->>Compliance: ExpireDocumentCommand
    Compliance->>Compliance: UserDocument VALID -> EXPIRED
    Compliance->>Identity: DocumentExpiredEvent (BLOCK_ACCESS)
    Compliance->>Audit: DocumentExpiredEvent

    Identity->>Identity: BlockUser (ACTIVE -> BLOCKED)
    Identity->>Audit: UserBlockedEvent
```

---

## Flujo 3: Promocion de Rol (FS-12)

```mermaid
sequenceDiagram
    participant Worker as Background Worker
    participant IGA as BC-H IGA
    participant Approvals as BC-F Approvals
    participant Auth as BC-B Authorization
    participant Audit as BC-D Audit

    Worker->>IGA: MarkCriteriaMetCommand
    IGA->>IGA: EVALUATING -> CRITERIA_MET
    IGA->>Approvals: SubmitApprovalRequest (ROLE_PROMOTION)
    IGA->>Audit: PromotionCriteriaMetEvent

    Approvals->>Approvals: ApproveRequest (Admin)
    Approvals->>IGA: ApprovalRequestApprovedEvent
    Approvals->>Auth: ApprovalRequestApprovedEvent

    IGA->>IGA: PENDING_APPROVAL -> PROMOTED
    IGA->>Audit: PromotionApprovedEvent

    Auth->>Auth: Actualiza Profile (nuevo RoleId)
    Auth->>Audit: PermissionMutatedEvent
```

---

## Tabla de Enrutamiento de Eventos

| Evento | Emisor | Receptor(es) | Accion del Receptor |
|--------|--------|-------------|---------------------|
| `UserRegisteredEvent` | Identity | IGA, Compliance, Approvals | Inicializar tracking |
| `UserActivatedEvent` | Identity | Authorization | Habilitar asignacion de Profiles |
| `UserBlockedEvent` | Identity | Authorization | Suspender Profiles activos |
| `DocumentExpiredEvent` | Compliance | Identity | Ejecutar `EnforcementAction` |
| `ApprovalRequestApprovedEvent` (ONBOARDING) | Approvals | Identity | Activar UserAccount |
| `ApprovalRequestApprovedEvent` (PROFILE_ASSIGNMENT) | Approvals | Authorization | Asignar Profile |
| `ApprovalRequestApprovedEvent` (ROLE_PROMOTION) | Approvals | IGA, Authorization | Completar promotion + actualizar Profile |
| `PromotionApprovedEvent` | IGA | Authorization | Actualizar RoleId en Profile del usuario |
| `PermissionMutatedEvent` | Authorization | Cache | Invalidar `auth_graph:{userId}:*` |
| `AppConfigUpdatedEvent` | Configuration | Cache | Invalidar `cfg:*` para el scope |
| Todos | Todos | Audit | Appendear `AuditRecord` inmutable |

---

**[Anterior: Compliance Context](./09-compliance-context.md)** | **[Indice DDD](./index.md)** | **[Siguiente: DDD Primitives](./11-ddd-primitives.md)**
