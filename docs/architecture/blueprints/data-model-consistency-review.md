# Data Model Consistency Review

**Document Type:** Architecture Consistency Review  
**Status:** Active Reference  
**Scope:** Conceptual model, DDD aggregate model, physical ER, and EF Core persistence records  
**Owner:** UMS Architecture Board

---

## 1. Purpose

This document exists to prevent drift between four views of the UMS data model:

| View | Source | Audience | Purpose |
|---|---|---|---|
| Conceptual model | [Conceptual Data Model](../../governance/requirements/conceptual-data-model.md) | Product, QA, business reviewers | Business-readable terminology and high-level relationships. |
| Domain model | [Domain Aggregate Index](../../domain/index.md) | Architects, backend engineers | DDD Aggregate Roots, owned entities, invariants, events, and behavior. |
| Physical ER model | [Database Design ER](./database-design-er.md) | Architects, DB reviewers, backend engineers | SQL Server / EF Core-aligned physical structure. |
| Persistence model | `UmsPlatformDbContext` | Backend engineers | EF Core DbSets, configurations, filters, and infrastructure persistence records. |

The **physical ER model** is authoritative for SQL Server persistence. The **Domain Aggregate Index** is authoritative for aggregate ownership and DDD terminology.

---

## 2. Terminology Decision

UMS documentation uses **DDD-first language**.

Use these terms as the primary architecture vocabulary:

- Aggregate Root
- Entity
- Owned Entity
- Value Object
- Domain Event
- Invariant
- Bounded Context

Avoid using `POCO` as the primary descriptor for the domain layer. It may be mentioned only as a .NET implementation constraint meaning "plain C# object with no framework dependency".

---

## 3. Conceptual-to-Physical Mapping

| Conceptual Name | DDD / Physical Name | Current Status | Notes |
|---|---|---|---|
| `ORGANIZATION` | `TENANT` / `Tenant` | Aligned with mapping | Conceptual language retained for business readability. |
| `USER` | `USER_ACCOUNT` / `UserAccount` | Aligned with mapping | DDD and persistence use UserAccount. |
| `BRANCH` | `BRANCH` / `Tenant.Branch` | Aligned | Owned by Tenant. |
| `PROFILE` | `PROFILE` / `Profile` | Aligned | Authorization profile. |
| `AUTH_TEMPLATE` | `PERMISSION_TEMPLATE` / `PermissionTemplate` | Aligned with mapping | Conceptual alias only. |
| `AUTHORIZATION` | `PERMISSION_TEMPLATE_ITEM` / `PROFILE_PERMISSION` | Aligned with mapping | Represents both template and materialized permission views. |
| `SYSTEM` | `SYSTEM_SUITE` / `SystemSuite` | Aligned with mapping | Physical and DDD name is SystemSuite. |
| `MODULE` | `FUNCTIONAL_MODULE` / `SystemSuite.Module` | Aligned | Functional hierarchy level. |
| `MENU` | `FUNCTIONAL_MENU` / `SystemSuite.Menu` | Aligned | Functional hierarchy level. |
| `SUBMENU` | `FUNCTIONAL_SUBMENU` / `SystemSuite.SubMenu` | Corrected | Added to conceptual model to match physical hierarchy. |
| `OPTION` | `FUNCTIONAL_OPTION` / `SystemSuite.Option` | Aligned | Functional hierarchy level. |
| `ACTION` | `ACTION` / `SystemSuite.Action` | Aligned | Action belongs to authorization graph. |
| `SYSTEM_CONFIGURATION` | `APP_CONFIGURATION` / `AppConfiguration` | Aligned with mapping | Conceptual alias for app/system behavior configuration. |
| `PROFILE_ACCESS_REQUEST` | `APPROVAL_REQUEST` / `ApprovalRequest` | Aligned with mapping | Implemented as generic approval request. |
| `EXTERNAL_ACCESS_REQUEST` | `APPROVAL_REQUEST` / `ApprovalRequest` | Watch | Dedicated persistence record should require ADR if introduced. |

---

## 4. Bounded Context vs Implementation Namespace

| Logical BC | Schema / Persistence Area | Implementation Namespace | Status | Notes |
|---|---|---|---|---|
| Identity | `ums_identity` | `Ums.Domain.Identity` | Aligned | Tenant, user, delegation. |
| Authorization | `ums_authz` | `Ums.Domain.Authorization` | Aligned | SystemSuite, Role, PermissionTemplate, Profile. |
| Configuration | `ums_config` | `Ums.Domain.Configuration` | Aligned | AppConfiguration, IdpConfiguration, FeatureFlag, parameters. |
| Approvals | `ums_approval` | `Ums.Domain.Approvals` | Aligned | ApprovalWorkflow, ApprovalRequest. |
| Compliance | `ums_compliance` | `Ums.Domain.Approvals` | Intentional unification | Logical BC remains distinct; implementation is unified for simplicity. |
| IGA | `ums_iga` | `Ums.Domain.IGA` | Aligned | PromotionRequest, RoleMaturityStatus. |
| Audit | `ums_audit` | `Ums.Domain.Audit` | Aligned | AuditRecord. |
| Cache | support | Infrastructure | Support context | Not a domain aggregate context. |
| Console | support | Presentation / Application | Support context | Not a domain aggregate context. |

---

## 5. Aggregate Root Source of Truth

The [Domain Aggregate Index](../../domain/index.md) is the source of truth for aggregate-root inventory.

Current authoritative aggregate roots:

| Bounded Context | Aggregate Roots |
|---|---|
| Identity | Tenant, UserAccount, UserManagementDelegation |
| Authorization | SystemSuite, Role, PermissionTemplate, Profile |
| Configuration | IdpConfiguration, AppConfiguration, FeatureFlag |
| Approvals / Compliance | ApprovalWorkflow, ApprovalRequest, DocumentType, UserDocument, AccessEnforcementPolicy |
| IGA | PromotionRequest, RoleMaturityStatus |
| Audit | AuditRecord |

Architecture summaries must not define independent aggregate totals. They should reference the Domain Aggregate Index instead.

---

## 6. Persistence Coverage Matrix

This matrix tracks whether EF Core persistence records are represented by the DDD and ER documentation.

| Persistence Record / DbSet | DDD Owner | ER / Conceptual Coverage | Status |
|---|---|---|---|
| `TenantRecord`, `TenantBranchRecord`, `TenantBrandingRecord`, `TenantIdentityProviderRecord` | Tenant | Identity / Tenant | Covered |
| `TenantParameterRecord` | Tenant / Configuration | Parameterization model | Covered with parameterization docs |
| `TenantSignupRequestRecord` | Tenant onboarding | Onboarding alignment | Covered |
| `UserAccountRecord`, `UserAccountPasswordCredentialRecord`, `UserAccountMfaEnrollmentRecord` | UserAccount | Identity / UserAccount | Covered |
| `UserManagementDelegationRecord` | UserManagementDelegation | Identity / delegation | Covered in domain index |
| `SystemSuiteRecord`, `SystemSuiteModuleRecord`, `SystemSuiteMenuRecord`, `SystemSuiteSubMenuRecord`, `SystemSuiteOptionRecord` | SystemSuite | Functional hierarchy | Covered after conceptual SubMenu correction |
| `SystemSuiteActionRecord` | SystemSuite | Authorization graph | Covered |
| `SystemSuiteDomainResourceRecord` | SystemSuite | Domain resource mapping | Watch — ensure ER section references domain resources explicitly |
| `SystemSuiteAppSettingRecord` | SystemSuite / Configuration | App settings | Watch — clarify if governed by AppConfiguration or suite-owned setting |
| `RoleRecord`, `RoleMaturityStatusRecord` | Role / RoleMaturityStatus | Authorization / IGA | Covered |
| `PermissionTemplateRecord`, `PermissionTemplateItemRecord` | PermissionTemplate | Authorization templates | Covered |
| `ProfileRecord`, `ProfilePermissionRecord` | Profile | Profile authorization | Covered |
| `AppConfigurationRecord` | AppConfiguration | Configuration | Covered |
| `ParameterDefinitionRecord`, `ParameterGlobalValueRecord`, `ParameterTenantValueRecord` | Configuration | Parameterization model | Covered with parameterization docs; should be visible in ER index |
| `FeatureFlagRecord`, `FeatureFlagCriteriaRecord`, `FeatureFlagEvaluationLogRecord` | FeatureFlag | Feature flags | Covered / Watch for criteria visibility |
| `IdpConfigurationRecord` | IdpConfiguration | IdP configuration | Covered |
| `ApprovalWorkflowRecord`, `ApprovalRequiredDocumentRecord`, `ApprovalRequestRecord` | ApprovalWorkflow / ApprovalRequest | Approvals | Covered |
| `DocumentTypeRecord`, `UserDocumentRecord`, `NotificationRuleRecord`, `AccessNotificationRecord`, `AccessEnforcementPolicyRecord` | Compliance under Approvals namespace | Compliance / approvals | Covered with namespace clarification |
| `PromotionRequestRecord`, `PromotionImpactAnalysisRecord` | PromotionRequest | IGA | Covered |
| `AuditRecordRecord` | AuditRecord | Audit | Covered |
| `OutboxMessage`, `DeadLetterMessage` | Infrastructure / Outbox | Operational persistence | Covered by technical enabler, not DDD aggregate |

---

## 7. Current Corrective Actions Completed

| Issue | Correction |
|---|---|
| README used `Pure POCOs` as domain description | Replaced with DDD-first terminology. |
| Architecture overview used `Pure POCO Aggregates` | Replaced with pure DDD model language. |
| Architecture portal duplicated API and Web sections | Removed duplicate entries. |
| Conceptual hierarchy missed `SubMenu` | Added `SUBMENU` to conceptual ER and attribute section. |
| Conceptual model did not clearly map to physical names | Added conceptual-to-physical mapping. |
| Approvals/Compliance namespace unification was not explicit enough | Added logical BC vs implementation namespace table. |
| Aggregate root counts diverged across documents | Established Domain Aggregate Index as the authoritative source. |

---

## 8. Open Watch Items

| Watch Item | Reason | Suggested Follow-up |
|---|---|---|
| `SystemSuiteDomainResourceRecord` ER visibility | Present in persistence model; may need explicit ER section. | Add or cross-link ER subsection if missing. |
| `SystemSuiteAppSettingRecord` ownership | Could be interpreted as Authorization or Configuration concern. | Clarify ownership via ADR or ER note. |
| `FeatureFlagCriteriaRecord` visibility | Present in persistence model and may not be obvious in high-level diagrams. | Ensure FeatureFlag aggregate page documents criteria as owned child entity. |
| `EXTERNAL_ACCESS_REQUEST` conceptual alias | Conceptual entity exists, but implementation maps to ApprovalRequest. | Dedicated persistence requires ADR before implementation. |

---

## 9. Review Rule

Every future data-model change must update at least one of the following:

1. Domain Aggregate Index and aggregate page, when behavior or ownership changes.
2. Database Design ER, when persistence changes.
3. Conceptual Data Model, when business terminology changes.
4. This consistency review, when cross-view mapping changes.

---

**[Back to Blueprints](./index.md)** | **[Back to Architecture Portal](../index.md)**
