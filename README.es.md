# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular estandarizado para Identidad y Autorización Unificada.**
>
> ![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- [English](./README.md) | **Español**

---

## 🧭 Índice Maestro de Navegación
Empieza aquí si eres nuevo en UMS. Este índice le da a cada lector una ruta rápida dentro del repositorio sin necesidad de conocer la estructura de carpetas.

| Quiero... | Empezar Aquí | Luego Leer |
| :--- | :--- | :--- |
| Entender el producto | [Visión de Producto](./governance/product-es/product-vision.md) | [Contexto de Negocio](./governance/product-es/business-context.md) → [Alcance](./governance/product-es/scope.md) |
| Ver qué debe construirse primero | [Priorización MVP](./governance/roadmap/mvp-functional-prioritization.es.md) | [Backlog de Producto MVP](./governance/project-es/mvp-product-backlog.md) |
| Revisar requisitos funcionales | [Índice de Requisitos](./governance/requirements-es/index.md) | [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) → [Glosario](./governance/requirements-es/glossary.md) |
| Validar datos y modelo de dominio | [Modelo de Datos Conceptual](./governance/requirements-es/conceptual-data-model.md) | [Formatos de Exportación ER](./architecture/blueprints-es/er-export-formats.md) → [Diseño ER de Base de Datos](./architecture/blueprints-es/database-design-er.md) |
| Entender la arquitectura | [Portal de Arquitectura](./architecture/index.es.md) | [Especificación C4](./architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./architecture/adrs-es/index.md) |
| Construir u operar el sistema | [Engine Room](./src/) | [Habilitadores Técnicos](./architecture/blueprints-es/technical-enablers/index.md) → [Portal de Operaciones](./operations/index.md) |
| Navegar todo | [Índice Maestro](./MASTER_INDEX.es.md) | Árbol completo de documentación por fase de ciclo de vida. |

---

## 🚀 Inicio Rápido (Motor Técnico)
```powershell
cd src
npm install; npx nx run app-web:dev
```

---

## 📍 Hub de Conocimiento
| Dominio | Índice del Portal | Contenidos |
| :--- | :--- | :--- |
| ⚖️ **Gobernanza** | [Portal de Gobernanza](./governance/index.es.md) | Dirección de producto, estándares, roadmap, backlog de proyecto y auditorías. |
| 🗂️ **Entrega de Proyecto** | [Backlog de Proyecto](./governance/project-es/index.md) | Épicas MVP, historias de usuario, orden de prioridad, fases y línea de corte. |
| 📋 **Requisitos** | [Índice de Requisitos](./governance/requirements-es/index.md) | Historias funcionales, glosario de negocio, modelo de permisos y modelo conceptual. |
| 🏗️ **Arquitectura** | [Portal de Arquitectura](./architecture/index.es.md) | Stack, registro ADR, especificación C4, contextos delimitados y habilitadores técnicos. |
| 🛠️ **Infraestructura** | [Infraestructura](./infrastructure/index.md) | Docker, Kong, Kubernetes y configuración de ambientes. |
| 🚀 **Operaciones** | [Portal de Operaciones](./operations/index.md) | Runbooks, observabilidad, operación SQL y prácticas SRE. |
| 🎓 **Conocimiento** | [Base de Conocimiento](./knowledge/index.es.md) | Rutas de lectura recomendadas, POCs, investigación y onboarding. |

---

## 👥 Lectura Recomendada por Rol
| Perfil | Objetivo de Lectura | Ruta de Aprendizaje (Links Directos) |
| :--- | :--- | :--- |
| **Director / Ejecutivo** | Valor de negocio, alcance MVP y confianza de entrega | [Visión de Producto](./governance/product-es/product-vision.md) → [Contexto de Negocio](./governance/product-es/business-context.md) → [Priorización MVP](./governance/roadmap/mvp-functional-prioritization.es.md) → [Backlog de Producto MVP](./governance/project-es/mvp-product-backlog.md) |
| **Product Owner** | Alcance funcional, secuencia y propiedad del backlog | [Requisitos](./governance/requirements-es/index.md) → [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) → [Priorización MVP](./governance/roadmap/mvp-functional-prioritization.es.md) → [Backlog de Producto](./governance/project-es/mvp-product-backlog.md) |
| **Analista de Negocio** | Narrativa de negocio, reglas, criterios de aceptación y trazabilidad | [Estándar de Historias Funcionales](./governance/requirements-es/functional-stories/functional-story-standard.md) → [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) → [Glosario](./governance/requirements-es/glossary.md) → [Modelo Conceptual](./governance/requirements-es/conceptual-data-model.md) |
| **Arquitecto de SW** | Diseño técnico, decisiones y límites de dominio | [Portal de Arquitectura](./architecture/index.es.md) → [Especificación C4](./architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./architecture/adrs-es/index.md) → [Mapa de Contextos](./architecture/blueprints-es/bounded-context-map.md) |
| **Developer (BE/FE)** | Qué construir, por qué importa y cómo debe integrarse | [Backlog de Producto](./governance/project-es/mvp-product-backlog.md) → [Historias Funcionales](./governance/requirements-es/functional-stories/index.md) → [Habilitadores Técnicos](./architecture/blueprints-es/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Ambientes, confiabilidad, observabilidad y soporte | [Infraestructura](./infrastructure/index.md) → [Portal de Operaciones](./operations/index.md) → [Runbooks](./operations/runbooks/index.md) → [Estrategia de Observabilidad](./architecture/artifacts-es/observability-strategy.md) |
| **QA / Seguridad** | Quality gates, riesgos de acceso y estrategia de verificación | [Plan de Pruebas de Contrato](./architecture/artifacts-es/contract-testing-plan.md) → [Especificación IAM Empresarial](./architecture/artifacts-es/enterprise-iam-ums-specification.md) → [Modelo de Madurez Arquitectónica](./architecture/artifacts-es/architecture-maturity-model.md) |
| **IA / Agentes** | BMAD-METHOD, reglas del repositorio y contexto de automatización | [Reglas de Agentes](./AGENTS.md) → [Base de Conocimiento](./knowledge/index.es.md) → [Auditoría BMAD](./architecture/artifacts-es/bmad-master-audit-alignment-report.md) → [Auditoría Taxonomía](./governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## 🤝 Contribución y Gobernanza
- **Flujo**: Este repo utiliza [BMAD-METHOD](./AGENTS.md) para documentación dirigida por especificaciones.
- **Navegación**: Visita el [**Índice Maestro**](./MASTER_INDEX.es.md) para ver el árbol completo de documentos.
