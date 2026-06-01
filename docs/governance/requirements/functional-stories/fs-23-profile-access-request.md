# FS-23: Profile Access Request from Lobby User

## 1. Business Purpose

Users admitted to a tenant may still need the right business profile before they can use operational menus. UMS must let an authenticated user without an assigned profile request the access needed for their work, without granting permissions automatically, and must trace the request until an authorized approver closes it.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Lobby User | Authenticated user admitted to the tenant but without an active profile. |
| Tenant Admin | Receives profile requests and validates whether access is appropriate. |
| Branch Manager | May review requests for users working in a specific branch when delegated by the tenant. |

## 3. Business Preconditions

- The user account is active for the tenant.
- The user has no active profile for the requested system or scope.
- The tenant has at least one system, branch, and role available for request.

## 4. Main Functional Flow

1. The user signs in successfully and reaches the lobby screen.
2. The lobby explains that the user belongs to the tenant but has no assigned profile yet.
3. The user opens the profile request form.
4. The user selects the system, then the branch, then the suggested role.
5. The user may enter a business justification.
6. The system stores the request as pending assignment.
7. The request appears in the profile request inbox for the responsible approver.
8. The user can see that the request is pending until a final approved or denied decision is made.

## 5. Alternative Flows and Exceptions

### A. No Available Roles

If no role is available for the selected system and branch, the user cannot submit the request and is told to contact the tenant administrator.

### B. Existing Active Profile

If the user already has an active profile for the same system and branch, the system prevents a duplicate request and shows the existing access state.

### C. Request Already Pending

If the same user already has a pending request for the selected system and branch, the system shows the pending request instead of creating another one.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Tenant access and profile authorization are separate phases. |
| BR-02 | A lobby user may authenticate but must not see operational menus until a profile is assigned. |
| BR-03 | The request must capture system, branch, suggested role, and optional justification. |
| BR-04 | The request remains pending until an authorized approver decides. |
| BR-05 | Duplicate pending requests for the same user, system, and branch are not allowed. |
| BR-06 | Each profile request lifecycle must be traceable inside the tenant from submission through final closure. |
| BR-07 | The final business outcomes are Approved and Denied; approval may grant the requested role or a modified role. |
| BR-08 | The requester must be notified automatically when the profile request reaches a final outcome. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An active user without a profile lands in the lobby after login. |
| 2 | The lobby user can request a profile by selecting system, branch, and suggested role. |
| 3 | The branch and role options depend on the previous selections. |
| 4 | The user can include a business justification. |
| 5 | The request is stored as pending assignment. |
| 6 | The user does not receive operational menu access before approval. |
| 7 | The user can see the pending state while the request is waiting for a decision. |
| 8 | The user is notified when the request is approved or denied. |

## 8. Technical Requirements

- Introduce a profile access request model or reuse the existing approval request model with a profile assignment request type.
- Persist requested system, branch, requested role, justification, requester, tenant, and status.
- Persist lifecycle metadata for submission, current status, final outcome, approver, decision date, and decision reason.
- Return a controlled lobby response when the authorization graph cannot resolve an active profile for the authenticated user.
- Keep the lobby route outside operational menus and available only after successful authentication.
- Enforce application-layer tenant filtering as the primary isolation mechanism.
- Add or map notification templates for final profile request approval and denial.

## 9. Traceability

| Type | References |
|---|---|
| Related Stories | FS-22, FS-24, FS-05 |
| Domain Entities | `UserAccount`, `Profile`, `Role`, `SystemSuite`, `Branch`, `ApprovalRequest` |
| Notifications | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
