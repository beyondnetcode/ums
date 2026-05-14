# 🧪 Functional Story 10: B2B External Access Request and Approval Flow

This document specifies the transaction flow, actors, preconditions, postconditions, and exception handling for sponsoring, approving, and provisioning access for external B2B organizations (clients, suppliers, partners) and their users under the **BMAD-METHOD spec-driven AI strategy**.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | B2B External Access Request and Approval Flow |
| **Primary Actor** | Sponsor User (Internal Corporate Employee) |
| **Secondary Actors** | PAP Administrator (Approver), Target External User |
| **Preconditions** | The Sponsor User is authenticated and belongs to an `INTERNAL` organization. Holds the `CREATE_EXTERNAL_REQUEST` permission. |
| **Postconditions** | An immutable audit trail `EXTERNAL_ACCESS_REQUEST` is created. The external User and Organization are provisioned and logically isolated. |

---

## 🔄 2. Transaction Flow

### A. Main Flow (Happy Path)
1. **Initiation:** The Sponsor User navigates to the "B2B Access Management" module and selects "New External Request".
2. **Organization Data Entry:** The Sponsor specifies the target organization. If the organization does not exist, they provide the legal name, ERP reference code, and organization type (`CLIENT` or `SUPPLIER`).
3. **User Data Entry:** The Sponsor enters the target external user's email and selects an allowed Profile (e.g. "Supplier Portal — Read Only").
4. **Justification:** The Sponsor writes a mandatory business justification and submits the form.
5. **Record Creation:** The UMS Engine (.NET 8) creates an `EXTERNAL_ACCESS_REQUEST` record with status `PENDING_APPROVAL`.
6. **Notification:** The UMS Engine dispatches an internal notification to PAP Administrators alerting them of a pending request.
7. **Evaluation:** A PAP Administrator reviews the request in the pending queue and clicks "Approve".
8. **Provisioning (Event-Driven):**
    * The `EXTERNAL_ACCESS_REQUEST` status is updated to `APPROVED` along with the `approved_by` audit trail.
    * The UMS Engine creates the external `ORGANIZATION` (if it did not exist) linked to the primary `tenant_id`.
    * The UMS Engine pre-registers the `USER` with the requested `PROFILE`.
9. **Onboarding:** The UMS Engine sends a secure Magic Link (Passwordless) or a Welcome Email to the external user's email address to complete system enrollment.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Rejection by the PAP
* If the PAP Administrator clicks "Reject" and provides a rejection reason: The `EXTERNAL_ACCESS_REQUEST` status is updated to `REJECTED`. The Sponsor is notified and no external entities are provisioned.

### Alternative Flow B: Invalid Requested Profile
* If the Sponsor attempts to bypass UI controls and sends an API payload requesting a highly privileged internal Profile: The UMS Engine rejects the request returning a `403 Forbidden` exception, logging a security audit event for privilege escalation attempt.

### Alternative Flow C: External Organization Already Exists
* If the Engine detects that the organization already exists during provisioning: The Engine safely attaches the new user to the existing organization instead of raising a conflict error.

---

## 📋 4. Postconditions and Audit
* An immutable audit trail (`EXTERNAL_ACCESS_REQUEST`) exists that explains exactly why and by whom access was granted to the external entity.
* The external User is logically isolated within their own `ORGANIZATION` boundary, enabling native Row-Level Security (RLS) enforcement in **SQL Server 2022** via `SESSION_CONTEXT` and Security Policies.
