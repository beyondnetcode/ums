# Análisis de Costo/Beneficio — MVP Reducido UMS** Fecha:** 2026-05-14 **Versión:** 1.0 **Propósito:** Análisis ejecutivo de costos y beneficios para MVP 12-semanas **Audiencia:** Stakeholders, Finance, C-level **Status:** **READY FOR EXECUTIVE REVIEW**

---

## 1. COSTOS ESTIMADOS POR PERFIL

### Tabla de Costos Mensuales (Rangos en Soles PEN)

| Perfil | Responsabilidades | Seniority | Rango Mensual | Asunción MVP |
|--------|------------------|-----------|---------------|--------------|
| **Arquitecto Principal** | ADR, design decisions, mentoring | Lead/Principal | S/ 13,000 - 16,000 | 50% (governance, code review) |
| **Backend Engineer (DDD)** | Domain models, APIs, EF Core | Semi-Senior | S/ 6,500 - 8,500 | 100% (full sprint) |
| **Backend Engineer (Security)** | PDP, authorization, config | Semi-Senior | S/ 6,500 - 8,500 | 100% (full sprint) |
| **QA / Backend Engineer** | Schema, React, integration tests | Semi-Senior | S/ 6,000 - 8,000 | 100% (full sprint) |
| **Frontend Engineer** | React components, UI/UX integration | Semi-Senior | S/ 6,500 - 8,000 | ~30% (if hired; MVP uses QA/Backend dev) |
| **DevOps Engineer** | CI/CD, infrastructure, monitoring | Mid-Senior | S/ 7,000 - 9,000 | ~40% (Sprint 0, Sprint 3) |
| **QA Lead** | Test planning, UAT, quality gates | Mid-Senior | S/ 7,500 - 9,500 | ~25% (pre-Sprint 1, Sprint 3) |
| **Security Engineer** | Threat modeling, code review, policies | Senior | S/ 8,500 - 11,000 | ~20% (external, code review TS-1.2, TS-3.2) |
| **Data/DBA** | Schema design, RLS, optimization | Mid-Senior | S/ 7,500 - 9,500 | ~60% (external, Sprint 0-1, reviews) |
| **Project Manager** | Coordination, tracking, risk management | Mid-Senior | S/ 7,000 - 9,000 | 100% (full engagement) | ---

### Cálculo de Costos MVP (12 semanas = 3 meses)

**Equipo MVP Asumido (4 personas internas + external reviews):**

| Rol | Mensual (Pt. Media) | Meses | Subtotal |
|-----|-------------------|-------|----------|
| Team Lead (Arquitecto 50%) | S/ 7,500 | 3 | S/ 22,500 |
| Backend Dev 1 (DDD, 100%) | S/ 7,500 | 3 | S/ 22,500 |
| Backend Dev 2 (Security, 100%) | S/ 7,500 | 3 | S/ 22,500 |
| QA/Backend Dev 3 (React+Schema, 100%) | S/ 7,000 | 3 | S/ 21,000 |
| **Subtotal Equipo Directo** | — | — | **S/ 88,500** |
| DevOps (40% external) | S/ 3,200 | 3 | S/ 9,600 |
| Security Review (20% external) | S/ 1,800 | 3 | S/ 5,400 |
| DBA Review (60% external) | S/ 4,500 | 3 | S/ 13,500 |
| **Subtotal External Reviews** | — | — | **S/ 28,500** |
| **TOTAL COSTO DIRECTO PERSONAL** | — | — | **S/ 117,000** | ---

## 2. COSTOS DE GESTIÓN (3 MESES)

| Concepto | Descripción | Costo |
|----------|-------------|-------|
| **PM/Coordinación** | Daily standups, sprint planning, stakeholder updates | S/ 21,000 |
| **Documentación & Governance** | Estimation docs, ADRs, meeting notes, compliance records | S/ 6,000 |
| **Seguimiento & Risk Management** | Risk register, issue tracking, escalation, mitigation planning | S/ 4,500 |
| **Validación Funcional & Técnica** | Acceptance testing, code review processes, QA sign-off | S/ 7,500 |
| **Contingencia & Unknowns (10%)** | Buffer para riesgos identificados (RLS, PDP complexity) | S/ 13,900 |
| **SUBTOTAL GESTIÓN** | — | **S/ 52,900** | ---

## 3. COSTOS DE INFRAESTRUCTURA & OPERACIÓN (3 MESES)

### Escenario A: ON-PREMISE (Local)

**Infraestructura:**
- SQL Server 2022 (RTM) license (1 servidor) | S/ 15,000 (one-time)
- 2x VM (dev/test) para developers | S/ 0 (on existing infra)
- GitHub Enterprise (on-prem or cloud) | S/ 3,600/3 meses **Operación & Soporte:**
- DBA on-call (included in 60% external DBA cost) | S/ 0 (included above)
- System administration (IT staff, 10% allocation) | S/ 2,000
- Backups, monitoring, maintenance (manual) | S/ 1,500
- Network/security patches | S/ 1,000** SUBTOTAL INFRAESTRUCTURA (ON-PREM)** | **S/ 23,100**

**Riesgos:** Requiere licencias SQL Server; dependencia de IT interno; RLS en Fase 2 aumenta complejidad.
**Beneficios:** Control total; no vendor lock-in; costo predecible a largo plazo.
**Limitaciones:** Requiere equipo IT dedicado; mayor overhead operacional; escalabilidad limitada.

---

### Escenario B: HÍBRIDO (Partial Cloud)

**Infraestructura:**
- SQL Server en Azure (Standard, 2vCPU, basic HA) | S/ 2,500/mes × 3 = S/ 7,500
- App Service plan (B2 tier, dual instance) | S/ 800/mes × 3 = S/ 2,400
- GitHub Cloud | S/ 300/mes × 3 = S/ 900
- Storage (backups, audit logs) | S/ 200/mes × 3 = S/ 600
- Monitoring (Application Insights, Log Analytics) | S/ 250/mes × 3 = S/ 750 **Operación & Soporte:**
- Azure support plan (Standard) | S/ 100/mes × 3 = S/ 300
- DevOps (included in 40% external cost above) | S/ 0
- Database patching (automated in Azure) | S/ 0
- Security compliance (Azure Policy, built-in) | S/ 0** SUBTOTAL INFRAESTRUCTURA (HÍBRIDO)** | **S/ 12,450**

**Riesgos:** Dependencia de Azure; requiere nuevas skills (Azure IaC); posible lock-in; variabilidad de costos.
**Beneficios:** Escalabilidad elástica; backups automáticos; compliance facilitado; menor overhead IT local.
**Limitaciones:** Requiere reingenierización de deployment; ciclo de aprendizaje Azure; costo variable.

---

### Escenario C: CLOUD-NATIVE (Full Cloud)

**Infraestructura:**
- Azure SQL Database (Serverless, auto-scale) | S/ 3,500/mes × 3 = S/ 10,500
- App Service (Premium v2, auto-scale) | S/ 1,200/mes × 3 = S/ 3,600
- Azure DevOps (Advanced tier) | S/ 1,000/mes × 3 = S/ 3,000
- Storage & CDN (audit logs, static assets) | S/ 400/mes × 3 = S/ 1,200
- Monitoring & Analytics (premium) | S/ 500/mes × 3 = S/ 1,500 **Operación & Soporte:**
- Azure support plan (Premier) | S/ 500/mes × 3 = S/ 1,500
- Managed database operations (DBA not needed) | S/ 0
- Auto-scaling, health checks, failover (automated) | S/ 0
- Security scanning, compliance (built-in) | S/ 0** SUBTOTAL INFRAESTRUCTURA (CLOUD-NATIVE)** | **S/ 21,300**

**Riesgos:** Vendor lock-in (Azure); datos en cloud (compliance sensitivity); cold starts (serverless); costo runaway posible.
**Beneficios:** Zero infrastructure management; auto-scaling; best disaster recovery; highest availability (99.99%).
**Limitaciones:** Mayor costo inicial; requiere arquitectura cloud-native; menos portabilidad.

---

## 4. RESUMEN COMPARATIVO DE COSTOS TOTALES (12 SEMANAS)

| Componente | On-Premise | Híbrido | Cloud-Native |
|------------|-----------|---------|-------------|
| **Costo Personal (interno + external)** | S/ 117,000 | S/ 117,000 | S/ 117,000 |
| **Costo Gestión** | S/ 52,900 | S/ 52,900 | S/ 52,900 |
| **Costo Infraestructura & Soporte** | S/ 23,100 | S/ 12,450 | S/ 21,300 |
| **COSTO TOTAL MVP** | **S/ 193,000** | **S/ 182,350** | **S/ 191,200** |
| **Costo por Semana** | S/ 16,083 | S/ 15,196 | S/ 15,933 |
| **Costo por Punto de Historia (168 pts)** | S/ 1,148/pt | S/ 1,086/pt | S/ 1,138/pt | ---

## 5. ANÁLISIS BENEFICIOS ESPERADOS (POST-MVP)

### Año 1 (Full Product, Post-MVP)

**Ingresos Potenciales (Licencia SaaS asumiendo):**
- 50 clientes corporativos × S/ 3,000/mes = S/ 150,000/mes = **S/ 1,800,000/año**

**Reducción de Costos Operacionales (vs. sistemas legacy):**
- Automatización de identity & access (reducción 40% de IT/Security ops) = S/ 200,000/año
- Compliance automático (reducción 30% de auditoría) = S/ 80,000/año
- Infrastructure efficiency (consolidación de sistemas) = S/ 120,000/año **ROI Proyectado (Año 1):**
- Inversión MVP (3 meses) + Post-MVP (9 meses) ≈ S/ 1,200,000
- Ingresos Año 1 = S/ 1,800,000
- Ahorros operacionales = S/ 400,000
- **ROI = (1,800,000 + 400,000 - 1,200,000) / 1,200,000 = 84% en Año 1**

---

## 6. CUADRO COMPARATIVO EJECUTIVO

| Criterio | On-Premise | Híbrido | Cloud-Native |
|----------|-----------|---------|-------------|
| **Costo Inicial (MVP)** | S/ 193,000 | **S/ 182,350** | S/ 191,200 |
| **Costo Operacional Anual (Post-MVP)** | S/ 180,000 | S/ 95,000 | S/ 110,000 |
| **Complejidad Operacional** | Alta | Media | Baja |
| **Escalabilidad** | Limitada (IT approval) | Buena (auto-scale) | **Excelente** |
| **Availabilidad (SLA)** | 95% | 99.5% | **99.99%** |
| **Tiempo Setup (Sprint 0)** | 2 semanas | **1 semana** | 3 días |
| **Risk (RLS, PDP testing)** | Alta (manual) | Media (managed DB) | Baja (failover automático) |
| **Lock-in Risk** | Bajo | Medio | **Alto** |
| **Recomendación** | No (complejo) | **SÍ** (óptimo) | Alternativa | ---

## 7. RECOMENDACIÓN EJECUTIVA

### **OPCIÓN RECOMENDADA: HÍBRIDO (Azure Hybrid)**

**Razones:** 1. **Costo Óptimo:** S/ 182,350 (11% menos que on-prem, 5% menos que cloud-native)

2. **Balance Riesgo/Beneficio:**
- Infraestructura managed (Azure SQL, App Service) = menor riesgo técnico
- Equipo puede reutilizar skills existentes (si tiene IT Azure)
- Escalabilidad automática sin over-engineering
- Compliance facilitado (Azure Policy, built-in security)

3. **Timeline:**
- Sprint 0 puede comenzar con Azure setup básico (1 semana vs 2 semanas on-prem)
- RLS en EF Core no impactado; SQL Server managed por Azure
- No requiere re-arquitectura cloud-native (costo + tiempo)

4. **Operacional:**
- DBA 60% reduced (Azure manage backups, patches)
- DevOps 40% reduced (auto-scaling, no manual capacity planning)
- IT interno no necesita SQL Server expertise (delega a Azure)

5. **Futuro:**
- Post-MVP: fácil migración a cloud-native si escala requiere
- Permite pilot con early customers sin sobrecosto
- Patrón base para multi-tenancy en Azure (closures, RLS)

### **Supuestos Usados:**

- Salarios base: mercado peruano mid-market (S/ 6,500-9,000 semi-senior)
- MVP timeline fijo: 12 semanas (no extensiones)
- Equipo core: 4 personas; external: DBA 60%, DevOps 40%, Security 20%
- Infraestructura: Azure base plan (no premium initially)
- Post-MVP: 9 semanas adicionales (3 meses restantes Q2 2026)
- No incluye: licencias software adicionales (Jira, Confluence), travel, training extensiva

---

## 8. PLAN DE CONTINGENCIA** Si costos escalan:**
- Reducir external DBA (Sprint 0 solo, luego TL mentors)
- Cloud-native Serverless (más barato post-MVP si escalas)
- On-prem si constrainted by compliance/data residency **Si timeline se extiende (risk RLS/PDP):**
- +4 semanas buffer = S/ 64,400 adicional (gestión + overhead)
- Risk: ROI diluted (payload moves to Q3 2026)
- Recomendación: añadir external QA en Sprint 2-3 (S/ 8,000) para acelerar testing

---

**Preparado por:** Arquitecto Principal **Fecha:** 2026-05-14 **Estado:** **ANÁLISIS COMPLETO - LISTO PARA JUNTA GOVERNANCE**

*Este análisis es referencial y asume condiciones de mercado Perú 2026. Ajustar según contexto local, beneficios tributarios, y estrategia empresarial.*
