# 🏢 UMS Monorepo Workspace (`src/ums-workspace/`)

Welcome to the core monorepo workspace for the **User Management System (UMS)**. This workspace is orchestrated via **Nx** and **npm Workspaces** for the frontend, residing alongside the C# **.NET 8 LTS** backend solution.

> 🌎 **Bilingual Documentation Portals:**
> - 🇺🇸 **[English Knowledge Base](./docs/en/index.md)**
> - 🇪🇸 **[Base de Conocimientos en Español](./docs/es/index.md)**

---

## ⚡ 1. Target Technology Stack

Following the corporate **bMAD Method**, UMS enforces strict segregation between frontend capabilities and a highly-performant backend kernel:
- **Frontend:** React (v18+, Latest Stable), Vite, Zustand, TanStack Query.
- **Backend Core:** .NET 8 LTS, ASP.NET Core Minimal APIs, MediatR, FluentValidation.
- **Data Layer:** PostgreSQL 16, Entity Framework Core (EF Core via Npgsql).
- **Tooling & Quality:** Nx, npm Workspaces, CSharpier, ESLint, CodeQL.

For a detailed breakdown of the migration roadmap, please consult the **[.NET Migration & Tech Stack Plan](./docs/en/02-architecture/dotnet-migration-and-tech-stack-plan.md)**.

---

## 🚀 2. Quick Start & Commands

All workspace actions should be initiated using standard terminal shells inside `./src/ums-workspace/`:

### 🎨 Frontend Execution
```bash
# 1. Install NPM dependencies
npm install

# 2. Start the React Enterprise Portal (Vite development server)
npx nx run apps-web:dev
```

### ⚙️ Backend Execution
Navigate to `./src/ums-workspace/apps/api-dotnet/` (or open `Ums.sln` in your IDE):
```bash
# 1. Restore dependencies and build the solution
dotnet build

# 2. Run unit and integration test suite
dotnet test

# 3. Start the Web API Presentation layer
dotnet run --project apps/api-dotnet/Ums.Presentation
```

---

## 📖 3. Canonical Reading Path

To guarantee compliance with corporate engineering standards, do not wander through directories. Please refer to the **Root Portal Indices**:
1. 👉 **[Global Master Index](../../MASTER_INDEX.md)**: Accelerated reading paths customized by engineering role.
2. 👉 **[Root README](../../README.md)**: Primary repository introduction, disclaimers, and developer harnesses.
3. 👉 **[AGENTS.md Developer Harness](../../AGENTS.md)**: Mandatory instructions for LLMs and AI-augmented coding assistants.
