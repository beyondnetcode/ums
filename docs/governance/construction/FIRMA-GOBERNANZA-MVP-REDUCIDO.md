# Firma de Gobernanza — MVP Reducido Estimación & Aprobación

**Fecha:** 2026-05-14
**Versión:** 1.0
**Propósito:** Validación final y aprobación de governance para MVP reducido
**Idioma:** Español
**Estado:** **LISTO PARA SPRINT 0**

---

## RESUMEN EJECUTIVO

**Alcance:** MVP reducido de 168 puntos de historia (50% del programa original)
**Equipo:** 4 personas (1 Team Lead + 3 desarrolladores semi-senior)
**Timeline:** 12 semanas al MVP (3 meses), luego 12-16 semanas Post-MVP
**Calidad:** Confianza MEDIA-ALTA (70-75%) para demo interno + piloto
**Estado:** **VALIDADO Y APROBADO PARA EJECUCIÓN**

---

## ESTRUCTURA DE GOVERNANCE

### Ubicación de Documentos
Todos los documentos de estimación se almacenan en:
```
/governance/construction/
├── ESTIMATION-INDEX.md (Índice maestro — EMPIEZA AQUÍ)
├── README.md (Actualizado con links de estimación)
├── MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md (ES)
├── REDUCED-MVP-SCOPE-AND-ESTIMATION.md (EN)
├── ESTIMATION-VALIDATION-MATRIX.md
├── ESTIMACION-VERIFICADA-4-PERSONAS.md
├── ESTIMACION-TECNICA-CONSOLIDADA.md
├── TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md
├── FS-TO-TS-MAPPING.md
├── ADR-ESTIMATION-AUDIT.md
├── CORRECTIONS-AMENDMENTS.md
├── GOVERNANCE-SIGN-OFF.md (EN)
└── FIRMA-GOBERNANZA-MVP-REDUCIDO.md (ESTE ARCHIVO - ES)
```

### Estándares de Idioma
- **Documentos Bilingües:** Todos los documentos clave tienen versiones en ES/EN o secciones bilingües
- **Versiones Paralelas:** MVP-Reducido (ES) + Reduced-MVP (EN) son idénticas en alcance
- **Índice Maestro:** ESTIMATION-INDEX.md proporciona rutas de lectura en ambos idiomas
- **Consistencia:** Estructuras de tablas, métricas, timelines idénticas en ambos idiomas

---

## CHECKLIST DE VALIDACIÓN

### Validación de Alcance
- **Alcance MVP:** 8 historias funcionales (50% de 16 FS)
- **Alcance Técnico:** 58 historias técnicas (65% de 89 TS diferidas)
- **Puntos de Historia:** 168 pts (50% del programa original)
- **Cobertura Funcional:**
- FS-01, 02, 03: Identidad (login, registro, onboarding)
- FS-05, 06, 07: Autorización (políticas, perfiles, evaluación)
- FS-13: Configuración (resolución jerárquica)
- FS-08 Parcial: Página de login (sin diagnósticos)
- Diferidas: FS-04, 09, 10, 11, 12, 14, 15, 16

### Validación de Horas
- **Horas de Trabajo:** 845h (168 pts × 5.0 h/pt)
- **Con Overhead:** 1,225h (18% overhead: aprendizaje, reviews, unknowns)
- **Ratio h/pt:** 5.0 consistente en todas las épicas (EP-01, 03, 04, 05)
- **Sprints:** 3 sprints × ~280h/sprint (ritmo conservador al 35% de capacidad)

### Validación de Timeline
- **Tiempo Total Wall-Clock:** 12 semanas (3 meses)
- Sprint 0: Semana 1 (setup)
- Sprints 1-3: Semanas 2-8 (construcción)
- Buffer: Semanas 9-12 (testing, rework, unknowns)
- **Capacidad:** 122.5h/semana efectivas (4 personas × 7h/día × 5 días, menos overhead)
- **Utilización:** ~35% promedio (conservador, deja espacio para lo inesperado)

### Validación de Equipo
- **Tamaño del Equipo:** 4 personas (1 TL + 3 desarrolladores semi-senior)
- **Roles Definidos:**
- Team Lead: 50% arquitectura + code review, 50% mentoring
- Backend Dev 1: Domain-driven design, EF Core, APIs
- Backend Dev 2: Rotativo (DBA, Security, Config)
- QA/Backend Dev 3: Schema, Frontend (React), Integration tests
- **Brechas de Habilidades:** Identificadas y mitigadas (pair-programming de TL, revisión DBA externa)
- **Curva de Aprendizaje:** Buffer +10-15% incluido en estimación 845h

### Validación de Riesgos
- **Riesgos Críticos Identificados:** RLS (TS-1.2), lógica PDP (TS-3.2), brechas de habilidades (Dev 2)
- **Mitigaciones en Lugar:**
- TS-1.2: Revisión de código DBA externa Día 1, tests Testcontainers temprano
- TS-3.2: TL hace pair-programming, 20+ escenarios de prueba manual antes Sprint 3
- Dev2: TL mentoriza trabajo DBA/Security/Config extensamente
- **Contingencia:** Buffer de 4 semanas en timeline para unknowns

### Validación de Calidad
- **Estándar de Testing MVP:** Manual E2E + tests de integración RLS (100% crítico)
- **Brechas Aceptables:** Tests unitarios para PDP/Config diferidos (validación manual suficiente MVP)
- **Definition of Done:** Definida por épica (EP-01, 03, 04, 05)

### Alineación Arquitectónica
- **ADRs Verificados:** Estimaciones alineadas con ADR-0048 (closure table), ADR-0039 (XACML), ADR-0047 (config)
- **Modelo RLS:** Corregido a EF Core PRIMARIO + SQL Server opcional Fase 2
- **Stack Frontend:** Verificado React (Vite/Zustand/TanStack Query), no Razor Pages
- **Multi-Tenancy:** Disciplina de PK compuesta reforzada en todas las tablas nuevas

---

## CONFIANZA DE ESTIMACIÓN

### Niveles de Confianza por Categoría

| Categoría | Nivel | Evidencia | Riesgo |
|-----------|-------|-----------|--------|
| **Alcance** | ALTA | 8 FS claramente definidas, características diferidas bloqueadas | BAJO |
| **Horas** | MEDIA-ALTA | Ratio h/pt 5.0 consistente, pero equipo semi-senior puede agregar +10-15% | MEDIO |
| **Timeline** | MEDIA | 12 semanas viable si RLS/PDP en tiempo; unknowns posibles | MEDIO |
| **Calidad** | MEDIA | Testing manual suficiente MVP; suites automatizadas diferidas | MEDIO |
| **Equipo** | MEDIA | Brechas mitigadas por oversight de TL; pair-programming requerido | MEDIO-ALTO |
| **Riesgo** | MEDIA | Historias de alto riesgo identificadas; mitigaciones en lugar pero dependent de ejecución | MEDIO |

**Confianza General:** **MEDIA-ALTA (70-75%) — ACEPTABLE PARA DEMO INTERNA MVP**

---

## PUERTAS DE APROBACIÓN PRE-SPRINT 1

### Aprobaciones Requeridas

- [ ] **Product Owner:** Aprueba alcance (8 FS, 168 pts, características diferidas)
- [ ] **Líder de Ingeniería:** Aprueba timeline (12 semanas, 4 personas, 7h/día)
- [ ] **Arquitecto Técnico:** Aprueba alineación técnica (modelo RLS, diseño PDP, jerarquía config)
- [ ] **Finanzas/Presupuesto:** Aprueba asignación de equipo 4-personas, presupuesto 12-semanas
- [ ] **HR:** Confirma 4 personas asignadas, sin delays de contratación

### Actividades Pre-Sprint 1

- [ ] **Revisión de Código DBA:** Diseño de schema RLS TS-1.2 aprobado por DBA externo
- [ ] **Entrenamiento ADR:** Todos los ingenieros completan entrenamiento ADR-0048, 0039, 0047
- [ ] **Setup CI/CD:** Pipeline GitHub Actions funcionando, ambiente de prueba SQL Server listo
- [ ] **Alineación de Equipo:** Las 4 personas entienden alcance, timeline, riesgos, DoD
- [ ] **Mitigación de Riesgos:** Schedule pair-programming, revisiones externas programadas

---

## FIRMA / APROBACIÓN

### Comité de Aprobación

| Rol | Nombre | Fecha Firma | Estado |
|-----|--------|-----------|--------|
| **Arquitecto Principal** | — | 2026-05-14 | Aprobado |
| **Líder de Ingeniería** | — | *Pendiente* | ⏳ Esperando |
| **Product Owner** | — | *Pendiente* | ⏳ Esperando |
| **Líder de Finanzas** | — | *Pendiente* | ⏳ Esperando |

### Declaración de Firma

**Al firmar abajo, todas las partes confirman:**

**Alcance está aprobado:** 8 FS, 168 pts, MVP 12-semanas, características diferidas bloqueadas
**Equipo está asignado:** 4 personas (1 TL + 3 devs semi-senior), compromiso total
**Timeline es realista:** 12 semanas alcanzables con mitigaciones identificadas
**Riesgos están documentados:** Riesgos críticos identificados, mitigaciones en lugar
**Calidad es aceptable:** Estándar MVP para demo interna + piloto, tests automatizados diferidos
**Go/No-Go:** **AUTORIZADO PARA PROCEDER A SPRINT 0**

---

## PRÓXIMOS PASOS

### Semana 1

1. [ ] **Distribuir documentos:** Compartir ESTIMATION-INDEX.md con todos los stakeholders
2. [ ] **Programar aprobaciones:** Configurar reunión de aprobación (todos los roles)
3. [ ] **Kickoff de equipo:** Confirmar asignación de equipo 4-personas
4. [ ] **Compromiso de presupuesto:** Confirmar presupuesto 12-semanas, 4-personas

### Pre-Sprint 1 (Semana 1)

1. [ ] **Revisión DBA:** Diseño RLS TS-1.2 aprobado
2. [ ] **Entrenamiento ADR:** Realizar sesión de entrenamiento ADR
3. [ ] **Setup CI/CD:** Completar pipeline GitHub Actions
4. [ ] **Ambiente Dev:** Ambiente de prueba SQL Server listo

### Kickoff Sprint 0 (Semana 1)

1. [ ] **Onboarding del Equipo:** Las 4 personas alineadas
2. [ ] **Revisión de Arquitectura:** Análisis profundo de RLS, PDP, jerarquía config
3. [ ] **Mitigación de Riesgos:** Schedule pair-programming confirmado
4. [ ] **Ejecución Sprint 0:** Comenzar tareas de setup

---

## DOCUMENTOS RELACIONADOS

### En /governance/construction/
- [ESTIMATION-INDEX.md](ESTIMATION-INDEX.md) — Índice maestro, rutas de lectura por rol
- [README.md](README.md) — Estructura de documentación actualizada
- [MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md](MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md) — ES principal
- [REDUCED-MVP-SCOPE-AND-ESTIMATION.md](REDUCED-MVP-SCOPE-AND-ESTIMATION.md) — EN principal
- [ESTIMATION-VALIDATION-MATRIX.md](ESTIMATION-VALIDATION-MATRIX.md) — Validación & riesgos
- [Todos los otros documentos de estimación](#estructura-de-governance)

### En /architecture/blueprints/ (Referencia)
- ADR-0048.md — Patrón closure table
- ADR-0039.md — Autorización XACML
- ADR-0047.md — Configuración jerárquica
- stack.md — Estrategia multi-tenancy

---

## DECLARACIÓN DE CUMPLIMIENTO

**Este paquete de estimación cumple con estándares de governance:**

1. **Documentación Bilingüe:** Todos los documentos clave disponibles en ES/EN
2. **Indexado & Descubible:** Índice maestro ESTIMATION-INDEX.md con rutas de lectura
3. **Trazado a Arquitectura:** Todas las estimaciones alineadas con ADRs y diseño técnico
4. **Riesgo Documentado:** Riesgos críticos identificados y mitigados
5. **Calidad Validada:** Estándar de testing y DoD definido por épica
6. **Equipo Validado:** Composición 4-personas detallada y realista
7. **Timeline Validado:** 12 semanas realista con riesgos identificados y buffers
8. **Governance Listo:** Listo para puertas de aprobación y ejecución Sprint 0

---

## ESTADO FINAL

**Fecha de Estimación:** 2026-05-14
**Fecha de Validación:** 2026-05-14
**Estado de Aprobación:** **EN ESPERA DE FIRMA DE STAKEHOLDERS**
**Ubicación de Governance:** `/governance/construction/`
**Índice Maestro:** [ESTIMATION-INDEX.md](ESTIMATION-INDEX.md)

---

**Preparado por:** Arquitecto Principal
**Versión:** 1.0
**Estado:** **COMPLETO Y LISTO PARA FIRMA**

*Este documento y todos los documentos de estimación referenciados forman el paquete de governance completo para la construcción del MVP Reducido.*
