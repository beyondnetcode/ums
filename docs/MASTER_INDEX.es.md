# Índice Maestro -- Hub de Navegación de UMS

> **Idioma:** [English](./MASTER_INDEX.md) | [Español](./MASTER_INDEX.es.md)

Especificaciones de Ingeniería y Ciclo de Vida del Producto para el User Management System (UMS).

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
- [Épica 06: Aprobaciones](./governance/project-es/ep-06-approvals-detailed-design.md)
- [Épica 07: Cumplimiento](./governance/project-es/ep-07-compliance-detailed-design.md)
- [Épica 08: IGA](./governance/project-es/ep-08-iga-detailed-design.md)

### Fase 03 -- Arquitectura y Diseño de Base de Datos

- [Diseño de Base de Datos ER](./architecture/blueprints-es/database-design-er.md)
- [Formatos de Exportación ER](./architecture/blueprints-es/er-export-formats.md)
- [Arquitectura de Librerías Shell](./architecture/blueprints-es/shell-library-architecture.md)
- [Portal de Arquitectura](./architecture/index.es.md)

### Fase 04 -- Construccion y Diseño de Dominio

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
- [ADR-0054: Aislamiento de Librerías Shell](./architecture/adrs/0054-shell-library-isolation.md)
- [Decisiones de Diseno y Vacios](./governance/construction/ddd-design/12-design-decisions.md)
- [Viewer Interactivo DDD](./governance/construction/ddd-design/interactive-ddd-viewer.html)

### Fase 04b -- Arquitectura de Agregados de Dominio

- [Indice de Agregados de Dominio](./domain-es/index.md)
- **Identity BC:** [Tenant](./domain-es/identity/tenant.md) · [Branch](./domain-es/identity/branch.md) · [Branding](./domain-es/identity/branding.md) · [IdentityProvider](./domain-es/identity/identity-provider.md) · [UserAccount](./domain-es/identity/user-account.md) · [PasswordCredential](./domain-es/identity/password-credential.md) · [MfaEnrollment](./domain-es/identity/mfa-enrollment.md)
- **Authorization BC:** [SystemSuite](./domain-es/authorization/system-suite.md) · [Module](./domain-es/authorization/module.md) · [Menu](./domain-es/authorization/menu.md) · [SubMenu](./domain-es/authorization/sub-menu.md) · [Option](./domain-es/authorization/option.md) · [Action](./domain-es/authorization/action.md) · [PermissionTemplate](./domain-es/authorization/permission-template.md) · [PermissionTemplateItem](./domain-es/authorization/permission-template-item.md) · [Profile](./domain-es/authorization/profile.md) · [ProfilePermission](./domain-es/authorization/profile-permission.md)
- **Configuration BC:** [AppConfiguration](./domain-es/configuration/app-configuration.md) · [FeatureFlag](./domain-es/configuration/feature-flag.md) · [FlagEvaluationLog](./domain-es/configuration/flag-evaluation-log.md) · [IdpConfiguration](./domain-es/configuration/idp-configuration.md)
- **Approvals BC:** [ApprovalWorkflow](./domain-es/approvals/approval-workflow.md) · [ApprovalRequiredDocument](./domain-es/approvals/approval-required-document.md) · [ApprovalRequest](./domain-es/approvals/approval-request.md) · [DocumentType](./domain-es/approvals/document-type.md) · [NotificationRule](./domain-es/approvals/notification-rule.md) · [UserDocument](./domain-es/approvals/user-document.md) · [AccessNotification](./domain-es/approvals/access-notification.md) · [AccessEnforcementPolicy](./domain-es/approvals/access-enforcement-policy.md)
- **IGA BC:** [PromotionRequest](./domain-es/iga/promotion-request.md) · [PromotionImpactAnalysis](./domain-es/iga/promotion-impact-analysis.md) · [RoleMaturityStatus](./domain-es/iga/role-maturity-status.md)
- **Audit BC:** [AuditRecord](./domain-es/audit/audit-record.md)
