# Functional Story Gap Tracker

> **Language:** English | [Leer en espanol](../project-es/functional-story-gap-tracker.md)

Living tracker for the gap between the functional story catalog and the current implementation evidence in UMS.

## Purpose

This document keeps a dynamic view of what is already implemented, what is partial, and what is still deferred. It is intended to be updated whenever the backlog, domain design, or API implementation tracker changes.

## Source of Truth

- [Functional Stories Directory](../requirements/functional-stories/index.md)
- [MVP Product Backlog](./mvp-product-backlog.md)
- [API Aggregate Implementation Tracker](./api-aggregate-implementation-tracker.md)
- [DDD Design Portal](../construction/ddd-design/index.md)
- [Traceability Matrix](../../architecture/traceability-matrix.md)

## Coverage Snapshot

| Status | Count | Story IDs |
|---|---:|---|
| Implemented / usable | 22 | [FS-01](../requirements/functional-stories/fs-01-user-authentication.md), [FS-02](../requirements/functional-stories/fs-02-create-authorization-template.md), [FS-03](../requirements/functional-stories/fs-03-register-organization.md), [FS-04](../requirements/functional-stories/fs-04-register-system-topology.md), [FS-05](../requirements/functional-stories/fs-05-create-profile-manual-template.md), [FS-06](../requirements/functional-stories/fs-06-auto-assign-template.md), [FS-07](../requirements/functional-stories/fs-07-visual-graph-resolver.md), [FS-08](../requirements/functional-stories/fs-08-hosted-login-redirection.md), [FS-09](../requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md), [FS-10](../requirements/functional-stories/fs-10-external-b2b-access-request-approval.md), [FS-11](../requirements/functional-stories/fs-11-user-document-upload.md), [FS-15](../requirements/functional-stories/fs-15-notification-rules.md), [FS-16](../requirements/functional-stories/fs-16-access-enforcement-policy.md), [FS-17](../requirements/functional-stories/fs-17-maintain-system-roles.md), [FS-18](../requirements/functional-stories/fs-18-manage-local-user-password.md), [FS-19](../requirements/functional-stories/fs-19-admin-password-reset-validity-management.md), [FS-20](../requirements/functional-stories/fs-20-system-parameter-management.md), [FS-21](../requirements/functional-stories/fs-21-tenant-signup-request-approval.md), [FS-22](../requirements/functional-stories/fs-22-user-signup-request-approval.md), [FS-25](../requirements/functional-stories/fs-25-ddd-domain-resource-hierarchy.md), [FS-26](../requirements/functional-stories/fs-26-auth-graph-preview-from-profile.md), [FS-27](../requirements/functional-stories/fs-27-state-change-consistency-broken-rules.md) |
| Partial | 5 | [FS-12](../requirements/functional-stories/fs-12-role-promotion-process.md), [FS-13](../requirements/functional-stories/fs-13-hierarchical-config.md), [FS-14](../requirements/functional-stories/fs-14-delegated-management.md), [FS-23](../requirements/functional-stories/fs-23-profile-access-request.md), [FS-24](../requirements/functional-stories/fs-24-profile-request-approval.md) |
| Deferred | 0 | — |

## Tracking Legend

| Field | Meaning |
|---|---|
| Status signal | `Green` = implemented / usable, `Amber` = partial, `Red` = deferred or missing |
| Priority | `P1` = highest follow-up priority, `P2` = important follow-up, `P3` = deferrable |
| Criticality | `H` = security / launch / compliance risk, `M` = product gap without immediate risk, `L` = backfill or traceability gap |
| Complexity | `H` = multi-aggregate or multi-layer change, `M` = one context with several surfaces, `L` = contained change |
| Target | Review target for the next update cycle; use `TBD` until a date is committed |
| Sort order | `FS` number asc, then `Priority` desc, then `Criticality` desc, then `Complexity` desc |

## Open Gap Register

| FS | Story | Signal | Priority | Criticality | Complexity | Owner | Target | Status | Main gap | Next action |
|---|---|---|---|---|---|---|---|---|---|---|
| [FS-12](../requirements/functional-stories/fs-12-role-promotion-process.md) | Execute Role Promotion Process | Amber | P1 | H | H | IGA | TBD | Open | The promotion flow still needs the full manager/security review, execution, verification, and impact analysis closure. | Finish the promotion state machine and align the approval steps with the domain contract. |
| [FS-13](../requirements/functional-stories/fs-13-hierarchical-config.md) | Configure Hierarchical System Parameters | Amber | P1 | H | H | Platform / Configuration | TBD | Open | 8 specific gaps identified (see breakdown below): hierarchy only resolves 2 levels, non-overridable flag is missing, Suite/Module scopes are unresolved, sensitive value protection is incomplete. | Work gaps in priority order as listed in the FS-13 breakdown section. |
| [FS-14](../requirements/functional-stories/fs-14-delegated-management.md) | Delegate User Management Between Administrators | Amber | P2 | M | M | Identity | TBD | Open | Delegation exists as a model, but the end-to-end scope and audit flow still need final validation. | Close the delegated action coverage and verify the acceptance path. |
| [FS-23](../requirements/functional-stories/fs-23-profile-access-request.md) | Profile Access Request from Lobby User | Amber | P1 | H | H | Approvals | TBD | Open | The request model still needs the requested role and audit fidelity expected by the design. | Extend the request contract and lifecycle tracking. |
| [FS-24](../requirements/functional-stories/fs-24-profile-request-approval.md) | Profile Request Approval and Manual Assignment | Amber | P1 | H | H | Approvals | TBD | Open | The decision record still needs requested role, granted role, reason, and notification result coverage. | Extend the approval result payload and persistence model. |

---

## FS-13 Gap Breakdown

> Gaps ordered by priority and criticality. Work them top to bottom. Each gap references the acceptance criterion or business rule it blocks.

| # | Gap | Severity | Blocks | What to implement | Key files |
|---|---|---|---|---|---|
| 1 | ~~**Full 4-level hierarchy resolver**~~ ✅ **Done** — `GetWithPrecedence(code, tenantId, suiteId, moduleId)` added. Full Module → Suite → Tenant → Global cascade implemented. 10 new tests covering all fallback paths. | 🟢 Closed | AC-3, BR-1 | — | `Ums.Infrastructure/Configuration/InMemoryConfigurationCache.cs`, `ConfigurationProvider.cs` |
| 2 | ~~**Non-overridable flag**~~ ✅ **Done** — `IsNonOverridable` added to domain, record, DTO, and command. Guard in `CreateAppConfigurationCommandHandler` rejects overrides when any parent scope has `IsNonOverridable=true`. Migration `Fs13AddIsNonOverridableToAppConfiguration` generated. 4 new guard tests. | 🟢 Closed | AC-2, BR-2 | — | `AppConfigurationProps.cs`, `AppConfigurationRecord.cs`, `CreateAppConfigurationCommandHandler.cs` |
| 3 | **Suite and Module scopes not processed** — `ConfigurationScope.Suite` and `.Module` are defined but no command handler or resolver processes them. A value saved at Suite scope is effectively invisible. | 🔴 P1 / H | AC-1, AC-3 | Wire Suite and Module scope filtering into `CreateAppConfigurationCommandHandler` and `GetWithPrecedence`. Add scope-aware DB queries in the repository. | `Ums.Application/Configuration/AppConfiguration/Commands/CreateAppConfigurationCommandHandler.cs`, `Ums.Infrastructure/Persistence/Configuration/` |
| 4 | **Sensitive value protection** — `IsEncrypted` flag is stored but never used. Raw value is always returned in the DTO regardless of the flag or the caller's role. | 🔴 P1 / H | BR-5 | Implement AES encryption on write in the command handler. Redact `Value` in `AppConfigurationDto` when `IsEncrypted=true` and the caller is not a Global Admin. Add an authorization check in the query handler. | `Ums.Application/Configuration/AppConfiguration/DTOs/AppConfigurationDto.cs`, `Ums.Application/Configuration/AppConfiguration/Commands/CreateAppConfigurationCommandHandler.cs` |
| 5 | **Missing `/resolve` endpoint for AppConfiguration** — Clients have no way to request the effective resolved value for a given scope chain. The equivalent endpoint exists for `IdpConfiguration` but not for `AppConfiguration`. | 🟠 P2 / M | AC-3 | Add `GET /app-configurations/resolve?code=X&tenantId=Y&suiteId=Z&moduleId=W` endpoint. Wire it to the hierarchy resolver from gap #1. | `Ums.Presentation/Endpoints/Configuration/AppConfiguration/AppConfigurationEndpoints.cs` |
| 6 | **No REST endpoints for Parameters** — `ParameterDefinition`, `ParameterGlobalValue`, and `ParameterTenantValue` have application handlers but no HTTP surface and no frontend UI. | 🟠 P2 / M | AC-1 | Add `POST/PUT/GET/DELETE /parameters` and `/parameters/{id}/values` endpoints. Add a basic management panel in the frontend. | `Ums.Presentation/Endpoints/Configuration/`, `ums.web-app/src/presentation/configuration/` |
| 7 | **Scope model inconsistency** — `ParameterScope` (3 values: GlobalOnly/TenantOnly/GlobalAndTenant) and `ConfigurationScope` (5 values: Global/Tenant/User/Suite/Module) are incompatible. The hierarchy resolver cannot be unified until these are aligned. | 🟡 P3 / L | BR-1 | Decide on a single scope model. Migrate `ParameterScope` to use `ConfigurationScope` values, or add Suite and Module to `ParameterScope`. Update all usages. | `Ums.Domain/Configuration/Parameter/ValueObjects/ParameterScope.cs`, `Ums.Domain/Enums/ConfigurationScope.cs` |
| 8 | **`IdpConfiguration` missing Archive lifecycle** — Only Activate/Deactivate exist. There is no Archive command/endpoint to permanently retire an IdP configuration. | 🟡 P3 / L | Lifecycle completeness | Add `ArchiveIdpConfigurationCommand` + handler + `POST /idp-configurations/{id}/archive` endpoint, mirroring the `AppConfiguration` archive pattern. | `Ums.Application/Configuration/IdpConfiguration/Commands/`, `Ums.Presentation/Endpoints/Configuration/IdpConfiguration/` |

---

## Review Cadence

- Update this tracker whenever a story moves in the backlog or the implementation tracker changes.
- Re-audit the open gaps after any domain, API, or documentation change affecting the listed stories.
- Keep the English and Spanish versions synchronized in structure and content.

## Last Review

2026-06-04 (FS-13 detailed breakdown)
