# FS-29: Entitlement Packages for Governed Access Bundles

> **Status:** Pending implementation

## 1. Business Purpose

Business owners need reusable access bundles that group roles, permissions, and application access into a single governed package. UMS must allow administrators to define, request, approve, and revoke those bundles without managing every entitlement one by one.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Entitlement Administrator | Designs and maintains access packages. |
| Requester | Requests a package instead of individual entitlements. |
| Approver | Decides whether the package can be granted. |
| Auditor | Verifies who received which bundle and why. |

## 3. Business Preconditions

- The system catalog and authorization catalog are already defined.
- The package owner knows which roles, permissions, or scopes belong together.
- Approval routing is configured for the target tenant or system.

## 4. Main Functional Flow

1. The Entitlement Administrator creates a package with a business name and scope.
2. The administrator adds the roles, permissions, and other access items that belong together.
3. The requester selects the package instead of selecting individual entitlements.
4. The approver reviews the request and decides whether the package can be granted.
5. If approved, the system assigns all included entitlements in one governed action.
6. If access must be removed later, the system revokes the whole package or the affected items according to policy.

## 5. Alternative Flows and Exceptions

### A. Package Not Allowed for Scope

If the package is not valid for the tenant, system, or branch, the request cannot proceed.

### B. Partial Inclusion Conflict

If one item in the package conflicts with existing access, the system warns the approver before the decision is finalized.

### C. Package Expired

If the package is time-bound and has expired, the system does not grant it and informs the requester.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | A package must represent a governed business bundle, not an arbitrary list. |
| BR-02 | A package may only include entitlements allowed by the target scope. |
| BR-03 | Approval must grant or deny the complete package decision, not an ambiguous partial state. |
| BR-04 | Each assignment must be auditable by user, package, scope, and outcome. |
| BR-05 | Expired packages must not remain active. |
| BR-06 | A package can be reused only when its content and version remain controlled. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An administrator can define a package that contains multiple governed entitlements. |
| 2 | A requester can ask for the package instead of selecting each entitlement separately. |
| 3 | An approver can approve or deny the package request. |
| 4 | Approved packages grant all included entitlements in one controlled action. |
| 5 | The package assignment and its outcome are auditable. |
| 6 | Expired or invalid packages cannot be activated. |

## 8. Technical Requirements

- Introduce a package model with versioned package definitions and item membership.
- Persist package scope, included entitlements, approval state, and effective assignment state.
- Reuse approval routing and audit recording so package decisions remain traceable.
- Support package expiration and revocation rules in the access enforcement layer.
- Keep the package composition aligned with the system and authorization catalogs.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `SystemSuite`, `Role`, `Profile`, `PermissionTemplate`, `ApprovalRequest` |
| Functional Stories | FS-02, FS-05, FS-24 |
| ADRs | ADR-0012, ADR-0015, ADR-0035 |
