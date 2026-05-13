# ADR-0033: .NET API Endpoint Strategy (Minimal APIs vs. Controllers)

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Principal .NET Architect

---

## 🏛️ 1. Context and Problem
In .NET 8, 9, and leading into .NET 10, **Minimal APIs** have transitioned into a fully production-ready technology. Microsoft unequivocally positions them as the definitive future of ASP.NET Core, especially for cloud-native deployments.

Given UMS's current infrastructure, we must formalize clear technical governance criteria establishing when to adopt each model (Minimal APIs vs. traditional Controllers), preventing chaotic pattern sprawl and establishing strict corporate design and decoupling standards.

---

## 🎯 2. Architectural Decision
We adopt a **Hybrid Strategy** governed by the following explicit selection criteria:

### 🚀 2.1 When to Use Minimal APIs
It is the default and mandatory approach for:
1.  **New Microservices and Cloud-Native Services:** Where low memory footprint and low latency are critical.
2.  **Serverless and Event-Driven Services:** To optimize cold start times.
3.  **BFFs (Backend-for-Frontend):** Due to their direct nature and low coupling.
4.  **Modules Based on Vertical Slice Architecture (VSA):** Facilitating spatial cohesion of the feature flow.

### 📦 2.2 When to Maintain ASP.NET Controllers
Traditional controllers are permitted exclusively for:
1.  **Enterprise APIs with Complex Filter Logic:** Where the inherited hierarchy of `ActionFilters` provides real value.
2.  **Legacy Modules Under Active Maintenance:** To avoid mass rewrites and unnecessary operational risks.
3.  **Services with Advanced Model Binding:** That heavily rely on the classic MVC reflective engine.

---

## 🛠️ 3. Mandatory Standards for Minimal APIs
To prevent the "monolithic Program.cs" anti-pattern, every endpoint developed under Minimal APIs in the organization **MUST** strictly comply with:

*   **Isolated Handlers:** Complex inline lambdas are strictly prohibited. Handlers must be defined as static or pure class methods (Single Responsibility).
*   **Structure by Feature:** Mandatory use of `IEndpointRouteBuilder` extension methods segmented by functional module (*Feature Module*).
*   **Secure Grouping:** Employs `MapGroup` per resource for unified route prefixes, versioning injection, and shared security policies.
*   **Alignment with the Base SDK:** Mandatory consumption of the Corporate Base SDK that provides standardized helpers for endpoint registration, versioning policies, and observability telemetry.

---

## ⚖️ 4. Consequences and Trade-offs

### ✅ Benefits
*   **High Performance & AOT-Readiness:** Maximum execution speed and guaranteed Ahead-of-Time compilation compatibility for cloud resource optimization.
*   **Incremental Adoption:** Smooth and secure transition without forcing costly refactoring of current production code.

### ⚠️ Challenges and Mitigations
*   **Coexistence of Two Models:** Introduces initial navigation complexity. *Mitigation:* Explicit documentation will be provided in the Onboarding process along with official templates in the monorepo.
*   **Skill Gap in Team:** Developers must become proficient in both classic MVC Object-Oriented and Minimal API Functional flows.

---

## 🚀 5. Review and Evolution Roadmap
This decision shall be subject to a comprehensive technical review in **Q2 of the following year**. The goal will be to evaluate if Traditional Controllers can be officially marked as deprecated for the development of any new projects or services within the company.
