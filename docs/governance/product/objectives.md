# Strategic Product Objectives (OKRs) - User Management System (UMS)

To align technical delivery with corporate goals, the development of the User Management System (UMS) is governed by the following measurable **Objectives and Key Results (OKRs)**:

---

## Objective 1: Deliver Enterprise-Grade, Passwordless Security
Ensure the platform eliminates traditional security risks associated with identity management.

- **KR 1.1**: Achieve **Zero local password hashes stored** for federated users, while maintaining a secure, bcrypt-hashed **Native Credentials Store** as an optional internal fallback.
- **KR 1.2**: Support **MANDATORY Multi-Factor Authentication (MFA)** and WebAuthn (Passkeys) for 100% of administrative accounts at Go-Live, utilizing native or pluggable providers.
- **KR 1.3**: Pass all external penetration tests with **Zero High or Critical vulnerabilities** (OWASP Top 10 compliant).

---

## Objective 2: Maximize Authorization Latency & Build Performance
Guarantee that centralized permission checks do not degrade the downstream user experience.

- **KR 2.1**: Keep compiled permission graph retrieval latency **under 50ms** using Read-Aside Redis Caching with TTL < 1 hour.
- **KR 2.2**: Ensure the monorepo build time is **under 5 minutes** by utilizing Nx high-performance task caching.
- **KR 2.3**: Achieve an **EF Core / PostgreSQL row-level security overhead of < 5ms** per SQL query execution.

---

## Objective 3: Enable Self-Service B2B Scalability
Offload administrative tasks to clients, reducing support overhead.

- **KR 3.1**: Reduce client tenant onboarding time from **days to < 10 minutes** via self-service tenant registration portals.
- **KR 3.2**: Achieve **Zero support tickets** for password resets, since users manage their own cryptographic credentials through native or external federated self-service portals.
- **KR 3.3**: Enable **Dynamic Menu Customization** allowing client admins to create and assign permission templates in real-time.

---

## Objective 4: Achieve True Plug-and-Play Extensibility (Vendor-Neutral Core)
Design the system such that all external infrastructures (IdPs and Feature Flag Managers) are completely optional and easily pluggable.

- **KR 4.1**: Deliver a fully functional **Native Fallback Engine** for both Identity (bcrypt/internal DB) and Feature Flags (SQL Server-backed targets/evaluator) out of the box, requiring zero external SaaS dependencies.
- **KR 4.2**: Standardize **100% of external integrations** behind hexagonal ports (`IAuthenticationPort` and `IFeatureFlagPort`), enabling zero-downtime additions/swapping of vendors (Zitadel, Azure AD, Okta, LaunchDarkly, Unleash, ConfigCat) via configuration alone.
- **KR 4.3**: Enforce **Zero external vendor SDK imports** within the pure `core/` or `application/` layers of the NestJS application.
