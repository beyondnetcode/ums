# Análisis Comparativo: Modelo Ejecución Humano vs AI-Driven **Fecha:** 2026-05-14 **Versión:** 1.0 **Propósito:** Evaluar costo/beneficio y gobernanza: ejecución humana tradicional vs AI-Driven supervisada **Scope:** MVP 12 semanas, 168 pts, 3 escenarios infraestructura **Status:** **COMPARATIVE ANALYSIS COMPLETE**

---

## RESUMEN EJECUTIVO

| Dimensión | Modelo Humano | Modelo AI-Driven | Ventaja |
|-----------|--------------|-----------------|---------|
| **Costo Total MVP** | S/ 182,350 - 193,000 | S/ 125,800 - 148,200 | **AI-Driven (35% menos)** |
| **Tiempo Entrega** | 12 semanas | 8-9 semanas | **AI-Driven (25% más rápido)** |
| **Calidad Arquitectónica** | Excelente | Buena (si supervisión fuerte) | **Humano (mayor confianza)** |
| **Riesgo Técnico** | Bajo | Medio-Alto (alucinaciones) | **Humano (menor riesgo)** |
| **Gobernanza/Control** | Tradicional bien establecido | Requiere nuevos procesos | **Humano (más maduro)** |
| **Escalabilidad Post-MVP** | Limitada (hiring) | Excelente (add agents) | **AI-Driven (más flexible)** |
| **Recomendación** | Si prioriazas confianza | Si prioriazas velocidad/costo | **Depende estrategia** | ---

## 1. MODELO TRADICIONAL HUMANO

### 1.1 Supuestos Base

- **Equipo:** 4 personas (1 TL architect, 3 semi-senior devs) + external reviews
- **Estructura:** Roles especializados (backend, frontend, QA, etc.)
- **Gestión:** PM dedicada, governance tradicional
- **Controlado por:** Procesos, reuniones, code review, testing
- **Métrica de productividad:** Horas/persona/día, story points/sprint

### 1.2 Recursos Requeridos

```
Recursos Humanos:
├── Team Lead (Arquitecto) | 50% dedication | S/ 6,500/mes
├── Backend Dev 1 (DDD) | 100% | S/ 7,500/mes
├── Backend Dev 2 (Security) | 100% | S/ 7,500/mes
├── QA/Backend Dev 3 (React+Schema) | 100% | S/ 7,000/mes
├── Project Manager | 100% | S/ 7,500/mes
├── DevOps (40% external) | 40% contracted | S/ 3,200/mes
├── Security Review (20% external) | 20% contracted | S/ 1,800/mes
└── DBA (60% external) | 60% contracted | S/ 4,500/mes

Recursos Técnicos:
├── GitHub (standard) | S/ 300/mes
├── SQL Server licensing | S/ 5,000 one-time
├── CI/CD infrastructure | ~S/ 1,500-3,600/3 meses
├── Monitoring & logging | S/ 200/mes
└── Communication tools | S/ 300/mes (Slack, etc.)

Proceso & Gobernanza:
├── JIRA / project management | S/ 600/mes
├── Confluence / documentation | S/ 400/mes
├── Security scanning tools | S/ 300/mes
└── Testing tools (Playwright, etc) | S/ 200/mes
```

### 1.3 Costos Estimados (3 meses MVP)

| Componente | Mensual | 3 Meses | Notas |
|-----------|---------|---------|-------|
| **Personal Directo** | S/ 29,000 | S/ 87,000 | 4 personas core |
| **External Reviews** | S/ 9,500 | S/ 28,500 | DevOps, Security, DBA |
| **PM/Gestión** | S/ 7,500 | S/ 22,500 | Project management |
| **Herramientas Desarrollo** | S/ 2,000 | S/ 6,000 | GitHub, JIRA, Confluence |
| **Infraestructura** | Variable | S/ 12,450-23,100 | Según modelo (hybrid: S/ 12,450) |
| **Contingencia (10%)** | — | S/ 13,900 | Risk buffer |
| **TOTAL MVP** | **S/ 48,000** | **S/ 182,350** | **Modelo Hybrid recomendado** | ### 1.4 Ventajas Modelo Humano **Arquitectura robusta:** Expertos toman decisiones complejas (RLS, PDP, config hierarchy)
**Debugging efectivo:** Humans entienden contexto, resuelven ambigüedades rápidamente **Creatividad & Innovation:** Proponen mejores diseños, patrones optimizados **Conocimiento a largo plazo:** El equipo se queda, mantiene el código **Responsabilidad clara:** Cada dev es accountable por su código **Control de calidad predecible:** Code review tradicional, testing manual + automated **Cumplimiento normativo:** Trazabilidad clara, auditoría documentada

### 1.5 Riesgos Modelo Humano **Dependencia de personas:** If key dev leaves, knowledge loss **Variabilidad de productividad:** Vacaciones, enfermedad, rotación **Costo de hiring:** Si escalas post-MVP, recruiting time **Overhead comunicacional:** Más reuniones, more meetings = menos coding **Tiempo setup:** 4 personas need ramp-up (2-3 semanas perdidas)

### 1.6 Limitaciones Modelo Humano

 Escalabilidad limitada: Agregar más devs aumenta overhead comunicacional
 Costo fijo: No puedes "pausar" personas fácilmente post-MVP
 Documentación manual: Depende de disciplina del equipo
 Testing manual tedioso: RLS testing (3 tenants, multi-level) es tedioso con humanos
 No 24/7: Si necesitas work de noche, requiere shift rotation

---

## 2. MODELO AI-DRIVEN SUPERVISADO

### 2.1 Supuestos Base

- **Equipo Core:** 1 AI-Driven Architect + 1 Tech Lead (validation)
- **Agentes IA:** Claude (backend), GPT-4 (frontend), AgentQL (QA), etc.
- **Orquestación:** Harness, GitLab Pipelines, custom orchestration
- **Control:** Arquitecto valida decisiones; TL revisa output antes de merge
- **Métrica:** Velocidad agent execution, quality gate pass rate, rework rate
- **Método:** BMAD (Behavioral Model-based Architectural Design) o equivalente

### 2.2 Recursos Requeridos

```
Recursos Humanos (Reducido):
├── AI-Driven Architect (Specialista IA) | 100% | S/ 10,000/mes
├── Tech Lead (Validación + Control) | 100% | S/ 8,000/mes
└── (Senior QA/Security rotation) | 25% | S/ 1,500/mes (part-time review)

Agentes IA & Herramientas:
├── Claude API (backend, architecture) | ~S/ 3,000-5,000/mes | 100K+ tokens/día
├── GPT-4 API (frontend, UI generation) | ~S/ 2,000-3,000/mes | 50K+ tokens/día
├── AgentQL / QA Agent | ~S/ 1,500-2,000/mes | Automated testing
├── SonarQube (code quality) | ~S/ 800/mes | Continuous scanning
├── Harness (orchestration & deployment) | ~S/ 1,200/mes | Multi-agent coordination
├── GitHub Actions (CI/CD enhancement) | ~S/ 500/mes | Agent-triggered workflows
├── Datadog (monitoring agents) | ~S/ 1,000/mes | Agent behavior monitoring
└── SecurityAI (threat scanning by agents)| ~S/ 800/mes | Automated security review

Infraestructura Especializada:
├── GPU cluster (agent execution) | ~S/ 2,500-4,000/mes | Optional: local LLM inference
├── Agent state management (Redis) | ~S/ 500/mes | Shared state for agents
├── Logging & audit (agents) | ~S/ 600/mes | Agent action tracking
└── Retry/recovery infrastructure | ~S/ 400/mes | Handle agent failures gracefully

Documentación & Knowledge:
├── Langchain / LlamaIndex instances | ~S/ 400/mes | Agent memory/context
└── Vector DB (Pinecone / Weaviate) | ~S/ 300/mes | Embeddings for RAG

Contingencia:
├── Retrabajo por alucinaciones | ~S/ 3,000-5,000 | Estimated rework effort
├── Expert review backup | ~S/ 2,000-3,000 | When agents fail
└── Emergency human intervention | ~S/ 1,500-2,000 | Scope creep buffer
```

### 2.3 Costos Estimados (3 meses MVP)

| Componente | Mensual | 3 Meses | Notas |
|-----------|---------|---------|-------|
| **Personal Core** | S/ 19,500 | S/ 58,500 | 2-2.5 personas (Architect + TL) |
| **APIs IA (Claude + GPT-4)** | S/ 5,000-8,000 | S/ 15,000-24,000 | Token consumption variable |
| **Herramientas Orquestación** | S/ 4,000-5,500 | S/ 12,000-16,500 | Harness, SonarQube, etc |
| **Infraestructura Agents** | S/ 4,000-5,000 | S/ 12,000-15,000 | GPU, Redis, monitoring |
| **Retrabajo Estimado (8-10%)** | — | S/ 12,000-18,000 | Alucinaciones, falsos positivos |
| **Herramientas Desarrollo** | S/ 1,500 | S/ 4,500 | GitHub, monitoring |
| **TOTAL MVP** | **S/ 34,000-40,500** | **S/ 125,800-148,200** | **vs S/ 182,350 humano** | ---

## 3. TABLA COMPARATIVA MODELO TRADICIONAL vs AI-DRIVEN

### 3.1 Dimensión Estratégica

| Criterio | Modelo Humano | Modelo AI-Driven | Observación |
|----------|--------------|-----------------|------------|
| **Costo Total 12 semanas** | S/ 182,350 | S/ 125,800 - 148,200 | **AI ahorra 30-35%** |
| **Costo por Punto Historia** | S/ 1,086/pt | S/ 749-883/pt | **AI más eficiente** |
| **Personas requeridas** | 4-5 core | 2-3 core | **AI reduce headcount 50%** |
| **Timeline realista** | 12 semanas | 8-10 semanas | **AI comprime timeline 20-30%** |
| **Velocidad Desarrollo** | ~14 pts/semana | ~18-20 pts/semana | **Agents parallelizan trabajo** |
| **Conocimiento institucional** | Alto (grow with team) | Medio (lives in agents) | **Humano ventaja post-MVP** |
| **Documentación** | Manual (tedious) | Auto-generated (agents) | **AI genera docs on-the-fly** |
| **Testing Coverage** | ~70% automated | ~90% automated (agents write tests) | **AI improve coverage** | ### 3.2 Dimensión Riesgos & Control

| Criterio | Modelo Humano | Modelo AI-Driven | Crítico? |
|----------|--------------|-----------------|---------|
| **Alucinaciones / Errores lógicos** | Bajo (humans debug) | Medio-Alto (agents hallucinate) | HIGH RISK |
| **Seguridad (code injection en prompts)** | Bajo | Medio (token exposure risk) | MEDIUM RISK |
| **Compliance & Auditoría** | Excelente (clear accountability) | Buena (logs agent actions) | Mitigable |
| **RLS Complexity** | Humans entienden contexto | Agents guided by few-shot examples | HIGH RISK |
| **PDP Logic (20+ rules interactions)** | Expert validation | AI generates patterns, TL validates | MEDIUM RISK |
| **Escalabilidad Post-MVP** | Limited (hiring) | Excelente (add agents) | AI ADVANTAGE |
| **Vendor Lock-in** | Bajo (standard skills) | Medio (tied to Claude/OpenAI) | MEDIUM RISK |
| **Cost Overrun** | Predictable (fixed salaries) | Variable (token consumption) | Mitigable (caps on tokens) | ### 3.3 Dimensión Gobernanza & Control

| Aspecto | Modelo Humano | Modelo AI-Driven | Requerimiento |
|--------|--------------|-----------------|--------------|
| **Code Review Process** | Peer review (2 devs min) | AI-generated → TL validates | Más riguroso en AI |
| **Architectural Decisions** | Architect decides | Architect + Agent suggestions → decision | AI asiste, humans deciden |
| **Quality Gates** | Manual testing + CI | Automated (agents run tests 24/7) | Más coverage en AI |
| **Deployment Control** | PM approval + TL sign-off | Automated via Harness + TL gate | Similar rigor, más velocidad en AI |
| **Audit Trail** | Git commits + meeting notes | Git + agent action logs + API calls | AI más trazable |
| **Rollback Capability** | Git revert + manual fix | Same (git revert) + rerun agents | Similar |
| **Knowledge Bus Factor** | HIGH (if TL leaves, risky) | MEDIUM (agents contain patterns) | Humano vulnerability |
| **Hands-on Control Needed** | Moderate (code review) | High (must validate agent outputs constantly) | AI requiere más supervisión activa | ---

## 4. ESCENARIOS DE INFRAESTRUCTURA

### Escenario A: ON-PREMISE** Modelo Humano:**
- Costo infraestructura: S/ 23,100 (SQL Server license + local hardware)
- Costo total MVP: S/ 205,450 (sum: personal + gestión + infra)
- Limitaciones: DBA on-site needed, manual backups, limited scalability **Modelo AI-Driven:**
- Costo infraestructura: S/ 28,000-35,000 (GPU cluster for local LLM inference, agent state)
- Costo total MVP: S/ 154,000-183,000 (sum: personal + APIs + herramientas + infra + rework)
- Ventajas: Faster with agents, no cloud dependency, control total de datos
- Riesgos: Requiere IT expertise en GPU/orchestration, on-prem LLM (Llama 2) fallible **Recomendación:** ON-PREM no óptimo para AI-Driven (overhead infraestructura alto)

---

### Escenario B: HÍBRIDO (RECOMENDADO)

**Modelo Humano (BASELINE):**
```
Costo Infraestructura: S/ 12,450
├── Azure SQL (managed) S/ 7,500
├── App Service (app hosting) S/ 2,400
├── Storage, monitoring S/ 1,350
└── Support plan S/ 300

Costo Total MVP: S/ 194,800 (persona S/ 117,000 + gestión S/ 52,900 + infra S/ 12,450 + contingencia S/ 12,450)
Timeline: 12 semanas
ROI Year 1: 84%
```

**Modelo AI-Driven (OPTIMIZADO PARA HYBRID):**
```
Costo Infraestructura Adicional: S/ 8,000-12,000 (vs humano)
├── Agents run on App Service (shared, no extra cost)
├── Vector DB for agent memory S/ 2,000
├── Agent state management (Redis) S/ 1,500
├── Extra monitoring (Datadog) S/ 1,500
├── GPU acceleration (optional) S/ 2,500-5,000
└── Harness licensing S/ 1,200/mes

Costo Total MVP: S/ 141,000-164,000 (persona S/ 58,500 + APIs S/ 18,000 + herramientas S/ 15,000 + infra S/ 20,000 + rework S/ 15,000)
Timeline: 8-9 semanas (25% faster)
ROI Year 1: 112% (faster launch = earlier revenue)

Ventajas sobre Humano Hybrid:
 27% menos costo (S/ 141K vs S/ 195K)
 25% más rápido (8.5 vs 12 semanas)
 Agents generan docs automáticamente
 90% test coverage (vs 70% humano)
 Escalable: agregar agents cuesta poco

Riesgos sobre Humano Hybrid:
 Alucinaciones en RLS logic (mitigado: TL review + few-shot examples)
 Agents may miss edge cases en PDP rules (mitigado: manual scenarios 20+)
 Token costs pueden crecer si scope expands (mitigado: caps en prompts)
```

**Recomendación para Hybrid:** **AI-Driven supera Humano (30% costo, 25% tiempo, si gestión es fuerte)**

---

### Escenario C: CLOUD-NATIVE** Modelo Humano:**
```
Costo Total MVP: S/ 212,500 (personal S/ 117,000 + gestión S/ 52,900 + infra S/ 21,300 + contingencia S/ 21,300)
Timeline: 12 semanas
Limitaciones: Agents no optimizados para cloud-native architecture
```

**Modelo AI-Driven:**
```
Costo Total MVP: S/ 148,000-172,000 (personal S/ 58,500 + APIs S/ 18,000 + herramientas S/ 16,000 + infra S/ 22,000 + rework S/ 15,000)
Timeline: 7-8 semanas (optimizado para cloud patterns: containers, serverless)
Ventajas:
 Agents entienden Azure patterns (Dapr, Service Fabric, Functions)
 Infrastructure as Code generado por agents (Terraform, ARM templates)
 Auto-scaling configuration (agents optimize)
 Menor costo operacional post-MVP (serverless costs less)

Riesgos:
 Serverless cold starts (agents may not optimize for latency initially)
 Distributed tracing complexity (agents may miss observability)
 Vendor lock-in (Azure patterns hard to port)
```

**Recomendación para Cloud-Native:** **AI-Driven superior (30% costo, 33% tiempo)**

---

## 5. MATRIZ DE DECISIÓN

### 5.1 Si Priorizas COSTO:
```
 WINNER: AI-Driven Hybrid
- Ahorra S/ 53,800 (30% menos)
- Amortiza en 2 meses de ingresos si SaaS
- Menor riesgo de overrun presupuestario (IA más predecible que humano)
```

### 5.2 Si Priorizas VELOCIDAD:
```
 WINNER: AI-Driven Hybrid
- 3.5 semanas más rápido
- Agents parallelizan: backend + frontend + QA simultáneamente
- Lanzamiento más rápido = ingresos antes
```

### 5.3 Si Priorizas CALIDAD ARQUITECTÓNICA:
```
 WINNER: Modelo Humano Hybrid
- Arquitecto experto toma decisiones
- Mejor para RLS (multi-tenant complexity)
- Mejor para PDP (authorization logic nuances)
- Menos riesgo de re-work por alucinaciones
```

### 5.4 Si Priorizas CONTROL GOBERNANZA:
```
 WINNER: Modelo Humano Hybrid
- Trazabilidad clara (quien escribió qué código)
- Auditoría más simple (no hay "agent action logs" complejos)
- Cumplimiento normativo más straightforward
- Responsabilidad clara
```

### 5.5 Si Priorizas ESCALABILIDAD POST-MVP:
```
 WINNER: AI-Driven Hybrid
- Agregar feature: solo agregar prompt + agent
- No necesita contratar devs adicionales
- Agents escalan a 0 marginal cost
- Humano requiere hiring cycle (2-3 meses)
```

---

## 6. RECOMENDACIÓN FINAL EJECUTIVA

### Contexto UMS (Governance-First, Multi-Tenant, Complex Authorization)

**Para MVP Reducido (168 pts, 12 semanas, 4-personas):**

| Escenario | Recomendación | Justificación |
|-----------|--------------|---------------|
| **On-Premise** | **Modelo Humano** | Infra overhead alto en AI; humano es baseline |
| **Híbrido** | **AI-Driven (con condiciones)** | 30% costo, 25% tiempo; TL must validate RLS/PDP |
| **Cloud-Native** | **AI-Driven** | Agents entienden patterns; auto-scaling; menor ops | ### Condiciones de Éxito para AI-Driven Hybrid:

**CRÍTICAS (Must-Have):** 1. Arquitecto AI-Driven experto (10+ años software, 2+ años IA)
- Responsable: validar cada decisión arquitectónica de agents
- No debe ser "babysitter"; debe ser director creativo

2. Few-Shot Examples pre-cargadas en prompts
- RLS multi-tenant patterns (closure table, EF Core filters)
- PDP authorization logic (rule matching, effect aggregation)
- Agents aprenden del ejemplo, reducen alucinaciones

3. Quality gates rigurosas
- Code review 100% de agent output (vs peer review en humano)
- SonarQube + security scanning mandatory antes de merge
- Manual testing de RLS: 3-tenant isolation (agents may miss edge case)

4. Token budget caps
- Claude: Max S/ 5,000/mes (prevent runaway costs)
- Monitor daily consumption
- Throttle if exceeds 80% budget **IMPORTANTES (Should-Have):** 5. Harness orchestration + rollback strategy
- Agent-generated code fails? Harness reverts automatically
- No manual hotfixes (agents regenerate)

6. Rework buffer (8-10% of timeline)
- Assume agents hallucinate on ~8-10% of tasks
- RLS testing: agents generate test, TL manually validates
- PDP rules: agents write rules, architect validates 20+ scenarios

7. Knowledge base for agents (RAG)
- Load ADRs (0048, 0039, 0047) into vector DB
- Load prior UMS design docs
- Agents retrieve context before generating code

### Si NO puedes cumplir condiciones críticas:
```
 NO USES AI-Driven
→ FALLBACK: Modelo Humano Hybrid (S/ 182,350, 12 semanas, confianza 99%)
```

### ROI Comparison (Year 1):

**Modelo Humano:**
- Costo MVP: S/ 182,350
- Post-MVP (9 semanas): S/ 127,000
- Total Año 1: S/ 309,350
- Ingresos SaaS: S/ 1,800,000
- **Net: S/ 1,490,650 | ROI: 382%**

**Modelo AI-Driven (si exitoso):**
- Costo MVP: S/ 141,000 (8.5 semanas)
- Post-MVP (6 semanas): S/ 85,000 (faster, fewer agents needed)
- Total Año 1: S/ 226,000
- Ingresos SaaS: S/ 2,100,000 (launch 3.5 weeks earlier = +300K revenue)
- **Net: S/ 1,874,000 | ROI: 729%**

**Diferencia:** AI-Driven genera S/ 383,350 más en Year 1 (91% better ROI) SI ejecución es exitosa.

---

## 7. MATRIZ DE RIESGOS & MITIGACIÓN

### Riesgos AI-Driven Críticos:

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|--------|-----------|
| **Agente alucina en RLS multi-tenant logic** | MEDIUM | CRITICAL | Few-shot examples + manual 3-tenant tests (Sprint 1) |
| **PDP rule matching incompleto (edge cases)** | MEDIUM | HIGH | Arquitecto genera 20+ test scenarios before Sprint 3 |
| **Token costs exceed budget** | LOW-MEDIUM | MEDIUM | Caps en API, daily monitoring, throttle if >80% |
| **Agents generate insecure code** | LOW | CRITICAL | SonarQube + manual security review, no direct deploy |
| **Agents hallucinate new features** | MEDIUM | MEDIUM | Prompt constraints strict, no open-ended prompts |
| **Harness fails, agents stuck** | LOW | HIGH | Fallback: manual CI/CD pipeline ready |
| **TL bottleneck (validates everything)** | MEDIUM | MEDIUM | Automate quality gates, TL reviews only critical paths | ### Mitigación por Fase:

**Sprint 0:**
- Load ADRs + prior designs into agent knowledge base
- Run 10 agent-generated test scenarios (manual validation)
- Establish quality gate SonarQube/security baseline **Sprint 1:**
- Agents generate RLS schema + EF Core filters
- Architect validates against closure table pattern
- Manual 3-tenant isolation tests before merge **Sprint 2:**
- Agents generate PDP logic
- Architect reviews rule matching algorithm
- Manual 20-scenario testing before Sprint 3 **Sprint 3:**
- Agents generate APIs + React UI
- TL runs Playwright E2E tests (agents + manual)
- UAT with stakeholder before demo

---

## 8. CONCLUSIÓN

### Recomendación Dual:

**PRIMARY: AI-Driven Hybrid (S/ 141K, 8.5 weeks)**
- Si tienes arquitecto AI-Driven experto
- Si aceptas 8-10% rework para ganar velocidad + costo
- Si priorizas ROI (729% vs 382%)

**FALLBACK: Modelo Humano Hybrid (S/ 182K, 12 weeks)**
- Si no tienes expertise AI o risk aversion es alta
- Si priorizas confianza + gobernanza tradicional
- Si producto future es mission-critical (compliance first)

### Para UMS Específicamente:

**Contexto:** Governance-first, multi-tenant (RLS complex), authorization complex (PDP with 20+ rules)

**Recomendación:** **AI-Driven Hybrid CON CONDICIONES**
- Requiere arquitecto experto validando RLS + PDP
- Requiere rigorous quality gates
- Requiere 3-tenant RLS testing manual (agents pueden fallar)
- **Potencial:** S/ 383K más revenue en Year 1 si exitoso **Plan B si Risk Too High:** Modelo Humano Hybrid (baseline segura)

---

**Preparado por:** Arquitecto Principal **Fecha:** 2026-05-14 **Status:** **COMPARATIVE ANALYSIS COMPLETE - READY FOR STRATEGY DECISION**

*Analysis basado en market data 2026, token costs aproximados (Claude 3.5 Sonnet), herramientas open-source + SaaS estándar. Ajustar según capacidades internas y risk tolerance organizacional.*
