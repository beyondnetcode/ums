# Functional Story Gap Tracker

> **Language:** English | [Leer en espanol](./functional-story-gap-tracker.es.md)

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
| Implemented / usable | 11 | FS-01, FS-02, FS-03, FS-04, FS-05, FS-07, FS-08, FS-18, FS-21, FS-26, FS-27 |
| Partial | 15 | FS-09, FS-10, FS-11, FS-12, FS-13, FS-14, FS-15, FS-16, FS-17, FS-19, FS-20, FS-22, FS-23, FS-24, FS-25 |
| Deferred | 1 | FS-06 |

## Tracking Legend

| Field | Meaning |
|---|---|
| Status signal | `Green` = implemented / usable, `Amber` = partial, `Red` = deferred or missing |
| Priority | `P1` = highest follow-up priority, `P2` = important follow-up, `P3` = deferrable |
| Target | Review target for the next update cycle; use `TBD` until a date is committed |

## Open Gap Register

| FS | Story | Signal | Priority | Owner | Target | Main gap | Next action | Evidence |
|---|---|---|---|---|---|---|---|---|
| FS-06 | Auto-Assign Template on Profile Creation | Red | P3 | Authorization | TBD | `TemplateAssignmentRule` is intentionally deferred and the automation engine is not part of the current delivery scope. | Revisit the rule engine only when auto-assignment becomes a product commitment. | [Design decisions](../construction/ddd-design/12-design-decisions.md); [FS-06](../requirements/functional-stories/fs-06-auto-assign-template.md) |
| FS-09 | Adaptive MFA and Passwordless Authentication | Amber | P1 | Identity / Security | TBD | MFA has domain support, but passwordless, adaptive risk decisions, and active endpoint exposure are still incomplete. | Re-enable the API routes and complete the adaptive/passwordless flow. | [Identity context](../construction/ddd-design/03-identity-context.md); [UserAccount endpoints](../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/UserAccount/UserAccountEndpoints.cs) |
| FS-10 | B2B External Access Request and Approval Flow | Amber | P1 | Approvals | TBD | The external access flow still needs the `PROFILE_INTERNAL_ONLY` guard and a fully closed request-to-approval lifecycle. | Finish the validation rules and approval traceability for external onboarding. | [Design decisions](../construction/ddd-design/12-design-decisions.md); [Approvals context](../construction/ddd-design/07-approvals-context.md) |
| FS-11 | Upload and Validate User Document | Amber | P1 | Compliance | TBD | The document lifecycle still lacks reject, expire, re-upload, notification, and enforcement recording behavior. | Complete the document lifecycle commands and the related audit trail. | [API tracker](./api-aggregate-implementation-tracker.md); [DocumentType](../../domain/approvals/document-type.md); [UserDocument](../../domain/approvals/user-document.md) |
| FS-12 | Execute Role Promotion Process | Amber | P1 | IGA | TBD | The promotion flow still needs the full manager/security review, execution, verification, and impact analysis closure. | Finish the promotion state machine and align the approval steps with the domain contract. | [API tracker](./api-aggregate-implementation-tracker.md); [PromotionRequest](../../domain/iga/promotion-request.md) |
| FS-13 | Configure Hierarchical System Parameters | Amber | P1 | Platform / Configuration | TBD | Parameterization exists, but the formal Configuration context is still missing its complete API surface. | Implement the `AppConfiguration`, `FeatureFlag`, and `IdpConfiguration` APIs end to end. | [API tracker](./api-aggregate-implementation-tracker.md); [Configuration context](../construction/ddd-design/05-configuration-context.md) |
| FS-14 | Delegate User Management Between Administrators | Amber | P2 | Identity | TBD | Delegation exists as a model, but the end-to-end scope and audit flow still need final validation. | Close the delegated action coverage and verify the acceptance path. | [Identity context](../construction/ddd-design/03-identity-context.md); [UserManagementDelegation](../../domain/identity/user-management-delegation.md) |
| FS-15 | Configure Expiration Notification Rules | Amber | P2 | Compliance | TBD | Notification rule management is not fully complete for updates and configuration changes. | Add mutation endpoints and complete the notification rule lifecycle. | [API tracker](./api-aggregate-implementation-tracker.md); [DocumentType](../../domain/approvals/document-type.md); [NotificationRule](../../domain/approvals/notification-rule.md) |
| FS-16 | Define Access Policy on Expiration | Amber | P2 | Compliance | TBD | The enforcement policy still lacks complete update and execution-record coverage. | Implement the update action and the traceable enforcement events. | [API tracker](./api-aggregate-implementation-tracker.md); [AccessEnforcementPolicy](../../domain/approvals/access-enforcement-policy.md) |
| FS-17 | Maintain Roles for a System Suite | Amber | P2 | Authorization | TBD | Role lifecycle coverage is not yet documented or verified as end-to-end complete. | Close the role maintenance workflow and refresh traceability. | [Authorization context](../construction/ddd-design/04-authorization-context.md); [SystemSuite](../../domain/authorization/system-suite.md); [Role](../../domain/authorization/role.md) |
| FS-19 | Manage Admin Password Reset Validity | Amber | P2 | Identity | TBD | The admin reset validity lifecycle is not yet formalized as a complete feature. | Define the workflow and align it with the configuration model. | [FS-19](../requirements/functional-stories/fs-19-admin-password-reset-validity-management.md) |
| FS-20 | Manage System Parameters | Amber | P1 | Platform / Configuration | TBD | The tracker still marks the configuration aggregate trio as missing as a full API context. | Build repositories, commands, REST, GraphQL, and persistence for the configuration context. | [API tracker](./api-aggregate-implementation-tracker.md); [AppConfiguration](../../domain/configuration/app-configuration.md); [FeatureFlag](../../domain/configuration/feature-flag.md); [IdpConfiguration](../../domain/configuration/idp-configuration.md) |
| FS-22 | User Signup Request and Approval | Amber | P1 | Identity | TBD | The tenant-scoped inbox and final closure semantics still need tighter alignment. | Complete the tenant inbox flow and the final notification outcome. | [Identity context](../construction/ddd-design/03-identity-context.md); [FS-22](../requirements/functional-stories/fs-22-user-signup-request-approval.md) |
| FS-23 | Profile Access Request from Lobby User | Amber | P1 | Approvals | TBD | The request model still needs the requested role and audit fidelity expected by the design. | Extend the request contract and lifecycle tracking. | [Approvals context](../construction/ddd-design/07-approvals-context.md); [ApprovalRequest](../../domain/approvals/approval-request.md) |
| FS-24 | Profile Request Approval and Manual Assignment | Amber | P1 | Approvals | TBD | The decision record still needs requested role, granted role, reason, and notification result coverage. | Extend the approval result payload and persistence model. | [Approvals context](../construction/ddd-design/07-approvals-context.md); [ApprovalRequest](../../domain/approvals/approval-request.md) |
| FS-25 | Manage Domain Resources with DDD Hierarchy | Amber | P2 | Architecture | TBD | This story is mostly an architecture-to-domain traceability target, and the implementation mapping is still not fully explicit. | Add a concrete implementation map or narrow the scope if the story remains documentation-only. | [DDD design decisions](../construction/ddd-design/12-design-decisions.md); [ADR-0078](../../architecture/adrs/0078-ddd-domain-resource-hierarchy.md) |

## Review Cadence

- Update this tracker whenever a story moves in the backlog or the implementation tracker changes.
- Re-audit the open gaps after any domain, API, or documentation change affecting the listed stories.
- Keep the English and Spanish versions synchronized in structure and content.

## Last Review

2026-06-03
