# FS-33: Authorization Graph Explorer with What-If Simulation

> **Status:** Pending implementation

## 1. Business Purpose

Authorization administrators and support teams need to understand the impact of a change before it goes live. UMS must let them explore the effective authorization graph, compare current and proposed access, and preview the result of a profile, template, or package change.

## 2. Actors

| Actor | Responsibility |
|---|---|
| Authorization Administrator | Verifies the effective graph before approving changes. |
| Support Engineer | Diagnoses access issues using a safe simulation. |
| Auditor | Reviews why access changed or did not change. |

## 3. Business Preconditions

- The target profile, template, or package exists or is being proposed.
- The actor has diagnostic access to the relevant tenant scope.
- The current authorization graph can be resolved for the selected scope.

## 4. Main Functional Flow

1. The actor opens the graph explorer for a profile, template, or access package.
2. The system renders the current effective authorization graph.
3. The actor proposes a change and triggers a what-if simulation.
4. The system shows the expected graph after the change.
5. The actor compares current vs proposed access and identifies new or removed permissions.
6. The actor uses the result to approve, adjust, or reject the change.

## 5. Alternative Flows and Exceptions

### A. Graph Cannot Be Resolved

If the graph cannot be resolved for the selected scope, the system shows the reason and keeps the data unchanged.

### B. Proposed Change Creates Excess Access

If the proposed change expands access beyond the intended scope, the system highlights the risky paths so the actor can adjust the request.

### C. Incomplete Context

If the simulation lacks enough context to evaluate the graph, the system makes that limitation visible instead of guessing.

## 6. Business Rules

| Rule | Description |
|---|---|
| BR-01 | The explorer must not change live access by itself. |
| BR-02 | Current and proposed graphs must be clearly distinguishable. |
| BR-03 | The preview must explain the effective paths that grant access. |
| BR-04 | A change that increases access must be visible before approval. |
| BR-05 | The graph preview must be auditable as a diagnostic action. |

## 7. Acceptance Criteria

| # | Acceptance Criterion |
|---|---|
| 1 | An authorized actor can preview the current effective authorization graph. |
| 2 | The actor can simulate a proposed change before approving it. |
| 3 | The explorer shows the difference between current and proposed access. |
| 4 | The preview does not modify live access. |
| 5 | The result can be used to support approval, adjustment, or rejection. |

## 8. Technical Requirements

- Reuse the authorization graph engine for both live and simulated evaluation.
- Support a read-only diff view that compares the resolved graph before and after the proposed change.
- Keep the preview endpoint or query isolated from write operations.
- Emit diagnostic audit events for preview and simulation access.
- Preserve tenant scoping and permission checks during graph resolution.

## 9. Traceability

| Type | References |
|---|---|
| Domain Entities | `Profile`, `PermissionTemplate`, `Role`, `SystemSuite`, `AuditRecord` |
| Functional Stories | FS-07, FS-24, FS-29 |
| ADRs | ADR-0021, ADR-0071, ADR-0074 |
