# EP-09 Onboarding — Continuation Prompt

## Contexto del proyecto

UMS es un sistema de gestión de identidad y autorización (Identity & Access Management) construido en .NET con Clean Architecture + DDD.

Repo: `/Users/beyondnet/Source/ums`  
Branch activo: `main`

---

## Qué se está construyendo

**EP-09: Onboarding Approval Inbox** — modelo de dos fases:

- **Fase 1 (identidad):** El usuario se registra públicamente → `UserAccount` queda en `Pending`. El Tenant Admin aprueba o deniega desde el inbox.
- **Fase 2 (entitlements):** El usuario activo sin perfil entra al **lobby**, declara sistema + branch + rol sugerido → crea un `ApprovalRequest`. El Tenant Admin o Branch Manager aprueba (con rol final que puede diferir), modifica, o deniega.

Un usuario puede tener **múltiples perfiles** (N `ApprovalRequest` → N `Profile`).

---

## Invariantes de negocio

### User Signup
- Solo se acepta si no existe ningún `UserAccount` con ese email en ese tenant (en cualquier estado).

### Profile Request
- No se puede crear si ya existe un `ApprovalRequest` con `Status = Pending` para el mismo `UserId + SystemId + BranchId`.
- No se puede crear si ya existe un `Profile` activo para el mismo `UserId + RoleId + BranchId`.
- El usuario debe tener `UserAccount.Status = Active`.

---

## Patrones del codebase

| Capa | Patrón |
|---|---|
| Domain | `AggregateRoot<T, TProps>` — props en clase separada |
| Domain Events | `DomainEventsManager` heredado |
| Persistence | Record class separada (`*Record`) + EF `IEntityTypeConfiguration` |
| Rehydration | `*AggregateFactory` con reflection (ver `ApprovalsAggregateFactory.cs`) |
| Repository | `IUnitOfWork` + `SaveEntitiesAsync` que publica outbox |
| CQRS | `ICommandHandler<TCmd, TResult>` / `IQueryHandler<TQuery, TResult>` |
| Resultados | `Result<T>` / `Result` — nunca lanzar excepciones de dominio |
| Decorators | `[AuditTrail]` + `[LoggerAspect]` en handlers |
| Migrations | EF Core — correr `dotnet ef migrations add <Name>` en `Ums.Infrastructure` |

---

## Archivos clave

### Domain
- `src/apps/ums.api/Ums.Domain/Approvals/ApprovalRequest/ApprovalRequest.cs`
- `src/apps/ums.api/Ums.Domain/Approvals/ApprovalRequest/ApprovalRequestProps.cs`

### Infrastructure
- `src/apps/ums.api/Ums.Infrastructure/Persistence/Approvals/Entities/ApprovalRequestRecord.cs`
- `src/apps/ums.api/Ums.Infrastructure/Persistence/Approvals/Configurations/ApprovalRequestRecordConfiguration.cs`
- `src/apps/ums.api/Ums.Infrastructure/Persistence/Approvals/SqlServerApprovalRequestRepository.cs`
- `src/apps/ums.api/Ums.Infrastructure/Persistence/Reflection/ApprovalsAggregateFactory.cs`

### Application
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/CreateApprovalRequestCommand.cs`
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/CreateApprovalRequestCommandHandler.cs`
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/ApproveRequestCommand.cs`
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/ApproveRequestCommandHandler.cs`
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/RejectRequestCommand.cs`
- `src/apps/ums.api/Ums.Application/Approvals/ApprovalRequest/Commands/RejectRequestCommandHandler.cs`
- `src/apps/ums.api/Ums.Application/Identity/UserAccount/Commands/ActivateUserAccountCommandHandler.cs`
- `src/apps/ums.api/Ums.Application/Identity/Auth/Commands/SignupUserCommandHandler.cs`

### Presentation
- `src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Auth/AuthEndpoints.cs`
- `src/apps/ums.api/Ums.Presentation/Endpoints/Approvals/ApprovalRequest/ApprovalRequestEndpoints.cs`

---

## Trabajo pendiente (en orden)

### 1. Extender `ApprovalRequest` (dominio + persistencia)
Agregar a `ApprovalRequestProps`:
- `SystemSuiteId RequestedSystemId`
- `BranchId? RequestedBranchId`
- `RoleId RequestedRoleId`
- `RoleId? GrantedRoleId`
- `string? Justification`
- `string? DecisionReason`

Actualizar:
- `ApprovalRequest.Create()` — nuevos parámetros
- `ApprovalRequest.Approve(actorId, grantedRoleId, decisionReason?)` — acepta rol final
- `ApprovalRequest.Reject(actorId, decisionReason?)` — acepta razón
- `ApprovalRequestRecord` — nuevas columnas
- `ApprovalRequestRecordConfiguration` — mapeo EF
- `ApprovalsAggregateFactory.RehydrateRequest` — rehydration
- `SqlServerApprovalRequestRepository` — ToRecord / Apply
- EF Migration

### 2. Invariantes en repositorio
Agregar a `IApprovalRequestRepository`:
- `Task<bool> ExistsPendingForScopeAsync(Guid userId, Guid systemId, Guid? branchId, CancellationToken ct)`

Agregar a `IProfileRepository` (o en handler):
- Check perfil activo mismo scope antes de crear request

### 3. Actualizar Commands
- `CreateApprovalRequestCommand` — agregar `SystemId`, `BranchId?`, `RoleId`, `Justification?`
- `CreateApprovalRequestCommandHandler` — validar invariantes antes de crear
- `ApproveRequestCommand` — agregar `GrantedRoleId`, `DecisionReason?`
- `RejectRequestCommand` — agregar `DecisionReason?`

### 4. DenyUserSignupCommand (nuevo)
Archivo: `src/apps/ums.api/Ums.Application/Identity/UserAccount/Commands/DenyUserSignupCommand.cs`
- Transiciona `UserAccount` a `Denied` (nuevo estado o reusar `Blocked` — decidir)
- Guarda `DenialReason`
- Envía notificación `UserSignupDenied`

### 5. Queries del inbox
- `GetPendingUserSignupRequestsQuery` — `UserAccount` con `Status=Pending` por tenant
- `GetPendingProfileRequestsQuery` — `ApprovalRequest` con `Status=Pending` por tenant

### 6. Endpoints
- `POST /api/v1/identity/users/{id}/deny-signup` — DenyUserSignupCommand
- `GET /api/v1/onboarding/inbox/user-signups` — pending user signups
- `GET /api/v1/onboarding/inbox/profile-requests` — pending profile requests
- `PUT /api/v1/approvals/requests/{id}/approve` — actualizar con GrantedRoleId
- `PUT /api/v1/approvals/requests/{id}/reject` — actualizar con DecisionReason

### 7. DTOs
- `ApprovalRequestDto` — incluir campos nuevos
- Nuevo `OnboardingInboxDto` — vista compuesta

---

## Notificaciones requeridas
Templates ya existentes en `NotificationTemplates.cs`:
- `UserSignupRequestReceived` ✅
- `UserSignupApproved` ✅
- `UserSignupDenied` ← por crear
- `ProfileRequestApproved` ← por crear
- `ProfileRequestDenied` ← por crear

---

## Cómo correr el proyecto
```bash
cd src/apps/ums.api/Ums.Presentation
dotnet run
```

## Cómo agregar migración EF
```bash
cd src/apps/ums.api
dotnet ef migrations add <NombreMigracion> --project Ums.Infrastructure --startup-project Ums.Presentation
```
