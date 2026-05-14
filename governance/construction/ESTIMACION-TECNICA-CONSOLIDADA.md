# Estimación Técnica Consolidada — UMS Construction

**Fecha:** 2026-05-14  
**Versión:** 1.0  
**Propósito:** Tabla consolidada de estimaciones por perfil, trazable a Gantt  
**Formato:** Ejecutivo, sin extensiones

---

## TABLA CONSOLIDADA DE ESTIMACIONES

### Leyenda
- **Perfil:** Rol involucrado (Backend, DBA, QA, Frontend, Security, DevOps, Architect)
- **Actividad:** Qué entrega ese perfil
- **Tiempo:** Horas estimadas (rango si hay incertidumbre)
- **Supuesto:** Constraint o premisa clave
- **Dependencia:** Blocker principal
- **Riesgo:** Principal fallo posible
- **Criterio:** Cómo se estimó (complejidad, alcance, patrón conocido)
- **Justificación:** Razón técnica concisa

---

## EP-01: TENANT & IDENTITY (MVP)

| FS | TS | Sprint | Perfil | Actividad | Tiempo | Supuesto | Dependencia | Riesgo | Criterio | Justificación | Entregable |
|----|----|----|--------|-----------|--------|----------|-------------|--------|----------|---------------|-----------|
| **FS-01,02,03** | **TS-1.1** | 1 | Backend (DDD) | Tenant aggregate + TenantClosure pattern | 40h | ADR-0048 closure table pattern conocido | — | Modelado incompleto de closure | Patrón estándar DDD + closure table | Aggregate simple (4 props) + 1 value object + 2 eventos. No lógica compleja. | Tenant.cs, TenantClosure.cs, domain events |
| | **TS-1.2** | 1 | DBA | Schema: identity.tenants, users, tenant_closure con PK compuesto + índices + partición root_tenant_id | 48h | SQL Server 2022 RLS features activos | TS-1.1 (schema) | Índices insuficientes en closure queries | Schema estándar + 4 tablas + 8 índices + partición 1 nivel | 4 tablas, composite PK enforcement, 8 índices (tenant_id, user_id, closure queries), partición en root_tenant_id por ADR-0049. Complejo pero patrón conocido en SQL. | [identity].* schema + indices + partition function |
| | | 1 | Backend (EF Core) | EF Core DbContext mapping, migrations, navigation properties | 24h | EF Core 8 lazy loading disabled | TS-1.2 (schema) | Migrations desincronizadas | Mapping directo 1:1 a schema | 4 DbSets, composite key fluent config, 10 audit columns mapping, 3 navigation properties. Mecánica repetitiva. | DbContext.cs, migrations |
| | **TS-1.3** | 1-2 | Backend (.NET) | ICurrentTenantResolver (scope service), ModelBuilder global filters para todos los DbSets | 32h | TenantId siempre en claims/header | TS-1.2 (DbContext) | TenantId nulo → bypass | Global filter pattern (EF Core standard) | Intercepción en 50+ DbSets. Sin lógica condicional. Repetitivo pero crítico. | ICurrentTenantResolver.cs, model config, unit tests (5 tests) |
| | **TS-1.4** | 2 | Backend (DDD) | UserRegistrationDomainService, Ports: IPasswordHasher, IEmailService, IUserRepository; Adapters: Bcrypt, Sendgrid, SqlServerRepo | 40h | Sendgrid API key disponible, Bcrypt 12 rounds | TS-1.3 (tenant context) | Email delivery falla | Puertos/adaptadores estándar DDD + crypto | 3 puertos, 3 adaptadores, 1 domain service (validation + event raise). Patrones conocidos. | UserRegistrationDomainService.cs, 3 ports/adapters |
| | **TS-1.5** | 2 | Backend (API) | POST /auth/login, /auth/register, /tenants endpoints + FluentValidation + error handling | 32h | ASP.NET Core 8 setup base | TS-1.4 (domain) | Rate limiting no implementado | API estándar REST (3 endpoints, validación, error codes) | 3 endpoints, FluentValidator por endpoint (3), response models (3), error handling 400/409/500. Mecánico. | 3 controller actions, validators, response models |
| | **TS-1.6** | 3 | QA | Integration tests: 3 tenants isolated, Layer 1 (EF filter), Layer 2 (direct SQL blocked by RLS), failover, audit | 56h | LocalDB o test SQL Server, 3 tenants fixture | TS-1.2, TS-1.3 (RLS completo) | RLS predicates no aplicadas | Test scenarios (3 tenants, 4 verify points, 100% RLS coverage) | Fixture: 3 tenants + 6 usuarios (2 per tenant). 4 test methods (Layer 1 isolation, Layer 2 bypass blocked, failover, audit). 100% coverage crítica (CSF). | xUnit integration tests, fixtures, RLS validation |
| | **TS-1.2+1.3** | 1-3 | Backend (DDD) | Code review + advisory | 16h | Equipo en lugar | — | Revisión superficial | Oversight arquitectónico | Validation de PK composite discipline, ICurrentTenantResolver en todos los DbSets, partition key alignment | Review notes, PR feedback |

**EP-01 Totales:**
- **Total TS-1.1 a 1.6:** 288h
- **Por perfil:** Backend DDD 80h, DBA 48h, Backend EF 24h, Backend API 32h, QA 56h, Backend Arch 16h, Otros 32h
- **Total FS-01, FS-02, FS-03:** 288h (TS reutilizados en 3 FS)
- **Total EP-01:** 288h (50 story points: 5.7h/pt)

---

## EP-02: SYSTEM CATALOG (MVP)

| FS | TS | Sprint | Perfil | Actividad | Tiempo | Supuesto | Dependencia | Riesgo | Criterio | Justificación | Entregable |
|----|----|----|--------|-----------|--------|----------|-------------|--------|----------|---------------|-----------|
| **FS-04** | **TS-2.1** | 1 | Backend (DDD) | System + SystemTopology aggregates, SystemUrl, SystemType, EnvironmentType value objects + 2 eventos | 24h | Taxonomía de tipos de sistema (5-10) estable | EP-01 | Topology model incompleto | Aggregate simple (6 props) + 1 composite value object + 2 events | System aggregate: id, root_tenant_id, name, type, base_url, status. SystemTopology: env, instance_count, endpoints JSON. Sin lógica compleja. | System.cs, SystemTopology.cs, value objects |
| | **TS-2.2** | 1 | DBA | [console].systems, [console].system_topologies tablas + FK a tenants + índices + partición | 32h | SQL Server 2022 | TS-1.2 (RLS base) | FK constraints insuficientes | 2 tablas, FK (system_id, root_tenant_id) → tenants, 4 índices, partición root_tenant_id | 2 tablas, FK enforced (composite), 4 índices (tenant, system_name, topology queries), partición heredada de TS-1.2. | [console].* schema, FKs, indices |
| | | 1 | Backend (EF Core) | DbContext mapping + migrations | 16h | — | TS-2.2 (schema) | — | Mapping directo | 2 DbSets, FK fluent config, navigation, migrations | DbContext updates, migrations |
| | **TS-2.3** | 2 | Backend (DDD) | ISystemRepository port, SqlServerSystemRepository adapter, ITopologyAnalyzer service | 24h | — | TS-2.1, TS-2.2 (schema) | Repository query performance | Puertos/adaptadores + analyzer (simple queries) | Repository: 4 métodos (GetByIdAsync, ListByTenant, SaveAsync, DeleteAsync). Analyzer: 2 métodos (AnalyzeEnvironments, DetectHealth). Sin complejidad algorítmica. | SystemRepository.cs, TopologyAnalyzer.cs |
| | **TS-2.4** | 2 | Backend (API) | POST/GET /systems endpoints, PATCH topology, validation, OpenAPI docs | 24h | — | TS-2.3 (service) | — | API estándar (4 endpoints, validators, OpenAPI) | 4 endpoints (register, list, detail, update topology), FluentValidator (2), response models (3), OpenAPI. | System controller, validators, response models |
| | **TS-2.5** | 3 | QA | Integration tests: system isolation T1/T2, topology updates, validation rules, audit | 40h | — | TS-2.2 (schema) | — | Integration tests (4 scenarios, isolation + validation + audit) | 4 test methods: registration isolation, topology update reflected, validation rules (name unique per tenant, URL format), audit logged. | xUnit integration tests |

**EP-02 Totales:**
- **Total TS-2.1 a 2.5:** 160h
- **Por perfil:** Backend DDD 48h, DBA 32h, Backend EF 16h, Backend API 24h, QA 40h
- **Total FS-04:** 160h (31 story points: 5.1h/pt)
- **Total EP-02:** 160h

---

## EP-03: AUTHORIZATION (MVP)

| FS | TS | Sprint | Perfil | Actividad | Tiempo | Supuesto | Dependencia | Riesgo | Criterio | Justificación | Entregable |
|----|----|----|--------|-----------|--------|----------|-------------|--------|----------|---------------|-----------|
| **FS-05,06,07** | **TS-3.1** | 1 | Backend (DDD, Security) | Policy + Rule + Profile + Permission aggregates, Condition value object, domain events (ADR-0039) | 56h | XACML pattern conocido, ADR-0039 leído | — | Modelado incompleto de condiciones | Policy aggregate: id, name, rules[], status. Rule: effect [ALLOW/DENY], conditions[], actions[], resources[]. Profile: id, name, policies[], assignments[]. Sin ejecutable, solo estructuras. | Policy.cs, Rule.cs, Profile.cs, Condition.cs, events |
| | **TS-3.2** | 2-3 | Backend (Security) | PDP (Policy Decision Point) + PAP (Policy Admin Point): Port IAuthorizationPolicyService (CRUD), Port IPolicyDecisionPoint (evaluate + explain), rule matching (first match), effect aggregation (ALLOW/DENY resolution), caching IAuthorizationCache en Redis (key: compiled_policy:v2:) | 56h | ADR-0039 (6-step pipeline) estudiado, Redis opcional pero incluido | TS-3.1 (domain) | Rule matching lógica incorrrecta, cache invalidation missing | PDP engine: rule matching (linear scan O(n)), effect aggregation, explanation generation. PAP: CRUD policies (4 métodos). Cache layer con TTL 5min. Algoritmo conocido, pero integración cache requiere cuidado. | IPolicyDecisionPoint.cs, IAuthorizationPolicyService.cs, caching logic, 20+ unit tests |
| | | 2-3 | Backend (EF Core) | Authorization DbContext mapping + migrations | 16h | — | TS-3.3 (schema) | — | Mapping directo | 4 DbSets, FK config, navigation, migrations | DbContext updates, migrations |
| | **TS-3.2b** | 2-3 | Backend (Security) | PIP (Policy Information Point): Port IAttributeRepository, resolve user/system/context attributes en runtime (tenure, role, delegation_scope, geo, device_reputation, system environment, risk_score, IP, network). Integración con TS-4.3 (config), TS-8.1 (IGA). Caching TTL 1h. | 56h | Fuentes de atributos disponibles (tenure logs, config, IGA store, risk engine), data access patterns estándar | TS-3.1, TS-4.3 (config para attrs), TS-6.2 (risk score) | Attribute resolution latency, missing source integration | PIP: 6 resolvers (user, system, context attributes). Cada resolver: 1-2 queries + cache check. Sin lógica compleja, pero calls distribuidas. Integration con 3 contextos. | IAttributeRepository.cs, 6 resolver implementations, caching |
| | **TS-3.3** | 1-2 | DBA | [authorization].policies, [authorization].rules, [authorization].profiles, [authorization].profile_assignments tablas + FK + índices + partición | 56h | SQL Server 2022 | TS-1.2 (RLS base) | índices missing, partition key mismatch | 4 tablas, FK compuesto (policy_id, root_tenant_id), 6 índices (tenant, user, policy queries), partición root_tenant_id | 4 tablas, rules/conditions en JSON column, FKs composite, 6 índices (tenant_id, user_id, profile queries), partición heredada. | [authorization].* schema, FKs, indices |
| | | 1-2 | Backend (EF Core) | DbContext mapping + migrations | 16h | — | TS-3.3 (schema) | — | Mapping directo | 4 DbSets, FK config, JSON navigation, migrations | DbContext updates, migrations |
| | **TS-3.4** | 2-3 | Backend (.NET) | Custom AuthorizationMiddleware en request pipeline: resolve user/tenant/system, call IAttributeRepository, call IPolicyDecisionPoint, enforce ALLOW/DENY, logging decision+time+cache hit/miss | 32h | ICurrentTenantResolver ya implementado (TS-1.3) | TS-3.2 (PDP), TS-3.2b (PIP) | Attribute resolution timeout, decision cache miss storms | Middleware: 5 steps (resolve context, gather attributes, PDP call, enforce, log). Sin custom logic, calls a ports. Performance-sensitive. | AuthorizationMiddleware.cs, logging config |
| | **TS-3.5** | 2 | Backend (API) | POST /authorization/policies, GET /policies, PATCH /policies/{id}, DELETE /policies/{id}, POST /profiles/{id}/assign, POST /authorization/check (test endpoint + response decision+explanation), FluentValidation, OpenAPI | 32h | — | TS-3.2 (PDP) | — | API estándar (6 endpoints, validators, test endpoint) | 6 endpoints, FluentValidator (3), response models (4), explanation payload, test endpoint devuelve decision+trace. | Authorization controller, validators, response models |
| | **TS-3.6** | 2-3 | Backend (Testing) | Unit tests para PDP: 20+ scenarios (simple ALLOW, DENY, AND conditions, OR conditions, attribute matching, wildcards, time-based hour matching, effect aggregation, missing attributes, malformed rules, circular logic, 1000 rules <100ms performance) | 56h | xUnit, NSubstitute, mocked repos | TS-3.2 (PDP logic) | Scenarios incompletos, performance regression | 20+ test methods, 3 mocked ports (IPolicyRepository, IAttributeRepository, IAuthorizationCache), assertions per scenario. 80% code coverage en PDP. | xUnit tests, ~400 LOC test code |
| | **TS-3.7** | 3 | QA | Integration tests: create policy (ALLOW read,write on System.A where role==Admin), create profile, assign to user, verify user allowed action, verify user denied action, test different attributes affect outcome, 100% happy path + error cases | 56h | — | TS-3.3 (schema), TS-3.2 (PDP) | — | Integration scenarios (4: policy creation, profile assignment, allow decision, deny decision) + edge cases (missing attrs, conflicting rules) | 4 main test methods + 3 edge case methods. Fixture: 1 policy + 1 profile + 2 users (1 allowed role, 1 denied). Full flow validation. | xUnit integration tests |

**EP-03 Totales:**
- **Total TS-3.1 a 3.7:** 456h
- **Por perfil:** Backend Security 112h, Backend DDD 56h, Backend EF 32h, DBA 56h, Backend API 32h, Backend Testing 56h, QA 56h, Arch 16h
- **Total FS-05, 06, 07:** 456h (89 story points: 5.1h/pt)
- **Total EP-03:** 456h

---

## EP-04: CONFIGURATION (MVP)

| FS | TS | Sprint | Perfil | Actividad | Tiempo | Supuesto | Dependencia | Riesgo | Criterio | Justificación | Entregable |
|----|----|----|--------|-----------|--------|----------|-------------|--------|----------|---------------|-----------|
| **FS-13** | **TS-4.1** | 1 | Backend (DDD) | ConfigurationParameter aggregate: id, key, value, type [STRING/INT/BOOL/JSON], scope [TENANT/SYSTEM/ENV]. ParameterKey, ParameterValue, ParameterType value objects. Domain events ConfigurationChangedEvent. Validation: key format, type coercion, scope constraints | 32h | Taxonomy de tipos estable (4 tipos), scope rules claros | — | Validación insuficiente de valores | Parameter aggregate: 5 props, 3 value objects, 1 domain service (validation), 1 event. Sin lógica compleja. | ConfigurationParameter.cs, value objects, events |
| | **TS-4.2** | 1 | DBA | [configuration].parameters, [configuration].parameter_history tablas + FK + índices + partición | 24h | SQL Server 2022 | TS-1.2 (RLS base) | History table insuficientemente indexada | 2 tablas, FK compuesto, 4 índices (key, scope, root_tenant_id, history queries), partición root_tenant_id | 2 tablas: parameters (current), parameter_history (audit trail de cambios). FKs composite, 4 índices (lookup por (key, scope, tenant) es O(1) expected). | [configuration].* schema, FKs, indices |
| | | 1 | Backend (EF Core) | DbContext mapping + migrations | 12h | — | TS-4.2 (schema) | — | Mapping directo | 2 DbSets, FK fluent, migrations | DbContext updates, migrations |
| | **TS-4.3** | 2 | Backend (DDD, Caching, Encryption) | IConfigurationService port, SqlServerConfigurationService adapter. Implementar: (1) 4-level hierarchy resolution (Module → Suite → Tenant → Global, "Closest Scope Wins"), (2) IsInheritable flag validation (prevent override of parent non-inheritable), (3) IsEncrypted flag handling (decrypt on retrieval, via Vault), (4) Redis caching (key: config:v2:{tenant}:{system}:{module}:{key}, TTL 5min), (5) cascade invalidation on parameter mutation, (6) feature flags support (bool + JSON complex). ADR-0047 full implementation. | 56h | Redis disponible (Valide con DevOps), Vault para secrets disponible, IsEncrypted/IsInheritable columns en schema TS-4.2 | TS-4.1 (domain), TS-4.2 (schema) | Cascade invalidation incompleto, encryption key mismatch | Hierarchical resolution: 4-scope query + merge logic, IsInheritable validation (1 check), IsEncrypted decryption (1 call), caching (Redis ops: get/set/del), invalidation (scan cached keys + del). Sin algoritmo complejo, pero integración de 5 concerns. | IConfigurationService.cs, SqlServerConfigurationService.cs, caching+encryption logic |
| | **TS-4.4** | 2 | Backend (API) | POST /configuration/parameters, GET /parameters, PATCH /parameters/{id}, DELETE /parameters/{id}, GET /configuration/resolve (resolution endpoint), FluentValidation, OpenAPI | 20h | — | TS-4.3 (service) | — | API estándar (5 endpoints, validators, OpenAPI) | 5 endpoints (CRUD + resolve), FluentValidator (2), response models (3). Resolve endpoint returns resolved value + scope trail. | Configuration controller, validators, response models |
| | **TS-4.5** | 3 | QA | Integration tests: set param at TENANT level, override at SYSTEM level, verify resolution uses SYSTEM value (not TENANT), cache hit on 2nd resolve, param update clears cache + fresh resolve, T1 params not visible to T2 | 40h | — | TS-4.2 (schema), TS-4.3 (resolution logic) | Cache state not isolated | 5 test methods: hierarchy resolution (3 levels), cache verification (get+hit), invalidation (update+clear), isolation (2 tenants) | xUnit integration tests, cache/scope fixtures |

**EP-04 Totales:**
- **Total TS-4.1 a 4.5:** 184h
- **Por perfil:** Backend DDD 32h, DBA 24h, Backend EF 12h, Backend (Config/Cache) 56h, Backend API 20h, QA 40h
- **Total FS-13:** 184h (31 story points: 5.9h/pt)
- **Total EP-04:** 184h

---

## EP-05: EXPERIENCE & DIAGNOSTICS (MVP)

| FS | TS | Sprint | Perfil | Actividad | Tiempo | Supuesto | Dependencia | Riesgo | Criterio | Justificación | Entregable |
|----|----|----|--------|-----------|--------|----------|-------------|--------|----------|---------------|-----------|
| **FS-08** | **TS-5.1** | 2 | Frontend (React/TS) | Hosted login page React component (apps/web/src/pages/Auth/Login.tsx): form (email + password inputs), client-side validation (Zod), error messages (invalid creds, locked, not verified, network), remember-me checkbox, responsive desktop+mobile (Tailwind), accessibility WCAG 2.1 AA, password reset link, tenant context from URL/query param, integration POST /auth/login (TS-1.5), error handling (network timeouts, 401/403). | 56h | React 18+, TypeScript, Vite, Zustand, Zod setup base disponible | EP-01 (auth endpoint) | Accessibility compliance insufficient, form state management complex | React form: 1 component, 2 inputs, 1 checkbox, 1 link. Zustand store para form state. Zod schema. Error UI (4 error types). Tailwind responsive. Sin lógica de negocio. | Login.tsx, form state store, Zod schema, Tailwind config |
| | **TS-5.2** | 3 | Frontend (React/TS, Charting) | Diagnostics dashboard React component (apps/web/src/pages/Admin/Diagnostics.tsx): widgets (tenant count, user count, active sessions, EF Core filter health, DB connection pool stats, audit events last 100, auth cache hit rate, API response time p50/p99). Charts: response time trends 24h (Recharts). Real-time updates TanStack Query polling (5s). Admin-only access (401/403). Data sources: GET /health, /audit/logs, /metrics. Performance: load <1s, metrics update <2s. | 56h | Recharts, TanStack Query, GET endpoints (TS-5.3, TS-5.4) disponibles | TS-1.6 (tests), TS-3.6 (PDP tests), TS-4.5 (config tests) para datos | Polling storms (5s * N users), data freshness gaps | Dashboard: 8 widgets (most simple text + number), 2 charts (line: 24h response times). TanStack Query polling config. Admin guard (JWT role check). Sin lógica, agregación de datos remotos. | Diagnostics.tsx, chart configs, polling config, useAdmin hook |
| | **TS-5.3** | 2 | Backend (API) | GET /audit/logs (paginated, filterable): filters (user_id, event_type, resource, date_range), response (audit event + details before/after), 100% audit completeness (CSF), searchable (ElasticSearch opcional, SQL fulltext OK MVP) | 24h | Audit schema ya definido (10 columns), eventos ya logged en otras historias | Cross-cutting audit context | ElasticSearch not MVP-ready | 1 endpoint, 4 filters, pagination (limit+offset), response model (10 fields), fulltext query (CONTAINS en SQL). Sin complejidad lógica. | AuditLog controller, response models, fulltext index |
| | **TS-5.4** | 1 | Backend (.NET) | /health endpoint (liveness), /health/ready (readiness): checks DB connection, RLS, cache (if used), external services. Response: UP/DOWN status + dependencies. Kubernetes-compatible (200 UP, 503 DOWN). | 16h | HealthChecks.Extensions NuGet disponible | All infrastructure stories | Dependency check too slow | HealthCheck middleware: 5 checks (DB, RLS SELECT 1, cache PING, external services timeout 2s). Sin lógica. Patrón estándar. | HealthCheckMiddleware.cs, health config |
| | **TS-5.5** | 3 | QA | Integration tests: login with valid/invalid creds, diagnostics dashboard loads with correct metrics, audit log querying (filters work), health check endpoint UP, performance: login <500ms, diagnostics load <1s | 32h | — | TS-5.1, TS-5.2 (frontend), TS-5.3, TS-5.4 (endpoints) | Performance regression (>500ms login) | 4 main test methods (login happy + error, diagnostics load, audit query, health check) + 1 perf test (assert <500ms, <1s). Playright for E2E or xUnit for API. | Playwright tests (E2E) or xUnit (API), perf assertions |

**EP-05 Totales:**
- **Total TS-5.1 a 5.5:** 184h
- **Por perfil:** Frontend (React) 112h, Backend API 24h, Backend (.NET) 16h, QA 32h
- **Total FS-08:** 184h (47 story points: 3.9h/pt)
- **Total EP-05:** 184h

---

## TOTALES MVP (EP-01 a EP-05)

| Épica | Total Hours | Story Points | h/pt | Sprints | FTE (6.25) | Weeks |
|-------|-------------|--------------|------|---------|-----------|-------|
| **EP-01** Tenant & Identity | 288h | 55 pts | 5.2 | 1-2 | — | — |
| **EP-02** System Catalog | 160h | 31 pts | 5.2 | 1-2 | — | — |
| **EP-03** Authorization | 456h | 89 pts | 5.1 | 2-3 | — | — |
| **EP-04** Configuration | 184h | 31 pts | 5.9 | 2-3 | — | — |
| **EP-05** Experience | 184h | 47 pts | 3.9 | 2-3 | — | — |
| **TOTAL MVP** | **1,272h** | **253 pts** | **5.0** | **1-3** | **6.25 FTE** | **6-7 weeks** |

**Validación MVP:**
- 1,272h ÷ 6.25 FTE ÷ 40h/semana = 5.1 semanas (alineado a 6-7 weeks con buffer)
- Velocidad: 253 pts ÷ 6 weeks ÷ 6.25 FTE = 6.7 pts/FTE/week (realista)
- **Status:** ✅ VIABLE

---

## POST-MVP RESUMEN POR ÉPICA

| Épica | Total Hours (est.) | Story Points | h/pt | FTE | Weeks |
|-------|-------------------|--------------|------|-----|-------|
| **EP-06** Security, MFA, B2B, Delegation | 680h (est.) | 150 pts | 4.5 | 1.5-2 | 10-11 |
| **EP-07** Compliance Lifecycle | 330h (est.) | 69 pts | 4.8 | 0.8-1 | 8-9 |
| **EP-08** IGA Role Promotion | 480h (est.) | 111 pts | 4.3 | 1.2-1.5 | 9-10 |
| **TOTAL POST-MVP** | **1,490h** | **330 pts** | **4.5** | **7.75 FTE** | **8-10 weeks** |

**Validación Post-MVP:**
- 1,490h ÷ 7.75 FTE ÷ 40h/semana = 4.8 semanas teórico, pero dominios complejos → 9-10 weeks estimadas
- Velocidad esperada más baja (XACML, compliance engines, delegation state machines = mayor complejidad)
- **Status:** ✅ VIABLE con Security specialist (1.5 FTE post-MVP)

---

## TOTALES PROGRAMA COMPLETO

| Fase | Hours | Points | FTE | Weeks |
|------|-------|--------|-----|-------|
| **MVP (EP-01 a EP-05)** | 1,272h | 253 pts | 6.25 | 6-7 |
| **Post-MVP (EP-06 a EP-08)** | 1,490h | 330 pts | 7.75 | 8-10 |
| **TOTAL PROGRAMA** | **2,762h** | **583 pts** | **~7 avg** | **14-17 weeks** |
| **TOTAL PERSON-WEEKS** | **69 semanas** | — | — | — |

---

## OBSERVACIONES EJECUTIVAS PARA PLANIFICACIÓN

### 1. HISTORIAS CRÍTICAS (Ruta Crítica)

**MVP Ruta Crítica:**
- TS-1.2 (RLS schema, 48h) → TS-1.3 (EF filters, 32h) → TS-3.2 (PDP, 56h) → TS-3.4 (middleware, 32h)
- **Path:** 168h en cadena, Semana 1-2 obligatorio
- **Bloqueador:** Sin TS-1.2 (RLS), todo EP-03 está bloqueado
- **Riesgo:** Si TS-1.2 se retrasa, Pierce TS-1.3, TS-3.2, TS-3.4 (crítico para MVP)

**Post-MVP Ruta Crítica:**
- TS-6.2 (Risk Scoring, 56h) → TS-6.3 (Passwordless, 56h) → TS-6.11 (Tests, ~56h)
- **Path:** 168h en cadena, Semana 8-10 (Sprint 3-4)
- **Bloqueador:** Sin TS-6.2, MFA incompleto

### 2. DEPENDENCIAS BLOQUEANTES

| Blocker | Bloqueado | Sprint | Impacto |
|---------|-----------|--------|---------|
| **TS-1.2 (RLS schema)** | TS-1.3, TS-2.2, TS-3.3, TS-4.2, TS-6.4, TS-7.4, TS-8.5 (todas las schemas) | 1 | Si retrasa 1 semana, todo schema impactado |
| **TS-1.3 (EF filters)** | TS-1.4, TS-3.2, TS-4.3, TS-6.1 (tenant context) | 1-2 | Sin filtros EF, queries retornan datos incorrectos |
| **TS-3.2 (PDP)** | TS-3.4 (middleware), TS-6.8 (delegation scope check), TS-8.3 (impact analysis) | 2-3 | Sin PDP, authorization no funciona; bloquea EP-06 completamente |
| **TS-4.3 (Config service)** | TS-3.2b (attribute config), TS-6.1 (MFA thresholds), TS-8.2 (eligibility thresholds) | 2-3 | Sin config resolver, no hay parametrización tenant |
| **TS-7.3 (Notification engine)** | TS-8.7 (eligibility notifications) | 3 | Si TS-7.3 no completa antes de Sprint 3, TS-8.7 duplica código (riesgo) |

### 3. RIESGOS PRINCIPALES

| Riesgo | Probabilidad | Impacto | Mitigation |
|--------|-------------|---------|-----------|
| **TS-1.2/1.3 RLS implementation** | MEDIA | ALTO (bloquea MVP) | Code review de DBA/Architect en día 1; tests integración en paralelo TS-1.6 |
| **TS-3.2 PDP rule matching logic** | MEDIA | ALTO (autorización) | Unit tests 20+ scenarios ANTES de TS-3.2b; mock-first testing |
| **TS-3.2b PIP attribute resolution latency** | MEDIA | MEDIO (performance) | Caching desde día 1; load test con 1000 atributos/seg |
| **TS-4.3 cascade invalidation** | MEDIA | MEDIO (data stale) | Cache key strategy documentado antes de coding; test invalidation paths |
| **TS-6.2 risk scoring ML baseline** | MEDIA-ALTA | MEDIO (false positives) | Usar stats simples MVP, ML post-product; threshold validation con business |
| **TS-5.1 accessibility WCAG AA** | BAJA | MEDIO (legal) | Automated checks (axe) en CI; manual audit en Sprint 3 |
| **TS-8.3 permission explosion detection** | MEDIA | MEDIO (security) | Risk thresholds validados con security team; test 10+ conflict scenarios |
| **Post-MVP team ramp-up (Security specialist)** | MEDIA | ALTO (EP-06 velocity) | Hire antes de Sprint 3; onboarding Sprints 1-2 con mentoring |

### 4. HISTORIAS PARALELIZABLES

**Sprint 1 Parallelizable (7 equipos):**
```
Team 1 (Backend DDD):   TS-1.1, TS-2.1, TS-3.1, TS-4.1  (domain models)
Team 2 (DBA):          TS-1.2, TS-2.2, TS-3.3, TS-4.2   (schemas en paralelo)
Team 3 (Backend API):  TS-1.5, TS-2.4, TS-4.4, TS-5.4   (APIs simples)
Team 4 (Backend EF):   TS-1.3 (DbContext mapping)
Team 5 (Security):     —— (light involvement, TS-3.1 review)
Team 6 (QA):          Fixture setup, test scaffolding
Team 7 (Frontend):    —— (waiting for TS-1.5 endpoint)
```
**Result:** 7 historias paralelas en Sprint 1, 0 conflictos, todas completan Week 1

**Sprint 2-3 Parallelizable:**
```
Layer 1 (APIs):        TS-1.5, TS-2.4, TS-3.5, TS-4.4, TS-5.3
Layer 2 (PDP engine):  TS-3.2, TS-3.2b (pueden solaparse con cuidado: TS-3.2b usa IAttributeRepository port definido en TS-3.2)
Layer 3 (Middleware):  TS-3.4 (espera TS-3.2, pero UI puede avanzar)
Layer 4 (Frontend):    TS-5.1, TS-5.2 (esperan endpoints TS-1.5, TS-5.3, TS-5.4)
Layer 5 (Config):      TS-4.3 (espera TS-4.1 domain, TS-4.2 schema: OK paralelo)
```
**Result:** 5-6 equipos paralelos, 2-3 dependencias gating, 1-2 semana compresión posible

### 5. HISTORIAS RECOMENDADAS MVP vs POST-MVP

**RECOMENDADO MVP (253 pts, 6-7 semanas):**
- ✅ **EP-01** Tenant & Identity (55 pts) — Foundation, **CRÍTICA**
- ✅ **EP-02** System Catalog (31 pts) — Lightweight, **RECOMENDADA**
- ✅ **EP-03** Authorization (89 pts) — Core security, **CRÍTICA**
- ✅ **EP-04** Configuration (31 pts) — Enabler para TS-3.2b + TS-8.2, **RECOMENDADA**
- ✅ **EP-05** Experience (47 pts) — UI + diagnostics, **RECOMENDADA**

**RECOMENDADO POST-MVP (330 pts, 8-10 semanas):**
- ✅ **EP-06** MFA, B2B, Delegation (150 pts) — Require Security specialist + DDD engineers
- ✅ **EP-07** Compliance (69 pts) — Background services, document handling
- ✅ **EP-08** IGA Promotion (111 pts) — Complex state machines + impact analysis

**No mover a POST-MVP si quieres MVP verdadero:** Todas las épicas MVP son críticas para product demo.

---

## DESGLOSE POR PERFIL (FULL PROGRAM)

| Perfil | MVP (h) | Post-MVP (h) | Total | FTE Weeks | Criticidad |
|--------|---------|--------------|-------|-----------|------------|
| **Backend (DDD)** | 280 | 380 | 660 | 16.5 w | CRÍTICA |
| **Backend (Security)** | 80 | 360 | 440 | 11 w | CRÍTICA (+Post) |
| **Backend (API/Infra)** | 160 | 200 | 360 | 9 w | ALTA |
| **DBA** | 200 | 120 | 320 | 8 w | CRÍTICA (MVP early) |
| **QA** | 256 | 280 | 536 | 13.4 w | ALTA |
| **Frontend (React)** | 112 | 120 | 232 | 5.8 w | MEDIA |
| **DevOps** | 40 | 40 | 80 | 2 w | MEDIA |
| **Architect** | 80 | 80 | 160 | 4 w | MEDIA |
| **TOTAL** | 1,272 | 1,490 | 2,762 | 69 w | — |

---

## SPRINT ROADMAP (SUGERIDO)

### MVP Phase (Sprints 1-3)

**Sprint 1 (Week 1-2):**
- Focus: Foundation (EP-01 domain + schema, EP-02 domain, EP-03 domain, EP-04 domain)
- Start: TS-1.1, TS-1.2, TS-2.1, TS-2.2, TS-3.1, TS-4.1
- Parallel: TS-1.1, TS-2.1, TS-3.1, TS-4.1 (4 DDD engineers)
- Capacity: ~180h consumed (~35 pts), 6.25 FTE available
- Blocker: TS-1.2 must complete for TS-1.3, TS-3.3, TS-4.2
- **Deliverable:** 4 domain models, 4 schemas, all mapped to EF Core

**Sprint 2 (Week 3-4):**
- Focus: Core logic (TS-3.2 PDP, TS-3.2b PIP, TS-4.3 Config, TS-1.3 EF, TS-1.4 Ports/Adapters)
- Start: TS-1.3, TS-1.4, TS-3.2, TS-3.2b, TS-4.3, TS-2.3, TS-2.4
- Parallel: TS-3.2 (Security) + TS-3.2b (Security, pero usa port from TS-3.2), TS-4.3 (Backend config), TS-1.4 (Backend DDD)
- Capacity: ~200h consumed (~40 pts), parallelized
- Blocker: TS-3.2 must define IAttributeRepository interface (used by TS-3.2b)
- **Deliverable:** PDP engine, PIP resolvers, Config service, registration flow

**Sprint 3 (Week 5-7):**
- Focus: APIs, tests, UI (TS-1.5, TS-1.6, TS-3.4, TS-3.5, TS-3.6, TS-3.7, TS-4.4, TS-4.5, TS-5.1, TS-5.2, TS-5.3, TS-5.4, TS-5.5)
- Start: Everything left in EP-01-05
- Parallel: APIs (3 teams), tests (2 teams), UI (1 team), health checks (1 team)
- Capacity: ~400h consumed (~90 pts), fully parallelized
- Blocker: TS-1.2 must be complete (all schema infrastructure)
- **Deliverable:** All APIs, integration tests pass, login page + diagnostics UI, health checks live

### Post-MVP Phase (Sprints 4-6, Weeks 8-17)

**Sprint 4 (Week 8-10):**
- Focus: EP-06 + EP-07 domain/schema
- Start: TS-6.1, TS-6.2, TS-6.4, TS-6.5, TS-6.6, TS-7.1, TS-7.2, TS-7.4
- Onboard: Security specialist (1.5 FTE)
- Capacity: ~280h consumed (~60 pts)
- **Deliverable:** Risk scoring engine, MFA domain, Approvals domain, Compliance domain, Document storage

**Sprint 5 (Week 11-13):**
- Focus: EP-06 + EP-07 + EP-08 continued
- Start: TS-6.3, TS-6.7, TS-6.8, TS-7.3, TS-7.5, TS-8.1, TS-8.2, TS-8.3
- Capacity: ~320h consumed (~70 pts)
- **Deliverable:** Passwordless methods, B2B flow, Delegation state machine, Notification engine, Enforcement engine, Maturity calculator

**Sprint 6 (Week 14-17):**
- Focus: APIs, middleware, tests, finalization
- Start: TS-6.9, TS-6.10, TS-6.11, TS-6.12, TS-7.6, TS-7.7, TS-8.4, TS-8.5, TS-8.6, TS-8.7, TS-8.8, TS-8.9
- Capacity: ~350h consumed (~120 pts)
- **Deliverable:** All APIs, integration tests, delegation enforcement, compliance workflow, promotion workflow

---

## PUNTOS DE VALIDACIÓN ARQUITECTÓNICA

**Before Sprint 1:**
- [ ] ADR-0048 (Closure table) walkthrough with DBA + Backend
- [ ] ADR-0039 (XACML PDP) walkthrough with Security specialist
- [ ] ADR-0047 (Config hierarchy) validation with Config team
- [ ] RLS two-layer model (EF Core + SQL Server) clarity with Architect

**Sprint 1 Code Review Gates:**
- [ ] TS-1.1, TS-1.2: Composite PK discipline (all new tables MUST enforce)
- [ ] TS-1.3: ICurrentTenantResolver implementation (must be scoped, must fail-fast if TenantId null)
- [ ] TS-3.1: Policy model alignment with ADR-0039 (Rule structure, conditions shape)

**Sprint 2 Code Review Gates:**
- [ ] TS-3.2: PDP rule matching logic (unit tests 20+ scenarios MUST pass before merge)
- [ ] TS-3.2b: PIP caching TTL strategy (must validate with Performance team)
- [ ] TS-4.3: IsInheritable + IsEncrypted logic (integration tests MUST cover all inheritance paths)

**Sprint 3 Code Review Gates:**
- [ ] TS-1.6: RLS integration tests (100% coverage, MUST isolate 3 tenants correctly)
- [ ] TS-3.7: Authorization integration tests (MUST cover policy + profile + assignment + evaluation)

---

## CRITERIOS DE DEFINICIÓN DE HECHO (POR ÉPICA)

### EP-01 (Identity)
- [ ] All tables created with composite PK (id, root_tenant_id)
- [ ] EF Core global query filters applied to all DbSets
- [ ] RLS integration tests isolate 3 tenants (Layer 1 + Layer 2)
- [ ] Login endpoint works end-to-end (email + password)
- [ ] User registration with email verification flow complete
- [ ] Org onboarding creates root tenant + admin user
- [ ] Audit columns (10 standard) populated on all operations

### EP-03 (Authorization)
- [ ] Policy, Rule, Profile aggregates map to database
- [ ] PDP engine evaluates rules (first match, effect aggregation)
- [ ] PIP resolvers populate 6+ attributes (user, system, context)
- [ ] Authorization middleware intercepts requests
- [ ] 20+ unit test scenarios pass (PDP logic)
- [ ] Policy + Profile assignment + Permission evaluation work E2E
- [ ] Cache hit rate logged

---

**Documento Aprobado por:** Principal Architect  
**Fecha:** 2026-05-14  
**Status:** ✅ **CONSOLIDADO, LISTO PARA GANTT**

*Use esta tabla como source-of-truth para planificación de sprints, estimaciones, y timeline Gantt.*
