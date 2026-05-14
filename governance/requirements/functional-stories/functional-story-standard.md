# Functional Story Writing Standard

This standard defines how UMS Functional Stories must be written so Product Owners, Business Analysts, QA, and Developers can use the same document without mixing business intent with implementation detail.

---

## 1. Required Structure

Every Functional Story MUST follow this structure:

1. **Business Purpose**: what problem is solved and why it matters.
2. **Actors**: primary and secondary participants, described by business responsibility.
3. **Business Preconditions**: conditions that must be true before the flow starts.
4. **Main Functional Flow**: user-facing business narrative, written without implementation details.
5. **Alternative Flows and Exceptions**: business-level outcomes for rejection, duplication, missing information, unavailable service, or invalid state.
6. **Business Rules**: domain rules that the Product Owner can validate.
7. **Acceptance Criteria**: testable conditions for PO/QA.
8. **Technical Requirements**: implementation constraints for developers.
9. **Traceability**: related entities, ADRs, Technical Enablers, and operational artifacts.

---

## 2. Functional Narrative Rules

Functional sections SHOULD use language understandable by a Product Owner or Business Analyst.

Functional sections MUST NOT lead with:

- API paths or HTTP methods,
- protocol names,
- database engine details,
- cache implementation details,
- payload examples,
- exception class names,
- framework/library names,
- infrastructure-specific behavior.

Those details belong in **Technical Requirements**.

---

## 3. Technical Requirements Rules

The Technical Requirements section SHOULD capture:

- APIs/endpoints,
- entities and tables,
- persistence, cache, and invalidation behavior,
- security controls,
- audit events,
- error codes,
- protocol or token requirements,
- integration contracts,
- constraints from ADRs or Technical Enablers.

This section exists so developers have precision without making the business flow harder to read.

---

## 4. Acceptance Criteria Rules

Acceptance criteria MUST be observable and business-validatable. They should describe expected outcomes, not implementation steps.

Good:

- "The sponsor can see whether the request was approved or rejected."
- "The system prevents external users from receiving internal administrative profiles."

Avoid in functional criteria:

- "The API returns `403 Forbidden`."
- "Redis keys are evicted."
- "The database writes to `APPROVAL_REQUEST`."

Move those details to Technical Requirements.
