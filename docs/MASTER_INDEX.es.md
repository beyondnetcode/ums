# Indice Maestro -- Hub de Navegacion de UMS

> **Idioma:** [English](./MASTER_INDEX.md) | [Espanol](./MASTER_INDEX.es.md)

Especificaciones de Ingenieria y Ciclo de Vida del Producto para el User Management System (UMS).

---

### Fase 00 -- Vision del Producto

- [Vision del Producto](./governance/product-es/product-vision.md)
- [Contexto de Negocio](./governance/product-es/business-context.md)
- [Alcance y Limites](./governance/product-es/scope.md)
- [Objetivos (OKRs)](./governance/product-es/objectives.md)
- [Stakeholders](./governance/product-es/stakeholders.md)

### Fase 01 -- Requerimientos de Dominio

- [Glosario (Lenguaje Ubicuo)](./governance/requirements-es/glossary.md)
- [Modelo de Datos Conceptual](./governance/requirements-es/conceptual-data-model.md)
- [Matriz de Permisos](./governance/requirements-es/permission-matrix-example.md)
- [Historias Funcionales](./governance/requirements-es/functional-stories/index.md)

### Fase 02 -- Planificacion MVP y Backlog del Proyecto

- [Backlog del Proyecto](./governance/project-es/index.md)
- [Backlog del Producto MVP](./governance/project-es/mvp-product-backlog.md)
- [Epica 06: Aprobaciones](./governance/project-es/ep-06-approvals-detailed-design.md)
- [Epica 07: Cumplimiento](./governance/project-es/ep-07-compliance-detailed-design.md)
- [Epica 08: IGA](./governance/project-es/ep-08-iga-detailed-design.md)

### Fase 03 -- Arquitectura y Diseno de Base de Datos

- [Diseno de Base de Datos ER](./architecture/blueprints-es/database-design-er.md)
- [Formatos de Exportacion ER](./architecture/blueprints-es/er-export-formats.md)
- [Visor ER Interactivo](./architecture/blueprints/interactive-er-viewer.html)
- [Mapa de Entidades de Servicio](./architecture/blueprints/service-entity-map.md)
- [Arquitectura de Librerias Shell](./architecture/blueprints-es/shell-library-architecture.md)
- [Arquitectura de Notificaciones y Feedback](./architecture/blueprints-es/notification-feedback-architecture.md)
- [Vision General de Arquitectura](./architecture/overview.es.md)
- [Portal de Arquitectura](./architecture/index.es.md)
- [Matriz de Trazabilidad (FS → ADR → TE)](./architecture/traceability-matrix.md)
- [Indice de Habilitadores Tecnicos](./architecture/blueprints/technical-enablers/index.md)
- [Indice de Patrones Canonicos](./architecture/artifacts/canonical-patterns/index.md)

### Fase 04 -- Construccion y Diseno de Dominio

- [Portal de Construccion](./governance/construction/index.es.md)
- [Portal DDD](./governance/construction/ddd-design/index.md)
- [Mapa de Bounded Contexts](./governance/construction/ddd-design/01-bounded-context-map.md)
- [Lenguaje Ubicuo](./governance/construction/ddd-design/02-ubiquitous-language.md)
- [Contexto Identity](./governance/construction/ddd-design/03-identity-context.md)
- [Contexto Authorization](./governance/construction/ddd-design/04-authorization-context.md)
- [Contexto Configuration](./governance/construction/ddd-design/05-configuration-context.md)
- [Contexto Audit](./governance/construction/ddd-design/06-audit-context.md)
- [Contexto Approvals](./governance/construction/ddd-design/07-approvals-context.md)
- [Contexto IGA](./governance/construction/ddd-design/08-iga-context.md)
- [Contexto Compliance](./governance/construction/ddd-design/09-compliance-context.md)
- [Flujos Cross-Contexto](./governance/construction/ddd-design/10-cross-context-flows.md)
- [Primitivas DDD](./governance/construction/ddd-design/11-ddd-primitives.md)
- [ADR-0050: Estandar de Nombrado y Taxonomia](./architecture/adrs/0050-naming-taxonomy-standard.md)
- [ADR-0051: Puerto Inyectable de Event Bus](./architecture/adrs/0051-event-bus-injectable-port.md)
- [ADR-0052: Audit Trail Inmutable](./architecture/adrs/0052-immutable-audit-trail-enforcement.md)
- [ADR-0053: Observabilidad OpenTelemetry](./architecture/adrs/0053-opentelemetry-observability.md)
- [ADR-0054: Aislamiento de Librerias Shell](./architecture/adrs/0054-shell-library-isolation.md) _(enmendado 2026-05-24 -- alcance extendido a AOP + Bootstrapper, grafo de dependencias corregido)_
- [ADR-0059: Decision de Tier API Unico](./architecture/adrs/0059-single-api-tier-decision.md)
- [ADR-0060: Estrategia de Concerns Cross-Cutting con AOP](./architecture/adrs/0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Patron Execution Context Accessor](./architecture/adrs/0061-execution-context-accessor.md) _(candidato Evolith)_
- [ADR-0062: Configuracion Serilog Segura de PII](./architecture/adrs/0062-pii-safe-serilog-configuration.md) _(candidato Evolith)_
- [ADR-0063: Middleware de Clave de Idempotencia](./architecture/adrs/0063-idempotency-middleware.md) _(candidato Evolith)_
- [ADR-0066: Contrato de Errores Accionables](./architecture/adrs/0066-actionable-user-error-contract.es.md) _(candidato Evolith)_
- [Indice ADR](./architecture/adrs/index.md)
- **Guias de Desarrollo de Librerias Shell** -- [Vision General](./architecture/shell-libraries/README.es.md) · [DDD](./architecture/shell-libraries/ddd.es.md) · [Factory](./architecture/shell-libraries/factory.md) · [AOP](./architecture/shell-libraries/aop.es.md) · [Bootstrapper](./architecture/shell-libraries/bootstrapper.md) · [Uso Combinado](./architecture/shell-libraries/combined-usage.md) · [Aspectos del API](./architecture/shell-libraries/api-aspects.es.md)
- [Decisiones de Diseno y Vacios](./governance/construction/ddd-design/12-design-decisions.md)
- [Viewer Interactivo DDD](./governance/construction/ddd-design/interactive-ddd-viewer.html)

### Fase 04b -- Arquitectura de Agregados de Dominio

> La Fase 04b documenta cada Aggregate Root con 8 secciones estructuradas: Vision del Agregado · Modelo de Objeto · Diagramas de Secuencia · Modelo ER · Modelo de Bounded Context · Contrato API · Notas de Persistencia · Seguridad y Auditoria. Las entidades hijas (Branch, Branding, IdentityProvider, etc.) se documentan dentro de la pagina de su Aggregate Root padre -- no como documentos separados.

- [Indice de Agregados de Dominio](./domain-es/index.md)
- **Identity BC:** [Tenant](./domain-es/identity/tenant.md) · [UserAccount](./domain-es/identity/user-account.md)
- **Authorization BC:** [SystemSuite](./domain-es/authorization/system-suite.md) · [PermissionTemplate](./domain-es/authorization/permission-template.md) · [Profile](./domain-es/authorization/profile.md)
- **Configuration BC:** [IdpConfiguration](./domain-es/configuration/idp-configuration.md) · [AppConfiguration](./domain-es/configuration/app-configuration.md) · [FeatureFlag](./domain-es/configuration/feature-flag.md)
- **Approvals BC:** [ApprovalWorkflow](./domain-es/approvals/approval-workflow.md) · [ApprovalRequest](./domain-es/approvals/approval-request.md) · [DocumentType](./domain-es/approvals/document-type.md) · [UserDocument](./domain-es/approvals/user-document.md)
- **IGA BC:** [PromotionRequest](./domain-es/iga/promotion-request.md) · [RoleMaturityStatus](./domain-es/iga/role-maturity-status.md)
- **Audit BC:** [AuditRecord](./domain-es/audit/audit-record.md)

> Las entidades hijas (Branch, Branding, IdentityProvider, MfaEnrollment, PasswordCredential, ProfilePermission, NotificationRule, AccessEnforcementPolicy, etc.) se documentan dentro de la pagina de su agregado raiz. Inventario completo: [Indice de Agregados de Dominio](./domain-es/index.md).

### Fase 05 -- Operaciones

- [Portal de Operaciones](./operations/index.md)
- [RB-01: Respuesta a Incidentes](./operations/runbooks/rb-01-incident-response.md)
- [RB-02: Procedimiento de Rollback](./operations/runbooks/rb-02-rollback-procedure.md)
- [RB-03: Recuperacion de Fallas de Cache](./operations/runbooks/rb-03-cache-failure-recovery.md)
- [RB-04: Failover de Base de Datos](./operations/runbooks/rb-04-database-failover.md)

### Metricas de Ingenieria

- [Dashboard de Metricas de Solucion](./operations/metrics/index.md)
  - Metricas API (ums.api)
  - Metricas Web (ums.web-app)
  - Metricas de Librerias (shell/*)
  - Metricas de Suite de Tests
  - Metricas Agregadas por Categoria