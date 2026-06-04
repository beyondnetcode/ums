# Índice Maestro -- Hub de Navegación de UMS

> **Idioma:** [English](./MASTER_INDEX.md) | [Español](./MASTER_INDEX.es.md)

Especificaciones de ingeniería y ciclo de vida del producto para el User Management System (UMS).

UMS es una implementación aplicada basada en Evolith. Evolith es la fuente de verdad para los estándares reutilizables; UMS documenta evidencia aplicada, decisiones locales y desviaciones justificadas mediante ADR.

---

### Fase 00 -- Visión del Producto

- [Visión del Producto](./governance/product-es/product-vision.md)
- [Contexto de Negocio](./governance/product-es/business-context.md)
- [Alcance y Límites](./governance/product-es/scope.md)
- [Objetivos (OKRs)](./governance/product-es/objectives.md)
- [Stakeholders](./governance/product-es/stakeholders.md)

### Fase 01 -- Requerimientos de Dominio

- [Glosario (Lenguaje Ubicuo)](./governance/requirements-es/glossary.md)
- [Modelo de Datos Conceptual](./governance/requirements-es/conceptual-data-model.md)
- [Matriz de Permisos](./governance/requirements-es/permission-matrix-example.md)
- [Historias Funcionales](./governance/requirements-es/functional-stories/index.md)

### Fase 02 -- Planificación MVP y Backlog del Proyecto

- [Backlog del Proyecto](./governance/project-es/index.md)
- [Backlog del Producto MVP](./governance/project-es/mvp-product-backlog.md)
- [Seguimiento de Brechas de Historias Funcionales](./governance/project-es/functional-story-gap-tracker.md)
- [Épica 06: Aprobaciones](./governance/project-es/ep-06-approvals-detailed-design.md)
- [Épica 07: Cumplimiento](./governance/project-es/ep-07-compliance-detailed-design.md)
- [Épica 08: IGA](./governance/project-es/ep-08-iga-detailed-design.md)

### Fase 03 -- Arquitectura y Diseño de Base de Datos

- [Diseño de Base de Datos ER](./architecture/blueprints-es/database-design-er.md)
- [Formatos de Exportación ER](./architecture/blueprints-es/er-export-formats.md)
- [Visor ER Interactivo](./architecture/blueprints/interactive-er-viewer.html)
- [Mapa de Entidades de Servicio](./architecture/blueprints/service-entity-map.es.md)
- [Arquitectura de Librerías Shell](./architecture/blueprints-es/shell-library-architecture.md)
- [Arquitectura de Notificaciones y Feedback](./architecture/blueprints-es/notification-feedback-architecture.md)
- [Visión General de Arquitectura](./architecture/overview.es.md)
- [Portal de Arquitectura](./architecture/index.es.md)
- [Matriz de Trazabilidad (FS → ADR → TE)](./architecture/traceability-matrix.es.md)
- [Índice de Habilitadores Técnicos](./architecture/blueprints/technical-enablers/index.es.md)
- [Índice de Patrones Canónicos](./architecture/artifacts/canonical-patterns/index.es.md)

### Fase 04 -- Construcción y Diseño de Dominio

- [Portal de Construcción](./governance/construction/index.es.md)
- [Portal DDD](./governance/construction/ddd-design/index.es.md)
- [Mapa de Bounded Contexts](./governance/construction/ddd-design/01-bounded-context-map.es.md)
- [Lenguaje Ubicuo](./governance/construction/ddd-design/02-ubiquitous-language.es.md)
- [Contexto de Identidad](./governance/construction/ddd-design/03-identity-context.es.md)
- [Contexto de Autorización](./governance/construction/ddd-design/04-authorization-context.es.md)
- [Contexto de Configuración](./governance/construction/ddd-design/05-configuration-context.es.md)
- [Contexto de Auditoría](./governance/construction/ddd-design/06-audit-context.es.md)
- [Contexto de Aprobaciones](./governance/construction/ddd-design/07-approvals-context.es.md)
- [Contexto de IGA](./governance/construction/ddd-design/08-iga-context.es.md)
- [Contexto de Cumplimiento](./governance/construction/ddd-design/09-compliance-context.es.md)
- [Flujos Cross-Contexto](./governance/construction/ddd-design/10-cross-context-flows.es.md)
- [Primitivas DDD](./governance/construction/ddd-design/11-ddd-primitives.es.md)
- [ADR-0050: Estándar de Nombrado y Taxonomía](./architecture/adrs/0050-naming-taxonomy-standard.es.md)
- [ADR-0051: Puerto Inyectable de Event Bus](./architecture/adrs/0051-event-bus-injectable-port.es.md)
- [ADR-0052: Audit Trail Inmutable](./architecture/adrs/0052-immutable-audit-trail-enforcement.es.md)
- [ADR-0053: Observabilidad OpenTelemetry](./architecture/adrs/0053-opentelemetry-observability.es.md)
- [ADR-0054: Aislamiento de Librerías Shell](./architecture/adrs/0054-shell-library-isolation.es.md) _(enmendado 2026-05-24 -- alcance extendido a AOP + Bootstrapper, grafo de dependencias corregido)_
- [ADR-0059: Decisión de Tier API Único](./architecture/adrs/0059-single-api-tier-decision.es.md)
- [ADR-0060: Estrategia de Concerns Cross-Cutting con AOP](./architecture/adrs/0060-aop-cross-cutting-concern-strategy.es.md)
- [ADR-0061: Patrón Execution Context Accessor](./architecture/adrs/0061-execution-context-accessor.es.md) _(candidato Evolith)_
- [ADR-0062: Configuración Serilog Segura de PII](./architecture/adrs/0062-pii-safe-serilog-configuration.es.md) _(candidato Evolith)_
- [ADR-0063: Middleware de Clave de Idempotencia](./architecture/adrs/0063-idempotency-middleware.es.md) _(candidato Evolith)_
- [ADR-0066: Contrato de Errores Accionables](./architecture/adrs/0066-actionable-user-error-contract.es.md) _(candidato Evolith)_
- [ADR-0071: Motor del Grafo de Autorización](./architecture/adrs/0071-auth-graph-engine.es.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](./architecture/adrs/0072-dynamic-auth-method-resolution.es.md)
- [ADR-0073: UMS SDK Multi-Runtime](./architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
- [ADR-0074: Política de Versionado del Schema del Grafo](./architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- [Índice ADR](./architecture/adrs/index.es.md)
- **Guías de Desarrollo de Librerías Shell** -- [Visión General](./architecture/shell-libraries/README.es.md) · [DDD](./architecture/shell-libraries/ddd.es.md) · [Factory](./architecture/shell-libraries/factory.es.md) · [AOP](./architecture/shell-libraries/aop.es.md) · [Bootstrapper](./architecture/shell-libraries/bootstrapper.es.md) · [Uso Combinado](./architecture/shell-libraries/combined-usage.es.md) · [Aspectos del API](./architecture/shell-libraries/api-aspects.es.md)
- [Decisiones de Diseño y Vacíos](./governance/construction/ddd-design/12-design-decisions.es.md)
- [Viewer Interactivo DDD](./governance/construction/ddd-design/interactive-ddd-viewer.html)

### Fase 04b -- Arquitectura de Agregados de Dominio

> La Fase 04b documenta cada Aggregate Root con 8 secciones estructuradas: Visión del Agregado · Modelo de Objeto · Diagramas de Secuencia · Modelo ER · Modelo de Bounded Context · Contrato API · Notas de Persistencia · Seguridad y Auditoría. Las entidades hijas (Branch, Branding, IdentityProvider, etc.) se documentan dentro de la página de su Aggregate Root padre -- no como documentos separados.

- [Índice de Agregados de Dominio](./domain-es/index.md)
- **BC de Identidad:** [Tenant](./domain-es/identity/tenant.md) · [UserAccount](./domain-es/identity/user-account.md) · [Grafo de Autorización](./domain-es/identity/auth-graph.md) · [Resolución del Método de Autenticación](./domain-es/identity/auth-method-resolution.md)
- **BC de Autorización:** [SystemSuite](./domain-es/authorization/system-suite.md) · [PermissionTemplate](./domain-es/authorization/permission-template.md) · [Profile](./domain-es/authorization/profile.md)
- **BC de Configuración:** [IdpConfiguration](./domain-es/configuration/idp-configuration.md) · [AppConfiguration](./domain-es/configuration/app-configuration.md) · [FeatureFlag](./domain-es/configuration/feature-flag.md)
- **BC de Aprobaciones:** [ApprovalWorkflow](./domain-es/approvals/approval-workflow.md) · [ApprovalRequest](./domain-es/approvals/approval-request.md) · [DocumentType](./domain-es/approvals/document-type.md) · [UserDocument](./domain-es/approvals/user-document.md)
- **BC de IGA:** [PromotionRequest](./domain-es/iga/promotion-request.md) · [RoleMaturityStatus](./domain-es/iga/role-maturity-status.md)
- **BC de Auditoría:** [AuditRecord](./domain-es/audit/audit-record.md)

> Las entidades hijas (Branch, Branding, IdentityProvider, MfaEnrollment, PasswordCredential, ProfilePermission, NotificationRule, AccessEnforcementPolicy, etc.) se documentan dentro de la página de su agregado raíz. Inventario completo: [Índice de Agregados de Dominio](./domain-es/index.md).

### Fase 06 -- UMS SDK

> El UMS SDK es la superficie oficial de integración cliente, distribuida en tres runtimes (.NET, TypeScript, NestJS) compartiendo un único contrato canónico — el JSON Schema del `AuthorizationGraph`. El código fuente vive bajo `src/libs/sdk/`. Ver [ADR-0073](./architecture/adrs/0073-ums-sdk-multi-runtime.es.md) y [ADR-0074](./architecture/adrs/0074-auth-graph-schema-versioning.es.md).

- [Portal SDK](./sdk-es/index.md)
- **Contratos:** [Schema Overview](./sdk-es/contracts/schema-overview.md) · [Códigos de Error](./sdk-es/contracts/error-codes.md) · [Política de Versionado](./sdk-es/contracts/versioning.md) · [Fixtures](./sdk-es/contracts/fixtures.md) · [Matriz de Compatibilidad](./sdk-es/contracts/compatibility-matrix.md)
- **.NET:** [README](./sdk-es/dotnet/README.md) · [Quickstart](./sdk-es/dotnet/quickstart.md)
- **TypeScript:** [README](./sdk-es/typescript/README.md) · [Quickstart](./sdk-es/typescript/quickstart.md)
- **NestJS:** [README](./sdk-es/nestjs/README.md) · [Quickstart](./sdk-es/nestjs/quickstart.md)

### Fase 05 -- Operaciones

- [Portal de Operaciones](./operations/index.es.md)
- [RB-01: Respuesta a Incidentes](./operations/runbooks/rb-01-incident-response.es.md)
- [RB-02: Procedimiento de Rollback](./operations/runbooks/rb-02-rollback-procedure.es.md)
- [RB-03: Recuperación de Fallas de Caché](./operations/runbooks/rb-03-cache-failure-recovery.es.md)
- [RB-04: Failover de Base de Datos](./operations/runbooks/rb-04-database-failover.es.md)

### Métricas de Ingeniería

- [Dashboard de Métricas de Solución](./operations/metrics/index.es.md)
  - Métricas API (ums.api)
  - Métricas Web (ums.web-app)
  - Métricas de Librerías (shell/*)
  - Métricas de Suite de Tests
  - Métricas Agregadas por Categoría
