# Technical Stories & Team Composition — UMS Construction Planning

**Version:** 1.0  
**Date:** 2026-05-14  
**Purpose:** Extract technical requirements as stories from each épica design + propose team profiles for execution  
**Scope:** EP-01 through EP-08 (MVP + Post-MVP)

---

## PART 1: TECHNICAL STORIES BY ÉPICA

### MVP SCOPE (Sprints 1-2: 6-7 weeks)

---

## EP-01: Tenant & Identity — Technical Stories

**Functional Stories:** FS-01, FS-02, FS-03  
**Bounded Context:** Identity  
**Primary Layers:** Domain, Application, Infrastructure (SQL), API

### TS-1.1: Domain Model — Tenant Hierarchy & Closure Table
**Size:** 8 pts | **Skills:** Backend (DDD), Database Architect  
**Description:** Implement Tenant aggregate with closure table pattern (ADR-0048) for hierarchical tenants  
**Acceptance Criteria:**
- Tenant aggregate (id, root_tenant_id, parent_tenant_id, tenant_type_id)
- TenantClosure materialized view for ancestor/descendant queries
- TenantType taxonomy (INTERNAL, PARTNER, CUSTOMER)
- Composite PK (id, root_tenant_id) enforced on domain model
- Value objects: TenantName, TenantCode, TenantStatus
- Domain events: TenantCreatedEvent, TenantHierarchyChangedEvent

**Dependencies:** None (foundational)

---

### TS-1.2: SQL Server Infrastructure — Tenant & Identity Tables (Schema & Indices)
**Size:** 8 pts | **Skills:** DBA (SQL Server), Backend (EF Core)  
**Description:** Create tenant, user, tenant_closure, tenant_types tables with composite keys + indices  
**Acceptance Criteria:**
- Tables: [identity].[tenants], [identity].[users], [identity].[tenant_closure], [identity].[tenant_types]
- All with composite PK (id, root_tenant_id) for multi-tenant partition pruning
- Partition function on root_tenant_id for tenant isolation (SQL Server infrastructure)
- Indices for: (user_id, root_tenant_id), (tenant_id, root_tenant_id), closure queries, soft delete
- Audit columns (10 standard): created_at, created_by, modified_at, modified_by, deleted_at, deleted_by, version, is_deleted, change_reason, audit_user_id
- ⚠️ **NOTE:** RLS enforcement is PRIMARY at Application Layer (TS-1.3, EF Core filters); SQL Server RLS is optional hardening (Phase 2)

**Dependencies:** TS-1.1

---

### TS-1.3: EF Core Global Query Filters — Application-Level Tenant Isolation (PRIMARY)
**Size:** 8 pts | **Skills:** Backend (.NET/EF Core)  
**Description:** Implement EF Core Global Query Filters for automatic TenantId filtering on all queries  
**Acceptance Criteria:**
- ICurrentTenantResolver: scoped service extracting tenant from JWT claims or X-Tenant-ID header
- ModelBuilder configuration: Apply global query filter `q => q.Where(e => e.root_tenant_id == currentTenant.Id)` to all entities
- All entities have mandatory TenantId denormalization (composite key: id, root_tenant_id)
- Filter applied automatically: no developer needs to add WHERE clauses
- Error handling: throw exception if TenantId resolution fails (architectural invariant)
- Unit tests: filter correctness, tenant isolation, soft deletes respect filter
- **Phase 2 (Optional):** SQL Server RLS via SESSION_CONTEXT as hardening failsafe (separate story)

**Dependencies:** TS-1.2

---

### TS-1.4: Identity Ports & Adapters — User Registration & Validation
**Size:** 8 pts | **Skills:** Backend (DDD, Ports/Adapters)  
**Description:** Implement registration flow with email validation port  
**Acceptance Criteria:**
- Port: IEmailService (abstract SendVerificationEmailAsync)
- Adapter: SendgridEmailAdapter (concrete implementation)
- Port: IPasswordHasher (abstract)
- Adapter: BcryptPasswordHasher (concrete, 12 rounds)
- Port: IUserRepository (abstract CRUD + query)
- Adapter: SqlServerUserRepository (EF Core implementation with RLS)
- Application service: RegisterUserCommandHandler
- Domain service: UserRegistrationDomainService (DDD pattern)

**Dependencies:** TS-1.3

---

### TS-1.5: API Endpoints — Tenant Onboarding & User Login
**Size:** 8 pts | **Skills:** Backend (REST API, .NET)  
**Description:** Create REST endpoints for corporate login, self-registration, org onboarding  
**Acceptance Criteria:**
- POST /api/v1/auth/login (corporate: email + password)
- POST /api/v1/auth/register (self-reg: email + name + password)
- POST /api/v1/tenants (onboarding: org name + contact)
- Response models follow DTO pattern
- Input validation with FluentValidation
- Error handling: 400 (bad request), 409 (user exists), 500 (server)
- OpenAPI 3.0 documentation
- Unit tests: happy path + error cases (70% coverage)

**Dependencies:** TS-1.4

---

### TS-1.6: Integration Tests — Two-Layer RLS Validation (Identity Context)
**Size:** 13 pts | **Skills:** QA Automation, Backend (test patterns)  
**Description:** Create integration tests validating RLS isolation across tenants  
**Acceptance Criteria:**
- Test setup: create 3 separate tenants (T1, T2, T3) with users
- Test Layer 1 RLS: User from T1 cannot query T2 data (EF filter)
- Test Layer 2 RLS: Bypass EF Core, direct SQL still blocked by RLS predicate
- Test failover: simulate Layer 1 bypass, Layer 2 catches it
- Test audit trail: all queries logged with user/tenant context
- 100% RLS test coverage (per Critical Success Factor)
- Integration DB: LocalDB or test SQL Server instance

**Dependencies:** TS-1.2, TS-1.3

---

## EP-02: System Catalog — Technical Stories

**Functional Stories:** FS-04  
**Bounded Context:** Console  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-2.1: Domain Model — System & Topology Registry
**Size:** 5 pts | **Skills:** Backend (DDD)  
**Description:** Implement System and SystemTopology aggregates  
**Acceptance Criteria:**
- System aggregate: (id, root_tenant_id, name, type, base_url, status)
- SystemTopology: environment (DEV, TEST, STAGING, PROD), instance count, endpoint details
- Value objects: SystemUrl, SystemType, EnvironmentType, TopologyNode
- Domain events: SystemRegisteredEvent, TopologyUpdatedEvent
- Constraints: System name unique per tenant, URL validation

**Dependencies:** EP-01 (Tenant context)

---

### TS-2.2: SQL Server Infrastructure — System Catalog Tables
**Size:** 8 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [console].[systems], [console].[system_topologies] tables  
**Acceptance Criteria:**
- Tables with composite PK (id, root_tenant_id)
- Foreign key: (system_id, root_tenant_id) → (tenants.id, root_tenant_id)
- Indices: (tenant_id, root_tenant_id), (system_name, root_tenant_id)
- Partitioning aligned with Tenant context
- Audit columns (10 standard)

**Dependencies:** TS-1.2

---

### TS-2.3: Ports & Adapters — System Repository & Topology Service
**Size:** 5 pts | **Skills:** Backend (DDD)  
**Description:** Implement repository and topology analysis service  
**Acceptance Criteria:**
- Port: ISystemRepository (GetByIdAsync, ListByTenantAsync, SaveAsync)
- Adapter: SqlServerSystemRepository (EF Core)
- Port: ITopologyAnalyzer (AnalyzeEnvironmentsAsync, DetectHealthAsync)
- Application service: RegisterSystemCommandHandler
- Domain service: SystemValidationService

**Dependencies:** TS-2.1, TS-2.2

---

### TS-2.4: API Endpoints — System Registration & Query
**Size:** 5 pts | **Skills:** Backend (REST API)  
**Description:** Create endpoints for system registration and catalog queries  
**Acceptance Criteria:**
- POST /api/v1/systems (register system)
- GET /api/v1/systems (list tenant systems)
- GET /api/v1/systems/{systemId} (detail + topology)
- PATCH /api/v1/systems/{systemId}/topology (update topology)
- Input validation, error handling
- OpenAPI documentation

**Dependencies:** TS-2.3

---

### TS-2.5: Integration Tests — System Catalog
**Size:** 8 pts | **Skills:** QA Automation  
**Description:** Integration tests for system registration and isolation  
**Acceptance Criteria:**
- System registered in T1 not visible to T2
- Topology changes reflected in queries
- Validation rules enforced (URL format, system name unique per tenant)
- Audit trail logged

**Dependencies:** TS-2.2

---

## EP-03: Authorization — Technical Stories

**Functional Stories:** FS-05, FS-06, FS-07  
**Bounded Context:** Authorization  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-3.1: Domain Model — XACML-Inspired Policy Model
**Size:** 13 pts | **Skills:** Backend (DDD, Security)  
**Description:** Implement XACML-style aggregates: Policy, Rule, Profile, Permission  
**Acceptance Criteria:**
- Policy aggregate: (id, root_tenant_id, name, description, rules[], status)
- Rule: (effect [ALLOW/DENY], conditions[], actions[], resources[])
- Profile aggregate: (id, root_tenant_id, name, policies[], assigned_users[])
- Permission value object: (action, resource, effect)
- Conditions: attribute-based (user.role, system.name, time.hour, etc.)
- Domain events: PolicyCreatedEvent, ProfileAssignedEvent, PermissionEvaluatedEvent
- ADR-0039 (Authorization Architecture), ADR-0021 (Compilation Model)

**Dependencies:** EP-01

---

### TS-3.2: Policy Decision & Administration (PDP + PAP Implementation)
**Size:** 13 pts | **Skills:** Backend (Security, XACML)  
**Description:** Implement Policy Decision Point (PDP) + Policy Administration Point (PAP) for policy evaluation  
**Acceptance Criteria:**
- **PAP (Policy Admin Point):** Port IAuthorizationPolicyService (CRUD policies/rules/profiles)
- **PDP (Policy Decision Point):** Port IPolicyDecisionPoint, evaluate rules against attributes
  - Rule matching engine (first match wins, or configurable aggregation)
  - Effect aggregation (ALLOW/DENY resolution)
  - Explanation generation (trace: which rule matched, why)
  - Caching layer: IAuthorizationCache (compiled policy graph stored in Redis)
- **Compilation model:** Convert profiles → compiled decision trees (ADR-0021)
- **NOTE:** Attribute Resolution (PIP) is separate story TS-3.2b

**Dependencies:** TS-3.1

---

### TS-3.2b: Policy Information Point (PIP) — Attribute Resolution
**Size:** 13 pts | **Skills:** Backend (Security, Data Access)  
**Description:** Implement Policy Information Point (PIP) for runtime attribute resolution  
**Acceptance Criteria:**
- **PIP (Policy Info Point):** Port IAttributeRepository, resolve user/system/resource attributes at request time
  - User attributes: role, tenure, delegation_scope, geo, device_reputation
  - System attributes: name, environment (DEV/TEST/STAGING/PROD), risk_level
  - Context attributes: time_of_day, ip_address, network, risk_score (from EP-06)
  - Attribute validation: ensure resolved values match policy expectations
  - Caching: attribute resolution results cached with TTL
- Integration with Configuration context (TS-4.3) for configurable attributes
- Integration with IGA context (TS-8.1) for role maturity data

**Dependencies:** TS-3.1, TS-4.3 (for attribute config)

**Dependencies:** TS-3.1

---

### TS-3.3: SQL Server Infrastructure — Authorization Tables
**Size:** 13 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [authorization].[policies], [authorization].[rules], [authorization].[profiles], [authorization].[profile_assignments]  
**Acceptance Criteria:**
- policies: (id, root_tenant_id, name, rules_json, status, ...)
- rules: (id, policy_id, root_tenant_id, effect, conditions_json, actions[], resources[], ...)
- profiles: (id, root_tenant_id, name, status, ...)
- profile_assignments: (id, user_id, profile_id, root_tenant_id, assigned_at, expires_at)
- Composite PK on all tables
- Indices: (tenant_id, root_tenant_id), (user_id, root_tenant_id)
- Partitioning on root_tenant_id
- Audit columns

**Dependencies:** TS-1.2

---

### TS-3.4: Authorization Middleware & Attribute Resolution
**Size:** 8 pts | **Skills:** Backend (.NET/middleware)  
**Description:** Implement ASP.NET Core authorization middleware with attribute resolution  
**Acceptance Criteria:**
- Custom AuthorizationMiddleware in request pipeline
- Resolve current user, tenant, system context from request
- Call IAttributeRepository to gather runtime attributes
- Pass to IPolicyDecisionPoint for evaluation
- Short-circuit on DENY, continue on ALLOW
- Logging: decision details, evaluation time, cache hit/miss

**Dependencies:** TS-3.2

---

### TS-3.5: API Endpoints — Policy Management & Authorization Check
**Size:** 8 pts | **Skills:** Backend (REST API)  
**Description:** Create admin endpoints for policy/profile management + check endpoint  
**Acceptance Criteria:**
- POST /api/v1/authorization/policies (create policy)
- GET /api/v1/authorization/policies (list)
- PATCH /api/v1/authorization/policies/{policyId} (update)
- DELETE /api/v1/authorization/policies/{policyId}
- POST /api/v1/authorization/profiles (create profile)
- POST /api/v1/authorization/profiles/{profileId}/assign (assign to user)
- POST /api/v1/authorization/check (test: {user, system, action, resource})
- Response includes decision + explanation

**Dependencies:** TS-3.2

---

### TS-3.6: Unit Tests — Authorization Decision Engine
**Size:** 13 pts | **Skills:** Backend (unit test, algorithms)  
**Description:** Comprehensive unit tests for PDP decision engine  
**Acceptance Criteria:**
- Test 20+ scenarios: simple ALLOW, DENY, AND conditions, OR conditions, attribute matching, wildcards, time-based (hour of day), effect aggregation
- Test edge cases: missing attributes, malformed rules, circular logic
- Test performance: 1000 rules evaluated in <100ms
- 80% code coverage on PDP logic
- Mocked IPolicyRepository, IAttributeRepository

**Dependencies:** TS-3.2

---

### TS-3.7: Integration Tests — Authorization Workflow
**Size:** 13 pts | **Skills:** QA Automation  
**Description:** Integration tests for full authorization flow  
**Acceptance Criteria:**
- Create policy (ALLOW actions [read, write] on resource [System.A] where user.role == "Admin")
- Create profile, assign to user
- Test user can perform allowed action
- Test user cannot perform denied action
- Test different attributes (role, system) affect outcome
- 100% Happy path + error cases

**Dependencies:** TS-3.3

---

## EP-04: Configuration — Technical Stories

**Functional Stories:** FS-13  
**Bounded Context:** Configuration  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-4.1: Domain Model — Hierarchical Configuration
**Size:** 8 pts | **Skills:** Backend (DDD)  
**Description:** Implement ConfigurationParameter aggregate with hierarchy (tenant → system → environment → parameter)  
**Acceptance Criteria:**
- ConfigurationParameter: (id, root_tenant_id, key, value, parameter_type [STRING/INT/BOOL/JSON], scope [TENANT/SYSTEM/ENVIRONMENT])
- Scope hierarchy: TENANT → SYSTEM → ENVIRONMENT (resolution order)
- Value objects: ParameterKey, ParameterValue, ParameterType
- Domain events: ConfigurationChangedEvent, ConfigurationResolvedEvent
- Validation: key format, value type coercion, scope constraints

**Dependencies:** EP-01 (Tenant), EP-02 (System)

---

### TS-4.2: SQL Server Infrastructure — Configuration Tables
**Size:** 5 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [configuration].[parameters], [configuration].[parameter_history]  
**Acceptance Criteria:**
- parameters: (id, root_tenant_id, system_id, environment, key, value, type, scope, ...)
- parameter_history: audit trail of all changes
- Composite PK, indices on (key, scope, root_tenant_id)
- Partitioning on root_tenant_id

**Dependencies:** TS-1.2

---

### TS-4.3: Hierarchical Configuration Resolution Service
**Size:** 8 pts | **Skills:** Backend (DDD, caching, encryption)  
**Description:** Implement 4-level hierarchical resolution with inheritance control + encryption (ADR-0047)  
**Acceptance Criteria:**
- Port: IConfigurationService (ResolveAsync(key, tenant, system, environment, moduleId))
- Adapter: SqlServerConfigurationService
- **4-level resolution hierarchy:** Module → Suite/System → Tenant → Global
  - Resolution strategy: "Closest Scope Wins"
  - Each level can be NULL (traversal skips NULL scopes)
- **Inheritance control:** IsInheritable flag
  - If IsInheritable=false at parent level, child scopes cannot override
  - Prevents bypass of platform-wide compliance mandates
- **Encryption handling:** IsEncrypted flag
  - Sensitive values (API keys, secrets) stored encrypted
  - Decrypt on retrieval (interact with Vault/Secrets store)
  - Transparent to callers (return plaintext)
- **Caching:** Redis (TTL: 5 minutes)
  - Cache key: `config:v2:{tenant}:{system}:{module}:{key}`
  - Cache invalidation: on parameter mutation (cascade to dependents)
- **Feature flags:** Support boolean + JSON complex structures

**Dependencies:** TS-4.1, TS-4.2

---

### TS-4.4: API Endpoints — Configuration Management
**Size:** 5 pts | **Skills:** Backend (REST API)  
**Description:** CRUD endpoints for configuration parameters  
**Acceptance Criteria:**
- POST /api/v1/configuration/parameters
- GET /api/v1/configuration/parameters (filtered by scope)
- PATCH /api/v1/configuration/parameters/{parameterId}
- DELETE /api/v1/configuration/parameters/{parameterId}
- GET /api/v1/configuration/resolve (resolution endpoint)

**Dependencies:** TS-4.3

---

### TS-4.5: Integration Tests — Configuration Hierarchy
**Size:** 8 pts | **Skills:** QA Automation  
**Description:** Integration tests for resolution hierarchy and caching  
**Acceptance Criteria:**
- Set parameter at TENANT level, override at SYSTEM level
- Resolve parameter: confirms system value used (not tenant)
- Cache verification: subsequent resolve hits cache
- Invalidation: update parameter, cache clears, next resolve fresh
- Cross-tenant isolation: T1 parameters not visible to T2

**Dependencies:** TS-4.2

---

## EP-05: Experience & Diagnostics — Technical Stories

**Functional Stories:** FS-08  
**Bounded Context:** Console  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-5.1: Hosted Login Page — React (Vite) Implementation
**Size:** 13 pts | **Skills:** Frontend (React/TypeScript), API Integration  
**Description:** Build branded, tenant-aware login page using React + Vite + Zustand  
**Acceptance Criteria:**
- React component: `apps/web/src/pages/Auth/Login.tsx` (brand colors from config)
- Form: email + password inputs (controlled components with Zustand store)
- Client-side validation: Zod or similar (pre-submission)
- Server-side validation: FluentValidation (API endpoint)
- Error messages: invalid credentials, account locked, email not verified, network errors
- Remember me checkbox (JWT token stored securely in httpOnly cookie)
- Responsive design: desktop + mobile (Tailwind CSS)
- Accessibility: WCAG 2.1 AA (form labels, contrast, keyboard nav, aria-labels)
- Password reset link (routes to TS-5.X reset flow)
- Tenant context: resolved from URL subdomain or query param, injected into API calls
- Integration: calls POST /api/v1/auth/login endpoint (TS-1.5)
- Error handling: network timeouts, 401/403 responses, form submission state

**Dependencies:** EP-01

---

### TS-5.2: Diagnostics Dashboard — React Admin Interface
**Size:** 13 pts | **Skills:** Frontend (React/TypeScript), API Integration, Data Visualization  
**Description:** Create admin dashboard showing system health + diagnostics  
**Acceptance Criteria:**
- React component: `apps/web/src/pages/Admin/Diagnostics.tsx`
- Dashboard widgets (using Recharts or similar for charts):
  - Tenant count, user count, active sessions (real-time)
  - EF Core filter health (query filter status)
  - Database connection pool stats (via health endpoint)
  - Recent audit events (last 100, paginated)
  - Authorization cache hit rate (Redis if used)
  - API response time metrics (p50, p99)
- Real-time updates: TanStack Query polling (5s interval, optional WebSocket)
- Charts: response time trends over 24h, authorization decision latency
- Admin-only access: guarded by authorization check (401/403 handling)
- Data sources: GET /api/v1/health, GET /api/v1/audit/logs, GET /api/v1/metrics (admin only)
- Performance: dashboard load <1s, metrics update <2s

**Dependencies:** TS-1.6, TS-3.6, TS-4.5

---

### TS-5.3: Audit Log Viewer Endpoint
**Size:** 8 pts | **Skills:** Backend (REST API)  
**Description:** Create queryable audit log API endpoint  
**Acceptance Criteria:**
- GET /api/v1/audit/logs (paginated, filterable)
- Filters: user_id, event_type, resource, date_range
- Response: audit event + details (before/after for changes)
- 100% audit completeness (per Critical Success Factor)
- Searchable (ElasticSearch optional, but SQL fulltext OK for MVP)

**Dependencies:** Audit Context (cross-cutting)

---

### TS-5.4: System Health Check Endpoint
**Size:** 5 pts | **Skills:** Backend (.NET)  
**Description:** Implement /health endpoint for monitoring + liveness probe  
**Acceptance Criteria:**
- GET /health (liveness)
- GET /health/ready (readiness)
- Checks: DB connection, RLS, cache (if used), external services
- Response: UP/DOWN status + dependencies
- Kubernetes-compatible (status codes: 200 UP, 503 DOWN)

**Dependencies:** All infrastructure stories

---

### TS-5.5: Integration Tests — Login & Diagnostics Flow
**Size:** 8 pts | **Skills:** QA Automation  
**Description:** Integration tests for login page and diagnostics  
**Acceptance Criteria:**
- Test login with valid/invalid credentials
- Test diagnostics dashboard loads with correct metrics
- Test audit log querying
- Test health check endpoint
- Performance: login <500ms, diagnostics load <1s

**Dependencies:** TS-5.1, TS-5.2

---

---

## POST-MVP SCOPE (Sprints 3-5: 8-10 weeks)

---

## EP-06: Security, External Access & Delegation — Technical Stories

**Functional Stories:** FS-09 (Adaptive MFA), FS-10 (B2B External Access), FS-14 (Delegated Administration)  
**Bounded Context:** Approvals (shared)  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-6.1: Domain Model — Adaptive MFA & Risk Scoring
**Size:** 13 pts | **Skills:** Backend (DDD, security algorithms)  
**Description:** Implement MFAChallenge, RiskScore, RiskDecision aggregates  
**Acceptance Criteria:**
- RiskScore aggregate: 6 weighted factors (frequency anomaly, geographic, device rep, network, failed attempts, tenant risk)
- RiskDecision: 4 levels (LOW: no MFA, MEDIUM: OTP, HIGH: MFA + biometric, CRITICAL: block + alert)
- MFAChallenge: track passwordless methods (FIDO2, Magic Link, App Notification)
- Value objects: AnomalyScore, DeviceReputation, GeographicRisk
- Thresholds configurable per tenant (stored in Configuration context)
- Domain events: RiskScoreCalculatedEvent, MFAChallengeIssuedEvent

**Dependencies:** EP-01, EP-04 (Configuration)

---

### TS-6.2: Risk Scoring Engine — Real-Time Calculation
**Size:** 21 pts | **Skills:** Backend (algorithms, analytics)  
**Description:** Implement real-time risk scoring during login  
**Acceptance Criteria:**
- Factor 1: Frequency Anomaly (is login rate abnormal for user? machine learning baseline)
- Factor 2: Geographic Anomaly (country different from last 5 logins? time zone jump impossible?)
- Factor 3: Device Reputation (device seen before? OS/browser matched historical?)
- Factor 4: Network Anomaly (IP in known bad list? ASN country mismatch?)
- Factor 5: Failed Attempts (how many failed in last 24h?)
- Factor 6: Tenant Risk Level (org-wide risk)
- Weighted sum → 0-100 risk score
- Decision thresholds: 0-25 LOW, 26-50 MEDIUM, 51-75 HIGH, 76-100 CRITICAL
- Machine learning: baseline user login patterns (optional: use simple stats for MVP)
- External calls: GeoIP API, IP reputation API (with fallback)
- Caching: device reputation (Redis, TTL 24h)

**Dependencies:** TS-6.1

---

### TS-6.3: Passwordless Methods Implementation
**Size:** 21 pts | **Skills:** Backend (security, external integrations)  
**Description:** Implement 3 passwordless authentication methods  
**Acceptance Criteria:**
- **FIDO2 (WebAuthn):**
  - Credential registration flow (Yubico WebAuthn lib)
  - Challenge-response authentication
  - Backup code generation (10 codes, store hashed)
  - Loss of device: account recovery flow

- **Magic Link (Email):**
  - Generate short-lived token (15 min TTL)
  - Send via email (Sendgrid)
  - One-click link validation
  - Used links cannot be reused

- **App Notification (Push):**
  - Register device/push token (Apple/Google FCM)
  - Send push notification on login attempt
  - User approves/denies in app
  - Timeout: if no response in 5 min, ask for password fallback

**Dependencies:** TS-6.2

---

### TS-6.4: SQL Server Infrastructure — MFA & Passwordless Tables
**Size:** 8 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [approvals].[mfa_challenges], [approvals].[passwordless_credentials], [approvals].[user_devices]  
**Acceptance Criteria:**
- mfa_challenges: (id, root_tenant_id, user_id, challenge_type, risk_score, decision_level, issued_at, expires_at, verified_at)
- passwordless_credentials: (id, user_id, root_tenant_id, type [FIDO2/MAGIC_LINK/PUSH], credential_data, is_primary, registered_at)
- user_devices: (id, user_id, root_tenant_id, device_fingerprint, push_token, os, browser, last_seen)
- backup_codes: (id, credential_id, code_hash, used_at) — for FIDO2 recovery
- Composite PK, indices on (user_id, root_tenant_id)

**Dependencies:** TS-1.2

---

### TS-6.5: Approvals Domain Model — Approval Workflows
**Size:** 13 pts | **Skills:** Backend (DDD, state machines)  
**Description:** Implement Approval, ApprovalRequest, ApprovalWorkflow aggregates  
**Acceptance Criteria:**
- ApprovalWorkflow: (name, approval_rules[], serial/parallel/quorum)
- ApprovalRequest: (id, root_tenant_id, workflow_id, requester_id, linked_entity [B2B access, delegation], status)
- Approval: (id, request_id, approver_id, decision [APPROVED/REJECTED/PENDING], decision_at, reason)
- State machine: DRAFT → PENDING_APPROVAL → APPROVED/REJECTED → EXECUTED
- Quorum rule: N of M approvers must approve
- Serial approval: A approves → B approves (sequence)
- Parallel approval: A and B can approve independently
- Domain events: ApprovalRequestedEvent, ApprovalGivenEvent, ApprovalCompletedEvent

**Dependencies:** EP-03 (Authorization context knowledge)

---

### TS-6.6: SQL Server Infrastructure — Approvals Tables
**Size:** 8 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [approvals].[approval_workflows], [approvals].[approval_requests], [approvals].[approvals]  
**Acceptance Criteria:**
- approval_workflows: (id, root_tenant_id, name, rules_json, status, ...)
- approval_requests: (id, root_tenant_id, workflow_id, requester_id, linked_entity_id, linked_entity_type, status, ...)
- approvals: (id, request_id, root_tenant_id, approver_id, decision, reason, decided_at)
- approval_attachments: (id, request_id, root_tenant_id, file_key, mime_type, size, uploaded_at)
- Composite PK, indices on (request_id, root_tenant_id), (approver_id, root_tenant_id)

**Dependencies:** TS-1.2

---

### TS-6.7: B2B External Access Approval Flow
**Size:** 13 pts | **Skills:** Backend (DDD, workflows)  
**Description:** Implement B2B external user approval + access grant  
**Acceptance Criteria:**
- ExternalAccessRequest aggregate: user_email, org_name, requested_systems[], reason, attachments
- Approval workflow: Requester → Security Reviewer → Manager (serial)
- Acceptance criteria scenarios:
  1. Approve: external user registered, access granted to requested systems
  2. Partial approve: access to subset of systems
  3. Request info: pause, ask requester for details
  4. Reject: external user cannot register
- Document attachment: store request documents (compliance, NDA, etc.)
- Audit: all decisions logged with who/when/why

**Dependencies:** TS-6.5, TS-6.6

---

### TS-6.8: Delegated Administration — State Machine & Scopes
**Size:** 21 pts | **Skills:** Backend (DDD, state machines)  
**Description:** Implement delegation with scope constraints and temporal expiration  
**Acceptance Criteria:**
- DelegationRequest aggregate: delegator → delegate, scope, duration, constraints
- Scope model: 5 types:
  1. SYSTEM: can admin specific systems only
  2. ORGANIZATION: can admin own org only
  3. ROLE: can only delegate users to specific role
  4. OPERATION: can only perform specific actions (read-only, write, delete)
  5. TEMPORARY: valid for duration + auto-expires
- State machine (8 states):
  - DRAFT → PENDING_APPROVAL → ACTIVE → EXTENDED → EXPIRING → EXPIRED/REVOKED → ARCHIVED
- Principle of Least Privilege: enforce scope constraints on delegated actions
- Temporal: start_date, end_date, auto-revoke on expiration
- Audit: all delegated actions logged with delegator credit
- Scenarios: delegate read-only, delegate with approval threshold, delegate for 30 days

**Dependencies:** TS-6.5, TS-6.6

---

### TS-6.9: Delegation Enforcement Middleware
**Size:** 8 pts | **Skills:** Backend (middleware, authorization)  
**Description:** Middleware to enforce delegated admin constraints  
**Acceptance Criteria:**
- Intercept admin API calls
- Check if caller is delegated admin
- Validate action within scope (system, role, operation)
- Validate temporal constraints (not expired)
- Audit delegated action with delegator credit
- Block out-of-scope actions with 403 Forbidden

**Dependencies:** TS-6.8

---

### TS-6.10: API Endpoints — MFA, B2B Access, Delegation
**Size:** 13 pts | **Skills:** Backend (REST API)  
**Description:** Create endpoints for MFA management, B2B access requests, delegation  
**Acceptance Criteria:**
- MFA:
  - POST /api/v1/auth/mfa/challenges (initiate challenge)
  - POST /api/v1/auth/mfa/verify (verify response)
  - POST /api/v1/auth/passwordless/register (register FIDO2/magic link/push)
  
- B2B External Access:
  - POST /api/v1/external-access/request (request access)
  - GET /api/v1/external-access/requests (list pending, admin)
  - POST /api/v1/external-access/requests/{id}/approve
  - POST /api/v1/external-access/requests/{id}/reject

- Delegation:
  - POST /api/v1/delegation/request
  - GET /api/v1/delegation/my-delegations
  - POST /api/v1/delegation/{id}/activate
  - POST /api/v1/delegation/{id}/revoke
  - PATCH /api/v1/delegation/{id}/extend

**Dependencies:** TS-6.2, TS-6.7, TS-6.8

---

### TS-6.11: Integration Tests — MFA & Passwordless Flow
**Size:** 13 pts | **Skills:** QA Automation  
**Description:** Integration tests for MFA risk scoring and passwordless auth  
**Acceptance Criteria:**
- Test login with LOW risk: no MFA prompted
- Test login with HIGH risk: MFA prompted
- Test FIDO2 registration and authentication
- Test Magic Link flow (email received, link validated)
- Test app push notification (mock push service)
- Test backup codes for FIDO2 device loss
- 100% passwordless scenario coverage

**Dependencies:** TS-6.4, TS-6.3

---

### TS-6.12: Integration Tests — B2B Access & Delegation
**Size:** 13 pts | **Skills:** QA Automation  
**Description:** Integration tests for approval workflows and delegation  
**Acceptance Criteria:**
- B2B Access:
  - Request access, submit for approval
  - Approver receives, approves, external user granted access
  - Verify external user can access requested systems
  
- Delegation:
  - Delegate read-only scope on System A
  - Delegate tries to write: blocked (403)
  - Delegate tries to access System B: blocked (403)
  - Delegation expires: verify revoked

**Dependencies:** TS-6.7, TS-6.8

---

## EP-07: Compliance Lifecycle — Technical Stories

**Functional Stories:** FS-11 (Document Upload), FS-15 (Expiration Notifications), FS-16 (Access Behavior on Expiration)  
**Bounded Context:** Compliance  
**Primary Layers:** Domain, Application, Infrastructure, API, Background Services

### TS-7.1: Domain Model — Compliance & Document Lifecycle
**Size:** 8 pts | **Skills:** Backend (DDD)  
**Description:** Implement Compliance, Document, DocumentValidator aggregates  
**Acceptance Criteria:**
- Compliance aggregate: user_id, document_list, expiration_policy, notification_rules
- Document: (id, type [ID, PASSPORT, CERT, LICENSE, TRAINING, etc.], file_key, expiration_date, validator_id, status)
- DocumentValidator: automated or manual validation (signature verification, expiration check, issuer verification)
- DocumentType taxonomy: 15+ types (ID/passport, professional certificates, training certs, licenses, insurance, background checks, etc.)
- Value objects: DocumentType, ExpirationDate, ValidationStatus
- Domain events: DocumentUploadedEvent, DocumentValidatedEvent, DocumentExpiringEvent, DocumentExpiredEvent

**Dependencies:** EP-01 (User context)

---

### TS-7.2: Document Upload & Validation Service
**Size:** 13 pts | **Skills:** Backend (file handling, security)  
**Description:** Implement secure document upload with type validation  
**Acceptance Criteria:**
- Port: IDocumentStorageService (abstract upload/download/delete)
- Adapter: S3DocumentStorageAdapter (AWS S3 with encryption at rest + in transit)
- Virus scanning: ClamAV or VirusTotal API on upload
- File type validation: whitelist PDF, JPG, PNG; reject executables
- Max file size: 10 MB
- Secure URLs: pre-signed S3 URLs with 1-hour expiration
- Audit: all uploads logged with user/document type/timestamp
- Encryption: files encrypted with tenant-specific key (stored in Key Vault)

**Dependencies:** TS-7.1

---

### TS-7.3: Expiration Notification Rules Engine
**Size:** 13 pts | **Skills:** Backend (background services, schedulers)  
**Description:** Implement background service for expiration notifications  
**Acceptance Criteria:**
- ExpirationNotificationEngine: runs hourly (Quartz scheduler or Hangfire)
- Rule model: trigger on N days before expiration (7, 30, 60, 90 days)
- Notification channels: EMAIL, IN_APP, SMS, WEBHOOK, SLACK
- Frequency options: ONCE, DAILY, WEEKLY, ON_LOGIN
- Example rule: "30 days before ID expiration, send EMAIL daily until 1 day before"
- Smart filtering: don't spam (check if already notified)
- Retry logic: if notification fails, retry with exponential backoff (Polly)
- Audit: notification attempts logged (sent_at, channel, status, retry_count)
- Configuration per tenant (stored in Configuration context)

**Dependencies:** TS-7.1, EP-04 (Configuration)

---

### TS-7.4: SQL Server Infrastructure — Compliance Tables
**Size:** 8 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [compliance].[documents], [compliance].[document_validators], [compliance].[expiration_notification_rules]  
**Acceptance Criteria:**
- documents: (id, user_id, root_tenant_id, document_type, file_key, file_size, mime_type, expiration_date, validator_id, validation_status, uploaded_at, expires_at)
- document_validators: (id, root_tenant_id, document_type, validation_logic_json, is_automated, required_approvers)
- expiration_notification_rules: (id, root_tenant_id, document_type, days_before_expiration, notification_channels_json, frequency, enabled_at, last_triggered_at)
- Composite PK, indices on (user_id, root_tenant_id), (expiration_date, root_tenant_id)
- Partitioning on root_tenant_id

**Dependencies:** TS-1.2

---

### TS-7.5: Access Expiration Enforcement Engine
**Size:** 13 pts | **Skills:** Backend (background services, policy enforcement)  
**Description:** Implement enforcement of access on document expiration  
**Acceptance Criteria:**
- AccessExpirationEnforcementEngine: runs every 6 hours
- 3 enforcement modes (tenant-configurable):
  1. WARNING: show banner on login "doc expired", access allowed
  2. SUSPEND: block access until document renewed
  3. REVOKE: permanently revoke access (must appeal to admin)
- Grace period: configurable delay before enforcement applies (0-30 days)
- Extension request: user can request extension (up to N days, requires optional reapproval)
- Appeal process: if REVOKE, user can submit appeal to admin
- Audit: all enforcement actions logged (user_id, action, reason, who_enforced)
- State transitions: ACTIVE → EXPIRING (grace period) → SUSPENDED/REVOKED

**Dependencies:** TS-7.1, TS-7.4

---

### TS-7.6: API Endpoints — Document & Compliance Management
**Size:** 8 pts | **Skills:** Backend (REST API, file handling)  
**Description:** Create endpoints for document upload, expiration rules, enforcement  
**Acceptance Criteria:**
- POST /api/v1/compliance/documents (upload with type, expiration)
- GET /api/v1/compliance/documents (list user docs)
- DELETE /api/v1/compliance/documents/{docId}
- POST /api/v1/compliance/documents/{docId}/download (pre-signed URL)
- POST /api/v1/compliance/extension-request (request extension)
- GET /api/v1/compliance/expiration-status (current status: active, expiring, suspended)
- Admin:
  - POST /api/v1/compliance/notification-rules (create rule)
  - POST /api/v1/compliance/enforcement-policies (configure WARN/SUSPEND/REVOKE)

**Dependencies:** TS-7.2, TS-7.5

---

### TS-7.7: Integration Tests — Document Upload & Expiration
**Size:** 13 pts | **Skills:** QA Automation  
**Description:** Integration tests for compliance lifecycle  
**Acceptance Criteria:**
- Upload document with future expiration
- Verify document stored securely, audit logged
- Verify expiration notification triggered (mock Quartz trigger)
- Test 3 enforcement modes: warning → suspend → revoke
- Test extension request and optional reapproval workflow
- Test grace period: access allowed before enforcement applies
- Test cross-tenant isolation

**Dependencies:** TS-7.4, TS-7.5

---

## EP-08: Advanced IGA (Role Promotion) — Technical Stories

**Functional Stories:** FS-12 (Role Promotion & Maturity)  
**Bounded Context:** IGA  
**Primary Layers:** Domain, Application, Infrastructure, API

### TS-8.1: Domain Model — Role Maturity & Promotion
**Size:** 13 pts | **Skills:** Backend (DDD)  
**Description:** Implement RoleMaturityStatus, PromotionRequest, PromotionImpactAnalysis aggregates  
**Acceptance Criteria:**
- RoleMaturityStatus: (user_id, role_id, current_level [JUNIOR/INTERMEDIATE/SENIOR/LEAD/PRINCIPAL], eligible_next_level, timeline [assigned_at, current_level_since, eligible_for_promotion_at], compliance [certifications, trainings, performance_score, has_no_compliance_issues], blocking_factor)
- PromotionRequest: (id, user_id, current_role, target_role, requested_at, requested_by, manager_id, security_id, status [DRAFT→PENDING→APPROVED→EXECUTED→VERIFIED])
- PromotionImpactAnalysis: (permissions added/removed, conflicting, affected_systems, risk_score 0-100, risk_factors)
- Value objects: MaturityLevel, RolePromotionStatus, RiskScore
- Domain events: PromotionRequestedEvent, PromotionEligibilityCalculatedEvent, PromotionApprovedEvent, PromotionExecutedEvent

**Dependencies:** EP-01, EP-03 (Authorization)

---

### TS-8.2: Eligibility Checker & Background Promotion Watcher
**Size:** 13 pts | **Skills:** Backend (algorithms, background services)  
**Description:** Implement 4 eligibility criteria checkers + background job for promotion eligibility (ADR-0046)  
**Acceptance Criteria:**
- **Eligibility criteria (4 independent checkers):** (ADR-0046 Flag-Driven approach)
  1. FlagSeniority: Minimum days in current role
     - JUNIOR → INTERMEDIATE: 6 months
     - INTERMEDIATE → SENIOR: 18 months
     - SENIOR → LEAD: 3 years
     - LEAD → PRINCIPAL: 5 years
  2. FlagCompliance: All mandatory documents/certifications valid (integrate with EP-07 document system)
     - INTERMEDIATE requires 2 certs
     - SENIOR requires 3 certs + no compliance issues
     - LEAD requires 3 certs + no issues in last 2 years
  3. FlagBusinessScore: Performance rating meets threshold
     - INTERMEDIATE: score ≥3.5
     - SENIOR: score ≥4.0
  4. FlagManualApproval: Human intervention flag (optional override)
- **Background Promotion Watcher** (Quartz scheduler, runs daily)
  - Scan users against active criteria flags
  - When all flags met: emit PromotionEligibilityCalculatedEvent
  - Update RoleMaturityStatus.eligible_for_promotion_at
  - Track which flags are blocking (blocking_factor field)
- **State transitions:** CRITERIA_NOT_MET → CRITERIA_MET (fires event, triggers notification)
- **Tenant-configurable:** Thresholds stored in Configuration context (TS-4.3)
  - Enable/disable individual flags per tenant
  - Adjust tenure requirements, score thresholds

**Dependencies:** TS-8.1, TS-4.3 (for configurable thresholds), EP-07 (for document compliance)

---

### TS-8.3: Promotion Impact Analysis Engine
**Size:** 21 pts | **Skills:** Backend (algorithms, authorization knowledge)  
**Description:** Analyze permission & system impact of promotion  
**Acceptance Criteria:**
- Get current user permissions from Authorization context
- Get target role permissions
- Calculate delta: added permissions, removed permissions
- Detect conflicts: (CREATE + DELETE) on same resource risky, wildcard permissions risky
- Identify affected systems (which systems will see permission changes)
- Calculate risk score (0-100):
  - +20 per conflicting permission
  - +10 per critical system affected
  - +5 per sensitive permission added (delete, modify)
  - +5 if high permission explosion (>10 new permissions)
- Risk factors list: ["Conflicting permissions", "Critical system access", "Sensitive operations granted"]
- Suggest mitigations: ["Require security review", "Implement time-limited grant", "Require MFA for new systems"]

**Dependencies:** TS-8.1, EP-03 (Authorization)

---

### TS-8.4: Promotion State Machine & Workflow
**Size:** 8 pts | **Skills:** Backend (state machines)  
**Description:** Implement promotion approval workflow with state transitions  
**Acceptance Criteria:**
- States: DRAFT → PENDING_MANAGER_APPROVAL → PENDING_SECURITY_REVIEW → APPROVED_READY_TO_EXECUTE → EXECUTED → VERIFIED
- Transitions:
  - Manager rejects: → REJECTED
  - Low risk (<25): skip security review → APPROVED_READY_TO_EXECUTE
  - High risk (≥25): mandatory security review → PENDING_SECURITY_APPROVAL
  - Security approves/rejects: → APPROVED or REJECTED
- Timeouts: pending manager 5 days, pending security 3 days (auto-escalate)
- Domain events: status change → event raised → audit logged

**Dependencies:** TS-8.1

---

### TS-8.5: SQL Server Infrastructure — IGA Tables
**Size:** 8 pts | **Skills:** DBA, Backend (EF Core)  
**Description:** Create [iga].[role_maturity_levels], [iga].[promotion_requests], [iga].[promotion_impact_analysis]  
**Acceptance Criteria:**
- role_maturity_levels: (id, user_id, role_id, root_tenant_id, current_level, next_level, assigned_at, current_level_since, eligible_for_promotion_at, certifications_count, trainings_count, performance_score, blocking_factor, last_reviewed_at)
- promotion_requests: (id, user_id, current_role_id, target_role_id, root_tenant_id, requested_at, manager_id, manager_approval_status, security_approval_status, status, executed_at, verified_at)
- promotion_impact_analysis: (id, promotion_request_id, root_tenant_id, risk_score, risk_level, new_permissions_count, removed_permissions_count, affected_systems_count, conflicting_permissions_json, risk_factors_json, analyzed_at)
- Composite PK, indices on (user_id, root_tenant_id), (status, root_tenant_id)

**Dependencies:** TS-1.2

---

### TS-8.6: Promotion Workflow Integration
**Size:** 13 pts | **Skills:** Backend (DDD, workflows)  
**Description:** Wire approval workflow + permission grant on execution  
**Acceptance Criteria:**
- PromotionRequestCreatedEvent → create ApprovalRequest in Approvals context (if risk HIGH)
- ApprovalGivenEvent (approved) → transition promotion to APPROVED_READY_TO_EXECUTE
- On EXECUTE: call Authorization context to revoke old role, grant new role
- Audit: all transitions logged with who/when/why
- On VERIFIED: ensure permissions correctly applied

**Dependencies:** TS-8.4, TS-6.5

---

### TS-8.7: Promotion Eligibility Notification Engine
**Size:** 5 pts | **Skills:** Backend (background services)  
**Description:** Notify users when promotion eligible (reuse notification pattern from EP-07 if available)  
**Acceptance Criteria:**
- Background job (daily): check role_maturity_levels for eligible_for_promotion_at ≤ today
- Send IN_APP notification: "You're eligible for promotion to SENIOR"
- Track: user_acknowledged_at when user opens notification
- **Pattern reuse:** Leverage TS-7.3 NotificationEngine if available
  - If TS-7.3 complete: +2 pts (IGA-specific logic only)
  - If TS-7.3 not started: +3 pts (duplicate base notification engine)
- Tenant-configurable: enable/disable per org
- **Recommended:** Schedule TS-7.3 (Compliance Notifications) before Sprint 3 TS-8.7

**Dependencies:** TS-8.2, TS-7.3 (preferred) or duplicate notification engine

---

### TS-8.8: API Endpoints — Promotion Management
**Size:** 8 pts | **Skills:** Backend (REST API)  
**Description:** Create endpoints for promotion request, review, approval, execution  
**Acceptance Criteria:**
- POST /api/v1/iga/promotion/request (user initiates)
- GET /api/v1/iga/promotion/my-maturity (current maturity level)
- GET /api/v1/iga/promotion/eligible-next (when eligible)
- Admin:
  - GET /api/v1/iga/promotion/pending (list pending requests)
  - GET /api/v1/iga/promotion/{id}/impact (impact analysis)
  - POST /api/v1/iga/promotion/{id}/execute (execute approved)
  - POST /api/v1/iga/promotion/{id}/verify (verify completed)

**Dependencies:** TS-8.4

---

### TS-8.9: Integration Tests — Role Promotion Flow
**Size:** 13 pts | **Skills:** QA Automation  
**Description:** Integration tests for complete promotion lifecycle  
**Acceptance Criteria:**
- Create user in JUNIOR role, assign to INTERMEDIATE after 6 months
- Verify eligible for promotion notification sent
- User requests promotion to INTERMEDIATE
- Manager reviews request (impact analysis shown)
- Low risk: auto-approved, executed, permissions changed
- High risk: security review triggered, approval workflow started
- Verify user new permissions applied in Authorization context
- Test blocking scenarios: missing certifications, low performance, compliance issues

**Dependencies:** TS-8.5, TS-8.3

---

---

## PART 2: TECHNICAL STORIES SUMMARY TABLE

| Épica | Story | Size | Skills | Dependencies |
|-------|-------|------|--------|--------------|
| **EP-01** | TS-1.1: Tenant Hierarchy Domain | 8 | Backend (DDD) | — |
| | TS-1.2: RLS + Partition SQL | 13 | DBA, Backend | TS-1.1 |
| | TS-1.3: DbConnectionInterceptor | 5 | Backend | TS-1.2 |
| | TS-1.4: Registration Ports/Adapters | 8 | Backend | TS-1.3 |
| | TS-1.5: Auth API Endpoints | 8 | Backend | TS-1.4 |
| | TS-1.6: RLS Integration Tests | 13 | QA | TS-1.2, TS-1.3 |
| **EP-02** | TS-2.1: System Catalog Domain | 5 | Backend | EP-01 |
| | TS-2.2: Catalog SQL Tables | 8 | DBA, Backend | TS-1.2 |
| | TS-2.3: Repository & Service | 5 | Backend | TS-2.1, TS-2.2 |
| | TS-2.4: System API Endpoints | 5 | Backend | TS-2.3 |
| | TS-2.5: Catalog Integration Tests | 8 | QA | TS-2.2 |
| **EP-03** | TS-3.1: XACML Domain Model | 13 | Backend (Security) | EP-01 |
| | TS-3.2: Policy Decision & Admin (PDP+PAP) | 13 | Backend (Security) | TS-3.1 |
| | TS-3.2b: Attribute Resolution (PIP) | 13 | Backend (Security) | TS-3.1, TS-4.3 |
| | TS-3.3: Authorization SQL Tables | 13 | DBA, Backend | TS-1.2 |
| | TS-3.4: Middleware & Attributes | 8 | Backend | TS-3.2, TS-3.2b |
| | TS-3.5: Policy API Endpoints | 8 | Backend | TS-3.2 |
| | TS-3.6: PDP Unit Tests | 13 | Backend (Testing) | TS-3.2 |
| | TS-3.7: Authorization Integration Tests | 13 | QA | TS-3.3 |
| **EP-04** | TS-4.1: Config Domain Model | 8 | Backend | EP-01, EP-02 |
| | TS-4.2: Config SQL Tables | 5 | DBA, Backend | TS-1.2 |
| | TS-4.3: Hierarchical Resolution & Encryption | 8 | Backend | TS-4.1, TS-4.2 |
| | TS-4.4: Config API Endpoints | 5 | Backend | TS-4.3 |
| | TS-4.5: Config Hierarchy Tests | 8 | QA | TS-4.2 |
| **EP-05** | TS-5.1: Login Page (Razor) | 13 | Backend/Frontend | EP-01 |
| | TS-5.2: Diagnostics Dashboard | 13 | Backend/Frontend | TS-1.6, TS-3.6, TS-4.5 |
| | TS-5.3: Audit Log Endpoint | 8 | Backend | Audit (cross-cutting) |
| | TS-5.4: Health Check Endpoint | 5 | Backend | All infra |
| | TS-5.5: Login & Diagnostics Tests | 8 | QA | TS-5.1, TS-5.2 |
| **EP-06** | TS-6.1: MFA Domain Model | 13 | Backend (DDD) | EP-01, EP-04 |
| | TS-6.2: Risk Scoring Engine | 21 | Backend (Algorithms) | TS-6.1 |
| | TS-6.3: Passwordless Methods | 21 | Backend (Security) | TS-6.2 |
| | TS-6.4: MFA SQL Tables | 8 | DBA, Backend | TS-1.2 |
| | TS-6.5: Approvals Domain Model | 13 | Backend (DDD) | EP-03 |
| | TS-6.6: Approvals SQL Tables | 8 | DBA, Backend | TS-1.2 |
| | TS-6.7: B2B Access Flow | 13 | Backend | TS-6.5, TS-6.6 |
| | TS-6.8: Delegated Admin Scopes | 21 | Backend (State Machines) | TS-6.5, TS-6.6 |
| | TS-6.9: Delegation Middleware | 8 | Backend | TS-6.8 |
| | TS-6.10: Security API Endpoints | 13 | Backend | TS-6.2, TS-6.7, TS-6.8 |
| | TS-6.11: MFA Integration Tests | 13 | QA | TS-6.4, TS-6.3 |
| | TS-6.12: B2B & Delegation Tests | 13 | QA | TS-6.7, TS-6.8 |
| **EP-07** | TS-7.1: Compliance Domain Model | 8 | Backend (DDD) | EP-01 |
| | TS-7.2: Document Upload Service | 13 | Backend (Security) | TS-7.1 |
| | TS-7.3: Notification Engine | 13 | Backend (Background Services) | TS-7.1, EP-04 |
| | TS-7.4: Compliance SQL Tables | 8 | DBA, Backend | TS-1.2 |
| | TS-7.5: Enforcement Engine | 13 | Backend (Background Services) | TS-7.1, TS-7.4 |
| | TS-7.6: Compliance API Endpoints | 8 | Backend | TS-7.2, TS-7.5 |
| | TS-7.7: Compliance Tests | 13 | QA | TS-7.4, TS-7.5 |
| **EP-08** | TS-8.1: Maturity Domain Model | 13 | Backend (DDD) | EP-01, EP-03 |
| | TS-8.2: Eligibility Checker & Watcher | 13 | Backend (Background Services) | TS-8.1, TS-4.3 |
| | TS-8.3: Impact Analysis Engine | 21 | Backend (Algorithms) | TS-8.1, EP-03 |
| | TS-8.4: Promotion State Machine | 8 | Backend | TS-8.1 |
| | TS-8.5: IGA SQL Tables | 8 | DBA, Backend | TS-1.2 |
| | TS-8.6: Workflow Integration | 13 | Backend | TS-8.4, TS-6.5 |
| | TS-8.7: Eligibility Notification | 5 | Backend | TS-8.2, TS-7.3 (preferred) |
| | TS-8.8: IGA API Endpoints | 8 | Backend | TS-8.4 |
| | TS-8.9: Promotion Integration Tests | 13 | QA | TS-8.5, TS-8.3 |

---

## PART 3: TEAM PROFILES & COMPOSITION

### Profile 1: Backend Engineer (Domain-Driven Design Specialist)

**Role Purpose:** Implement domain models, aggregates, value objects, and business logic  
**Seniority:** Senior (5+ years)  
**Key Skills:**
- .NET 8 / C# 12+
- Domain-Driven Design (tactical: Aggregates, Value Objects, Domain Services)
- Entity Framework Core 8 (configuration, querying, migrations)
- DDD patterns (CQRS, Event Sourcing optional)
- SQL Server fundamentals
- Unit testing (xUnit, NSubstitute)

**Tech Stack Proficiency:**
- Languages: C#, T-SQL (basic)
- Frameworks: ASP.NET Core, EF Core
- Testing: xUnit, NSubstitute, FluentAssertions
- VCS: Git, GitHub

**Story Ownership (Primary):**
- TS-1.1, TS-1.4, TS-2.1, TS-2.3
- TS-3.1, TS-4.1, TS-4.3
- TS-6.1, TS-6.5, TS-7.1
- TS-8.1, TS-8.2, TS-8.4

**Story Involvement (Support):**
- TS-1.2, TS-2.2, TS-3.3, TS-4.2, TS-6.6, TS-7.4, TS-8.5 (model advice, EF mapping)
- TS-3.2, TS-6.3, TS-8.3 (algorithm design)

**Estimated Capacity:**
- MVP (EP-01-05): 2 engineers, ~13 weeks
- Post-MVP (EP-06-08): 2 engineers, ~8 weeks
- **Recommendation:** Keep same 2 engineers across both phases for domain continuity

---

### Profile 2: Backend Engineer (Security & Algorithms Specialist)

**Role Purpose:** Implement complex security features, risk scoring, impact analysis, state machines  
**Seniority:** Senior/Staff (6+ years, security focus)  
**Key Skills:**
- Security fundamentals (XACML, RBAC, ABAC)
- Cryptography (password hashing, encryption, signatures)
- Authentication & authorization patterns
- Risk analysis & threat modeling
- Algorithm design (scoring, matching, conflict detection)
- Async & event-driven patterns
- High-load systems (caching, optimization)

**Tech Stack Proficiency:**
- Languages: C#, T-SQL
- Libraries: Yubico WebAuthn, Polly (resilience), MassTransit (optional)
- Security: OWASP, CWE knowledge
- Testing: xUnit, integration test patterns
- External APIs: GeoIP, IP reputation services

**Story Ownership (Primary):**
- TS-3.2 (PEP/PDP/PAP/PIP)
- TS-6.1, TS-6.2, TS-6.3, TS-6.8 (MFA, risk, delegation)
- TS-7.3, TS-7.5 (notification, enforcement engines)
- TS-8.3 (impact analysis)

**Story Involvement (Support):**
- TS-6.7 (B2B workflows)
- TS-8.6 (approval integration)

**Estimated Capacity:**
- MVP (EP-01-05): 0.5 engineer (light: TS-4.1 config only)
- Post-MVP (EP-06-08): 1.5 engineers, ~10 weeks
- **Recommendation:** Onboard for Sprint 3 (Post-MVP), but light involvement in MVPe planning phase

---

### Profile 3: Database Administrator (SQL Server & Optimization)

**Role Purpose:** Design & implement schema, indexing, partitioning, RLS, performance tuning  
**Seniority:** Mid/Senior (4+ years SQL Server)  
**Key Skills:**
- SQL Server 2022 internals
- Schema design (normalization, composite keys, audit columns)
- Partitioning strategies
- Row-Level Security (RLS) predicates
- Indexing & query optimization
- Execution plans & performance analysis
- Backup, recovery, high availability
- Monitoring & alerting (SQL Server Agent, extended events)

**Tech Stack Proficiency:**
- SQL Server 2022 (T-SQL, DMVs, Query Store)
- Tools: SSMS, Azure Data Studio
- EF Core integration (migrations, DbContext configuration)
- Monitoring: Application Insights, Datadog
- VCS: Git (SQL scripts)

**Story Ownership (Primary):**
- TS-1.2 (Tenant RLS + partitioning)
- TS-2.2, TS-3.3, TS-4.2 (catalog, authorization, config tables)
- TS-6.4, TS-6.6 (MFA, approvals tables)
- TS-7.4 (compliance tables)
- TS-8.5 (IGA tables)

**Story Involvement (Support):**
- TS-1.3 (RLS implementation review)
- TS-1.6, TS-3.7, TS-7.7 (integration test environment setup)

**Estimated Capacity:**
- MVP (EP-01-05): 1 engineer, ~6 weeks (heavy schema work early, light after)
- Post-MVP (EP-06-08): 0.5 engineer, ~4 weeks (incremental table additions)
- **Recommendation:** Front-load DBA in MVP phase; transition to part-time in Post-MVP

---

### Profile 4: Backend Engineer (API & Infrastructure)

**Role Purpose:** Build REST APIs, middleware, health checks, error handling, integration  
**Seniority:** Mid/Senior (4+ years .NET)  
**Key Skills:**
- REST API design (OpenAPI, versioning)
- ASP.NET Core pipeline (middleware, filters, routing)
- Input validation (FluentValidation)
- Error handling & logging (Serilog)
- Async/await patterns
- Dependency injection
- Integration with domain logic
- Configuration management

**Tech Stack Proficiency:**
- ASP.NET Core 8
- OpenAPI 3.0 / Swagger
- FluentValidation
- Serilog / structured logging
- xUnit integration tests
- HTTP client patterns

**Story Ownership (Primary):**
- TS-1.5, TS-2.4, TS-4.4
- TS-3.4, TS-3.5 (middleware + endpoints)
- TS-5.3, TS-5.4 (audit, health endpoints)
- TS-6.10, TS-8.8 (security, IGA endpoints)
- TS-7.6 (compliance endpoints)

**Estimated Capacity:**
- MVP (EP-01-05): 1 engineer, ~6 weeks
- Post-MVP (EP-06-08): 1 engineer, ~5 weeks
- **Recommendation:** Part of core team across both phases

---

### Profile 5: QA Engineer (Integration & E2E Testing)

**Role Purpose:** Write integration tests validating RLS, workflows, end-to-end scenarios  
**Seniority:** Mid (3+ years automation)  
**Key Skills:**
- Integration test patterns
- Test data setup (fixtures, seeding)
- SQL Server test database management
- Multi-tenant test scenarios
- API testing
- Security testing (RLS, authorization boundaries)
- Test documentation

**Tech Stack Proficiency:**
- xUnit / NUnit
- Testcontainers (SQL Server in Docker)
- REST client libraries (RestClient.Net or similar)
- Git, CI/CD pipeline understanding
- Test assertion libraries (FluentAssertions)

**Story Ownership (Primary):**
- TS-1.6 (RLS integration tests)
- TS-2.5, TS-3.7, TS-4.5
- TS-5.5 (login & diagnostics)
- TS-6.11, TS-6.12 (MFA, B2B, delegation)
- TS-7.7 (compliance)
- TS-8.9 (promotion)

**Estimated Capacity:**
- MVP (EP-01-05): 1 engineer, ~8 weeks (heavy testing phase)
- Post-MVP (EP-06-08): 1 engineer, ~10 weeks (complex workflows)
- **Recommendation:** Dedicated full-time QA across entire construction

---

### Profile 6: Frontend Engineer (React/TypeScript, Vite, TanStack)

**Role Purpose:** Build React frontend (login page, dashboards, forms) using Vite + Zustand + TanStack Query  
**Seniority:** Mid (3+ years React + TypeScript)  
**Key Skills:**
- React (v18+) with Hooks
- TypeScript strict mode
- Vite build tooling
- Zustand state management
- TanStack Query (data fetching)
- CSS (Tailwind or styled-components)
- Accessibility (WCAG 2.1 AA)
- Form handling & validation (Zod, React Hook Form)
- API integration (REST, error handling)

**Tech Stack Proficiency:**
- React 18+, TypeScript 5.4+
- Vite (build, dev server)
- Zustand (state)
- TanStack Query (async state)
- Tailwind CSS or Emotion
- Recharts (for dashboards/charts)
- Chrome DevTools, React DevTools

**Story Ownership (Primary):**
- TS-5.1 (login page)
- TS-5.2 (diagnostics dashboard)

**Story Involvement (Support):**
- TS-6.10 (B2B access request UI)
- TS-7.6 (document upload form + file handling)
- TS-8.8 (promotion request UI + maturity display)

**Estimated Capacity:**
- MVP (EP-01-05): 0.5 engineer, ~3-4 weeks
- Post-MVP (EP-06-08): 0.5 engineer, ~3 weeks (forms + dialogs)
- **Recommendation:** Part-time, flexible across sprints (can parallelize with backend)

---

### Profile 7: DevOps Engineer (CI/CD, Infrastructure)

**Role Purpose:** Set up GitHub Actions CI/CD, SQL Server test environment, monitoring  
**Seniority:** Mid (3+ years DevOps/.NET)  
**Key Skills:**
- GitHub Actions workflows
- Docker & containerization
- SQL Server deployment & management
- Environment configuration
- Secrets management (Key Vault)
- Monitoring setup (Application Insights, logging)
- Performance profiling

**Tech Stack Proficiency:**
- GitHub Actions (YAML)
- Docker, docker-compose
- SQL Server (deployment, backup)
- Azure (optional: KeyVault, App Service)
- PowerShell / Bash scripting
- Logging: Serilog, Application Insights

**Story Involvement (Support):**
- Infrastructure setup (pre-Sprint 1)
- TS-1.2, TS-3.3 (test DB management)
- TS-1.6, TS-3.7 (integration test env)
- All stories (CI/CD pipeline, build/test automation)

**Estimated Capacity:**
- Pre-construction: 1 engineer, ~2 weeks (setup)
- MVP (EP-01-05): 0.25 engineer (maintenance)
- Post-MVP (EP-06-08): 0.25 engineer (maintenance)
- **Recommendation:** Front-load in Sprint 0; minimal involvement thereafter

---

### Profile 8: Solutions Architect / Tech Lead

**Role Purpose:** Oversight, ADR guidance, technical decisions, cross-context integration  
**Seniority:** Staff/Principal (10+ years)  
**Key Skills:**
- Enterprise architecture
- DDD & microservices
- Security architecture
- Technology strategy
- Communication & mentoring
- ADR writing & decision records

**Story Involvement (Oversight):**
- All stories (technical review, architecture alignment)
- Cross-context integration (Approvals ↔ Authorization ↔ IGA, etc.)
- ADR decisions
- Risk mitigation

**Estimated Capacity:**
- MVP (EP-01-05): 0.5 engineer (weekly reviews, decisions)
- Post-MVP (EP-06-08): 0.5 engineer (weekly reviews, decisions)
- **Recommendation:** Principal Architect from existing team (part-time)

---

## PART 4: TEAM COMPOSITION MATRIX

### MVP Phase (Sprints 1-2: 6-7 weeks)

| Role | Count | Names | Allocation | Epics |
|------|-------|-------|------------|-------|
| **Backend (DDD)** | 2 | — | 100% | EP-01, EP-02, EP-03, EP-04, EP-05 |
| **Backend (API/Infra)** | 1 | — | 100% | EP-01, EP-02, EP-03, EP-04, EP-05 |
| **DBA (SQL Server)** | 1 | — | 100% | EP-01, EP-02, EP-03, EP-04 |
| **QA Automation** | 1 | — | 100% | EP-01, EP-02, EP-03, EP-04, EP-05 |
| **Frontend (Razor)** | 0.5 | — | 50% | EP-05 |
| **DevOps** | 0.25 | — | 25% | Infra (pre-sprint 1) |
| **Tech Lead** | 0.5 | — | 50% | All (oversight) |
| **Total FTE** | 6.25 | — | — | — |

**Team Structure:**
- **Scrum Master:** 1 person (from existing org)
- **Product Owner:** 1 person (from existing org, not counted in engineering FTE)
- **Core Engineering:** 5.75 FTE
- **Recommended:** Dedicated Product Owner, shared Scrum Master

---

### Post-MVP Phase (Sprints 3-5: 8-10 weeks)

| Role | Count | Names | Allocation | Epics |
|------|-------|-------|------------|-------|
| **Backend (DDD)** | 2 | (same) | 100% | EP-06, EP-07, EP-08 |
| **Backend (Security)** | 1.5 | — | 100% | EP-06 (primary), EP-07 (support) |
| **Backend (API/Infra)** | 1 | (same) | 100% | EP-06, EP-07, EP-08 |
| **DBA (SQL Server)** | 0.5 | (same) | 50% | EP-06, EP-07, EP-08 |
| **QA Automation** | 1 | (same) | 100% | EP-06, EP-07, EP-08 |
| **Frontend (Razor)** | 0.5 | (same) | 50% | EP-06, EP-07, EP-08 |
| **DevOps** | 0.25 | (same) | 25% | Maintenance |
| **Tech Lead** | 0.5 | (same) | 50% | All (oversight) |
| **Total FTE** | 7.75 | — | — | — |

**Team Changes:**
- **Add:** 1 Backend Security engineer (onboard Sprint 2)
- **Reduce:** DBA from 100% → 50% (lighter schema work)
- **Maintain:** Core DDD engineers, API engineer, QA, Frontend

---

### Full Program (MVP + Post-MVP: 14-17 weeks)

**Total Program Effort:** ~7 FTE × 15 weeks ≈ **105 person-weeks** (excluding PM/Scrum)

**Ramp-Up Profile:**
```
Sprint 0 (Week 1):       DevOps setup → 0.25 FTE, Tech Lead → 0.5 FTE
Sprint 1-2 (Weeks 2-7):  MVP team (6.25 FTE)
Sprint 3-5 (Weeks 8-17): Post-MVP team (7.75 FTE, includes security specialist)
```

---

## PART 5: SKILLS MATRIX & HIRING GAPS

### Required Skills Inventory

| Skill | Role | Level | Criticality |
|-------|------|-------|-------------|
| **Domain-Driven Design** | Backend (DDD) | Expert | CRITICAL |
| **Entity Framework Core 8** | Backend (DDD), DBA | Advanced | CRITICAL |
| **.NET 8 / C#** | Backend (DDD), API, Security | Advanced | CRITICAL |
| **SQL Server 2022** | DBA | Expert | CRITICAL |
| **Row-Level Security (RLS)** | DBA, Backend (DDD) | Advanced | CRITICAL |
| **REST API Design** | Backend (API) | Advanced | HIGH |
| **XACML / Authorization** | Backend (Security) | Advanced | HIGH |
| **Cryptography & Security** | Backend (Security) | Advanced | HIGH |
| **Performance Optimization** | DBA, Backend (Security) | Advanced | HIGH |
| **Integration Testing** | QA | Advanced | HIGH |
| **ASP.NET Razor Pages** | Frontend | Intermediate | MEDIUM |
| **GitHub Actions CI/CD** | DevOps | Advanced | MEDIUM |
| **Event-Driven Architecture** | Backend (DDD) | Intermediate | MEDIUM |
| **State Machines** | Backend (Security) | Intermediate | MEDIUM |

### Hiring Recommendations

**Gap Analysis:**

If existing team has:
- ✅ 2 mid-level .NET backend engineers
- ✅ 1 DBA with SQL Server experience
- ✅ 0 security/authorization specialists

**Recommended Hiring:**
1. **Senior Backend Engineer (Security)** — 1 FTE, 6+ years security focus (START: Sprint 2)
2. **Mid-level QA Engineer** — 1 FTE, automation focus (START: Sprint 1)
3. **Frontend Engineer** — 0.5 FTE (can be part-time or shared from another team)
4. **DevOps Engineer** — 0.25 FTE (can be infrastructure/platform team member)

**Optional Ramp-Up Approach:**
- Sprint 0: Hire & onboard Security specialist + QA
- Sprint 1: Start with 4.75 FTE (existing 3 + QA + partial DevOps)
- Sprint 3: Add Security specialist (1.5 FTE role)

---

## PART 6: DEPENDENCIES & CRITICAL PATH

### Cross-Context Dependencies

```
EP-01 (Tenant & Identity)
  ↓ (tenants, users, RLS foundation)
  ├─→ EP-02 (System Catalog) ─→ EP-04 (Configuration)
  │                              ↓
  ├─→ EP-03 (Authorization)  ←─┘
  │    ↓ (policy evaluation engine)
  │    ├─→ EP-06 (Security)
  │    │    ├─→ TS-6.5/6.6 (Approvals)
  │    │    └─→ TS-6.8 (Delegation)
  │    │
  │    └─→ EP-08 (IGA)
  │         └─→ TS-8.3 (Impact Analysis)
  │
  └─→ EP-05 (Experience)
       ├─→ TS-5.3 (Audit endpoint — cross-cutting)
       └─→ TS-5.2 (Diagnostics — aggregates metrics)
```

### Critical Path (MVP)

**Longest chain:** EP-01 → TS-1.2 (RLS) → TS-1.3 (Interceptor) → EP-03 → TS-3.2 (PDP)

**Weeks 1-2:** Parallel TS-1.1, TS-1.2, TS-2.1, TS-3.1, TS-4.1  
**Weeks 3-4:** Parallel TS-1.3, TS-1.4, TS-2.2, TS-3.2, TS-4.2  
**Weeks 5-6:** Parallel API endpoints (TS-1.5, TS-2.4, TS-3.5, TS-4.4) + Integration tests  
**Weeks 6-7:** EP-05 (UI + diagnostics), final integration testing

### Critical Path (Post-MVP)

**Longest chain:** TS-6.2 (Risk Scoring) → TS-6.3 (Passwordless) → TS-6.10 (API)

**Weeks 1-2:** Parallel TS-6.1, TS-7.1, TS-8.1  
**Weeks 2-3:** Parallel TS-6.2, TS-6.5, TS-7.3, TS-8.3  
**Weeks 3-4:** Parallel TS-6.3, TS-6.6, TS-7.4, TS-8.5  
**Weeks 4-5:** TS-6.7, TS-6.8, TS-7.5, TS-8.4  
**Weeks 6-8:** API endpoints, integration testing, compliance validation

---

## PART 7: EFFORT ESTIMATION VALIDATION

### Story Points Summary

| Épica | Total Points | Weeks (Scrum: 15 pts/week) | FTE |
|-------|--------------|---------------------------|-----|
| **EP-01** | 55 pts | 3.7 weeks | — |
| **EP-02** | 31 pts | 2.1 weeks | — |
| **EP-03** | 89 pts | 5.9 weeks | — |
| **EP-04** | 31 pts | 2.1 weeks | — |
| **EP-05** | 47 pts | 3.1 weeks | — |
| **MVP TOTAL** | **256 pts** | **~17.1 weeks** | **~6.25 FTE × 1 sprint = FITS** |
|||||
| **EP-06** | 150 pts | 10 weeks | — |
| **EP-07** | 69 pts | 4.6 weeks | — |
| **EP-08** | 111 pts | 7.4 weeks | — |
| **Post-MVP TOTAL** | **330 pts** | **~22.0 weeks** | **~7.75 FTE × 1 sprint = FITS** |
|||||
| **FULL PROGRAM** | **586 pts** | **~39.1 weeks** | **~7 FTE avg** |

**Validation Notes (UPDATED per ADR audit):**
- MVP: 256 pts ÷ 6.25 FTE ÷ 1.5 sprints (6-7 weeks) ≈ **27.4 pts/week per engineer** (realistic for mature team)
- Post-MVP: 330 pts ÷ 7.75 FTE ÷ 2.5 sprints (10 weeks) ≈ **17.0 pts/week per engineer** (conservative, complex domain)
- **Adjustment:** +8 pts MVP, +5 pts Post-MVP (from ADR audit of TS-3.2, TS-4.3, TS-8.2)
- **Contingency:** Built-in 10-15% buffer for unknowns, integration complexity

---

## SUMMARY: IMMEDIATE NEXT STEPS

### For Sprint 0 (Week 1 before construction)

**By Engineering Lead:**
1. ✅ Validate tech stack: .NET 8, EF Core 8, SQL Server 2022, xUnit
2. ✅ Create GitHub Actions CI/CD pipeline (DevOps engineer)
3. ✅ Set up SQL Server test environment with RLS enabled
4. ✅ Create shared NuGet structure for domain models, contracts
5. ✅ Run ADR training session (all engineers) — focus on ADR-0048, ADR-0049, ADR-0039, ADR-0021
6. ✅ Finalize hiring plan: onboard Security specialist + QA before Sprint 1

**By Product Owner:**
1. ✅ Backlog refinement: convert stories to tickets in GitHub Projects
2. ✅ Sprint planning: allocate stories to Sprint 1-2 sprints
3. ✅ Clarify tenant onboarding success criteria with business

**By Tech Lead:**
1. ✅ Review SERVICE-IMPLEMENTATION-PLAN.md with team
2. ✅ Clarify RLS two-layer model (EF Core Layer 1 + SQL Server Layer 2)
3. ✅ Confirm partition key strategy (root_tenant_id on all queries)

---

**Document Approval:**
- **Prepared by:** Principal Architect
- **Date:** 2026-05-14
- **Status:** ✅ **READY FOR CONSTRUCTION PLANNING**

---

*For questions on team composition, see PART 4. For technical story breakdown, see PART 1. For effort estimates, see PART 7.*
