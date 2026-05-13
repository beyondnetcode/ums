# 🏢 Workspace del Monorepo UMS (`src/ums-workspace/`)

Bienvenido al workspace central del monorepo del **Sistema de Gestión de Usuarios (UMS)**. Este espacio está orquestado mediante **Nx** y **npm Workspaces** para el frontend, conviviendo junto a la solución backend corporativa en **.NET 8 LTS**.

> 🌎 **Portales de Documentación Bilingüe:**
> - 🇺🇸 **[Base de Conocimiento en Inglés](./docs/en/index.md)**
> - 🇪🇸 **[Base de Conocimiento en Español](./docs/es/index.md)**

---

## ⚡ 1. Stack Tecnológico Objetivo

Siguiendo la **estrategia spec-driven AI BMAD-METHOD** corporativo, UMS aplica una segregación estricta entre las capacidades de la interfaz de usuario y un núcleo backend de alto rendimiento:
- **Frontend:** React (v18+, Última Versión Estable), Vite, Zustand, TanStack Query.
- **Núcleo Backend:** .NET 8 LTS, ASP.NET Core Minimal APIs, MediatR, FluentValidation.
- **Capa de Datos:** PostgreSQL 16, Entity Framework Core (EF Core vía Npgsql).
- **Herramientas y Calidad:** Nx, npm Workspaces, CSharpier, ESLint, CodeQL.

Para obtener un desglose detallado del mapa de ruta de migración, consulte el **[Plan de Migración .NET y Stack Tecnológico](./docs/es/02-architecture/dotnet-migration-and-tech-stack-plan.md)**.

---

## 🚀 2. Inicio Rápido y Comandos

Todas las acciones del workspace deben iniciarse utilizando terminales estándar dentro de `./src/ums-workspace/`:

### 🎨 Ejecución del Frontend
```bash
# 1. Instalar dependencias NPM
npm install

# 2. Iniciar el Portal React Enterprise (Servidor de desarrollo Vite)
npx nx run apps-web:dev
```

### ⚙️ Ejecución del Backend
Navegue a `./src/ums-workspace/apps/api-dotnet/` (o abra la solución `Ums.sln` en su IDE):
```bash
# 1. Restaurar dependencias y compilar la solución
dotnet build

# 2. Ejecutar suite de pruebas unitarias e integración
dotnet test

# 3. Iniciar la capa de Presentación de Web API
dotnet run --project apps/api-dotnet/Ums.Presentation
```

---

## 📖 3. Ruta de Lectura Canónica

Para garantizar el cumplimiento con los estándares de ingeniería corporativos, no explore directorios aleatoriamente. Por favor, refiérase a los **Índices de los Portales Raíz**:
1. 👉 **[Índice Maestro Global](../../MASTER_INDEX.es.md)**: Rutas de lectura aceleradas personalizadas por perfil de ingeniería.
2. 👉 **[README Raíz](../../README.es.md)**: Introducción primaria del repositorio, disclaimers y guías para agentes.
3. 👉 **[AGENTS.md Developer Harness](../../AGENTS.md)**: Instrucciones obligatorias para LLMs y asistentes de codificación aumentados por IA.
