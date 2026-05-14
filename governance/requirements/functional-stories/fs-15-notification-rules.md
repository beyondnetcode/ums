# 📘 Functional Story 15: Configure Expiration Notification Rules

This document specifies the flow for parameterizing preventive alerts sent to users before their documentation expires.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Configure Expiration Notification Rules |
| **Primary Actor** | Compliance Administrator |
| **Preconditions** | The `DOCUMENT_TYPE` is configured. |
| **Postconditions** | Notification rules are persisted and ready to be processed by the alert engine. |

---

## 🔄 2. Transaction Flow

### A. Main Flow
1.  The administrator selects the document type to parameterize.
2.  Defines the number of advance days for the alert (e.g., 30 days, 15 days, 5 days).
3.  Selects the delivery channel (Email, In-App, SMS).
4.  The system allows adding multiple notification steps for the same document.
5.  The system persists the rules in the `NOTIFICATION_RULE` table.
6.  The background process (Worker) will use these rules to compare against the `ExpirationDate` of user documents and trigger alerts.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Rule Duplication
*   If an identical rule (same day and channel) is attempted for the same Tenant/Document, the system requests modification of the parameter to avoid notification spam.

---

## 📋 4. Implementation Details

### Involved Entities
- `NOTIFICATION_RULE`
- `DOCUMENT_TYPE`

### Acceptance Criteria
1.  The system must allow configuring at least 3 default alert levels.
2.  Notifications must be filterable by Tenant to allow differentiated regional policies.
3.  The system must record the traceability of each successfully sent notification.
