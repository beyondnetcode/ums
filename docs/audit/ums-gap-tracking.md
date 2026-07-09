# Evolith UMS — Gap Tracking Board

> **Bilingual Navigation:** this board is bilingual-by-content (mixed EN/ES); the canonical machine fields (status, criticality, complexity) are English.

**Status:** Active Tracking  
**Owner:** Evolith UMS Team  
**Last Updated:** 2026-07-09 (Deployment Strategy Hardening wave `DS-*` seeded — 7 items traced to the Evolith suite deployment strategy §15 consolidated risk register: two-writer tenant ownership, consumer-correctness defects in the tenant-projection path, contract-copy drift, and consumer observability. All items open — no closures yet.)  
**Gap Details:** [Gap Reference Catalog](./ums-gap-reference-catalog.md)  
**Closure Authority:** [Gap Closure Evidence Standard](./ums-gap-closure-evidence-standard.md) · [`ums-gap-closure-evidence.json`](./ums-gap-closure-evidence.json)  
**Maturity:** [`ums-maturity-reconciliation.json`](./ums-maturity-reconciliation.json)

This board is the single source of truth for UMS technical debt, gaps, opportunities, coherence findings, priority, and status. Select a gap ID to open its problem, purpose, evidence, closure criteria, and references in the catalog.

> One table with every gap. `*-*` IDs link to their full detail in the [catalog](./ums-gap-reference-catalog.md). Order: pending first (by criticality `P0`→`P3`, then complexity `XS`→`XL`), then completed. GitHub renders Markdown statically (no interactive sort/search): the **Component** column categorizes and GitHub file search (`/`) finds an ID or term.

> **ID series:** `DS-*` **Deployment Strategy Hardening** wave (2026-07-09) — items traced to the Evolith suite deployment strategy (`evolith/product/suite/architecture/evolith-suite-deployment-strategy.md`) §15 consolidated risk register, covering the tenant master-data projection seam, the two-writer tenant-ownership migration, event-contract packaging, and consumer observability. **Criticality:** `P0` critical · `P1` high · `P2` medium · `P3` low. **Complexity:** `XS`/`S`/`M`/`L`/`XL`. **Status:** `PENDING` · `IN-PROGRESS` · `BLOCKED` · `DEFERRED` · `DONE`.

| ID | Gap | Component | Phase | Criticality | Complexity | Status |
|---|---|:---:|:---:|:---:|:---:|:---:|
| [`DS-01`](./ums-gap-reference-catalog.md#ds-01) | UMS aún autora tenants localmente contra la maestría de MMS (estado de dos-escritores): `CreateTenantCommand` + `TenantEndpoints` escriben tenants mientras MMS es el writer-of-record (ADR-0106). Requiere la escalera de migración de ownership M0–M4 hasta leer de `masterdata.tenant_projection` y borrar las rutas de escritura. | `Backend` | Cross | P0 | L | `PENDING` |
| [`DS-04`](./ums-gap-reference-catalog.md#ds-04) | 🔧 En progreso — inbox cableado en la ConsumerDefinition (commit 73f6b1e5, UMS develop); pendiente verificación viva (fila InboxState) en gate G1. Inbox de dedup no cableado en el consumidor de proyección: se llama `AddEntityFrameworkOutbox<TenantProjectionDbContext>()` a nivel de bus pero la consumer definition nunca llama `UseEntityFrameworkOutbox<TenantProjectionDbContext>(context)`; `InboxState` existe pero nunca se consulta. | `Messaging` | Release | P1 | S | `IN-PROGRESS` |
| [`DS-05`](./ums-gap-reference-catalog.md#ds-05) | 🔧 En progreso — upsert condicional set-based (commit 73f6b1e5); **SQL probado en vivo contra Postgres real** (descarte de stale/duplicado/fuera-de-orden); pendiente prueba viva dentro de la tx de inbox en gate G1. Carrera read-check-write en el upsert versionado (regresión permanente): sin token de concurrencia, dos eventos en vuelo del mismo tenant pueden regresar la proyección. Requiere escritura condicional set-based `ON CONFLICT … DO UPDATE … WHERE version < EXCLUDED.version`. | `Messaging` | Release | P1 | S | `IN-PROGRESS` |
| [`DS-07`](./ums-gap-reference-catalog.md#ds-07) | 🔧 En progreso — guardia de fallback aplicada en el working tree de DependencyInjection.cs (convive con WIP del usuario, aún sin commitear); build-clean. Fallback `Default`/`DefaultConnection` → el contexto de proyección apunta a localhost en silencio cuando falta `MasterDataDb`; el DI cae a un connection string por defecto. Requiere corregir el fallback + `ConnectionStrings__MasterDataDb` explícito en el chart. | `Infra` | Release | P1 | S | `IN-PROGRESS` |
| [`DS-08`](./ums-gap-reference-catalog.md#ds-08) | Migración al arranque compite con `replicas>1`: aplicar migraciones en el boot del proceso condiciona con varias réplicas. Requiere el patrón migrate-Job (como Tracker). | `Infra` | Release | P1 | M | `PENDING` |
| [`DS-12`](./ums-gap-reference-catalog.md#ds-12) | Consumir el contrato compartido en vez de la copia local de `TenantEvent`: hoy hay una copia local del envelope. Requiere consumir el NuGet `Evolith.Messaging.Contracts` (namespace `Evolith.Contracts.MasterData`) con contract tests contra fixtures del productor. | `Contracts` | Cross | P2 | M | `PENDING` |
| [`DS-15`](./ums-gap-reference-catalog.md#ds-15) | Consumo no observable: falta `AddSource("MassTransit")`, los spans del consumidor son invisibles y no se exponen las métricas estándar de proyección. Requiere `AddSource("MassTransit")` + meters `masterdata_projection_applied/discarded_total`, lag y profundidad `_error`. | `Observability` | Cross | P2 | M | `PENDING` |

**Progress:** 0 / 7 done · 3 in progress · 4 pending · 0 blocked · 0 deferred

**Wave 2026-07-09 (Deployment Strategy Hardening `DS-01`…`DS-15`):** Items sourced from the Evolith suite deployment strategy (`evolith/product/suite/architecture/evolith-suite-deployment-strategy.md`) as it pertains to UMS: the tenant master-data **projection consumer** (§5.5 verified consumer-correctness defects), the **two-writer tenant-ownership** migration (§12, the #1 architectural risk), the shared **event-contract package** (§11), and **consumer observability** (§14). IDs map 1:1 to the §15 consolidated risk register rows (`DS-01`↔risk #1, `DS-04`↔#4, `DS-05`↔#5, `DS-07`↔#7, `DS-08`↔#8, `DS-12`↔#12, `DS-15`↔#15). All 7 items imported `PENDING`; no closures recorded (`ums-gap-closure-evidence.json` → `records: []`).

**Ordering:** one table, ordered by status (active then completed), then criticality (`P0`→`P3`), then complexity (`XS`→`XL`). IDs link to the [Gap Reference Catalog](./ums-gap-reference-catalog.md).

---
[Back to Docs](../README.md)
