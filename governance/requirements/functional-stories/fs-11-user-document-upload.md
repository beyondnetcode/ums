# 📘 Functional Story 11: Upload and Validate User Document

This document specifies the flow for uploading, recording metadata, and validating documents required for user compliance and identity.

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Upload and Validate User Document |
| **Primary Actor** | User / Identity Administrator |
| **Preconditions** | The user exists and the `DOCUMENT_TYPE` is configured in the system. |
| **Postconditions** | The document is stored in the file server and the operational record is persisted with its validity date. |

---

## 🔄 2. Transaction Flow

### A. Main Flow
1.  The actor selects the type of document to upload (e.g., Identity, Contract, Certificate).
2.  The actor attaches the physical file and provides the issue date (`IssueDate`) and expiration date (`ExpirationDate`).
3.  The system generates an integrity hash (`Checksum`) of the file.
4.  The system stores the file in the file server/cloud storage and retrieves the access path (`FileStoragePath`).
5.  The system registers the `USER_DOCUMENT` entity linking the file to the user and classifying it by type.
6.  The system marks the document status as `VALID` (if within the date range) or `PENDING_RENEWAL`.

---

## 🛡️ 3. Alternative Flows and Exception Handling

### Alternative Flow A: Document Expired on Upload
*   If the entered expiration date is earlier than the current date, the system registers the document with status `EXPIRED` and triggers an immediate regularization notification.

### Alternative Flow B: Integrity Error
*   If the Checksum calculation fails or a corrupted file is detected, the system aborts the persistence and requests the user to upload the file again.

---

## 📋 4. Implementation Details

### Involved Entities
- `USER_DOCUMENT`
- `DOCUMENT_TYPE`

### Acceptance Criteria
1.  The system must reject files that exceed the configured maximum size.
2.  The expiration date must mandatorily be later than the issue date.
3.  The database record must contain the exact file server path for subsequent retrieval.
