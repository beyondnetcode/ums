# Functional Story 12: Execute Role Promotion Process

## 1. Business Purpose

UMS must support controlled role evolution so users can advance when they meet seniority, compliance, performance, or approval criteria. Promotions must be explainable and auditable.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Promotion Evaluator** | Detects users eligible for promotion. |
| **Approving Administrator** | Reviews and approves or rejects the promotion. |
| **User** | Receives the resulting role change. |

## 3. Business Preconditions

- Role hierarchy is defined.
- Promotion criteria are configured.
- The user has an active profile eligible for promotion.

## 4. Main Functional Flow

1. The system evaluates users against configured promotion criteria.
2. If a user meets the criteria, the system marks the user as eligible for promotion.
3. The responsible administrator receives the promotion opportunity.
4. The administrator reviews the evidence and decides whether to approve.
5. If approved, the user's role is updated.
6. The system records the promotion decision and its evidence.

## 5. Alternative Flows and Exceptions

### A. Criteria Not Met

If the user does not meet one or more criteria, the system keeps the user in their current role and records the reason.

### B. Promotion Rejected

If the administrator rejects the promotion, the user remains in the current role and the rejection reason is retained.

## 6. Business Rules

1. Promotion can only move toward a higher role level.
2. Mandatory compliance requirements must be satisfied before promotion.
3. Manual approval may be required depending on configured criteria.
4. Every promotion decision must be traceable.

## 7. Acceptance Criteria

1. Eligible users can be identified based on configured criteria.
2. Users with expired mandatory documents are not promoted.
3. Approved promotions update the user's effective role.
4. Rejected promotions preserve a clear reason.

## 8. Technical Requirements

- Evaluate criteria from `ROLE_PROMOTION_CRITERIA`.
- Track process state in `USER_PROMOTION_PROCESS`.
- Use `APPROVAL_REQUEST` when manual approval is required.
- Persist immutable audit events for promotion eligibility, approval, rejection, and completion.

## 9. Traceability

- Entities: `ROLE_PROMOTION_CRITERIA`, `USER_PROMOTION_PROCESS`, `ROLE`, `APPROVAL_REQUEST`
- ADRs: ADR-0046, ADR-0036
- Related Stories: FS-11, FS-14
