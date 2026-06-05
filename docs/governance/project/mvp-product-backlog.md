# MVP Product Backlog

## Purpose

This backlog organizes the UMS product work into epics and user stories based on the functional priority order defined in the [MVP Functional Story Prioritization](../roadmap/mvp-functional-prioritization.md).

The backlog is intended for Product Owner, Business Analyst, Executive, and Delivery Team use. It focuses on business value and delivery sequencing, while keeping traceability to the Functional Stories.

## Priority Rules

- `P1` is the highest product priority.
- Lower priority numbers should generally be refined and delivered first.
- Stories marked as `MVP Core` are required to prove the minimum end-to-end product.
- Stories marked as `Post-MVP` or `Later` should not block the MVP unless a launch customer or regulatory condition requires them.

## Epic Summary

| Epic | Name | Scope | MVP Role |
|---|---|---|---|
| EP-01 | Tenant and Identity Foundation | Organization registration, tenant boundary, IdP strategy, corporate authentication | MVP Core |
| EP-02 | Governed System Catalog | System, module, menu, option, and action topology | MVP Core |
| EP-03 | Authorization Design and Assignment | Authorization templates, profiles, manual assignment, later auto-assignment | MVP Core / Later |
| EP-04 | Configuration Foundation | Hierarchical tenant/system parameters and operational configuration | MVP Core |
| EP-05 | Access Experience and Diagnosis | Permission graph diagnosis and hosted login experience | MVP Stabilization |
| EP-06 | Security, External Access, and Delegation | MFA/passwordless, B2B access, delegated administration | Post-MVP |
| EP-07 | Compliance Lifecycle | User documents, expiration notifications, expiration-based access policy | Post-MVP |
| EP-08 | Advanced IGA Automation | Role promotion and governance maturity automation | Later |
| EP-09 | Onboarding Approval Inbox | Tenant signup requests, user signup requests, scoped approvals, future payment-verification readiness | MVP Launch Readiness |
| EP-10 | Advanced Access Governance | Access review campaigns, entitlement packages, privileged access elevation | Phase 1 |
| EP-11 | Integration and Operational Reliability | Provisioning connectors, duplicate-request protection, concurrency safeguards, tenant-safe delivery | Phase 1 |
| EP-12 | Explainable Governance Intelligence | Authorization graph simulation, business-semantic packages, continuous access health | Phase 1 |

## Prioritized User Story Backlog

| Rank | Story ID | Epic | Source FS | Phase | User Story | Business Outcome | Initial Acceptance Focus |
|---:|---|---|---|---|---|---|---|
| 1 | US-001 | EP-01 | FS-03 | MVP Core | As a Platform Administrator, I want to register an organization/tenant so that UMS can manage users and access within a clear business boundary. | UMS has a controlled tenant scope. | Organization is created with required identity, ownership, status, and audit information. |
| 2 | US-002 | EP-01 | FS-03 | MVP Core | As a Platform Administrator, I want to configure the tenant IdP strategy so that users authenticate through the correct corporate identity provider. | Authentication strategy is governed per tenant. | IdP configuration is registered, validated, activated, and traceable. |
| 3 | US-003 | EP-02 | FS-04 | MVP Core | As a System Owner, I want to register a governed system so that UMS can manage access for that system. | UMS knows which system it governs. | System is created under the correct organization/tenant with lifecycle status and ownership. |
| 4 | US-004 | EP-02 | FS-04 | MVP Core | As a System Owner, I want to define modules, menus, options, and actions so that business permissions can be assigned over real functional capabilities. | Authorization is based on a governed functional catalog. | Functional topology is complete, ordered, unique, and available for authorization templates. |
| 5 | US-005 | EP-01 | FS-01 | MVP Core | As a Corporate User, I want to authenticate through my external IdP so that I can access UMS without separate credentials. | Users can enter UMS using corporate identity. | Valid users can sign in and receive the correct tenant/session context. |
| 6 | US-006 | EP-01 | FS-01 | MVP Core | As a Security Administrator, I want failed or invalid authentication attempts to be handled clearly so that access remains controlled. | Authentication failures are understandable and auditable. | Invalid users, disabled tenants, and IdP errors produce controlled outcomes and audit records. |
| 7 | US-007 | EP-03 | FS-02 | MVP Core | As an Authorization Administrator, I want to create an authorization template so that standard access patterns can be reused. | Access design becomes repeatable. | Template has name, scope, status, version, description, and selected permissions. |
| 8 | US-008 | EP-03 | FS-02 | MVP Core | As an Authorization Administrator, I want to instantiate an authorization template for a tenant/system so that it can be assigned consistently. | Standard permissions can be applied in a controlled way. | Template instance preserves version, scope, permissions, and traceability. |
| 9 | US-009 | EP-03 | FS-05 | MVP Core | As an Administrator, I want to create a user profile so that a user can receive a business access identity inside UMS. | Users can be represented for access governance. | Profile is linked to tenant/user context and has a governed lifecycle status. |
| 10 | US-010 | EP-03 | FS-05 | MVP Core | As an Administrator, I want to manually assign an authorization template to a profile so that the user receives the intended permissions. | First end-to-end access grant is operational. | Assignment is validated, traceable, auditable, and reflected in effective permissions. |
| 11 | US-011 | EP-04 | FS-13 | MVP Core | As a Configuration Administrator, I want to define hierarchical parameters so that UMS behavior can vary by tenant, system, or scope. | Runtime behavior can be configured without code changes. | Parameters include code, value, description, scope, status, version, and audit information. |
| 12 | US-012 | EP-04 | FS-13 | MVP Core | As a Product Owner, I want configuration inheritance to be predictable so that business teams understand which value applies. | Configuration behavior is explainable. | Effective value resolution follows the documented hierarchy and exposes its source. |
| 13 | US-013 | EP-05 | FS-07 | MVP Stabilization | As a Support Administrator, I want to diagnose why a user has access so that incidents can be resolved quickly. | Access support becomes transparent. | Diagnosis shows the path from profile/template to effective permission. |
| 14 | US-014 | EP-05 | FS-07 | MVP Stabilization | As an Auditor, I want to see why a user does not have access so that rejected access can be explained. | Access denial becomes explainable. | Missing permissions, inactive templates, expired grants, and scope mismatches are visible. |
| 15 | US-015 | EP-05 | FS-08 | MVP Launch Readiness | As a Tenant Administrator, I want to customize the hosted login experience so that users recognize the correct organization context. | Login feels tenant-aware and trustworthy. | Branding, tenant hints, and redirect behavior are configurable within approved limits. |
| 16 | US-016 | EP-05 | FS-08 | MVP Launch Readiness | As a Corporate User, I want to be redirected back to the correct application after login so that access feels seamless. | Authentication supports a clean product experience. | Successful login redirects to the intended application and preserves valid context. |
| 17 | US-017 | EP-06 | FS-09 | Post-MVP Security | As a Security Administrator, I want adaptive MFA rules so that higher-risk access requires stronger verification. | Security posture improves without making all users follow the same friction. | MFA requirement changes based on configured risk, tenant, user, or action conditions. |
| 18 | US-018 | EP-06 | FS-09 | Post-MVP Security | As a Corporate User, I want passwordless authentication when allowed so that login is secure and low-friction. | User experience improves while maintaining control. | Passwordless options are available only when configured and valid for the tenant/user. |
| 19 | US-019 | EP-06 | FS-10 | Post-MVP Expansion | As a Sponsor User, I want to request external B2B access so that a partner can collaborate under controlled approval. | External access can be requested without informal processes. | Request captures sponsor, external identity, business reason, target access, and expiration. |
| 20 | US-020 | EP-06 | FS-10 | Post-MVP Expansion | As an Approver, I want to approve or reject B2B access requests so that external access is governed. | Partner onboarding is controlled and auditable. | Decision records approval outcome, justification, scope, expiration, and audit trail. |
| 21 | US-021 | EP-06 | FS-14 | Post-MVP Governance | As a Senior Administrator, I want to delegate user management so that local administrators can manage users within controlled limits. | Administration scales without losing governance. | Delegation defines scope, allowed actions, validity, and responsible administrator. |
| 22 | US-022 | EP-06 | FS-14 | Post-MVP Governance | As a Delegated Administrator, I want to manage only users within my assigned scope so that responsibilities remain clear. | Delegated operations respect organizational boundaries. | Delegated admin can act only inside assigned tenant/system/user scope. |
| 23 | US-023 | EP-07 | FS-11 | Post-MVP Compliance | As a User or Administrator, I want to upload required user documents so that access eligibility can be supported by evidence. | Compliance evidence can be collected. | Document is uploaded, classified, linked to the user, and assigned validation status. |
| 24 | US-024 | EP-07 | FS-11 | Post-MVP Compliance | As a Validator, I want to approve or reject user documents so that only valid evidence affects access governance. | Compliance decisions become governed. | Validation captures decision, reason, validator, timestaamp, and document status. |
| 25 | US-025 | EP-07 | FS-15 | Post-MVP Compliance | As a Compliance Administrator, I want notification rules for expiring items so that responsible users are warned before impact. | Expiration risk is reduced proactively. | Rule includes code, value, description, audience, timing, channel, and scope. |
| 26 | US-026 | EP-07 | FS-15 | Post-MVP Compliance | As a Responsible User, I want to receive expiration notifications so that I can act before access is affected. | Users and admins can respond before enforcement. | Notification is delivered according to configured rule and tracked for audit. |
| 27 | US-027 | EP-07 | FS-16 | Post-MVP Compliance | As a Compliance Administrator, I want to define access behavior when an item expires so that UMS enforces compliance consistently. | Expired conditions produce predictable access outcomes. | Policy defines warning, restriction, suspension, or exception behavior by scope. |
| 28 | US-028 | EP-07 | FS-16 | Post-MVP Compliance | As an Auditor, I want expiration-based access changes to be traceable so that enforcement can be reviewed. | Compliance enforcement is explainable. | Enforcement records reason, affected user, affected access, policy, and timestaamp. |
| 29 | US-029 | EP-03 | FS-06 | Later Automation | As an Administrator, I want templates to be auto-assigned when a profile is created so that repetitive access setup is reduced. | Administration becomes faster after manual assignment is stable. | Auto-assignment follows configured rules and can be reviewed before activation when required. |
| 30 | US-030 | EP-03 | FS-06 | Later Automation | As a Product Owner, I want auto-assignment rules to be transparent so that business owners trust automated access decisions. | Automated access remains understandable. | Auto-assignment outcome explains which rule matched and why. |
| 31 | US-031 | EP-08 | FS-12 | Later Advanced IGA | As an IGA Administrator, I want to promote a role through a governed process so that role maturity evolves without uncontrolled changes. | Role governance becomes mature and controlled. | Promotion request captures current role, target state, reason, review, and approval. |
| 32 | US-032 | EP-08 | FS-12 | Later Advanced IGA | As an Approver, I want to review role promotion impact so that risky role changes are not promoted blindly. | Role changes are risk-aware. | Review shows affected profiles, permissions, systems, and expected business impact. |
| 33 | TE-07 | INFRA | ADR-0058 | SaaS Evolution — Technical Debt | As a Platform Architect, I want to introduce a centralized YARP API Gateway so that all future clients (web, mobile) share a single, governed entry point for security policy and routing. | UMS can scale to multiple client surfaces without duplicating security configuration. | Gateway routes `/api/**` and `/graphql` to `ums-api`; security headers centralized; nginx reduced to static file server; mobile client can connect without special configuration. |
| 34 | US-033 | EP-09 | FS-21 | MVP Launch Readiness | As a System Admin, I want a single onboarding inbox for tenant signup requests so that new companies can be reviewed and approved in one place. | Company onboarding is governed centrally. | Request is visible as pending, only global approvers can review it, and approval creates the tenant plus the first admin account. |
| 35 | US-034 | EP-09 | FS-22 | MVP Launch Readiness | As a Tenant Admin, I want a tenant-scoped onboarding inbox for user signup requests so that I can approve or deny access for my own tenant. | User onboarding stays local to the tenant and reaches a final outcome. | Request is visible only inside the tenant, the Tenant Admin closes it as approved or denied, and the applicant is notified of the final result. |
| 36 | US-035 | EP-09 | FS-23 | MVP Launch Readiness | As an authenticated user without a profile, I want to request system, branch, and role access from the lobby so that administrators know what access I need. | Tenant admission stays separate from entitlement assignment. | The user sees a lobby, submits a profile request, receives no operational menus before approval, and can track the request until final closure. |
| 37 | US-036 | EP-09 | FS-24 | MVP Launch Readiness | As a Tenant or Branch Approver, I want to approve, modify, or deny profile requests so that users receive only appropriate access. | Profile assignment becomes governed, auditable, and closed. | The decision records approver, date, requested role, granted role when approved, final outcome, reason when provided, and notification result. |
| 38 | US-041 | EP-11 | FS-32 | Phase 1 | As a Platform Administrator, I want operational guardrails for governance actions so that retries, concurrency, and tenant scope do not create inconsistent state. | Administrative operations become more reliable under real production conditions. | Duplicate requests, concurrency conflicts, and tenant mismatches produce clear outcomes and no silent corruption. |
| 39 | US-037 | EP-10 | FS-28 | Phase 1 | As an Access Governance Administrator, I want recurring access review campaigns so that stale access can be recertified and removed. | Access remains justified over time. | Review scope, reviewers, decisions, and resulting access changes are all auditable. |
| 40 | US-038 | EP-10 | FS-29 | Phase 1 | As an Entitlement Administrator, I want governed entitlement packages so that common access bundles can be requested and assigned as one unit. | Access is packaged into reusable bundles. | The package contains controlled entitlements, follows approval, and stays auditable by version. |
| 41 | US-039 | EP-11 | FS-30 | Phase 1 | As an Identity Operations Administrator, I want downstream provisioning connectors so that access changes are reflected in external systems. | UMS stays synchronized with downstream applications. | Provisioning and deprovisioning actions are tracked, retried, and auditable. |
| 42 | US-040 | EP-10 | FS-31 | Phase 1 | As a Security Administrator, I want time-bound privileged access so that elevated access is temporary and justified. | Privileged access becomes safer and easier to review. | Every elevation has a reason, duration, expiry, and auditable decision. |
| 43 | US-042 | EP-12 | FS-33 | Phase 1 | As an Authorization Administrator, I want to preview and simulate the authorization graph so that I can approve changes with confidence before they go live. | Access changes become explainable and safer to approve. | Current and proposed graphs are comparable, and the preview does not change live access. |
| 44 | US-043 | EP-12 | FS-34 | Phase 1 | As an Entitlement Architect, I want business-semantic access packages so that access can be composed and governed in language the business understands. | Access packages map to real business operations. | Packages are versioned, scope-aware, and reusable without losing history. |
| 45 | US-044 | EP-12 | FS-35 | Phase 1 | As a Security Administrator, I want continuous access health insights so that I can see risky or stale access before it becomes an incident. | Governance becomes proactive instead of only reactive. | Health signals, recommendations, and remediation paths are visible and auditable. |

## MVP Cut Line

The recommended MVP backlog includes `US-001` through `US-012` as core scope.

`US-013` through `US-016` should be treated as launch stabilization scope: not always required for a technical MVP, but strongly recommended before a real production pilot.

`US-017` onward should be planned after the MVP unless the launch customer requires stronger security, external access, or compliance gating from day one.

`US-033` through `US-036` should be treated as launch readiness for onboarding. If self-service onboarding goes live, these stories become a release gate because they control company admission, tenant-scoped account activation, and entitlement assignment.

`US-037` onward is Phase 1. It is ordered by delivery priority and should focus first on operational guardrails, then access governance depth, downstream provisioning, time-bound privilege management, and explainable governance intelligence.
