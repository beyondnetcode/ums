# 📘 Functional Story 16: Define Access Policy on Expiration

This document specifies the flow for configuring the automatic actions the system must take when a critical user document expires.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Define Access Policy on Expiration |
| **Primary Actor** | Security Architect / Global Administrator |
| **Preconditions** | The system has document compliance validation enabled. |
| **Postconditions** | The policy is active and will be executed by the compliance engine upon detecting expired documents. |

---

## 🔄 2. Transaction Flow

### A. Main Flow
1.  The actor selects a document type configured as "Critical for Access" (`IsAccessCritical = TRUE`).
2.  Defines the action to execute upon expiration:
    - **BLOCK_USER**: Disables the user's total access to the system.
    - **RESTRICT_PROFILE**: Blocks only specific profiles linked to the document (e.g., an expired driver's license blocks the driver profile).
    - **LOG_ONLY**: Only generates an audit alert without restricting access.
3.  Persists the configuration in `ACCESS_ENFORCEMENT_POLICY`.
4.  The compliance engine (Worker) evaluates validity daily and executes the defined action immediately upon detecting expiration.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Re-activation After Renewal
*   Once the user uploads a new valid document (FS-11) and it is approved, the system must automatically revert the access restriction imposed by the policy.

---

## 📋 4. Implementation Details

### Involved Entities
- `ACCESS_ENFORCEMENT_POLICY`
- `DOCUMENT_TYPE`
- `USER_ACCOUNT`
- `PROFILE`

### Acceptance Criteria
1.  The `BLOCK_USER` action must invalidate all active user sessions immediately.
2.  There must be a clear audit trail explaining that the block was "Automatic due to Document Expiration".
3.  The system must allow configuring grace periods before executing the definitive block.
