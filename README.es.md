# 🏛️ Sistema de Gestión de Usuarios (UMS) - Monorepo Corporativo de Referencia

> 🌍 **Selector de Idioma:** [🇪🇸 Español](./README.es.md) | [🇺🇸 English](./README.md)

---

## 💡 1. Introducción y Objetivos Clave

Bienvenido al **Sistema de Gestión de Usuarios (UMS)**, un monolito modular empresarial altamente resiliente, diseñado para gestionar identidades corporativas, control de acceso y ciclos de vida de usuarios.

Esta solución es una implementación en .NET 8 LTS que sirve como una instancia concreta de la [Arquitectura de Referencia Políglota Corporativa Unificada (bMAD)](https://github.com/beyondnetcode/arc32_progresive_monolith). UMS implementa un diseño evolutivo "Primero el Monolito" utilizando el **Método BMAD**, permitiendo que los dominios de negocio se extraigan como microservicios independientes en el futuro (usando Dapr, gRPC o arquitecturas dirigidas por eventos) con cero reescritura de las reglas del núcleo de dominio.

### 🎯 Objetivos Primarios:
*   **Desacoplamiento Estricto y Arquitectura Limpia:** Diseño guiado por el dominio (DDD) utilizando principios hexagonales (Puertos y Adaptadores) para garantizar independencia absoluta de frameworks.
*   **Evolución Progresiva:** Diseñado dentro de un Nx Monorepo altamente desacoplado, facilitando una transición fluida hacia microservicios distribuidos cuando los KPIs técnicos así lo exijan.
*   **Aislamiento de Identidad Agnóstico:** Desacopla la autorización del núcleo de los Proveedores de Identidad comerciales (Auth0, Keycloak, Entra ID), eliminando el bloqueo del proveedor (vendor lock-in).
*   **Núcleo de Autorización Zero-Trust:** Proporciona permisos contextuales de grano fino (RBAC/ABAC) y un compilador de grafos de autorización de alto rendimiento con latencias <5ms.

---

## 🧭 2. Hub Unificado de Navegación Maestro

🚀 **No explores los directorios al azar.** Todas las rutas técnicas y de cumplimiento están organizadas explícitamente por rol de usuario:

1.  👉 **[Índice Maestro Global (ES)](./MASTER_INDEX.es.md)**: La puerta de entrada canónica. Encuentra tu ruta de lectura obligatoria según tu perfil (Proveedor, Desarrollador, Arquitecto, Project Manager).
2.  🏛️ **[Documentación Maestra bMAD](./src/ums-workspace/docs/index.es.md)**: Índice unificado central que mapea desde la Fase 00 hasta la Fase 05.
3.  📜 **[Hub de Registro ADR](./src/ums-workspace/docs/index.es.md#📜-phase-03---registros-de-decisión-arquitectónica-adrs)**: Acceso directo a los 29 Registros de Decisiones Arquitectónicas (ADR) activas.

---

## ⚠️ 3. Disclaimers Críticos y Recomendaciones de Uso

Para interactuar de forma segura con el código base de UMS, todos los desarrolladores DEBEN respetar las siguientes directrices:

### 🛑 Disclaimers Importantes:
*   **Agnóstico de Proveedores de Auth:** Diseñado para funcionar tanto como un bloque de autorización autónomo como en tándem con proveedores de identidad corporativos existentes.
*   **Instancia de Referencia:** Sirve como la base de referencia corporativa para aplicaciones modulares .NET/C#. Prioriza la limpieza demostrativa sobre micro-eficiencias densas y optimizadas.

### ✅ Recomendaciones de Uso Cruciales:
1.  **Nunca Saltarse los Puertos:** Nunca inyectes dependencias de frameworks externos (controladores ASP.NET Core), Bases de Datos (EF Core) o librerías de terceros directamente dentro de las carpetas `/domain`.
2.  **Sincronización con la Base ADR:** Cualquier desviación del diseño técnico exige la consulta del registro histórico de decisiones arquitectónicas autorizadas antes de su implementación.
3.  **Adoptar Docs-as-Code:** Mantén actualizados los mapas de documentación siguiendo estrictamente las **fases numéricas bMAD** (00-Producto, 01-Requisitos, 02-Arquitectura, 03-ADRs, 04-Artefactos, 05-Roadmap).

---

## ⚡ 4. Arquitectura de Alto Nivel y Mapa Rápido del Ecosistema

### 🚀 Stack Tecnológico (Horizonte 2026)

| Componente | Tecnología / Framework | Rol |
| :--- | :--- | :--- |
| **Núcleo Backend** | .NET 8 LTS / ASP.NET Core | Monolito Modular / API Transaccional |
| **Frontend** | React (v18) / Vite / Zustand / Query | Portal Corporativo Unificado |
| **Motor de Monorepo** | Nx / npm Workspaces / .NET SLN | Orquestación de Dependencias y Velocidad de Build |
| **Capa de Datos** | PostgreSQL 16 / EF Core | Identidades Persistentes y Grafo de Permisos |
| **Seguridad** | CodeQL / SonarJS / CSharpier / ESLint | Puertas de Calidad Automatizadas en CI/CD |

### 🛠️ Directorio de Accesos Rápidos
*   🏛️ **[Blueprint de Referencia Corporativa](https://github.com/beyondnetcode/arc32_progresive_monolith)**: Especificaciones corporativas oficiales para sistemas políglotas.
*   📊 **[Análisis de Brecha y Estrategia de Deuda](./src/ums-workspace/docs/04-artifacts/gap-analysis-and-optimization-plan.md)**: Evaluación de arquitectura frente a 16 criterios empresariales.
*   ⚙️ **[Estrategia de Observabilidad](./src/ums-workspace/docs/04-artifacts/observability-strategy.md)**: Telemetría de costo cero usando OpenTelemetry y Grafana Loki.
*   📈 **[Hoja de Ruta de Lanzamientos](./src/ums-workspace/docs/05-roadmap/versioning-and-audit-strategy.md)**: Estrategia automatizada de versionado semántico.

---

🤖 **Habilitación Aumentada por IA:** Este repositorio soporta nativamente agentes autónomos e IA asistida en el desarrollo, siguiendo el estándar de ingeniería BMAD Harness.
👉 **[Explorar Developer Harness en AGENTS.md](./AGENTS.md)**
