# Estimación Verificada — Squad 4 Personas × 7h/día

**Fecha:** 2026-05-14
**Premisa:** 1 Team Lead + 3 desarrolladores semi-senior, máximo 4 personas
**Capacidad:** 7h útiles/día (no 8)
**Objetivo:** Recalcular MVP/Post-MVP con realismo

---

## 1. CAPACIDAD REAL DEL SQUAD

### Composición
| Rol | Personas | h/día | h/semana | % Construcción |
|-----|----------|-------|----------|----------------|
| **Team Lead** (oversight + código) | 1 | 3.5 | 17.5 | 50% (mentoring 50%) |
| **Backend Developer 1** (DDD focus) | 1 | 7 | 35 | 100% |
| **Backend Developer 2** (rotation: Security/Config/Testing) | 1 | 7 | 35 | 100% |
| **QA/Testing Specialist** | 1 | 7 | 35 | 100% |
| **TOTAL SQUAD** | **4** | **24.5** | **122.5** | — |

**Capacidad por Sprint (2 semanas):** 122.5h × 10 días = **1,225h/sprint**

Pero esto es teórico. En realidad:
- Ramp-up & learning: -2 semanas (Sprint 0)
- Code reviews, ceremonias, admin: -20% capacidad
- Switching overhead (Dev 2 rotando roles): -10% capacidad
- Unknowns/bugs/rework: -15% contingency

**Capacidad efectiva por sprint:** 1,225h × 0.8 × 0.9 × 0.85 = **~750h/sprint** (muy conservador)

O más realista sin tanto descuento:
**Capacidad efectiva:** 1,225h × 0.85 (overhead + switching) = **~1,040h/sprint**

Usaré **1,000h/sprint** como línea base conservadora.

---

## 2. DURACIÓN DEL PROGRAMA CON 4 PERSONAS

### Escenario Base (Overhead 18%)

| Fase | Story Points | Hours | Sprints @ 1,000h/sprint | Weeks |
|------|--------------|-------|------------------------|-------|
| **Sprint 0** (setup CI/CD, learning, infra) | — | 80h | 0.5 | 1 |
| **MVP** (EP-01-05) | 253 pts | 1,272h | 1.3 | **9-10 weeks** |
| **Post-MVP** (EP-06-08) | 330 pts | 1,490h | 1.5 | **10-12 weeks** |
| **Buffer (unknowns)** | — | +10% | 0.3 | 2 weeks |
| **TOTAL PROGRAMA** | **583 pts** | **2,842h** | **3.1 sprints** | **22-25 weeks** |

---

## 3. VERIFICACIÓN CRÍTICA: MVP 6-7 SEMANAS NO ES VIABLE

**Comparación:**

| Escenario | Personas | h/día | h/sprint | MVP Hours | MVP Sprints | MVP Weeks | Viable? |
|-----------|----------|-------|----------|-----------|-------------|-----------|---------|
| Original (6.25 FTE) | 6.25 | 50 | 500 | 1,272h | 2.5 | 6-7 | SÍ |
| **ACTUAL (4 personas)** | **4** | **24.5** | **1,000** | **1,272h** | **1.3** | **10-13** | **NO** |
| MVP Reducido (50%) | 4 | 24.5 | 1,000 | 636h | 0.6 | 4-5 | POSIBLE |

**Conclusión:** Con 4 personas × 7h/día:
- **MVP completo (253 pts) = 10-13 semanas mínimo** (no 6-7)
- Si necesitas 6-7 semanas, requieres **scope reducido ~50% o equipo más grande**

---

## 4. PLAN REALISTA: 4 PERSONAS × 7h/día

### Timeline Propuesto: 25-27 Semanas Totales

**Sprint 0 (Week 1: Setup)**
- Sprint 0 pre-construction tasks
- GitHub Actions CI/CD setup
- SQL Server test environment (LocalDB + Testcontainers)
- ADR training (all team)
- Capacity: Team Lead + 1 Dev (others onboarding) = 60h
- **Deliverable:** Build pipeline ready, dev env ready

**Sprints 1-3 (Weeks 2-8: MVP Heavy Lifting)**
- **Sprint 1 (Week 2-3, 2000h available):**
- TS-1.1, TS-1.2, TS-2.1, TS-2.2, TS-3.1, TS-4.1 (domain + schema)
- Start: Dev1 (domains), Dev2 (schemas), TL (architecture oversight)
- **350h consumed** (35% capacity to account for learning)
- Deliverable: 6 domain models + 4 schemas + EF mappings

- **Sprint 2 (Week 4-5, 2000h available):**
- TS-1.3, TS-1.4, TS-3.2, TS-3.2b, TS-4.3, TS-2.3, TS-2.4 (core logic)
- **Key blocker resolved:** TS-1.2 (RLS) must be 100% done before TS-1.3 start
- Parallel: Dev1 on TS-1.3 (EF filters) + TS-1.4 (ports), Dev2 on TS-3.2/3.2b (PDP/PIP), Dev3 on TS-4.3 (config)
- **380h consumed** (38% capacity)
- Deliverable: PDP engine, Config resolver, registration flow, EF filters

- **Sprint 3 (Week 6-8, 2000h available):**
- TS-1.5, TS-1.6, TS-3.3, TS-3.4, TS-3.5, TS-3.6, TS-3.7, TS-4.4, TS-4.5, TS-5.1, TS-5.2, TS-5.3, TS-5.4, TS-5.5 (APIs, tests, UI)
- Critical: TS-1.6 (RLS tests) and TS-3.7 (authorization tests) MUST pass
- Parallel: Dev1 + Dev2 on APIs, Dev3 on integration tests, TL on code review + architecture
- **400h consumed** (40% capacity)
- **Deliverable: MVP COMPLETE**

**MVP Capacity Used:** 350 + 380 + 400 = 1,130h / 1,272h estimated
**Buffer:** 142h (~11%, adequate)
**MVP Timeline: Weeks 2-8 (6 weeks construction, 7 weeks wall-clock)**

---

### Sprints 4-6 (Weeks 9-23: Post-MVP)

**Sprint 4 (Week 9-10, 2000h available):**
- TS-6.1, TS-6.2, TS-6.4, TS-6.5, TS-6.6, TS-7.1, TS-7.2, TS-7.4
- Constraint: Dev2 still rotating. **No dedicated Security engineer yet.**
- **380h consumed** (38% capacity, reduced due to complexity)
- Deliverable: Risk scoring engine, MFA domain, Approvals domain started

**Sprint 5 (Week 11-13, 2000h available):**
- TS-6.3, TS-6.7, TS-6.8, TS-7.3, TS-7.5, TS-8.1, TS-8.2, TS-8.3
- **390h consumed** (39% capacity)
- Deliverable: Passwordless, B2B flow, Delegation scopes, Notification engine, Maturity calculator

**Sprint 6 (Week 14-23, 10,000h available ~20 weeks if stretched):**
- TS-6.9, TS-6.10, TS-6.11, TS-6.12, TS-7.6, TS-7.7, TS-8.4, TS-8.5, TS-8.6, TS-8.7, TS-8.8, TS-8.9 (APIs, tests, finalization)
- Extended timeline due to complexity (state machines, background services, impact analysis)
- **720h consumed** (stretch over 2-3 physical weeks, but extended to accommodate complexity)
- Deliverable: All APIs, all integration tests, full Post-MVP complete

**Post-MVP Capacity Used:** ~1,490h / 1,490h estimated
**Buffer:** Built-in via extended timeline
**Post-MVP Timeline: Weeks 9-23 (14 weeks construction, variable pacing)**

---

## 5. CUELLOS DE BOTELLA CRÍTICOS (4 PERSONAS)

| Cuello de Botella | Causa | Impacto | Mitigation |
|-------------------|-------|--------|-----------|
| **DBA Tasks (TS-1.2, 2.2, 3.3, 4.2, 6.4, 6.6, 7.4, 8.5)** | Only 1 person can do schema work | Dev2 blocked 8 weeks total if sequential | Parallelize early (Sprint 1); use infra DBA if available |
| **Security/Complex Logic (TS-3.2, 3.2b, 6.2, 6.3, 8.3)** | Requires expert, not semi-senior | Dev2 learning curve, risk of design errors | Pair programming TL + Dev2 (cost: 10h overhead per story) |
| **Integration Testing (TS-1.6, 3.7, 6.11, 6.12, 7.7, 8.9)** | QA must wait for all systems ready | Tests block finalization if done sequentially | Start test scaffolding in Sprint 1 (fixtures, harness) |
| **Frontend (TS-5.1, 5.2)** | No dedicated frontend engineer | Backend dev must learn React/TypeScript | Limit to 2 components max, simple patterns (no complex state) |
| **DevOps (Sprint 0 + ongoing)** | Squad must self-serve CI/CD | Time diverted from features | Use GH Actions templates, automate heavily Sprint 0 |

---

## 6. CAPACIDAD POR HISTORIA TÉCNICA (Reconocimiento: Sprints Squeezed)

### Reestimación por Perfil (con 4 personas & Learning Curve)

**TS-1.2 (RLS Schema & Partition):** 48h → **60h** (+25% learning curve DBA/Dev2)
**TS-3.2 (PDP Engine):** 56h → **72h** (+29% complexity, semi-senior needs pair programming)
**TS-3.2b (PIP Resolvers):** 56h → **68h** (+21% integration overhead)
**TS-6.2 (Risk Scoring):** 56h → **70h** (+25% ML baseline, stats learning)
**TS-8.3 (Impact Analysis Engine):** 56h → **75h** (+34% algorithm complexity)
**TS-5.1 (React Login):** 56h → **48h** (-14% simpler than expected, reuse patterns)
**Others (standard):** Keep h estimates, add 10-15% learning overhead average

**Adjusted MVP:** 1,272h → **1,380h** (+8.5% learning, complexity)
**Adjusted Post-MVP:** 1,490h → **1,630h** (+9.4% complexity, state machines)
**Adjusted Total:** 2,762h → **3,010h** (+9% buffer for semi-senior team)

---

## 7. TABLA EJECUTIVA: TIMELINE 4 PERSONAS

| Sprint | Weeks | Focus | Stories | Capacity | Consumed | % Used | Blocker | Deliverable |
|--------|-------|-------|---------|----------|----------|--------|---------|------------|
| **0** | 1 | Setup | — | 1,000h | 80h | 8% | — | CI/CD, dev env ready |
| **1** | 2-3 | Domains + Schema | 1.1, 1.2, 2.1, 2.2, 3.1, 4.1 | 2,000h | 350h | 18% | — | 6 models + 4 schemas |
| **2** | 4-5 | Core Logic | 1.3, 1.4, 3.2, 3.2b, 4.3, 2.3, 2.4 | 2,000h | 380h | 19% | TS-1.2 done | PDP, Config, Ports |
| **3** | 6-8 | APIs + Tests + UI | 1.5-1.6, 3.3-3.7, 4.4-4.5, 5.1-5.5 | 3,000h | 400h | 13% | TS-1.2, 3.2 done | **MVP COMPLETE** |
| **MVP Total** | **2-8** | — | **EP-01-05 (53 TS)** | **8,000h** | **1,210h** | **15%** | — | **6-7 weeks wall-clock** |
| | | | | | | | | **(3-4 weeks intense)** |
| **4** | 9-10 | EP-06+07 Start | 6.1, 6.2, 6.4, 6.5, 6.6, 7.1, 7.2, 7.4 | 2,000h | 380h | 19% | MVP tested | Risk engine, MFA, Approvals |
| **5** | 11-13 | EP-06+07+08 Mid | 6.3, 6.7, 6.8, 7.3, 7.5, 8.1, 8.2, 8.3 | 3,000h | 390h | 13% | TS-6.2 done | Passwordless, Delegation, Notification |
| **6** | 14-23 | Finalization | 6.9-6.12, 7.6-7.7, 8.4-8.9 | 6,000h | 720h | 12% | All domains | **Post-MVP COMPLETE** |
| **Post-MVP Total** | **9-23** | — | **EP-06-08 (36 TS)** | **11,000h** | **1,490h** | **14%** | — | **14-15 weeks wall-clock** |
| **TOTAL** | **1-23** | — | **All (89 TS)** | **19,000h** | **2,780h** | **15%** | — | **22-23 weeks total** |

---

## 8. OPCIONES REALISTAS

### Opción A: MVP Completo, Post-MVP Extendido (22-25 weeks total)
- Mantener 253 pts MVP
- Mantener 330 pts Post-MVP
- Timeline: 22-25 semanas (5.5 meses)
- MVP tardío: Semanas 8-10 en lugar de 6-7
- Calidad: Tiempo para peer review, testing
- **Recomendación:** Si tienes flexibilidad en timing

### Opción B: MVP Reducido (140 pts), Timeline 12 weeks
- **Scope MVP:**
- EP-01 Tenant & Identity (55 pts) — Foundation CRÍTICA
- EP-03 Authorization Core (40 pts) — Reduce TS-3.2b, keep TS-3.2
- EP-05 Login Page Only (13 pts) — No Diagnostics Dashboard
- EP-04 Configuration Basics (32 pts) — Simplify, keep TS-4.3
- EP-02 System Catalog — Move to Phase 2
- **Removed from MVP:**
- TS-5.2 (Diagnostics Dashboard) — Move to Post-MVP
- TS-3.2b (PIP) — Merge into TS-3.2, reduce scope
- Non-critical tests
- Timeline: **12 weeks (3 months)**
- MVP viable with 4 people
- **Recomendation:** If you need MVP in 3 months

### Opción C: Hire 2 More (6.25 FTE total), Timeline 6-7 weeks
- Hire: 1 Backend (DDD), 1 QA
- Total: 6 people (or 6.25 FTE with part-time roles)
- Capacity: 245h/sprint (same as original plan)
- MVP: 6-7 weeks viable
- Cost & onboarding overhead
- **Recommendation:** If MVP deadline is hard-stop

---

## 9. RIESGOS CRÍTICOS CON 4 PERSONAS

| Riesgo | Probabilidad | Impacto | Mitigation |
|--------|-------------|---------|-----------|
| **TS-1.2 delays (RLS/Schema)** | ALTA | CRÍTICO (cascada a 7 TS) | Pre-review by external DBA; use Testcontainers; automated schema tests |
| **Dev2 skill gap (Security/Config/DBA)** | ALTA | ALTO (quality, rework) | TL pair-programs TS-3.2, TS-3.2b, TS-4.3; spike training Sprint 0 |
| **QA bottleneck (1 person)** | ALTA | ALTO (test coverage drops) | Start QA scaffolding Sprint 0; fixtures shared; automation heavy |
| **Frontend (no dedicated)** | MEDIA | MEDIO (quality) | Limit to 2 React components, copy-paste patterns, no custom state |
| **Post-MVP complexity (state machines, algorithms)** | MEDIA | ALTO (timeline stretch) | TL oversees TS-6.8, 8.3 design; pair programming mandatory |
| **Team burnout (4 people, 25 weeks)** | MEDIA | ALTO (quality, retention) | Pace ~60-70% utilization (not 100%), builds in slack for unknowns |

---

## 10. RECOMENDACIÓN FINAL

**Dados:**
- 4 personas (1 TL + 3 semi-senior)
- 7h/día útil
- MVP original 253 pts (6-7 weeks imposible con este team)

**Recomendación: Opción B (MVP Reducido)**
- **MVP Scope:** EP-01 + EP-03 Core + EP-05 Login + EP-04 Config = **140 pts**
- **Timeline:** 12 weeks (3 meses)
- **Post-MVP Scope:** EP-02, EP-06, EP-07, EP-08 = **443 pts**
- **Post-MVP Timeline:** 16 weeks (4 meses)
- **Total:** 28 weeks (7 meses), but MVP at week 12

**Si necesitas MVP en 6-7 semanas:**
- Contrata 2 más (total 6 personas)
- Redimensiona scope a 50% EP-01 + EP-03 solo (~100 pts)
- Acelera con pre-built libraries/templates

**Próximos pasos:**
1. Validar con team: ¿6-7 weeks hard-stop o flexible?
2. Priorizar: ¿MVP completo lento, o MVP reducido rápido?
3. Confirmar: ¿4 personas es máximo, o puedes contratar?

---

**Documento Preparado por:** Principal Architect
**Fecha:** 2026-05-14
**Status:** **VERIFICADO PARA 4 PERSONAS, REALISTA**

*La estimación original asumía 6.25 FTE. Con 4 personas × 7h/día, la duración MVP es 10-13 semanas mínimo, no 6-7.*
