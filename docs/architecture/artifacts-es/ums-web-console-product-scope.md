> **Nota de Arquitectura:** Este documento se encuentra actualmente en su versin original (Ingls) y está programado para traduccin oficial en la hoja de ruta.

# UMS Administrative Web Console - Product & Functional Specification

This specification details the product vision, module breakdowns, user roles, and UI/UX architecture for the **UMS Administrative Web Console**, serving as the primary **Policy Administration Point (PAP)** and **Policy Information Point (PIP) Monitor** under the **spec-driven AI strategy BMAD-METHOD**.

---

## 1. Product Vision & Value Proposition
In a highly distributed, multi-tenant enterprise SaaS ecosystem, access governance cannot be handled via raw database scripts or CLI tools. The **UMS Administrative Web Console** is a premium, high-fidelity back-office control panel that empowers both global IT Security Administrators and local B2B Tenant Operations Managers to govern access control on-the-fly.

By serving as a secure, web-based visual interface, the console guarantees **Zero-Ticket Operations** for tenant onboarding, system registrations, and permission updates, while providing real-time SRE monitoring over authorization latency (p95 < 5ms) and active sessions—ensuring maximum security, total auditability, and modern operational excellence.

---

## 2. User Roles & Personas

### Global Security Administrator (SuperAdmin)
* **Mission**: Configures global parameters, registers new Client Systems (ERP, CRM, etc.), onboard new Organizations (Tenants), and audits system-wide access attempts.
* **Pain Point**: Administrative overhead from manual B2B tenant onboarding and lack of centralized observability.

### Tenant Operations Manager (LocalAdmin)
* **Mission**: Delegated administrator of their local organization (e.g., *Logistics Corp*). Manages branch-specific (sedes) user assignments, profile selections, and local resource restrictions.
* **Pain Point**: Dependency on primary software vendor tickets for simple user onboarding.

### SRE / Support Engineer (SRE)
* **Mission**: Audits live authorization latencies, monitors Redis cache hit ratios, and diagnoses permission discrepancies for users (e.g., why a specific Transportation Analyst was blocked from dispatching a fleet).
* **Pain Point**: Opaque "black-box" permission compilation that is difficult to trace.

---

## 3. Core Functional Modules

```

 UMS Web Portal Dashboard

 Organizations Systems Profiles/Templates Security & Graph
 (Tenants) (Resources) (Authorizations) Monitors

```

### Module A: Organizations (Tenant Management)
* **Description**: Fully manages the lifecycle of corporate organizations (tenants) under a strict multi-tenant SaaS model.
* **Functional Criteria**:
* **Tenant CRUD**: Create, read, update, or suspend corporate organizations.
* **Agnostic IdP Strategy Configurator**: Configure the preferred authentication mechanism for the tenant (internal Bcrypt store, external Zitadel, Microsoft Entra ID, Okta, SAML2, or generic OIDC).
* **Branch Context Registry (Sedes)**: Register local branches (such as *Callao Terminal* or *Lurin Warehouse*) to support hierarchical, context-aware authorization routing.

### Module B: Systems & Resources (Systems & Resource Registry)
* **Description**: Registers client systems and defines their dynamic UI/UX navigation topologies.
* **Functional Criteria**:
* **System Registration**: Define systems, their associated modules (e.g., Fleet Management, HR), and generate secure API credentials for gateway validation.
* **UI Layout Topology CRUD**: Define the nested hierarchy of **Modules Menus Options Actions** available in the client system, with **Modules** as the top-level grouping entity (first-class CRUD).
* **Action Taxonomy Manager**: Configure granular action scopes (`create`, `read`, `update`, `delete`, `export`, `approve_dispatch`).

### Module C: Profiles, Templates & Advanced Assignment Engine
* **Description**: Constructs reusable authorization schemas (Templates) and maps them to active user Profiles both manually and automatically.
* **Functional Criteria**:
* **Policy Template Builder**: Build reusable access blocks (e.g., *Baseline Analyst Permissions*) containing allowed modules, menus, options, actions, and contextual ABAC rules.
* **Manual Assignment Workflow**: Direct visual interface where administrators can select a user Profile and manually attach or detach active Authorization Templates.
* **Automatic Rule-Based Assignment Engine**: Trigger-based automated mapping when a profile is created (via automated B2B customer registration, tenant onboarding, or HR sync).
* *Rule Configuration*: Define matching rules (e.g., *if role equals 'TransportationAnalyst' and tenant equals 'LogisticsCorp', automatically assign 'Template_Analyst_Baseline'*).
* *Instant Activation*: Applies baseline templates instantly upon profile generation, ensuring zero administrative latency for new users.
* **Precedence Controller**: Toggle explicit authorization precedence overrides (e.g., enforcing `Explicit-Deny` to instantly block suspended branches).

### Module D: Security Ledger & Observability Monitor
* **Description**: Real-time auditing of authentication attempts and authorization graph resolution.
* **Functional Criteria**:
* **Access Audit Trail**: Live, search-optimized tabular feed of successful and blocked access requests carrying user, IP address, organization, timestaamp, and gateway correlation IDs.
* **OpenTelemetry Tracing Integration**: Clickable links on log records to navigate instantly to Jaeger/Zipkin distributed traces.

### Module E: Interactive Graph Resolver & Visualizer
* **Description**: A powerful visual diagnostic utility for SREs and support teams.
* **Functional Criteria**:
* **Principal Search**: Search for a user using their email or unique ID (e.g., `usr_analyst_callao_098`).
* **Context Selector**: Select target Organization, Branch, and System.
* **Visual Compiled Tree**: Render an interactive tree node showing allowed paths in **Green** and denied paths/actions in **Red** with explicit reasons (e.g., *Blocked by Geofencing ABAC policy*).

---

## 4. Premium UI/UX Architecture & Layout Specifications

To achieve a **Wow Factor** and provide a state-of-the-art administrative portal, the UMS Web Console implements a high-fidelity, premium design system:

* **Sleek Dark Mode**: Rich obsidian backgrounds (`#0B0F19`) coupled with deep indigo accents (`#3B82F6`) and slate text.
* **Glassmorphism Cards**: Transparent, frosted card components (`backdrop-filter: blur(12px)`) with subtle border shadows (`rgba(255, 255, 255, 0.05)`).
* **Harmony Color Palette**:
* *Primary (Interactive)*: Sleek vibrant blue (`hsl(217, 91%, 60%)`)
* *Success (Allowed/Active)*: Emerald mint green (`hsl(142, 70%, 45%)`)
* *Warning/Danger (Blocked/Deny)*: Crimson red (`hsl(350, 80%, 55%)`)
* **Smooth Micro-animations**: Subtle interactive scale transitions on hover (`transform: scale(1.02); transition: all 0.2s ease-in-out`), and elegant skeleton loaders during graph rendering.
* **Modern Typography**: Roboto or Outfit sans-serif typeface to ensure clean scannability.

