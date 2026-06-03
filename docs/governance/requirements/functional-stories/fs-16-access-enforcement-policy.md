# Functional Story 16: Define Access Policy on Expiration

> **Status:** Implemented

## 1. Business Purpose

Security and compliance teams need to define what should happen when a critical user document expires. UMS must apply predictable access consequences while keeping the reason visible and auditable.

---

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Security Architect** | Defines access impact for expired critical documents. |
| **Global Administrator** | Publishes or updates enforcement policies. |
| **Affected User** | Receives access restrictions or warnings based on policy. |
## 3. Business Preconditions

- Document compliance validation is enabled.
- The document type is marked as relevant for access control.
- The actor has permission to manage enforcement policies.

---

## 4. Main Functional Flow

1. The actor selects a document type that can affect access.
2. The actor chooses the business consequence that should apply after expiration.
3. The actor defines whether a grace period applies.
4. The actor provides a clear description of the policy purpose and impact.
5. The system saves and activates the policy.
6. When a user's document expires, the system applies the configured consequence.
7. The user and administrators can see why the restriction or warning was applied.

---

## 5. Alternative Flows and Exceptions

### A. Re-activation After Renewal

When the user provides a renewed and approved document, the system removes the restriction according to the configured policy.

### B. Non-critical Document

If the selected document type is not access-critical, the system prevents publication of a blocking policy and suggests a notification-only rule.

---

## 6. Business Rules

1. Critical document expiration may block access, restrict profiles, or only generate an audit warning.
2. Policies must include `code`, `value`, and `description`.
3. The user must be able to understand why access was restricted.
4. Renewal must allow access restoration when the policy conditions are satisfied.

---

## 7. Acceptance Criteria

1. An administrator can configure an access consequence for an access-critical document type.
2. The system prevents blocking policies for non-critical documents.
3. Access restrictions are traceable and visible to administrators.
4. Renewing a valid document can restore access according to policy.

---

## 8. Technical Requirements

> [!NOTE]
> En la implementación real de C# (base de código), `AccessEnforcementPolicy` es una Entidad hija encapsulada dentro del Agregado **DocumentType**, bajo el espacio de nombres unificado **Ums.Domain.Approvals**.

- Persist policies as part of the `DocumentType` Aggregate Root.
- Mandatory fields: `Code`, `Value` (JSON containing policy timing/action), `Description`.
- Enforce uniqueness by `Code`, tenant scope, and `DocumentTypeId`.
- Supported actions include `BLOCK_USER`, `RESTRICT_PROFILE`, and `LOG_ONLY`.
- Allow policy action updates through `PUT /access-enforcement-policies/{policyId}/action`.
- Record enforcement execution through the user document workflow so the applied outcome remains traceable.
- Emit domain and audit events when restrictions are applied or reverted.

---

## 9. Traceability

- Entities: `DocumentType` (AR), `AccessEnforcementPolicy` (Child Entity), `UserAccount` (AR), `Profile` (AR)
- ADRs: ADR-0045, ADR-0035
- Related Stories: FS-11, FS-15

## 10. Acceptance Test Evidence

- [`AccessEnforcementPolicyE2ETests.cs`](../../../../src/apps/ums.api/Ums.Presentation.IntegrationTest/E2E/AccessEnforcementPolicyE2ETests.cs) covers policy creation, GET by ID, action updates, deactivation, and validation when neither `ProfileId` nor `RoleId` is supplied.
- [`UpdateAccessEnforcementActionCommandValidatorTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Approvals/AccessEnforcementPolicy/Commands/UpdateAccessEnforcementActionCommandValidatorTests.cs) verifies that the command accepts the domain action names `BlockUser`, `RestrictProfile`, and `LogOnly`.
- [`AccessEnforcementPolicyCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Approvals/AccessEnforcementPolicy/AccessEnforcementPolicyCommandHandlerTests.cs) covers the create, deactivate, and update command handlers.
- [`UserDocumentEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Approvals/UserDocument/UserDocumentEndpoints.cs) exposes the enforcement execution route used to preserve traceability when the policy is applied.
