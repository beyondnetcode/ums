# 📜 ADR-0027 — Dual-Protocol REST & gRPC API Structure with Kong Gateway

**Status:** Accepted  
**Date:** 2026-05-09  
**Deciders:** Solutions Architect, Lead Developer, Principal Architect  
**ADR Type:** API Design & Gateway Layer  
**Related Specs:** [`stack.md`](../../02-architecture/stack.md), [`stack-summary.md`](../../02-architecture/stack-summary.md)

---

## 📋 Context

The User Management System (UMS) serves as the central identity, session, and authorization gatekeeper for multiple integrated SaaS platforms (such as SCM, ERP, and WMS). As an agnostically deployable security kernel, UMS must satisfy two competing API integration requirements:

1. **Universal Client Compatibility:** Public client systems, browser portals, and mobile frontends must integrate seamlessly with UMS using standard, universally accessible HTTP transport protocols with zero native SDK requirements.
2. **Sub-millisecond Internal Performance:** Internal service-to-service validation and permission graph compilation requests (e.g., BFF to UMS core) must complete with minimal network overhead to satisfy our high-performance SLA of **p95 < 5ms**.

Additionally, to support localized on-premise deployments, the gateway routing, rate-limiting, and load-balancing layers must remain fully self-hostable and cloud-agnostic, precluding the use of proprietary cloud API gateways (e.g., AWS API Gateway).

---

## ⚖️ Decision

We will implement a **Dual-Protocol API Architecture** combined with **Kong API Gateway (OSS)**:

1. **Public REST (JSON) Interface:** All public client portals, administrative consoles, and the UMS Hosted Login redirect endpoints will communicate via REST over HTTP/1.1 or HTTP/2, using standard JSON serialization and Swagger OpenAPI documentation.
2. **Internal gRPC (Protocol Buffers) Interface:** All high-performance inter-service validation calls, token verifications, and hierarchical authorization graph compilations will communicate via gRPC over HTTP/2.
3. **Kong Gateway Enforcement:** We will deploy Kong API Gateway (OSS) at the network edge. Kong will manage:
   - Client-facing SSL termination.
   - Sliding-window rate-limiting using a Redis-backed state engine.
   - Routing and path-based forwarding to our NestJS endpoints.

---

## 📐 Architecture Constraints

- **Single NestJS Monolith Deployment:** Both REST and gRPC endpoints are exposed by the same modular monolith application pods, eliminating the deployment overhead of separate gRPC servers.
- **Protocol Buffers as Single Source of Truth:** All shared internal service contracts are defined inside standard `.proto` files in the Nx monorepo (`libs/contracts`), from which TypeScript interfaces are generated automatically.
- **Self-Hostable Redis State:** Kong rate-limiting uses the same self-hosted Redis cluster allocated for application caching, avoiding separate key-store infrastructure.

---

## ✅ Consequences

### Positive
- **Optimized Internal Latency:** gRPC over HTTP/2 features multiplexing and binary serialization, reducing network overhead to a fraction of traditional JSON REST calls.
- **Universal Compatibility:** Public integration remains simple, clean, and easily auditable via standard REST APIs and interactive Swagger UI.
- **Zero Cloud Lock-in:** Both Kong Gateway and the Redis-backed rate limiter are fully open-source and self-hostable inside local Kubernetes clusters on-premise.

### Negative
- **Increased Setup Complexity:** Requires managing and compiling Protocol Buffers (`.proto` files) and maintaining dual controllers (REST and gRPC) inside the NestJS application.
- **Tooling Overhead:** Developers must utilize specialized tools (like Postman with gRPC support or `grpcurl`) to test internal gRPC endpoints locally.

---

## 🔗 ADR Impact Cross-References

| ADR | Impact |
| :--- | :--- |
| ADR-0002 (Clean Architecture)| Decoupled controllers are defined in the infrastructure adapters layer, keeping the core domain completely agnostic of both REST and gRPC decorators. |
| ADR-0021 (High-Performance Auth)| The high-concurrency `/api/v1/auth/login` endpoint is exposed via REST for clients, but triggers gRPC sub-calls internally, satisfying performance targets. |
