# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular estándarizado para Identidad y Autorización Unificada.**
>
> ![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## Language / Bilingüe
- [English](./README.md) | **Español**

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
| Construir u operar el sistema | [Engine Room](./src/) | [Habilitadores Técnicos](./docs/architecture/blueprints-es/technical-enablers/index.md) → [Portal de Operaciones](./docs/operations/index.md) |
| Navegar todo | [Índice Maestro](./docs/MASTER_INDEX.es.md) | Árbol completo de documentación por fase de ciclo de vida.
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
| **Requisitos** | [Índice de Requisitos](./docs/governance/requirements-es/index.md) | Historias funcionales, glosario de negocio, modelo de permisos y modelo conceptual. |
| **Arquitectura** | [Portal de Arquitectura](./docs/architecture/index.md) | Stack, registro ADR, especificación C4, contextos delimitados y habilitadores técnicos. |
| **Infraestructura** | [Infraestructura](./docs/infrastructure/index.md) | Docker, Kong, Kubernetes y configuración de ambientes. |
| **Operaciones** | [Portal de Operaciones](./docs/operations/index.md) | Runbooks, observabilidad, operación SQL y prácticas SRE. |
| **Conocimiento** | [Base de Conocimiento](./docs/knowledge/index.md) | Rutas de lectura recomendadas, POCs, investigación y onboarding.
## Lectura Recomendada por Rol
| Perfil | Objetivo de Lectura | Ruta de Aprendizaje (Links Directos) |
| :--- | :--- | :--- |
| **Director / Ejecutivo** | Valor de negocio, alcance MVP y confianza de entrega | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Priorización MVP](./docs/governance/roadmap/mvp-functional-prioritization.es.md) → [Backlog de Producto MVP](./docs/governance/project-es/mvp-product-backlog.md) |
| **Product Owner** | Alcance funcional, secuencia y propiedad del backlog | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Requisitos](./docs/governance/requirements-es/index.md) → [Priorización MVP](./docs/governance/roadmap/mvp-functional-prioritization.es.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) |
| **Analista de Negocio** | Narrativa de negocio, reglas y criterios de aceptación | [Requisitos](./docs/governance/requirements-es/index.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Glosario](./docs/governance/requirements-es/glossary.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) |
| **Arquitecto de SW** | Diseño técnico, decisiones y límites de dominio | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Portal de Arquitectura](./docs/architecture/index.md) → [Especificación C4](./docs/architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./docs/architecture/adrs-es/index.md) |
| **Developer (BE/FE)** | Qué construir, por qué importa y cómo encaja | [Visión de Producto](./docs/governance/product-es/product-vision.md) → [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Historias Funcionales](./docs/governance/requirements-es/functional-stories/index.md) → [Habilitadores Técnicos](./docs/architecture/blueprints-es/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Ambientes, confiabilidad y observabilidad | [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Infraestructura](./docs/infrastructure/index.md) → [Portal de Operaciones](./docs/operations/index.md) → [Estrategia de Observabilidad](./docs/architecture/artifacts-es/observability-strategy.md) |
| **QA / Seguridad** | Calidad, riesgos y estrategia de verificación | [Backlog de Producto](./docs/governance/project-es/mvp-product-backlog.md) → [Plan de Pruebas de Contrato](./docs/architecture/artifacts-es/contract-testing-plan.md) → [Especificación IAM](./docs/architecture/artifacts-es/enterprise-iam-ums-specification.md) → [Modelo de Madurez](./docs/architecture/artifacts-es/architecture-maturity-model.md) |
| **IA / Agentes** | BMAD-METHOD y reglas del repositorio | [Reglas de Agentes](./AGENTS.md) → [Auditoría BMAD](./docs/architecture/artifacts-es/bmad-master-audit-alignment-report.md) → [Auditoría Taxonomía](./docs/governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

## Contribución y Gobernanza
- **Flujo**: Este repo utiliza [BMAD-METHOD](./AGENTS.md) para documentación dirigida por especificaciones.
- **Navegación**: Visita el [**Índice Maestro**](./docs/MASTER_INDEX.es.md) para ver el árbol completo de documentos.
