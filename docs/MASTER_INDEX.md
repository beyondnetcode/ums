# Master Index -- UMS Navigation Hub

> **Language:** [English](./MASTER_INDEX.md) | [Español](./MASTER_INDEX.es.md)

Product Lifecycle and Engineering Specifications for the User Management System (UMS).

---

### Phase 00 -- Product Vision

- [Product Vision](./governance/product/product-vision.md)
- [Business Context](./governance/product/business-context.md)
- [Scope and Boundaries](./governance/product/scope.md)
- [Objectives (OKRs)](./governance/product/objectives.md)
- [Stakeholders](./governance/product/stakeholders.md)
- [UX/UI Design Proposal](./governance/product/ux-ui-design-proposal.md)

### Phase 01 -- Domain Requirements

- [Glossary (Ubiquitous Language)](./governance/requirements/glossary.md)
- [Conceptual Data Model](./governance/requirements/conceptual-data-model.md)
- [Permission Matrix](./governance/requirements/permission-matrix-example.md)
- [Functional Stories](./governance/requirements/functional-stories/index.md)

### Phase 02 -- MVP Planning and Project Backlog

- [Project Backlog](./governance/project/index.md)
- [MVP Product Backlog](./governance/project/mvp-product-backlog.md)
- [Epic 06: Approvals](./governance/project/ep-06-approvals-detailed-design.md)
- [Epic 07: Compliance](./governance/project/ep-07-compliance-detailed-design.md)
- [Epic 08: IGA](./governance/project/ep-08-iga-detailed-design.md)

### Phase 03 -- Architecture & Database Design

- [Database Design ER](./architecture/blueprints/database-design-er.md)
- [ER Export Formats](./architecture/blueprints/er-export-formats.md)
- [Interactive ER Viewer](./architecture/blueprints/interactive-er-viewer.html)
- [Service Entity Map](./architecture/blueprints/service-entity-map.md)
- [Shell Library Architecture](./architecture/blueprints/shell-library-architecture.md)
- [Architecture Portal](./architecture/index.md)
- [Traceability Matrix (FS → ADR → TE)](./architecture/traceability-matrix.md)
- [Technical Enablers Index](./architecture/blueprints/technical-enablers/index.md)
- [Canonical Patterns Index](./architecture/artifacts/canonical-patterns/index.md)

### Phase 04 -- Construction & Domain Design

- [Construction Portal](./governance/construction/index.md)
- [DDD Design Portal](./governance/construction/ddd-design/index.md)
- [Bounded Context Map](./governance/construction/ddd-design/01-bounded-context-map.md)
- [Ubiquitous Language](./governance/construction/ddd-design/02-ubiquitous-language.md)
- [Identity Context](./governance/construction/ddd-design/03-identity-context.md)
- [Authorization Context](./governance/construction/ddd-design/04-authorization-context.md)
- [Configuration Context](./governance/construction/ddd-design/05-configuration-context.md)
- [Audit Context](./governance/construction/ddd-design/06-audit-context.md)
- [Approvals Context](./governance/construction/ddd-design/07-approvals-context.md)
- [IGA Context](./governance/construction/ddd-design/08-iga-context.md)
- [Compliance Context](./governance/construction/ddd-design/09-compliance-context.md)
- [Cross-Context Flows](./governance/construction/ddd-design/10-cross-context-flows.md)
- [DDD Primitives](./governance/construction/ddd-design/11-ddd-primitives.md)
- [ADR-0050: Naming & Taxonomy Standard](./architecture/adrs/0050-naming-taxonomy-standard.md)
- [ADR-0051: Event Bus Injectable Port](./architecture/adrs/0051-event-bus-injectable-port.md)
- [ADR-0052: Immutable Audit Trail](./architecture/adrs/0052-immutable-audit-trail-enforcement.md)
- [ADR-0053: OpenTelemetry Observability](./architecture/adrs/0053-opentelemetry-observability.md)
- [ADR-0054: Shell Library Isolation](./architecture/adrs/0054-shell-library-isolation.md)
- [ADR Registry](./architecture/adrs/index.md)
- [Design Decisions & Gaps](./governance/construction/ddd-design/12-design-decisions.md)
- [Interactive DDD Viewer](./governance/construction/ddd-design/interactive-ddd-viewer.html)

### Phase 04b -- Domain Aggregate Architecture

- [Domain Aggregate Index](./domain/index.md)
- **Identity BC:** [Tenant](./domain/identity/tenant.md) · [Branch](./domain/identity/branch.md) · [Branding](./domain/identity/branding.md) · [IdentityProvider](./domain/identity/identity-provider.md) · [UserAccount](./domain/identity/user-account.md) · [PasswordCredential](./domain/identity/password-credential.md) · [MfaEnrollment](./domain/identity/mfa-enrollment.md)
- **Authorization BC:** [SystemSuite](./domain/authorization/system-suite.md) · [Module](./domain/authorization/module.md) · [Menu](./domain/authorization/menu.md) · [SubMenu](./domain/authorization/sub-menu.md) · [Option](./domain/authorization/option.md) · [Action](./domain/authorization/action.md) · [PermissionTemplate](./domain/authorization/permission-template.md) · [PermissionTemplateItem](./domain/authorization/permission-template-item.md) · [Profile](./domain/authorization/profile.md) · [ProfilePermission](./domain/authorization/profile-permission.md)
- **Configuration BC:** [AppConfiguration](./domain/configuration/app-configuration.md) · [FeatureFlag](./domain/configuration/feature-flag.md) · [FlagEvaluationLog](./domain/configuration/flag-evaluation-log.md) · [IdpConfiguration](./domain/configuration/idp-configuration.md)
- **Approvals BC:** [ApprovalWorkflow](./domain/approvals/approval-workflow.md) · [ApprovalRequiredDocument](./domain/approvals/approval-required-document.md) · [ApprovalRequest](./domain/approvals/approval-request.md) · [DocumentType](./domain/approvals/document-type.md) · [NotificationRule](./domain/approvals/notification-rule.md) · [UserDocument](./domain/approvals/user-document.md) · [AccessNotification](./domain/approvals/access-notification.md) · [AccessEnforcementPolicy](./domain/approvals/access-enforcement-policy.md)
- **IGA BC:** [PromotionRequest](./domain/iga/promotion-request.md) · [PromotionImpactAnalysis](./domain/iga/promotion-impact-analysis.md) · [RoleMaturityStatus](./domain/iga/role-maturity-status.md)
- **Audit BC:** [AuditRecord](./domain/audit/audit-record.md)

### Phase 05 -- Operations

- [Operations Portal](./operations/index.md)
- [RB-01: Incident Response](./operations/runbooks/rb-01-incident-response.md)
- [RB-02: Rollback Procedure](./operations/runbooks/rb-02-rollback-procedure.md)
- [RB-03: Cache Failure Recovery](./operations/runbooks/rb-03-cache-failure-recovery.md)
- [RB-04: Database Failover](./operations/runbooks/rb-04-database-failover.md)
