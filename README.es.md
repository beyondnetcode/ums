# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular estandarizado para Identidad y Autorización Unificada.**
>
> ![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## Language / Bilingüe

- [English](./README.md) | **Español**

---

## PARA DIRECTORES / CTO / INVERSORES — COMIENZA AQUÍ (5 min)

> **¿Tienes 5 minutos para una decisión de inversión?**

| # | Documento | Tiempo | Propósito |
|---|-----------|--------|-----------|
| 1 | **[Resumen Ejecutivo (ES/EN)](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md)** | 5 min | Inversión S/ 141K-195K, ROI Y1 84-112%, timeline 8.5-12 semanas, GO/NO-GO |
| 2 | **[Matriz de Decisión](./docs/governance/DECISION-MATRIX.md)** | 5 min | Formulario de aprobación: CTO + CFO + Líder de Ingeniería |
| 3 | **[Presentación al Directorio (12 slides)](./docs/governance/BOARD-PRESENTATION.md)** | 20 min | Deck en markdown para reunión del directorio (exportable a PDF/PPTX) |

**Cifras clave:** S/ 141K AI-Driven (8.5 semanas) | S/ 182K Humano (12 semanas) | ROI Y1 84%-112% | Payback 3 meses | LTV/CAC 14.9x

---

## Índice Maestro de Navegación

Empieza aquí si eres nuevo en UMS. Este índice le da a cada lector una ruta rápida dentro del repositorio sin necesidad de conocer la estructura de carpetas.

| Quiero... | Empezar Aquí | Luego Leer |
| :--- | :--- | :--- |
| Entender el producto | [Visión de Producto](./docs/governance/product-es/product-vision.md) | [Contexto de Negocio](./docs/governance/product-es/business-context.md) → [Alcance](./docs/governance/product-es/scope.md) |
| Ver Épicas y Prioridades | [Backlog de Producto MVP](./docs/governance/project-es/mvp-product-backlog.md) | [Priorización MVP](./docs/governance/roadmap/mvp-functional-prioritization.es.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) |
| Revisar requisitos funcionales | [Índice de Requisitos](./docs/governance/requirements-es/index.md) | [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Glosario](./docs/governance/requirements-es/glossary.md) |
| Validar datos y modelo de dominio | [Modelo de Datos Conceptual](./docs/governance/requirements-es/conceptual-data-model.md) | [Formatos de Exportación ER](./docs/architecture/blueprints-es/er-export-formats.md) → [Diseño ER de Base de Datos](./docs/architecture/blueprints-es/database-design-er.md) |
| Entender la arquitectura | [Portal de Arquitectura](./docs/architecture/index.md) | [Especificación C4](./docs/architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./docs/architecture/adrs-es/index.md) |
| **Aprobar Inversión / Decidir** | [**Resumen Ejecutivo**](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) | [Matriz de Decisión](./docs/governance/DECISION-MATRIX.md) → [Presentación al Directorio](./docs/governance/BOARD-PRESENTATION.md) → [Modelo de Revenue](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) |
| **Planificar y Ejecutar Construcción** | [**Índice de Construcción**](./docs/governance/construction/ESTIMATION-INDEX.md) | [Alcance MVP (ES)](./docs/governance/construction/MVP-REDUCIDO-ALCANCE-Y-ESTIMACION.md) → [Análisis de Costos](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Modelos de Ejecución](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) |
| Construir u operar el sistema | [Engine Room](./src/) | [Habilitadores Técnicos](./docs/architecture/blueprints-es/technical-enablers/index.md) → [Portal de Operaciones](./docs/operations/index.md) |
| Navegar todo | [Índice Maestro](./docs/MASTER_INDEX.es.md) | Árbol completo de documentación por fase de ciclo de vida. |

## Inicio Rápido (Motor Técnico)

```powershell
cd src
npm install; npx nx run app-web:dev
```

---

## Hub de Conocimiento

| Dominio | Índice del Portal | Contenidos |
| :--- | :--- | :--- |
| **Gobernanza** | [Portal de Gobernanza](./docs/governance/index.md) | Dirección de producto, estándares, roadmap, backlog de proyecto y auditorías. |
| **Entrega de Proyecto** | [Backlog de Proyecto](./docs/governance/project-es/index.md) | Épicas MVP, historias de usuario, orden de prioridad, fases y línea de corte. |
| **Planificación de Construcción** | [**Índice de Construcción**](./docs/governance/construction/ESTIMATION-INDEX.md) | **Alcance MVP (168 pts, 12 semanas), análisis de costos (3 escenarios), modelos de ejecución (Humano vs AI-Driven), composición del equipo, historias técnicas (89 TS), mitigación de riesgos.** [Ver Índice Completo](./docs/governance/construction/README.md) |
| **Requisitos** | [Índice de Requisitos](./docs/governance/requirements-es/index.md) | Historias funcionales, glosario de negocio, modelo de permisos y modelo conceptual. |
| **Arquitectura** | [Portal de Arquitectura](./docs/architecture/index.md) | Stack, registro ADR, especificación C4, contextos delimitados y habilitadores técnicos. |
| **Infraestructura** | [Infraestructura](./docs/infrastructure/index.md) | Docker, Kong, Kubernetes y configuración de ambientes. |
| **Operaciones** | [Portal de Operaciones](./docs/operations/index.md) | Runbooks, observabilidad, operación SQL y prácticas SRE. |
| **Conocimiento** | [Base de Conocimiento](./docs/knowledge/index.md) | Rutas de lectura recomendadas, POCs, investigación y onboarding. |

## Lectura Recomendada por Rol

| Perfil | Objetivo de Lectura | Ruta de Aprendizaje (Links Directos) |
| :--- | :--- | :--- |
| **Director / Ejecutivo / Inversor** | Decisión de inversión: costo, ROI, timeline, riesgo | [**Resumen Ejecutivo (5 min)**](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [**Matriz de Decisión**](./docs/governance/DECISION-MATRIX.md) → [**Presentación al Directorio**](./docs/governance/BOARD-PRESENTATION.md) → [Visión de Producto](./docs/governance/product-es/product-vision.md) |
| **Product Owner** | Alcance funcional, secuencia y propiedad del backlog | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Requisitos](./docs/governance/requirements-es/index.md) → [Priorización MVP](./docs/governance/roadmap/mvp-functional-prioritization.es.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [**Alcance MVP y Mapeo TS**](./docs/governance/construction/README.md) |
| **Analista de Negocio** | Narrativa de negocio, reglas y criterios de aceptación | [Requisitos](./docs/governance/requirements-es/index.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Glosario](./docs/governance/requirements-es/glossary.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) |
| **Arquitecto de SW** | Diseño técnico, decisiones y límites de dominio | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Portal de Arquitectura](./docs/architecture/index.md) → [Especificación C4](./docs/architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./docs/architecture/adrs-es/index.md) |
| **Developer (BE/FE)** | Qué construir, por qué importa y cómo encaja | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Habilitadores Técnicos](./docs/architecture/blueprints-es/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Ambientes, confiabilidad y observabilidad | [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Infraestructura](./docs/infrastructure/index.md) → [Portal de Operaciones](./docs/operations/index.md) → [Estrategia de Observabilidad](./docs/architecture/artifacts-es/observability-strategy.md) |
| **QA / Seguridad** | Calidad, riesgos y estrategia de verificación | [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Plan de Pruebas de Contrato](./docs/architecture/artifacts-es/contract-testing-plan.md) → [Especificación IAM](./docs/architecture/artifacts-es/enterprise-iam-ums-specification.md) → [Modelo de Madurez](./docs/architecture/artifacts-es/architecture-maturity-model.md) |
| **Finanzas / Propietario de Presupuesto** | Estimación de costos, ROI y trade-offs de infraestructura | [**Análisis de Costos (3 escenarios)**](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [**Comparación de Modelos de Ejecución**](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) → [Alcance MVP](./docs/governance/construction/ESTIMATION-INDEX.md) |
| **Líder de Ingeniería / Arquitecto Técnico** | Qué construir, composición del equipo y ruta crítica | [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [**Índice de Construcción (89 TS, equipo, dependencias)**](./docs/governance/construction/ESTIMATION-INDEX.md) → [Historias Técnicas](./docs/governance/construction/README.md) → [Portal de Arquitectura](./docs/architecture/index.md) |
| **IA / Agentes** | BMAD-METHOD y reglas del repositorio | [Reglas de Agentes](./AGENTS.md) → [Auditoría BMAD](./docs/architecture/artifacts-es/bmad-master-audit-alignment-report.md) → [Auditoría Taxonomía](./docs/governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## Portal Ejecutivo y de Directorio — Decisión de Inversión (MVP S/ 141K-195K)

**Para Directores, CTO, Inversores — Documentos Listos para Decisión:**

| Necesidad | Documento | Tiempo | Información Clave |
| :--- | :--- | :--- | :--- |
| **Resumen ejecutivo de 5 min** | [Resumen Ejecutivo (ES/EN)](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) | **5 min** | Inversión, ROI, timeline, recomendación GO/NO-GO |
| **Firmar decisión Go/No-Go** | [Matriz de Decisión](./docs/governance/DECISION-MATRIX.md) | 5 min | Formulario de aprobación: CTO + CFO + Líder de Ingeniería |
| **Pitch para reunión de directorio** | [Presentación al Directorio](./docs/governance/BOARD-PRESENTATION.md) | 20 min | 12 slides: problema → solución → costo → ROI → solicitud |
| **Revenue y unit economics** | [Modelo de Revenue Y1](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) | 15 min | CAC, LTV (14.9x), plan de ramp-up a 50 clientes, sensibilidad |
| **Posicionamiento competitivo** | [Análisis Competitivo](./docs/governance/construction/COMPETITIVE-ANALYSIS.md) | 10 min | vs Okta/Auth0/Azure AD; TCO ahorra S/ 164K-352K (3 años) |

---

## Portal de Planificación de Construcción (MVP Reducido — 8.5-12 semanas)

**Navegación Rápida por Necesidad:**

| Necesidad | Documento | Tiempo | Información Clave |
| :--- | :--- | :--- | :--- |
| **Ver plan completo de construcción** | [Índice de Construcción](./docs/governance/construction/ESTIMATION-INDEX.md) | 5 min | Índice maestro bilingüe con rutas de lectura por rol |
| **Entender alcance y equipo del MVP** | [README (Construcción)](./docs/governance/construction/README.md) | 10 min | 8 FS, 168 pts, equipo de 4 personas, 12 semanas, DoD por épica |
| **Presupuesto y análisis de costos** | [Análisis Costo/Beneficio](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) | 15 min | 10 perfiles, 3 escenarios (On-Prem/Híbrido/Nube), S/ 182K recomendado, ROI 84% Y1 |
| **Decisión Humano vs AI-Driven** | [Modelos de Ejecución](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) | 20 min | Humano (S/ 182K, 12 sem, ROI 382%) vs AI (S/ 141K, 8.5 sem, ROI 729%), matriz de riesgos |
| **Credibilidad del Timeline AI** | [Justificación Timeline AI](./docs/governance/construction/JUSTIFICACION-TIMELINE-AI-DRIVEN.md) | 10 min | Desglose honesto: agentes 12%, validación 59%, setup 15%, retrabajo 10% |
| **Detalle de historias técnicas** | [Historias Técnicas y Equipo](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) | 60 min | 89 TS en 8 épicas, perfiles de equipo, rutas críticas |
| **Trazabilidad FS-a-TS** | [Mapeo FS](./docs/governance/construction/MAPEO-FS-A-TS.md) | 30 min | Validación de cobertura de requisitos, alineación de criterios de aceptación |
| **Diseño RLS y Seguridad** | [Construcción README](./docs/governance/construction/README.md) | 30 min | Modelo de dos capas, SESSION_CONTEXT, manejo de errores, failover |
| **Arquitectura e Implementación** | [Plan de Servicio](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) | 40 min | Estructura .NET 8, pirámide de pruebas, CI/CD, patrones DI |

---

**Rutas de Lectura por Rol:**

| Rol | Ruta | Duración |
| :--- | :--- | :--- |
| **Directorio / Inversores** | [Resumen Ejecutivo](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [Presentación al Directorio](./docs/governance/BOARD-PRESENTATION.md) → [Matriz de Decisión](./docs/governance/DECISION-MATRIX.md) | **30 min** |
| **C-Level / Finanzas** | [Resumen Ejecutivo](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [Análisis de Costos](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Modelo de Revenue](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) → [Competitivo](./docs/governance/construction/COMPETITIVE-ANALYSIS.md) | 50 min |
| **Product Owner** | [Construcción README](./docs/governance/construction/README.md) → [Mapeo FS](./docs/governance/construction/MAPEO-FS-A-TS.md) → [Matriz de Validación](./docs/governance/construction/MATRIZ-VALIDACION-ESTIMACION.md) | 40 min |
| **Líder de Ingeniería** | [Índice de Construcción](./docs/governance/construction/ESTIMATION-INDEX.md) → [Historias Técnicas](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [RLS Deep Dive](./docs/governance/construction/README.md) | 90 min |
| **Líder de QA** | [Construcción README](./docs/governance/construction/README.md) → [Mapeo FS](./docs/governance/construction/MAPEO-FS-A-TS.md) → [Matriz de Validación](./docs/governance/construction/MATRIZ-VALIDACION-ESTIMACION.md) | 45 min |
| **DevOps / SRE** | [Plan de Servicio](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) → [Implementación RLS](./docs/governance/construction/README.md) | 60 min |
| **Developer** | [Construcción README](./docs/governance/construction/README.md) → [Historias Técnicas](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [Plan de Servicio](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) | 90 min |

---

## Contribución y Gobernanza

- **Flujo**: Este repo utiliza [BMAD-METHOD](./AGENTS.md) para documentación dirigida por especificaciones.
- **Navegación**: Visita el [**Índice Maestro**](./docs/MASTER_INDEX.es.md) para ver el árbol completo de documentos.
