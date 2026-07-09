# Gap Closure Evidence Standard — Evolith UMS

> **Bilingual Navigation:** [Versión en Español](./ums-gap-closure-evidence-standard.es.md)

**Status:** Active
**Owner:** Evolith UMS Team
**Design Origin:** mirrors the Evolith Tracker [Gap Closure Evidence Standard](https://github.com/beyondnetcode/evolith_tracker/blob/main/docs/audit/tracker-gap-closure-evidence-standard.md), which mirrors the Evolith Core standard.
**Machine-Readable Registry:** [`ums-gap-closure-evidence.json`](./ums-gap-closure-evidence.json)

## 1. Purpose

This standard makes a completed gap an evidence-backed governance claim. A consistent board row is necessary but not sufficient: closure must be reproducible from repository history and resolvable artifacts. It governs the [Gap Tracking Board](./ums-gap-tracking.md) and the [Gap Reference Catalog](./ums-gap-reference-catalog.md).

## 2. Required Closure Record

Every gap marked `DONE` should have exactly one entry in the canonical registry with:

| Field | Requirement |
|---|---|
| `id` | Existing gap identifier present in the board and catalog (`DS-*`) |
| `closedAt` | ISO date that is not in the future |
| `closureCommit` | Existing Git commit containing or establishing the closure |
| `evidence` | One or more repository-relative files that demonstrate the result |
| `validationCommands` | One or more reproducible commands used to validate the result |
| `dependencyDisposition` | `none`, `satisfied`, `accepted-scope`, or `deferred` |
| `dependencyRationale` | Required whenever disposition is not `none` |

`IN-PROGRESS` is reserved for gaps addressed at the spec/governance level whose code/infrastructure implementation is still pending (mirrors Core's distinction between a documented decision and a shipped capability).

## 3. Semantic Enforcement (intended)

A `validate-tracking` harness check (to be wired under `.harness/scripts/` or the UMS CI) should fail when:

1. a completed gap has no closure record;
2. a closure record points to a missing gap, commit, or evidence file;
3. a completed catalog section contains an unchecked `- [ ]` acceptance criterion;
4. closure metadata is incomplete, duplicated, future-dated, or uses an unsupported dependency disposition;
5. the Board and Catalog disagree on ID set or status.

Pending, in-progress, and deferred gaps must not have active closure records. Historical rationale remains in the catalog.

## 4. Closure Workflow

1. Complete and validate the scoped work.
2. Commit the implementation or documentation evidence.
3. Add the closure record using that real commit.
4. Resolve every acceptance-criteria checkbox in the catalog.
5. Change the board status to `DONE`.
6. Run tracking, documentation, and bilingual validation.

No placeholder commit, speculative evidence, or waived checkbox may be used to satisfy closure.

## 5. Dependency Dispositions

`DS-*` items carry cross-repo dependencies (notably on MMS as the tenant writer-of-record). When a gap closes with an unresolved or externally-owned dependency, record the disposition (`satisfied`, `accepted-scope`, or `deferred`) and a rationale — for example, closing `DS-12` (consume the shared contract package) depends on MMS publishing `Evolith.Messaging.Contracts`, and `DS-01` (tenant ownership migration) depends on MMS as the authority across the M0–M4 ladder.

---
[Back to Gap Tracking Board](./ums-gap-tracking.md)
