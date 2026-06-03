# FS-22: User Signup Request and Approval

> **Status:** Implemented

## 1. Business Purpose

Users who want to join an existing tenant need a controlled request path instead of direct account creation. UMS must let the applicant choose the tenant, submit the request, keep it pending, and force the tenant owner to close it with an explicit approval or denial from a dedicated inbox.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Applicant | Requests access to an existing tenant and submits the required identity data. |
| Tenant Admin | Reviews pending access requests for the tenant and approves or denies access when appropriate. |
| Applicant | Receives the final onboarding notification after the request is approved or denied. |

## 3. Business Preconditions

- The target tenant already exists in UMS.
- The applicant has a name, email address, and password that satisfy the signup rules.
- The Tenant Admin has access to the tenant onboarding inbox.

## 4. Main Functional Flow

1. The applicant opens the login experience and chooses the access request option.
2. The applicant selects the tenant to which access is being requested.
3. The applicant enters the name, email, and password.
4. The system creates the user account in pending status and shows a confirmation message.
5. The pending request appears in the tenant onboarding inbox.
6. A Tenant Admin reviews the request and closes it by approving or denying the request.
7. If approved, the system activates the user account and notifies the user that access has been approved.
8. If denied, the system closes the request without activating access and notifies the user that access has been denied.

## 5. Alternative Flows and Exceptions

### A. Invalid Tenant

If the applicant selects a tenant that does not exist or is not available for onboarding, the request is not submitted.

### B. Email Already In Use

If the email already exists for the same tenant, the system does not create a duplicate account.

### C. Request Not Decided Yet

If the Tenant Admin has not made a final decision, the request remains pending and the applicant cannot sign in.

### D. Request Denied

If the Tenant Admin denies the request, the request reaches a terminal denied state, the applicant cannot sign in, and the denial is communicated automatically.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | User signup requests belong only to the target tenant. |
| BR-02 | Tenant Admins can review only the requests of their own tenant. |
| BR-03 | A pending request does not grant access until approved. |
| BR-04 | Approval must activate the account and notify the applicant. |
| BR-05 | Pending requests must be visible in a dedicated inbox so they are not lost inside general account administration. |
| BR-06 | Each request lifecycle must be traceable inside the target tenant from submission through final closure. |
| BR-07 | The only terminal business outcomes are Approved and Denied. |
| BR-08 | A final decision is mandatory; the request cannot be silently removed or left without a closure record. |
| BR-09 | The final decision must trigger an automatic notification to the applicant with the access result. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An applicant can submit a complete user signup request for an existing tenant. |
| 2 | The request is stored as pending and appears in the tenant onboarding inbox. |
| 3 | Only the target tenant can see the request. |
| 4 | A Tenant Admin can approve or deny the request from the inbox. |
| 5 | Approval activates the user account and notifies the applicant. |
| 6 | Denial closes the request without activating access and notifies the applicant. |
| 7 | Pending requests do not allow login before approval. |
| 8 | The system stores the tenant, current status, decision outcome, decision date, approver, and reason when available. |

## 8. Technical Requirements

- Persist the request as a `UserAccount` in `Pending` status rather than creating a separate onboarding table.
- Keep the public request endpoint anonymous and use the public client without tenant headers.
- Use tenant scoping to ensure that inbox queries return only the pending accounts of the current tenant.
- Reuse the existing activation flow to approve the request and emit the approval notification.
- Add an explicit denial command or equivalent application operation that records terminal denial without activating the account.
- Persist lifecycle metadata for submission, decision, approver, final status, and optional denial reason in an auditable form.
- Keep the inbox as a UI composition over the user account list so the workflow remains aligned with the core account model.
- Add or map notification templates for approved and denied final outcomes.

## 9. Traceability

| Type | References |
|---|---|
| Functional Stories | FS-22 |
| Domain Entities | `UserAccount`, `Tenant` |
| Notifications | `UserSignupRequestReceived`, `UserSignupApproved`, `UserSignupDenied` |
| ADRs | ADR-0075 |

## 10. Acceptance Test Evidence

- [`UserAccountOnboardingCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Identity/UserAccount/UserAccountOnboardingCommandHandlerTests.cs) covers pending signup visibility, denial, tenant scoping, and final-state handling.
- [`UserAccountCommandHandlerTests.cs`](../../../../src/apps/ums.api/Ums.Application.Test/Identity/UserAccount/UserAccountCommandHandlerTests.cs) covers activation of a pending account and the approved state transition used by the onboarding flow.
- [`UserAccountEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/UserAccount/UserAccountEndpoints.cs) and [`OnboardingInboxEndpoints.cs`](../../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Onboarding/OnboardingInboxEndpoints.cs) expose the inbox and terminal action paths used by the story.
