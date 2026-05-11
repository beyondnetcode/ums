# 👥 Project Stakeholders & Users - User Management System (UMS)

To ensure the success of UMS, the following internal and external **Stakeholders** are mapped with their respective roles, responsibilities, and core expectations:

---

## 🏢 1. Internal Stakeholders

| Stakeholder Role | Primary Responsibility | Core Expectation from UMS |
| :--- | :--- | :--- |
| **Principal Software Architect** | Architectural design, Hexagonal boundary governance, security, and Dapr microservices readiness. | High decoupling, clean interfaces (Ports), Zero-tolerance vulnerability, and perfect C4 model alignment. |
| **Product Owner / Business Analyst** | Requirements collection, scope definition, feature prioritizing, and user stories. | Self-service tenant onboarding, dynamic UI menu injection, and intuitive RBAC/ABAC assignment logic. |
| **Lead Developer** | Code implementation in NestJS, React, PostgreSQL integration, and unit testing. | Clear interfaces, Type Safety, excellent Nx task runner support, and well-documented API endpoints. |
| **QA / Security Analyst** | Contract testing, local penetration testing, coverage verification, and quality gates. | High code testability, Pact contract compliance, and immutable business audit logs. |
| **DevOps / SRE Engineer** | Infrastructure topology, Docker orchestration, and Grafana Loki telemetry pipelines. | Smooth CI/CD builds (< 5m), high observability (OpenTelemetry), and reliable PostgreSQL RLS isolation. |

---

## 🌐 2. External Users

| User Persona | Context | Key Benefit from UMS |
| :--- | :--- | :--- |
| **Client Tenant Admin** | IT Administrator at an integrated B2B client company (Tenant). | Complete self-service autonomy to manage employee profiles, roles, and authorization scopes without submitting support tickets. |
| **B2B End User** | Employee at a client company (e.g., forklift operator, freight planner). | Fast, frictionless passwordless login (Passkey/SSO) and a clean, dynamic portal displaying only their permitted applications. |

