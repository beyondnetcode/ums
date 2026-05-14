# Functional Story 10: B2B External Access Request and Approval Flow

## 1. Business Purpose

Internal users need a controlled way to requestá access for external business partners such as clients, suppliers, and partner organizations. UMS must ensure that external access is justified, approved, traceable, and limited to the correct business scope.

---

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Sponsor User** | Requests and justifies access for an external user. |
| **PAP Administrator** | Reviews, approves, or rejects the requestá. |
| **External User** | Receives onboarding after approval.
## 3. Business Preconditions

- The sponsor is an authenticated internal corporate user.
- The sponsor has permission to requestá external access.
- The requested access profile is available for external users.

---

## 4. Main Functional Flow

1. The sponsor opens the B2B access management area and starts a new external access requestá.
2. The sponsor identifies the external organization. If it does not exist, the sponsor provides the legal name, external reference code, and organization type.
3. The sponsor enters the external user's email address and selects an allowed external profile.
4. The sponsor provides a mandatory business justification.
5. The system creates a pending access requestá and notifies the responsible approvers.
6. A PAP Administrator reviews the requestá, justification, target organization, and requested profile.
7. If the requestá is appropriate, the PAP Administrator approves it.
8. The system provisions or links the external organization and prepares the external user for onboarding.
9. The external user receives a secure onboarding message to complete registration.

---

## 5. Alternative Flows and Exceptions

### A. Request Rejected

If the PAP Administrator rejects the requestá, the sponsor is notified with the rejection reason and no external access is granted.

### B. Requested Profile Is Not Allowed

If the sponsor requests a profile that is not allowed for external users, the system blocks the requestá and records the attempted privilege escalation.

### C. External Organization Already Exists

If the organization already exists, the system links the new user to the existing organization instead of creating a duplicate.

---

## 6. Business Rules

1. External access must always have an internal sponsor.
2. External access must have a business justification.
3. External users must not receive internal administrative profiles.
4. Approval or rejection must be traceable to the approving administrator.
5. External users must remain logically isolated within their organization boundary.

---

## 7. Acceptance Criteria

1. A sponsor can submit a complete external access requestá.
2. A PAP Administrator can approve or reject the requestá with a visible outcome.
3. Rejected requests do not provision users or organizations.
4. Duplicate external organizations are handled without creating conflicting records.
5. Privileged internal profiles cannot be assigned to external users.

---

## 8. Technical Requirements

- Persist the requestá as `EXTERNAL_ACCESS_REQUEST` / `APPROVAL_REQUEST` with pending, approved, and rejected states.
- Record immutable audit data including sponsor, approver, justification, status, and timestaamps.
- Enforce profile validation at the service/API boundary.
- Use application-layer tenant filtering as the primary isolation control and SQL Server 2022 RLS as infrastructure hardening.
- Emit provisioning and audit events after approval.
- Return an authorization failure for privilege escalation attempts.

---

## 9. Traceability

- Entities: `APPROVAL_REQUEST`, `APPROVAL_WORKFLOW`, `USER_ACCOUNT`, `PROFILE`, `TENANT`
- ADRs: ADR-0031, ADR-0032, ADR-0038, ADR-0044
- Related Stories: FS-03, FS-14
