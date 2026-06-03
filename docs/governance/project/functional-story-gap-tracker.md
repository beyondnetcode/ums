# Functional Story Gap Tracker

> **Language:** English | [Leer en espanol](../project-es/functional-story-gap-tracker.md)

Living tracker for the gap between the functional story catalog and the current implementation evidence in UMS.

## Purpose

This document keeps a dynamic view of what is already implemented, what is partial, and what is still deferred. It is intended to be updated whenever the backlog, domain design, or API implementation tracker changes.

## Source of Truth

- [Functional Stories Directory](../requirements/functional-stories/index.md)
- [MVP Product Backlog](./mvp-product-backlog.md)
- [API Aggregate Implementation Tracker](./api-aggregate-implementation-tracker.md)
- [DDD Design Portal](../construction/ddd-design/index.md)
- [Traceability Matrix](../../architecture/traceability-matrix.md)

## Coverage Snapshot

| Status | Count | Story IDs |
|---|---:|---|
| Implemented / usable | 14 | FS-01, FS-02, FS-03, FS-04, FS-05, FS-07, FS-08, FS-10, FS-11, FS-18, FS-21, FS-22, FS-26, FS-27 |
| Partial | 12 | FS-09, FS-12, FS-13, FS-14, FS-15, FS-16, FS-17, FS-19, FS-20, FS-23, FS-24, FS-25 |
| Deferred | 1 | FS-06 |

## Tracking Legend

| Field | Meaning |
|---|---|
| Status signal | `Green` = implemented / usable, `Amber` = partial, `Red` = deferred or missing |
| Priority | `P1` = highest follow-up priority, `P2` = important follow-up, `P3` = deferrable |
| Criticality | `H` = security / launch / compliance risk, `M` = product gap without immediate risk, `L` = backfill or traceability gap |
| Complexity | `H` = multi-aggregate or multi-layer change, `M` = one context with several surfaces, `L` = contained change |
| Target | Review target for the next update cycle; use `TBD` until a date is committed |
| Sort order | `FS` number asc, then `Priority` desc, then `Criticality` desc, then `Complexity` desc |

## Recently Completed

| FS | Story | Status | Completed | What was done |
|---|---|---|---|---|
| FS-10 | B2B External Access Request and Approval Flow | Green — Done | 2026-06-03 | Added `PROFILE_INTERNAL_ONLY` guard in `CreateApprovalRequestCommandHandler` (blocks external/B2B users from internal-only workflows with privilege escalation error). Added approval and rejection notifications in `ApproveRequestCommandHandler` and `RejectRequestCommandHandler`. Added domain error constant `WorkflowNotAllowedForUserCategory`. 6 new tests: guard for External and B2B categories, cross-workflow category pass, approval/rejection notification assertions. 571 tests pass. |
| FS-11 | Upload and Validate User Document | Green — Done | 2026-06-03 | Added `UserDocumentRejected` and `UserDocumentValidated` notification templates. Updated `RejectUserDocumentCommandHandler` and `ValidateUserDocumentCommandHandler` with `IUserAccountRepository` + `INotificationService` to notify the document owner. Created `RecordEnforcementExecutedCommand` + handler + `POST /user-documents/{id}/enforcement` endpoint. 8 new tests: notification assertions for reject/validate, `WhenNotPendingReview`, `WhenAlreadyExpired`, `WhenDocumentIsValid` (re-upload guard), and enforcement recording happy/failure paths. 579 tests pass. |
| FS-22 | User Signup Request and Approval | Green — Done | 2026-06-03 | Added `SignupUserCommandHandlerTests.cs` (8 tests) covering the public applicant signup flow: happy path, duplicate email, invalid tenant, and no-admin guard. Documentation updated with Status badge, corrected traceability reference, and Section 10 acceptance test evidence. |

## Open Gap Register

| FS | Story | Signal | Priority | Criticality | Complexity | Owner | Target | Status | Main gap | Next action |
|---|---|---|---|---|---|---|---|---|---|---|
| FS-06 | Auto-Assign Template on Profile Creation | Red | P3 | M | H | Authorization | TBD | Deferred | `TemplateAssignmentRule` is intentionally deferred and the automation engine is not part of the current delivery scope. | Revisit the rule engine only when auto-assignment becomes a product commitment. |
| FS-09 | Adaptive MFA and Passwordless Authentication | Amber | P1 | H | H | Identity / Security | TBD | Open | MFA has domain support, but passwordless, adaptive risk decisions, and active endpoint exposure are still incomplete. | Re-enable the API routes and complete the adaptive/passwordless flow. |
| FS-12 | Execute Role Promotion Process | Amber | P1 | H | H | IGA | TBD | Open | The promotion flow still needs the full manager/security review, execution, verification, and impact analysis closure. | Finish the promotion state machine and align the approval steps with the domain contract. |
| FS-13 | Configure Hierarchical System Parameters | Amber | P1 | H | H | Platform / Configuration | TBD | Open | Parameterization exists, but the formal Configuration context is still missing its complete API surface. | Implement the `AppConfiguration`, `FeatureFlag`, and `IdpConfiguration` APIs end to end. |
| FS-14 | Delegate User Management Between Administrators | Amber | P2 | M | M | Identity | TBD | Open | Delegation exists as a model, but the end-to-end scope and audit flow still need final validation. | Close the delegated action coverage and verify the acceptance path. |
| FS-15 | Configure Expiration Notification Rules | Amber | P2 | M | M | Compliance | TBD | Open | Notification rule management is not fully complete for updates and configuration changes. | Add mutation endpoints and complete the notification rule lifecycle. |
| FS-16 | Define Access Policy on Expiration | Amber | P2 | M | M | Compliance | TBD | Open | The enforcement policy still lacks complete update and execution-record coverage. | Implement the update action and the traceable enforcement events. |
| FS-17 | Maintain Roles for a System Suite | Amber | P2 | M | M | Authorization | TBD | Open | Role lifecycle coverage is not yet documented or verified as end-to-end complete. | Close the role maintenance workflow and refresh traceability. |
| FS-19 | Manage Admin Password Reset Validity | Amber | P2 | M | L | Identity | TBD | Open | The admin reset validity lifecycle is not yet formalized as a complete feature. | Define the workflow and align it with the configuration model. |
| FS-20 | Manage System Parameters | Amber | P1 | H | H | Platform / Configuration | TBD | Open | The tracker still marks the configuration aggregate trio as missing as a full API context. | Build repositories, commands, REST, GraphQL, and persistence for the configuration context. |
| FS-23 | Profile Access Request from Lobby User | Amber | P1 | H | H | Approvals | TBD | Open | The request model still needs the requested role and audit fidelity expected by the design. | Extend the request contract and lifecycle tracking. |
| FS-24 | Profile Request Approval and Manual Assignment | Amber | P1 | H | H | Approvals | TBD | Open | The decision record still needs requested role, granted role, reason, and notification result coverage. | Extend the approval result payload and persistence model. |
| FS-25 | Manage Domain Resources with DDD Hierarchy | Amber | P2 | L | L | Architecture | TBD | Open | This story is mostly an architecture-to-domain traceability target, and the implementation mapping is still not fully explicit. | Add a concrete implementation map or narrow the scope if the story remains documentation-only. |

## Review Cadence

- Update this tracker whenever a story moves in the backlog or the implementation tracker changes.
- Re-audit the open gaps after any domain, API, or documentation change affecting the listed stories.
- Keep the English and Spanish versions synchronized in structure and content.

## Last Review

2026-06-03
