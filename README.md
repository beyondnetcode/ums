<img src="./governance/assets/header-banner.png" height="100" width="100%" alt="Software Architecture Banner" />

# UMS — Enterprise User Management System

> **High-scale Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Arch](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Engine](https://img.shields.io/badge/Engine-.NET_8_/_React-informational)

---

### 🚀 Navigation Hub
Explore the repository layers:

- [⚖️ **Governance**](./governance/) — Vision, Roadmap & Requirements.
- [🏗️ **Architecture**](./architecture/) — ADRs, Blueprints & C4 Models.
- [🛠️ **Infrastructure**](./infrastructure/) — Docker, K8s & Gateway.
- [🚀 **Operations**](./operations/) — Observability & Monitoring.
- [🎓 **Knowledge**](./knowledge/) — Onboarding & POCs.
- [💻 **Source Code**](./src/) — The Engine Room (Apps & Libs).

---

### 🏛️ Technical DNA
- **Patterns**: Modular Monolith, DDD, Clean Architecture, Hexagonal.
- **Backend**: .NET 8 LTS (Result Pattern, MediatR).
- **Frontend**: React 18 + Vite (State Management with Zustand).
- **Security**: Row-Level Security (RLS) + OAuth2/OIDC.

### 🚦 Quick Start
```powershell
# 1. Enter the Engine Room
cd src

# 2. Install & Start Frontend
npm install; npx nx run app-web:dev

# 3. Build Backend
dotnet build ./apps/app-api-dotnet/Ums.sln
```

### ⚖️ Engineering Governance
Detailed technical navigation is available in the [**Master Index**](./MASTER_INDEX.md). This project follows the **BMAD-METHOD** for AI-augmented documentation traceability.
