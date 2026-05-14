# Functional Story 2: Create and Instantiate Authorization Template

## 1. Business Purpose

Administrators need reusable authorization templates so common access patterns can be governed consistently across profiles and systems. UMS must let administrators create, version, and assign templates without redefining permissions manually for every profile.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Global IT Administrator** | Creates and maintains reusable authorization templates. |
| **Tenant/System Administrator** | Assigns approved templates to profiles when allowed. |

## 3. Business Preconditions

- The target system topology is registered.
- The available actions are defined.
- The administrator has permission to manage templates.

## 4. Main Functional Flow

1. The administrator opens the template manager.
2. The administrator creates a new template with a name, version, and purpose.
3. The administrator selects the systems, menus, options, and actions that the template will allow or deny.
4. The system validates that selected permissions belong to valid registered resources.
5. The administrator publishes the template.
6. The administrator assigns the template to an existing or new profile.
7. Users linked to that profile receive the effective authorization behavior from the assigned template.

## 5. Alternative Flows and Exceptions

### A. Invalid Permission Selection

If the administrator selects an invalid or unavailable resource/action, the system prevents publication and explains what must be corrected.

### B. Incompatible Template Update

If a new template version conflicts with local profile overrides, the system warns the administrator and requires explicit confirmation before applying the change.

## 6. Business Rules

1. Templates must be versioned.
2. Templates must only reference valid registered resources.
3. Template assignment must be auditable.
4. Changes that affect existing users must be traceable.

## 7. Acceptance Criteria

1. An administrator can create and publish a valid authorization template.
2. Invalid resources cannot be included in a template.
3. A template can be assigned to a profile.
4. Template changes are auditable and traceable.

## 8. Technical Requirements

- Persist templates and permissions using `PERMISSION_TEMPLATE`, `PROFILE`, and `PROFILE_PERMISSION`.
- Validate resource hierarchy integrity before publication.
- Invalidate compiled authorization graph cache for affected users after assignment or update.
- Emit audit events for template creation, publication, assignment, and version changes.
- Preserve semantic version lineage.

## 9. Traceability

- Entities: `PERMISSION_TEMPLATE`, `PROFILE`, `PROFILE_PERMISSION`, `ACTION`
- ADRs: ADR-0039, ADR-0042, ADR-0021
- Technical Enabler: TE-01
