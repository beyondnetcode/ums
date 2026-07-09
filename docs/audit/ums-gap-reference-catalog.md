# Evolith UMS — Gap Reference Catalog

**Owner:** Evolith UMS Team  
**Status Authority:** [Gap Tracking Board](./ums-gap-tracking.md)  
**Closure Authority:** [Gap Closure Evidence Standard](./ums-gap-closure-evidence-standard.md) · [`ums-gap-closure-evidence.json`](./ums-gap-closure-evidence.json)  
**Design Origin:** mirrors the Evolith Tracker [Gap Reference Catalog](https://github.com/beyondnetcode/evolith_tracker/blob/main/docs/audit/tracker-gap-reference-catalog.md) format, which mirrors the Evolith Core Gap Reference Catalog.

This catalog explains each gap: problem, purpose, evidence, closure criteria, and references. It is not a tracking board; priority and status are authoritative only in the [Gap Tracking Board](./ums-gap-tracking.md).

---

## 1. Deployment Strategy Hardening Wave (DS-01…DS-15)

> Imported 2026-07-09 from the Evolith suite deployment strategy (`evolith/product/suite/architecture/evolith-suite-deployment-strategy.md`). Each `DS-*` id maps 1:1 to a row in the §15 consolidated risk register; UMS owns the consumer-side of the tenant master-data projection and the two-writer tenant-ownership problem. Affected-file paths are verified against the UMS `src/` tree.

#### DS-01

**Title:** UMS still authors tenants locally against MMS mastership (two-writer state).

- **Purpose:** Master Management Service (MMS) is the tenant **writer-of-record** (ADR-0106). UMS must not author tenants; it should consume the master-data projection and read tenant identity from it. Today UMS keeps a local tenant write path, so two writers (UMS and Tracker, plus MMS) can diverge — the suite's #1 architectural risk.
- **Evidence:** §12 — "Two-writer state today: UMS (`CreateTenantCommand` + `TenantEndpoints`) and Tracker (`CreateTenantCommandHandler`) still author tenants locally against MMS mastership." UMS ships `CreateTenantCommand`/`CreateTenantCommandHandler` and a `TenantEndpoints` write surface, and a `PostgreSqlTenantRepository` local aggregate store.
- **Impact:** Tenant identity can drift between UMS's local store and the MMS master; authorization decisions may key off a stale or divergent tenant set.
- **Risk:** Divergent tenant IDs / lifecycle state across the suite; no single source of truth; unauditable ownership; blocks safe ingress exposure and prod promotion of projection features (§13 hard rule).
- **Affected files:** `src/apps/ums.api/Ums.Application/Identity/Tenant/Commands/CreateTenantCommand.cs`; `src/apps/ums.api/Ums.Application/Identity/Tenant/Commands/CreateTenantCommandHandler.cs`; `src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Tenant/TenantEndpoints.cs`; `src/apps/ums.api/Ums.Infrastructure/Persistence/Identity/PostgreSqlTenantRepository.cs`; `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionDbContext.cs`.
- **Component:** `Backend` · **Phase:** Cross · **Type:** arquitectura
- **Criticality:** P0 · **Complexity:** L
- **Proposed fix:** Execute the ownership-migration ladder M0–M4 (§12): M0 plumb (broker + `MasterDataDb` wiring, commit DI gating, fix `Default` fallbacks, apply §5.5 fixes) → M1 backfill (export local tenants → `POST /tenants` on MMS, keep local→master ID map, replay into projection) → M2 freeze writers (feature-flag OFF the UMS tenant write path) → M3 switch reads (authz reads from `masterdata.tenant_projection`) → M4 contract (delete local write paths, then local aggregates). Interim rule: between M0 and M2, local tenant creation is dev/demo-only by policy.
- **Acceptance criteria:**
  - [ ] Local tenant write paths removed (`CreateTenantCommand`/`TenantEndpoints` write surface retired or dev-gated per phase).
  - [ ] Authorization reads tenant identity from `masterdata.tenant_projection`.
  - [ ] 24 h reconciliation shows zero drift between the projection and MMS master.
  - [ ] ADR-0083 (UMS tenant ownership) marked Accepted.
- **Dependencies:** MMS (authority — MMS becomes the ID authority in M1); [DS-04](#ds-04), [DS-05](#ds-05), [DS-07](#ds-07) (all M0 prerequisites).
- **Status:** `PENDING` — deployment strategy §15 risk #1; not yet addressed beyond the plan.

#### DS-04

**Title:** Dedup inbox not actually wired in the projection consumer.

- **Purpose:** The tenant-projection consumer must be idempotent under redelivery. MassTransit's EF inbox provides at-least-once dedup by `messageId`, but the inbox is only active when the **endpoint** enables it — the bus-level `AddEntityFrameworkOutbox` alone does not wire the consumer inbox.
- **Evidence:** §5.5-1 — "both repos call `AddEntityFrameworkOutbox<TenantProjectionDbContext>()` at bus level but the consumer definitions never call `endpointConfigurator.UseEntityFrameworkOutbox<TenantProjectionDbContext>(context)` — `InboxState` exists but is never consulted." Verified: `src/apps/ums.api/Ums.Infrastructure/DependencyInjection.cs:182` calls `AddEntityFrameworkOutbox<TenantProjectionDbContext>` at bus level; `TenantProjectionConsumerDefinition` does not call `UseEntityFrameworkOutbox`.
- **Impact:** Redelivered messages are reprocessed with no dedup guard; the consumer's idempotency claim rests only on the versioned upsert (which is itself racy — see [DS-05](#ds-05)).
- **Risk:** Duplicate processing on broker redelivery; `InboxState` never populated → G1 "InboxState row written on consume" assertion (§13) fails.
- **Affected files:** `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionConsumerDefinition.cs`; `src/apps/ums.api/Ums.Infrastructure/DependencyInjection.cs` (bus config, line ~182).
- **Component:** `Messaging` · **Phase:** Release · **Type:** validación
- **Criticality:** P1 · **Complexity:** S
- **Proposed fix:** In `TenantProjectionConsumerDefinition.ConfigureConsumer`, call `endpointConfigurator.UseEntityFrameworkOutbox<TenantProjectionDbContext>(context)` so the inbox is consulted on every consume.
- **Acceptance criteria:**
  - [ ] Inbox is consulted on consume (an `InboxState` row is written when a message is consumed).
- **Dependencies:** none.
- **Status:** `PENDING` — deployment strategy §15 risk #4.

#### DS-05

**Title:** Read-check-write race in the versioned upsert (permanent regression).

- **Purpose:** The projection upsert must be monotonic per tenant: a stale event must never overwrite a newer projected version. A read-then-write upsert without a concurrency token allows two in-flight events for one tenant to interleave and permanently regress the projection.
- **Evidence:** §5.5-2 — "the versioned upsert has no concurrency token; two in-flight events for one tenant can permanently regress the projection." The `TenantProjectionConsumer` documents order-tolerance ("applies only when the incoming sequence is newer than the stored version") but implements it as read-check-write rather than a single set-based conditional statement.
- **Impact:** Under concurrent delivery the stored projection can be silently rolled back to an older tenant state.
- **Risk:** Permanent data regression in `masterdata.tenant_projection`; violates the "freshness only, never correctness" degradation guarantee (§5.6); breaks 24 h zero-drift reconciliation.
- **Affected files:** `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionConsumer.cs`; `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionDbContext.cs`.
- **Component:** `Messaging` · **Phase:** Release · **Type:** validación
- **Criticality:** P1 · **Complexity:** S
- **Proposed fix:** Replace the read-check-write with a set-based conditional write: `INSERT … ON CONFLICT (tenant_id) DO UPDATE SET … WHERE tenant_projection.version < EXCLUDED.version` (cheapest; also removes a round-trip).
- **Acceptance criteria:**
  - [ ] Upsert is a single version-conditional statement.
  - [ ] A reordering test (older event after newer) does not regress the stored projection.
- **Dependencies:** none.
- **Status:** `PENDING` — deployment strategy §15 risk #5.

#### DS-07

**Title:** `Default`/`DefaultConnection` fallback → the projection context silently targets localhost.

- **Purpose:** The projection `DbContext` must fail closed when its connection string is missing, not silently fall back to a hardcoded localhost. The suite standardizes the key `MasterDataDb` for the projection store.
- **Evidence:** §5.5-4 — "the projection context silently targets hardcoded localhost when `MasterDataDb` is unset — always set `ConnectionStrings__MasterDataDb` explicitly in charts and fix the fallback." Verified: `src/apps/ums.api/Ums.Infrastructure/DependencyInjection.cs:168` resolves `GetConnectionString("MasterDataDb")` for the projection context.
- **Impact:** With `MasterDataDb` unset the consumer writes to the wrong database (localhost) with no error, so the projection appears empty/stale in the real store.
- **Risk:** Silent misconfiguration in staging/prod; the projection diverges from MMS with no signal; hard to diagnose.
- **Affected files:** `src/apps/ums.api/Ums.Infrastructure/DependencyInjection.cs` (MasterData context registration, line ~168); the UMS Helm chart values/secret templates under `infra/local/kubernetes/helm`.
- **Component:** `Infra` · **Phase:** Release · **Type:** arquitectura
- **Criticality:** P1 · **Complexity:** S
- **Proposed fix:** Remove the silent localhost fallback (fail fast when `MasterDataDb` is unset) and set `ConnectionStrings__MasterDataDb` explicitly in the chart.
- **Acceptance criteria:**
  - [ ] No silent fallback: startup fails clearly when `MasterDataDb` is unset.
  - [ ] `MasterDataDb` is set explicitly in the UMS chart.
- **Dependencies:** none.
- **Status:** `PENDING` — deployment strategy §15 risk #7.

#### DS-08

**Title:** Startup migrations race at `replicas>1`.

- **Purpose:** Schema migrations must run exactly once per rollout, decoupled from process startup, so scaling the API/consumer to multiple replicas does not race concurrent migrators.
- **Evidence:** §5.5-3 / §10 — "Startup migrations race at replicas>1"; the strategy mandates the migrate-Job Helm-hook pattern (Tracker's `migrate-job.yaml`) suite-wide, "never at startup".
- **Impact:** Concurrent `MigrateAsync` on multiple replicas can deadlock or partially apply migrations.
- **Risk:** Failed/partial migrations, CrashLoop on rollout, non-idempotent startup at scale.
- **Affected files:** UMS host startup / migrator wiring (`src/apps/ums.api/Ums.Infrastructure/DependencyInjection.cs` migration invocation and host bootstrap); the UMS Helm chart under `infra/local/kubernetes/helm` (add a migrate-Job hook template).
- **Component:** `Infra` · **Phase:** Release · **Type:** arquitectura
- **Criticality:** P1 · **Complexity:** M
- **Proposed fix:** Adopt the Tracker migrate-Job Helm-hook pattern; run migrations in a pre-install/pre-upgrade Job and make process startup idempotent (no `MigrateAsync` at boot).
- **Acceptance criteria:**
  - [ ] Migrations run via a Helm migrate-Job, not at process startup.
  - [ ] Startup is idempotent at `replicas>1`.
- **Dependencies:** none.
- **Status:** `PENDING` — deployment strategy §15 risk #8.

#### DS-12

**Title:** Consume the shared contract package instead of the local `TenantEvent` copy.

- **Purpose:** The wire contract is defined by the shared package `Evolith.Messaging.Contracts` (published from the MMS repo). MassTransit routes by namespace+type, so the namespace **`Evolith.Contracts.MasterData`** *is* the contract. UMS must consume the package, not a verbatim local copy, so all three consumers stay in lockstep with the producer.
- **Evidence:** §11 / §15 #12 — "Contract drift across 3 TenantEvent copies"; the package "replaces the three verbatim copies." Verified: UMS keeps a local copy at `src/apps/ums.api/Ums.Infrastructure/MasterData/Contracts/TenantEvent.cs`.
- **Impact:** The local copy can drift from the producer's schema; additive/breaking changes are not coordinated through one package.
- **Risk:** Silent deserialization mismatches; namespace/type divergence breaks MassTransit routing; contract drift discovered late in integration.
- **Affected files:** `src/apps/ums.api/Ums.Infrastructure/MasterData/Contracts/TenantEvent.cs`; `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionConsumer.cs` (consumes the contract type).
- **Component:** `Contracts` · **Phase:** Cross · **Type:** integración
- **Criticality:** P2 · **Complexity:** M
- **Proposed fix:** Reference the NuGet `Evolith.Messaging.Contracts` (namespace `Evolith.Contracts.MasterData`); delete the local copy; a consumer version subscribes to exactly one schema major; add contract tests that deserialize the committed producer fixtures through the real consumer path.
- **Acceptance criteria:**
  - [ ] Local `TenantEvent` copy replaced by the shared package.
  - [ ] Consumer deserializes the producer's committed fixtures (contract test green).
- **Dependencies:** MMS `DS-12` (MMS publishes the `Evolith.Messaging.Contracts` package).
- **Status:** `PENDING` — deployment strategy §15 risk #12.

#### DS-15

**Title:** Consumer behavior is unobservable — missing `AddSource("MassTransit")`.

- **Purpose:** The projection consumer must emit traces and standard meters so lag, applied/discarded counts, and error-queue depth are visible — a prerequisite for the G2 staging soak.
- **Evidence:** §14 / §15 #15 — "UMS `ObservabilityExtensions` misses `AddSource("MassTransit")` (consumer spans invisible)"; standard meters required: `masterdata_projection_applied/discarded_total`, consumer lag, `_error` queue depth, e2e latency histogram.
- **Impact:** Consumer spans never reach the collector; no signal to analyze consume behavior, lag, or poison-message handling.
- **Risk:** Blind operation of the projection path; no basis for canary/SLO analysis (§10 blue-green "not yet" is itself gated on these spans); G2 prerequisites unmet.
- **Affected files:** UMS `ObservabilityExtensions` / OTel tracing registration (under `src/apps/ums.api/Ums.Infrastructure` or `Ums.Presentation`); `src/apps/ums.api/Ums.Infrastructure/MasterData/TenantProjectionConsumer.cs` (meter emission points).
- **Component:** `Observability` · **Phase:** Cross · **Type:** funcional
- **Criticality:** P2 · **Complexity:** M
- **Proposed fix:** Add `AddSource("MassTransit")` to the tracing configuration and emit the standard meters (`masterdata_projection_applied/discarded_total`, consumer lag, `_error` queue depth).
- **Acceptance criteria:**
  - [ ] Consumer spans are visible (traces reach the collector).
  - [ ] Standard projection meters are exposed.
- **Dependencies:** none.
- **Status:** `PENDING` — deployment strategy §15 risk #15.

---

## References

Evolith suite deployment strategy: `evolith/product/suite/architecture/evolith-suite-deployment-strategy.md` (§5.5 consumer-correctness fixes · §11 contract & event versioning · §12 tenant-ownership migration M0–M4 · §14 observability · §15 consolidated risk register). ADR-0106 (MMS tenant mastership) · ADR-0107 · UMS ADR-0083 · MMS `docs/architecture/tenant-master-data-projection.md` (canonical flow).

---
[Back to Gap Tracking Board](./ums-gap-tracking.md)
