# Functional Story 25: Manage Domain Resources with DDD Hierarchy

## 1. Business Purpose

Authorization administrators need to model the application's domain structure inside a System Suite so that permission templates can grant access at the right level of granularity — from whole aggregates down to individual domain operations (methods). Without this, every permission must be coarse-grained at the resource level, making it impossible to control access to specific business actions like `ResetPassword()` or `ApproveOrder()`.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **System Administrator** | Configures domain resources within a System Suite. |
| **Security Administrator** | Reviews the DDD structure before assigning permissions in templates. |

## 3. Business Preconditions

- The actor has permission to manage System Suite configuration.
- At least one System Suite exists for the target tenant.
- For DomainMethod resources, at least one Aggregate or Entity must already exist in the suite.

## 4. Main Functional Flow

1. The actor opens a System Suite and navigates to Domain Resources.
2. The actor selects the resource type: Aggregate, Entity, or DomainMethod.
3. For DomainMethod (or optionally Entity), the actor selects the parent resource.
4. The actor enters a code, name, and optional description.
5. The system validates the hierarchy rules and saves the resource.
6. The resource appears in the domain resource list and is available for use in permission templates.

## 5. Alternative Flows and Exceptions

### A. DomainMethod Without Parent

If the actor attempts to create a DomainMethod without selecting a parent, the system rejects the request with a validation error.

### B. Parent Is a DomainMethod

If the actor selects a DomainMethod as the parent for another resource, the system rejects the request (DomainMethods cannot have children).

### C. Remove Resource With Active Template References

If the resource is referenced in one or more active permission templates, the system returns HTTP 409 and shows the blocking dependencies.

## 6. Business Rules

1. `DomainMethod` resources must have a non-null parent (`Aggregate` or `Entity`).
2. A `DomainMethod` cannot be the parent of another resource.
3. Codes must be unique within the System Suite.
4. The parent resource must belong to the same System Suite.

## 7. Acceptance Criteria

1. Administrators can create Aggregate, Entity, and DomainMethod resources.
2. The DomainMethod form shows a parent selector that is required.
3. Creating a DomainMethod without a parent produces a validation error.
4. DomainMethod resources appear with their own icon and color in the resource list.
5. Filtering by type includes a "Methods" (DomainMethod) filter option.
6. Resources referencing a parent show the relationship in the auth graph.

## 8. Technical Requirements

- Type values: `Aggregate(1)`, `Entity(2)`, `DomainMethod(3)`.
- `ParentResourceId` stored as nullable `Guid` in `DOMAIN_RESOURCE`.
- Hierarchy invariants enforced by `SystemSuite.AddDomainResource(moduleId, parentResourceId, type, ...)`.
- Frontend: `SystemSuiteDomainResourcesPanel` must include DomainMethod option and conditional parent selector.

## 9. Traceability

- Entities: `DOMAIN_RESOURCE`, `SYSTEM_SUITE`
- ADRs: [ADR-0078](../../../architecture/adrs/0078-ddd-domain-resource-hierarchy.md), [ADR-0079](../../../architecture/adrs/0079-dependency-guard-policy.md)
