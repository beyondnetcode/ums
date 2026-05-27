# Functional Story 17: Maintain Roles for a System Suite

## 1. Business Purpose

UMS must let security administrators maintain the role catalog owned by each system suite, so profiles and permission templates reference understandable, governed responsibilities.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Security Administrator** | Creates and maintains roles and their hierarchy in a system suite. |
| **System Owner** | Confirms role meanings and promotion order. |

## 3. Business Preconditions

- A system suite has already been registered.
- The administrator is allowed to manage authorization catalogs.

## 4. Main Functional Flow

1. The administrator opens a system suite and selects its Roles view.
2. The administrator registers a role with its code, display value, and description.
3. The administrator optionally selects a parent role to represent responsibility hierarchy.
4. UMS displays the registered role and its active state in that suite.
5. The administrator can correct descriptive or hierarchy data and activate or deactivate the role.

## 5. Alternative Flows and Exceptions

### A. Duplicate Code

If the code already exists in the selected suite, UMS rejects the registration and identifies the duplicated code so the administrator can correct it.

### B. Invalid Hierarchy

If a selected parent belongs to another suite or creates a cycle, UMS rejects the change and explains which hierarchy rule must be corrected.

## 6. Business Rules

1. Every role belongs to one system suite and one tenant.
2. Every role must define `code`, `value`, and `description`.
3. A role code is unique within its system suite.
4. A root role has hierarchy level `0`; a child is exactly one level below its selected parent.
5. A hierarchy cannot contain cycles.
6. Deactivated roles remain traceable and cannot be treated as new active assignments.

## 7. Acceptance Criteria

1. The administrator can see Roles as a child view of a selected system suite.
2. The administrator can register, edit, activate, and deactivate a role.
3. Duplicate codes and invalid hierarchies show a human-readable correction reason.
4. Failed operations show a support error ID without exposing technical details.
5. No raw role or parent GUID is required in the UI flow.

## 8. Technical Requirements

- Domain aggregate: `Ums.Domain.Authorization.Role.Role`.
- Commands use REST endpoints nested under `/system-suites/{systemSuiteId}/roles`.
- Queries use GraphQL `rolesBySystemSuite(systemSuiteId)`.
- SQL Server persists `Roles` with FKs to `SystemSuites` and optional self-parent relation.
- Application-level tenant query filtering is mandatory; database controls remain secondary safeguards.
- Role commands use the Result Pattern and emit role lifecycle domain events.
- Validation/business failures return user-safe causes; unexpected technical details remain in Serilog/Loki logs correlated by `ErrorId`.
- The React view is localized in Spanish and English and uses runtime response validation.

## 9. Traceability

- Entities: `SystemSuite`, `Role`, `PermissionTemplate`, `Profile`
- Related stories: FS-02, FS-04, FS-05, FS-12
- Standards: catalog `code/value/description`, user-safe error response standard, tenant isolation rule
