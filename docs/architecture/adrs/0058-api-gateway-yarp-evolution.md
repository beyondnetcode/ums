# ADR-0058: API Gateway Evolution — YARP for Multi-Client SaaS

## Status

Proposed

## Date

2026-05-22

## Context

UMS currently uses nginx embedded inside the `ums.web-app` Docker image as both a static file server and a reverse proxy to the backend API. This approach is cohesive and sufficient for a single web client.

The planned SaaS evolution introduces at minimum two distinct client surfaces:

- `ums.web-app` — browser SPA (React + Vite)
- `ums.mobile-app` — native mobile application (future)

With multiple clients, the embedded nginx reverse proxy becomes a liability:

- Security headers (CSP, HSTS, X-Frame-Options) are duplicated or absent per client.
- Rate limiting, authentication token validation, and tenant routing cannot be centralized.
- Mobile clients bypass nginx entirely, losing cross-cutting enforcement.
- Any change to routing or security policy requires rebuilding the frontend container.

UMS already has `Yarp.ReverseProxy` as a declared dependency in `Ums.Presentation`, and the existing `IAuthenticationPort` abstraction is gateway-compatible. The technology is already present in the stack.

---

## Decision

**Introduce `ums.gateway` as a dedicated ASP.NET Core application using YARP as the centralized API Gateway for all UMS clients.**

The gateway will be the single entry point for all inbound traffic. nginx will be reduced to a pure static file server with no proxy responsibility.

### Scope of Responsibilities

| Concern | Current Owner | Target Owner |
|---|---|---|
| Static file serving (SPA) | nginx | nginx (unchanged) |
| Reverse proxy to API | nginx (per client) | YARP gateway (centralized) |
| Security headers | nginx.conf | YARP middleware |
| Rate limiting | Not implemented | YARP + `IPartitionedRateLimiter` |
| Tenant routing | Not implemented | YARP gateway |
| Mobile API access | Direct to API | YARP gateway |
| Auth token validation | API layer | YARP gateway (pre-routing) |

### Target Architecture

```
Internet
    │
    ▼
┌──────────────────────────────┐
│  ums.gateway  (YARP)         │  port 443 / 80
│  ASP.NET Core                │  Security headers, Rate limit,
│                              │  Tenant routing, Auth check
└────────────┬─────────────────┘
             │
    ┌────────┴──────────┐
    ▼                   ▼
┌──────────┐      ┌──────────────┐
│ ums.api  │      │ ums.web-app  │  nginx: static only
│ :8080    │      │ :80          │  no proxy logic
└──────────┘      └──────────────┘
                        ▲
                  ums.mobile-app
                  (future client,
                   routes via gateway)
```

### What nginx.conf Becomes

```nginx
server {
    listen 80;
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
}
```

All security headers, CSP policies, and proxy configuration migrate to the YARP gateway.

---

## Trigger for Implementation

This ADR is **Proposed** and should not be implemented until the mobile client development begins. The current nginx-embedded approach remains valid for the MVP with a single web client.

Implementation is triggered when **any of the following** occurs:

1. `ums.mobile-app` project is created.
2. A second consumer of the API requires independent routing or security policy.
3. Rate limiting or tenant-aware routing becomes a product requirement.

---

## Consequences

### Positive

- Single enforcement point for security headers, rate limiting, and tenant routing across all clients.
- Mobile and web clients share the same gateway contract without duplicating configuration.
- Gateway is written in C#, co-located with the domain model, and shares the OpenTelemetry instrumentation already in place (ADR-0053).
- `IPartitionedRateLimiter` integrates natively with UMS tenant context, enabling per-tenant rate limiting with no external tooling.
- nginx becomes stateless and replaceable — static hosting could move to a CDN without touching the API layer.

### Negative

- Adds one deployable unit (`ums.gateway`) to the Docker Compose and future Kubernetes manifests.
- Requires migration of nginx security headers into YARP middleware (one-time effort).
- Gateway becomes a single point of failure if not scaled or health-checked properly.

### Neutral

- No domain model changes required.
- `Yarp.ReverseProxy` dependency already present in `Ums.Presentation` — it should be moved to the new gateway project.

---

## References

- [TE-07: YARP API Gateway — Implementation Blueprint](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)
- [ADR-0053: OpenTelemetry Observability Strategy](./0053-opentelemetry-observability.md)
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md)
- Current nginx config: `ums/src/apps/ums.web-app/nginx.conf`

---

**[ADR Registry](./index.md)** | **[Architecture Portal](../index.md)**
