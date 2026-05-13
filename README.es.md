# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular estandarizado para Identidad y Autorización Unificada.**
>
> ![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- [English](./README.md) | **Español**

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
| ⚖️ **Gobernanza** | [Portal de Gobernanza](./governance/index.es.md) | Visión, Contexto, Roadmap y Auditorías. |
| 🏗️ **Arquitectura** | [Portal de Arquitectura](./architecture/index.es.md) | Registro ADR, Espec. C4 y Mapas de Contexto. |
| 📋 **Requisitos** | [Índice de Requisitos](./governance/requirements-es/index.md) | Historias de Usuario, Glosario y Modelos. |
| 🛠️ **Infraestructura** | [Infraestructura](./infrastructure/index.md) | Docker, Kong y Kubernetes. |
| 🚀 **Operaciones** | [Portal de Operaciones](./operations/index.md) | Observabilidad (OTel), SQL y SRE. |
| 🎓 **Conocimiento** | [Base de Conocimiento](./knowledge/index.md) | POCs, Investigación y Onboarding. |

---

## 👥 Lectura Recomendada por Rol
| Perfil | Objetivo de Lectura | Ruta de Aprendizaje (Links Directos) |
| :--- | :--- | :--- |
| **Director / Ejecutivo** | Estrategia y Valor de Negocio | [Visión](./governance/product-es/product-vision.md) → [Contexto](./governance/product-es/business-context.md) → [Roadmap](./governance/roadmap/index.md) |
| **Product Owner** | Requisitos y Alcance Funcional | [Requisitos](./governance/requirements-es/index.md) → [Historias](./governance/requirements-es/functional-stories/index.md) → [Glosario](./governance/requirements-es/glossary.md) |
| **Arquitecto de SW** | Diseño Técnico y Trazabilidad | [Espec. C4](./architecture/blueprints-es/architecture-spec.md) → [Registro ADR](./architecture/adrs-es/index.md) → [Mapa de Contextos](./architecture/blueprints-es/bounded-context-map.md) |
| **Developer (BE/FE)** | Patrones, Estándares y Código | [Estándares](./architecture/artifacts-es/engineering-standards.md) → [Historias](./governance/requirements-es/functional-stories/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Infra, Seguridad y Ops | [Infraestructura](./infrastructure/index.md) → [Portal de Ops](./operations/index.md) → [Espec. IAM](./architecture/artifacts-es/enterprise-iam-ums-specification.md) |
| **QA / Seguridad** | Calidad y Especificaciones | [Plan de Pruebas](./architecture/artifacts-es/contract-testing-plan.md) → [Espec. IAM](./architecture/artifacts-es/enterprise-iam-ums-specification.md) → [Madurez](./architecture/artifacts-es/architecture-maturity-model.md) |
| **IA / Agentes** | BMAD-METHOD y Automatización | [Reglas de Agentes](./AGENTS.md) → [Estándar BMAD](./architecture/artifacts-es/bmad-master-audit-alignment-report.md) → [Auditoría Taxonomía](./governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## 🤝 Contribución y Gobernanza
- **Flujo**: Este repo utiliza [BMAD-METHOD](./AGENTS.md) para documentación dirigida por especificaciones.
- **Navegación**: Visita el [**Índice Maestro**](./MASTER_INDEX.es.md) para ver el árbol completo de documentos.
