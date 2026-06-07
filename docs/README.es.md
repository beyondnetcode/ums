<div align="center">

# UMS — Sistema Empresarial de Gestión de Usuarios

> **Navegación Bilingüe:** [English](../README.md)

[![Status](https://img.shields.io/badge/Status-Activo-brightgreen?style=for-the-badge)]()
[![Platform](https://img.shields.io/badge/.NET_10_%7C_React_18-informational?style=for-the-badge)]()
[![Architecture](https://img.shields.io/badge/Evolith-Producto_Satélite-blueviolet?style=for-the-badge)](https://github.com/beyondnetcode/evolith_arch32)
[![ADRs](https://img.shields.io/badge/ADRs-32_decisiones-orange?style=for-the-badge)](./architecture/adrs/)
[![License](https://img.shields.io/badge/Licencia-Propietaria-red?style=for-the-badge)]()

<br/>

<a href="./diagrams/evolith-ums-satellite.png" title="Arquitectura E2E de Evolith — UMS Producto Satélite Oficial · clic para ampliar">
  <img src="./diagrams/evolith-ums-satellite.png"
       alt="Arquitectura E2E de Evolith — UMS Producto Satélite Oficial"
       width="780"
       style="border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.3);" />
</a>

<sub>↑ Framework de Arquitectura E2E de <a href="https://github.com/beyondnetcode/evolith_arch32">Evolith</a> · <strong>UMS — Producto Satélite Oficial</strong> · <i>clic para ampliar</i></sub>

<br/>

**UMS es un monolito modular para identidad, autorización, configuración, aprobaciones, cumplimiento, IGA y auditoría.**<br/>
Construido sobre **.NET 10 · SQL Server 2022 · React 18 · TypeScript · Nx**.<br/>
Satélite aplicado del framework de arquitectura corporativa [Evolith](https://github.com/beyondnetcode/evolith_arch32).

</div>

---

## 📑 Menú de Navegación Rápida

| Categoría | Punto de Entrada | Descripción |
|---|---|---|
| 📐 **Arquitectura** | [Portal de Arquitectura](./architecture/index.md) | Blueprints, ADRs, TEs, patrones canónicos |
| 🏛️ **ADRs** | [Registro de ADRs](./architecture/adrs/) | 32 decisiones arquitectónicas |
| 🧩 **Modelo de Dominio** | [Índice de Agregados](./domain/index.md) | Contextos acotados · agregados · entidades |
| 📦 **SDK** | [Portal SDK](./sdk/index.md) | .NET · TypeScript · NestJS |
| 📋 **Requisitos** | [Historias Funcionales](./governance/requirements/functional-stories/index.md) | Backlog completo del producto |
| 🚦 **Planificación** | [Backlog del Proyecto](./governance/project/index.md) | Épicas · MVP · gap tracker |
| ⚙️ **Operaciones** | [Portal de Operaciones](./operations/index.md) | Runbooks · métricas |
| 🧪 **QA** | [Reporte QA](./qa/qa_report.md) | Resultados de pruebas · cobertura · evidencias |
| 🏗️ **Infraestructura** | [Plan K8s](../infra/UMS_K8s_Deployment_Plan.md) | Guía de despliegue en Kubernetes |
| 📖 **Índice Completo** | [Índice Maestro](./MASTER_INDEX.md) | Navegación completa del ciclo de vida |
| 🔺 **Evolith Upstream** | [Framework Evolith](https://github.com/beyondnetcode/evolith_arch32) | Base de referencia arquitectónica |

---

## 🎯 Comienza Aquí — Elige Tu Camino

### Camino 1 — Vista General en 5 Minutos

📄 [Visión del Producto](./governance/product/product-vision.md) · [Vista General de Arquitectura](./architecture/overview.md) · [Matriz de Trazabilidad](./architecture/traceability-matrix.md)

*¿Qué es UMS? ¿Qué problema resuelve? ¿Cómo encaja con Evolith?*

### Camino 2 — Por Rol

| Rol | Comienza Aquí | Luego Lee |
|---|---|---|
| 🏛️ **Arquitecto** | [Portal de Arquitectura](./architecture/index.md) | [Registro ADR](./architecture/adrs/) · [Matriz de Trazabilidad](./architecture/traceability-matrix.md) |
| 👨‍💻 **Dev Backend** | [Patrones Canónicos](./architecture/artifacts/canonical-patterns/index.md) | [Agregados de Dominio](./domain/index.md) · [SDK .NET](./sdk/dotnet/README.md) |
| 🖥️ **Dev Frontend** | [ADR-0056: Arquitectura Limpia](./architecture/adrs/0056-clean-architecture-frontend.md) | [SDK TypeScript](./sdk/typescript/README.md) · [ADR-0057: Estado](./architecture/adrs/0057-zustand-tanstack-query-state.md) |
| 🛠️ **DevOps / SRE** | [Plan de Infraestructura](../infra/infrastructure_plan.md) | [Runbooks](./operations/runbooks/) · [Métricas](./operations/metrics/index.md) |
| 📦 **Producto / PM** | [Visión del Producto](./governance/product/product-vision.md) | [Gap Tracker](./governance/project/functional-story-gap-tracker.md) · [OKRs](./governance/product/objectives.md) |
| 🤖 **Contribuidor IA** | [AGENTS.md](../AGENTS.md) | [Plantilla ADR](./governance/sdlc/adr-template.md) |

### Camino 3 — Tomar una Decisión Arquitectónica

1. Revisa el [Registro ADR](./architecture/adrs/) — ¿ya existe la decisión?
2. Si no, usa la [Plantilla ADR](./governance/sdlc/adr-template.md)
3. Valida contra la [Matriz ADR de Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/adr-matrix.md) — ¿debería promoverse upstream?

---

## 📂 Estructura del Repositorio (Exploración Profunda)

### 📐 Arquitectura y Patrones

| Artefacto | Propósito |
|---|---|
| [Portal de Arquitectura](./architecture/index.md) | Entrada central para todos los artefactos de arquitectura |
| [Vista General de Arquitectura](./architecture/overview.md) | Diagrama de sistema y modelo por capas |
| [Matriz de Trazabilidad](./architecture/traceability-matrix.md) | Cobertura FS → ADR → Habilitador Técnico |
| [Blueprints](./architecture/blueprints/) | ER de base de datos, mapa servicio-entidad, diseño shell library |
| [Habilitadores Técnicos](./architecture/blueprints/technical-enablers/index.md) | TE-01 hasta TE-07 |
| [Patrones Canónicos](./architecture/artifacts/canonical-patterns/index.md) | CP-01 hasta CP-08 |
| [Referencia API (.NET)](./architecture/api-dotnet/) | Referencia del contrato HTTP |

### 🏛️ Registros de Decisiones Arquitectónicas

| ADR | Título | Evolith |
|---|---|---|
| [0050](./architecture/adrs/0050-naming-taxonomy-standard.md) | Estándar de Nomenclatura y Taxonomía | Adopta ADR-0056 |
| [0051](./architecture/adrs/0051-event-bus-injectable-port.md) | Puerto Inyectable de Event Bus | Adopta ADR-0015 |
| [0052](./architecture/adrs/0052-immutable-audit-trail-enforcement.md) | Auditoría Inmutable | Adopta ADR-0016 |
| [0053](./architecture/adrs/0053-opentelemetry-observability.md) | Observabilidad OpenTelemetry | Adopta ADR-0007 |
| [0054](./architecture/adrs/0054-shell-library-isolation.md) | Aislamiento de Shell Library | Específico UMS |
| [0055](./architecture/adrs/0055-graphql-rest-hybrid-api.md) | API Híbrida GraphQL/REST | Implementa ADR-0012 |
| [0056](./architecture/adrs/0056-clean-architecture-frontend.md) | Arquitectura Limpia Frontend | → Evolith nodejs/0044 |
| [0057](./architecture/adrs/0057-zustand-tanstack-query-state.md) | Zustand + TanStack Query | → Evolith nodejs/0045 |
| [0058](./architecture/adrs/0058-api-gateway-yarp-evolution.md) | API Gateway YARP | Implementa ADR-0008 |
| [0059](./architecture/adrs/0059-single-api-tier-decision.md) | Capa API Única | Override de baseline Evolith |
| [0060](./architecture/adrs/0060-aop-cross-cutting-concern-strategy.md) | Estrategia AOP Cross-Cutting | → Evolith dotnet/0072 |
| [0061](./architecture/adrs/0061-execution-context-accessor.md) | Accessor de Contexto de Ejecución | → Evolith dotnet/0064 |
| [0062](./architecture/adrs/0062-pii-safe-serilog-configuration.md) | Serilog Seguro para PII | → Evolith dotnet/0065 |
| [0063](./architecture/adrs/0063-idempotency-middleware.md) | Middleware de Idempotencia | → Evolith dotnet/0066 |
| [0064](./architecture/adrs/0064-lean-root-repository-taxonomy.md) | Taxonomía Lean de Repositorio Raíz | → Evolith core/0070 |
| [0065](./architecture/adrs/0065-no-raw-guids-in-ui.md) | Sin GUIDs Crudos en UI | → Evolith nodejs/0046 |
| [0066](./architecture/adrs/0066-actionable-user-error-contract.md) | Contrato de Error Accionable | → Evolith nodejs/0047 |
| [0067](./architecture/adrs/0067-modular-monolith-schema-per-domain.md) | Schema por Dominio en Monolito Modular | → Evolith core/0067 |
| [0068](./architecture/adrs/0068-feature-flag-system-scope.md) | Alcance del Sistema de Feature Flags | → Evolith nodejs/0048 |
| [0069](./architecture/adrs/0069-domain-inheritance-strategy.md) | Estrategia de Herencia de Dominio | → Evolith core/0071 |
| [0070](./architecture/adrs/0070-database-schema-strategy-decision.md) | Estrategia de Schema de Base de Datos | Adopta ADR-0067 |
| [0071](./architecture/adrs/0071-auth-graph-engine.md) | Motor de Grafo de Autorización | Específico UMS |
| [0072](./architecture/adrs/0072-dynamic-auth-method-resolution.md) | Resolución Dinámica de Método Auth | Específico UMS |
| [0073](./architecture/adrs/0073-ums-sdk-multi-runtime.md) | SDK UMS Multi-Runtime | Específico UMS |
| [0074](./architecture/adrs/0074-auth-graph-schema-versioning.md) | Versionado del Schema del Grafo Auth | Específico UMS |
| [0075](./architecture/adrs/0075-onboarding-approval-inbox-and-scope-based-authorization.md) | Bandeja de Aprobación de Onboarding | Específico UMS |
| [0076](./architecture/adrs/0076-utc-dates-timezone-language-resolution.md) | Fechas UTC y Resolución de Idioma | → Evolith core/0072 |
| [0077](./architecture/adrs/0077-tenant-portal-management-authorization-boundary.md) | Límite de Auth del Portal de Tenant | Específico UMS |
| [0078](./architecture/adrs/0078-ddd-domain-resource-hierarchy.md) | Jerarquía de Recursos DDD | Específico UMS |
| [0079](./architecture/adrs/0079-dependency-guard-policy.md) | Política de Guardia de Dependencias | Específico UMS |
| [0080](./architecture/adrs/0080-auth-graph-preview-internal-pipeline.md) | Pipeline de Preview del Grafo Auth | Específico UMS |
| [0081](./architecture/adrs/0081-semantic-auth-graph-client-contract.md) | Contrato Semántico del Grafo Auth | Específico UMS |

### 🧩 Modelo de Dominio

| Contexto Acotado | Agregados |
|---|---|
| **Identidad** | [Tenant](./domain/identity/tenant.md) · [UserAccount](./domain/identity/user-account.md) · [Delegación](./domain/identity/user-management-delegation.md) · [Grafo Auth](./domain/identity/auth-graph.md) · [Método Auth](./domain/identity/auth-method-resolution.md) |
| **Autorización** | [SystemSuite](./domain/authorization/system-suite.md) · [PermissionTemplate](./domain/authorization/permission-template.md) · [Profile](./domain/authorization/profile.md) |
| **Configuración** | [IdpConfiguration](./domain/configuration/idp-configuration.md) · [AppConfiguration](./domain/configuration/app-configuration.md) · [FeatureFlag](./domain/configuration/feature-flag.md) |
| **Aprobaciones** | [ApprovalWorkflow](./domain/approvals/approval-workflow.md) · [ApprovalRequest](./domain/approvals/approval-request.md) · [DocumentType](./domain/approvals/document-type.md) |
| **IGA** | [PromotionRequest](./domain/iga/promotion-request.md) · [RoleMaturityStatus](./domain/iga/role-maturity-status.md) |
| **Auditoría** | [AuditRecord](./domain/audit/audit-record.md) |

También: [Mapa de Contextos](./governance/construction/ddd-design/01-bounded-context-map.md) · [Flujos Cross-Context](./governance/construction/ddd-design/10-cross-context-flows.md) · [Primitivos DDD](./governance/construction/ddd-design/11-ddd-primitives.md)

### 📦 SDK

| Runtime | README | Inicio Rápido |
|---|---|---|
| **.NET** | [README](./sdk/dotnet/README.md) | [Quickstart](./sdk/dotnet/quickstart.md) |
| **TypeScript** | [README](./sdk/typescript/README.md) | [Quickstart](./sdk/typescript/quickstart.md) |
| **NestJS** | [README](./sdk/nestjs/README.md) | [Quickstart](./sdk/nestjs/quickstart.md) |

Contratos: [Vista General del Schema](./sdk/contracts/schema-overview.md) · [Códigos de Error](./sdk/contracts/error-codes.md) · [Matriz de Compatibilidad](./sdk/contracts/compatibility-matrix.md) · [Contrato Semántico](./sdk/contracts/semantic-client-contract.md)

### 📋 Producto y Requisitos

| Artefacto | Propósito |
|---|---|
| [Visión del Producto](./governance/product/product-vision.md) | Estrategia, objetivos y posicionamiento de mercado |
| [Contexto de Negocio](./governance/product/business-context.md) | Espacio del problema y contexto de mercado |
| [Alcance y Límites](./governance/product/scope.md) | Qué es y qué no es UMS |
| [Objetivos (OKRs)](./governance/product/objectives.md) | Criterios de éxito medibles |
| [Stakeholders](./governance/product/stakeholders.md) | Roles y responsabilidades |
| [Glosario](./governance/requirements/glossary.md) | Lenguaje ubicuo |
| [Historias Funcionales](./governance/requirements/functional-stories/index.md) | Backlog completo de requisitos |
| [Modelo de Datos Conceptual](./governance/requirements/conceptual-data-model.md) | Modelo de dominio de alto nivel |
| [Matriz de Permisos](./governance/requirements/permission-matrix-example.md) | Referencia de roles y permisos |

### 🚦 Planificación y Backlog

| Artefacto | Propósito |
|---|---|
| [Backlog del Proyecto](./governance/project/index.md) | Épicas y planificación de sprints |
| [Backlog MVP del Producto](./governance/project/mvp-product-backlog.md) | Alcance MVP priorizado |
| [Gap Tracker](./governance/project/functional-story-gap-tracker.md) | Estado de implementación por historia |
| [Épica 06: Aprobaciones](./governance/project/ep-06-approvals-detailed-design.md) | Diseño detallado del flujo de aprobaciones |
| [Épica 07: Cumplimiento](./governance/project/ep-07-compliance-detailed-design.md) | Diseño del módulo de cumplimiento |
| [Épica 08: IGA](./governance/project/ep-08-iga-detailed-design.md) | Diseño de Identity Governance |

### ⚙️ Operaciones

| Artefacto | Propósito |
|---|---|
| [Dashboard de Métricas](./operations/metrics/index.md) | Métricas de API, frontend, pruebas y agregadas |
| [RB-01: Respuesta a Incidentes](./operations/runbooks/rb-01-incident-response.md) | Playbook de guardia para incidentes |
| [RB-02: Procedimiento de Rollback](./operations/runbooks/rb-02-rollback-procedure.md) | Pasos seguros de rollback |
| [RB-03: Recuperación de Caché](./operations/runbooks/rb-03-cache-failure-recovery.md) | Recuperación de fallos en Redis |
| [RB-04: Failover de Base de Datos](./operations/runbooks/rb-04-database-failover.md) | Procedimiento de failover SQL Server |
| [Anonimización DB de Dev](./operations/runbooks/dev-db-anonymization.md) | Anonimización de PII para entornos dev |
| [Retención de Backups GDPR](./operations/runbooks/gdpr-backup-retention-policy.md) | Cumplimiento de retención de backups |

### 🧪 QA y Pruebas

| Artefacto | Propósito |
|---|---|
| [Reporte QA](./qa/qa_report.md) | Estado general de QA |
| [Resultados de Pruebas Unitarias](./governance/testing/unit-testing-results.md) | Cobertura y resultados de pruebas unitarias |
| [Resultados de Pruebas de Integración](./governance/testing/integration-testing-results.md) | Estado de pruebas de integración |
| [Plan de Pruebas de Rendimiento](./governance/testing/performance-testing-plan.md) | Estrategia de pruebas de carga |
| [Resultados de Pruebas de Rendimiento](./governance/testing/performance-testing-results.md) | Resultados de pruebas de carga |
| [Evidencias QA](./qa/evidences/) | Capturas US-001 hasta US-008 |

### 🏗️ Infraestructura

| Artefacto | Propósito |
|---|---|
| [Plan de Despliegue K8s](../infra/UMS_K8s_Deployment_Plan.md) | Guía completa de despliegue en Kubernetes |
| [Plan de Infraestructura](../infra/infrastructure_plan.md) | Diseño de infraestructura |
| [Plan de Implementación](../infra/implementation_plan.md) | Pasos de implementación del despliegue |
| [Guía de Acceso Local](../infra/accesos_local.md) | Endpoints del entorno local |

---

## 🔧 Desarrollo Local

```bash
# Instalar todas las dependencias
cd src && npm install

# Frontend (React · Vite · puerto 5173)
npx nx run app-web:dev

# Backend (.NET 10 · puerto 7114)
cd src/apps/ums.api && dotnet build && dotnet run

# Ejecutar todas las pruebas
cd src && npx nx run-many --target=test --all

# Validar consistencia de documentación
python .bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

> **Nota:** Después de cualquier cambio que requiera recarga del servidor, detén el backend (:7114) y el frontend (:5173) y reinicia ambos.

---

## 🔺 UMS vs Evolith — Qué Va Dónde

| Pregunta | Evolith (Upstream) | UMS (Producto Satélite) |
|---|---|---|
| **¿Qué pertenece aquí?** | Estándares reutilizables, ADRs cross-producto, patrones canónicos, gobernanza | Implementación específica del producto, agregados de dominio, schemas, seeds |
| **¿Cómo contribuye un producto?** | Promover un ADR respaldado por evidencia real de UMS | Proveer prueba ejecutable y luego proponer upstream |
| **¿Qué se mantiene local?** | Las políticas enterprise requieren revisión de gobernanza | Rutas del producto, configs de tenant, branding, ADRs específicos de UMS |

13 ADRs de UMS han sido promovidos a Evolith. Ver la [tabla de ADRs](#-registros-de-decisiones-arquitectónicas) (columna → Evolith).

---

## 🤝 Contribución

Antes de contribuir, lee:

- [AGENTS.md](../AGENTS.md) — Reglas y convenciones para agentes
- [Estándares](./STANDARDS.md) — Estándares de ingeniería
- [Plantilla ADR](./governance/sdlc/adr-template.md) — Cómo proponer una decisión
- [Guía de Herencia de Repositorios Hijo de Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/onboarding/child-repository-inheritance-guide.md) — Cómo UMS hereda de Evolith

---

## 📋 Todos los Índices de Navegación

| Índice | Propósito |
|---|---|
| [Índice Maestro](./MASTER_INDEX.md) | Navegación completa del ciclo de vida (Fase 00–06) |
| [Índice de Arquitectura](./architecture/index.md) | Todos los artefactos de arquitectura |
| [Índice de Dominio](./domain/index.md) | Todos los agregados por contexto acotado |
| [Portal SDK](./sdk/index.md) | Todos los runtimes y contratos del SDK |
| [Portal de Operaciones](./operations/index.md) | Runbooks y métricas |

---

<div align="center">
  <sub>UMS — Sistema Empresarial de Gestión de Usuarios · Satélite de <a href="https://github.com/beyondnetcode/evolith_arch32">Evolith</a> · .NET 10 · React 18 · Monolito Modular</sub>
</div>
