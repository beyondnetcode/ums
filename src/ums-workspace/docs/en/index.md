# Master Navigation Map -- UMS Knowledge Base

> **Language:** [Espanol](../es/index.md) | [English](./index.md)

Structured documentation for the User Management System following BMAD-METHOD phases.

---

### Phase 00 -- Product Vision

- [Product Vision](./00-product/product-vision.md)
- [Business Context](./00-product/business-context.md)
- [Scope and Boundaries](./00-product/scope.md)
- [Strategic Objectives (OKRs)](./00-product/objectives.md)
- [Stakeholders Map](./00-product/stakeholders.md)

### Phase 01 -- Domain Requirements

- [Glossary (Ubiquitous Language)](./01-requirements/glossary.md)
- [Conceptual Data Model](./01-requirements/conceptual-data-model.md)
- [Granular Permission Matrix](./01-requirements/permission-matrix-example.md)
- Functional Stories:
  - [FS-01: User Authentication via External IdP](./01-requirements/functional-stories/fs-01-user-authentication.md)
  - [FS-02: Create Authorization Template](./01-requirements/functional-stories/fs-02-create-authorization-template.md)
  - [FS-03: Register Organization & Configure IdP](./01-requirements/functional-stories/fs-03-register-organization.md)
  - [FS-04: Register System & Define Menu Topology](./01-requirements/functional-stories/fs-04-register-system-topology.md)
  - [FS-05: Create Profile & Assign Template](./01-requirements/functional-stories/fs-05-create-profile-manual-template.md)
  - [FS-06: Auto-Assign Template](./01-requirements/functional-stories/fs-06-auto-assign-template.md)
  - [FS-07: Visual Graph Resolver](./01-requirements/functional-stories/fs-07-visual-graph-resolver.md)
  - [FS-08: Hosted Login Page](./01-requirements/functional-stories/fs-08-hosted-login-redirection.md)
  - [FS-09: MFA & Passwordless Auth](./01-requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md)
  - [FS-10: External B2B Access Request](./01-requirements/functional-stories/fs-10-external-b2b-access-request-approval.md)
- Technical Enablers:
  - [TE-01: Compile Authorization Graph](./02-architecture/technical-enablers/te-01-build-authorization-graph.md)
  - [TE-02: Resolve Hierarchical Config](./02-architecture/technical-enablers/te-02-resolve-hierarchical-config.md)
  - [TE-03: Enforce Organization RLS](./02-architecture/technical-enablers/te-03-enforce-organization-rls-postgresql.md)

### Phase 02 -- Software Architecture

- [.NET Migration & Tech Stack Plan](./02-architecture/dotnet-migration-and-tech-stack-plan.md)
- [Bounded Context Map](./02-architecture/bounded-context-map.md)
- [C4 Architecture Spec](./02-architecture/architecture-spec.md)
- [Authoritative Technology Stack](./02-architecture/stack.md)
- [Technology Stack Cheat Sheet](./02-architecture/stack-summary.md)
- [Tactical DDD Evaluation](./02-architecture/nestjslatam-ddd-evaluation.md)
- [Vendor Lock-In Risk Assessment](./02-architecture/vendor-risk-assessment.md)

### Phase 03 -- Architectural Decision Records

- [Full ADR Registry](./03-adrs/) -- 29 active decisions
- Key ADRs:
  - [ADR-0010: Multi-Tenancy SaaS Strategy](./03-adrs/0010-multi-tenancy-architecture-strategy.md)
  - [ADR-0020: Identity Provider Abstraction](./03-adrs/0020-identity-provider-abstraction-strategy.md)
  - [ADR-0024: Configuration & Feature Management](./03-adrs/0024-configuration-feature-management-platform.md)
  - [ADR-0034: Hierarchical Multi-Tenancy](./03-adrs/0034-hierarchical-multi-tenancy-domain-model.md)

### Phase 04 -- Engineering Standards & Artifacts

- [Global Engineering Standards](./04-artifacts/engineering-standards.md)
- [Architecture Maturity Model](./04-artifacts/architecture-maturity-model.md)
- [Contract Testing Plan](./04-artifacts/contract-testing-plan.md)
- [Observability Strategy](./04-artifacts/observability-strategy.md)
- [Enterprise IAM Spec](./04-artifacts/enterprise-iam-ums-specification.md)
- [High-Concurrency Auth Spec](./04-artifacts/high-concurrency-auth-specification.md)
- [Configuration Platform Spec](./04-artifacts/ums-configuration-platform-spec.md)
- [MFA & Passwordless Spec](./04-artifacts/mfa-passwordless-security-spec.md)
- [UMS Web Console Spec](./04-artifacts/ums-web-console-product-scope.md)
- [Gap Analysis & Optimization Plan](./04-artifacts/gap-analysis-and-optimization-plan.md)
- [Multi-Tenant Governance Report](./04-artifacts/enterprise-multitenant-governance-report.md)
- [BMAD Master Audit Report](./04-artifacts/bmad-master-audit-alignment-report.md)

### Phase 05 -- Release Roadmap

- [Versioning & Release Strategy](./05-roadmap/versioning-and-audit-strategy.md)
