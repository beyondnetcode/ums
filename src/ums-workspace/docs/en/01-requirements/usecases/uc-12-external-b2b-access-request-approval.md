# 📖 UC-12: External B2B Access Request & Approval Workflow

This document specifies the transaction flow, actors, preconditions, postconditions, and exception handling for sponsoring, approving, and provisioning access to external B2B organizations (clients, suppliers, partners) and their users under the **spec-driven AI strategy BMAD-METHOD**.

---

## 🎯 1. Objective
To provide a secure, auditable, and federated workflow allowing internal corporate users (Sponsors) to request access for external third-party organizations and their employees, ensuring explicit approval from a Policy Administration Point (PAP) before any external identity is provisioned or granted access to the UMS.

---

## 🎭 2. Actors
*   **Sponsor User (Internal):** A corporate employee (e.g., Commercial Executive, Procurement Officer) who initiates the access request for a third-party organization.
*   **PAP Administrator / Owner:** An authorized user with governance privileges who evaluates and approves or rejects the external access request.
*   **External User (Target):** The third-party employee (Client/Supplier) who will receive the invitation and onboard the system once approved.
*   **UMS Engine:** The background processor handling event dispatching, organization creation, user pre-registration, and email delivery.

---

## 🚦 3. Preconditions
1.  The Sponsor User must have an active session and belong to an internal organization (`ORGANIZATION.type = INTERNAL`).
2.  The Sponsor User must possess the `CREATE_EXTERNAL_REQUEST` permission in their compiled authorization graph.
3.  The suggested Profile for the external user must be explicitly marked as safe for external use (e.g., cannot be an internal Admin profile).

---

## 🚀 4. Main Success Scenario (Happy Path)

1.  **Initiation:** The Sponsor User navigates to the "B2B Access Management" module and selects "New External Request".
2.  **Organization Data Entry:** The Sponsor specifies the target organization. If the organization does not exist, they provide the legal name, ERP reference code, and organization type (`CLIENT` or `SUPPLIER`).
3.  **User Data Entry:** The Sponsor enters the target external user's email address and selects an allowable Profile (e.g., "Supplier Portal - Read Only").
4.  **Justification:** The Sponsor writes a mandatory business justification and submits the form.
5.  **Record Creation:** The UMS Engine creates an `EXTERNAL_ACCESS_REQUEST` record with status `PENDING_APPROVAL`.
6.  **Notification:** The UMS Engine dispatches an internal notification (or email) to the PAP Administrators advising of a pending request.
7.  **Evaluation:** A PAP Administrator reviews the request in the pending queue and clicks "Approve".
8.  **Provisioning (Event Driven):**
    *   The `EXTERNAL_ACCESS_REQUEST` status is updated to `APPROVED` with the `approved_by` audit trail.
    *   The UMS Engine creates the external `ORGANIZATION` (if it didn't exist) linked to the main `tenant_id`.
    *   The UMS Engine pre-registers the `USER` with the requested `PROFILE`.
9.  **Onboarding:** The UMS Engine sends a secure Magic Link (Passwordless) or Welcome Email to the external user's email to complete onboarding.

---

## 🛑 5. Exception Paths

*   **5a. PAP Rejection:**
    *   *Step 7 alternative:* The PAP Administrator clicks "Reject" and provides a rejection reason.
    *   *Resolution:* The `EXTERNAL_ACCESS_REQUEST` status is updated to `REJECTED`. The Sponsor is notified, and no external entities are provisioned.
*   **5b. Invalid Profile Requested:**
    *   *Step 3 alternative:* The Sponsor attempts to bypass UI controls and send an API payload requesting an internal highly-privileged Profile.
    *   *Resolution:* The UMS Engine rejects the payload returning a `403 Forbidden` Exception, logging a security audit event for privilege escalation attempt.
*   **5c. External Organization Already Exists:**
    *   *Step 8 alternative:* The Engine detects the organization already exists during provisioning.
    *   *Resolution:* The Engine safely attaches the new user to the existing organization instead of throwing a conflict error.

---

## 🏁 6. Postconditions
*   An immutable audit trail (`EXTERNAL_ACCESS_REQUEST`) exists explaining exactly why and who granted access to the external entity.
*   The external User is logically isolated within their own `ORGANIZATION` boundary, allowing native Row-Level Security (RLS) enforcement.
