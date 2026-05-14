# Functional Story 11: Upload and Validate User Document

## 1. Business Purpose

Users and administrators need to provide required documents so UMS can validate identity, compliance, and access eligibility. The system must keep document status understandable and traceable.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **User** | Uploads their own required document. |
| **Identity Administrator** | Uploads or reviews documents on behalf of users. |
| **Compliance Reviewer** | Confirms whether the document is acceptable. |

## 3. Business Preconditions

- The user exists.
- The document type is configured.
- The actor has permission to upload or review the document.

## 4. Main Functional Flow

1. The actor selects the required document type.
2. The actor uploads the document file and provides issue and expiration dates.
3. The system validates that the dates are coherent.
4. The system records the document and marks its initial compliance status.
5. The document becomes available for review and future compliance checks.

## 5. Alternative Flows and Exceptions

### A. Document Already Expired

If the document is expired at upload time, the system records it as expired and starts the regularization flow.

### B. File Cannot Be Accepted

If the file is corrupted, unreadable, or violates upload rules, the system asks the actor to upload a valid file.

## 6. Business Rules

1. Expiration date must be later than issue date.
2. Required documents must be linked to a configured document type.
3. Critical documents may affect access when expired.
4. Every document status change must be traceable.

## 7. Acceptance Criteria

1. A valid document can be uploaded and linked to the user.
2. Invalid dates are rejected.
3. Expired documents are clearly marked.
4. The document can be used by compliance and access enforcement flows.

## 8. Technical Requirements

- Persist metadata in `USER_DOCUMENT`.
- Classify documents through `DOCUMENT_TYPE`.
- Store file location and checksum for retrieval and integrity validation.
- Emit audit events for upload, validation, rejection, and status changes.

## 9. Traceability

- Entities: `USER_DOCUMENT`, `DOCUMENT_TYPE`
- ADRs: ADR-0045, ADR-0016
- Related Stories: FS-15, FS-16
