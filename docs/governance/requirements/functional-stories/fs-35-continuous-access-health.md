# FS-35: Continuous Access Health and Recommendations

> **Status:** Pending implementation

## 1. Business Purpose

Security and governance teams need a continuous view of access quality, not just point-in-time approvals. UMS must calculate access health, highlight stale or excessive access, and recommend the next best governance action.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Security Administrator | Reviews the access health signals and thresholds. |
| IGA Administrator | Uses recommendations to launch cleanup or review actions. |
| Auditor | Reviews the health trend and the remediation trail. |

## 3. Business Preconditions

- UMS has enough access, audit, and review data to evaluate health signals.
- The tenant has a policy that defines what is healthy, risky, or stale.
- The actor can access the governance diagnostics area.

## 4. Main Functional Flow

1. The actor opens the access health dashboard.
2. The system computes a health score using signals such as stale access, unused access, expired reviews, and overprivileged roles.
3. The actor inspects the reasons behind the score.
4. The system recommends a next action, such as opening a review campaign, trimming a package, or expiring a privileged grant.
5. The actor can use the recommendation to launch the next governance step.

## 5. Alternative Flows and Exceptions

### A. Insufficient Data

If the system does not have enough signals to compute a reliable score, it shows the limitation clearly.

### B. Critical Risk Threshold

If the score crosses a critical threshold, the system highlights the accounts or packages that need immediate review.

### C. No Remediation Needed

If the access is healthy, the system explains why and keeps the score visible for trend monitoring.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | The access health score must be explainable. |
| BR-02 | Recommendations must be derived from observable access and audit signals. |
| BR-03 | The system must not auto-remove access unless policy explicitly allows it. |
| BR-04 | High-risk trends must be visible before they become incidents. |
| BR-05 | Health scoring must be auditable and repeatable for the same dataset. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An actor can see a health score for access within a tenant or scope. |
| 2 | The system explains the signals that influenced the score. |
| 3 | The system recommends a next governance action. |
| 4 | The actor can use the recommendation to launch a review or cleanup flow. |
| 5 | Healthy and risky access states are visible over time. |

## 8. Technical Requirements

- Compute the health score from audit, assignment, review, and expiry signals.
- Provide explainable scoring rules and thresholds.
- Support recommendations without automatically changing access.
- Allow health outcomes to feed review campaigns, package cleanup, and privileged access expiry.
- Preserve tenant isolation and deterministic recalculation from the same inputs.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `AuditRecord`, `Profile`, `Role`, `ApprovalRequest`, `AccessEnforcementPolicy` |
| Functional Stories | FS-16, FS-28, FS-31, FS-32 |
| ADRs | ADR-0016, ADR-0033, ADR-0066 |
