# Flujos de Eventos Cross-Contexto

**Tipo:** DDD — Inter-Context Event Flows  
**Version:** 2.1 | **Fecha:** 2026-06-01

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

## Flujo 1A: Alta Publica de Tenant (FS-21)

```mermaid
sequenceDiagram
    participant Visitante as Contacto Empresa
    participant Identity as BC-A Identity
    participant Tenant as TenantSignupRequest
    participant Admin as System Admin
    participant Notify as Notification Service
    participant Audit as BC-D Audit

    Visitante->>Identity: RequestTenantSignup
    Identity->>Tenant: Create(Pending)
    Identity->>Notify: TenantSignupRequestReceived
    Identity->>Audit: TenantSignupRequestCreated
    Admin->>Identity: ApproveTenantSignup
    Identity->>Tenant: Approve(ApprovedTenantId)
    Identity->>Notify: TenantSignupApproved
    Identity->>Audit: TenantSignupApproved
```

---

## Flujo 1B: Alta Publica de Usuario en Tenant Existente (FS-22)

```mermaid
sequenceDiagram
    participant Solicitante as Solicitante
    participant Identity as BC-A Identity
    participant User as UserAccount
    participant Admin as Tenant Admin
    participant Notify as Notification Service
    participant Auth as BC-B Authorization
    participant Audit as BC-D Audit

    Solicitante->>Identity: SignupUser(tenant, email, password)
    Identity->>User: Create(Status = Pending)
    Identity->>Notify: UserSignupRequestReceived
    Identity->>Audit: UserRegisteredEvent
    Admin->>Identity: Approve or deny signup
    Identity->>User: Activate when approved
    Identity->>Notify: UserSignupApproved or UserSignupDenied
    Identity->>Audit: UserSignupDecisionRecorded
    Identity->>Auth: UserActivatedEvent when approved
```

> La denegacion de alta de usuario es resultado requerido por EP-09; el codigo actual implementa creacion pendiente y activacion, pero aun requiere comando dedicado de denegacion y metadata de decision.

---

## Flujo 1C: Solicitud y Decision de Perfil desde Lobby (FS-23, FS-24)

```mermaid
sequenceDiagram
    participant Usuario as Usuario en Lobby
    participant Approvals as BC-F Approvals
    participant Admin as Tenant o Branch Approver
    participant Auth as BC-B Authorization
    participant Notify as Notification Service
    participant Audit as BC-D Audit

    Usuario->>Approvals: Submit profile request
    Approvals->>Approvals: Create ApprovalRequest(Pending)
    Approvals->>Audit: ApprovalRequestSubmittedEvent
    Admin->>Approvals: Approve, modify, or deny
    Approvals->>Auth: ApprovalRequestApprovedEvent when approved
    Auth->>Auth: Assign final Profile
    Approvals->>Notify: ProfileRequestApproved or ProfileRequestDenied
    Approvals->>Audit: ApprovalRequestFinalDecision
```

> El estado implementado `ApprovalStatus.Rejected` se traduce como resultado de negocio `Denied` en onboarding de perfiles.

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
| `ApprovalRequestRejectedEvent` (PROFILE_ASSIGNMENT) | Approvals | Notification, Audit | Notificar denegacion de solicitud de perfil |
| `TenantSignupApproved` | Identity | Notification, Audit | Notificar aprobacion y registrar cierre global |
| `UserSignupApproved` | Identity | Notification, Authorization, Audit | Notificar al solicitante y habilitar lobby si no tiene Profile |
| `UserSignupDenied` | Identity | Notification, Audit | Notificar denegacion y cerrar solicitud |
| `ApprovalRequestApprovedEvent` (ROLE_PROMOTION) | Approvals | IGA, Authorization | Completar promotion + actualizar Profile |
| `PromotionApprovedEvent` | IGA | Authorization | Actualizar RoleId en Profile del usuario |
| `PermissionMutatedEvent` | Authorization | Cache | Invalidar `auth_graph:{userId}:*` |
| `AppConfigUpdatedEvent` | Configuration | Cache | Invalidar `cfg:*` para el scope |
| Todos | Todos | Audit | Appendear `AuditRecord` inmutable |

---

**[Anterior: Compliance Context](./09-compliance-context.md)** | **[Indice DDD](./index.md)** | **[Siguiente: DDD Primitives](./11-ddd-primitives.md)**
