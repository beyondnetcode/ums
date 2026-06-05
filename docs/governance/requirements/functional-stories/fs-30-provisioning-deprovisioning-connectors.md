# FS-30: Provisioning and Deprovisioning Connectors for Downstream Systems

> **Status:** Pending implementation

## 1. Business Purpose

When access changes in UMS, downstream systems must receive the corresponding create, update, or remove instruction automatically. UMS must provide governed provisioning connectors so identity operations remain synchronized across external applications.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Identity Operations Administrator | Configures the connector and its mapping rules. |
| External System Owner | Confirms the downstream target and access model. |
| Integration Connector | Applies the provisioning or deprovisioning action. |
| Auditor | Reviews the provisioning history and failures. |

## 3. Business Preconditions

- A downstream system or application has been registered as a target.
- Mapping rules exist for the data that must be synchronized.
- The tenant has an approved workflow for provisioning and deprovisioning.

## 4. Main Functional Flow

1. The Identity Operations Administrator registers a downstream target and defines the mapping.
2. When a user, profile, or entitlement changes in UMS, the system generates a provisioning action.
3. The connector delivers the change to the downstream system.
4. If the change cannot be delivered immediately, the system keeps the action traceable until it succeeds or is resolved.
5. When access is removed, the system sends the deprovisioning action so the downstream system does not keep stale access.

## 5. Alternative Flows and Exceptions

### A. Target System Unavailable

If the downstream system is unavailable, the action is retained and retried according to policy.

### B. Mapping Error

If the mapped data is incomplete or invalid, the system marks the action as failed and requires correction.

### C. Access Removal Required

If a user loses access, the deprovisioning action must still be attempted and tracked until the downstream system is no longer granting that access.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Every governed access change must be reflected downstream when the connector is enabled. |
| BR-02 | Deprovisioning is mandatory when access is revoked or expired. |
| BR-03 | Failures must be visible and retryable; they cannot silently grant or keep access. |
| BR-04 | Mapping rules must be explicit and versioned for each target system. |
| BR-05 | The same user or profile change must not create duplicate downstream operations. |
| BR-06 | The downstream access result must remain auditable. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | A connector can be registered for a downstream system. |
| 2 | Access changes generate the expected provisioning or deprovisioning action. |
| 3 | The action remains traceable until the downstream system confirms it or it is corrected. |
| 4 | Access removal triggers a deprovisioning action. |
| 5 | Failures are visible to operations and can be retried. |
| 6 | Duplicate downstream actions are prevented for the same change. |

## 8. Technical Requirements

- Introduce a connector model for downstream provisioning targets and their mappings.
- Persist provisioning actions, delivery state, retry count, and final outcome.
- Emit and consume outbox events for profile, role, and entitlement changes.
- Support manual retry and operational visibility for failed deliveries.
- Keep connectors isolated behind ACL-style adapters so external schemas do not leak into the domain.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `UserAccount`, `Profile`, `Role`, `SystemSuite`, `IdentityProvider`, `AuditRecord` |
| Functional Stories | FS-03, FS-05, FS-24 |
| ADRs | ADR-0015, ADR-0033, ADR-0072 |
