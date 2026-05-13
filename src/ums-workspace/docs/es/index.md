# Ã°Å¸â€”ÂºÃ¯Â¸Â Mapa de NavegaciÃƒÂ³n Maestro - Base de Conocimiento de UMS

> Ã°Å¸Å’Â **Selector de Idioma:** [Ã°Å¸â€¡ÂªÃ°Å¸â€¡Â¸ EspaÃƒÂ±ol](./index.md) | [Ã°Å¸â€¡ÂºÃ°Å¸â€¡Â¸ English](../en/index.md)

Bienvenido a la documentaciÃƒÂ³n tÃƒÂ©cnica maestra del **Sistema de GestiÃƒÂ³n de Usuarios (UMS)**. Esta base de conocimiento estÃƒÂ¡ estructurada bajo el **estrategia spec-driven AI BMAD-METHOD (fases secuenciales numÃƒÂ©ricas)** para garantizar la mÃƒÂ¡xima descubribilidad, trazabilidad y soporte continuo tanto para desarrolladores humanos como para copilotos de IA autÃƒÂ³nomos.

---

## Ã°Å¸Â§Â­ ÃƒÂndice de NavegaciÃƒÂ³n Basado en Fases

### Ã°Å¸Å½Â¯ [Fase 00 - VisiÃƒÂ³n de Producto](./00-product/)
Define el contexto de negocio, los pilares estratÃƒÂ©gicos del producto y el mapa de interesados.
*   Ã°Å¸â€œâ€ž **[VisiÃƒÂ³n del Producto](./00-product/product-vision.md)**: Pilares de identidad soberana, delegaciÃƒÂ³n de autenticaciÃƒÂ³n y multi-tenancy dinÃƒÂ¡mico.
*   Ã°Å¸â€œâ€ž **[Contexto de Negocio](./00-product/business-context.md)**: DeclaraciÃƒÂ³n del problema, soluciÃƒÂ³n propuesta y diagramas conceptuales de integraciÃƒÂ³n.
*   Ã°Å¸â€œâ€ž **[Alcance y LÃƒÂ­mites](./00-product/scope.md)**: Detalle de capacidades dentro y fuera del alcance.
*   Ã°Å¸â€œâ€ž **[Objetivos EstratÃƒÂ©gicos (OKRs)](./00-product/objectives.md)**: MÃƒÂ©tricas cuantificables y KRs para seguridad, latencia y autoservicio.
*   Ã°Å¸â€œâ€ž **[Mapa de Interesados (Stakeholders)](./00-product/stakeholders.md)**: Matriz de responsabilidades y mapeo de expectativas para roles tÃƒÂ©cnicos y de negocio.

---

### Ã°Å¸â€œâ€¹ [Fase 01 - Requisitos de Dominio](./01-requirements/)
Detalla reglas de negocio, secuencias interactivas, diagramas conceptuales de bases de datos y la definiciÃƒÂ³n formal del Lenguaje Ubicuo.
*   Ã°Å¸â€œâ€ž **[Glosario de TÃƒÂ©rminos (Lenguaje Ubicuo)](./01-requirements/glossary.md)**: Diccionario formal DDD del dominio central.
*   Ã°Å¸â€œâ€ž **[Modelo de Datos Conceptual](./01-requirements/conceptual-data-model.md)**: LÃƒÂ³gica relacional de PostgreSQL y polÃƒÂ­ticas de Seguridad a Nivel de Fila (RLS).
*   Ã°Å¸â€œâ€ž **[Matriz de Permisos Granulares](./01-requirements/permission-matrix-example.md)**: LÃƒÂ³gica de acceso detallada (RBAC/ABAC) y regla de precedencia de denegaciÃƒÂ³n explÃƒÂ­cita.
*   Ã°Å¸â€œâ€š **[Casos de Uso AtÃƒÂ³micos](./01-requirements/functional-stories/)**:
    *   [FS-01: AutenticaciÃƒÂ³n de Usuario vÃƒÂ­a IdP Externo](./01-requirements/functional-stories/fs-01-user-authentication.md)
    *   [TE-01: CompilaciÃƒÂ³n de Grafo de AutorizaciÃƒÂ³n](./01-requirements/../02-architecture/technical-enablers/te-01-build-authorization-graph.md)
    *   [FS-02: Crear e Instanciar Plantilla de AutorizaciÃƒÂ³n](./01-requirements/functional-stories/fs-02-create-authorization-template.md)
    *   [FS-03: Registrar OrganizaciÃƒÂ³n y Configurar Estrategia de IdP](./01-requirements/functional-stories/fs-03-register-organization.md)
    *   [FS-04: Registrar Sistema y Definir TopologÃƒÂ­a de MenÃƒÂº](./01-requirements/functional-stories/fs-04-register-system-topology.md)
    *   [FS-05: Crear Perfil y Asignar Plantilla Manualmente](./01-requirements/functional-stories/fs-05-create-profile-manual-template.md)
    *   [FS-06: Auto-Asignar Plantilla al Crear Perfil](./01-requirements/functional-stories/fs-06-auto-assign-template.md)
    *   [FS-07: Diagnosticar Permisos vÃƒÂ­a Visualizador de Grafos](./01-requirements/functional-stories/fs-07-visual-graph-resolver.md)
    *   [TE-02: Resolver ConfiguraciÃƒÂ³n JerÃƒÂ¡rquica del Sistema](./01-requirements/../02-architecture/technical-enablers/te-02-resolve-hierarchical-config.md)
    *   [FS-08: Autenticar vÃƒÂ­a PÃƒÂ¡gina de Inicio Personalizable](./01-requirements/functional-stories/fs-08-hosted-login-redirection.md)
    *   [FS-09: AutenticaciÃƒÂ³n Adaptativa Multifactor y Sin ContraseÃƒÂ±a](./01-requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md)
    *   [FS-10: Flujo de AprobaciÃƒÂ³n y PeticiÃƒÂ³n de Acceso Externo B2B](./01-requirements/functional-stories/fs-10-external-b2b-access-request-approval.md)

---

### Ã°Å¸Ââ€”Ã¯Â¸Â [Fase 02 - Arquitectura de Software](./02-architecture/)
Contiene la especificaciÃƒÂ³n arquitectÃƒÂ³nica del sistema basada en el estÃƒÂ¡ndar C4 Model y el stack tecnolÃƒÂ³gico autorizado.
*   Ã°Å¸â€œâ€ž **[Plan de MigraciÃƒÂ³n .NET & Stack TecnolÃƒÂ³gico](./02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Roadmap para la migraciÃƒÂ³n a la lÃƒÂ­nea base corporativa .NET 8 LTS.
*   Ã°Å¸â€œâ€ž **[Mapa de Contextos Delimitados](./02-architecture/bounded-context-map.md)**: LÃƒÂ­mites de contexto DDD, patrones de integraciÃƒÂ³n y Capas Anti-CorrupciÃƒÂ³n (ACL).
*   Ã°Å¸â€œâ€ž **[EspecificaciÃƒÂ³n de Arquitectura C4 e Inventario TÃƒÂ©cnico](./02-architecture/architecture-spec.md)**: Diagramas tÃƒÂ©cnicos de Nivel 1 (Contexto), Nivel 2 (Contenedor) y Nivel 3 (Componente).
*   Ã°Å¸â€œâ€ž **[DefiniciÃƒÂ³n de Stack TecnolÃƒÂ³gico Autorizado](./02-architecture/stack.md)**: Definiciones de stack tÃƒÂ©cnico 100% agnÃƒÂ³stico a la nube y con capacidad local, y registro de riesgos.
*   Ã°Å¸â€œâ€ž **[Hoja de Referencia del Stack TecnolÃƒÂ³gico](./02-architecture/stack-summary.md)**: Referencia rÃƒÂ¡pida de alta densidad de todas las herramientas y capas seleccionadas.
*   Ã°Å¸â€œâ€ž **[EvaluaciÃƒÂ³n de Primitivas DDD](./02-architecture/nestjslatam-ddd-evaluation.md)**: EvaluaciÃƒÂ³n tÃƒÂ©cnica de capacidades de dominio para el NÃƒÂºcleo Central.

---

### Ã°Å¸â€œÅ“ [Phase 03 - Registros de DecisiÃƒÂ³n ArquitectÃƒÂ³nica (ADRs)](./03-adrs/)
El libro de contabilidad cronolÃƒÂ³gico e inmutable de las decisiones crÃƒÂ­ticas de diseÃƒÂ±o en formato MADR.
*   Ã°Å¸â€œâ€ž **[Libro Mayor ADR](./03-adrs/)**: Acceso al ÃƒÂ­ndice completo de **29 decisiones arquitectÃƒÂ³nicas activas** (que abarcan desde Nx Monorepo, Arquitectura Limpia, RLS, hasta AbstracciÃƒÂ³n de Proveedor de Identidad, CompilaciÃƒÂ³n de Grafos de Alto Rendimiento, Proyecciones de Salida Intercambiables, NÃƒÂºcleo de AutorizaciÃƒÂ³n Centralizado, Plataforma de ConfiguraciÃƒÂ³n, AbstracciÃƒÂ³n de Proveedor de Feature Flags, MFA/Sin ContraseÃƒÂ±a Adaptativo, APIs de Doble Protocolo, Infraestructura Autohospedada y Primitivas DDD TÃƒÂ¡cticas).
*   Ã°Å¸â€œâ€ž **[ADR-0024: Plataforma de GestiÃƒÂ³n de ConfiguraciÃƒÂ³n y Features](./03-adrs/0024-configuration-feature-management-platform.md)**: Establece config Multi-IdP, ConfiguraciÃƒÂ³n de Comportamiento del Sistema y framework de Feature Flags.
*   Ã°Å¸â€œâ€ž **[ADR-0025: AbstracciÃƒÂ³n de Proveedor de Feature Flags](./03-adrs/0025-feature-flag-provider-abstraction.md)**: Define el patrÃƒÂ³n enchufable `IFeatureFlagPort`: compatible con motor interno, LaunchDarkly, Unleash, ConfigCat, Azure App Config.
*   Ã°Å¸â€œâ€ž **[ADR-0026: MFA Adaptativo Multi-Tenancy y AutenticaciÃƒÂ³n Sin ContraseÃƒÂ±a](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md)**: Establece MFA adaptativo dinÃƒÂ¡mico, WebAuthn/Passkeys, dispositivos criptogrÃƒÂ¡ficos de confianza y recuperaciÃƒÂ³n de autoservicio.
*   Ã°Å¸â€œâ€ž **[ADR-0027: Estructura de API REST y gRPC de Doble Protocolo con Kong Gateway](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md)**: Establece interfaces duales REST para acceso pÃƒÂºblico y gRPC de alto rendimiento para consumo interno.
*   Ã°Å¸â€œâ€ž **[ADR-0028: Infraestructura de CÃƒÂ³digo Abierto Autohospedada para Despliegues HÃƒÂ­bridos y Locales](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md)**: Garantiza capacidad agnÃƒÂ³stica a la nube y despliegues localizados sin bloqueo de nube vÃƒÂ­a MinIO, RabbitMQ y HashiCorp Vault.
*   Ã°Å¸â€œâ€ž **[ADR-0029: Biblioteca de Primitivas DDD TÃƒÂ¡cticas](./03-adrs/0029-tactical-ddd-primitives-library.md)**: Estandariza el uso de bibliotecas autorizadas para soporte tÃƒÂ¡ctico de diseÃƒÂ±o conducido por dominio.

#### Ã°Å¸â€ºÂ¡Ã¯Â¸Â GestiÃƒÂ³n de Riesgos Operativos
*   Ã°Å¸â€œâ€ž **[EvaluaciÃƒÂ³n de Riesgo Financiero y Bloqueo de Proveedor](./02-architecture/vendor-risk-assessment.md)**: DocumentaciÃƒÂ³n de base que analiza Proveedores de Identidad, licenciamiento de Redis, plataformas de Feature Flags y cachÃƒÂ© de Nx Cloud para prevenir cargos financieros inesperados.

#### Ã°Å¸Ââ€ºÃ¯Â¸Â Gobernanza ArquitectÃƒÂ³nica y Matriz de Estado ADR

Antes de comenzar la fase de codificaciÃƒÂ³n, el Product Owner (PO) tiene autoridad absoluta para aprobar, diferir o vetar cualquier Registro de DecisiÃƒÂ³n ArquitectÃƒÂ³nica (ADR). A continuaciÃƒÂ³n se presenta la clasificaciÃƒÂ³n exhaustiva de las 29 decisiones activas que coinciden con sus estados de archivo:

### Ã°Å¸Å¸Â¢ 1. APROBADO Y ACEPTADO (LÃƒÂ­nea Base Autorizada Ã¢â‚¬â€ Listo para CodificaciÃƒÂ³n)
Estas decisiones estÃƒÂ¡n oficialmente **Aprobadas** y forman la arquitectura base del sistema. El desarrollo debe adherirse estrictamente a estos patrones:

| ID ADR | TÃƒÂ­tulo de la DecisiÃƒÂ³n | Estado | Impacto / Alcance | Siguientes Pasos / AcciÃƒÂ³n |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [OrquestaciÃƒÂ³n de Monorepo con Nx](./03-adrs/0001-monorepo-orchestration-nx.md) | Ã°Å¸Å¸Â¢ **Aceptado** | OrganizaciÃƒÂ³n central del monorepo y optimizaciÃƒÂ³n de velocidad. | LÃƒÂ­nea base aprobada. |
| **ADR-0002** | [Arquitectura Limpia y LÃƒÂ­mites Hexagonales](./03-adrs/0002-clean-architecture-nestjs.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Desacoplamiento de reglas de negocio del dominio central de la base de datos y frameworks. | LÃƒÂ­nea base aprobada. |
| **ADR-0003** | [EstÃƒÂ¡ndares Estrictos de TypeScript](./03-adrs/0003-strict-typescript-standards.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Puerta de calidad de anÃƒÂ¡lisis estÃƒÂ¡tico y obligatoriedad de tipado. | LÃƒÂ­nea base aprobada. |
| **ADR-0004** | [Arquitectura Offline de React Query](./03-adrs/0004-frontend-offline-resilience.md) | Ã°Å¸Å¸Â¢ **Aceptado** | CachÃƒÂ© local y mecanismo de contingencia para resiliencia de clientes. | LÃƒÂ­nea base aprobada. |
| **ADR-0005** | [Seguridad de Costo Cero vÃƒÂ­a CodeQL](./03-adrs/0005-ci-cd-quality-codeql.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Escaneo automatizado de vulnerabilidades en el pipeline de CI. | LÃƒÂ­nea base aprobada. |
| **ADR-0007** | [Estrategia de Loki y OpenTelemetry](./03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Rastreo distribuido y recolecciÃƒÂ³n de logs centralizada. | LÃƒÂ­nea base aprobada. |
| **ADR-0008** | [Patrones de Gateway y BFF](./03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Optimiza solicitudes de red para clientes multimÃƒÂ³dulo. | LÃƒÂ­nea base aprobada. |
| **ADR-0009** | [FijaciÃƒÂ³n Estricta de Dependencias](./03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Mitiga vulnerabilidades de inyecciÃƒÂ³n en la cadena de suministro. | LÃƒÂ­nea base aprobada. |
| **ADR-0010** | [Estrategia SaaS Multi-Tenancy](./03-adrs/0010-multi-tenancy-architecture-strategy.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Define la estrategia de aislamiento de base de datos por cliente corporativo. | LÃƒÂ­nea base aprobada. |
| **ADR-0011** | [Tolerancia a Fallos y Resiliencia](./03-adrs/0011-fault-tolerance-resiliency-patterns.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Cortacircuitos (`opossum`) y reintentos exponenciales. | LÃƒÂ­nea base aprobada. |
| **ADR-0012** | [AutorizaciÃƒÂ³n Avanzada (RBAC/ABAC)](./03-adrs/0012-advanced-authorization-rbac-abac.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Modelado de permisos contextuales de grano fino. | LÃƒÂ­nea base aprobada. |
| **ADR-0014** | [CachÃƒÂ© Distribuida (Redis)](./03-adrs/0014-distributed-caching-strategy-redis.md) | Ã°Å¸Å¸Â¢ **Aceptado** | CachÃƒÂ©s en memoria para validaciones de autenticaciÃƒÂ³n y sesiones activas. | LÃƒÂ­nea base aprobada. |
| **ADR-0015** | [Arquitectura Dirigida por Eventos](./03-adrs/0015-event-driven-architecture-intra-domain.md) | Ã°Å¸Å¸Â¢ **Aceptado** | PublicaciÃƒÂ³n de eventos asÃƒÂ­ncronos para sincronizaciÃƒÂ³n de estado. | LÃƒÂ­nea base aprobada. |
| **ADR-0016** | [Rastro de AuditorÃƒÂ­a de Negocio Inmutable](./03-adrs/0016-immutable-business-audit-trail.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Estrategia de auditorÃƒÂ­a a nivel de aplicaciÃƒÂ³n usando Eventos de Dominio. | LÃƒÂ­nea base aprobada. |
| **ADR-0017** | [Estrategia de Feature Flagging](./03-adrs/0017-feature-flagging-strategy.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Feature Flags inyectadas por infraestructura (Unleash/LaunchDarkly). | LÃƒÂ­nea base aprobada. |
| **ADR-0018** | [Puertas de Calidad de la PirÃƒÂ¡mide de Pruebas](./03-adrs/0018-testing-pyramid-quality-gates.md) | Ã°Å¸Å¸Â¢ **Aceptado** | LÃƒÂ­mites de cobertura para pruebas E2E, de contrato y unitarias (>70%). | LÃƒÂ­nea base aprobada. |
| **ADR-0019** | [Patrones de Dominio TÃƒÂ¡ctico](./03-adrs/0019-tactical-design-patterns-future-proofing.md) | Ã°Å¸Å¸Â¢ **Aceptado** | PatrÃƒÂ³n Result, Null Objects y Decoradores. | LÃƒÂ­nea base aprobada. |
| **ADR-0020** | [AbstracciÃƒÂ³n de Proveedor de Identidad](./03-adrs/0020-identity-provider-abstraction-strategy.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Desacopla UMS de Auth0, Keycloak o Entra ID. | LÃƒÂ­nea base aprobada. |
| **ADR-0021** | [Grafo de AutorizaciÃƒÂ³n de Alto Rendimiento](./03-adrs/0021-high-performance-auth-and-graph-compilation.md) | Ã°Å¸Å¸Â¢ **Aceptado** | CompilaciÃƒÂ³n de permisos optimizada bajo un lÃƒÂ­mite de latencia <5ms. | LÃƒÂ­nea base aprobada. |
| **ADR-0022** | [Proyecciones de Salida Enchufables](./03-adrs/0022-contextual-auth-and-pluggable-projections.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Capas de proyecciÃƒÂ³n de lectura conscientes del contexto fuera del nÃƒÂºcleo. | LÃƒÂ­nea base aprobada. |
| **ADR-0023** | [Acceso Centralizado vs Descentralizado](./03-adrs/0023-centralized-ums-vs-decentralized-access.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Establece el lÃƒÂ­mite del nÃƒÂºcleo de autorizaciÃƒÂ³n autoritativo. | LÃƒÂ­nea base aprobada. |
| **ADR-0024** | [ConfiguraciÃƒÂ³n y Feature Management](./03-adrs/0024-configuration-feature-management-platform.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Motor dinÃƒÂ¡mico de parÃƒÂ¡metros Multi-IdP. | LÃƒÂ­nea base aprobada. |
| **ADR-0025** | [AbstracciÃƒÂ³n de Proveedor de Feature Flag](./03-adrs/0025-feature-flag-provider-abstraction.md) | Ã°Å¸Å¸Â¢ **Aceptado** | `IFeatureFlagPort` enchufable para Unleash/ConfigCat. | LÃƒÂ­nea base aprobada. |
| **ADR-0026** | [MFA y AutenticaciÃƒÂ³n Sin ContraseÃƒÂ±a](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md) | Ã°Å¸Å¸Â¢ **Aceptado** | WebAuthn, Passkeys, TOTP y MFA adaptativo basado en riesgo. | LÃƒÂ­nea base aprobada. |
| **ADR-0027** | [Estructura de API REST & gRPC Doble Protocolo](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md) | Ã°Å¸Å¸Â¢ **Aceptado** | APIs RESTful pÃƒÂºblicas y servicios gRPC internos. | LÃƒÂ­nea base aprobada. |
| **ADR-0028** | [Infraestructura HÃƒÂ­brida Autohospedada](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Capacidad agnÃƒÂ³stica de nube (MinIO, RabbitMQ, Vault OSS). | LÃƒÂ­nea base aprobada. |
| **ADR-0029** | [Biblioteca de Primitivas DDD TÃƒÂ¡cticas](./03-adrs/0029-tactical-ddd-primitives-library.md) | Ã°Å¸Å¸Â¢ **Aceptado** | Estandariza librerÃƒÂ­as autorizadas para dominio modular tÃƒÂ¡ctico. | LÃƒÂ­nea base aprobada. |

### Ã°Å¸Å¸Â¡ 2. PROPUESTO Y PENDIENTE DE REVISIÃƒâ€œN (Pendientes de RevisiÃƒÂ³n/AprobaciÃƒÂ³n por el PO)
Estas decisiones se encuentran actualmente **Propuestas** y representan backlogs estratÃƒÂ©gicos. **Deben ser aprobadas formalmente por el PO antes de comenzar la codificaciÃƒÂ³n**:

| ID ADR | TÃƒÂ­tulo de la DecisiÃƒÂ³n | Estado | Impacto / Alcance | AcciÃƒÂ³n del PO Requerida |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0006** | [Futuros Microservicios vÃƒÂ­a Dapr](./03-adrs/0006-future-microservices-transition-dapr.md) | Ã°Å¸Å¸Â¡ **Propuesto** | IntegraciÃƒÂ³n de sidecar para estado distribuido y mensajerÃƒÂ­a. | **PO revisar/aprobar** para activar migraciÃƒÂ³n a microservicios. |
| **ADR-0013** | [Infraestructura en la Nube y DR](./03-adrs/0013-cloud-infrastructure-topology-dr.md) | Ã°Å¸Å¸Â¡ **Propuesto** | LÃƒÂ­mites de replicaciÃƒÂ³n de recuperaciÃƒÂ³n ante desastres multirregiÃƒÂ³n. | **PO revisar/aprobar** para autorizar presupuesto de despliegue. |
| **ADR-0031** | [AbstracciÃƒÂ³n de Identidad a Sujeto](./03-adrs/0031-abstract-identity-domain-subject.md) | Ã°Å¸Å¸Â¡ **Propuesto** | TransiciÃƒÂ³n de concepto de Empleado hacia Sujeto agnÃƒÂ³stico ligado a OrganizaciÃƒÂ³n. | **PO revisar/aprobar** para habilitar soporte completo B2B y multi-tenant. |
| **ADR-0032** | [La OrganizaciÃƒÂ³n como LÃƒÂ­mite del Dominio](./03-adrs/0032-organization-as-strategic-domain-boundary.md) | Ã°Å¸Å¸Â¡ **Propuesto** | Establece OrganizaciÃƒÂ³n como contenedor de Sujetos y Sistemas para gobernanza y seguridad. | **PO revisar/aprobar** para formalizar fronteras de propiedad y Zero Trust. |

### Ã°Å¸â€Â´ 3. CANCELADO, RECHAZADO O VETADO (Rechazados o Descartados por el PO)
Estas decisiones arquitectÃƒÂ³nicas han sido **Vetadas / Rechazadas** o **Canceladas** por el Product Owner y **nunca deben ser implementadas**:

*   *Actualmente, no hay decisiones rechazadas ni canceladas. Usted tiene plena autoridad para mover cualquier ADR a esta secciÃƒÂ³n para vetar su implementaciÃƒÂ³n.*

---

### Ã°Å¸â€ºÂ Ã¯Â¸Â [Fase 04 - EstÃƒÂ¡ndares y Artefactos de IngenierÃƒÂ­a](./04-artifacts/)
Pautas tÃƒÂ©cnicas, reglas de cÃƒÂ³digo limpio, estÃƒÂ¡ndares de seguridad y planes de calidad tÃƒÂ©cnica.
*   Ã°Å¸â€œâ€ž **[EstÃƒÂ¡ndares Globales de IngenierÃƒÂ­a](./04-artifacts/engineering-standards.md)**: Cumplimiento de SOLID, Arquitectura Limpia, OWASP y pautas DDD.
*   Ã°Å¸â€œâ€ž **[Modelo de Madurez ArquitectÃƒÂ³nica (AMM)](./04-artifacts/architecture-maturity-model.md)**: EvaluaciÃƒÂ³n de madurez de TOGAF ACMM y Well-Architected Framework.
*   Ã°Å¸â€œâ€ž **[Plan de Pruebas de Contrato](./04-artifacts/contract-testing-plan.md)**: IntegraciÃƒÂ³n segura de microservicios usando Pact.
*   Ã°Å¸â€œâ€ž **[Estrategia de Observabilidad Distribuida](./04-artifacts/observability-strategy.md)**: TelemetrÃƒÂ­a unificada usando OpenTelemetry y Grafana Loki.
*   Ã°Å¸â€œâ€ž **[AnÃƒÂ¡lisis de Brecha y Deuda TÃƒÂ©cnica](./04-artifacts/gap-analysis-and-optimization-plan.md)**: EvaluaciÃƒÂ³n de madurez de arquitectura y plan de mitigaciÃƒÂ³n tÃƒÂ©cnica.
*   Ã°Å¸â€œâ€ž **[EspecificaciÃƒÂ³n IAM Empresarial](./04-artifacts/enterprise-iam-ums-specification.md)**: Contratos y especificaciones del grafo de autorizaciÃƒÂ³n dinÃƒÂ¡mica.
*   Ã°Å¸â€œâ€ž **[EspecificaciÃƒÂ³n de AutenticaciÃƒÂ³n de Alta Concurrencia](./04-artifacts/high-concurrency-auth-specification.md)**: Esquemas de almacenamiento en cachÃƒÂ© de rendimiento y rotaciÃƒÂ³n de tokens.
*   Ã°Å¸â€œâ€ž **[Espec de Producto de la Consola Web UMS](./04-artifacts/ums-web-console-product-scope.md)**: Panel de control administrativo PAP y monitores SRE.
*   Ã°Å¸â€œâ€ž **[Espec de Plataforma de GestiÃƒÂ³n de ConfiguraciÃƒÂ³n y Features](./04-artifacts/ums-configuration-platform-spec.md)**: Motor de configuraciÃƒÂ³n multi-IdP y marco de feature flags.
*   Ã°Å¸â€œâ€ž **[Espec de AutenticaciÃƒÂ³n MFA y Sin ContraseÃƒÂ±a](./04-artifacts/mfa-passwordless-security-spec.md)**: EspecificaciÃƒÂ³n de autenticaciÃƒÂ³n adaptativa basada en riesgos y multifactor.
*   Ã°Å¸â€œâ€ž **[Espec Maestra de AuditorÃƒÂ­a Empresarial y bMAD](./04-artifacts/bmad-master-audit-alignment-report.md)**: EspecificaciÃƒÂ³n exhaustiva de alineaciÃƒÂ³n business-models-architecture-delivery.

---

### Ã°Å¸â€œË† [Fase 05 - Hoja de Ruta de Lanzamientos](./05-roadmap/)
Estrategias de liberaciÃƒÂ³n de cÃƒÂ³digo, despliegue continuo y automatizaciÃƒÂ³n de despliegue.
*   Ã°Å¸â€œâ€ž **[Estrategia de Versionado y Lanzamiento](./05-roadmap/versioning-and-audit-strategy.md)**: GestiÃƒÂ³n de tags y publicaciones utilizando Nx Release.
