# FS-24: Profile Request Approval and Manual Assignment

## 1. Business Purpose

Tenant administrators and delegated branch managers need to review profile requests before users receive operational access. UMS must support approval, modification, and denial so the assigned profile matches the real business need, the request lifecycle is closed, and the decision remains auditable.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Tenant Admin | Reviews profile requests across the tenant and assigns the final profile. |
| Branch Manager | Reviews profile requests for delegated branch scope. |
| Lobby User | Receives the decision and gains access only after a profile is assigned. |
| Auditor | Reviews who approved access, when, and what role was granted. |

## 3. Business Preconditions

- A profile request exists in pending assignment status.
- The approver has authority over the tenant or branch scope.
- The requested system, branch, and role are still active.

## 4. Main Functional Flow

1. The approver opens the profile request inbox.
2. The approver reviews the user, system, branch, suggested role, and justification.
3. The system warns the approver about visible conflicts or role-risk conditions.
4. The approver chooses one of three outcomes: approve as requested, approve with a different role, or deny.
5. If approved, the system assigns the final profile to the user.
6. The system records who approved the assignment, when it happened, and which role was granted.
7. If denied, the system closes the request without assigning a profile.
8. The user is notified that the profile decision is complete.

## 5. Alternative Flows and Exceptions

### A. Approval with Modified Role

The approver may approve the request with a role different from the requested one when a lower or more appropriate access level is required.

### B. Denial

The approver may deny the request. The user remains without operational access for that system and branch.

### C. Conflict Warning

If the selected role conflicts with existing roles or creates a separation-of-duties risk, the system warns the approver before the decision is completed.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Approval must be limited to the approver's tenant or delegated branch scope. |
| BR-02 | The approver may grant the requested role or a different role within their authority. |
| BR-03 | Denial must keep the user without operational access for the requested scope. |
| BR-04 | The final role, approver, decision date, and decision reason must be auditable. |
| BR-05 | Role conflict and separation-of-duties warnings must be shown before final approval when detectable. |
| BR-06 | Every profile request must reach a terminal Approved or Denied status. |
| BR-07 | A final decision must trigger an automatic notification to the requester. |
| BR-08 | Requests cannot be deleted or hidden as a substitute for a final decision. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An authorized approver can see pending profile requests in their scope. |
| 2 | The approver can approve the requested role. |
| 3 | The approver can approve the request with a different role. |
| 4 | The approver can deny the request. |
| 5 | The system records approver, date, granted role, and decision outcome. |
| 6 | The user is notified after approval or denial. |
| 7 | The system warns about detectable role conflicts before approval. |
| 8 | A decided request is closed and no longer appears as pending. |

## 8. Technical Requirements

- Reuse or extend the approval request model for `PROFILE_ASSIGNMENT` decisions.
- Store requested role and granted role separately.
- Record decision metadata in immutable audit records.
- Invoke profile assignment only after approval.
- Invalidate the authorization graph for the user after profile assignment.
- Prepare the approval routing model for branch-scoped delegation.
- Expose terminal statuses as Approved and Denied in user-facing and audit views, even if internal events reuse existing rejected terminology.
- Emit notification events/templates for both approved and denied final outcomes.

## 9. Traceability

| Type | References |
|---|---|
| Related Stories | FS-23, FS-05, FS-07, FS-14 |
| Domain Entities | `ApprovalRequest`, `Profile`, `Role`, `UserAccount`, `Branch` |
| Domain Events | `ProfileAssignedToUserEvent`, `ApprovalRequestApprovedEvent`, `ApprovalRequestDeniedEvent` |
| Notifications | `ProfileRequestApproved`, `ProfileRequestDenied` |
| ADRs | ADR-0075, ADR-0071 |
