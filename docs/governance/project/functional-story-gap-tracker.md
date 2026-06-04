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
| Implemented / usable | 21 | FS-01, FS-02, FS-03, FS-04, FS-05, FS-06, FS-07, FS-08, FS-09, FS-10, FS-11, FS-15, FS-16, FS-17, FS-18, FS-19, FS-20, FS-21, FS-22, FS-26, FS-27 |
| Partial | 6 | FS-12, FS-13, FS-14, FS-23, FS-24, FS-25 |
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
| FS-12 | Execute Role Promotion Process | Amber | P1 | H | H | IGA | TBD | Open | The promotion flow still needs the full manager/security review, execution, verification, and impact analysis closure. | Finish the promotion state machine and align the approval steps with the domain contract. |
| FS-13 | Configure Hierarchical System Parameters | Amber | P1 | H | H | Platform / Configuration | TBD | Open | Parameterization exists, but the formal Configuration context is still missing its complete API surface. | Implement the `AppConfiguration`, `FeatureFlag`, and `IdpConfiguration` APIs end to end. |
| FS-14 | Delegate User Management Between Administrators | Amber | P2 | M | M | Identity | TBD | Open | Delegation exists as a model, but the end-to-end scope and audit flow still need final validation. | Close the delegated action coverage and verify the acceptance path. |
| FS-23 | Profile Access Request from Lobby User | Amber | P1 | H | H | Approvals | TBD | Open | The request model still needs the requested role and audit fidelity expected by the design. | Extend the request contract and lifecycle tracking. |
| FS-24 | Profile Request Approval and Manual Assignment | Amber | P1 | H | H | Approvals | TBD | Open | The decision record still needs requested role, granted role, reason, and notification result coverage. | Extend the approval result payload and persistence model. |
| FS-25 | Manage Domain Resources with DDD Hierarchy | Amber | P2 | L | L | Architecture | TBD | Open | This story is mostly an architecture-to-domain traceability target, and the implementation mapping is still not fully explicit. | Add a concrete implementation map or narrow the scope if the story remains documentation-only. |

## Review Cadence

- Update this tracker whenever a story moves in the backlog or the implementation tracker changes.
- Re-audit the open gaps after any domain, API, or documentation change affecting the listed stories.
- Keep the English and Spanish versions synchronized in structure and content.

## Last Review

2026-06-04 (FS-20)
