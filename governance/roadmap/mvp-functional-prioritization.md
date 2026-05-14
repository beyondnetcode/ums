# MVP Functional Story Prioritization

## Purpose

This document defines the recommended creation order for the UMS Functional Stories from a Product Owner, Business Analyst, and Executive perspective.

Priority `1` represents the most critical functional capability required for a minimum viable UMS product. The highest priority number represents the least core or most optional capability, which can be delivered after the MVP unless a specific customer, regulatory, or contractual constraint changes the business context.

## Prioritization Criteria

The prioritization considers:

- Functional dependency: capabilities that other stories need in order to work.
- MVP value: capabilities required to prove the UMS product works end to end.
- Business risk reduction: capabilities that reduce uncertainty for onboarding, access, and governance.
- Operational readiness: capabilities needed to support and diagnose the first production use.
- Delivery sequencing: capabilities that should be implemented only after their business foundation exists.

## MVP Assumption

The MVP must prove that UMS can:

- Register an organization/tenant and its identity strategy.
- Register a system and its functional access structure.
- Authenticate users through the configured identity provider.
- Define reusable authorization templates.
- Assign access to users through profiles.
- Support core configuration and operational diagnosis.

Compliance automation, delegated administration, external B2B flows, notifications, and advanced IGA automation are valuable, but they depend on the core identity and authorization foundation.

## Recommended Priority Order

| Priority | Story | Functional Capability | MVP Classification | Executive / Product Rationale | Key Dependency |
|---:|---|---|---|---|---|
| 1 | FS-03 | Register organization and configure IdP strategy | MVP Core | Establishes the tenant, organization, and identity boundary. Without this, UMS has no controlled business scope. | None |
| 2 | FS-04 | Register system and define menu topology | MVP Core | Defines the systems, modules, options, and actions that UMS will govern. Authorization cannot be meaningful without this catalog. | FS-03 |
| 3 | FS-01 | Corporate user authentication via external IdP | MVP Core | Enables users to enter UMS using the configured identity provider. This validates the identity foundation. | FS-03 |
| 4 | FS-02 | Create and instantiate authorization template | MVP Core | Creates reusable permission blueprints for systems and functional options. This is the foundation for consistent access management. | FS-04 |
| 5 | FS-05 | Create profile and manually assign authorization template | MVP Core | Converts authorization design into real user access. This completes the first end-to-end MVP access path. | FS-02, FS-03 |
| 6 | FS-13 | Configure hierarchical system parameters | MVP Core | Provides the configuration foundation for tenant, system, feature, security, workflow, and business rule behavior. | FS-03, FS-04 |
| 7 | FS-07 | Diagnose permissions via graph visualizer | MVP Stabilization | Gives administrators and support teams visibility into why a user has or does not have access. This reduces launch risk. | FS-02, FS-05 |
| 8 | FS-08 | Authenticate via customizable hosted login page | MVP Launch Readiness | Improves the login experience, branding, and redirect behavior once basic authentication is already proven. | FS-01, FS-03 |
| 9 | FS-09 | Adaptive MFA and passwordless authentication | Post-MVP Security | Strengthens authentication with adaptive controls, but should follow the stable external IdP authentication base. | FS-01 |
| 10 | FS-10 | External B2B access request and approval flow | Post-MVP Business Expansion | Enables controlled external access and sponsor-led onboarding after the internal access model is stable. | FS-03, FS-05 |
| 11 | FS-14 | Delegate user management between administrators | Post-MVP Governance Scale | Supports distributed administration when the organization grows beyond centralized administration. | FS-05 |
| 12 | FS-11 | Upload and validate user document | Post-MVP Compliance Foundation | Introduces document-based compliance evidence after the core user and access model exists. | FS-03, FS-05 |
| 13 | FS-15 | Configure expiration notification rules | Post-MVP Compliance Operations | Adds proactive operational control over expiring documents, permissions, or lifecycle events. | FS-11, FS-13 |
| 14 | FS-16 | Define access policy on expiration | Post-MVP Compliance Enforcement | Enforces access behavior when compliance conditions expire. This should follow document capture and notification rules. | FS-11, FS-15 |
| 15 | FS-06 | Auto-assign authorization template on profile creation | Later Automation | Optimizes repetitive administration, but manual assignment must exist and be validated first. | FS-05 |
| 16 | FS-12 | Execute role promotion process | Later Advanced IGA | Automates role evolution and promotion. Valuable for maturity, but not required for the first functional MVP. | FS-05, FS-06 |

## Recommended Delivery Phases

| Phase | Priorities | Scope | Product Intent |
|---|---:|---|---|
| MVP Core | 1-6 | Tenant, system catalog, authentication, authorization templates, profile assignment, hierarchical configuration | Prove the minimum end-to-end UMS capability. |
| MVP Stabilization | 7-8 | Permission diagnosis and hosted login experience | Make the MVP usable, supportable, and launch-ready. |
| Post-MVP Security and Expansion | 9-11 | Adaptive authentication, B2B access, delegated administration | Expand control, reach, and operational governance. |
| Post-MVP Compliance | 12-14 | Documents, expiration notifications, expiration-based access policy | Add compliance lifecycle management. |
| Later Advanced IGA | 15-16 | Auto-assignment and role promotion | Automate and mature the governance model. |

## Product Owner Guidance

The MVP should not start with automation-heavy stories. UMS first needs a clear business foundation: who owns the tenant, what system is being governed, how users authenticate, what access can be granted, and how that access is assigned.

If a specific launch customer requires document-gated access, then FS-11, FS-15, and FS-16 should be promoted into the MVP scope after FS-05 and FS-13. Otherwise, they are better delivered after the core access model is stable.

