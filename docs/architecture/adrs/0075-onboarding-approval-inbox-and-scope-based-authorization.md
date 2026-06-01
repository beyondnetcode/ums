# ADR-0075: Onboarding Approval Inbox and Scope-Based Authorization

**Status:** Accepted  
**Date:** 2026-06-01  
**Context:** UMS Identity, Tenant Onboarding, User Onboarding

## Context

UMS now supports two different identity onboarding flows and must now separate them from entitlement assignment:

| Flow | Source of truth | Approval scope |
|---|---|---|
| Tenant onboarding | `TenantSignupRequest` aggregate | Global, System Admin only |
| User onboarding | `UserAccount` in `Pending` status | Tenant scoped, Tenant Admin only |

The product needs a single operator-friendly place to review pending onboarding items, but the underlying business objects are not the same. Tenant signup requests represent a dedicated aggregate, while user signup requests are already modeled by the pending user account lifecycle.

The architecture must also prevent a common shortcut: approving a user into a tenant must not automatically grant operational entitlements. A user can be active in the tenant and still remain in a lobby state until a profile is requested and approved.

The product now requires the complete lifecycle of user signup and profile requests to be traceable by tenant. Administrators must close each request with a terminal business decision, either Approved or Denied, and the requester must receive an automatic notification when that final outcome is reached.

## Decision

UMS will expose an **Onboarding Approval Inbox** as a composite UI and query surface, not as a new generic inbox aggregate or table.

| Decision Area | Choice |
|---|---|
| UI placement | Add a new Identity-level menu entry for onboarding approvals. |
| Tenant onboarding data source | Query pending `TenantSignupRequest` records. |
| User onboarding data source | Query pending `UserAccount` records within the current tenant scope. |
| Entitlement onboarding | Route profile requests through the approval model before assigning a `Profile`. |
| Lobby state | Authenticated users without an active profile receive a controlled lobby experience instead of operational menus. |
| Approval authorization | Require explicit approval capability by scope, not just screen visibility. |
| Lifecycle closure | Require terminal Approved or Denied outcomes for user signup and profile requests. |
| Persistence model | Reuse existing aggregates; do not create a generic inbox table. |
| Future payment readiness | Keep the tenant onboarding state model extensible for payment verification states. |

## Why This Decision

1. It keeps the business model honest. Tenant signup and user signup are related, but they are not the same object.
2. It avoids unnecessary duplication. A generic inbox table would duplicate state already owned by the aggregates.
3. It preserves tenant isolation. User onboarding approvals must never leak across tenants.
4. It improves usability. Approvers need one discoverable place to work, even if the source records differ.
5. It prevents tenant admission from becoming implicit authorization.
6. It supports future evolution. Additional review states, such as payment verification, can be added without changing the operator entry point.

## Authorization Model

Approval access must be driven by explicit capability, not only by menu visibility.

| Capability | Scope | Allowed action |
|---|---|---|
| `ApproveTenantSignup` | Global | View and approve or reject tenant signup requests. |
| `ApproveUserSignup` | Current tenant | View and approve or deny user signup requests for the active tenant only. |
| `RequestProfileAccess` | Current tenant | Submit a profile request from the lobby. |
| `ApproveProfileRequest` | Tenant or delegated branch | Approve, modify, or deny profile requests. |
| `ViewOnboardingInbox` | Global or tenant scoped | Open the inbox and inspect pending items. |

The first release may enforce this through role and tenant-context checks, but the architectural expectation is that the action remains authorization-driven and auditable.

## Data and State Model

### Tenant onboarding

| State | Meaning |
|---|---|
| Pending | Request submitted and waiting for a global decision. |
| Approved | Tenant created and first admin account provisioned. |
| Rejected | Request closed without creating the tenant. |

### User onboarding

| State | Meaning |
|---|---|
| Pending | User account created, but not yet activated. |
| ActiveWithoutProfile | Tenant admin approved the request, the user can authenticate, but no operational profile is assigned. |
| Active | User has at least one active profile and can receive operational menus according to the authorization graph. |
| Denied | Request closed without activating tenant access. |

### Profile request onboarding

| State | Meaning |
|---|---|
| PendingAssignment | The user requested a system, branch, and suggested role. |
| Approved | Request closed with an assigned role. The granted role may match the requested role or be modified by the approver. |
| Denied | Request closed without assigning a profile for the requested scope. |

Terminal status labels are business-facing terms. Existing internal events or persistence values that use rejected terminology must be translated consistently at application boundaries so users, administrators, and auditors see Approved or Denied as the final outcomes.

## Alternatives Considered

| Alternative | Decision | Reason |
|---|---|---|
| Create a generic onboarding inbox table | Rejected | Duplicates aggregate state and introduces coupling between unrelated flows. |
| Put tenant onboarding inside the user list | Rejected | Hides a global business process inside a tenant-scoped screen. |
| Rely only on role-based screen hiding | Rejected | UI hiding is not an authorization model and is too weak for approvals. |
| Create separate inboxes with separate navigation entries | Rejected | Splits the operator experience and makes approvals harder to discover. |
| Activate user and assign default profile immediately | Rejected | Collapses identity admission and entitlement approval, increasing access risk. |

## Consequences

### Positive

- The operator experience is simpler.
- The domain model remains clean and explicit.
- Tenant isolation stays enforceable at the query boundary.
- Users can be admitted to a tenant without receiving operational entitlements.
- Future verification states can be introduced without redesigning the inbox.

### Trade-offs

- The inbox is a composed read model, so the UI must stitch together two sources of truth.
- Approval permissions must be documented and enforced consistently across endpoints and screens.
- The login flow must handle the no-profile case as a first-class lobby state.
- User signup and profile request denial paths require explicit commands, audit metadata, and notification templates.

## Implementation Notes

| Area | Guidance |
|---|---|
| Navigation | Add the inbox under the Identity module, not under Authorization. |
| Queries | Compose the inbox from pending tenant signup requests, pending user accounts, and pending profile assignment requests. |
| Commands | Keep tenant signup approval, user account activation, user signup denial, profile assignment approval, and profile request denial as separate command paths. |
| Lobby | Route authenticated users without active profile to a lobby screen with a profile request form. |
| Notifications | Continue using simulated notifications in the UI and notification templates in the backend; every terminal approval or denial must notify the requester. |
| Future policy | If payment verification is introduced, model it as an additional onboarding review state or policy, not as a separate user-facing flow. |
