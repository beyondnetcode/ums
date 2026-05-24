# From a Simple nginx Question to a Governed Architecture Decision

*How one infrastructure question revealed the right way to manage architectural decisions across repositories.*

---

A few days ago I was reviewing the infrastructure of a SaaS product I'm building — a User Management System (UMS) — when a simple question came up:

> "I don't see nginx details in the source code. Is it being configured at the Docker level?"

That question, which seemed trivial at first, ended up triggering a chain of architectural decisions that I want to share — because the way we answered it says a lot about how mature teams should handle technical evolution.

---

## The Starting Point: nginx Embedded in the Frontend

The UMS web application uses a multi-stage Docker build. The final stage takes the compiled React assets and serves them through nginx. The `nginx.conf` file lives alongside the frontend code:

```
ums/src/apps/ums.web-app/
├── nginx.conf          ← security headers, reverse proxy to API
├── Dockerfile          ← copies nginx.conf into the image
└── dist/               ← compiled React app
```

The configuration handles two responsibilities:

1. **Static file serving** — the React SPA files
2. **Reverse proxy** — routing `/api/**` and `/graphql` requests to the .NET backend

```nginx
server {
    listen 80;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header Content-Security-Policy "..." always;
    add_header Strict-Transport-Security "..." always;

    # Proxy to backend
    location /api/ {
        proxy_pass http://ums-api:8080;
    }

    # SPA fallback
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
}
```

**Is this correct?** Yes — for a single client. The configuration is versioned alongside the code that it serves, security headers are declared explicitly, and the multi-stage Docker build is the right pattern.

---

## The Moment the Question Changes

When I mentioned that UMS is planned as a SaaS product with at least a web app and a mobile app in the near future, the entire framing shifted.

With a single client, embedded nginx is cohesive and simple. With multiple clients:

- Security headers are either duplicated per client or absent from some.
- The mobile app bypasses nginx entirely — no centralized enforcement.
- Rate limiting, tenant routing, and auth token validation cannot be shared.
- Any policy change requires rebuilding the frontend container.

nginx was designed to serve files and proxy traffic — not to be an API Gateway for a multi-client SaaS.

---

## The Right Evolution: YARP as a Centralized API Gateway

UMS already had `Yarp.ReverseProxy` declared as a dependency. YARP (Yet Another Reverse Proxy) is Microsoft's reverse proxy library for ASP.NET Core — and it's exactly what a .NET-native team should use when they need a gateway.

The target architecture:

```
Internet
    │
    ▼
┌──────────────────────────────┐
│  ums.gateway  (YARP)         │  ← single entry point
│  Security headers            │
│  Rate limiting per tenant    │
│  Tenant routing              │
└────────────┬─────────────────┘
             │
    ┌────────┴──────────┐
    ▼                   ▼
┌──────────┐      ┌──────────────┐
│ ums.api  │      │ ums.web-app  │  ← nginx: static files only
│ :8080    │      │ :80          │
└──────────┘      └──────────────┘
                        ▲
                  ums.mobile-app
                  (future client)
```

nginx becomes a pure static file server — which is what it's best at. The gateway, written in C#, shares the existing middleware pipeline, OpenTelemetry instrumentation, and tenant context natively.

**But here's the important part: we didn't implement this immediately.**

The decision was documented as a *proposed* ADR (Architectural Decision Record) with an explicit trigger:

> *Do not implement until `ums.mobile-app` is initiated or multi-client routing becomes a confirmed product requirement.*

This is not procrastination. This is avoiding premature complexity. The current nginx approach is fully valid for MVP with a single web client. Building the gateway before the second client exists is speculation.

---

## The Second Question: Should We Split the API into Tiers?

While reviewing the gateway decision, a related question came up:

> "Should queries and commands live in separate API tiers? One service for GraphQL reads, another for REST writes?"

UMS already separates them at the protocol level — GraphQL for queries, REST for commands. The question was whether that separation should become a deployment separation.

The answer was no — and the reasoning matters:

**CQRS separates read and write *models*, not *deployment units*.**

The separation already exists at three levels:

| Level | Separation |
|---|---|
| Protocol | GraphQL (queries) vs REST (commands) |
| Code | Distinct handlers, distinct HTTP clients |
| Routing | `/graphql` vs `/api/v1/...` |

Splitting into deployment tiers would double operational cost — two Dockerfiles, two health checks, two scaling policies, two connection pools — with no measurable benefit at current load.

For a SaaS product, there's one additional consideration: in a multi-tenant environment, heavy GraphQL queries from a large tenant could impact command latency (login, provisioning). This is real. But the right mitigation at MVP scale is:

1. GraphQL query complexity limits at the schema level
2. Differentiated timeouts per operation type
3. Per-tenant rate limiting at the gateway layer

Tier separation is the *escalation path* — not the starting point. This decision was documented in ADR-0059 with explicit triggers for when it should be revisited.

---

## The Governance Pattern Behind the Decisions

Here's where the story gets more interesting for teams managing multiple repositories.

UMS is built on top of a corporate architecture reference — [Evolith](https://github.com/beyondnetcode/evolith_arch32) — which defines baseline decisions for all products in the organization: naming conventions, infrastructure patterns, API gateway strategy, event bus design, and more.

UMS inherits this baseline. But UMS also makes its own decisions that diverge from it. The question is: **how do you manage that divergence without losing the reference?**

The answer is a three-mode inheritance model:

| Mode | When to use | UMS Example |
|---|---|---|
| **Adopt** | Base policy applies as-is | ADR-0050: Naming taxonomy adopted verbatim |
| **Extend** | Base policy applies but needs domain constraints | ADR-0052: Immutable audit trail extended with SQL Server specifics |
| **Override** | UMS diverges with explicit justification | ADR-0059: Single API tier — CQRS at protocol, not deployment level |

Every override must answer three questions:

1. **Why** does the base decision not apply here?
2. **What** is the alternative with its own trade-off analysis?
3. **When** should this decision be revisited?

That third question is often skipped — and it's the most important one. An override without a revert trigger becomes invisible debt. Future developers assume it was the intended path, not a deliberate divergence that should be re-evaluated.

---

## What This Looks Like in Practice

The decisions from this session produced four artifacts, all versioned in the repository:

**ADR-0058** — Documents the decision to evolve toward a YARP gateway, with status *Proposed* and an explicit implementation trigger.

**ADR-0059** — Documents why UMS uses a single API tier, with the full trade-off analysis and triggers for when the decision should be revisited.

**TE-07** — A Technical Enabler blueprint with the full migration path: step by step, risk level per step, and the simplified nginx configuration that results.

**Architecture Portal update** — The governance model (Adopt / Extend / Override) is documented in the Architecture Portal so every new developer understands the inheritance contract on day one.

And in the Evolith base repository, a new section was added to the Child Repository Inheritance Guide using UMS as the reference implementation — so future teams bootstrapping satellite products have a real example to follow, not just theory.

---

## Key Takeaways

**For CTOs and Tech Leads:**

- Architectural decisions should be documented with the *trigger to change them*, not just the rationale for making them. An undated, un-triggered decision becomes invisible debt.
- The right time to introduce infrastructure complexity (gateways, tier splits) is when a specific, concrete requirement demands it — not when you can imagine a future that might need it.
- If you maintain a corporate architecture baseline, define explicit modes for how products can diverge from it. Silence is not adoption — it's risk.

**For Senior Developers and Architects:**

- CQRS does not imply separate deployment units. Protocol-level separation (GraphQL/REST) is a valid and often sufficient form of CQRS separation.
- YARP is the right choice for a .NET-native team needing a programmable gateway — it integrates natively with the middleware pipeline, DI container, and OpenTelemetry.
- nginx inside a frontend container is a legitimate pattern for single-client applications. The moment you have multiple clients, centralize the gateway.
- Per-tenant rate limiting in a SaaS gateway is not optional infrastructure — it's the first line of defense against noisy-neighbor problems at scale.

---

## The Repository

UMS and Evolith are open source. The ADRs, Technical Enablers, and governance documents referenced in this article are all publicly available:

- **UMS:** https://github.com/beyondnetcode/ums
- **Evolith base:** https://github.com/beyondnetcode/evolith_arch32

---

*Have you dealt with similar decisions in your own stack? How do you manage architectural divergence between a corporate baseline and a product team's local decisions? I'd be interested to hear your approach.*
