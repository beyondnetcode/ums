# MVP Reducido — Alcance, Estimación y Entregables

**Fecha:** 2026-05-14
**Versión:** 1.0
**Equipo:** 4 personas (1 TL + 3 semi-senior), 7h/día
**Timeline MVP:** 12 semanas (3 meses)
**Status:** **REALISTA Y VIABLE**

---

## 1. DEFINICIÓN MVP REDUCIDO

### Historias Funcionales en MVP (8 de 16)

| FS | Título | Alcance MVP | Notas |
|----|--------|-------------|-------|
| **FS-01** | Corporate User Login | Completo | Email + password, credential validation |
| **FS-02** | User Self-Registration | Completo | Email + name + password, verification email |
| **FS-03** | Organization Onboarding | Completo | Create root tenant + admin user |
| **FS-05** | Define Authorization Policy | Completo | Create policies with rules, conditions, effects |
| **FS-06** | Assign Authorization Profile | Completo | Create profiles, assign to users |
| **FS-07** | Evaluate User Permissions | Completo | Runtime decision point, middleware enforcement |
| **FS-13** | Define Hierarchical Configuration | Completo | 4-level hierarchy, caching, encryption |
| **FS-08** | Hosted Login Page | Partial | Login page only (no diagnostics dashboard) |
| **FS-04** | System Catalog | **Post-MVP** | Register systems, define topology |
| **FS-09** | Adaptive MFA | **Post-MVP** | Risk scoring, passwordless methods |
| **FS-10** | B2B External Access | **Post-MVP** | External user approval workflow |
| **FS-11** | Document Upload | **Post-MVP** | Compliance document storage |
| **FS-12** | Role Promotion | **Post-MVP** | Role maturity tracking, promotion workflow |
| **FS-14** | Delegated Administration | **Post-MVP** | Delegation with scope constraints |
| **FS-15** | Expiration Notifications | **Post-MVP** | Background notification engine |
| **FS-16** | Access Enforcement | **Post-MVP** | Access suspension/revocation on expiration |

**MVP Funcional:** 8 de 16 FS (50%)

---

## 2. HISTORIAS TÉCNICAS EN MVP (58 de 89)

### EP-01: Tenant & Identity (COMPLETO)

| TS | Título | Size | Include? | Notas |
|----|--------|------|----------|-------|
| TS-1.1 | Tenant Hierarchy Domain Model | 8 | | Tenant aggregate, TenantClosure pattern |
| TS-1.2 | SQL Server RLS + Partition | 8 | | 4 tables, composite PK, partition root_tenant_id |
| TS-1.3 | EF Core Global Query Filters | 8 | | ICurrentTenantResolver, global filters |
| TS-1.4 | Registration Ports/Adapters | 8 | | IPasswordHasher, IEmailService, IUserRepository |
| TS-1.5 | Auth API Endpoints | 8 | | /login, /register, /tenants endpoints |
| TS-1.6 | RLS Integration Tests | 13 | | 3-tenant isolation, Layer 1+2 RLS validation |
| **EP-01 Total** | — | **55** | | **Foundational, all stories required** |

### EP-03: Authorization (CORE ONLY)

| TS | Título | Size | Include? | Notas |
|----|--------|------|----------|-------|
| TS-3.1 | XACML Domain Model | 13 | | Policy, Rule, Profile, Permission aggregates |
| TS-3.2 | PDP + PAP Implementation | 13 | | Policy Decision Point + Policy Admin Point (core logic) |
| TS-3.2b | PIP (Attribute Resolution) | 13 | | **Deferred to Post-MVP** (use basic attributes MVP) |
| TS-3.3 | Authorization SQL Tables | 13 | | policies, rules, profiles, assignments tables |
| TS-3.4 | Authorization Middleware | 8 | | Intercept requests, enforce ALLOW/DENY |
| TS-3.5 | Policy API Endpoints | 8 | | CRUD policies/profiles, decision test endpoint |
| TS-3.6 | PDP Unit Tests | 13 | | **Deferred to Post-MVP** (20+ scenarios optional MVP) |
| TS-3.7 | Authorization Integration Tests | 13 | | **Deferred to Post-MVP** (manual E2E testing MVP) |
| **EP-03 Total (MVP)** | — | **56** | | **Core authorization, no advanced testing** |
| **EP-03 Total (Post-MVP)** | — | **39** | | TS-3.2b, TS-3.6, TS-3.7 deferred |

### EP-04: Configuration (COMPLETO)

| TS | Título | Size | Include? | Notas |
|----|--------|------|----------|-------|
| TS-4.1 | Config Domain Model | 8 | | ConfigurationParameter aggregate, scope hierarchy |
| TS-4.2 | Config SQL Tables | 5 | | parameters, parameter_history tables |
| TS-4.3 | Hierarchical Resolution Service | 8 | | 4-level resolution, encryption, caching |
| TS-4.4 | Config API Endpoints | 5 | | CRUD parameters, resolve endpoint |
| TS-4.5 | Config Integration Tests | 8 | | Hierarchy resolution, cache, invalidation |
| **EP-04 Total** | — | **31** | | **Complete, required by TS-3.2 + TS-8.2 (Post-MVP)** |

### EP-05: Experience & Diagnostics (LIGHT)

| TS | Título | Size | Include? | Notas |
|----|--------|------|----------|-------|
| TS-5.1 | Hosted Login Page (React) | 13 | | Branded login form, responsive, validation |
| TS-5.2 | Diagnostics Dashboard | 13 | | **Deferred to Post-MVP** (admin metrics, real-time) |
| TS-5.3 | Audit Log Endpoint | 8 | | GET /audit/logs, queryable, paginated |
| TS-5.4 | Health Check Endpoint | 5 | | /health, /health/ready for monitoring |
| TS-5.5 | Frontend Integration Tests | 8 | | **Deferred to Post-MVP** (manual testing MVP) |
| **EP-05 Total (MVP)** | — | **26** | | **Login + minimal monitoring** |
| **EP-05 Total (Post-MVP)** | — | **21** | | TS-5.2, TS-5.5 deferred |

### EP-02: System Catalog (DEFERRED)

| TS | Título | Size | Include? | Notas |
|----|--------|------|----------|-------|
| TS-2.1 to TS-2.5 | All | 31 | | **Entirely deferred to Post-MVP Phase 2** (can run apps without system registry initially) |

---

## 3. TOTALES MVP REDUCIDO

| Épica | TS Included | Total Points | Hours Est. | % of Original |
|-------|-------------|--------------|-----------|---------------|
| **EP-01** | TS-1.1 to 1.6 | 55 | 280h | 100% (complete) |
| **EP-03** | TS-3.1, 3.2, 3.3, 3.4, 3.5 | 56 | 280h | 63% (defer PIP, tests) |
| **EP-04** | TS-4.1 to 4.5 | 31 | 155h | 100% (complete) |
| **EP-05** | TS-5.1, 5.3, 5.4 | 26 | 130h | 55% (defer diagnostics, tests) |
| **TOTAL MVP** | **58 TS of 89** | **168 pts** | **845h** | **28.8% of 2,762h program** |

**Validation:**
- 845h ÷ 4 people ÷ 7h/day ÷ 5 days = 6 weeks pure work
- Plus overhead (20%), learning (15%), unknowns (10%) = 845h × 1.45 = 1,225h estimate
- 1,225h ÷ 122.5h/week = **10 weeks = 2.5 sprints**
- **MVP Timeline: 12 weeks wall-clock (includes Sprint 0 setup, buffer)**

---

## 4. DESGLOSE POR SPRINT (MVP REDUCIDO)

### Sprint 0: Week 1 (Setup & Learning)
**Capacity:** 80h (light week, onboarding)

**Activities:**
- [ ] GitHub Actions CI/CD setup (DevOps reference, TL oversees)
- [ ] SQL Server LocalDB + Testcontainers config
- [ ] ADR training: ADR-0048 (closure), ADR-0039 (XACML), ADR-0047 (config)
- [ ] Code style, branch strategy, PR template review
- [ ] Initial DB schema sketches (DBA + TL)

**Deliverable:** Build pipeline working, dev env ready, team aligned on architecture

---

### Sprint 1: Weeks 2-3 (Foundation Models & Schema)
**Capacity:** 500h available, 320h consumed (64%)

**Assignments:**

| Dev | Story | Activity | Hours | Blocker |
|----|-------|----------|-------|---------|
| **Dev1 (Backend DDD)** | TS-1.1 | Tenant aggregate + TenantClosure | 40h | — |
| **Dev1 (Backend DDD)** | TS-3.1 | Policy + Rule + Profile aggregates | 56h | — |
| **Dev1 (Backend DDD)** | TS-4.1 | ConfigurationParameter aggregate | 32h | — |
| **Dev2 (DBA)** | TS-1.2 | Identity schema: tenants, users, tenant_closure + indices + partition | 60h | — |
| **Dev2 (DBA)** | TS-4.2 | Configuration schema: parameters, parameter_history | 24h | TS-1.2 done |
| **Dev3 (QA/Backend)** | TS-3.3 | Authorization schema: policies, rules, profiles | 52h | TS-1.2 done |
| **Dev3 (QA/Backend)** | EF Core Setup | DbContext mapping, migrations for all 3 schemas | 36h | Schemas done |
| **TL** | Architecture Review | Domain model reviews, ADR alignment checks | 20h | — |

**Total Sprint 1:** 320h / 500h available (64%)

**Deliverable:**
- 3 domain models (Tenant, Policy, Configuration)
- 3 SQL schemas (identity, authorization, configuration)
- EF Core DbContext with all mappings
- Migrations ready to run

**Blocker Resolution:**
- TS-1.2 must complete by Day 5, before TS-4.2 + TS-3.3 can finalize schema

---

### Sprint 2: Weeks 4-5 (Core Logic & Layers)
**Capacity:** 500h available, 350h consumed (70%)

**Assignments:**

| Dev | Story | Activity | Hours | Dependency |
|----|-------|----------|-------|-----------|
| **Dev1 (Backend DDD)** | TS-1.3 | ICurrentTenantResolver, global EF filters on all DbSets | 32h | TS-1.2 done |
| **Dev1 (Backend DDD)** | TS-1.4 | User registration domain service, ports/adapters (Bcrypt, Sendgrid, repo) | 40h | TS-1.3 |
| **Dev2 (Security/Backend)** | TS-3.2 | PDP + PAP engine: rule matching, effect aggregation, explanation, caching | 72h | TS-3.1, TS-3.3 done |
| **Dev3 (Backend/Config)** | TS-4.3 | Hierarchical resolution service: 4-level resolution, IsInheritable, IsEncrypted, Redis cache | 68h | TS-4.1, TS-4.2 done |
| **Dev3 (Backend)** | TS-2.3 (light) | Repository + service for System Catalog (light version, defer full TS-2.1-2.2) | 16h | Optional |
| **TL** | Code Review | TS-3.2 PDP logic review, TS-4.3 encryption integration | 20h | Dev2, Dev3 progress |

**Total Sprint 2:** 350h / 500h available (70%)

**Deliverable:**
- EF Core global filters applied, tenant isolation working
- User registration flow (domain service, email sending)
- PDP engine evaluates policies with caching
- Configuration resolution with hierarchy + encryption
- All 3 major business layers functional

**Critical Validation:**
- TS-3.2 rule matching must pass peer review (no logic errors)
- TS-4.3 cache invalidation must be correct (test locally)

---

### Sprint 3: Weeks 6-8 (APIs, Tests, UI)
**Capacity:** 750h available (3 weeks), 350h consumed

**Assignments:**

| Dev | Story | Activity | Hours | Dependency |
|----|-------|----------|-------|-----------|
| **Dev1 (Backend)** | TS-1.5 | /login, /register, /tenants endpoints + FluentValidation + error handling | 32h | TS-1.4 done |
| **Dev1 (Backend)** | TS-3.5 | /policies CRUD, /profiles/{id}/assign, /authorization/check endpoint | 32h | TS-3.2 done |
| **Dev1 (Backend)** | TS-4.4 | /configuration CRUD, /resolve endpoint | 20h | TS-4.3 done |
| **Dev3 (QA/Backend)** | TS-5.1 | React Login page: form, validation, Zustand state, Tailwind, responsive | 56h | TS-1.5 endpoint |
| **Dev2 (Backend)** | TS-3.4 | Authorization middleware: resolve context, call PDP, enforce DENY | 32h | TS-3.2, TS-3.3 done |
| **Dev2 (Backend)** | TS-5.3 | GET /audit/logs endpoint, filtering, pagination | 24h | Audit context |
| **Dev2 (Backend)** | TS-5.4 | /health + /health/ready endpoints | 16h | — |
| **Dev3 (QA)** | TS-1.6 | Integration tests: 3-tenant RLS isolation (Layer 1 EF + Layer 2 direct SQL) | 56h | TS-1.2, TS-1.3 done |
| **TL** | Code Review + Testing | API review, authorization middleware audit, test validation | 24h | — |

**Total Sprint 3:** 350h / 750h available

**Deliverable:**
- All REST APIs functional (login, register, policies, config, audit, health)
- Authorization middleware intercepts requests
- React login page working (responsive, accessible WCAG A)
- RLS integration tests pass (100% coverage critical)
- **MVP COMPLETE & TESTED**

**Release Readiness:**
- All endpoints tested manually
- RLS isolation validated
- Login flow E2E working
- Health checks passing
- Ready for internal demo / pilot

---

## 5. RESUMEN SPRINT ROADMAP

| Sprint | Week | Focus | Team Load | Hours | Deliverable | Gate |
|--------|------|-------|-----------|-------|------------|------|
| **0** | 1 | Setup | 20% | 80h | CI/CD, dev env | — |
| **1** | 2-3 | Domains + Schema | 64% | 320h | Models, schemas, EF | TS-1.2 review |
| **2** | 4-5 | Core Logic | 70% | 350h | PDP, config, EF filters | TS-3.2, TS-4.3 code review |
| **3** | 6-8 | APIs + Tests + UI | 47% | 350h | **MVP COMPLETE** | TS-1.6 tests pass |
| **Total** | **1-8** | — | **~60% avg** | **1,100h** | **MVP Production-Ready** | — |

**Wall-Clock Time:** 8 weeks to MVP (vs 12 weeks with buffer for unknowns)

---

## 6. ¿QUÉ FALTA EN MVP REDUCIDO?

### Deferred to Post-MVP (Justificación)

| Épica | TS Deferred | Reason | Post-MVP Sprint |
|-------|------------|--------|-----------------|
| **EP-02** | 2.1-2.5 (31 pts) | System Catalog optional for MVP (apps run without registry initially) | Sprint 4 |
| **EP-03** | 3.2b (13 pts) | PIP attribute resolution can use static attributes MVP, dynamic Post-MVP | Sprint 5 |
| **EP-03** | 3.6, 3.7 (26 pts) | Unit + integration tests deferred, manual testing MVP | Sprint 5 |
| **EP-05** | 5.2 (13 pts) | Diagnostics dashboard (nice-to-have, not critical) | Sprint 4 |
| **EP-05** | 5.5 (8 pts) | Frontend tests (manual E2E MVP) | Sprint 5 |
| **EP-06** | 6.1-6.12 (150 pts) | MFA, B2B, Delegation (advanced features) | Sprints 5-6 |
| **EP-07** | 7.1-7.7 (69 pts) | Compliance (document upload, notifications, enforcement) | Sprints 5-6 |
| **EP-08** | 8.1-8.9 (111 pts) | IGA role promotion (advanced maturity tracking) | Sprints 5-6 |
| **TOTAL Post-MVP** | **417 pts** | 50% of program | 12+ weeks |

### Impact on Functional Stories

**MVP Covers (8 FS):**
- FS-01: Corporate Login
- FS-02: Self-Registration
- FS-03: Org Onboarding
- FS-05: Policy Definition
- FS-06: Profile Assignment
- FS-07: Permission Evaluation
- FS-13: Config Hierarchy
- FS-08: Login Page (partial, no diagnostics)

**Post-MVP Covers (8 FS):**
- FS-04: System Catalog
- FS-09: Adaptive MFA
- FS-10: B2B External Access
- FS-11: Document Upload
- FS-12: Role Promotion
- FS-14: Delegated Admin
- FS-15: Expiration Notifications
- FS-16: Access Enforcement

---

## 7. CAPACIDADES MVP

### What MVP Can Do

**Identity & Access:**
- Users create accounts (email + password)
- Corporate users login with email + password
- Organizations onboard with admin user
- Multi-tenant isolation (3-layer: composite PK + EF filter + RLS optional)

**Authorization:**
- Admins define policies with rules, conditions, effects (XACML-style)
- Admins create profiles (bundles of policies)
- Admins assign profiles to users
- Runtime permission evaluation: user requests action → PDP evaluates → ALLOW/DENY decision
- Authorization middleware enforces decisions at API layer

**Configuration:**
- Admins define parameters at tenant/system/environment level
- Hierarchical resolution: system param overrides tenant param
- Encryption for sensitive values
- Caching with invalidation
- Feature flag support

**UI & Monitoring:**
- Branded, responsive login page (React)
- Audit log API (queryable, filterable)
- Health check endpoints (Kubernetes-compatible)

**Security (Foundational):**
- Password hashing (Bcrypt, 12 rounds)
- Email verification (Sendgrid)
- JWT authentication (httpOnly cookies)
- RLS tenant isolation (100% coverage)

---

## 8. CAPACIDADES NOT IN MVP

### What's Deferred to Post-MVP

**System Catalog:**
- No system registry (can hardcode system names MVP)
- No topology definition
- Deferred: FS-04

**Advanced Authorization:**
- No Policy Information Point (PIP) attribute resolution (use static attributes)
- No advanced unit tests (20+ scenarios)
- No integration test suite (manual E2E validation)
- Deferred: TS-3.2b, TS-3.6, TS-3.7

**Security (Advanced):**
- No MFA / risk scoring
- No passwordless (FIDO2, magic link, push)
- No B2B external access workflows
- No delegated administration with scope constraints
- Deferred: FS-09, FS-10, FS-14

**Compliance:**
- No document upload
- No expiration notifications
- No access enforcement (suspension/revocation)
- No role promotion / maturity tracking
- Deferred: FS-11, FS-12, FS-15, FS-16, EP-07, EP-08

**Admin Features:**
- No diagnostics dashboard (system health metrics)
- No real-time monitoring widgets
- Deferred: TS-5.2, TS-5.5

---

## 9. EQUIPO & ROLES DURANTE MVP

### 4-Person Squad

| Rol | Person | Focus | Hours/Week |
|-----|--------|-------|-----------|
| **Team Lead** | 1 | Architecture, code reviews, mentoring, RLS design validation | 17.5h (50%) |
| **Backend Dev 1** | 1 (Semi-Senior) | Domain models, EF Core, API endpoints | 35h (100%) |
| **Backend Dev 2** | 1 (Semi-Senior, Rotating) | DBA tasks (Sprint 1-2), Security (TS-3.2), Config (TS-4.3), Middleware (TS-3.4) | 35h (100%) |
| **QA/Backend Dev 3** | 1 (Semi-Senior) | Authorization schema (TS-3.3), Frontend (TS-5.1), Integration tests (TS-1.6), health checks | 35h (100%) |

**Total Capacity:** 122.5h/week = **980h over 8 weeks**
**Sprint 0-3 Usage:** 1,100h (includes buffer)

---

## 10. PUNTOS DE VALIDACIÓN ARQUITECTÓNICA

### Pre-Sprint 1
- [ ] ADR-0048 walkthrough (closure table pattern) with DBA
- [ ] ADR-0039 walkthrough (XACML PDP) with team
- [ ] ADR-0047 walkthrough (config hierarchy) with config owner
- [ ] Composite PK strategy approved (all new tables MUST enforce)

### Sprint 1 Code Gates
- [ ] TS-1.1, 1.2 review: composite PK discipline checked
- [ ] TS-3.1 review: Policy structure aligns with ADR-0039
- [ ] TS-4.1 review: Scope hierarchy matches ADR-0047

### Sprint 2 Code Gates
- [ ] TS-3.2 code review: PDP rule matching logic (20+ scenarios manually tested)
- [ ] TS-4.3 code review: IsInheritable + IsEncrypted logic correct
- [ ] TS-1.3 + TS-1.4 integration: EF filters + ports/adapters working

### Sprint 3 Code Gates
- [ ] TS-1.6 tests: RLS isolation 100% (3 tenants, Layer 1+2 validation)
- [ ] TS-3.4 middleware: Authorization decisions enforced
- [ ] TS-5.1 + TS-1.5: Login flow E2E working

---

## 11. CRITERIOS DE DEFINICIÓN DE HECHO (MVP)

### EP-01 (Identity)
- [ ] All identity tables created with composite PK (id, root_tenant_id)
- [ ] EF Core global query filters block cross-tenant queries
- [ ] RLS integration tests isolate 3 tenants (Layer 1 + Layer 2)
- [ ] Login endpoint works (email + password)
- [ ] User registration includes email verification flow
- [ ] Org onboarding creates root tenant + admin user
- [ ] Audit columns populated on all operations

### EP-03 (Authorization MVP)
- [ ] Policy, Rule, Profile aggregates implemented
- [ ] PDP engine evaluates rules (first match, effect aggregation)
- [ ] Authorization middleware intercepts requests
- [ ] 5 endpoints functional: policy CRUD, profile assignment, decision check
- [ ] Cache hit rate logged
- [ ] Basic attribute resolution working (user role, system name, time of day)

### EP-04 (Configuration)
- [ ] ConfigurationParameter aggregate + domain service
- [ ] 4-level resolution hierarchy (Module → Suite → Tenant → Global)
- [ ] IsInheritable validation (prevent parent override)
- [ ] IsEncrypted flag handling (decrypt on retrieval)
- [ ] Redis caching with TTL + cascade invalidation
- [ ] 5 endpoints functional (CRUD + resolve)
- [ ] Integration tests: hierarchy, caching, isolation

### EP-05 (Experience MVP)
- [ ] Login page: email + password form, responsive, WCAG A accessible
- [ ] Error messages: invalid creds, locked, network errors
- [ ] Audit log endpoint: queryable with filters, paginated
- [ ] Health checks: /health + /health/ready returning UP/DOWN
- [ ] JWT authentication working with httpOnly cookies

---

## 12. RIESGOS MVP Y MITIGACIONES

| Riesgo | Probabilidad | Impacto | Mitigation |
|--------|-------------|---------|-----------|
| **TS-1.2 RLS delay** | ALTA | CRÍTICO (cascada 7 TS) | External DBA code review Day 1; Testcontainers setup early |
| **TS-3.2 PDP logic bug** | MEDIA | ALTO (security regression) | Dev2 pairs with TL; manual 20+ scenarios testing before Sprint 3 |
| **Dev2 skill gap (rotation)** | MEDIA | ALTO (rework, quality) | TL mentors TS-3.2, TS-4.3, TS-3.4; pair programming mandatory |
| **React learning (Dev3)** | BAJA | MEDIO (schedule slip) | TS-5.1 simplified (2 components, patterns-only, no custom logic) |
| **Post-MVP scope creep** | MEDIA | ALTO (timeline stretch) | Lock MVP scope now, defer any additions to Phase 2 |
| **Testing gaps** | MEDIA | MEDIO (bugs in prod) | TS-1.6 unit tests + manual E2E tests comprehensive |

---

## RESUMEN EJECUTIVO

### MVP Reducido (168 pts, 12 weeks, 4 personas)

**Entra:**
- Tenant & identity (55 pts)
- Authorization core (56 pts)
- Configuration (31 pts)
- Login page + audit (26 pts)
- **Total: 168 pts, 845h work, 1,225h with overhead = 10 weeks sprint time, 12 weeks wall-clock**

**Sale:**
- System Catalog (31 pts)
- MFA / Security advanced (163 pts)
- Compliance lifecycle (69 pts)
- IGA role promotion (111 pts)
- Diagnostics dashboard (13 pts)
- **Total deferred: 387 pts, ~1,530h work, 15+ weeks Post-MVP**

**Viability:**
- Realista con 4 personas × 7h/día
- 12 weeks viable timeline
- Core functionality complete (login, authorization, config)
- Quality gates built-in (code review, tests)
- Escalable: Post-MVP adds advanced features without rework

**Próximos Pasos:**
1. [ ] Validar alcance MVP con stakeholders (8 FS, 168 pts)
2. [ ] Confirmar deferred features no rompen roadmap producto
3. [ ] Revisar riesgos con team (RLS, PDP logic)
4. [ ] Iniciar Sprint 0 (hiring, setup, training)

---

**Aprobado por:** Principal Architect
**Fecha:** 2026-05-14
**Status:** **MVP REDUCIDO DEFINIDO, REALISTA Y VIABLE**

*Este MVP es completo funcional para demostración interna y tiene valor real para usuarios. Post-MVP agrega seguridad avanzada, compliance y automatización.*
