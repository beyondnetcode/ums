# ADR 0045: Automated Document-Based Access Enforcement

## Status
Approved

## Context
In a highly regulated environment, system access must be conditional upon the validity of supporting documentation (identity, B2B contracts, certificates). Manually monitoring expiration dates is inefficient and error-prone. We need a system that proactively alerts users and automatically restricts access when critical documentation expires.

## Decision
We will implement an **Automated Document Lifecycle & Enforcement** framework:

1.  **Unified Document Repository**:
    *   `USER_DOCUMENT` replaces simple attachments, adding `IssueDate`, `ExpirationDate`, and `Status`.
    *   Documents are classified via `DOCUMENT_TYPE`, which defines if the document is "Access Critical".

2.  **Parametric Notification Engine**:
    *   The `NOTIFICATION_RULE` table allows defining N-step alerts (e.g., 60, 30, 15, 5 days before expiration).
    *   Alerts are scoped by Tenant, User Category, and Document Type.

3.  **Automated Enforcement Policies**:
    *   The `ACCESS_ENFORCEMENT_POLICY` defines the action to take upon expiration (e.g., `BLOCK_USER`).
    *   **Logic**: If a `DOCUMENT_TYPE` marked as `IsAccessCritical` expires, the associated policy is triggered.

4.  **Decoupled, Event-Driven Architecture**:
    *   **Worker/Scheduler**: Scans documents daily and publishes events (`DocumentNearExpirationEvent`, `DocumentExpiredEvent`).
    *   **Notification Service**: Consumes expiration events and dispatches multi-channel alerts.
    *   **Policy Engine**: Consumes `DocumentExpiredEvent` and interacts with `IdentityService` to lock accounts or restrict profiles.

## Consequences
*   **Positive**: Guarantees 100% compliance with identity governance requirements. Reduces manual overhead for tenant admins. Provides a clear audit trail of why access was restricted.
*   **Negative**: Users may be blocked unexpectedly if they ignore multiple notifications. Requires robust "Grace Period" logic and easy renewal workflows in the UI.
