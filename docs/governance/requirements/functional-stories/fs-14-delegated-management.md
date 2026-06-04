# FS-14: Controlled UMS Management Delegation within the Tenant

## 1. Business Purpose

An authorized tenant user with active and effective UMS access must be able to delegate limited UMS management capabilities to another user within the same tenant. This delegation enables administrative work without introducing privilege escalation or cross-tenant administration.

The delegation applies only to the internal UMS scope and must remain bounded by the delegator's original permissions.

## 2. Functional Scope

- Delegation always occurs within the same tenant.
- The delegator must be an authorized tenant user with active and effective UMS access.
- The delegatee must belong to the same tenant and be eligible to receive UMS permissions.
- Delegation may include permissions to create users, manage users, assign allowed profiles, modify allowed permissions within the delegator's scope, and revoke or block users if the delegator has those capabilities.
- Delegation may require approval depending on the permission type, risk level, or tenant policy.
- Delegation must be approved, traceable, auditable, and revocable.

## 3. Out of Scope

- Cross-tenant delegation.
- Global administration outside the tenant.
- Delegation by `Organization`, `Department`, `System`, or `Team` if those scopes are not explicitly enabled for this story.
- Granting permissions the delegator does not possess.
- Using delegation as a shortcut around UMS approval controls.

## 4. Actors

| Actor | Responsibility |
|---|---|
| Delegator | Grants, modifies, or revokes a limited delegation within their own UMS scope. |
| Delegatee | Executes UMS management actions within the delegated permissions and tenant boundary. |
| Authorized Approver | Approves the delegation when tenant policy requires it. |
| Authorized Superior Administrator | Can revoke or suspend delegations for control or security reasons. |
| Auditor | Verifies who delegated what, when, for how long, and with what outcome. |

## 5. Business Preconditions

- The delegator and delegatee belong to the same tenant.
- The delegator has active and effective UMS access.
- The delegator already holds the permissions they intend to delegate.
- The delegatee is eligible to receive UMS permissions.
- No active restriction invalidates the delegation due to prior revocation, deactivation, or access loss.
- Tenant policy defines whether the delegation requires approval, time limits, or additional validation.

## 6. Main Functional Flow

1. The delegator opens UMS delegation management within the tenant.
2. The delegator selects the delegatee user from the same tenant.
3. The delegator defines the UMS permissions to delegate.
4. The system validates that every requested permission exists in the delegator's effective profile.
5. The system rejects any attempt at escalation, self-delegation, cycles, or cross-tenant delegation.
6. If required by policy, the delegation enters an approval flow.
7. If approved or activated, the delegatee receives only the delegated permissions.
8. The delegatee can execute the authorized capabilities within the tenant and delegated scope.
9. The delegator can modify the delegation by adding or removing permissions, provided the result never exceeds the delegator's own scope.
10. The delegator or an authorized superior administrator can revoke the delegation.

## 7. Alternative Flows and Exceptions

### A. Permission not held by delegator

If the delegator attempts to delegate a permission they do not hold, the system rejects the operation.

### B. Delegatee not eligible

If the delegatee does not belong to the same tenant, is blocked, is inactive, or fails the eligibility rule, the system rejects the delegation.

### C. Approval required

If tenant policy requires approval, the delegation remains pending until a final decision is recorded. Approval may accept, limit, or reject the request.

### D. Delegator loses authority

If the delegator loses permissions, becomes inactive, changes profile, or loses active UMS access, derived delegations must be reviewed, suspended, or revoked according to tenant policy.

### E. Cycle or self-delegation

If the operation creates self-delegation, a delegation cycle, or unsafe chaining, the system rejects it.

## 8. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Delegation applies only to the internal UMS scope within the tenant. |
| BR-02 | Cross-tenant delegation is not allowed. |
| BR-03 | The delegator may only delegate permissions already present in their effective UMS profile. |
| BR-04 | The delegatee must belong to the same tenant and be eligible to receive UMS permissions. |
| BR-05 | Delegation cannot grant more authority than the delegator possesses. |
| BR-06 | Every delegation must be traceable by tenant, delegator, delegatee, delegated permissions, validity, and state. |
| BR-07 | Delegation may require approval depending on policy or permission sensitivity. |
| BR-08 | Every create, modify, approve, activate, reject, revoke, suspend, or expire transition must be audited. |
| BR-09 | Delegation must be revocable by the delegator or an authorized superior administrator. |
| BR-10 | Cycles, self-delegation, and unsafe escalation must be prevented. |
| BR-11 | If the delegator loses access or permissions, derived delegations must be reviewed or suspended. |
| BR-12 | Delegation may also be time-bound. |

## 9. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | The system allows delegation of UMS management only within the same tenant. |
| 2 | The system blocks cross-tenant delegation. |
| 3 | The system prevents the delegatee from receiving permissions the delegator does not hold. |
| 4 | The delegatee receives only explicitly delegated UMS capabilities. |
| 5 | Delegation can require approval when tenant policy defines it. |
| 6 | The delegator can modify the delegation without exceeding their own scope. |
| 7 | Delegation can be revoked by the delegator or an authorized superior administrator. |
| 8 | Every delegation state transition is audited. |
| 9 | The system prevents self-delegation, cycles, and unsafe escalation. |
| 10 | If the delegator loses access or permissions, derived delegations are reviewed or suspended according to policy. |

## 10. Audit and Traceability Requirements

- Record tenant, delegator, delegatee, delegated permissions, scope, validity, reason, and state.
- Record who approved, activated, modified, revoked, rejected, or expired the delegation.
- Record the timestamp of every state transition.
- Preserve evidence of the delegator's original effective permissions at creation and update time.
- Maintain traceability for forced suspension or review when the delegator loses authority.

## 11. Technical Requirements

- If a technical structure already exists, such as `UserManagementDelegation`, it must align to this minimum scope: tenant, delegator user, delegatee user, delegated UMS permissions, approval, audit, and revocation.
- The model must validate that the delegated permission set is a subset of the delegator's effective permissions.
- The model must support approval, activation, rejection, revocation, expiration, and suspension states.
- Cycle prevention and self-delegation checks must be enforced at the application layer and reinforced in the domain model.
- Do not introduce additional scopes such as Organization, Department, System, or Team unless a future story explicitly enables them.

## 12. Traceability

| Type | References |
|---|---|
| Entities | `UserManagementDelegation`, `UserAccount`, `Profile` |
| Related UMS capabilities | User creation, user blocking/revocation, profile assignment, modification of allowed permissions |
| Domain events | Delegation create, modify, approve, activate, reject, revoke, and expire events |
| Related Stories | FS-10, FS-12, FS-21, FS-22, FS-24 |
| ADRs | ADR-0038, ADR-0044 |
