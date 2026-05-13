# 📜 ADR-0024 — Centralized Configuration & Feature Management Platform

**Status:** Accepted  
**Date:** 2026-05-09  
**Deciders:** Enterprise Architect, Product Owner, Lead Developer  
**ADR Type:** New Capability — Cross-Cutting Concern  
**Related Specs:** [`ums-configuration-platform-spec.md`](../../04-artifacts/ums-configuration-platform-spec.md)

---

## 📋 Context

The UMS serves as the central authorization kernel for a multi-tenant enterprise SaaS ecosystem. As the platform scales and onboards new B2B tenants and client systems, three recurring operational pain points have emerged:

1. **Static IdP Coupling**: Adding or switching Identity Providers requires code changes and redeployment, violating the zero-downtime operational mandate.
2. **Hardcoded System Behavior**: Runtime behaviors (MFA enforcement, session expiry, module toggles, branding) are either hardcoded or managed via environment variables — neither auditable nor tenant-specific.
3. **No Structured Feature Rollout**: Progressive feature delivery (Canary, Beta, A/B) has no centralized governance, causing fragmented flags scattered across client applications.

These gaps violate the UMS design principles of **vendor neutrality**, **zero-deployment governance**, and **centralized auditability**.

---

## ⚖️ Decision

We will introduce a **Configuration & Feature Management Context** as a first-class bounded context within the UMS platform. This context owns three cohesive capability domains:

### 1. Multi-IdP Configuration Engine
A dynamic registry of Identity Provider configurations (per tenant, per system, with priority/fallback rules) exposed via a secure, cached API (`/v1/config/idp`). Credentials are encrypted at rest using AES-256 with vault-based secret references. The Auth Gateway reads this configuration at login time, enabling zero-downtime IdP switching and hybrid authentication.

### 2. System Behavioral Configuration Model
A versioned, auditable, multi-tenant JSON configuration store (`/v1/config/system/{system_id}`) that governs runtime behavior: authentication strategies, session policies, MFA enforcement, onboarding flows, branding, and module enablement. Client systems consume this configuration at startup and on real-time push events.

### 3. Feature Flag Management Framework
A centralized flag engine supporting Boolean, Variant, and Percentage flag types with multi-dimensional targeting (tenant, org, branch, role, user, environment, system). Evaluated at runtime via `/v1/flags/evaluate`. Supports Canary, Beta, and A/B rollout strategies. Flags are cached in Redis for sub-3ms evaluation on cache hits.

---

## 📐 Architecture Constraints

- The new context is implemented as a **dedicated NestJS module** within the existing monorepo (not a separate service), following Hexagonal Architecture patterns.
- All config and flag entities are governed by **PostgreSQL RLS** using the active `tenant_id` context.
- Config mutations trigger **domain events** (`IdpConfigUpdatedEvent`, `SystemConfigPublishedEvent`, `FeatureFlagStateChangedEvent`) consumed by the Audit Context and Redis eviction hooks.
- The config API is read-optimized: Redis cache namespaces `cfg:*` (IdP + system config) and `flags:*` (evaluated flag sets) reduce DB load during high-concurrency read patterns.
- **Secret management**: OAuth client secrets, SAML certs, and LDAP credentials are **never** stored in plaintext. They are referenced via `config_secret_ref` pointing to an external vault (e.g., AWS Secrets Manager, HashiCorp Vault).

---

## ✅ Consequences

### Positive
- **Zero-Deployment IdP Switching**: Tenants can switch or add IdPs without redeployment.
- **Tenant-Personalized Runtime**: Each B2B client system behaves according to its configured parameters without shared code changes.
- **Controlled Feature Rollout**: Engineering teams can release features to 5% of users, validate metrics, and graduate rollouts — reducing incident risk.
- **Full Auditability**: Every configuration change and flag state transition is logged immutably.
- **Separation of Concerns**: Feature flags are no longer scattered across client applications.

### Negative
- **New cache namespace complexity**: Redis key space grows. Requires namespace governance (`cfg:*`, `flags:*`, `auth_graph:*`).
- **Secret vault dependency**: Introduces an optional but recommended external vault for production credential security.
- **Slight initial schema complexity**: Four new entity types (`IDP_CONFIGURATION`, `SYSTEM_CONFIGURATION`, `FEATURE_FLAG`, `FLAG_EVALUATION_LOG`) require migrations.

### Neutral
- Feature flag evaluation at p95 < 3ms (cache hit) does not affect the existing authorization graph SLA of p95 < 5ms.

---

## 🔗 ADR Impact Cross-References

| ADR | Impact |
| :--- | :--- |
| ADR-0010 (Multi-Tenancy) | No change — RLS tenant isolation applies automatically to new entities |
| ADR-0014 (Redis Caching) | Extended: new cache namespaces `cfg:*` and `flags:*` added to Redis governance |
| ADR-0016 (Immutable Audit) | Extended: config mutation subscribers added alongside existing permission subscribers |
| ADR-0017 (Feature Flagging) | **Superseded by this ADR** for the centralized flag engine design |
| ADR-0020 (IdP Abstraction) | **Extended**: Multi-IdP with priority/fallback model expands ADR-0020's single-IdP-per-tenant scope |
