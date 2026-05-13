# 🗺️ Mapa de Navegación Maestro - Base de Conocimiento de UMS

> 🌍 **Selector de Idioma:** [🇪🇸 Español](./index.md) | [🇺🇸 English](../en/index.md)

Bienvenido a la documentación técnica maestra del **Sistema de Gestión de Usuarios (UMS)**. Esta base de conocimiento está estructurada bajo el **estrategia spec-driven AI BMAD-METHOD (fases secuenciales numéricas)** para garantizar la máxima descubribilidad, trazabilidad y soporte continuo tanto para desarrolladores humanos como para copilotos de IA autónomos.

---

## 🧭 Índice de Navegación Basado en Fases

### 🎯 [Fase 00 - Visión de Producto](./00-product/)
Define el contexto de negocio, los pilares estratégicos del producto y el mapa de interesados.
*   📄 **[Visión del Producto](./00-product/product-vision.md)**: Pilares de identidad soberana, delegación de autenticación y multi-tenancy dinámico.
*   📄 **[Contexto de Negocio](./00-product/business-context.md)**: Declaración del problema, solución propuesta y diagramas conceptuales de integración.
*   📄 **[Alcance y Límites](./00-product/scope.md)**: Detalle de capacidades dentro y fuera del alcance.
*   📄 **[Objetivos Estratégicos (OKRs)](./00-product/objectives.md)**: Métricas cuantificables y KRs para seguridad, latencia y autoservicio.
*   📄 **[Mapa de Interesados (Stakeholders)](./00-product/stakeholders.md)**: Matriz de responsabilidades y mapeo de expectativas para roles técnicos y de negocio.

---

### 📋 [Fase 01 - Requisitos de Dominio](./01-requirements/)
Detalla reglas de negocio, secuencias interactivas, diagramas conceptuales de bases de datos y la definición formal del Lenguaje Ubicuo.
*   📄 **[Glosario de Términos (Lenguaje Ubicuo)](./01-requirements/glossary.md)**: Diccionario formal DDD del dominio central.
*   📄 **[Modelo de Datos Conceptual](./01-requirements/conceptual-data-model.md)**: Lógica relacional de PostgreSQL y políticas de Seguridad a Nivel de Fila (RLS).
*   📄 **[Matriz de Permisos Granulares](./01-requirements/permission-matrix-example.md)**: Lógica de acceso detallada (RBAC/ABAC) y regla de precedencia de denegación explícita.
*   📂 **[Casos de Uso Atómicos](./01-requirements/usecases/)**:
    *   [UC-01: Autenticación de Usuario vía IdP Externo](./01-requirements/usecases/uc-01-user-authentication.md)
    *   [UC-02: Compilación de Grafo de Autorización](./01-requirements/usecases/uc-02-build-authorization-graph.md)
    *   [UC-03: Crear e Instanciar Plantilla de Autorización](./01-requirements/usecases/uc-03-create-authorization-template.md)
    *   [UC-04: Registrar Organización y Configurar Estrategia de IdP](./01-requirements/usecases/uc-04-register-organization.md)
    *   [UC-05: Registrar Sistema y Definir Topología de Menú](./01-requirements/usecases/uc-05-register-system-topology.md)
    *   [UC-06: Crear Perfil y Asignar Plantilla Manualmente](./01-requirements/usecases/uc-06-create-profile-manual-template.md)
    *   [UC-07: Auto-Asignar Plantilla al Crear Perfil](./01-requirements/usecases/uc-07-auto-assign-template.md)
    *   [UC-08: Diagnosticar Permisos vía Visualizador de Grafos](./01-requirements/usecases/uc-08-visual-graph-resolver.md)
    *   [UC-09: Resolver Configuración Jerárquica del Sistema](./01-requirements/usecases/uc-09-resolve-hierarchical-config.md)
    *   [UC-10: Autenticar vía Página de Inicio Personalizable](./01-requirements/usecases/uc-10-hosted-login-redirection.md)
    *   [UC-11: Autenticación Adaptativa Multifactor y Sin Contraseña](./01-requirements/usecases/uc-11-mfa-passwordless-adaptive-auth.md)
    *   [UC-12: Flujo de Aprobación y Petición de Acceso Externo B2B](./01-requirements/usecases/uc-12-external-b2b-access-request-approval.md)

---

### 🏗️ [Fase 02 - Arquitectura de Software](./02-architecture/)
Contiene la especificación arquitectónica del sistema basada en el estándar C4 Model y el stack tecnológico autorizado.
*   📄 **[Plan de Migración .NET & Stack Tecnológico](./02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Roadmap para la migración a la línea base corporativa .NET 8 LTS.
*   📄 **[Mapa de Contextos Delimitados](./02-architecture/bounded-context-map.md)**: Límites de contexto DDD, patrones de integración y Capas Anti-Corrupción (ACL).
*   📄 **[Especificación de Arquitectura C4 e Inventario Técnico](./02-architecture/architecture-spec.md)**: Diagramas técnicos de Nivel 1 (Contexto), Nivel 2 (Contenedor) y Nivel 3 (Componente).
*   📄 **[Definición de Stack Tecnológico Autorizado](./02-architecture/stack.md)**: Definiciones de stack técnico 100% agnóstico a la nube y con capacidad local, y registro de riesgos.
*   📄 **[Hoja de Referencia del Stack Tecnológico](./02-architecture/stack-summary.md)**: Referencia rápida de alta densidad de todas las herramientas y capas seleccionadas.
*   📄 **[Evaluación de Primitivas DDD](./02-architecture/nestjslatam-ddd-evaluation.md)**: Evaluación técnica de capacidades de dominio para el Núcleo Central.

---

### 📜 [Phase 03 - Registros de Decisión Arquitectónica (ADRs)](./03-adrs/)
El libro de contabilidad cronológico e inmutable de las decisiones críticas de diseño en formato MADR.
*   📄 **[Libro Mayor ADR](./03-adrs/)**: Acceso al índice completo de **29 decisiones arquitectónicas activas** (que abarcan desde Nx Monorepo, Arquitectura Limpia, RLS, hasta Abstracción de Proveedor de Identidad, Compilación de Grafos de Alto Rendimiento, Proyecciones de Salida Intercambiables, Núcleo de Autorización Centralizado, Plataforma de Configuración, Abstracción de Proveedor de Feature Flags, MFA/Sin Contraseña Adaptativo, APIs de Doble Protocolo, Infraestructura Autohospedada y Primitivas DDD Tácticas).
*   📄 **[ADR-0024: Plataforma de Gestión de Configuración y Features](./03-adrs/0024-configuration-feature-management-platform.md)**: Establece config Multi-IdP, Configuración de Comportamiento del Sistema y framework de Feature Flags.
*   📄 **[ADR-0025: Abstracción de Proveedor de Feature Flags](./03-adrs/0025-feature-flag-provider-abstraction.md)**: Define el patrón enchufable `IFeatureFlagPort`: compatible con motor interno, LaunchDarkly, Unleash, ConfigCat, Azure App Config.
*   📄 **[ADR-0026: MFA Adaptativo Multi-Tenancy y Autenticación Sin Contraseña](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md)**: Establece MFA adaptativo dinámico, WebAuthn/Passkeys, dispositivos criptográficos de confianza y recuperación de autoservicio.
*   📄 **[ADR-0027: Estructura de API REST y gRPC de Doble Protocolo con Kong Gateway](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md)**: Establece interfaces duales REST para acceso público y gRPC de alto rendimiento para consumo interno.
*   📄 **[ADR-0028: Infraestructura de Código Abierto Autohospedada para Despliegues Híbridos y Locales](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md)**: Garantiza capacidad agnóstica a la nube y despliegues localizados sin bloqueo de nube vía MinIO, RabbitMQ y HashiCorp Vault.
*   📄 **[ADR-0029: Biblioteca de Primitivas DDD Tácticas](./03-adrs/0029-tactical-ddd-primitives-library.md)**: Estandariza el uso de bibliotecas autorizadas para soporte táctico de diseño conducido por dominio.

#### 🛡️ Gestión de Riesgos Operativos
*   📄 **[Evaluación de Riesgo Financiero y Bloqueo de Proveedor](./02-architecture/vendor-risk-assessment.md)**: Documentación de base que analiza Proveedores de Identidad, licenciamiento de Redis, plataformas de Feature Flags y caché de Nx Cloud para prevenir cargos financieros inesperados.

#### 🏛️ Gobernanza Arquitectónica y Matriz de Estado ADR

Antes de comenzar la fase de codificación, el Product Owner (PO) tiene autoridad absoluta para aprobar, diferir o vetar cualquier Registro de Decisión Arquitectónica (ADR). A continuación se presenta la clasificación exhaustiva de las 29 decisiones activas que coinciden con sus estados de archivo:

### 🟢 1. APROBADO Y ACEPTADO (Línea Base Autorizada — Listo para Codificación)
Estas decisiones están oficialmente **Aprobadas** y forman la arquitectura base del sistema. El desarrollo debe adherirse estrictamente a estos patrones:

| ID ADR | Título de la Decisión | Estado | Impacto / Alcance | Siguientes Pasos / Acción |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [Orquestación de Monorepo con Nx](./03-adrs/0001-monorepo-orchestration-nx.md) | 🟢 **Aceptado** | Organización central del monorepo y optimización de velocidad. | Línea base aprobada. |
| **ADR-0002** | [Arquitectura Limpia y Límites Hexagonales](./03-adrs/0002-clean-architecture-nestjs.md) | 🟢 **Aceptado** | Desacoplamiento de reglas de negocio del dominio central de la base de datos y frameworks. | Línea base aprobada. |
| **ADR-0003** | [Estándares Estrictos de TypeScript](./03-adrs/0003-strict-typescript-standards.md) | 🟢 **Aceptado** | Puerta de calidad de análisis estático y obligatoriedad de tipado. | Línea base aprobada. |
| **ADR-0004** | [Arquitectura Offline de React Query](./03-adrs/0004-frontend-offline-resilience.md) | 🟢 **Aceptado** | Caché local y mecanismo de contingencia para resiliencia de clientes. | Línea base aprobada. |
| **ADR-0005** | [Seguridad de Costo Cero vía CodeQL](./03-adrs/0005-ci-cd-quality-codeql.md) | 🟢 **Aceptado** | Escaneo automatizado de vulnerabilidades en el pipeline de CI. | Línea base aprobada. |
| **ADR-0007** | [Estrategia de Loki y OpenTelemetry](./03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | 🟢 **Aceptado** | Rastreo distribuido y recolección de logs centralizada. | Línea base aprobada. |
| **ADR-0008** | [Patrones de Gateway y BFF](./03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | 🟢 **Aceptado** | Optimiza solicitudes de red para clientes multimódulo. | Línea base aprobada. |
| **ADR-0009** | [Fijación Estricta de Dependencias](./03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | 🟢 **Aceptado** | Mitiga vulnerabilidades de inyección en la cadena de suministro. | Línea base aprobada. |
| **ADR-0010** | [Estrategia SaaS Multi-Tenancy](./03-adrs/0010-multi-tenancy-architecture-strategy.md) | 🟢 **Aceptado** | Define la estrategia de aislamiento de base de datos por cliente corporativo. | Línea base aprobada. |
| **ADR-0011** | [Tolerancia a Fallos y Resiliencia](./03-adrs/0011-fault-tolerance-resiliency-patterns.md) | 🟢 **Aceptado** | Cortacircuitos (`opossum`) y reintentos exponenciales. | Línea base aprobada. |
| **ADR-0012** | [Autorización Avanzada (RBAC/ABAC)](./03-adrs/0012-advanced-authorization-rbac-abac.md) | 🟢 **Aceptado** | Modelado de permisos contextuales de grano fino. | Línea base aprobada. |
| **ADR-0014** | [Caché Distribuida (Redis)](./03-adrs/0014-distributed-caching-strategy-redis.md) | 🟢 **Aceptado** | Cachés en memoria para validaciones de autenticación y sesiones activas. | Línea base aprobada. |
| **ADR-0015** | [Arquitectura Dirigida por Eventos](./03-adrs/0015-event-driven-architecture-intra-domain.md) | 🟢 **Aceptado** | Publicación de eventos asíncronos para sincronización de estado. | Línea base aprobada. |
| **ADR-0016** | [Rastro de Auditoría de Negocio Inmutable](./03-adrs/0016-immutable-business-audit-trail.md) | 🟢 **Aceptado** | Estrategia de auditoría a nivel de aplicación usando Eventos de Dominio. | Línea base aprobada. |
| **ADR-0017** | [Estrategia de Feature Flagging](./03-adrs/0017-feature-flagging-strategy.md) | 🟢 **Aceptado** | Feature Flags inyectadas por infraestructura (Unleash/LaunchDarkly). | Línea base aprobada. |
| **ADR-0018** | [Puertas de Calidad de la Pirámide de Pruebas](./03-adrs/0018-testing-pyramid-quality-gates.md) | 🟢 **Aceptado** | Límites de cobertura para pruebas E2E, de contrato y unitarias (>70%). | Línea base aprobada. |
| **ADR-0019** | [Patrones de Dominio Táctico](./03-adrs/0019-tactical-design-patterns-future-proofing.md) | 🟢 **Aceptado** | Patrón Result, Null Objects y Decoradores. | Línea base aprobada. |
| **ADR-0020** | [Abstracción de Proveedor de Identidad](./03-adrs/0020-identity-provider-abstraction-strategy.md) | 🟢 **Aceptado** | Desacopla UMS de Auth0, Keycloak o Entra ID. | Línea base aprobada. |
| **ADR-0021** | [Grafo de Autorización de Alto Rendimiento](./03-adrs/0021-high-performance-auth-and-graph-compilation.md) | 🟢 **Aceptado** | Compilación de permisos optimizada bajo un límite de latencia <5ms. | Línea base aprobada. |
| **ADR-0022** | [Proyecciones de Salida Enchufables](./03-adrs/0022-contextual-auth-and-pluggable-projections.md) | 🟢 **Aceptado** | Capas de proyección de lectura conscientes del contexto fuera del núcleo. | Línea base aprobada. |
| **ADR-0023** | [Acceso Centralizado vs Descentralizado](./03-adrs/0023-centralized-ums-vs-decentralized-access.md) | 🟢 **Aceptado** | Establece el límite del núcleo de autorización autoritativo. | Línea base aprobada. |
| **ADR-0024** | [Configuración y Feature Management](./03-adrs/0024-configuration-feature-management-platform.md) | 🟢 **Aceptado** | Motor dinámico de parámetros Multi-IdP. | Línea base aprobada. |
| **ADR-0025** | [Abstracción de Proveedor de Feature Flag](./03-adrs/0025-feature-flag-provider-abstraction.md) | 🟢 **Aceptado** | `IFeatureFlagPort` enchufable para Unleash/ConfigCat. | Línea base aprobada. |
| **ADR-0026** | [MFA y Autenticación Sin Contraseña](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md) | 🟢 **Aceptado** | WebAuthn, Passkeys, TOTP y MFA adaptativo basado en riesgo. | Línea base aprobada. |
| **ADR-0027** | [Estructura de API REST & gRPC Doble Protocolo](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md) | 🟢 **Aceptado** | APIs RESTful públicas y servicios gRPC internos. | Línea base aprobada. |
| **ADR-0028** | [Infraestructura Híbrida Autohospedada](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md) | 🟢 **Aceptado** | Capacidad agnóstica de nube (MinIO, RabbitMQ, Vault OSS). | Línea base aprobada. |
| **ADR-0029** | [Biblioteca de Primitivas DDD Tácticas](./03-adrs/0029-tactical-ddd-primitives-library.md) | 🟢 **Aceptado** | Estandariza librerías autorizadas para dominio modular táctico. | Línea base aprobada. |

### 🟡 2. PROPUESTO Y PENDIENTE DE REVISIÓN (Pendientes de Revisión/Aprobación por el PO)
Estas decisiones se encuentran actualmente **Propuestas** y representan backlogs estratégicos. **Deben ser aprobadas formalmente por el PO antes de comenzar la codificación**:

| ID ADR | Título de la Decisión | Estado | Impacto / Alcance | Acción del PO Requerida |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0006** | [Futuros Microservicios vía Dapr](./03-adrs/0006-future-microservices-transition-dapr.md) | 🟡 **Propuesto** | Integración de sidecar para estado distribuido y mensajería. | **PO revisar/aprobar** para activar migración a microservicios. |
| **ADR-0013** | [Infraestructura en la Nube y DR](./03-adrs/0013-cloud-infrastructure-topology-dr.md) | 🟡 **Propuesto** | Límites de replicación de recuperación ante desastres multirregión. | **PO revisar/aprobar** para autorizar presupuesto de despliegue. |

### 🔴 3. CANCELADO, RECHAZADO O VETADO (Rechazados o Descartados por el PO)
Estas decisiones arquitectónicas han sido **Vetadas / Rechazadas** o **Canceladas** por el Product Owner y **nunca deben ser implementadas**:

*   *Actualmente, no hay decisiones rechazadas ni canceladas. Usted tiene plena autoridad para mover cualquier ADR a esta sección para vetar su implementación.*

---

### 🛠️ [Fase 04 - Estándares y Artefactos de Ingeniería](./04-artifacts/)
Pautas técnicas, reglas de código limpio, estándares de seguridad y planes de calidad técnica.
*   📄 **[Estándares Globales de Ingeniería](./04-artifacts/engineering-standards.md)**: Cumplimiento de SOLID, Arquitectura Limpia, OWASP y pautas DDD.
*   📄 **[Modelo de Madurez Arquitectónica (AMM)](./04-artifacts/architecture-maturity-model.md)**: Evaluación de madurez de TOGAF ACMM y Well-Architected Framework.
*   📄 **[Plan de Pruebas de Contrato](./04-artifacts/contract-testing-plan.md)**: Integración segura de microservicios usando Pact.
*   📄 **[Estrategia de Observabilidad Distribuida](./04-artifacts/observability-strategy.md)**: Telemetría unificada usando OpenTelemetry y Grafana Loki.
*   📄 **[Análisis de Brecha y Deuda Técnica](./04-artifacts/gap-analysis-and-optimization-plan.md)**: Evaluación de madurez de arquitectura y plan de mitigación técnica.
*   📄 **[Especificación IAM Empresarial](./04-artifacts/enterprise-iam-ums-specification.md)**: Contratos y especificaciones del grafo de autorización dinámica.
*   📄 **[Especificación de Autenticación de Alta Concurrencia](./04-artifacts/high-concurrency-auth-specification.md)**: Esquemas de almacenamiento en caché de rendimiento y rotación de tokens.
*   📄 **[Espec de Producto de la Consola Web UMS](./04-artifacts/ums-web-console-product-scope.md)**: Panel de control administrativo PAP y monitores SRE.
*   📄 **[Espec de Plataforma de Gestión de Configuración y Features](./04-artifacts/ums-configuration-platform-spec.md)**: Motor de configuración multi-IdP y marco de feature flags.
*   📄 **[Espec de Autenticación MFA y Sin Contraseña](./04-artifacts/mfa-passwordless-security-spec.md)**: Especificación de autenticación adaptativa basada en riesgos y multifactor.
*   📄 **[Espec Maestra de Auditoría Empresarial y bMAD](./04-artifacts/bmad-master-audit-alignment-report.md)**: Especificación exhaustiva de alineación business-models-architecture-delivery.

---

### 📈 [Fase 05 - Hoja de Ruta de Lanzamientos](./05-roadmap/)
Estrategias de liberación de código, despliegue continuo y automatización de despliegue.
*   📄 **[Estrategia de Versionado y Lanzamiento](./05-roadmap/versioning-and-audit-strategy.md)**: Gestión de tags y publicaciones utilizando Nx Release.
