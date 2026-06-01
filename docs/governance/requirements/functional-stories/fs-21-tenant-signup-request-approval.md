# FS-21: Tenant Signup Request and Approval

## 1. Business Purpose

New companies need a controlled way to request access to UMS and start their onboarding without creating a tenant manually in the back office. UMS must capture the request, keep it pending until a global administrator reviews it, and create the tenant only after approval.

## 2. Actors

| Actor | Responsibility |
|---|---|
| System Admin | Reviews tenant signup requests and decides whether a new company is admitted to UMS. |
| Company Contact | Submits the company details and receives the onboarding outcome. |
| Tenant Admin User | Receives the first administrative account created for the approved company. |

## 3. Business Preconditions

- The company is not yet registered as an active tenant in UMS.
- The applicant has a valid company name, reference code, contact name, and contact email.
- The System Admin has access to the onboarding review area.

## 4. Main Functional Flow

1. The company representative opens the registration option from the login experience.
2. The representative enters the company name, reference code, contact name, and contact email.
3. The system records the request as pending and shows a confirmation message.
4. The request appears in the global onboarding inbox for System Admins.
5. A System Admin reviews the request and decides whether the company should be admitted.
6. If the request is approved, the system creates the tenant and the first administrative user for that tenant.
7. The system generates a temporary password for the first administrative user and notifies the company contact.
8. The onboarding request remains traceable with its final outcome.

## 5. Alternative Flows and Exceptions

### A. Duplicate Company Reference

If the company reference already belongs to an existing tenant, the system does not create a duplicate tenant and the request is not approved.

### B. Request Rejected

If the System Admin rejects the request, the company does not receive a tenant and the request is marked as rejected.

### C. Future Payment Verification

If the business later requires payment verification before admission, the onboarding flow must support additional review states without changing the company-facing request entry point.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Tenant signup requests belong to global scope and are reviewed only by System Admins. |
| BR-02 | A tenant is created only after a request is approved. |
| BR-03 | The approved tenant must receive its first administrative user as part of the same approval outcome. |
| BR-04 | The onboarding request must remain auditable from submission to final decision. |
| BR-05 | Future review states may be added for payment verification, but the public request flow must remain stable. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | A company can submit a tenant signup request with the required company and contact data. |
| 2 | The request appears as pending in the global onboarding inbox. |
| 3 | Only System Admins can review and approve or reject the request. |
| 4 | Approval creates the tenant and the first tenant admin account. |
| 5 | The company contact receives the onboarding outcome and temporary credentials after approval. |
| 6 | Rejected requests do not create tenants or admin users. |

## 8. Technical Requirements

- Persist tenant onboarding requests in the `TenantSignupRequest` aggregate with `Pending`, `Approved`, and `Rejected` states.
- Keep the public request entry point anonymous and tenant-agnostic.
- Use a composite approval inbox read model in the UI rather than a separate generic inbox table.
- Create the tenant and the first administrative user as part of the approval command.
- Send the approval notification with the generated temporary password and account details.
- Preserve room for future payment verification states in the onboarding state model and documentation.

## 9. Traceability

| Type | References |
|---|---|
| Functional Stories | FS-22 |
| Domain Entities | `TenantSignupRequest`, `Tenant`, `UserAccount` |
| Notifications | `TenantSignupRequestReceived`, `TenantSignupApproved` |
| ADRs | ADR-0075 |

