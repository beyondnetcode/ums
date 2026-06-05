# FS-32: Operational Reliability Guardrails for Governance Actions

> **Status:** Pending implementation

## 1. Business Purpose

Administrators need predictable governance operations even when requests are repeated, concurrent, or retried. UMS must prevent duplicate state, avoid cross-tenant mistakes, and keep failures visible so access administration remains trustworthy.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Platform Administrator | Performs tenant and system governance actions. |
| Tenant Administrator | Performs scoped administration for a tenant. |
| Support Engineer | Investigates failed or duplicated actions. |
| Auditor | Reviews whether the final state is trustworthy and complete. |

## 3. Business Preconditions

- The actor is authenticated with the correct tenant or platform scope.
- The target entity exists and is eligible for the requested action.
- The tenant context is available before the operation is submitted.

## 4. Main Functional Flow

1. The actor submits a governance action such as create, update, approve, or revoke.
2. The system checks whether the same action was already submitted.
3. The system checks whether a concurrent edit would overwrite a newer change.
4. If the action is valid, the system completes it once and records the result.
5. If a failure happens during delivery or retry, the system keeps the outcome visible instead of silently losing it.
6. The actor sees a clear success, conflict, or retry outcome.

## 5. Alternative Flows and Exceptions

### A. Duplicate Submission

If the same action is submitted again, the system reuses the prior outcome or rejects the duplicate instead of creating duplicate state.

### B. Concurrent Update Conflict

If another actor changed the same record first, the system asks for a fresh read instead of overwriting the newer value.

### C. Missing Tenant Context

If the request does not carry a valid tenant context, the action is rejected and not applied globally.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Repeated submissions must not create duplicate governance state. |
| BR-02 | Concurrent updates must not silently overwrite newer changes. |
| BR-03 | Tenant scope must always be known before a change is accepted. |
| BR-04 | Delivery retries must not lose the business result or the audit trail. |
| BR-05 | Operational failures must be visible to support and audit users. |
| BR-06 | Cross-tenant data must never be mixed by mistake. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | Repeating the same action does not create duplicate business state. |
| 2 | A concurrent change is detected instead of being silently overwritten. |
| 3 | A request without tenant context is rejected. |
| 4 | Failed deliveries remain visible until they are resolved. |
| 5 | Support and audit users can explain the final outcome of the action. |
| 6 | The final state remains consistent after retries. |

## 8. Technical Requirements

- Introduce request deduplication for supported command flows.
- Add optimistic concurrency checks for mutable records that can be edited concurrently.
- Enforce tenant scoping before the operation enters the application workflow.
- Keep outbox or event delivery observable so operations can see when a change is still pending.
- Surface actionable conflict and retry feedback instead of generic failures.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `Tenant`, `UserAccount`, `Profile`, `ApprovalRequest`, `AppConfiguration`, `AuditRecord` |
| Functional Stories | FS-03, FS-05, FS-13, FS-24 |
| ADRs | ADR-0010, ADR-0033, ADR-0063, ADR-0066 |
