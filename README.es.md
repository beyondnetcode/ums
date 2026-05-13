# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular estandarizado para Identidad y Autorización Unificada.**
>
> ![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- [English](./README.md) | **Español**

---

## 🚀 Inicio Rápido (Motor Técnico)
¿Listo para compilar? Ejecuta esto desde el directorio `src/`:
```powershell
# Frontend: React 18 + Vite
npm install; npx nx run app-web:dev
# Backend: .NET 8 LTS
dotnet build ./apps/app-api-dotnet/Ums.sln
```

---

## 🗺️ Hub de Navegación de Conocimiento
Acceso directo a todos los dominios del proyecto sin navegar por carpetas.

| Dominio | Portal e Índice | Alcance |
| :--- | :--- | :--- |
| ⚖️ **Gobernanza** | [Portal de Gobernanza](./governance/index.es.md) | Visión de Negocio, Roadmap y Stakeholders. |
| 🏗️ **Arquitectura** | [Portal de Arquitectura](./architecture/index.es.md) | Registro de ADRs, Espec. C4 y Mapas de Contexto. |
| 📋 **Requisitos** | [Índice de Requisitos](./governance/requirements-es/index.md) | Glosario, Historias de Usuario y Modelos de Dominio. |
| 🛠️ **Infraestructura** | [Infraestructura](./infrastructure/index.md) | Docker, Kong Gateway y Config de K8s. |
| 🚀 **Operaciones** | [Portal de Operaciones](./operations/index.md) | Observabilidad (OTel), Monitoreo y SRE. |
| 🎓 **Conocimiento** | [Base de Conocimiento](./knowledge/index.md) | POCs, Investigación y Guías de Onboarding. |

---

## 👥 Lectura Recomendada por Rol
Rutas de onboarding personalizadas para maximizar el contexto y minimizar el ruido.

<details>
<summary><b>🏢 Ejecutivo / Director</b></summary>

- **Objetivo**: Entender el valor de negocio, visión e impacto estratégico.
1. [Visión del Producto](./governance/product-es/product-vision.md)
2. [Contexto de Negocio](./governance/product-es/business-context.md)
3. [Roadmap del Producto](./governance/roadmap/index.md)
</details>

<details>
<summary><b>📦 Product Owner / Analista</b></summary>

- **Objetivo**: Definir comportamiento, requisitos y límites funcionales.
1. [Índice de Requisitos](./governance/requirements-es/index.md)
2. [Registro de Historias de Usuario](./governance/requirements-es/functional-stories/index.md)
3. [Glosario (Lenguaje Ubicuo)](./governance/requirements-es/glossary.md)
</details>

<details>
<summary><b>🏗️ Arquitecto / Team Lead</b></summary>

- **Objetivo**: Asegurar coherencia técnica, escalabilidad y trazabilidad.
1. [Espec. de Arquitectura (Modelos C4)](./architecture/blueprints-es/architecture-spec.md)
2. [Registro de ADRs](./architecture/adrs-es/index.md)
3. [Mapa de Contextos](./architecture/blueprints-es/bounded-context-map.md)
4. [Estándares de Ingeniería](./architecture/artifacts-es/engineering-standards.md)
</details>

<details>
<summary><b>⚙️ Desarrolladores Backend y Frontend</b></summary>

- **Objetivo**: Implementación, calidad de código y patrones técnicos.
1. [Estándares de Ingeniería](./architecture/artifacts-es/engineering-standards.md)
2. [Stack Tecnológico](./architecture/blueprints-es/stack.md)
3. [Código Fuente (Engine Room)](./src/)
4. [Registro de Historias Funcionales](./governance/requirements-es/functional-stories/index.md)
</details>

<details>
<summary><b>☁️ DevOps / SRE / Seguridad</b></summary>

- **Objetivo**: Infraestructura, despliegue, seguridad y observabilidad.
1. [Configuración de Infraestructura](./infrastructure/index.md)
2. [Estrategia de Observabilidad](./architecture/artifacts-es/observability-strategy.md)
3. [Especificación IAM Empresarial](./architecture/artifacts-es/enterprise-iam-ums-specification.md)
4. [Portal de Operaciones](./operations/index.md)
</details>

<details>
<summary><b>🧪 QA / Automatización</b></summary>

- **Objetivo**: Estrategias de prueba, validación de contratos y gates de calidad.
1. [Plan de Contract Testing](./architecture/artifacts-es/contract-testing-plan.md)
2. [Modelo de Madurez Arquitectónica](./architecture/artifacts-es/architecture-maturity-model.md)
3. [Historias de Usuario (Criterios de Aceptación)](./governance/requirements-es/functional-stories/index.md)
</details>

<details>
<summary><b>🤖 Colaboradores IA y Automatización</b></summary>

- **Objetivo**: Colaboración eficiente con agentes IA usando BMAD-METHOD.
1. [Reglas de Agentes (AGENTS.md)](./AGENTS.md)
2. [Estándar BMAD-METHOD](./architecture/artifacts-es/bmad-master-audit-alignment-report.md)
3. [Guía de Taxonomía del Repositorio](./governance/audits/2026-05-13-taxonomy-normalization-audit.md)
</details>

---

## 🤝 Contribución y Gobernanza
- **Flujo**: Este repo utiliza [BMAD-METHOD](./AGENTS.md) para documentación dirigida por especificaciones.
- **Navegación Completa**: Visita el [**Índice Maestro**](./MASTER_INDEX.es.md) para ver el árbol completo de documentos.
