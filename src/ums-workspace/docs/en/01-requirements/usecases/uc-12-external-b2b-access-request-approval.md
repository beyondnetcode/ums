# 🧪 Use Case 12: External B2B Access Request & Approval Workflow

This document specifies the transaction flow, actors, preconditions, postconditions, and exception handling for sponsoring, approving, and provisioning access to external B2B organizations (clients, suppliers, partners) and their users under the **spec-driven AI strategy BMAD-METHOD**.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | External B2B Access Request & Approval Workflow |
| **Primary Actor** | Sponsor User (Internal Corporate Employee) |
| **Secondary Actor** | PAP Administrator (Approver), Target External User |
| **Preconditions** | Sponsor User is authenticated and belongs to an INTERNAL organization. Sponsor has CREATE_EXTERNAL_REQUEST permission. |
| **Postconditions** | An immutable EXTERNAL_ACCESS_REQUEST audit trail is created. The external User and Organization are provisioned and isolated. |

---

## 🔄 2. Transaction Flow

### A. Main Success Scenario (Happy Path)
1. **Initiation:** The Sponsor User navigates to the "B2B Access Management" module and selects "New External Request".
2. **Organization Data Entry:** The Sponsor specifies the target organization. If the organization does not exist, they provide the legal name, ERP reference code, and organization type (`CLIENT` or `SUPPLIER`).
3. **User Data Entry:** The Sponsor enters the target external user's email address and selects an allowable Profile (e.g., "Supplier Portal - Read Only").
4. **Justification:** The Sponsor writes a mandatory business justification and submits the form.
5. **Record Creation:** The .NET 8 UMS Engine creates an `EXTERNAL_ACCESS_REQUEST` record with status `PENDING_APPROVAL`.
6. **Notification:** The UMS Engine dispatches an internal notification to the PAP Administrators advising of a pending request.
7. **Evaluation:** A PAP Administrator reviews the request in the pending queue and clicks "Approve".
8. **Provisioning (Event Driven):**
    * The `EXTERNAL_ACCESS_REQUEST` status is updated to `APPROVED` with the `approved_by` audit trail.
    * The UMS Engine creates the external `ORGANIZATION` (if it didn't exist) linked to the main `tenant_id`.
    * The UMS Engine pre-registers the `USER` with the requested `PROFILE`.
9. **Onboarding:** The UMS Engine sends a secure Magic Link (Passwordless) or Welcome Email to the external user's email to complete onboarding.

---

## 🛡️ 3. Alternative Flows & Exception Handling

### Alternative Flow A: PAP Rejection
* If the PAP Administrator clicks "Reject" and provides a rejection reason: The `EXTERNAL_ACCESS_REQUEST` status is updated to `REJECTED`. The Sponsor is notified, and no external entities are provisioned.

### Alternative Flow B: Invalid Profile Requested
* If the Sponsor attempts to bypass UI controls and send an API payload requesting an internal highly-privileged Profile: The UMS Engine rejects the payload returning a `403 Forbidden` Exception, logging a security audit event for privilege escalation attempt.

### Alternative Flow C: External Organization Already Exists
* If the Engine detects the organization already exists during provisioning: The Engine safely attaches the new user to the existing organization instead of throwing a conflict error.

---

## 📋 4. Postconditions & Audit
* An immutable audit trail (`EXTERNAL_ACCESS_REQUEST`) exists explaining exactly why and who granted access to the external entity.
* The external User is logically isolated within their own `ORGANIZATION` boundary, allowing native application of Row-Level Security (RLS) in PostgreSQL.
