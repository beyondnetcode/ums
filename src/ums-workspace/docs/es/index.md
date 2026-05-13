# Mapa de Navegacion Maestro -- Base de Conocimiento UMS

> **Idioma:** [Espanol](./index.md) | [English](../en/index.md)

Documentacion estructurada del Sistema de Gestion de Usuarios (UMS) siguiendo las fases BMAD-METHOD.

---

### Fase 00 -- Vision de Producto

- [Vision del Producto](./00-product/product-vision.md)
- [Contexto de Negocio](./00-product/business-context.md)
- [Alcance y Limites](./00-product/scope.md)
- [Objetivos Estrategicos (OKRs)](./00-product/objectives.md)
- [Mapa de Interesados](./00-product/stakeholders.md)

### Fase 01 -- Requisitos de Dominio

- [Glosario (Lenguaje Ubicuo)](./01-requirements/glossary.md)
- [Modelo de Datos Conceptual](./01-requirements/conceptual-data-model.md)
- [Matriz de Permisos Granulares](./01-requirements/permission-matrix-example.md)
- Historias Funcionales:
  - [FS-01: Autenticacion via IdP Externo](./01-requirements/functional-stories/fs-01-user-authentication.md)
  - [FS-02: Crear Plantilla de Autorizacion](./01-requirements/functional-stories/fs-02-create-authorization-template.md)
  - [FS-03: Registrar Organizacion y Configurar IdP](./01-requirements/functional-stories/fs-03-register-organization.md)
  - [FS-04: Registrar Sistema y Topologia de Menu](./01-requirements/functional-stories/fs-04-register-system-topology.md)
  - [FS-05: Crear Perfil y Asignar Plantilla](./01-requirements/functional-stories/fs-05-create-profile-manual-template.md)
  - [FS-06: Auto-Asignar Plantilla](./01-requirements/functional-stories/fs-06-auto-assign-template.md)
  - [FS-07: Visualizador de Grafos](./01-requirements/functional-stories/fs-07-visual-graph-resolver.md)
  - [FS-08: Pagina de Inicio Personalizable](./01-requirements/functional-stories/fs-08-hosted-login-redirection.md)
  - [FS-09: Autenticacion Adaptativa MFA](./01-requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md)
  - [FS-10: Acceso Externo B2B](./01-requirements/functional-stories/fs-10-external-b2b-access-request-approval.md)
- Habilitadores Tecnicos:
  - [TE-01: Compilar Grafo de Autorizacion](./02-architecture/technical-enablers/te-01-build-authorization-graph.md)
  - [TE-02: Resolver Configuracion Jerarquica](./02-architecture/technical-enablers/te-02-resolve-hierarchical-config.md)
  - [TE-03: Aplicar RLS por Organizacion](./02-architecture/technical-enablers/te-03-enforce-organization-rls-postgresql.md)

### Fase 02 -- Arquitectura de Software

- [Plan de Migracion .NET y Stack Tecnologico](./02-architecture/dotnet-migration-and-tech-stack-plan.md)
- [Mapa de Contextos Delimitados](./02-architecture/bounded-context-map.md)
- [Especificacion de Arquitectura C4](./02-architecture/architecture-spec.md)
- [Stack Tecnologico Autorizado](./02-architecture/stack.md)
- [Hoja de Referencia del Stack](./02-architecture/stack-summary.md)
- [Evaluacion de Riesgo de Bloqueo](./02-architecture/vendor-risk-assessment.md)

### Fase 03 -- Registros de Decision Arquitectonica (ADRs)

- [Registro Completo de ADRs](./03-adrs/) -- 29 decisiones activas
- ADRs Clave:
  - [ADR-0010: Estrategia SaaS Multi-Tenancy](./03-adrs/0010-multi-tenancy-architecture-strategy.md)
  - [ADR-0020: Abstraccion de Proveedor de Identidad](./03-adrs/0020-identity-provider-abstraction-strategy.md)
  - [ADR-0024: Gestion de Configuracion y Features](./03-adrs/0024-configuration-feature-management-platform.md)
  - [ADR-0034: Multi-Inquilino Jerarquico](./03-adrs/0034-hierarchical-multi-tenancy-domain-model.md)

### Fase 04 -- Estandares de Ingenieria y Artefactos

- [Estandares Globales de Ingenieria](./04-artifacts/engineering-standards.md)
- [Modelo de Madurez Arquitectonica](./04-artifacts/architecture-maturity-model.md)
- [Plan de Pruebas de Contrato](./04-artifacts/contract-testing-plan.md)
- [Estrategia de Observabilidad](./04-artifacts/observability-strategy.md)
- [Especificacion IAM Empresarial](./04-artifacts/enterprise-iam-ums-specification.md)
- [Especificacion de Alta Concurrencia](./04-artifacts/high-concurrency-auth-specification.md)
- [Especificacion de Plataforma de Configuracion](./04-artifacts/ums-configuration-platform-spec.md)
- [Especificacion MFA y Sin Contrasena](./04-artifacts/mfa-passwordless-security-spec.md)
- [Especificacion de Consola Web UMS](./04-artifacts/ums-web-console-product-scope.md)
- [Analisis de Brecha y Deuda Tecnica](./04-artifacts/gap-analysis-and-optimization-plan.md)
- [Reporte de Gobernanza Multi-Tenant](./04-artifacts/enterprise-multitenant-governance-report.md)
- [Informe de Auditoria Maestra bMAD](./04-artifacts/bmad-master-audit-alignment-report.md)

### Fase 05 -- Hoja de Ruta de Lanzamientos

- [Estrategia de Versionado y Lanzamiento](./05-roadmap/versioning-and-audit-strategy.md)
