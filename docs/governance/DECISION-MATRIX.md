# UMS — Matriz de Decisión Go/No-Go

> **Propósito:** Documento formal de aprobación para iniciar construcción del MVP.
> **Firmantes requeridos:** CTO, CFO, Head of Engineering
> **Fecha límite de decisión:** 7 días desde recepción
> **Próximo paso si GO:** Kickoff Sprint 0 dentro de 7 días

**Versión:** 1.0 | **Fecha:** 2026-05-15 | **Estado:** ⏳ PENDIENTE DE FIRMA

---

## 📋 SECCIÓN 1: Decisiones Estratégicas

### 1.1 Modelo de Ejecución

| Opción | Costo | Timeline | ROI Y1 | Riesgo | Recomendación |
|--------|-------|----------|--------|--------|--------------|
| ☐ **A. AI-Driven Híbrido** | S/ 141,000 | 8.5 sem | 729% | Medio | ⭐ **RECOMENDADO** |
| ☐ **B. Equipo Humano Tradicional** | S/ 182,350 | 12 sem | 382% | Bajo | Plan B conservador |
| ☐ **C. NO-GO** | — | — | — | — | Si no se cumplen condiciones |

**Selección:** ☐ A   ☐ B   ☐ C

---

### 1.2 Modelo de Infraestructura

| Opción | Costo Infra (3 meses) | Costo Op. Anual | Lock-in | Recomendación |
|--------|----------------------|-----------------|---------|--------------|
| ☐ **On-Premise** | S/ 23,100 | S/ 180,000 | Bajo | ❌ Complejo, no recomendado |
| ☐ **Híbrido (Azure)** | S/ 12,450 | S/ 95,000 | Medio | ⭐ **RECOMENDADO** |
| ☐ **Cloud-Native** | S/ 21,300 | S/ 110,000 | Alto | Alternativa futura |

**Selección:** ☐ On-Premise   ☐ Híbrido   ☐ Cloud-Native

---

### 1.3 Alcance MVP

| Opción | Story Points | Funcionalidades | Timeline | Recomendación |
|--------|--------------|-----------------|----------|--------------|
| ☐ **MVP Reducido (8 FS)** | 168 pts | Identidad + Authz + Config + Login | 8.5–12 sem | ⭐ **RECOMENDADO** |
| ☐ **MVP Completo (16 FS)** | 336 pts | Todo el producto | 25+ semanas | ❌ Excede capacidad equipo |

**Selección:** ☐ Reducido   ☐ Completo

---

## 📋 SECCIÓN 2: Condiciones de Aprobación

Marcar cada condición como aceptada antes de firmar:

### 2.1 Condiciones Técnicas
- [ ] **Arquitectura validada:** 49 ADRs aprobados, especialmente ADR-0048 (closure tables), ADR-0039 (XACML), ADR-0047 (config caching)
- [ ] **RLS multi-tenant:** Diseño de dos capas (EF Core + SQL Server) aceptado, code review externo DBA agendado para Sprint 0
- [ ] **Stack tecnológico:** .NET 8, EF Core 8, SQL Server 2022, React 18+, GitHub Actions confirmado
- [ ] **Confianza estimación:** 70-75% MEDIUM-HIGH aceptada como nivel de riesgo de planificación

### 2.2 Condiciones de Equipo (Modelo A: AI-Driven)
- [ ] **Arquitecto AI-Driven:** Identificado o contratable (10+ años exp., 2+ años IA) — inicio semana 1
- [ ] **Tech Lead complementario:** Confirmado (50% dedicación)
- [ ] **Quality gates:** SonarQube + code review humano obligatorio configurado
- [ ] **Presupuesto retrabajo:** 8-10% reservado para corregir hallucinations (~S/ 12K)

### 2.2 Condiciones de Equipo (Modelo B: Humano)
- [ ] **1 Team Lead:** Identificado (50% arquitectura + 50% mentoring)
- [ ] **3 Desarrolladores semi-senior:** Backend DDD + Backend Security + QA/Backend
- [ ] **Hires pendientes (2):** Senior Security Engineer + QA Automation Engineer — máximo semana 2 de Sprint 0
- [ ] **Plan B Hiring:** Contractor 3 meses (S/ 18K extra) reservado si no se completan hires

### 2.3 Condiciones Operacionales
- [ ] **Infraestructura aprobada:** Modelo Híbrido (Azure SQL + App Service) confirmado por IT
- [ ] **CI/CD pipeline:** GitHub Actions + entornos dev/test configurables en Sprint 0
- [ ] **Reuniones de seguimiento:** Daily standups + sprint review cada 2 semanas comprometidos
- [ ] **Stakeholders disponibles:** Product Owner + Tech Lead para aclaraciones diarias

### 2.4 Condiciones Financieras
- [ ] **Presupuesto aprobado:** S/ 141K (Modelo A) o S/ 182K (Modelo B) liberado
- [ ] **Contingencia:** 10% adicional (~S/ 14-18K) reservado para imprevistos
- [ ] **Plan de pago:** Definido (hitos por sprint, milestone-based, o mensual)
- [ ] **ROI medible:** KPIs Year-1 definidos (clientes adquiridos, ARPU, churn) — ver [REVENUE-MODEL-YEAR-1.md](./construction/REVENUE-MODEL-YEAR-1.md)

---

## 📋 SECCIÓN 3: Riesgos Aceptados

Confirmar comprensión y aceptación de cada riesgo:

| # | Riesgo | Prob. | Impacto | Mitigación | ✓ Acepto |
|---|--------|-------|---------|------------|---------|
| 1 | Complejidad RLS multi-tenant subestimada | Media | Alto | Code review DBA externo + pair programming TL | ☐ |
| 2 | Hiring 2 ingenieros (si Modelo B) no completo en 2 sem | Media | Medio | Contractor backup S/ 18K reservado | ☐ |
| 3 | Alucinaciones AI (si Modelo A) generan retrabajo | Media-Alta | Medio | Quality gates + 10% buffer presupuestado | ☐ |
| 4 | Retrasos por dependencia entre TS críticos (RLS → PDP → Config) | Baja-Media | Alto | Buffer 4 semanas en timeline 12 sem | ☐ |
| 5 | Cliente adoption Y1 menor a 50 (Revenue model) | Media | Alto | Sensitivity analysis 25/50/100 clientes en revenue model | ☐ |

---

## 📋 SECCIÓN 4: Métricas de Éxito Acordadas

Definir KPIs verificables para sign-off final del MVP:

### Técnicos (verificables en código)
- [ ] **Aislamiento multi-tenant:** 100% tests de aislamiento pasando (no leakage cross-tenant)
- [ ] **Performance:** Login + autorización < 500ms p95
- [ ] **Cobertura tests:** 70%+ unit, 100% historias críticas (TS-1.2, TS-3.2, TS-4.3)
- [ ] **ADRs respetados:** Verificación automatizada de patrones (ADR-0048, 0039, 0047)

### Funcionales (verificables por usuario)
- [ ] **FS-01:** Login corporativo funciona con email + password
- [ ] **FS-02:** Auto-registro con verificación de email funciona
- [ ] **FS-03:** Onboarding de organización crea tenant + admin
- [ ] **FS-05/06/07:** Políticas XACML creables, asignables, evaluables en runtime
- [ ] **FS-13:** Configuración jerárquica resuelve correctamente (ENV > SISTEMA > TENANT)
- [ ] **FS-08:** Login page brandeable + dashboard de diagnóstico operativo

### Negocio (verificables post-launch)
- [ ] **Pilot customers:** 3-5 clientes piloto onboarded en mes 1 post-MVP
- [ ] **Time-to-onboard:** < 4 horas por cliente (vs 4-8h baseline)
- [ ] **NPS pilot:** > 7/10 en feedback post-onboarding

---

## 📋 SECCIÓN 5: Firmas

### Firmante 1: Chief Technology Officer

**Decisión:** ☐ GO Modelo A (AI-Driven)   ☐ GO Modelo B (Humano)   ☐ NO-GO

**Comentarios/Condiciones adicionales:**
```
_________________________________________________________________
_________________________________________________________________
```

**Nombre:** _______________________________
**Firma:** _______________________________
**Fecha:** _______________________________

---

### Firmante 2: Chief Financial Officer

**Decisión:** ☐ APROBAR S/ 141K (Modelo A)   ☐ APROBAR S/ 182K (Modelo B)   ☐ NO-GO

**Comentarios/Condiciones financieras:**
```
_________________________________________________________________
_________________________________________________________________
```

**Nombre:** _______________________________
**Firma:** _______________________________
**Fecha:** _______________________________

---

### Firmante 3: Head of Engineering

**Decisión:** ☐ APROBAR equipo + timeline   ☐ NO-GO

**Comentarios/Compromisos de equipo:**
```
_________________________________________________________________
_________________________________________________________________
```

**Nombre:** _______________________________
**Firma:** _______________________________
**Fecha:** _______________________________

---

## 📋 SECCIÓN 6: Próximos Pasos Post-Firma

### Si decisión es GO:

#### Día 1 (post-firma)
- [ ] Comunicar decisión al equipo
- [ ] Iniciar proceso de contratación (si Modelo B) o engagement Arquitecto AI (si Modelo A)
- [ ] Aprovisionar infraestructura Azure híbrida (1 semana setup)

#### Semana 1 (Sprint 0)
- [ ] Kickoff con equipo completo
- [ ] ADR training (especialmente 0048, 0039, 0047)
- [ ] CI/CD pipeline operativo
- [ ] Code review DBA externo agendado para TS-1.2

#### Semana 2 (Inicio Sprint 1)
- [ ] Comenzar desarrollo TS-1.1 (Tenant domain model)
- [ ] Hires confirmados (si Modelo B)
- [ ] Primer review con stakeholders

#### Sprint Review Cadence
- **Cada 2 semanas:** Demo a stakeholders + actualización ROI tracker
- **Sprint 3 (mid-MVP):** Go/No-Go intermedio sobre pivot scope si necesario
- **Sprint 6 (fin Modelo A) o Sprint 6 (fin Modelo B):** UAT + decisión Phase 2

### Si decisión es NO-GO:

- [ ] Documentar razones específicas
- [ ] Identificar condiciones que deben cumplirse para reconsiderar
- [ ] Re-evaluación agendada en: _______ semanas
- [ ] Comunicar al equipo de planificación

---

## 🔗 Documentos de Soporte para esta Decisión

| Pregunta del Firmante | Documento de Referencia | Tiempo Lectura |
|----------------------|------------------------|----------------|
| **"¿Por qué confiar en estos números?"** | [JUSTIFICACION-TIMELINE-AI-DRIVEN.md](./construction/JUSTIFICACION-TIMELINE-AI-DRIVEN.md) | 10 min |
| **"¿Cómo se compara con la competencia?"** | [COMPETITIVE-ANALYSIS.md](./construction/COMPETITIVE-ANALYSIS.md) | 10 min |
| **"¿Cómo logramos 50 clientes Y1?"** | [REVENUE-MODEL-YEAR-1.md](./construction/REVENUE-MODEL-YEAR-1.md) | 15 min |
| **"¿Detalle de costos por escenario?"** | [ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md](./construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) | 15 min |
| **"¿Qué pasa si X se retrasa?"** | [ESTIMATION-VALIDATION-MATRIX.md](./construction/ESTIMATION-VALIDATION-MATRIX.md) | 20 min |
| **"¿Resumen rápido?"** | [RESUMEN-EJECUTIVO-DIRECTORES.md](./RESUMEN-EJECUTIVO-DIRECTORES.md) | 5 min |
| **"¿Presentación visual?"** | [BOARD-PRESENTATION.md](./BOARD-PRESENTATION.md) | 20 min |

---

**Documento preparado por:** Arquitecto Principal
**Fecha:** 2026-05-15
**Última actualización:** 2026-05-15
**Estado:** ⏳ PENDIENTE DE FIRMA DE LOS 3 FIRMANTES

---

> 💡 **Nota:** Este documento es el "botón Sign Here" del governance. Una vez firmado por los 3 firmantes, este documento se archiva en `/governance/audits/` con fecha y se inicia el plan de ejecución.
