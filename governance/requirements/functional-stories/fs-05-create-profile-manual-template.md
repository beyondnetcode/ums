# Functional Story 5: Create Profile and Manually Assign Authorization Template

## 1. Business Purpose

Administrators need to create profiles that represent real work responsibilities and manually attach approved authorization templates when automatic assignment is not appropriate.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Security Administrator** | Creates profiles and assigns templates. |
| **Tenant Operations Manager** | Manages local profiles within delegated scope. |

## 3. Business Preconditions

- The target organization exists.
- The target branch exists when branch-scoped access is required.
- At least one authorization template is available.

## 4. Main Functional Flow

1. The administrator opens profile management.
2. The administrator creates a profile with name, organization, and optional branch scope.
3. The system creates the profile as a manually managed profile.
4. The administrator selects one or more approved authorization templates.
5. The system links the selected templates to the profile.
6. Users assigned to the profile receive the effective permissions from those templates.

## 5. Alternative Flows and Exceptions

### A. Template Conflict

If the selected template conflicts with existing profile rules, the system warns the administrator and asks for confirmation.

### B. Template Replacement

If the profile already has an active template, the administrator must confirm whether the new template replaces or complements the existing one.

## 6. Business Rules

1. A profile must belong to an organization.
2. Branch scope is optional but must be explicit when used.
3. Manual template assignment must be traceable.
4. Explicit deny rules must take precedence over allowed permissions.

## 7. Acceptance Criteria

1. An administrator can create a profile in an allowed organization.
2. An approved template can be manually assigned to the profile.
3. Conflicting assignments require administrator confirmation.
4. Affected users receive updated effective permissions.

## 8. Technical Requirements

- Persist profiles in `PROFILE`.
- Link templates through `PROFILE_PERMISSION` / template assignment relationship.
- Invalidate authorization graph cache for affected users.
- Emit `ProfileCreatedEvent` and `TemplateAssignedEvent`.
- Preserve assignment metadata including whether assignment was manual.

## 9. Traceability

- Entities: `PROFILE`, `PERMISSION_TEMPLATE`, `PROFILE_PERMISSION`
- ADRs: ADR-0039, ADR-0042, ADR-0043, ADR-0035
- Technical Enabler: TE-01
