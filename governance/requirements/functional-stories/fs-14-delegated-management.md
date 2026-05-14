# 📘 Functional Story 14: Delegate User Management Between Administrators

This document specifies the flow for an administrator to delegate the management of a pool of users to another administrator, enabling decentralized and secure administration.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Delegate User Management Between Administrators |
| **Primary Actor** | Delegating Administrator |
| **Preconditions** | Both users hold administrative roles within the same Tenant. |
| **Postconditions** | A management relationship is created in the `USER_MANAGEMENT_DELEGATION` table. |

---

## 🔄 2. Transaction Flow

### A. Main Flow
1.  The Delegating Administrator selects one or more users under their charge.
2.  Selects the "Child Administrator" who will receive the management authority.
3.  Defines the scope of the delegation (e.g., restricted to a specific System/Suite).
4.  The system validates that the receiving administrator does not have a hierarchical level that creates a circular authority conflict.
5.  The system records the delegation in `USER_MANAGEMENT_DELEGATION`.
6.  From this point, the receiving administrator can view and manage the profiles of the delegated users within the defined scope.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Scope Not Owned
*   If the Delegating Administrator attempts to delegate permissions over a system they do not themselves administer, the system blocks the operation under the principle of "No one delegates what they do not own".

---

## 📋 4. Implementation Details

### Involved Entities
- `USER_MANAGEMENT_DELEGATION`
- `USER_ACCOUNT`
- `SYSTEM_SUITE`

### Acceptance Criteria
1.  The receiving administrator must be able to perform actions (password reset, profile assignment) only on the delegated users.
2.  The delegation must be revocable at any time by the delegating administrator or a superior administrator.
3.  The system must support multiple delegation (a user managed by more than one administrator).
