# Product Scope & Boundaries - User Management System (UMS)

## 1. In-Scope Capabilities
The User Management System (UMS) is an **abstract, sovereign security kernel completely decoupled from any specific application suite**. It is designed to govern identities, behavioral configurations, and fine-grained permissions for any generic downstream system, communicating purely via:
- **Synchronous REST APIs and gRPC services** to evaluate permissions, fetch configurations, and authenticate contexts.
- **Asynchronous Message Buses (Events)** to broadcast configuration updates, audit traces, and lifecycle mutations.

The UMS manages the following key functional capabilities:


### A. Organization & Multi-Tenancy Governance
- Definition of **Organizations (Tenants)** and their hierarchical structures.
- Strict database isolation using **PostgreSQL row-level security and database policies** as infrastructure safeguards.
- Delegated administration: allows tenant admins to manage their own local users and assignments.

### B. Granular Authorization & Profile Engine
- Management of **Client Systems** registering to UMS.
- Real-time compilation of the hierarchical **Permission Graph** for users.
- Management of **Policy Templates** that attach reusable authorization blocks to multiple profiles.
- Strict **Explicit-Deny Precedence** rule processing at the core authorization engine.

### C. Dynamic UI Layout Injection
- Dynamic metadata schemas for **Menus, Options, and Actions** mapped to system roles.
- APIs exposing customized navigation structures to clients based on active permissions.

### D. Immutable Business Auditing
- Automatic tracking of all critical data mutations (creation, modification, deletion of users/roles) using database subscribers.
- Secure, isolated, and immutable audit logs.

### E. Administrative UMS Web Portal (Policy Administration Point - PAP)
- **Central Control Panel**: Dynamic maintenance (CRUDs) of Organizations (Tenants), Systems, Modules, Menus, Options, Actions, Profiles, and Roles.
- **Active Session & Telemetry Monitors**: Real-time auditing of authentication attempts, cache hit ratios, and Redis-cache evictions.
- **Visual Graph Resolver**: Interactive visualization of the compiled authorization graph for specific user/tenant contexts (e.g., debugging Transportation Analyst permissions).

### F. Configuration & Feature Management Platform
- **Multi-IdP Configuration Engine**: Per-tenant, per-system registry of Identity Providers with priority/fallback rules, hybrid authentication, and encrypted credential management.
- **System Behavioral Configuration**: Versioned, auditable, multi-tenant JSON configuration controlling auth strategy, session policies, MFA, onboarding flows, branding, and module enablement per system.
- **Feature Flag Framework**: Centralized toggle engine supporting Boolean, Variant, and Percentage flags with multi-dimensional targeting (tenant, org, branch, role, user, environment, system). Supports Canary and Beta rollout strategies.

### G. Customizable Hosted Login Portal
- **Sovereign Login Experience**: Centralized, secure authentication portal hosted by UMS that downstream client systems redirect to for user authentication, featuring phishing-resistant **WebAuthn/Passkeys** and adaptive multi-factor (TOTP, Email, SMS) authentication.
- **Dynamic CSS & Branding Injection**: Login screens are styled dynamically at runtime per tenant or system context using logos, custom stylesheets, font choices, and primary colors fetched from the hierarchical configuration engine.
- **Zero-Trust Token Emission**: Safely processes authentication via federated IdPs or native stores, emitting standardized, signed JWTs with custom tenant headers directly to the redirect URL callback.


---



## 2. Out-of-Scope Capabilities
To prevent scope creep and keep the UMS highly specialized, the following domains are strictly **Out-of-Scope**:

- **Sovereign User Store / Password Hashing** *(Optional Adapter)*: By default, credential verification is delegated to an external Identity Provider (e.g., Zitadel, Azure AD, Okta). Internal Bcrypt-based credential storage is **supported as an optional, pluggable adapter** (`IAuthenticationPort`), activatable on a per-tenant basis. When active, `password_hash` is stored per user. This is not a core UMS responsibility but an opt-in infrastructure adapter.
- **Billing and Subscription Management**: Credit card processing, tenant invoicing, and subscription tier limits are managed by a separate Billing microservice.
- **Direct Mail/SMS Gateways**: Delivery of notifications or verification messages is delegated to Twilio/SendGrid adapters; UMS only triggers the events.
- **Transactional Domain Operations**: Core operations of downstream applications (such as TMS freight planning or WMS warehouse stock) are completely isolated from UMS.
