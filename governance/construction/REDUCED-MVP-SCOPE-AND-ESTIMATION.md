# Reduced MVP — Scope, Estimation & Deliverables

**Date:** 2026-05-14  
**Version:** 1.0  
**Team:** 4 persons (1 TL + 3 semi-senior developers), 7h/day  
**MVP Timeline:** 12 weeks (3 months)  
**Status:** ✅ **REALISTIC AND VIABLE**

---

## 1. REDUCED MVP DEFINITION

### Functional Stories in MVP (8 of 16)

| FS | Title | MVP Scope | Notes |
|----|-------|-----------|-------|
| **FS-01** | Corporate User Login | ✅ Complete | Email + password, credential validation |
| **FS-02** | User Self-Registration | ✅ Complete | Email + name + password, verification email |
| **FS-03** | Organization Onboarding | ✅ Complete | Create root tenant + admin user |
| **FS-05** | Define Authorization Policy | ✅ Complete | Create policies with rules, conditions, effects |
| **FS-06** | Assign Authorization Profile | ✅ Complete | Create profiles, assign to users |
| **FS-07** | Evaluate User Permissions | ✅ Complete | Runtime decision point, middleware enforcement |
| **FS-13** | Define Hierarchical Configuration | ✅ Complete | 4-level hierarchy, caching, encryption |
| **FS-08** | Hosted Login Page | ✅ Partial | Login page only (no diagnostics dashboard) |
| **FS-04** | System Catalog | ❌ **Post-MVP** | Register systems, define topology |
| **FS-09** | Adaptive MFA | ❌ **Post-MVP** | Risk scoring, passwordless methods |
| **FS-10** | B2B External Access | ❌ **Post-MVP** | External user approval workflow |
| **FS-11** | Document Upload | ❌ **Post-MVP** | Compliance document storage |
| **FS-12** | Role Promotion | ❌ **Post-MVP** | Role maturity tracking, promotion workflow |
| **FS-14** | Delegated Administration | ❌ **Post-MVP** | Delegation with scope constraints |
| **FS-15** | Expiration Notifications | ❌ **Post-MVP** | Background notification engine |
| **FS-16** | Access Enforcement | ❌ **Post-MVP** | Access suspension/revocation on expiration |

**MVP Functional:** 8 of 16 FS (50%)

---

## 2. TECHNICAL STORIES IN MVP (58 of 89)

### EP-01: Tenant & Identity (COMPLETE)

| TS | Title | Size | Include? | Notes |
|----|-------|------|----------|-------|
| TS-1.1 | Tenant Hierarchy Domain Model | 8 | ✅ | Tenant aggregate, TenantClosure pattern |
| TS-1.2 | SQL Server RLS + Partition | 8 | ✅ | 4 tables, composite PK, partition root_tenant_id |
| TS-1.3 | EF Core Global Query Filters | 8 | ✅ | ICurrentTenantResolver, global filters |
| TS-1.4 | Registration Ports/Adapters | 8 | ✅ | IPasswordHasher, IEmailService, IUserRepository |
| TS-1.5 | Auth API Endpoints | 8 | ✅ | /login, /register, /tenants endpoints |
| TS-1.6 | RLS Integration Tests | 13 | ✅ | 3-tenant isolation, Layer 1+2 RLS validation |
| **EP-01 Total** | — | **55** | ✅ | **Foundational, all stories required** |

### EP-03: Authorization (CORE ONLY)

| TS | Title | Size | Include? | Notes |
|----|-------|------|----------|-------|
| TS-3.1 | XACML Domain Model | 13 | ✅ | Policy, Rule, Profile, Permission aggregates |
| TS-3.2 | PDP + PAP Implementation | 13 | ✅ | Policy Decision Point + Policy Admin Point (core logic) |
| TS-3.2b | PIP (Attribute Resolution) | 13 | ❌ | **Deferred to Post-MVP** (use basic attributes MVP) |
| TS-3.3 | Authorization SQL Tables | 13 | ✅ | policies, rules, profiles, assignments tables |
| TS-3.4 | Authorization Middleware | 8 | ✅ | Intercept requests, enforce ALLOW/DENY |
| TS-3.5 | Policy API Endpoints | 8 | ✅ | CRUD policies/profiles, decision test endpoint |
| TS-3.6 | PDP Unit Tests | 13 | ❌ | **Deferred to Post-MVP** (20+ scenarios optional MVP) |
| TS-3.7 | Authorization Integration Tests | 13 | ❌ | **Deferred to Post-MVP** (manual E2E testing MVP) |
| **EP-03 Total (MVP)** | — | **56** | ✅ | **Core authorization, no advanced testing** |
| **EP-03 Total (Post-MVP)** | — | **39** | ❌ | TS-3.2b, TS-3.6, TS-3.7 deferred |

### EP-04: Configuration (COMPLETE)

| TS | Title | Size | Include? | Notes |
|----|-------|------|----------|-------|
| TS-4.1 | Config Domain Model | 8 | ✅ | ConfigurationParameter aggregate, scope hierarchy |
| TS-4.2 | Config SQL Tables | 5 | ✅ | parameters, parameter_history tables |
| TS-4.3 | Hierarchical Resolution Service | 8 | ✅ | 4-level resolution, encryption, caching |
| TS-4.4 | Config API Endpoints | 5 | ✅ | CRUD parameters, resolve endpoint |
| TS-4.5 | Config Integration Tests | 8 | ✅ | Hierarchy resolution, cache, invalidation |
| **EP-04 Total** | — | **31** | ✅ | **Complete, required by TS-3.2 + TS-8.2 (Post-MVP)** |

### EP-05: Experience & Diagnostics (LIGHT)

| TS | Title | Size | Include? | Notes |
|----|-------|------|----------|-------|
| TS-5.1 | Hosted Login Page (React) | 13 | ✅ | Branded login form, responsive, validation |
| TS-5.2 | Diagnostics Dashboard | 13 | ❌ | **Deferred to Post-MVP** (admin metrics, real-time) |
| TS-5.3 | Audit Log Endpoint | 8 | ✅ | GET /audit/logs, queryable, paginated |
| TS-5.4 | Health Check Endpoint | 5 | ✅ | /health, /health/ready for monitoring |
| TS-5.5 | Frontend Integration Tests | 8 | ❌ | **Deferred to Post-MVP** (manual testing MVP) |
| **EP-05 Total (MVP)** | — | **26** | ✅ | **Login + minimal monitoring** |
| **EP-05 Total (Post-MVP)** | — | **21** | ❌ | TS-5.2, TS-5.5 deferred |

### EP-02: System Catalog (DEFERRED)

| TS | Title | Size | Include? | Notes |
|----|-------|------|----------|-------|
| TS-2.1 to TS-2.5 | All | 31 | ❌ | **Entirely deferred to Post-MVP Phase 2** |

---

## 3. MVP REDUCED TOTALS

| Epic | TS Included | Total Points | Hours Est. | % of Original |
|------|-------------|--------------|-----------|---------------|
| **EP-01** | TS-1.1 to 1.6 | 55 | 280h | 100% (complete) |
| **EP-03** | TS-3.1, 3.2, 3.3, 3.4, 3.5 | 56 | 280h | 63% (defer PIP, tests) |
| **EP-04** | TS-4.1 to 4.5 | 31 | 155h | 100% (complete) |
| **EP-05** | TS-5.1, 5.3, 5.4 | 26 | 130h | 55% (defer diagnostics, tests) |
| **TOTAL MVP** | **58 TS of 89** | **168 pts** | **845h** | **28.8% of 2,762h program** |

**Validation:**
- 845h ÷ 4 people ÷ 7h/day ÷ 5 days = 6 weeks pure work
- Plus overhead (20%), learning (15%), unknowns (10%) = 845h × 1.45 = 1,225h estimate
- 1,225h ÷ 122.5h/week = **10 weeks = 2.5 sprints** ✅
- **MVP Timeline: 12 weeks wall-clock (includes Sprint 0 setup, buffer)**

---

## 4. SPRINT BREAKDOWN (REDUCED MVP)

### Sprint 0: Week 1 (Setup & Learning)

**Capacity:** 80h

**Activities:**
- GitHub Actions CI/CD setup
- SQL Server LocalDB + Testcontainers config
- ADR training: ADR-0048, ADR-0039, ADR-0047
- Code style, branch strategy, PR templates
- Initial DB schema sketches

**Deliverable:** Build pipeline working, dev env ready

---

### Sprint 1: Weeks 2-3 (Foundation Models & Schema)

**Capacity:** 500h available, 320h consumed (64%)

**Assignments:**
- Dev1 (DDD): TS-1.1 (Tenant, 40h), TS-3.1 (Policy, 56h), TS-4.1 (Config, 32h) = 128h
- Dev2 (DBA): TS-1.2 (Identity schema, 60h), TS-4.2 (Config schema, 24h) = 84h
- Dev3 (QA): TS-3.3 (Auth schema, 52h), EF Core mapping (36h) = 88h
- TL: Architecture review & approval (20h)

**Deliverable:**
- ✅ 3 domain models (Tenant, Policy, Configuration)
- ✅ 3 SQL schemas (identity, authorization, configuration)
- ✅ EF Core DbContext with all mappings

---

### Sprint 2: Weeks 4-5 (Core Logic & Layers)

**Capacity:** 500h available, 350h consumed (70%)

**Assignments:**
- Dev1 (Backend): TS-1.3 (EF filters, 32h), TS-1.4 (ports/adapters, 40h) = 72h
- Dev2 (Security): TS-3.2 (PDP + PAP, 72h) = 72h
- Dev3 (Config): TS-4.3 (hierarchical resolution, 68h) = 68h
- TL: Code review, architecture oversight (20h)

**Deliverable:**
- ✅ EF Core global filters, tenant isolation working
- ✅ User registration flow complete
- ✅ PDP engine evaluates policies with caching
- ✅ Configuration resolution with hierarchy + encryption

---

### Sprint 3: Weeks 6-8 (APIs, Tests, UI)

**Capacity:** 750h available, 350h consumed

**Assignments:**
- Dev1: TS-1.5 (auth API, 32h), TS-3.5 (policy API, 32h), TS-4.4 (config API, 20h) = 84h
- Dev2: TS-3.4 (middleware, 32h), TS-5.3 (audit endpoint, 24h), TS-5.4 (health, 16h) = 72h
- Dev3: TS-5.1 (React login, 56h), TS-1.6 (RLS tests, 56h) = 112h
- TL: Code review, testing validation (24h)

**Deliverable:**
- ✅ All REST APIs functional
- ✅ Authorization middleware enforces decisions
- ✅ React login page working
- ✅ RLS integration tests pass
- ✅ **MVP COMPLETE & TESTED** ✅

---

## 5. WHAT MVP CAN DO

### ✅ In MVP:
- Users create accounts (email + password, verification)
- Corporate login (email + password)
- Organizations onboard (create root tenant + admin)
- **Multi-tenant isolation** (100% RLS coverage)
- **Authorization:** Admins define policies, assign profiles, runtime decisions
- **Configuration:** Hierarchical parameters, encryption, caching
- **UI:** Branded, responsive login page
- **Audit:** Query audit logs
- **Health:** Monitoring endpoints

### ❌ Deferred to Post-MVP:
- System registry
- Advanced MFA / Risk scoring
- B2B external access workflows
- Document upload & compliance
- Role maturity & promotion
- Delegated administration
- Diagnostics dashboard
- Advanced attribute resolution (PIP)

---

## 6. TEAM (4 PERSONS)

| Role | Capacity | Focus |
|-----|----------|-------|
| **Team Lead (1)** | 17.5h/week (50%) | Architecture, code review, mentoring |
| **Backend Dev 1** | 35h/week (100%) | Domain models, EF Core, API endpoints |
| **Backend Dev 2** | 35h/week (100%) | Rotating: DBA, Security (PDP), Config, Middleware |
| **QA/Backend Dev 3** | 35h/week (100%) | Schema, Frontend (React), Integration tests |

**Total:** 122.5h/week = 980h/8 weeks

---

## 7. CRITICAL RISKS & MITIGATIONS

| Risk | Mitigation |
|------|-----------|
| **TS-1.2 RLS delay** | External DBA code review Day 1 |
| **TS-3.2 PDP logic bug** | TL pair-programs, manual 20+ scenarios testing |
| **Dev2 skill gap** | TL mentors DBA/Security/Config work |
| **React learning** | Simplified scope (2 components, pattern-based) |

---

## 8. DEFINITION OF DONE (MVP)

### EP-01 (Identity)
- [ ] All tables with composite PK (id, root_tenant_id)
- [ ] EF Core filters block cross-tenant queries
- [ ] RLS tests isolate 3 tenants (Layer 1+2)
- [ ] Login/registration/onboarding working

### EP-03 (Authorization MVP)
- [ ] Policy, Rule, Profile aggregates implemented
- [ ] PDP evaluates rules (first match, effect aggregation)
- [ ] Middleware intercepts requests
- [ ] 5 endpoints functional: CRUD + check

### EP-04 (Configuration)
- [ ] 4-level resolution hierarchy working
- [ ] Encryption + caching + invalidation
- [ ] All tests passing

### EP-05 (Experience MVP)
- [ ] Login page: responsive, accessible WCAG A
- [ ] Audit endpoint: queryable, paginated
- [ ] Health checks: /health, /health/ready

---

## 9. SIGN-OFF CHECKLIST

- [ ] Scope lock: 8 FS, 168 pts confirmed
- [ ] Team assigned: 4 people, 12-week commitment
- [ ] RLS design reviewed by external DBA
- [ ] PDP logic reviewed by TL + Security
- [ ] ADR training completed
- [ ] CI/CD pipeline ready
- [ ] Estimation baseline approved (h/pt = 5.0)

---

**Approved by:** Principal Architect  
**Date:** 2026-05-14  
**Confidence Level:** 🟡 **MEDIUM-HIGH (70-75%)**  
**Status:** ✅ **READY FOR EXECUTION**
