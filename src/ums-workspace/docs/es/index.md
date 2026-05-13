# ðŸ—ºï¸ Mapa de NavegaciÃ³n Maestro - Base de Conocimiento de UMS

> ðŸŒ **Selector de Idioma:** [ðŸ‡ªðŸ‡¸ EspaÃ±ol](./index.md) | [ðŸ‡ºðŸ‡¸ English](../en/index.md)

Bienvenido a la documentaciÃ³n tÃ©cnica maestra del **Sistema de GestiÃ³n de Usuarios (UMS)**. Esta base de conocimiento estÃ¡ estructurada bajo el **estrategia spec-driven AI BMAD-METHOD (fases secuenciales numÃ©ricas)** para garantizar la mÃ¡xima descubribilidad, trazabilidad y soporte continuo tanto para desarrolladores humanos como para copilotos de IA autÃ³nomos.

---

## ðŸ§­ Ãndice de NavegaciÃ³n Basado en Fases

### ðŸŽ¯ [Fase 00 - VisiÃ³n de Producto](./00-product/)
Define el contexto de negocio, los pilares estratÃ©gicos del producto y el mapa de interesados.
*   ðŸ“„ **[VisiÃ³n del Producto](./00-product/product-vision.md)**: Pilares de identidad soberana, delegaciÃ³n de autenticaciÃ³n y multi-tenancy dinÃ¡mico.
*   ðŸ“„ **[Contexto de Negocio](./00-product/business-context.md)**: DeclaraciÃ³n del problema, soluciÃ³n propuesta y diagramas conceptuales de integraciÃ³n.
*   ðŸ“„ **[Alcance y LÃ­mites](./00-product/scope.md)**: Detalle de capacidades dentro y fuera del alcance.
*   ðŸ“„ **[Objetivos EstratÃ©gicos (OKRs)](./00-product/objectives.md)**: MÃ©tricas cuantificables y KRs para seguridad, latencia y autoservicio.
*   ðŸ“„ **[Mapa de Interesados (Stakeholders)](./00-product/stakeholders.md)**: Matriz de responsabilidades y mapeo de expectativas para roles tÃ©cnicos y de negocio.

---

### ðŸ“‹ [Fase 01 - Requisitos de Dominio](./01-requirements/)
Detalla reglas de negocio, secuencias interactivas, diagramas conceptuales de bases de datos y la definiciÃ³n formal del Lenguaje Ubicuo.
*   ðŸ“„ **[Glosario de TÃ©rminos (Lenguaje Ubicuo)](./01-requirements/glossary.md)**: Diccionario formal DDD del dominio central.
*   ðŸ“„ **[Modelo de Datos Conceptual](./01-requirements/conceptual-data-model.md)**: LÃ³gica relacional de PostgreSQL y polÃ­ticas de Seguridad a Nivel de Fila (RLS).
*   ðŸ“„ **[Matriz de Permisos Granulares](./01-requirements/permission-matrix-example.md)**: LÃ³gica de acceso detallada (RBAC/ABAC) y regla de precedencia de denegaciÃ³n explÃ­cita.
*   ðŸ“‚ **[Casos de Uso AtÃ³micos](./01-requirements/functional-stories/)**:
    *   [UC-01: AutenticaciÃ³n de Usuario vÃ­a IdP Externo](./01-requirements/functional-stories/uc-01-user-authentication.md)
    *   [UC-02: CompilaciÃ³n de Grafo de AutorizaciÃ³n](./01-requirements/functional-stories/uc-02-build-authorization-graph.md)
    *   [UC-03: Crear e Instanciar Plantilla de AutorizaciÃ³n](./01-requirements/functional-stories/uc-03-create-authorization-template.md)
    *   [UC-04: Registrar OrganizaciÃ³n y Configurar Estrategia de IdP](./01-requirements/functional-stories/uc-04-register-organization.md)
    *   [UC-05: Registrar Sistema y Definir TopologÃ­a de MenÃº](./01-requirements/functional-stories/uc-05-register-system-topology.md)
    *   [UC-06: Crear Perfil y Asignar Plantilla Manualmente](./01-requirements/functional-stories/uc-06-create-profile-manual-template.md)
    *   [UC-07: Auto-Asignar Plantilla al Crear Perfil](./01-requirements/functional-stories/uc-07-auto-assign-template.md)
    *   [UC-08: Diagnosticar Permisos vÃ­a Visualizador de Grafos](./01-requirements/functional-stories/uc-08-visual-graph-resolver.md)
    *   [UC-09: Resolver ConfiguraciÃ³n JerÃ¡rquica del Sistema](./01-requirements/functional-stories/uc-09-resolve-hierarchical-config.md)
    *   [UC-10: Autenticar vÃ­a PÃ¡gina de Inicio Personalizable](./01-requirements/functional-stories/uc-10-hosted-login-redirection.md)
    *   [UC-11: AutenticaciÃ³n Adaptativa Multifactor y Sin ContraseÃ±a](./01-requirements/functional-stories/uc-11-mfa-passwordless-adaptive-auth.md)
    *   [UC-12: Flujo de AprobaciÃ³n y PeticiÃ³n de Acceso Externo B2B](./01-requirements/functional-stories/uc-12-external-b2b-access-request-approval.md)

---

### ðŸ—ï¸ [Fase 02 - Arquitectura de Software](./02-architecture/)
Contiene la especificaciÃ³n arquitectÃ³nica del sistema basada en el estÃ¡ndar C4 Model y el stack tecnolÃ³gico autorizado.
*   ðŸ“„ **[Plan de MigraciÃ³n .NET & Stack TecnolÃ³gico](./02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Roadmap para la migraciÃ³n a la lÃ­nea base corporativa .NET 8 LTS.
*   ðŸ“„ **[Mapa de Contextos Delimitados](./02-architecture/bounded-context-map.md)**: LÃ­mites de contexto DDD, patrones de integraciÃ³n y Capas Anti-CorrupciÃ³n (ACL).
*   ðŸ“„ **[EspecificaciÃ³n de Arquitectura C4 e Inventario TÃ©cnico](./02-architecture/architecture-spec.md)**: Diagramas tÃ©cnicos de Nivel 1 (Contexto), Nivel 2 (Contenedor) y Nivel 3 (Componente).
*   ðŸ“„ **[DefiniciÃ³n de Stack TecnolÃ³gico Autorizado](./02-architecture/stack.md)**: Definiciones de stack tÃ©cnico 100% agnÃ³stico a la nube y con capacidad local, y registro de riesgos.
*   ðŸ“„ **[Hoja de Referencia del Stack TecnolÃ³gico](./02-architecture/stack-summary.md)**: Referencia rÃ¡pida de alta densidad de todas las herramientas y capas seleccionadas.
*   ðŸ“„ **[EvaluaciÃ³n de Primitivas DDD](./02-architecture/nestjslatam-ddd-evaluation.md)**: EvaluaciÃ³n tÃ©cnica de capacidades de dominio para el NÃºcleo Central.

---

### ðŸ“œ [Phase 03 - Registros de DecisiÃ³n ArquitectÃ³nica (ADRs)](./03-adrs/)
El libro de contabilidad cronolÃ³gico e inmutable de las decisiones crÃ­ticas de diseÃ±o en formato MADR.
*   ðŸ“„ **[Libro Mayor ADR](./03-adrs/)**: Acceso al Ã­ndice completo de **29 decisiones arquitectÃ³nicas activas** (que abarcan desde Nx Monorepo, Arquitectura Limpia, RLS, hasta AbstracciÃ³n de Proveedor de Identidad, CompilaciÃ³n de Grafos de Alto Rendimiento, Proyecciones de Salida Intercambiables, NÃºcleo de AutorizaciÃ³n Centralizado, Plataforma de ConfiguraciÃ³n, AbstracciÃ³n de Proveedor de Feature Flags, MFA/Sin ContraseÃ±a Adaptativo, APIs de Doble Protocolo, Infraestructura Autohospedada y Primitivas DDD TÃ¡cticas).
*   ðŸ“„ **[ADR-0024: Plataforma de GestiÃ³n de ConfiguraciÃ³n y Features](./03-adrs/0024-configuration-feature-management-platform.md)**: Establece config Multi-IdP, ConfiguraciÃ³n de Comportamiento del Sistema y framework de Feature Flags.
*   ðŸ“„ **[ADR-0025: AbstracciÃ³n de Proveedor de Feature Flags](./03-adrs/0025-feature-flag-provider-abstraction.md)**: Define el patrÃ³n enchufable `IFeatureFlagPort`: compatible con motor interno, LaunchDarkly, Unleash, ConfigCat, Azure App Config.
*   ðŸ“„ **[ADR-0026: MFA Adaptativo Multi-Tenancy y AutenticaciÃ³n Sin ContraseÃ±a](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md)**: Establece MFA adaptativo dinÃ¡mico, WebAuthn/Passkeys, dispositivos criptogrÃ¡ficos de confianza y recuperaciÃ³n de autoservicio.
*   ðŸ“„ **[ADR-0027: Estructura de API REST y gRPC de Doble Protocolo con Kong Gateway](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md)**: Establece interfaces duales REST para acceso pÃºblico y gRPC de alto rendimiento para consumo interno.
*   ðŸ“„ **[ADR-0028: Infraestructura de CÃ³digo Abierto Autohospedada para Despliegues HÃ­bridos y Locales](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md)**: Garantiza capacidad agnÃ³stica a la nube y despliegues localizados sin bloqueo de nube vÃ­a MinIO, RabbitMQ y HashiCorp Vault.
*   ðŸ“„ **[ADR-0029: Biblioteca de Primitivas DDD TÃ¡cticas](./03-adrs/0029-tactical-ddd-primitives-library.md)**: Estandariza el uso de bibliotecas autorizadas para soporte tÃ¡ctico de diseÃ±o conducido por dominio.

#### ðŸ›¡ï¸ GestiÃ³n de Riesgos Operativos
*   ðŸ“„ **[EvaluaciÃ³n de Riesgo Financiero y Bloqueo de Proveedor](./02-architecture/vendor-risk-assessment.md)**: DocumentaciÃ³n de base que analiza Proveedores de Identidad, licenciamiento de Redis, plataformas de Feature Flags y cachÃ© de Nx Cloud para prevenir cargos financieros inesperados.

#### ðŸ›ï¸ Gobernanza ArquitectÃ³nica y Matriz de Estado ADR

Antes de comenzar la fase de codificaciÃ³n, el Product Owner (PO) tiene autoridad absoluta para aprobar, diferir o vetar cualquier Registro de DecisiÃ³n ArquitectÃ³nica (ADR). A continuaciÃ³n se presenta la clasificaciÃ³n exhaustiva de las 29 decisiones activas que coinciden con sus estados de archivo:

### ðŸŸ¢ 1. APROBADO Y ACEPTADO (LÃ­nea Base Autorizada â€” Listo para CodificaciÃ³n)
Estas decisiones estÃ¡n oficialmente **Aprobadas** y forman la arquitectura base del sistema. El desarrollo debe adherirse estrictamente a estos patrones:

| ID ADR | TÃ­tulo de la DecisiÃ³n | Estado | Impacto / Alcance | Siguientes Pasos / AcciÃ³n |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [OrquestaciÃ³n de Monorepo con Nx](./03-adrs/0001-monorepo-orchestration-nx.md) | ðŸŸ¢ **Aceptado** | OrganizaciÃ³n central del monorepo y optimizaciÃ³n de velocidad. | LÃ­nea base aprobada. |
| **ADR-0002** | [Arquitectura Limpia y LÃ­mites Hexagonales](./03-adrs/0002-clean-architecture-nestjs.md) | ðŸŸ¢ **Aceptado** | Desacoplamiento de reglas de negocio del dominio central de la base de datos y frameworks. | LÃ­nea base aprobada. |
| **ADR-0003** | [EstÃ¡ndares Estrictos de TypeScript](./03-adrs/0003-strict-typescript-standards.md) | ðŸŸ¢ **Aceptado** | Puerta de calidad de anÃ¡lisis estÃ¡tico y obligatoriedad de tipado. | LÃ­nea base aprobada. |
| **ADR-0004** | [Arquitectura Offline de React Query](./03-adrs/0004-frontend-offline-resilience.md) | ðŸŸ¢ **Aceptado** | CachÃ© local y mecanismo de contingencia para resiliencia de clientes. | LÃ­nea base aprobada. |
| **ADR-0005** | [Seguridad de Costo Cero vÃ­a CodeQL](./03-adrs/0005-ci-cd-quality-codeql.md) | ðŸŸ¢ **Aceptado** | Escaneo automatizado de vulnerabilidades en el pipeline de CI. | LÃ­nea base aprobada. |
| **ADR-0007** | [Estrategia de Loki y OpenTelemetry](./03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | ðŸŸ¢ **Aceptado** | Rastreo distribuido y recolecciÃ³n de logs centralizada. | LÃ­nea base aprobada. |
| **ADR-0008** | [Patrones de Gateway y BFF](./03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | ðŸŸ¢ **Aceptado** | Optimiza solicitudes de red para clientes multimÃ³dulo. | LÃ­nea base aprobada. |
| **ADR-0009** | [FijaciÃ³n Estricta de Dependencias](./03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | ðŸŸ¢ **Aceptado** | Mitiga vulnerabilidades de inyecciÃ³n en la cadena de suministro. | LÃ­nea base aprobada. |
| **ADR-0010** | [Estrategia SaaS Multi-Tenancy](./03-adrs/0010-multi-tenancy-architecture-strategy.md) | ðŸŸ¢ **Aceptado** | Define la estrategia de aislamiento de base de datos por cliente corporativo. | LÃ­nea base aprobada. |
| **ADR-0011** | [Tolerancia a Fallos y Resiliencia](./03-adrs/0011-fault-tolerance-resiliency-patterns.md) | ðŸŸ¢ **Aceptado** | Cortacircuitos (`opossum`) y reintentos exponenciales. | LÃ­nea base aprobada. |
| **ADR-0012** | [AutorizaciÃ³n Avanzada (RBAC/ABAC)](./03-adrs/0012-advanced-authorization-rbac-abac.md) | ðŸŸ¢ **Aceptado** | Modelado de permisos contextuales de grano fino. | LÃ­nea base aprobada. |
| **ADR-0014** | [CachÃ© Distribuida (Redis)](./03-adrs/0014-distributed-caching-strategy-redis.md) | ðŸŸ¢ **Aceptado** | CachÃ©s en memoria para validaciones de autenticaciÃ³n y sesiones activas. | LÃ­nea base aprobada. |
| **ADR-0015** | [Arquitectura Dirigida por Eventos](./03-adrs/0015-event-driven-architecture-intra-domain.md) | ðŸŸ¢ **Aceptado** | PublicaciÃ³n de eventos asÃ­ncronos para sincronizaciÃ³n de estado. | LÃ­nea base aprobada. |
| **ADR-0016** | [Rastro de AuditorÃ­a de Negocio Inmutable](./03-adrs/0016-immutable-business-audit-trail.md) | ðŸŸ¢ **Aceptado** | Estrategia de auditorÃ­a a nivel de aplicaciÃ³n usando Eventos de Dominio. | LÃ­nea base aprobada. |
| **ADR-0017** | [Estrategia de Feature Flagging](./03-adrs/0017-feature-flagging-strategy.md) | ðŸŸ¢ **Aceptado** | Feature Flags inyectadas por infraestructura (Unleash/LaunchDarkly). | LÃ­nea base aprobada. |
| **ADR-0018** | [Puertas de Calidad de la PirÃ¡mide de Pruebas](./03-adrs/0018-testing-pyramid-quality-gates.md) | ðŸŸ¢ **Aceptado** | LÃ­mites de cobertura para pruebas E2E, de contrato y unitarias (>70%). | LÃ­nea base aprobada. |
| **ADR-0019** | [Patrones de Dominio TÃ¡ctico](./03-adrs/0019-tactical-design-patterns-future-proofing.md) | ðŸŸ¢ **Aceptado** | PatrÃ³n Result, Null Objects y Decoradores. | LÃ­nea base aprobada. |
| **ADR-0020** | [AbstracciÃ³n de Proveedor de Identidad](./03-adrs/0020-identity-provider-abstraction-strategy.md) | ðŸŸ¢ **Aceptado** | Desacopla UMS de Auth0, Keycloak o Entra ID. | LÃ­nea base aprobada. |
| **ADR-0021** | [Grafo de AutorizaciÃ³n de Alto Rendimiento](./03-adrs/0021-high-performance-auth-and-graph-compilation.md) | ðŸŸ¢ **Aceptado** | CompilaciÃ³n de permisos optimizada bajo un lÃ­mite de latencia <5ms. | LÃ­nea base aprobada. |
| **ADR-0022** | [Proyecciones de Salida Enchufables](./03-adrs/0022-contextual-auth-and-pluggable-projections.md) | ðŸŸ¢ **Aceptado** | Capas de proyecciÃ³n de lectura conscientes del contexto fuera del nÃºcleo. | LÃ­nea base aprobada. |
| **ADR-0023** | [Acceso Centralizado vs Descentralizado](./03-adrs/0023-centralized-ums-vs-decentralized-access.md) | ðŸŸ¢ **Aceptado** | Establece el lÃ­mite del nÃºcleo de autorizaciÃ³n autoritativo. | LÃ­nea base aprobada. |
| **ADR-0024** | [ConfiguraciÃ³n y Feature Management](./03-adrs/0024-configuration-feature-management-platform.md) | ðŸŸ¢ **Aceptado** | Motor dinÃ¡mico de parÃ¡metros Multi-IdP. | LÃ­nea base aprobada. |
| **ADR-0025** | [AbstracciÃ³n de Proveedor de Feature Flag](./03-adrs/0025-feature-flag-provider-abstraction.md) | ðŸŸ¢ **Aceptado** | `IFeatureFlagPort` enchufable para Unleash/ConfigCat. | LÃ­nea base aprobada. |
| **ADR-0026** | [MFA y AutenticaciÃ³n Sin ContraseÃ±a](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md) | ðŸŸ¢ **Aceptado** | WebAuthn, Passkeys, TOTP y MFA adaptativo basado en riesgo. | LÃ­nea base aprobada. |
| **ADR-0027** | [Estructura de API REST & gRPC Doble Protocolo](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md) | ðŸŸ¢ **Aceptado** | APIs RESTful pÃºblicas y servicios gRPC internos. | LÃ­nea base aprobada. |
| **ADR-0028** | [Infraestructura HÃ­brida Autohospedada](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md) | ðŸŸ¢ **Aceptado** | Capacidad agnÃ³stica de nube (MinIO, RabbitMQ, Vault OSS). | LÃ­nea base aprobada. |
| **ADR-0029** | [Biblioteca de Primitivas DDD TÃ¡cticas](./03-adrs/0029-tactical-ddd-primitives-library.md) | ðŸŸ¢ **Aceptado** | Estandariza librerÃ­as autorizadas para dominio modular tÃ¡ctico. | LÃ­nea base aprobada. |

### ðŸŸ¡ 2. PROPUESTO Y PENDIENTE DE REVISIÃ“N (Pendientes de RevisiÃ³n/AprobaciÃ³n por el PO)
Estas decisiones se encuentran actualmente **Propuestas** y representan backlogs estratÃ©gicos. **Deben ser aprobadas formalmente por el PO antes de comenzar la codificaciÃ³n**:

| ID ADR | TÃ­tulo de la DecisiÃ³n | Estado | Impacto / Alcance | AcciÃ³n del PO Requerida |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0006** | [Futuros Microservicios vÃ­a Dapr](./03-adrs/0006-future-microservices-transition-dapr.md) | ðŸŸ¡ **Propuesto** | IntegraciÃ³n de sidecar para estado distribuido y mensajerÃ­a. | **PO revisar/aprobar** para activar migraciÃ³n a microservicios. |
| **ADR-0013** | [Infraestructura en la Nube y DR](./03-adrs/0013-cloud-infrastructure-topology-dr.md) | ðŸŸ¡ **Propuesto** | LÃ­mites de replicaciÃ³n de recuperaciÃ³n ante desastres multirregiÃ³n. | **PO revisar/aprobar** para autorizar presupuesto de despliegue. |

### ðŸ”´ 3. CANCELADO, RECHAZADO O VETADO (Rechazados o Descartados por el PO)
Estas decisiones arquitectÃ³nicas han sido **Vetadas / Rechazadas** o **Canceladas** por el Product Owner y **nunca deben ser implementadas**:

*   *Actualmente, no hay decisiones rechazadas ni canceladas. Usted tiene plena autoridad para mover cualquier ADR a esta secciÃ³n para vetar su implementaciÃ³n.*

---

### ðŸ› ï¸ [Fase 04 - EstÃ¡ndares y Artefactos de IngenierÃ­a](./04-artifacts/)
Pautas tÃ©cnicas, reglas de cÃ³digo limpio, estÃ¡ndares de seguridad y planes de calidad tÃ©cnica.
*   ðŸ“„ **[EstÃ¡ndares Globales de IngenierÃ­a](./04-artifacts/engineering-standards.md)**: Cumplimiento de SOLID, Arquitectura Limpia, OWASP y pautas DDD.
*   ðŸ“„ **[Modelo de Madurez ArquitectÃ³nica (AMM)](./04-artifacts/architecture-maturity-model.md)**: EvaluaciÃ³n de madurez de TOGAF ACMM y Well-Architected Framework.
*   ðŸ“„ **[Plan de Pruebas de Contrato](./04-artifacts/contract-testing-plan.md)**: IntegraciÃ³n segura de microservicios usando Pact.
*   ðŸ“„ **[Estrategia de Observabilidad Distribuida](./04-artifacts/observability-strategy.md)**: TelemetrÃ­a unificada usando OpenTelemetry y Grafana Loki.
*   ðŸ“„ **[AnÃ¡lisis de Brecha y Deuda TÃ©cnica](./04-artifacts/gap-analysis-and-optimization-plan.md)**: EvaluaciÃ³n de madurez de arquitectura y plan de mitigaciÃ³n tÃ©cnica.
*   ðŸ“„ **[EspecificaciÃ³n IAM Empresarial](./04-artifacts/enterprise-iam-ums-specification.md)**: Contratos y especificaciones del grafo de autorizaciÃ³n dinÃ¡mica.
*   ðŸ“„ **[EspecificaciÃ³n de AutenticaciÃ³n de Alta Concurrencia](./04-artifacts/high-concurrency-auth-specification.md)**: Esquemas de almacenamiento en cachÃ© de rendimiento y rotaciÃ³n de tokens.
*   ðŸ“„ **[Espec de Producto de la Consola Web UMS](./04-artifacts/ums-web-console-product-scope.md)**: Panel de control administrativo PAP y monitores SRE.
*   ðŸ“„ **[Espec de Plataforma de GestiÃ³n de ConfiguraciÃ³n y Features](./04-artifacts/ums-configuration-platform-spec.md)**: Motor de configuraciÃ³n multi-IdP y marco de feature flags.
*   ðŸ“„ **[Espec de AutenticaciÃ³n MFA y Sin ContraseÃ±a](./04-artifacts/mfa-passwordless-security-spec.md)**: EspecificaciÃ³n de autenticaciÃ³n adaptativa basada en riesgos y multifactor.
*   ðŸ“„ **[Espec Maestra de AuditorÃ­a Empresarial y bMAD](./04-artifacts/bmad-master-audit-alignment-report.md)**: EspecificaciÃ³n exhaustiva de alineaciÃ³n business-models-architecture-delivery.

---

### ðŸ“ˆ [Fase 05 - Hoja de Ruta de Lanzamientos](./05-roadmap/)
Estrategias de liberaciÃ³n de cÃ³digo, despliegue continuo y automatizaciÃ³n de despliegue.
*   ðŸ“„ **[Estrategia de Versionado y Lanzamiento](./05-roadmap/versioning-and-audit-strategy.md)**: GestiÃ³n de tags y publicaciones utilizando Nx Release.
