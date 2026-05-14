# Master Report: Evaluation of Multi-Tenant Governance and Organizational Structure

*   **Status:** Finalized
*   **Date:** 2026-05-13
*   **Area:** Enterprise Architecture & Product Ownership

---

## 1. Evaluation of Current Domain and Findings

After an exhaustive analysis of the UMS ecosystem, we have identified a **rigid structural coupling** in the original design that negatively impacts the platform's SaaS evolution.

### Key Architectural Findings
1.  **Identity Coupling (Employee vs. Subject):** The dependence on `identity_reference` as a mandatory field assumes an internal employment relationship. This blocks B2B scenarios where the user is an external supplier, a third-party driver, or an integration bot (M2M) lacking a record in the corporate HR database.
2.  **Orphaned Software Assets:** Systems (ERP, CRM, HCM) and their components (Menus, APIs) currently operate in a "flat" global catalog. There is no explicit definition of who is the logical owner of the resource and under what conditions a third party (Tenant) can consume it.
3.  **Ambiguous Security Boundaries:** The permission model focuses on the "What" (action) but not on "Who owns the object." This violates **Zero Trust** principles where the Organization should be the physical and logical frontier for every bit of information.

---

## 2. Target Domain Model (Enterprise-Grade)

We propose a restáructuring where the **Organization** is elevated as the **Strategic Aggregate Root** of the entire ecosystem.

### Recommended Hierarchy
*   **Organization (Tenant):** The absolute boundary for governance, isolation, and ownership.
    *   **Identities (Subjects):** Persons, Actors, and Service Accounts under the Org's tutelage.
    *   **Technological Resources:** Systems, Applications, and APIs owned by the Org.
    *   **Governance:** Roles, Permissions, Contextual ABAC Policies, and local approval Workflows.
    *   **Traceability:** Immutable audit logs isolated by Organization.

---

## 3. Multi-Tenant and IAM Strategy (Identity & Access Management)

To meet enterprise scale and **Zero Trust** security requirements, the system will adopt the following model:

### Data Isolation (RLS + Tenant-Aware APIs)
We will utilize a **Shared Database / Shared Schema** model powered by **PostgreSQL Row-Level Security (RLS)**.
*   Any database-level query will be automatically filtered by the PostgreSQL engine using the session variable `app.current_organization_id`.
*   This ensures total immunity against programming errors (e.g., forgetting a `WHERE` clause in the ORM), preventing cross-tenant data leaking.

### System Ownership Model
Systems and applications explicitly belong to an organization.
*   **Owner Org:** Manages the code, the menu catalog, and the API endpoints.
*   **Consumer Org:** Subscribes to system access through a **Contractual Delegation Relationship**, allowing its local users to consume the software without the original owner losing security control.

---

## 4. Risks, Trade-offs, and Mitigations

| Risk | Impact | Mitigation Strategy |
| :--- | :--- | :--- |
| **God Entity (Organization)** | High complexity in a single object. | Decouple via Bounded Contexts. The Org is a shared ID, but its logic is distributed. |
| **Noisy Neighbor** | Performance degradation. | Implement Rate Limiting and Quotas at the API Gateway level per `OrganizationId`. |
| **Migration Complexity** | Breaking changes in existing APIs. | **Coexistence and Deprecation Strategy** (Dual-Read/Write) detailed in ADR-0031. |
| **Service Accounts Governance** | Uncontrolled M2M access. | Inclusion of non-human identities under the Organization hierarchy with secret rotation in Vault.
## 5. Incremental Transition Strategy

1.  **Phase 01 (Foundation):** Implement the Organizations table as a boundary and associate 100% of current users with the "Root Organization."
2.  **Phase 02 (Decoupling):** Migrate from `identity_reference` to `identity_reference` (Agnostic Subject). Inject `X-Org-Context` in the API Gateway.
3.  **Phase 03 (Enforcement):** Activate RLS policies in PostgreSQL for all domain tables.
4.  **Phase 04 (Federation):** Enable the B2B Access Request module so external organizations can autonomously request access to internal systems.

---

## 6. End-to-End Validation Conclusion
This model ensures the business can grow from a closed corporate platform to a **Federated Multi-Tenant SaaS Ecosystem**. The architecture complies with:
*   **Security:** Zero Trust and RLS.
*   **Business:** Flexibility for partners, clients, and suppliers.
*   **Data:** High-performance physical and logical isolation.
*   **Governance:** Clear ownership of each resource and actor.
