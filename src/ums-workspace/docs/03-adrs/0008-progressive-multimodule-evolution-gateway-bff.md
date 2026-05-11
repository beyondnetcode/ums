# ADR 0008: Progressive Multi-Module Evolution with API Gateway and BFF Patterns

## Status
Accepted

## Date
2026-05-08

## Context
Currently, the UMS (User Management System) repository serves as an enterprise-grade reference architecture for 100% Node.js-based applications, operating as a single-solution modular monolith (with React frontend and NestJS backend). 

However, UMS is intended to scale into a unified gateway. In the future, the frontend application architecture must be preserved, but it will serve as the single entry point for multiple corporate modules and systems (such as Transport Management System - TMS, Warehouse Management System - WMS, etc.). These upcoming systems will be exposed as independent, decoupled services—each governed by its own business context and database. 

To prevent tight coupling between the clients and multiple backends, and to accommodate diverse clients (such as Web Portals, Mobile Applications, or Third-Party B2B integrations), we need a robust, progressive architecture that guides this evolution while strictly adhering to the **Backend For Frontend (BFF)** and **API Gateway** patterns. 

Without a BFF layer, different clients (e.g., mobile apps with limited bandwidth vs. rich web dashboards) would be forced to consume the same generic backend endpoints, leading to over-fetching, high network latency, and rigid client-side state management.

## Decision
We decided to adopt a **Progressive Multi-Module and Distributed Backend For Frontend (BFF) Gateway Architecture** for future phases:

1. **Frontend Preservation**: The React architecture (Zustand + TanStack Query) will remain unified as the central user portal, progressively lazy-loading modules (TMS, WMS, UMS) to optimize bundle size and performance.
2. **Exposed Independent Services**: Any new system (e.g., TMS, WMS) will be developed as a separate, decoupled backend service with its own isolated database (Database-per-Service pattern), adhering to bounded contexts.
3. **Backend For Frontend (BFF) Pattern**: Instead of exposing downstream services directly to the public web or using a single monolithic gateway, we will implement dedicated **BFF Gateways** tailored for each specific client type:
   * **Web BFF Gateway**: Optimized specifically for the React Web Portal. It aggregates calls to UMS, TMS, and WMS, formats large data payloads suitable for high-speed desktop browsers, and handles cookie-based secure sessions.
   * **Mobile BFF Gateway**: Optimized specifically for Mobile Applications (iOS/Android). It compresses payloads, combines multiple downstream HTTP requests into single roundtrips to reduce mobile network latency, translates protocols (e.g., HTTP/JSON to gRPC downstream), and caches mobile-specific resources.
   * **B2B API Gateway**: A separate ingress gateway for external clients or third-party integrations, enforcing strict rate-limiting, API keys, and contract-based schemas.
4. **Downstream Isolation**: Public clients never communicate directly with downstream services (UMS, TMS, WMS). All client traffic is routed through their respective BFFs, which act as the security boundary, session manager, and API composer.

### 🏛️ Progressive System Architecture Diagram

```
+------------------+     +--------------------+     +-------------------+
|  React Web App   |     | Mobile Client App  |     |  External B2B IP  |
|  (Desktop Web)   |     |   (iOS/Android)    |     |   (Third-Party)   |
+--------+---------+     +---------+----------+     +---------+---------+
         |                         |                          |
         | HTTP / HTTPS            | HTTP2 / JSON             | HTTPS / API Key
         v                         v                          v
+--------+---------+     +---------+----------+     +---------+---------+
|  Web BFF Gateway |     | Mobile BFF Gateway |     |  B2B API Gateway  |
|  (Express/Nest)  |     |   (Payload Comp.)  |     |  (Rate Limiter)   |
+--------+---------+     +---------+----------+     +---------+---------+
         |                         |                          |
         +-------------+-----------+--------------------------+
                       |
                       | Internal gRPC / High-Speed TCP
                       v
         +-------------+-------------+-------------+
         |                           |             |
         v                           v             v
+--------+--------+           +------+------+  +--------+--------+
|   UMS Service   |           | TMS Service |  |   WMS Service   |
| (PostgreSQL DB) |           | (Isolated)  |  | (Isolated DB)   |
+-----------------+           +-------------+  +-----------------+
```

## Consequences

### Positive (Pros)
* **Tailored Client Performance (BFF)**: Mobile apps suffer zero latency or over-fetching issues, as the Mobile BFF only returns the minimal payload required for mobile screens.
* **Independent Scalability**: The Web BFF and Mobile BFF can be scaled, updated, and deployed independently according to the traffic patterns of each platform (e.g., higher scaling of Mobile BFF during peak mobile usage).
* **Decoupled Contracts**: Downstream services (UMS, TMS, WMS) can change their internal APIs without breaking the public client apps, as the BFF layers act as an anti-corruption layer mapping the contracts.
* **Security & Centralization**: BFFs manage specific security protocols per client (e.g., HTTPOnly secure cookies for Web vs. OAuth2 Bearer Tokens/Keychain for Mobile).

### Negative (Cons)
* **Gateway Proliferation**: Managing multiple BFF codebases (Web BFF, Mobile BFF) increases development overhead.
* **Deployment Orchestration**: Requires robust CI/CD pipelines to manage deployment of multiple gateways alongside downstream microservices (easily handled via Nx monorepo configurations).
