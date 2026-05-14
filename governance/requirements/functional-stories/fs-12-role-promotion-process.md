# 📘 Functional Story 12: Execute Role Promotion Process

This document specifies the automated and manual flow for evolving a user's roles based on merit and compliance criteria.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Execute Role Promotion Process |
| **Primary Actor** | System (Background Worker) / Administrator (Approver) |
| **Preconditions** | Role hierarchy defined and promotion criteria configured in `ROLE_PROMOTION_CRITERIA`. |
| **Postconditions** | The user receives a new role in their profile and the promotion traceability is recorded. |

---

## 🔄 2. Transaction Flow

### A. Main Flow
1.  The background process (Worker) periodically scans users to evaluate their eligibility for the next hierarchical level.
2.  The system verifies the active flags in `ROLE_PROMOTION_CRITERIA` (Seniority, Valid Documents, Scoring).
3.  If all automated criteria are met, the system changes the process status to `CRITERIA_MET` and sends a "Promotion Opportunity" notification to the administrator.
4.  The system initializes an approval flow (`APPROVAL_REQUEST`).
5.  The administrator reviews the evidence and approves the request.
6.  The system updates the user's role in the `PROFILE` table and marks the process as `PROMOTED`.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Criteria Not Met
*   If the user does not meet one or more flags (e.g., expired document), the system maintains the `EVALUATING` status and records the reason for automatic rejection for the administrator's reference.

### Alternative Flow B: Promotion Rejection
*   If the administrator rejects the promotion (e.g., insufficient performance not measurable by flags), the process returns to `EVALUATING` with a configurable blocking period for new attempts.

---

## 📋 4. Implementation Details

### Involved Entities
- `ROLE_PROMOTION_CRITERIA`
- `USER_PROMOTION_PROCESS`
- `ROLE`
- `APPROVAL_REQUEST`

### Acceptance Criteria
1.  Promotion can only occur toward a role with a `HierarchyLevel` higher than the current one.
2.  The system must not allow promotion if mandatory documents are expired.
3.  Every successful promotion must be recorded in the immutable history with a reference to the manual approval.
