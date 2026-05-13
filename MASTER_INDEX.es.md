# 🗺️ Índice Maestro Global (Punto de Entrada de UMS)

> 🌍 **Navegación Bilingüe:** [🇺🇸 English Version (Master Index)](./MASTER_INDEX.md) | [🇪🇸 Versión en Español](./MASTER_INDEX.es.md)

Bienvenido al sistema nervioso central del **Sistema de Gestión de Usuarios (UMS)**. Este índice maestro sirve como la puerta de enrutamiento canónica para todos los actores que interactúan con este repositorio. Ubica tu perfil asignado a continuación para acceder a tu ruta acelerada de lectura obligatoria, garantizando el cumplimiento técnico y procedimental.

---

## 🚀 1. Rutas Aceleradas por Rol (Navegación por Perfiles)

Identifica tu relación actual con el proyecto para desbloquear la jerarquía de lectura obligatoria a medida:

| Rol Empresarial | Ruta de Lectura Recomendada | Cumplimiento Esperado |
| :--- | :--- | :--- |
| **Proveedor de Software Externo** | 1. [Documentación Maestra bMAD](./src/ums-workspace/docs/es/index.md)<br>2. [Estándares Globales de Ingeniería](./src/ums-workspace/docs/es/04-artifacts/engineering-standards.md)<br>3. [Especificación C4 Maestra](./src/ums-workspace/docs/es/02-architecture/architecture-spec.md) | Validar el stack técnico base, aislamiento de entornos y fronteras lógicas antes de iniciar cualquier trabajo. |
| **Desarrollador Backend / QA** | 1. [Plan de Migración .NET & Límites Hexagonales](./src/ums-workspace/docs/es/02-architecture/dotnet-migration-and-tech-stack-plan.md)<br>2. [Plan de Pruebas de Contrato](./src/ums-workspace/docs/es/04-artifacts/contract-testing-plan.md)<br>3. [Pirámide de Pruebas y Puertas de Calidad (ADR-0018)](./src/ums-workspace/docs/es/03-adrs/0018-testing-pyramid-quality-gates.md) | Garantizar umbrales de cobertura, patrones DDD y validación estática de código (linting), evitando fugas de lógica hacia el core. |
| **Arquitecto de Soluciones** | 1. [Mapa de Contextos Delimitados](./src/ums-workspace/docs/es/02-architecture/bounded-context-map.md)<br>2. [Registro Histórico ADR](./src/ums-workspace/docs/es/03-adrs/)<br>3. [Estrategia de Observabilidad Distribuida](./src/ums-workspace/docs/es/04-artifacts/observability-strategy.md) | Evaluar la integridad general de los patrones, los índices de riesgo técnico y aprobar los criterios de extracción de microservicios. |
| **Product Owner / PM** | 1. [Visión de Producto](./src/ums-workspace/docs/es/00-product/product-vision.md)<br>2. [Directorio de Casos de Uso Atómicos](./src/ums-workspace/docs/es/01-requirements/usecases/)<br>3. [Estrategia de Deuda y Análisis de Brechas](./src/ums-workspace/docs/es/04-artifacts/gap-analysis-and-optimization-plan.md) | Sincronizar hitos de lanzamientos con la madurez técnica de las ADRs y gobernar las definiciones de negocio de los casos de uso. |

---

## 🛡️ 2. Ruta Obligatoria de Cumplimiento (Línea Base Global)

Todos los contribuyentes del ecosistema—sin importar su antigüedad—DEBEN adherirse y hacer cumplir los anclajes fundamentales listados a continuación:

*   📄 **[Estándares Globales de Ingeniería](./src/ums-workspace/docs/es/04-artifacts/engineering-standards.md)**: Normas de codificación no negociables, principios SOLID, Clean Code y cumplimiento OWASP.
*   📄 **[Modelo de Madurez Arquitectónica (AMM)](./src/ums-workspace/docs/es/04-artifacts/architecture-maturity-model.md)**: Criterios exhaustivos para garantizar la madurez a nivel empresarial.
*   📄 **[Plan de Migración a .NET Core](./src/ums-workspace/docs/es/02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Criterios de desacoplamiento obligatorios para proteger los modelos de dominio bajo C#.
*   📄 **[Estrategia de Pruebas de Contrato](./src/ums-workspace/docs/es/04-artifacts/contract-testing-plan.md)**: Puertas de calidad para validaciones de integración desacopladas.

---

## 🏛️ 3. Estructura y Taxonomía bMAD (Navegación por Fases)

La documentación de este repositorio sigue el **Método bMAD** (fases secuenciales numéricas). Haz clic en las referencias para navegar a través de la base de conocimiento:

### 🎯 **[Fase 00 - Visión de Producto](./src/ums-workspace/docs/es/00-product/)**
*   [Visión de Producto](./src/ums-workspace/docs/es/00-product/product-vision.md) | [Contexto de Negocio](./src/ums-workspace/docs/es/00-product/business-context.md) | [Alcance y Límites](./src/ums-workspace/docs/es/00-product/scope.md) | [Objetivos Estratégicos (OKRs)](./src/ums-workspace/docs/es/00-product/objectives.md) | [Mapa de Stakeholders](./src/ums-workspace/docs/es/00-product/stakeholders.md)

### 📋 **[Fase 01 - Requisitos de Dominio](./src/ums-workspace/docs/es/01-requirements/)**
*   [Catálogo de Casos de Uso Atómicos](./src/ums-workspace/docs/es/01-requirements/usecases/) | [Modelo de Datos Conceptual](./src/ums-workspace/docs/es/01-requirements/conceptual-data-model.md) | [Matriz de Permisos Granulares](./src/ums-workspace/docs/es/01-requirements/permission-matrix-example.md) | [Glosario Ubicuo DDD](./src/ums-workspace/docs/es/01-requirements/glossary.md)

### 🏗️ **[Fase 02 - Diseño de Arquitectura](./src/ums-workspace/docs/es/02-architecture/)**
*   **[Plan de Migración .NET 8](./src/ums-workspace/docs/es/02-architecture/dotnet-migration-and-tech-stack-plan.md)** | [Especificación Maestra C4](./src/ums-workspace/docs/es/02-architecture/architecture-spec.md) | [Mapa de Contextos](./src/ums-workspace/docs/es/02-architecture/bounded-context-map.md) | [Evaluación de Riesgo de Bloqueo de Proveedor](./src/ums-workspace/docs/es/02-architecture/vendor-risk-assessment.md)

### 📜 **[Fase 03 - Registro de Decisiones Arquitectónicas (ADRs)](./src/ums-workspace/docs/es/03-adrs/)**
*   [Estrategia Nx Monorepo](./src/ums-workspace/docs/es/03-adrs/0001-monorepo-orchestration-nx.md) | [Límites de Arquitectura Limpia](./src/ums-workspace/docs/es/03-adrs/0002-clean-architecture-nestjs.md) | [Resiliencia Frontend Desconectado](./src/ums-workspace/docs/es/03-adrs/0004-frontend-offline-resilience.md) | [CodeQL Costo Cero](./src/ums-workspace/docs/es/03-adrs/0005-ci-cd-quality-codeql.md) | [Patrón Gateway & BFF](./src/ums-workspace/docs/es/03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | [Ver Historial de las 29 ADRs](./src/ums-workspace/docs/es/03-adrs/)

### 🛠️ **[Fase 04 - Estándares y Artefactos de Ingeniería](./src/ums-workspace/docs/es/04-artifacts/)**
*   [Madurez de Arquitectura](./src/ums-workspace/docs/es/04-artifacts/architecture-maturity-model.md) | [Estrategia de Observabilidad](./src/ums-workspace/docs/es/04-artifacts/observability-strategy.md) | [Especificación IAM Empresarial](./src/ums-workspace/docs/es/04-artifacts/enterprise-iam-ums-specification.md) | [Especificación de Alta Concurrencia](./src/ums-workspace/docs/es/04-artifacts/high-concurrency-auth-specification.md) | [Especificación MFA/Passwordless](./src/ums-workspace/docs/es/04-artifacts/mfa-passwordless-security-spec.md)

### 📈 **[Fase 05 - Hoja de Ruta de Lanzamientos](./src/ums-workspace/docs/es/05-roadmap/)**
*   [Estrategia de Versionado y Auditoría](./src/ums-workspace/docs/es/05-roadmap/versioning-and-audit-strategy.md) | [CHANGELOG del Monorepo](./src/ums-workspace/CHANGELOG.md)

---

👉 **[Volver al README Principal](./README.es.md)**


