# 📊 Permission Matrix Example (Artifact 4)

This document presents a practical demonstration of how the **ULPMS Resolution Engine** compiles conflicting and multi-profile permissions for a single corporate user under the **bMAD Method**.

---

## 👤 User Context Setup

Let's evaluate the permissions resolved for the following user session:
*   **User Name**: `Alex Arroyo`
*   **Tenant / Organization**: `Unimar LIMA-01`
*   **Assigned Profiles**:
    1.  `Terminal Operator Profile` (Linked to Template: `OperatorBaseline_v1.0.0`)
    2.  `Billing Guest Profile` (Custom Local Profile)

---

## 📊 Permission Compilation & Resolution Matrix

The following matrix represents the state of authorizations across assigned profiles and the final compiled access resolved at runtime:

| System | Menu | Option | Action | Profile 1: Operator (Template) | Profile 2: Billing Guest (Custom) | Final Compiled Access | Resolution Rationale |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| **Inventory** | Containers | Check-In | `create` | **ALLOW** | *None* | **ALLOW** | Granted by Profile 1 (Operator). |
| **Inventory** | Containers | Check-In | `read` | **ALLOW** | *None* | **ALLOW** | Granted by Profile 1 (Operator). |
| **Inventory** | Containers | Delete | `delete` | *None* | *None* | **DENY** | **Deny-by-Default** (No active grant exists). |
| **Billing** | Invoices | View | `read` | *None* | **ALLOW** | **ALLOW** | Granted by Profile 2 (Billing Guest). |
| **Billing** | Invoices | Dispatch | `update` | **ALLOW** | **DENY** | **DENY** | **Explicit Deny Overrides** the Allow in Profile 1. |

---

## ⚙️ Key Engine Precedence Axioms

1.  **Axiom 1 (Deny-by-Default)**: An action is blocked until an explicit `ALLOW` is declared.
2.  **Axiom 2 (Permissive Union)**: If no `DENY` is present, the user inherits all active `ALLOW` blocks.
3.  **Axiom 3 (Explicit Deny Dominance)**: A `DENY` from *any* active profile instantly invalidates any matching `ALLOW` blocks across other profiles.
