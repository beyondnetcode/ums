# Post-MVP Readiness Audit — EP-06, EP-07, EP-08

**Version:** 1.0
**Date:** 2026-05-14
**Objective:** Audit completeness of 3 post-MVP epics; identify gaps and analysis priorities.

---

## 1. Executive Summary

| Epic | Description | User Stories | Current Readiness | Critical Gap |
|------|-------------|---------------|-------------------|--------------|
| **EP-06** | Security, External Access, Delegation | US-017 to US-022 | 60% | FS-09, FS-14 incomplete; ADRs clear |
| **EP-07** | Compliance Lifecycle | US-023 to US-028 | 40% | FS-15, FS-16 pending; data model missing |
| **EP-08** | Advanced IGA (Role Promotion) | US-031, US-032 | 30% | FS-12 only 2 US; state machine incomplete |

**Recommendation:** Expand in sequence: **EP-06 → EP-07 → EP-08** (cascading dependencies)

---

## 2. EP-06: Security, External Access & Delegation

### 2.1 Current Status (60% Readiness)

#### Exists

| Component | Location | Details |
|-----------|----------|---------|
| **ADR-0044** | `/architecture/adrs/0044-delegated-admin-and-approvals.md` | Delegated admin + approval workflows defined |
| **ADR-0026** | `/architecture/adrs/0026-mfa-passwordless-adaptive-authentication.md` | MFA & passwordless strategy |
| **ADR-0038** | `/architecture/adrs/0038-delegated-administration-temporal-scopes.md` | Temporal scopes for delegation |
| **Approvals Context** | `/architecture/blueprints/bounded-context-map.md` | Defined with entities, routes, events |
| **User Stories** | `/governance/project/mvp-product-backlog.md` | US-017 to US-022 present |

#### Incomplete

| Component | Problem | Priority | Effort |
|-----------|---------|----------|--------|
| **FS-09: Adaptive MFA & Passwordless** | Header only; FS lacks acceptance criteria | HIGH | 3h |
| **FS-14: Delegated Admin Scopes** | Incomplete; state machine validation missing | HIGH | 4h |
| **ER Model for Approvals** | Not mapped to SQL entities; closure table for hierarchical approval missing | HIGH | 2h |
| **API Contract (OpenAPI)** | No detailed routes; DTOs pending | MEDIUM | 2h |
| **Integration Points** | Unclear how Approvals integrates with Authorization context | MEDIUM | 2h |

#### Completely Missing

| Component | Impact | Effort |
|-----------|--------|--------|
| **Approval State Machine** | What are valid states? Transitions? Timeout? | 3h |
| **Escalation Rules** | What happens if approver doesn't respond in X days? | 2h |
| **Delegation Validation Logic** | Principle of Least Privilege — how implemented in code? | 2h |

---

### 2.2 Functional Stories Detail (EP-06)

#### FS-09: Adaptive MFA & Passwordless Authentication

**User Stories:** US-017, US-018

**Gaps:**
- [ ] Risk scoring model (what qualifies as "high risk"?)
- [ ] Passwordless methods (FIDO2, magic link, etc.)
- [ ] Configuration UI/API
- [ ] Integration with Identity Context (FS-01)

**Dependencies:**
- ADR-0026 (MFA strategy)
- Identity Context (base authentication)

**Estimate:** 8 points (1-2 sprints)

---

#### FS-14: Delegated Administration & Scopes

**User Stories:** US-021, US-022

**Gaps:**
- [ ] Delegation table schema (what fields define a "scope"?)
- [ ] Scope validation (can delegated admin perform X action?)
- [ ] Temporal constraints (delegation expiration)
- [ ] Approval required to create delegation
- [ ] Audit logging of delegation changes

**Dependencies:**
- ADR-0044 (delegated admin)
- ADR-0038 (temporal scopes)
- Audit Context (auditing)
- Approvals Context (approval workflow)

**Estimate:** 13 points (2-3 sprints)

---

#### FS-10: B2B External Access Request & Approval Flow

**User Stories:** US-019, US-020

**Status:** Partially defined (Approvals Context exists)

**Gaps:**
- [ ] Document attachment handling (what documents required?)
- [ ] Approval chain (who approves? serial vs parallel?)
- [ ] Expiration enforcement (how revoke access on expiry?)
- [ ] Integration with Compliance Context (FS-11 document validation)

**Dependencies:**
- Approvals Context (workflow)
- Audit Context (trail)
- Configuration Context (approval rules)

**Estimate:** 8 points (1-2 sprints)

---

### 2.3 ER Model Gaps (EP-06)

```sql
-- Missing complete definition
CREATE TABLE [approval].[approval_workflows] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [name] VARCHAR(255),
 [trigger_type] VARCHAR(32), -- 'USER_ONBOARDING', 'PROFILE_ASSIGNMENT', 'DELEGATION'
 [approval_type] VARCHAR(32), -- 'SERIAL', 'PARALLEL', 'QUORUM'
 [required_approvals] INT,
 [timeout_days] INT,
-- Missing: approval_rules, escalation_config, metadata
);

-- Incomplete
CREATE TABLE [approval].[approval_requests] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [workflow_id] UNIQUEIDENTIFIER,
 [requester_id] UNIQUEIDENTIFIER,
 [target_user_id] UNIQUEIDENTIFIER,
 [requested_action] VARCHAR(255),
 [status] VARCHAR(32), -- 'PENDING', 'APPROVED', 'REJECTED'
 [created_at] DATETIME2,
 [expires_at] DATETIME2,
-- Missing: approval_attachments relationship, escalation tracking
);

-- Missing
CREATE TABLE [approval].[approval_approvers] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [approval_request_id] UNIQUEIDENTIFIER,
 [approver_id] UNIQUEIDENTIFIER,
 [approval_status] VARCHAR(32), -- 'PENDING', 'APPROVED', 'REJECTED'
 [approval_order] INT, -- For SERIAL approvals
 [approved_at] DATETIME2,
 [decision_reason] NVARCHAR(MAX),
 [root_tenant_id] UNIQUEIDENTIFIER
);

-- Missing
CREATE TABLE [delegation].[user_management_delegations] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [delegating_admin_id] UNIQUEIDENTIFIER, -- Who delegates
 [delegated_admin_id] UNIQUEIDENTIFIER, -- To whom
 [scope_type] VARCHAR(32), -- 'TENANT', 'ORGANIZATION', 'SYSTEM'
 [scope_id] UNIQUEIDENTIFIER,
 [allowed_actions] VARCHAR(MAX), -- JSON: ['CREATE_USER', 'ASSIGN_PROFILE', ...]
 [valid_from] DATETIME2,
 [valid_until] DATETIME2,
 [approval_request_id] UNIQUEIDENTIFIER, -- Link to approval
 [status] VARCHAR(32) -- 'ACTIVE', 'PENDING', 'EXPIRED', 'REVOKED'
);
```

**Impact:** Blocks FS-14 (delegation) and FS-10 (B2B approvals) implementation.

---

## 3. EP-07: Compliance Lifecycle

### 3.1 Current Status (40% Readiness)

#### Exists

| Component | Location | Details |
|-----------|----------|---------|
| **ADR-0045** | `/architecture/adrs/0045-document-lifecycle-enforcement.md` | Document lifecycle defined |
| **User Stories** | MVP backlog | US-023 to US-028 present |

#### Incomplete

| Component | Problem | Priority |
|-----------|---------|----------|
| **FS-15: Expiration Notification Rules** | Does not exist; only named in backlog | HIGH |
| **FS-16: Access Behavior on Expiration** | Does not exist; enforcement model missing | HIGH |
| **FS-11: Document Lifecycle** | Partial; validation workflow missing | MEDIUM |
| **ER Model** | Document, notification, policy tables not mapped | HIGH |

#### Completely Missing

| Component | Impact |
|-----------|--------|
| **Compliance Context** | Bounded context NOT defined |
| **Notification Engine** | How alerts sent (email, webhook, etc.)? |
| **Policy Enforcement Logic** | What happens when access expires (warning, suspend, revoke)? |
| **Document Validation Workflow** | Document states, validators, revalidation? |

---

### 3.2 Functional Stories Detail (EP-07)

#### FS-11: Upload & Validate User Document

**User Stories:** US-023, US-024

**Gaps:**
- [ ] Document types (what documents accepted?)
- [ ] Storage system (where stored? cloud storage? encrypted?)
- [ ] Validation workflow (who validates? what criteria?)
- [ ] Revalidation trigger (how often revalidated?)
- [ ] Integration with expiration enforcement

**Estimate:** 8 points

---

#### FS-15: Expiration Notification Rules (MISSING)

**User Stories:** US-025, US-026

**Must Define:**
- [ ] Rule model (what is a notification rule?)
- [ ] Trigger conditions (30 days before expiry? configurable?)
- [ ] Notification channels (email, SMS, in-app?)
- [ ] Audience (who notified: user, admin, both?)
- [ ] Frequency (single notification or repeated?)

**Estimate:** 8 points

---

#### FS-16: Access Behavior on Expiration (MISSING)

**User Stories:** US-027, US-028

**Must Define:**
- [ ] Enforcement modes (warning vs suspension vs revoke?)
- [ ] Configuration per policy (different behaviors by access type?)
- [ ] Grace periods (days before real enforcement?)
- [ ] Audit trail (what recorded when expires?)
- [ ] Recovery path (how reactivate expired access?)

**Estimate:** 10 points

---

### 3.3 ER Model Gaps (EP-07)

```sql
-- Missing
CREATE TABLE [compliance].[documents] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [user_id] UNIQUEIDENTIFIER,
 [document_type] VARCHAR(64), -- 'IDENTITY_PROOF', 'SERVICE_AGREEMENT', etc.
 [document_name] VARCHAR(255),
 [storage_uri] VARCHAR(MAX), -- Reference to file storage
 [uploaded_at] DATETIME2,
 [status] VARCHAR(32), -- 'UPLOADED', 'VALIDATING', 'APPROVED', 'REJECTED'
 [valid_until] DATETIME2, -- Revalidation deadline
 [validation_notes] NVARCHAR(MAX)
);

-- Missing
CREATE TABLE [compliance].[document_validators] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [document_id] UNIQUEIDENTIFIER,
 [validator_id] UNIQUEIDENTIFIER,
 [validation_status] VARCHAR(32), -- 'PENDING', 'APPROVED', 'REJECTED'
 [validation_date] DATETIME2,
 [validation_reason] NVARCHAR(MAX),
 [root_tenant_id] UNIQUEIDENTIFIER
);

-- Missing
CREATE TABLE [compliance].[expiration_notification_rules] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [code] VARCHAR(64),
 [name] VARCHAR(255),
 [scope_type] VARCHAR(32), -- 'GLOBAL', 'TENANT', 'ORGANIZATION'
 [target_user_category] VARCHAR(32), -- 'INTERNAL', 'EXTERNAL', 'B2B'
 [days_before_expiration] INT,
 [notification_channels] VARCHAR(MAX), -- JSON: ['EMAIL', 'IN_APP']
 [notify_user] BIT,
 [notify_admin] BIT,
 [notify_approver] BIT,
 [frequency] VARCHAR(32) -- 'ONCE', 'DAILY', 'WEEKLY'
);

-- Missing
CREATE TABLE [compliance].[access_expiration_policies] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [code] VARCHAR(64),
 [name] VARCHAR(255),
 [scope_type] VARCHAR(32), -- 'PROFILE', 'PERMISSION', 'DELEGATION'
 [on_expiration_action] VARCHAR(32), -- 'WARNING', 'SUSPEND', 'REVOKE'
 [grace_period_days] INT,
 [allow_extension] BIT,
 [require_reapproval_on_extend] BIT
);
```

**Impact:** Blocks all of EP-07 (compliance).

---

## 4. EP-08: Advanced IGA (Role Promotion)

### 4.1 Current Status (30% Readiness)

#### Exists

| Component | Location | Details |
|-----------|----------|---------|
| **ADR-0046** | `/architecture/adrs/0046-role-evolution-and-promotion.md` | Role evolution strategy |
| **User Stories** | MVP backlog | US-031, US-032 only |

#### Incomplete

| Component | Problem | Priority |
|-----------|---------|----------|
| **FS-12: Role Promotion Process** | Only 2 US (US-031, US-032); workflow detail missing | HIGH |
| **State Machine** | What states for promotion request? | HIGH |
| **Impact Analysis** | How calculated (new permissions, affected systems)? | MEDIUM |
| **IGA Context Definition** | Bounded context does NOT exist | HIGH |

#### Completely Missing

| Component | Impact |
|-----------|--------|
| **IGA Bounded Context** | Strategic domain missing |
| **Impact Analysis Engine** | How new permissions and risks calculated? |
| **Role Maturity Model** | What does "promote a role" mean? (evolution, lifecycle?) |
| **ER Model for Role Promotion** | Tables for requests, approvals, impact |

---

### 4.2 Functional Stories Detail (EP-08)

#### FS-12: Role Promotion Process (INCOMPLETE)

**User Stories:** US-031, US-032 (only 2 stories for 13-point epic?)

**Gaps:**
- [ ] Definition: "Promotion" = responsibility change? Role evolution?
- [ ] Current state: What is current role?
- [ ] Target state: What role to promote to?
- [ ] Impact analysis: What new permissions? What systems? What risk?
- [ ] Approval process: Who approves? Require documentation?
- [ ] Execution: How promotion applied?
- [ ] Audit trail: What recorded?

**Estimate:** 13 points (2-3 sprints)

---

### 4.3 ER Model Gaps (EP-08)

```sql
-- Missing
CREATE TABLE [iga].[role_promotion_requests] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [requesting_user_id] UNIQUEIDENTIFIER,
 [target_user_id] UNIQUEIDENTIFIER,
 [current_role_id] UNIQUEIDENTIFIER,
 [target_role_id] UNIQUEIDENTIFIER,
 [promotion_reason] NVARCHAR(MAX),
 [request_date] DATETIME2,
 [status] VARCHAR(32), -- 'DRAFT', 'PENDING_REVIEW', 'APPROVED', 'REJECTED', 'EXECUTED'
 [approval_request_id] UNIQUEIDENTIFIER -- Link to Approvals
);

-- Missing
CREATE TABLE [iga].[role_promotion_impact_analysis] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [promotion_request_id] UNIQUEIDENTIFIER,
 [new_permissions_count] INT,
 [affected_systems_count] INT,
 [risk_score] DECIMAL(3,2), -- 0.0 to 1.0
 [conflicting_permissions] VARCHAR(MAX), -- JSON array
 [analysis_timestamp] DATETIME2,
 [analyst_notes] NVARCHAR(MAX)
);

-- Missing
CREATE TABLE [iga].[role_maturity_levels] (
 [id] UNIQUEIDENTIFIER PRIMARY KEY,
 [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
 [role_id] UNIQUEIDENTIFIER,
 [current_maturity_level] VARCHAR(32), -- 'JUNIOR', 'SENIOR', 'LEAD', 'PRINCIPAL'
 [next_maturity_level] VARCHAR(32),
 [days_at_current_level] INT,
 [promotion_eligibility_date] DATETIME2,
 [last_review_date] DATETIME2
);
```

---

## 5. Dependency Matrix (Cross-Epic)

```
EP-06 (Approvals/Security)
 ├── FS-09 (MFA) → Identity Context
 ├── FS-14 (Delegation) → ADR-0044 , Audit Context
 └── FS-10 (B2B Approval) → Approvals Context (new)

EP-07 (Compliance)
 ├── FS-11 (Documents) → Approvals Context (EP-06), Document storage
 ├── FS-15 (Notifications) → Configuration Context
 └── FS-16 (Expiration) → Authorization Context

EP-08 (IGA)
 ├── FS-12 (Role Promotion) → Approvals Context (EP-06), Impact Analysis Engine
 └── → IGA Bounded Context (new)
```

**Conclusion:** EP-06 is prerequisite for EP-07 and EP-08. Sequence: **EP-06 → EP-07 → EP-08**.

---

## 6. Completion Plan by Epic

### EP-06: 5 days estimated

1. **FS-09 Adaptive MFA** (3h)
- Detailed acceptance criteria
- Risk scoring model

2. **FS-14 Delegated Admin** (4h)
- State machine (delegation states)
- Scope validation logic
- Temporal constraints

3. **FS-10 B2B Approval** (2h)
- Document attachment rules
- Approval chain definition

4. **ER Model EP-06** (2h)
- approval_workflows
- approval_requests, approval_approvers
- user_management_delegations

5. **Integration Map** (2h)
- Approvals ↔ Authorization
- Approvals ↔ Audit
- Approvals ↔ Configuration

**Total EP-06:** 13 hours

---

### EP-07: 5 days estimated

1. **FS-11 Document Lifecycle** (3h)
- Document type catalog
- Validation workflow
- Storage integration

2. **FS-15 Notification Rules (CREATE)** (3h)
- Rule model definition
- Trigger conditions
- Notification channels

3. **FS-16 Expiration Enforcement (CREATE)** (3h)
- Enforcement modes
- Grace periods
- Recovery paths

4. **ER Model EP-07** (2h)
- documents, document_validators
- expiration_notification_rules
- access_expiration_policies

5. **Compliance Context Definition** (2h)
- Bounded context (entities, ports, events)
- Integration with other contexts

**Total EP-07:** 13 hours

---

### EP-08: 4 days estimated

1. **FS-12 Role Promotion (EXPAND)** (4h)
- Break into sub-stories (5-6 stories)
- Impact analysis engine design
- Execution flow

2. **ER Model EP-08** (2h)
- role_promotion_requests
- role_promotion_impact_analysis
- role_maturity_levels

3. **IGA Context Definition** (2h)
- Bounded context design
- Port abstractions
- Event contracts

4. **Integration: IGA ↔ Approvals ↔ Authorization** (2h)

**Total EP-08:** 10 hours

---

## 7. Recommendations & Next Steps

### Critical (Blocks construction)

1. **EP-06 ER Model** (approval_workflows, requests, delegations)
- Required for Sprint 2 (after MVP)
- **Effort:** 2h
- **Priority:** Now

2. **EP-07 ER Model** (documents, notifications, policies)
- Required for Sprint 2
- **Effort:** 2h
- **Priority:** Now

3. **EP-06 FS-14 Scope Validation Logic**
- Design document required for correct implementation
- **Effort:** 2h
- **Priority:** This week

### Important (Impacts Sprint planning)

4. **EP-07 FS-15, FS-16 (CREATE)**
- New stories do not exist
- **Effort:** 6h
- **Priority:** This week

5. **EP-08 IGA Context + FS-12 Expansion**
- **Effort:** 4h
- **Priority:** Next week

### Recommended (Improves visibility)

6. **Bounded Context Maps** for Compliance + IGA
- Clear visualization
- **Effort:** 2h
- **Priority:** Next week

---

## 8. Exit Criteria (Post-MVP Readiness = 100%)

- [ ] EP-06: All FS have detailed acceptance criteria; ER Model > 90% complete
- [ ] EP-07: All FS defined (including new FS-15, FS-16); ER Model > 90% complete
- [ ] EP-08: FS-12 expanded to 5-6 stories; IGA Context designed; ER Model > 80% complete
- [ ] Bounded Context Maps (3 new): Approvals, Compliance, IGA
- [ ] Integration Matrix: How all 8 epics communicate
- [ ] ADR Cross-reference complete (all ADRs → contexts → stories)
- [ ] Final Readiness Assessment: % per epic, MVP + Post-MVP consolidated

---

**Next Step:** Proceed with detailed expansion of **EP-06** beginning today.

**Approved by:** [Architect]
**Date:** 2026-05-14
