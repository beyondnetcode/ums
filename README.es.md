# UMS — Sistema de Gestión de Usuarios Empresarial

> **Monolito Modular de alta escala para Gestión de Identidad y Autorización Unificada.**
>
> ![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito_Modular-blue) ![Lenguaje](https://img.shields.io/badge/Lenguaje-.NET_8_/_React-informational) ![Metodología](https://img.shields.io/badge/Metodología-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- 🇺🇸 [English](./README.md)
- 🇪🇸 [Español](./README.es.md)

---

## 🚀 Inicio Rápido
Para desarrolladores listos para levantar el entorno:
```powershell
# Entrar al Engine Room
cd src
# Iniciar Frontend
npm install; npx nx run app-web:dev
# Compilar Backend
dotnet build ./apps/app-api-dotnet/Ums.sln
```

---

## 🏛️ Arquitectura y Principios
Construido bajo **Clean Architecture**, **DDD** y patrones **Hexagonales**.
- **Patrón**: Monolito Modular/Progresivo con Contextos Delimitados estrictos.
- **Persistencia**: PostgreSQL 16 con Seguridad a Nivel de Fila (RLS).
- **Seguridad**: OAuth2/OIDC + Grafo de Autorización Multi-tenant.

---

## 📍 Hub de Documentación Global

| Dominio | Descripción | Contenido |
| :--- | :--- | :--- |
| [⚖️ **Gobernanza**](./governance/) | Estrategia de Negocio y Producto | Visión, Roadmap, Requisitos (Fases 00, 01, 05). |
| [🏗️ **Arquitectura**](./architecture/) | Planos Técnicos | ADRs, Modelos C4, Estándares de Ingeniería (Fases 02, 03, 04). |
| [🛠️ **Infraestructura**](./infrastructure/) | Plataforma e IaC | Docker, Kong Gateway, configs de Kubernetes. |
| [🚀 **Operaciones**](./operations/) | Monitoreo y SRE | Observabilidad (OTel/Tempo), Grafana Dashboards, SQL init. |
| [🎓 **Conocimiento**](./knowledge/) | Centro de Aprendizaje | POCs, Guías de Onboarding, Investigación de referencia. |
| [💻 **Código Fuente**](./src/) | Implementación | Código Fuente del Producto (`apps/` y `libs/`). |

---

## 👥 Lectura Recomendada por Rol
Selecciona tu perfil para una ruta de onboarding personalizada:

<details>
<summary><b>📦 Product Owner / Negocio</b></summary>

1. [Visión del Producto](./governance/product/product-vision.md)
2. [Contexto de Negocio y Solución](./governance/product/business-context.md)
3. [Historias Funcionales](./governance/requirements/functional-stories/)
4. [Roadmap del Producto](./governance/roadmap/versioning-and-audit-strategy.md)
</details>

<details>
<summary><b>🏗️ Arquitecto de Software</b></summary>

1. [Espec. de Arquitectura (Modelos C4)](./architecture/blueprints-es/architecture-spec.md)
2. [Registro de ADRs](./architecture/adrs-es/)
3. [Stack Tecnológico](./architecture/blueprints-es/stack.md)
4. [Mapa de Contextos](./architecture/blueprints-es/bounded-context-map.md)
</details>

<details>
<summary><b>⚙️ Desarrollador Backend</b></summary>

1. [Estándares de Ingeniería](./architecture/artifacts-es/engineering-standards.md)
2. [Código Fuente Backend](./src/apps/app-api-dotnet/)
3. [Habilitadores Técnicos](./architecture/blueprints-es/technical-enablers/)
4. [Plan de Migración .NET](./architecture/blueprints-es/dotnet-migration-and-tech-stack-plan.md)
</details>

<details>
<summary><b>💻 Desarrollador Frontend</b></summary>

1. [Código Fuente Frontend](./src/apps/app-web/)
2. [Alcance del Producto (Web Console)](./architecture/artifacts-es/ums-web-console-product-scope.md)
3. [Estándares de Ingeniería](./architecture/artifacts-es/engineering-standards.md)
</details>

<details>
<summary><b>☁️ DevOps / SRE</b></summary>

1. [Configuración de Infraestructura](./infrastructure/)
2. [Estrategia de Observabilidad](./architecture/artifacts-es/observability-strategy.md)
3. [Activos Operacionales](./operations/)
4. [Guía de Configuración de Kong](./architecture/artifacts-es/kong-plugins-configuration-guide.md)
</details>

<details>
<summary><b>🛡️ Seguridad / QA</b></summary>

1. [Espec. IAM Empresarial](./architecture/artifacts-es/enterprise-iam-ums-specification.md)
2. [Plan de Contract Testing](./architecture/artifacts-es/contract-testing-plan.md)
3. [Espec. MFA y Seguridad](./architecture/artifacts-es/mfa-passwordless-security-spec.md)
4. [Gobernanza Multi-Tenant](./architecture/artifacts-es/enterprise-multitenant-governance-report.md)
</details>

---

## 🤝 Contribuir
Por favor lee el [**Indice Maestro**](./MASTER_INDEX.es.md) y las [**Reglas de Agentes**](./AGENTS.md) antes de realizar cambios. Este proyecto sigue el **BMAD-METHOD** para desarrollo dirigido por especificaciones.
