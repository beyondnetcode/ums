# ADR 0031: Role Evolution and Promotion Governance

## Status
Approved

## Context
As users gain experience or complete certifications within the platform, they often move from junior roles to more senior positions. Manually managing these promotions is labor-intensive and prone to bias or oversight. We need a formal mechanism to define role hierarchies and automate the criteria verification for promotion while maintaining strict administrative oversight.

## Decision
We will implement a **Flag-Driven Role Evolution Engine**:

1.  **Hierarchical Roles**:
    *   The `ROLE` entity supports a self-referencing `ParentRoleId` to define organizational or authority trees.
    *   **Logic**: Promotion is strictly uni-directional (upwards) based on `HierarchyLevel` and `PromotionOrder`.

2.  **Toggleable Criteria (Flags)**:
    *   Promotions are governed by `ROLE_PROMOTION_CRITERIA`.
    *   Instead of hardcoded rules, we use a "Flag Table" approach:
        *   `FlagSeniority`: Checks minimum days in current role.
        *   `FlagCompliance`: Checks if all mandatory documents/certifications are valid.
        *   `FlagBusinessScore`: Checks for performance rankings.
        *   `FlagManualApproval`: Ensures the process cannot complete without human intervention.

3.  **Background Promotion Watcher**:
    *   A background worker periodically scans users against the active flags for their next potential role.
    *   When all active flags are met, it triggers a `PromotionOpportunityEvent`.

4.  **Mandatory Approval Workflow**:
    *   Meeting automated criteria only makes a user a "Candidate" (`CRITERIA_MET`).
    *   A formal `APPROVAL_REQUEST` is always initialized to finalize the promotion.

## Consequences
*   **Positive**: Encourages professional growth within the platform. Ensures that promotions are based on objective, verifiable data. Reduces manual HR/Admin overhead.
*   **Negative**: Complex to set up correctly (requires careful definition of levels). Potential for "phantom candidates" if criteria are too loose.
