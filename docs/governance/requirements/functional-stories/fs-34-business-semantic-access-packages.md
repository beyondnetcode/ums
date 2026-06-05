# FS-34: Business-Semantic Access Packages and Policy Composer

> **Status:** Pending implementation

## 1. Business Purpose

Business owners need access packages that are defined in business language, not only in technical entitlements. UMS must let them compose packages using concepts like tenant, branch, partner tier, system, and role, then publish governed packages with versioning and policy rules.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Entitlement Architect | Designs the package composition. |
| Business Owner | Validates that the package matches the business need. |
| Approver | Confirms whether the package can be published or assigned. |
| Tenant Admin | Reuses packages for repeated access patterns. |

## 3. Business Preconditions

- The system and authorization catalogs already exist.
- The package can be mapped to real business scopes such as tenant, branch, system, or partner type.
- Approval routing and expiry policy are configured.

## 4. Main Functional Flow

1. The Entitlement Architect creates a package using business terms and versioning.
2. The architect maps the package to roles, permissions, and scope rules.
3. The Business Owner reviews the package and confirms that it matches the process or partner model.
4. The package is published and becomes available for request or assignment.
5. When the package changes, a new version is created instead of silently rewriting history.

## 5. Alternative Flows and Exceptions

### A. Scope Mismatch

If the package does not match the target tenant, branch, or partner scope, the system blocks publication.

### B. Invalid Composition

If a package includes conflicting entitlements, the system warns the author before the package can be published.

### C. Expired Version

If an older version is no longer valid, the system does not allow it to be reused without explicit policy.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Packages must be named and modeled in business language. |
| BR-02 | Package versions must be immutable once published. |
| BR-03 | The package must reflect allowed business scope only. |
| BR-04 | A package can be requested, approved, assigned, and revoked as a governed unit. |
| BR-05 | Package composition changes must preserve traceability across versions. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | A package can be modeled with business scope and technical entitlements together. |
| 2 | A package can be versioned and published without losing history. |
| 3 | A business owner can confirm that the package matches the intended access pattern. |
| 4 | The system blocks invalid or conflicting package compositions. |
| 5 | The package can be reused for repeated business scenarios. |

## 8. Technical Requirements

- Add package metadata for business terms, scope, version, and lifecycle state.
- Keep package composition immutable after publication.
- Support package composition by roles, permissions, and access rules.
- Reuse approval and audit patterns so package publication and assignment remain traceable.
- Ensure package resolution can be evaluated by business scope without leaking internal implementation details.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `SystemSuite`, `Role`, `PermissionTemplate`, `Profile`, `ApprovalRequest` |
| Functional Stories | FS-02, FS-24, FS-29 |
| ADRs | ADR-0012, ADR-0015, ADR-0035 |
