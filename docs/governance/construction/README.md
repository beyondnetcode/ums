# UMS Construction Planning — Documentation Index **Version:** 2.0 **Date:** 2026-05-14 **Status:** Ready for Construction — **MVP REDUCED (168 pts, 12 weeks, 4 people)**

**[Back to Main README](../../../README.md)**

Welcome to the **Construction** portal. Here you will find the documentation and resources required to understand and manage this area of the system.

**Language / Idioma:** Bilingual ES/EN

---

## START HERE / COMIENZA AQUÍ

### **Reduced MVP (MVP Reducido) — Primary Reference**
- **English:** [REDUCED-MVP-SCOPE-AND-ESTIMATION.md](REDUCED-MVP-SCOPE-AND-ESTIMATION.md)
- **Español:** [MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md](MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md)

**Contains / Contiene:** Scope (8 FS, 168 pts), timeline (12 weeks), team roles, deliverables by sprint

### **Estimation Index (Índice de Estimación)**
- **File / Archivo:** [ESTIMATION-INDEX.md](ESTIMATION-INDEX.md) — Master index of all estimation docs (Bilingual)
- **Contains / Contiene:** Reading paths by role, document version matrix, governance structure

---

## Directory Structure

```
/governance/construction/
├── README.md (this file — updated 2026-05-14)
├── ESTIMATION-INDEX.md (Master index, bilingual ES/EN)
│
├── [MVP REDUCIDO / REDUCED MVP]
├── MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md (ES)
├── REDUCED-MVP-SCOPE-AND-ESTIMATION.md (EN)
├── ESTIMATION-VALIDATION-MATRIX.md (EN/bilingual)
│
├── [TECHNICAL DETAILS / DETALLES TÉCNICOS]
├── ESTIMACION-TECNICA-CONSOLIDADA.md (ES)
├── ESTIMACION-VERIFICADA-4-PERSONAS.md (ES/EN)
├── TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md (EN/ES sections)
├── FS-TO-TS-MAPPING.md (EN/ES bilingual)
│
├── [ECONOMIC & STRATEGIC ANALYSIS / ANÁLISIS ECONÓMICO Y ESTRATÉGICO] NEW
├── [ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md](ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) (ES, executive summary for C-level)
├── [MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md](MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) (ES, strategic decision framework)
├── [JUSTIFICACION-TIMELINE-AI-DRIVEN.md](JUSTIFICACION-TIMELINE-AI-DRIVEN.md) (ES, honest breakdown of AI timeline)
│
├── [AUDITS & CORRECTIONS / AUDITORÍAS Y CORRECCIONES]
├── ADR-ESTIMATION-AUDIT.md (ES/EN)
├── CORRECTIONS-AMENDMENTS.md (ES/EN)
├── FIRMA-GOBERNANZA-MVP-REDUCIDO.md (ES, sign-off template)
├── GOVERNANCE-SIGN-OFF.md (EN, sign-off template)
│
├── /templates/ (story, PR templates)
└── /artifacts/ (burndowns, metrics — generated)
```

---

## Documents Overview (with MVP Reduced Focus)

### 1. **REDUCED MVP SCOPE & ESTIMATION** (Primary Reference — NEW 2026-05-14)

**Files:**
- **EN:** [REDUCED-MVP-SCOPE-AND-ESTIMATION.md](REDUCED-MVP-SCOPE-AND-ESTIMATION.md)
- **ES:** [MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md](MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md)

**What:** Complete scope definition for reduced MVP (168 pts, 8 FS, 58 TS) + sprint breakdown + team roles + DoD** Who:** Product owners, engineering leads, all stakeholders **When:** Use before Sprint 0 to approve scope + staffing **Key Sections:**

| Section | Purpose |
|---------|---------|
| **1. MVP Definition** | 8 functional stories, 58 technical stories in MVP; what's deferred |
| **2. Technical Stories** | Detailed breakdown by épica (EP-01, EP-03, EP-04, EP-05) |
| **3. Totals** | 168 story points, 845h work, 12-week timeline |
| **4. Sprint Breakdown** | Sprint 0 (setup), Sprint 1-3 (construction by theme) |
| **5. Deliverables** | What MVP can do, what's deferred to Post-MVP |
| **6. Team** | 4 persons (1 TL + 3 semi-senior), 7h/day |
| **7. Risks & Mitigations** | Critical risks (RLS, PDP logic, skill gaps) |
| **8. Definition of Done** | DoD per épica | **Key Metrics (MVP Reduced):**
- **Scope:** 168 story points (50% of original 336 pts program)
- **Functional Stories:** 8 of 16 (50% of product)
- **Technical Stories:** 58 of 89 (65% deferred to Post-MVP)
- **Timeline:** 12 weeks wall-clock (3 months)
- **Team:** 4 people (1 TL + 3 semi-senior)
- **Hours:** 845h work + 380h overhead = 1,225h total
- **Confidence:** MEDIUM-HIGH (70-75%)

**Use this for:** Sprint 0 kickoff, scope lock, team assignment, timeline commitment

---

### 2. **ESTIMATION VALIDATION MATRIX** (NEW 2026-05-14)

**File:** [ESTIMATION-VALIDATION-MATRIX.md](ESTIMATION-VALIDATION-MATRIX.md) (EN/bilingual)

**What:** Comprehensive validation of MVP reduced estimation; risk analysis, confidence levels, sign-off gates **Who:** Architects, tech leads, QA leads, risk owners **Key Contents:**

| Section | Purpose |
|---------|---------|
| **Scope Validation** | 8 FS, 168 pts, 12 weeks, 4 people |
| **h/pt Validation** | Hours-per-point ratio 5.0 consistent across all épicas |
| **Capacity Validation** | Team capacity 122.5h/week, conservative pacing (35% utilization) |
| **Dependency Validation** | Critical paths clear, no blocking bottlenecks |
| **Risk Validation** | High-risk stories (TS-1.2, TS-3.2, TS-4.3) with mitigations |
| **Quality Validation** | MVP testing standard (manual E2E, deferred automated suites) |
| **Confidence Levels** | Scope (HIGH), Hours (MEDIUM-HIGH), Timeline (MEDIUM) |
| **Sign-off Gates** | Pre-Sprint 1, Sprint 1-3 gates | **Confidence Summary:** **VALIDATION SUCCESSFUL** (70-75% confidence level)

---

### 3. **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md** (Reference — Complete Program)

**File:** [TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md](TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) (EN/ES sections)

**What:** Complete technical story breakdown for all 8 épicas + original team profiles (6.25 FTE MVP, 7.75 FTE Post-MVP)
**Who:** Engineering architects, tech leads, resource planning **When:** For deep-dive on all 89 TS (MVP + Post-MVP scope)
**Contents (PARTS 1-8):**

| Section | Purpose |
|---------|---------|
| **PART 1: TS by Épica** | 89 technical stories organized by épica (EP-01 to EP-08) |
| **PART 2: TS Summary Table** | Quick lookup: all stories, size, skills, dependencies |
| **PART 3: Team Profiles (8 roles)** | Original team roles, skills, story ownership |
| **PART 4: Team Composition Matrix** | Original MVP (6.25 FTE) + Post-MVP (7.75 FTE) |
| **PART 5: Skills Inventory** | Hiring gaps for original plan |
| **PART 6: Dependencies & Critical Paths** | Cross-épica dependencies |
| **PART 7: Effort Validation** | 578 pts total (original plan) |
| **PART 8: Sprint 0 Checklist** | Pre-construction tasks | **Note:** This document uses original 6.25 FTE team assumption. **See REDUCED-MVP for 4-person team replan.**

---

### 4. **FS-TO-TS-MAPPING.md** (Traceability)

**File:** [FS-TO-TS-MAPPING.md](FS-TO-TS-MAPPING.md) (EN/ES bilingual)

**What:** Maps each Functional Story (FS-01 to FS-16) to its Technical Stories (TS)
**Who:** Product owners, business analysts, QA, test leads **When:** Use to validate requirements coverage + test planning **Key Contents:**

| Section | Purpose |
|---------|---------|
| **FS by Épica** | All 16 FS with mapped TS for each |
| **Acceptance Criteria Alignment** | Which TS delivers each AC per FS |
| **Summary Table** | All 16 FS × TS coverage, story points, validation | **MVP FS Covered (8 of 16):**
- FS-01, 02, 03 (Identity)
- FS-05, 06, 07 (Authorization)
- FS-13 (Configuration)
- FS-08 partial (Login page, no diagnostics)

**Post-MVP FS Deferred (8 of 16):**
- FS-04, 09, 10, 11, 12, 14, 15, 16

---

### 5. **4-PERSON TEAM VERIFICATION** (NEW 2026-05-14)

**File:** [ESTIMACION-VERIFICADA-4-PERSONAS.md](ESTIMACION-VERIFICADA-4-PERSONAS.md) (ES/EN sections)

**What:** Recalculation of original plan for realistic 4-person team (1 TL + 3 semi-senior)
**Who:** Engineering leads, HR, budget owners **Key Finding:** Original MVP 6-7 week timeline NOT VIABLE with 4 people; requires 12 weeks **Contents:**

| Section | Purpose |
|---------|---------|
| **Capacity Real** | 4 people × 7h/day = 122.5h/week effective |
| **Timeline Recalc** | Original 6-7 weeks → 10-13 weeks actual |
| **3 Options** | A) Complete MVP slow (25 weeks total), B) Reduced MVP fast (12 weeks MVP), C) Hire more (6.25 FTE) |
| **Bottlenecks** | RLS schema, PDP logic, skill gaps identified |
| **Risks** | Team burnout, TS-3.2 complexity, QA bottleneck | **Recommendation:** **Option B — Reduced MVP (168 pts, 12 weeks)**

---

### 6. **TECHNICAL ESTIMATION CONSOLIDATED** (Reference Detail)

**File:** [ESTIMACION-TECNICA-CONSOLIDADA.md](ESTIMACION-TECNICA-CONSOLIDADA.md) (ES, with EN sections)

**What:** Story-by-story estimation breakdown for all 89 TS (original full program)
**Who:** Sprint planners, engineers, project managers **Contents:**

| Section | Purpose |
|---------|---------|
| **EP-01 to EP-08 tables** | Detailed breakdown: Perfil, Actividad, Tiempo, Supuesto, Dependencia, Riesgo |
| **Totals by Epic** | Points, hours, FTE, weeks |
| **Sprint Roadmap** | Suggested sprint allocation |
| **Observations** | Critical path, parallelization, Gantt preparation | **Note:** Uses original estimates. **For MVP Reduced details, see REDUCED-MVP.**

---

### 7. **ECONOMIC ANALYSIS & STRATEGIC DECISIONS** (NEW 2026-05-14)

#### **A. Cost/Benefit Analysis (Análisis de Costo/Beneficio)**

**File:** [ ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md](ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) (ES)

**What:** Complete economic breakdown for MVP 12-week project with 3 infrastructure scenarios **Who:** Finance leaders, Budget owners, C-level decision makers **When:** Use for budget approval, infrastructure selection, ROI justification **Key Contents:**

| Section | Purpose |
|---------|---------|
| **Costs by Profile** | 10 roles with monthly salary ranges (S/ 5K-16K) |
| **Management Costs** | PM, documentation, governance, validation (S/ 52.9K for 3 months) |
| **3 Infrastructure Scenarios** | On-Premise (S/ 193K), Hybrid (S/ 182.4K), Cloud-Native (S/ 191.2K) |
| **Cost Summary** | Total MVP costs by scenario; cost per week; cost per story point |
| **Benefits Analysis** | Year 1 revenue projection (50 clients × S/ 3K/mo = S/ 1.8M) |
| **ROI Calculation** | 84% ROI in Year 1 (S/ 1.8M revenue + S/ 400K savings - S/ 1.2M investment) |
| **Executive Recommendation** | Hybrid model: optimal balance of cost, scalability, and operational complexity | **Key Finding:** **RECOMMENDATION: HYBRID MODEL**
- Lowest MVP cost: S/ 182,350 (11% less than on-prem)
- Operational savings: Managed DB + auto-scaling (no DBA 60% overhead)
- Future-proof: Easy migration to cloud-native post-MVP
- Setup time: 1 week (vs 2 weeks on-prem, 3 days cloud-native but higher lock-in)

---

#### **B. Execution Model Comparison (Modelo de Ejecución)**

**File:** [ MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md](MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) (ES)

**What:** Strategic analysis comparing traditional Human team vs AI-Driven supervised model **Who:** C-level, CTO, strategic decision makers **When:** Use to decide execution approach (before team hiring/setup)
**Key Contents:**

| Dimension | Human Team (4 people) | AI-Driven Supervised |
|-----------|----------------------|---------------------|
| **Cost** | S/ 182,350 | S/ 141,000 (35% savings) |
| **Timeline** | 12 weeks | 8.5 weeks (25% faster) |
| **Team Size** | 4 full-time | 1 AI Architect + 1 TL + agents |
| **ROI (Year 1)** | 382% | 729% (2x improvement) |
| **Architectural Risk** | LOW (proven pattern) | MEDIUM (RLS/PDP validation critical) |
| **Quality** | HIGH (robust, creativity) | MEDIUM (hallucinations risk, 8-10% rework) |
| **Post-MVP Scalability** | Good (add team members) | EXCELLENT (agents parallelize) | **Key Finding:** **DUAL RECOMMENDATION**
- **PRIMARY:** AI-Driven model IF expert architect available (10+ yrs experience, 2+ yrs AI)
- S/ 41K cost savings + 3.5 week acceleration
- ROI doubles to 729%
- Risk mitigated with quality gates (SonarQube), external code review (RLS/PDP)
- **FALLBACK:** Human team model IF risk aversion is high
- More predictable, robust execution
- Proven track record, no hallucinations
- Still delivers MVP in 12 weeks @ S/ 182K

---

#### **C. AI Timeline Justification (Justificación de Timeline AI)**

**File:** [ JUSTIFICACION-TIMELINE-AI-DRIVEN.md](JUSTIFICACION-TIMELINE-AI-DRIVEN.md) (ES)

**What:** Detailed, honest breakdown of where the 8.5-week AI-Driven timeline comes from **Who:** Skeptics, decision makers, risk managers **When:** Use when questioning whether AI speedup is real or marketing hype **Key Contents:**

| Component | Hours | % of Total |
|-----------|-------|-----------|
| **Agent Code Generation** | ~100h | 12% |
| **Human Validation** | ~500h | 59% |
| **Architectural Design (human-only)** | ~36h | 4% |
| **Rework from Hallucinations** | ~85h | 10% |
| **Setup & Integration** | ~124h | 15% | **Key Finding:** **HONEST TRUTH ABOUT AI SPEED**
- Agents save ~30% COST (fewer people needed), not 70% TIME
- Validation overhead = 50% of total project (still human work)
- 8.5-week timeline comes from parallelization + team size, NOT agent speed alone
- Where agents EXCEL: REST endpoints (60% faster), React (50% faster)
- Where agents STRUGGLE: RLS schema (slower/equal), PDP logic (slower/equal), security (risky)

**Research-Backed Analysis:**
- GitHub Copilot study (2024): 55% faster WITH code review (actual net 35-45%)
- Anthropic internal metrics: Complex algorithms 10-20% SLOWER than human
- McKinsey (2024): Real-world 15-25% acceleration (NOT 50-75% as marketed)
- Y Combinator companies: 20-30% net faster (boilerplate 3-5x faster, architecture 0.9x)

**Bottom Line:** If you believe 8.5 weeks is realistic, validation overhead (not agent speed) is the reason. Agents excel at coding, humans must validate.

---

### 8. **ADR ESTIMATION AUDIT** (Validation Against Architecture)

**File:** [ADR-ESTIMATION-AUDIT.md](ADR-ESTIMATION-AUDIT.md) (ES/EN)

**What:** Verification that story estimates align with ADR complexity definitions **Who:** Architects, tech leads **Key Finding:** 5 stories identified as underestimated in original plan; adjustments applied **Adjustments Made:**
- TS-3.2: 21 → 26 pts (split to TS-3.2b)
- TS-4.3: 5 → 8 pts (+3 complexity)
- TS-8.2: 8 → 13 pts (+5 complexity)
- Total impact: +17 pts (~1 week Post-MVP)

---

### 8. **CORRECTIONS & AMENDMENTS** (2026-05-14)

**File:** [CORRECTIONS-AMENDMENTS.md](CORRECTIONS-AMENDMENTS.md) (ES/EN)

**What:** Critical corrections to original estimations (RLS model, frontend stack)
**Key Corrections:** 1. **Error #1: RLS Model**
- Was: SQL Server RLS as PRIMARY
- Now: EF Core filters as PRIMARY, SQL Server RLS optional Phase 2
- Impact: TS-1.2 reduced 13→8 pts, TS-1.3 increased 5→8 pts

2. **Error #2: Frontend Stack**
- Was: Razor Pages (server-side)
- Now: React (v18+) / Vite / Zustand / TanStack Query
- Impact: TS-5.1, TS-5.2 updated to React; Profile 6 updated

---

## IMPORTANT: Stack Corrections (2026-05-14)

**RLS Model:**
- **PRIMARY:** Application Layer using EF Core Global Query Filters (TS-1.3)
- **Optional Hardening:** SQL Server RLS via SESSION_CONTEXT (Phase 2)
- See: stack.md Section 6.1 (Multi-tenancy Strategy)

**Frontend Stack:**
- **Correct:** React (v18+) / Vite / Zustand / TanStack Query
- **NOT Razor Pages** (that's a misunderstanding)
- See: dotnet-migration-and-tech-stack-plan.md

---

## Quick Start: How to Use These Docs

### For **Product Owner / Scrum Master **1. Read: **FS-TO-TS-MAPPING.md** (2-3 min per épica)
- Understand what TS will deliver each FS
- Validate no functional requirements are missed

2. Read: **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md PART 4**
- See team composition for MVP + Post-MVP
- Know when to start hiring (Sprint 0 for QA + Security)

3. Action: Use PART 2 (TS Summary Table) for backlog import
- Copy TS into GitHub Projects / Jira
- Link TS to FS in your tracking system

---

### For **Engineering Lead / Tech Architect **1. Read: **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md PART 1**
- Understand all 89 technical stories
- Note dependencies (e.g., TS-1.2 RLS must complete before TS-1.3 interceptor)

2. Read: **PART 6 (Dependencies & Critical Paths)**
- Plan parallel work (TS-1.1 + TS-2.1 + TS-3.1 + TS-4.1 can start day 1)
- Identify blockers (TS-1.2 RLS blocks TS-1.3, TS-2.2, TS-3.3, TS-4.2)

3. Action: Create sprint plan using PART 7 (Effort Validation)
- Sprint 1-2 (MVP): allocate ~180-210 pts/sprint
- Sprint 3-5 (Post-MVP): allocate ~100-150 pts/sprint

---

### For **Hiring / HR** 1. Read: **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md PART 3 + PART 5**
- See 8 team roles + required skills
- See gap analysis: need 1 Senior Security engineer + 1 Mid QA engineer

2. Action: Start recruiting
- Backend (DDD): 2 engineers (may already have)
- Backend (Security): 1 engineer (need to hire)
- QA Automation: 1 engineer (need to hire)
- Others: DBA (existing?), API engineer (existing?), Frontend (flex), DevOps (flex)

3. Timeline: Hire by Sprint 0 end (week 1 before Sprint 1 starts)

---

### For **QA / Test Lead **1. Read: **FS-TO-TS-MAPPING.md** (full document)
- For each FS, see the integration test TS (e.g., FS-01 → TS-1.6 RLS Integration Tests)
- These are your test scenarios

2. Read: **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md PART 3 (QA Profile)**
- See story ownership: TS-1.6, TS-2.5, TS-3.7, TS-4.5, TS-5.5, TS-6.11, TS-6.12, TS-7.7, TS-8.9

3. Action: Create test plans
- Per épica, extract all "Integration Tests" TS
- Define test scenarios, data setup, assertions
- Example: TS-1.6 RLS Integration Tests → "User from T1 cannot query T2 data" scenario

---

### For **DevOps Engineer **1. Read: **TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md PART 1 (TS-1.2, TS-1.3, etc.)**
- Understand infrastructure needs: SQL Server RLS, partitioning, SESSION_CONTEXT
- Understand test infrastructure: LocalDB or test SQL Server

2. Read: **SERVICE-IMPLEMENTATION-PLAN.md** (in parent governance/ dir)
- See .NET 8 monorepo structure
- See GitHub Actions CI/CD setup details

3. Action: Sprint 0
- Set up GitHub Actions workflows (build, test, deploy)
- Set up SQL Server test environment (Testcontainers for integration tests)
- Set up monitoring (Application Insights, logging)

---

### For **Finance / Budget Owner / C-Level Executive **NEW

#### **Quick Decision: Cost & Infrastructure (15 min)**1. Read: **ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md sections 4-7**
- See 3 infrastructure scenarios (On-Prem S/ 193K vs Hybrid S/ 182K vs Cloud S/ 191K)
- See cost breakdown: Personnel (S/ 88.5K) + External (S/ 28.5K) + Management (S/ 52.9K) + Infrastructure (S/ 12.4-23K)
- See ROI projection: 84% Year 1 (S/ 1.8M revenue + S/ 400K savings)
- **ACTION:** Approve Hybrid infrastructure model (recommended)

#### **Strategic Decision: Execution Model (20 min)**2. Read: **MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md sections 1-3**
- See comparison table: Human (S/ 182K, 12 weeks, ROI 382%) vs AI-Driven (S/ 141K, 8.5 weeks, ROI 729%)
- See risk analysis: Human = LOW risk, AI-Driven = MEDIUM risk (mitigable)
- See executive recommendation: PRIMARY = AI-Driven (if architect available), FALLBACK = Human (if risk-averse)
- **ACTION:** Decide execution approach (Human vs AI-Driven)

#### **Credibility Check: Is AI Timeline Real? (10 min)**3. Read: **JUSTIFICACION-TIMELINE-AI-DRIVEN.md section 1-2**
- See honest breakdown: Agents 12% time, Human validation 59%, Setup 15%, Rework 10%
- See research: McKinsey 15-25% acceleration (NOT 50-75% as marketed)
- See truth: Agents save COST (30%), not TIME (validation overhead = 50% of work)
- **ACTION:** Understand AI timeline is credible BUT based on human validation, not agent speed

#### **Scope Confirmation (5 min)**4. Read: **REDUCED-MVP-SCOPE-AND-ESTIMATION.md section 1 ONLY**
- MVP = 168 pts (50% of product), 8 of 16 functional stories
- Post-MVP deferred = 417 pts (50% deferred to Phase 2)
- **ACTION:** Confirm scope lock with product owner

#### **Summary for C-Level Board Decision:**

| Decision | Recommendation | Cost | Timeline | ROI |
|----------|-----------------|------|----------|-----|
| **Infrastructure** | Hybrid (Azure SQL + App Service) | S/ 182,350 | 3 months | ROI 84% Year 1 |
| **Execution Model** | AI-Driven (if expert architect) OR Human (safe alternative) | S/ 141K or S/ 182K | 8.5 or 12 weeks | 729% or 382% |
| **MVP Scope** | Reduced (168 pts, 8 FS, core MVP) | Included above | Included above | Included above | **Bottom Line for Board:**
- **HYBRID + AI-DRIVEN = Best Case:** S/ 141K total cost, 8.5 weeks to MVP, 729% Year 1 ROI
- **HYBRID + HUMAN = Safe Case:** S/ 182K total cost, 12 weeks to MVP, 382% Year 1 ROI
- Both paths deliver core MVP (50% of product) by Q3 2026
- Post-MVP phase (remaining 50%) planned for Phase 2

---

## Related Documentation

### In `/governance/project/`

- **product-readiness-final-assessment.md** — Full readiness report, 100% of 8 épicas
- **ep-06-approvals-detailed-design.md** — Detailed design for Approvals context (MFA, B2B, Delegation)
- **ep-07-compliance-detailed-design.md** — Detailed design for Compliance context (docs, notifications, enforcement)
- **ep-08-iga-detailed-design.md** — Detailed design for IGA context (role maturity, promotion)
- **mvp-product-backlog.md** — Original backlog with all FS-01 to FS-16

### In `/governance/construction/`

- **TE-03-rls-sql-server-implementation.md** — Deep dive: RLS two-layer model, SQL Server setup, error handling
- **SERVICE-IMPLEMENTATION-PLAN.md** — .NET 8 structure, package organization, test pyramid, CI/CD

### In `/architecture/blueprints/`

- **ADR-0048.md** — Closure table pattern for hierarchical tenants
- **ADR-0049.md** — SQL Server partition functions on root_tenant_id
- **ADR-0039.md** — XACML authorization architecture
- **ADR-0021.md** — Compilation model for policy optimization
- **ADR-0047.md** — Hierarchical configuration caching

---

## Key Metrics At a Glance

| Metric | Value | Status |
|--------|-------|--------|
| **Total Story Points** | 578 pts | |
| **Number of Épicas** | 8 (5 MVP + 3 Post-MVP) | |
| **Number of Functional Stories** | 16 (FS-01 to FS-16) | |
| **Number of Technical Stories** | 89 TS | |
| **MVP Points** | 253 pts | |
| **MVP Duration** | 6-7 weeks | |
| **MVP Team** | 6.25 FTE | |
| **Post-MVP Points** | 325 pts | |
| **Post-MVP Duration** | 8-10 weeks | |
| **Post-MVP Team** | 7.75 FTE | |
| **Hiring Gaps** | 2 engineers (Security + QA) | |
| **Critical Success Factors** | 6 (RLS, partition discipline, events, versioning, SIP, ADR training) | |
| **Risk Level** | LOW | | ---

## Next Steps (Sprint 0: Week 1)

### By **Monday (Day 1-2)**
- [ ] Engineering lead reads PART 6 (dependencies) + PART 7 (effort)
- [ ] Product owner reads FS-TO-TS-MAPPING.md
- [ ] Hiring posts job descriptions (Security engineer, QA automation)
- [ ] Tech lead schedules ADR training session

### By **Wednesday (Day 3-4)**
- [ ] CI/CD pipeline setup (GitHub Actions)
- [ ] SQL Server test environment ready (Testcontainers)
- [ ] Backlog imported to GitHub Projects (89 TS, linked to FS)

### By **Friday (Day 5)**
- [ ] Sprint planning: allocate TS to Sprint 1-2
- [ ] ADR training session completed
- [ ] Hiring: first interviews with candidates

### Before **Sprint 1 Starts**
- [ ] QA engineer onboarded (or by mid-Sprint 1)
- [ ] Security engineer starts (or on call for design reviews)
- [ ] All engineers reviewed SERVICE-IMPLEMENTATION-PLAN.md

---

## FAQ** Q: Should I read all documents? **A: No. Start with your role's section above (PO, engineer, QA, etc.). Deep dive only as needed.

**Q: What if we're missing a skill? **A: See PART 5 (Skills Matrix). We identified 2 gaps: Senior Security engineer + QA Automation. Plan hiring accordingly.

**Q: Can we start with a smaller team? **A: Possible, but risky. TS-3.2 (PDP engine, 21 pts) needs security expertise. TS-1.6 (RLS tests, 13 pts) needs experienced QA. Recommend full team as specified.

**Q: What if stories change during construction? **A: Expected. Update TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md + FS-TO-TS-MAPPING.md to keep traceability. Use ADR process for architecture changes.

**Q: How do I validate progress? **A: Use PART 4 (Team Composition Matrix) as baseline. Track actual velocity. Target 27 pts/week (MVP) or 17 pts/week (Post-MVP) per engineer average.

---

## Contacts & Escalation

- **Principal Architect:** Reviews PART 6 (dependencies), Sprint planning
- **Engineering Lead:** Owns PART 1 + PART 7 (execution plan)
- **Product Owner:** Owns PART 4 (team allocation), FS-TO-TS-MAPPING.md validation
- **Tech Lead:** Owns PART 3 (team profiles), mentoring

---

**Last Updated:** 2026-05-14 **Next Review:** Sprint 1 Retrospective (2 weeks)
**Approvals:** Principal Architect

