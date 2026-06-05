# FS-28: Access Review Campaigns for Role and Permission Recertification

> **Status:** Pending implementation

## 1. Business Purpose

Security and business owners need recurring campaigns to confirm that users still need the access they have. UMS must let administrators review active roles, profiles, and templates, capture reviewer decisions, and remove access that is no longer justified.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Access Governance Administrator | Creates and manages review campaigns. |
| Resource Owner / Manager | Reviews assigned access for users in scope. |
| Reviewer | Confirms, reduces, or removes access during the campaign. |
| User | Is affected by the review outcome and may receive a notification. |
| Auditor | Reviews the campaign history and the final decisions. |

## 3. Business Preconditions

- The tenant has active access assignments that require periodic review.
- Reviewers and resource owners are known for the targeted scope.
- The organization has a review cadence or a triggered review rule.

## 4. Main Functional Flow

1. The Access Governance Administrator creates a review campaign for a tenant, system, role, or profile scope.
2. The system prepares the list of access items that must be reviewed.
3. The assigned reviewer opens the campaign and sees each item with the current access state.
4. The reviewer confirms the access, reduces it, removes it, or escalates it for additional review.
5. The system records each decision and applies the resulting access change.
6. When all items are closed, the campaign is completed and remains auditable.

## 5. Alternative Flows and Exceptions

### A. Reviewer Does Not Respond

If a reviewer does not complete the campaign on time, the system escalates or closes the review according to the configured policy.

### B. Self-Review Not Allowed

If the policy forbids self-review, the user cannot approve their own access and another reviewer must complete the item.

### C. Access No Longer Exists

If the access item is already removed or inactive, the system skips it and keeps the campaign outcome consistent.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | Sensitive access must be periodically recertified. |
| BR-02 | Reviewers can only act on items that are within their assigned scope. |
| BR-03 | A review decision must be closed with a final business outcome. |
| BR-04 | Decisions must be auditable with reviewer, date, scope, and reason. |
| BR-05 | Removed access must no longer appear as active after the campaign closes. |
| BR-06 | Policies may require escalation or auto-removal when the review is overdue. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An administrator can create a review campaign for a defined scope. |
| 2 | The reviewer can see the access items that belong to the campaign. |
| 3 | The reviewer can approve, reduce, or remove access for each item. |
| 4 | The system records the reviewer decision and the final outcome. |
| 5 | Access removed during the campaign is no longer active when the campaign closes. |
| 6 | The campaign reaches a terminal and auditable completion state. |

## 8. Technical Requirements

- Introduce an access review campaign model that can track scope, reviewer assignment, item status, and final outcome.
- Persist each review decision with reviewer, timestamp, reason, and resulting access action.
- Support campaign-level escalation or auto-close rules for overdue reviews.
- Emit auditable events for campaign creation, item decision, completion, and enforcement changes.
- Keep tenant scoping strict so a reviewer only sees items in the assigned tenant and business scope.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `Role`, `Profile`, `PermissionTemplate`, `AccessEnforcementPolicy`, `AuditRecord` |
| Functional Stories | FS-16, FS-24 |
| ADRs | ADR-0016, ADR-0033, ADR-0035 |
