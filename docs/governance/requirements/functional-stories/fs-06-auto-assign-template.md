# Functional Story 6: Auto-Assign Authorization Template on Profile Creation

## 1. Business Purpose

UMS should reduce manual administration by assigning the right authorization template when a new profile matches approved business rules.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Security Administrator** | Configures assignment rules. |
| **UMS Rule Engine** | Applies matching rules during profile creation. | ## 3. Business Preconditions

- At least one assignment rule is active.
- The created profile contains attributes that can be evaluated.
- A matching template is available and active.

## 4. Main Functional Flow

1. The administrator defines a rule that maps profile attributes to an authorization template.
2. A new profile is created.
3. UMS evaluates the profile against active assignment rules.
4. If a rule matches, UMS assigns the corresponding template automatically.
5. The profile is marked as automatically assigned.
6. Affected users receive the resulting permissions.

## 5. Alternative Flows and Exceptions

### A. No Rule Matches

If no active rule matches the profile, the profile remains without automatic template assignment and can be handled manually.

### B. Multiple Rules Match

If more than one rule matches, UMS applies the highest-priority rule and records why it was selected.

## 6. Business Rules

1. Automatic assignment must be explainable.
2. Rule priority must be deterministic.
3. Manual assignment remains available when automation does not match.
4. Automatic assignments must be auditable.

## 7. Acceptance Criteria

1. A matching rule assigns a template automatically.
2. A profile with no matching rule remains available for manual assignment.
3. Rule priority resolves multiple matches consistently.
4. The assignment reason is visible to administrators.

## 8. Technical Requirements

> [!NOTE]
> **IMPLEMENTATION STATUS: Green — Done (2026-06-03)**  
> `TemplateAssignmentRule` aggregate is implemented. `CreateProfileCommandHandler` auto-assigns the highest-priority matching template on profile creation. REST endpoints for rule management are exposed at `/template-assignment-rules`.

- Persist assignment state on the profile/template relationship. Done (`ProfilePermission` entries created via `Profile.AssignTemplate`)
- Invalidate authorization graph cache for affected users. (cache invalidation handled by existing `TemplateLinkedToProfileEvent` observer)
- Emit domain and audit events for template assignments. Done (`TemplateAutoAssignedEvent` emitted)

## 9. Traceability

- Entities: `Profile` (AR), `PermissionTemplate` (AR), `TemplateAssignmentRule` (Deferred)
- ADRs: ADR-0042, ADR-0043, ADR-0035
- Technical Enabler: TE-01
