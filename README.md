# UMS — Enterprise User Management System

> Language: [English](./README.md) | [Español](./docs/README.es.md)

Modular monolith for identity, authorization, configuration, approvals, compliance, IGA, and audit.
Built on **.NET 10 · SQL Server 2022 · React 18 · TypeScript · Nx**.
Applied implementation of the [Evolith](https://github.com/beyondnetcode/evolith_arch32) architecture framework.

---

## Navigation Index

### Product & Requirements

| Document | Description |
|---|---|
| [Product Vision](./docs/governance/product/product-vision.md) | Strategy, goals, and positioning |
| [Business Context](./docs/governance/product/business-context.md) | Market context and problem space |
| [Scope & Boundaries](./docs/governance/product/scope.md) | What UMS is and is not |
| [Objectives (OKRs)](./docs/governance/product/objectives.md) | Measurable success criteria |
| [Stakeholders](./docs/governance/product/stakeholders.md) | Roles and responsibilities |
| [Innovation Roadmap](./docs/governance/product/innovation-roadmap.md) | Future evolution plan |
| [Glossary](./docs/governance/requirements/glossary.md) | Ubiquitous language |
| [Conceptual Data Model](./docs/governance/requirements/conceptual-data-model.md) | High-level domain model |
| [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | Full requirements backlog |
| [Permission Matrix](./docs/governance/requirements/permission-matrix-example.md) | Role/permission reference |

### Planning & Backlog

| Document | Description |
|---|---|
| [Project Backlog](./docs/governance/project/index.md) | Epics and sprint planning |
| [MVP Product Backlog](./docs/governance/project/mvp-product-backlog.md) | Prioritized MVP scope |
| [Functional Story Gap Tracker](./docs/governance/project/functional-story-gap-tracker.md) | Implementation status per story |
| [Epic 06: Approvals](./docs/governance/project/ep-06-approvals-detailed-design.md) | Approval workflow design |
| [Epic 07: Compliance](./docs/governance/project/ep-07-compliance-detailed-design.md) | Compliance module design |
| [Epic 08: IGA](./docs/governance/project/ep-08-iga-detailed-design.md) | Identity Governance design |

### Architecture

| Document | Description |
|---|---|
| [Architecture Portal](./docs/architecture/index.md) | Architecture hub |
| [Architecture Overview](./docs/architecture/overview.md) | System-wide diagram and layers |
| [Traceability Matrix](./docs/architecture/traceability-matrix.md) | FS → ADR → Technical Enabler |
| [Database Design ER](./docs/architecture/blueprints/database-design-er.md) | Entity-relationship model |
| [Service Entity Map](./docs/architecture/blueprints/service-entity-map.md) | Service-to-entity ownership |
| [Shell Library Architecture](./docs/architecture/blueprints/shell-library-architecture.md) | BeyondNetCode.Shell.* design |
| [Notification & Feedback Architecture](./docs/architecture/blueprints/notification-feedback-architecture.md) | Error/feedback wire contract |
| [Observability Architecture Flow](./docs/architecture/blueprints/observability-architecture-flow.md) | OTel tracing & logging |

### Architecture Decision Records (ADRs)

| ADR | Title | Evolith Relationship |
|---|---|---|
| [ADR-0050](./docs/architecture/adrs/0050-naming-taxonomy-standard.md) | Naming & Taxonomy Standard | Adopts Evolith ADR-0056 |
| [ADR-0051](./docs/architecture/adrs/0051-event-bus-injectable-port.md) | Event Bus Injectable Port | Adopts Evolith ADR-0015 |
| [ADR-0052](./docs/architecture/adrs/0052-immutable-audit-trail-enforcement.md) | Immutable Audit Trail | Adopts Evolith ADR-0016 |
| [ADR-0053](./docs/architecture/adrs/0053-opentelemetry-observability.md) | OpenTelemetry Observability | Adopts Evolith ADR-0007 |
| [ADR-0054](./docs/architecture/adrs/0054-shell-library-isolation.md) | Shell Library Isolation | UMS-specific |
| [ADR-0055](./docs/architecture/adrs/0055-graphql-rest-hybrid-api.md) | GraphQL/REST Hybrid API | Implements Evolith ADR-0012 |
| [ADR-0056](./docs/architecture/adrs/0056-clean-architecture-frontend.md) | Clean Architecture Frontend | Promoted → Evolith nodejs/0044 |
| [ADR-0057](./docs/architecture/adrs/0057-zustand-tanstack-query-state.md) | Zustand + TanStack Query | Promoted → Evolith nodejs/0045 |
| [ADR-0058](./docs/architecture/adrs/0058-api-gateway-yarp-evolution.md) | API Gateway YARP Evolution | Implements Evolith ADR-0008 |
| [ADR-0059](./docs/architecture/adrs/0059-single-api-tier-decision.md) | Single API Tier Decision | Override of Evolith baseline |
| [ADR-0060](./docs/architecture/adrs/0060-aop-cross-cutting-concern-strategy.md) | AOP Cross-Cutting Concern | Promoted → Evolith dotnet/0072 |
| [ADR-0061](./docs/architecture/adrs/0061-execution-context-accessor.md) | Execution Context Accessor | Promoted → Evolith dotnet/0064 |
| [ADR-0062](./docs/architecture/adrs/0062-pii-safe-serilog-configuration.md) | PII-Safe Serilog | Promoted → Evolith dotnet/0065 |
| [ADR-0063](./docs/architecture/adrs/0063-idempotency-middleware.md) | Idempotency Middleware | Promoted → Evolith dotnet/0066 |
| [ADR-0064](./docs/architecture/adrs/0064-lean-root-repository-taxonomy.md) | Lean Root Repository Taxonomy | Promoted → Evolith core/0070 |
| [ADR-0065](./docs/architecture/adrs/0065-no-raw-guids-in-ui.md) | No Raw GUIDs in UI | Promoted → Evolith nodejs/0046 |
| [ADR-0066](./docs/architecture/adrs/0066-actionable-user-error-contract.md) | Actionable User Error Contract | Promoted → Evolith nodejs/0047 |
| [ADR-0067](./docs/architecture/adrs/0067-modular-monolith-schema-per-domain.md) | Modular Monolith Schema Per Domain | Promoted → Evolith core/0067 |
| [ADR-0068](./docs/architecture/adrs/0068-feature-flag-system-scope.md) | Feature Flag System Scope | Promoted → Evolith nodejs/0048 |
| [ADR-0069](./docs/architecture/adrs/0069-domain-inheritance-strategy.md) | Domain Inheritance Strategy | Promoted → Evolith core/0071 |
| [ADR-0070](./docs/architecture/adrs/0070-database-schema-strategy-decision.md) | Database Schema Strategy | Adopts Evolith ADR-0067 |
| [ADR-0071](./docs/architecture/adrs/0071-auth-graph-engine.md) | Authorization Graph Engine | UMS-specific |
| [ADR-0072](./docs/architecture/adrs/0072-dynamic-auth-method-resolution.md) | Dynamic Auth Method Resolution | UMS-specific |
| [ADR-0073](./docs/architecture/adrs/0073-ums-sdk-multi-runtime.md) | UMS SDK Multi-Runtime | UMS-specific |
| [ADR-0074](./docs/architecture/adrs/0074-auth-graph-schema-versioning.md) | Auth Graph Schema Versioning | UMS-specific |
| [ADR-0075](./docs/architecture/adrs/0075-onboarding-approval-inbox-and-scope-based-authorization.md) | Onboarding Approval Inbox | UMS-specific |
| [ADR-0076](./docs/architecture/adrs/0076-utc-dates-timezone-language-resolution.md) | UTC Dates & Language Resolution | Promoted → Evolith core/0072 |
| [ADR-0077](./docs/architecture/adrs/0077-tenant-portal-management-authorization-boundary.md) | Tenant Portal Auth Boundary | UMS-specific |
| [ADR-0078](./docs/architecture/adrs/0078-ddd-domain-resource-hierarchy.md) | DDD Domain Resource Hierarchy | UMS-specific |
| [ADR-0079](./docs/architecture/adrs/0079-dependency-guard-policy.md) | Dependency Guard Policy | UMS-specific |
| [ADR-0080](./docs/architecture/adrs/0080-auth-graph-preview-internal-pipeline.md) | Auth Graph Preview Pipeline | UMS-specific |
| [ADR-0081](./docs/architecture/adrs/0081-semantic-auth-graph-client-contract.md) | Semantic Auth Graph Contract | UMS-specific |

### Technical Enablers & Canonical Patterns

| Document | Description |
|---|---|
| [Technical Enablers Index](./docs/architecture/blueprints/technical-enablers/index.md) | All TE documents |
| [TE-01: JWT / OIDC Flow](./docs/architecture/blueprints/technical-enablers/te-01-jwt-oidc-flow.md) | Authentication token flow |
| [TE-02: Permission Graph Compiler](./docs/architecture/blueprints/technical-enablers/te-02-permission-graph-compiler.md) | Auth graph build pipeline |
| [TE-03: Tenant Provisioning](./docs/architecture/blueprints/technical-enablers/te-03-tenant-provisioning.md) | Tenant onboarding flow |
| [TE-04: Transactional Outbox](./docs/architecture/blueprints/technical-enablers/te-04-transactional-outbox.md) | Reliable event publishing |
| [TE-05: Distributed Saga (Dapr)](./docs/architecture/blueprints/technical-enablers/te-05-distributed-saga-dapr.md) | Cross-context sagas |
| [TE-06: CQRS Projection Rebuild](./docs/architecture/blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md) | Read model rebuild strategy |
| [TE-07: YARP API Gateway](./docs/architecture/blueprints/technical-enablers/te-07-yarp-api-gateway.md) | Reverse proxy setup |
| [Canonical Patterns Index](./docs/architecture/artifacts/canonical-patterns/index.md) | All CP documents |
| [CP-01: Hexagonal Port/Adapter](./docs/architecture/artifacts/canonical-patterns/cp-01-hexagonal-port-adapter.md) | Port/adapter pattern |
| [CP-02: Aggregate Root & Domain Event](./docs/architecture/artifacts/canonical-patterns/cp-02-aggregate-root-domain-event.md) | DDD aggregate pattern |
| [CP-03: Result Pattern](./docs/architecture/artifacts/canonical-patterns/cp-03-result-pattern.md) | Error handling without exceptions |
| [CP-04: Multi-tenant Repository + RLS](./docs/architecture/artifacts/canonical-patterns/cp-04-multitenant-repository-rls.md) | Tenant-scoped data access |
| [CP-05: Execution Context Propagation](./docs/architecture/artifacts/canonical-patterns/cp-05-execution-context-propagation.md) | Request scope propagation |
| [CP-06: PII-Safe Structured Logging](./docs/architecture/artifacts/canonical-patterns/cp-06-pii-safe-structured-logging.md) | Serilog + redaction |
| [CP-07: Idempotency Middleware](./docs/architecture/artifacts/canonical-patterns/cp-07-idempotency-middleware.md) | Request deduplication |
| [CP-08: AOP Logging Decorator](./docs/architecture/artifacts/canonical-patterns/cp-08-aop-logging-decorator.md) | DispatchProxy AOP |

### Domain Model

| Document | Description |
|---|---|
| [Domain Aggregate Index](./docs/domain/index.md) | All aggregates with full structure |
| [Bounded Context Map](./docs/governance/construction/ddd-design/01-bounded-context-map.md) | Context map and relationships |
| [Ubiquitous Language](./docs/governance/construction/ddd-design/02-ubiquitous-language.md) | Domain vocabulary |
| **Identity BC** | [Tenant](./docs/domain/identity/tenant.md) · [UserAccount](./docs/domain/identity/user-account.md) · [Delegation](./docs/domain/identity/user-management-delegation.md) · [Auth Graph](./docs/domain/identity/auth-graph.md) · [Auth Method](./docs/domain/identity/auth-method-resolution.md) |
| **Authorization BC** | [SystemSuite](./docs/domain/authorization/system-suite.md) · [PermissionTemplate](./docs/domain/authorization/permission-template.md) · [Profile](./docs/domain/authorization/profile.md) |
| **Configuration BC** | [IdpConfiguration](./docs/domain/configuration/idp-configuration.md) · [AppConfiguration](./docs/domain/configuration/app-configuration.md) · [FeatureFlag](./docs/domain/configuration/feature-flag.md) |
| **Approvals BC** | [ApprovalWorkflow](./docs/domain/approvals/approval-workflow.md) · [ApprovalRequest](./docs/domain/approvals/approval-request.md) · [DocumentType](./docs/domain/approvals/document-type.md) |
| **IGA BC** | [PromotionRequest](./docs/domain/iga/promotion-request.md) · [RoleMaturityStatus](./docs/domain/iga/role-maturity-status.md) |
| **Audit BC** | [AuditRecord](./docs/domain/audit/audit-record.md) |
| [Cross-Context Flows](./docs/governance/construction/ddd-design/10-cross-context-flows.md) | Inter-context sagas and flows |
| [DDD Primitives](./docs/governance/construction/ddd-design/11-ddd-primitives.md) | Base classes and value objects |

### SDK

| Document | Description |
|---|---|
| [SDK Portal](./docs/sdk/index.md) | SDK hub |
| [Schema Overview](./docs/sdk/contracts/schema-overview.md) | AuthorizationGraph JSON schema |
| [Error Codes](./docs/sdk/contracts/error-codes.md) | AUTH_xxx code reference |
| [Versioning Policy](./docs/sdk/contracts/versioning.md) | Schema version compatibility |
| [Compatibility Matrix](./docs/sdk/contracts/compatibility-matrix.md) | Runtime version matrix |
| [Fixtures](./docs/sdk/contracts/fixtures.md) | Test fixture reference |
| [Semantic Client Contract](./docs/sdk/contracts/semantic-client-contract.md) | Code-first, ID-optional contract |
| **.NET** | [README](./docs/sdk/dotnet/README.md) · [Quickstart](./docs/sdk/dotnet/quickstart.md) |
| **TypeScript** | [README](./docs/sdk/typescript/README.md) · [Quickstart](./docs/sdk/typescript/quickstart.md) |
| **NestJS** | [README](./docs/sdk/nestjs/README.md) · [Quickstart](./docs/sdk/nestjs/quickstart.md) |

### Operations

| Document | Description |
|---|---|
| [Operations Portal](./docs/operations/index.md) | Operations hub |
| [Metrics Dashboard](./docs/operations/metrics/index.md) | API, frontend, tests, aggregate metrics |
| [RB-01: Incident Response](./docs/operations/runbooks/rb-01-incident-response.md) | On-call incident playbook |
| [RB-02: Rollback Procedure](./docs/operations/runbooks/rb-02-rollback-procedure.md) | Safe rollback steps |
| [RB-03: Cache Failure Recovery](./docs/operations/runbooks/rb-03-cache-failure-recovery.md) | Redis failure recovery |
| [RB-04: Database Failover](./docs/operations/runbooks/rb-04-database-failover.md) | SQL Server failover procedure |
| [Dev DB Anonymization](./docs/operations/runbooks/dev-db-anonymization.md) | PII anonymization for dev environments |
| [GDPR Backup Retention](./docs/operations/runbooks/gdpr-backup-retention-policy.md) | Backup retention compliance |

### QA & Testing

| Document | Description |
|---|---|
| [QA Report](./docs/qa/qa_report.md) | Overall QA status |
| [Unit Testing Results](./docs/governance/testing/unit-testing-results.md) | Unit test coverage and results |
| [Integration Testing Results](./docs/governance/testing/integration-testing-results.md) | Integration test status |
| [Performance Testing Plan](./docs/governance/testing/performance-testing-plan.md) | Load testing strategy |
| [Performance Testing Results](./docs/governance/testing/performance-testing-results.md) | Load test outcomes |
| [QA Evidences: Tenants](./docs/qa/evidences/tenants/US-001-Create-Tenant-Success.md) | US-001 through US-004 |
| [QA Evidences: Users](./docs/qa/evidences/users/US-005-Create-User.md) | US-005 through US-008 |

### Infrastructure

| Document | Description |
|---|---|
| [Infrastructure Plan](./infra/infrastructure_plan.md) | Kubernetes infrastructure design |
| [Implementation Plan](./infra/implementation_plan.md) | Deployment implementation steps |
| [K8s Deployment Plan](./infra/UMS_K8s_Deployment_Plan.md) | Full K8s deployment guide |
| [Local Access Guide](./infra/accesos_local.md) | Local environment endpoints |

### Standards & Governance

| Document | Description |
|---|---|
| [Standards](./docs/STANDARDS.md) | Engineering standards reference |
| [Documentation Version Log](./docs/releases/documentation-version-log.md) | Doc change history |
| [Release Checklist](./docs/releases/docs-v1.0.0-checklist.md) | v1.0.0 release gates |
| [Validation Summary](./docs/releases/validation-summary.md) | Documentation validation status |
| [Evolith Framework](https://github.com/beyondnetcode/evolith_arch32) | Upstream architecture reference |

---

## Quick Start

```bash
# Frontend
cd src && npm install && npx nx run app-web:dev

# Backend
cd src/apps/ums.api && dotnet build && dotnet run

# Validate docs consistency
cd src && python ../.bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

---

## License

See [LICENSE](./LICENSE) and [NOTICE](./NOTICE).
