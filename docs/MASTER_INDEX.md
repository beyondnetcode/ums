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
- [Notification and Feedback Architecture](./architecture/blueprints/notification-feedback-architecture.md)
- [Architecture Overview](./architecture/overview.md)
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
- [ADR-0054: Shell Library Isolation](./architecture/adrs/0054-shell-library-isolation.md) _(amended 2026-05-24 — scope extended to AOP + Bootstrapper, dep graph corrected)_
- [ADR-0059: Single API Tier Decision](./architecture/adrs/0059-single-api-tier-decision.md)
- [ADR-0060: AOP Cross-Cutting Concern Strategy](./architecture/adrs/0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Execution Context Accessor Pattern](./architecture/adrs/0061-execution-context-accessor.md) _(Evolith candidate)_
- [ADR-0062: PII-Safe Serilog Configuration](./architecture/adrs/0062-pii-safe-serilog-configuration.md) _(Evolith candidate)_
- [ADR-0063: Idempotency Key Middleware](./architecture/adrs/0063-idempotency-middleware.md) _(Evolith candidate)_
- [ADR-0066: Actionable User Error Contract](./architecture/adrs/0066-actionable-user-error-contract.md) _(Evolith candidate)_
- [ADR Registry](./architecture/adrs/index.md)
- **Shell Library Developer Guides** — [Overview](./architecture/shell-libraries/README.md) · [DDD](./architecture/shell-libraries/ddd.md) · [Factory](./architecture/shell-libraries/factory.md) · [AOP](./architecture/shell-libraries/aop.md) · [Bootstrapper](./architecture/shell-libraries/bootstrapper.md) · [Combined Usage](./architecture/shell-libraries/combined-usage.md) · [API Aspects](./architecture/shell-libraries/api-aspects.md)
- [Design Decisions & Gaps](./governance/construction/ddd-design/12-design-decisions.md)
- [Interactive DDD Viewer](./governance/construction/ddd-design/interactive-ddd-viewer.html)

### Phase 04b -- Domain Aggregate Architecture

> Phase 04b documents each Aggregate Root with 8 structured sections: Aggregate Overview · Object Model · Sequence Diagrams · ER Model · Bounded Context Model · API Contract · Persistence Notes · Security & Audit. Owned child entities (Branch, Branding, IdentityProvider, etc.) are documented within their parent Aggregate Root page — not as separate documents.

- [Domain Aggregate Index](./domain/index.md)
- **Identity BC:** [Tenant](./domain/identity/tenant.md) · [UserAccount](./domain/identity/user-account.md) · [UserManagementDelegation](./domain/identity/user-management-delegation.md)
- **Authorization BC:** [SystemSuite](./domain/authorization/system-suite.md) · [PermissionTemplate](./domain/authorization/permission-template.md) · [Profile](./domain/authorization/profile.md)
- **Configuration BC:** [IdpConfiguration](./domain/configuration/idp-configuration.md) · [AppConfiguration](./domain/configuration/app-configuration.md) · [FeatureFlag](./domain/configuration/feature-flag.md)
- **Approvals BC:** [ApprovalWorkflow](./domain/approvals/approval-workflow.md) · [ApprovalRequest](./domain/approvals/approval-request.md) · [DocumentType](./domain/approvals/document-type.md) · [UserDocument](./domain/approvals/user-document.md)
- **IGA BC:** [PromotionRequest](./domain/iga/promotion-request.md) · [RoleMaturityStatus](./domain/iga/role-maturity-status.md)
- **Audit BC:** [AuditRecord](./domain/audit/audit-record.md)

> Owned child entities (Branch, Branding, IdentityProvider, MfaEnrollment, PasswordCredential, ProfilePermission, NotificationRule, AccessEnforcementPolicy, etc.) are documented within their parent aggregate's page. Full entity inventory: [Domain Aggregate Index](./domain/index.md).

### Phase 05 -- Operations

- [Operations Portal](./operations/index.md)
- [RB-01: Incident Response](./operations/runbooks/rb-01-incident-response.md)
- [RB-02: Rollback Procedure](./operations/runbooks/rb-02-rollback-procedure.md)
- [RB-03: Cache Failure Recovery](./operations/runbooks/rb-03-cache-failure-recovery.md)
- [RB-04: Database Failover](./operations/runbooks/rb-04-database-failover.md)

### Engineering Metrics

- [Solution Metrics Dashboard](./operations/metrics/index.md)
  - API Metrics (ums.api)
  - Web Metrics (ums.web-app)
  - Library Metrics (shell/*)
  - Test Suite Metrics
  - Aggregate Metrics by Category
