# Functional Story Gap Tracker

> **Language:** English | [Leer en Español](../project-es/functional-story-gap-tracker.md)

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
| Implemented / usable | 25 | [FS-01](../requirements/functional-stories/fs-01-user-authentication.md), [FS-02](../requirements/functional-stories/fs-02-create-authorization-template.md), [FS-03](../requirements/functional-stories/fs-03-register-organization.md), [FS-04](../requirements/functional-stories/fs-04-register-system-topology.md), [FS-05](../requirements/functional-stories/fs-05-create-profile-manual-template.md), [FS-06](../requirements/functional-stories/fs-06-auto-assign-template.md), [FS-07](../requirements/functional-stories/fs-07-visual-graph-resolver.md), [FS-08](../requirements/functional-stories/fs-08-hosted-login-redirection.md), [FS-09](../requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md), [FS-10](../requirements/functional-stories/fs-10-external-b2b-access-request-approval.md), [FS-11](../requirements/functional-stories/fs-11-user-document-upload.md), [FS-13](../requirements/functional-stories/fs-13-hierarchical-config.md), [FS-14](../requirements/functional-stories/fs-14-delegated-management.md), [FS-15](../requirements/functional-stories/fs-15-notification-rules.md), [FS-16](../requirements/functional-stories/fs-16-access-enforcement-policy.md), [FS-17](../requirements/functional-stories/fs-17-maintain-system-roles.md), [FS-18](../requirements/functional-stories/fs-18-manage-local-user-password.md), [FS-19](../requirements/functional-stories/fs-19-admin-password-reset-validity-management.md), [FS-20](../requirements/functional-stories/fs-20-system-parameter-management.md), [FS-21](../requirements/functional-stories/fs-21-tenant-signup-request-approval.md), [FS-22](../requirements/functional-stories/fs-22-user-signup-request-approval.md), [FS-24](../requirements/functional-stories/fs-24-profile-request-approval.md), [FS-25](../requirements/functional-stories/fs-25-ddd-domain-resource-hierarchy.md), [FS-26](../requirements/functional-stories/fs-26-auth-graph-preview-from-profile.md), [FS-27](../requirements/functional-stories/fs-27-state-change-consistency-broken-rules.md) |
| Partial | 0 | — |
| Deferred | 9 | [FS-12](../requirements/functional-stories/fs-12-role-promotion-process.md), [FS-28](../requirements/functional-stories/fs-28-access-review-campaigns.md), [FS-29](../requirements/functional-stories/fs-29-entitlement-packages.md), [FS-30](../requirements/functional-stories/fs-30-provisioning-deprovisioning-connectors.md), [FS-31](../requirements/functional-stories/fs-31-privileged-access-time-bound-elevation.md), [FS-32](../requirements/functional-stories/fs-32-operational-reliability-guardrails.md), [FS-33](../requirements/functional-stories/fs-33-authorization-graph-explorer.md), [FS-34](../requirements/functional-stories/fs-34-business-semantic-access-packages.md), [FS-35](../requirements/functional-stories/fs-35-continuous-access-health.md) |

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
| [FS-12](../requirements/functional-stories/fs-12-role-promotion-process.md) | Execute Role Promotion Process | Deferred | P3 | M | H | IGA | TBD | Reserved | The promotion flow has been removed from the runtime and is reserved for future reprioritization. | Reintroduce the promotion state machine only if the product reopens this story. |
| [FS-28](../requirements/functional-stories/fs-28-access-review-campaigns.md) | Access Review Campaigns for Role and Permission Recertification | Deferred | P2 | H | H | IGA / Approvals | TBD | Proposed | No campaign aggregate, reviewer inbox, or decision loop exists yet. | Define the campaign model, reviewer assignment, and closure policy. |
| [FS-29](../requirements/functional-stories/fs-29-entitlement-packages.md) | Entitlement Packages for Governed Access Bundles | Deferred | P2 | H | H | Authorization / IGA | TBD | Proposed | No package aggregate or versioned bundle assignment exists yet. | Define the package model and governed assignment path. |
| [FS-30](../requirements/functional-stories/fs-30-provisioning-deprovisioning-connectors.md) | Provisioning and Deprovisioning Connectors for Downstream Systems | Deferred | P2 | H | H | Identity / Integration | TBD | Proposed | No connector runtime or delivery-state tracking exists yet. | Define connector adapters, retry state, and deprovisioning flow. |
| [FS-31](../requirements/functional-stories/fs-31-privileged-access-time-bound-elevation.md) | Privileged Access with Time-Bound Elevation | Deferred | P3 | M | M | Approvals | TBD | Proposed | No temporary elevation model or expiry enforcement exists yet. | Define the time-bound elevation request and auto-revoke behavior. |
| [FS-32](../requirements/functional-stories/fs-32-operational-reliability-guardrails.md) | Operational Reliability Guardrails for Governance Actions | Deferred | P1 | H | H | Platform | TBD | Proposed | Idempotency, concurrency protection, and tenant-safe delivery are still missing as first-class product guardrails. | Define the reliability guardrails and surface the conflict/retry contract. |
| [FS-33](../requirements/functional-stories/fs-33-authorization-graph-explorer.md) | Authorization Graph Explorer with What-If Simulation | Deferred | P2 | H | H | Authorization / Support | TBD | Proposed | No simulation or diff-based graph preview exists yet. | Define the read-only graph simulation and diff output. |
| [FS-34](../requirements/functional-stories/fs-34-business-semantic-access-packages.md) | Business-Semantic Access Packages and Policy Composer | Deferred | P2 | H | H | Authorization / Product | TBD | Proposed | No business-semantic package composer or immutable versioning model exists yet. | Define the package composer and publication lifecycle. |
| [FS-35](../requirements/functional-stories/fs-35-continuous-access-health.md) | Continuous Access Health and Recommendations | Deferred | P2 | M | M | IGA / Security | TBD | Proposed | No continuous health scoring or recommendation engine exists yet. | Define health signals, scoring, and remediation recommendations. |
---

## FS-13 Gap Breakdown

> Gaps ordered by priority and criticality. Work them top to bottom. Each gap references the acceptance criterion or business rule it blocks.

| # | Gap | Severity | Blocks | What to implement | Key files |
|---|---|---|---|---|---|
| 1 | ~~**Full 4-level hierarchy resolver**~~ **Done** — `GetWithPrecedence(code, tenantId, suiteId, moduleId)` added. Full Module → Suite → Tenant → Global cascade implemented. 10 new tests covering all fallback paths. | Closed | AC-3, BR-1 | — | `Ums.Infrastructure/Configuration/InMemoryConfigurationCache.cs`, `ConfigurationProvider.cs` |
| 2 | ~~**Non-overridable flag**~~ **Done** — `IsNonOverridable` added to domain, record, DTO, and command. Guard in `CreateAppConfigurationCommandHandler` rejects overrides when any parent scope has `IsNonOverridable=true`. Migration `Fs13AddIsNonOverridableToAppConfiguration` generated. 4 new guard tests. | Closed | AC-2, BR-2 | — | `AppConfigurationProps.cs`, `AppConfigurationRecord.cs`, `CreateAppConfigurationCommandHandler.cs` |
| 3 | ~~**Suite and Module scopes not processed**~~ **Done** — `ReloadTenantAsync` now fully invalidates Suite/Module buckets before repopulating. `BucketTenantConfigs` helper filters Published-only and buckets each scope correctly. Draft/Archived configs no longer enter the resolution cache. 3 new tests for stale-entry eviction and Published-only semantics. | Closed | AC-1, AC-3 | — | `ConfigurationProvider.cs` |
| 4 | ~~**Sensitive value protection**~~ **Done** — AES-256-CBC encryption wired end-to-end: encrypt on Create/Update (handler), decrypt on cache load (ConfigurationProvider), redact `"***"` in DTOs for non-admin callers. `IValueEncryptionService` + `AesValueEncryptionService` added. Key from `AppConfiguration:EncryptionKey` (dev fallback uses zero-key). 9 tests: round-trip, idempotency, prefix detection, dev fallback, key-length guard. | Closed | BR-5 | — | `AesValueEncryptionService.cs`, `CreateAppConfigurationCommandHandler.cs`, `ConfigurationProvider.cs`, query handlers |
| 5 | ~~**Missing `/resolve` endpoint**~~ **Done** — `GET /app-configurations/resolve?code=X&tenantId=Y&suiteId=Z&moduleId=W` added. `ResolveAppConfigurationQuery/Handler` wired to `IConfigurationProvider.GetWithPrecedence`. Returns `ResolvedAppConfigurationDto` with `ResolvedScope`, `SourceConfigId`, `Found` flag. Encrypted values redacted for non-admin. 5 new tests. | Closed | AC-3 | — | `ResolveAppConfigurationQueryHandler.cs`, `AppConfigurationQueryEndpoints.cs` |
| 6 | ~~**No REST endpoints for Parameters**~~ **Done (backend)** — Repositories, commands, and REST endpoints added for ParameterDefinition (Create/Update/Archive), ParameterGlobalValue (Create/Update/Publish/Archive), and ParameterTenantValue (Create/Update). Rehydration added to ConfigurationAggregateFactory. InMemory implementations for dev/tests. Frontend panel remains deferred. | Closed | AC-1 | — | `SqlServerParameterRepositories.cs`, `ParameterDefinitionCommands.cs`, `ParameterValueCommands.cs`, `ParameterEndpoints.cs` |
| 7 | ~~**Scope model inconsistency**~~ **Done** — `ParameterScope` extended with `SuiteLevel(4)` and `ModuleLevel(5)` matching `ConfigurationScope` IDs. `SupportsGlobal/Tenant/Suite/Module` helpers added. `AllScopes` updated. No migration needed (values are additive). | Closed | BR-1 | — | `Ums.Domain/Configuration/Parameter/ValueObjects/ParameterScope.cs` |
| 8 | ~~**`IdpConfiguration` missing Archive lifecycle**~~ **Done** — `IdpConfigStatus.Archived(4)` added. `IdpConfiguration.Archive()` domain method added (blocks if Active or already Archived). `ArchiveIdpConfigurationCommand` + handler + `POST /idp-configurations/{id}/archive` endpoint added. Update now blocks on Archived status. | Closed | Lifecycle completeness | — | `IdpConfigStatus.cs`, `IdpConfiguration.cs`, `ArchiveIdpConfigurationCommand*.cs`, `IdpConfigurationEndpoints.cs` |

---

## Review Cadence

- Update this tracker whenever a story moves in the backlog or the implementation tracker changes.
- Re-audit the open gaps after any domain, API, or documentation change affecting the listed stories.
- Keep the English and Spanish versions synchronized in structure and content.

## Last Review

2026-06-04 (FS-13, FS-14 and FS-24 fully implemented; FS-12 reserved for future; FS-28 to FS-35 added as pending implementation)
