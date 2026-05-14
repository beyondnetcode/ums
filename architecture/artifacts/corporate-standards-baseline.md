# Corporate Standards Baseline — UMS Project

- **Status:** Planning Reference
- **Date:** 2026-05-13
- **Source:** arc32_progresive_monolith (v1.0)

---

## Purpose

This document maps every Architectural Decision Record (ADR) in the UMS project against its counterpart in the [Corporate Reference Architecture (arc32)](https://github.com/beyondnetcode/arc32_progresive_monolith). It classifies each into one of three categories and defines which documentation is genuinely local vs. inheritable by reference.

---

## 1. Classification Legend

| Category | Badge | Meaning | Action |
|---|---|---|---|
| **BY_REFERENCE** | `[]` | UMS ADR is a verbatim adoption of a corporate standard. No project-specific nuance. | Reduce UMS ADR to a stub that links to arc32. Keep only in `corporate-standards-baseline.md`. |
| **ADAPTED** | `[]` | UMS ADR adopts the corporate standard but modifies it for .NET runtime or domain-specific needs. | Keep UMS ADR but add `Based-on: arc32-ADR-NNN` header. Document only the delta. |
| **LOCAL** | `[]` | Decision is specific to the UMS domain. No corporate counterpart exists. | Keep full UMS ADR as-is.
## 2. Full ADR Mapping

### Tier 0: Corporate Universal Core (Runtime Agnostic)

| UMS ADR | Title | arc32 Source | Classification | Rationale |
|---|---|---|---|---|
| 0001 | Monorepo Orchestáration (Nx) | arc32-0001 (core) | `[]` BY_REFERENCE | Same tool, same topology. Nx configuration is standard. |
| 0005 | CI/CD Quality (CodeQL) | arc32-0005 (core) | `[]` BY_REFERENCE | Pipeline configuration is verbatim corporate standard. |
| 0006 | Future Microservices (Dapr) | arc32-0006 (core) | `[]` BY_REFERENCE | Strategic direction, no project-specific deviation. |
| 0009 | Strict Dependency Pinning | arc32-0009 (core) | `[]` BY_REFERENCE | Security policy, verbatim adoption. |
| 0010 | Multi-Tenancy RLS Strategy | arc32-0010 (core) | `[]` ADAPTED | Corporate defines hybrid pooled. UMS adds hierarchical model (ADR-0034) and closure table. Keep as adaptation record. |
| 0011 | Fault Tolerance & Resiliency | arc32-0011 (core) | `[]` BY_REFERENCE | Circuit breaker / retry patterns are standard. |
| 0013 | Cloud Topology & DR | arc32-0013 (core) | `[]` BY_REFERENCE | Infrastructure strategy is corporate mandate. |
| 0014 | Distributed Caching (Redis) | arc32-0014 (core) | `[]` BY_REFERENCE | Cache topology is standard. |
| 0015 | Event-Driven Architecture | arc32-0015 (core) | `[]` BY_REFERENCE | Event bus pattern is standard. |
| 0016 | Immutable Business Audit Trail | arc32-0016 (core) | `[]` ADAPTED | Corporate defines row-level + ledger. UMS adds hierarchical context (who delegated what in which tenant chain). Keep as adaptation. |
| 0017 | Feature Flagging Strategy | arc32-0017 (core) | `[]` BY_REFERENCE | Flag evaluation model is standard. |
| 0018 | Testing Pyramid Quality Gates | arc32-0018 (core) | `[]` BY_REFERENCE | Coverage thresholds and pyramid definition are corporate mandate. |
| 0019 | Tactical Design Patterns | arc32-0019 (core) | `[]` BY_REFERENCE | Result Pattern, Null Object, etc. are standard. |
| 0020 | IdP Abstraction Strategy | arc32-0020 (core) | `[]` BY_REFERENCE | IdP adapter pattern is corporate standard. |
| 0024 | Config & Feature Management | arc32-0024 (core) | `[]` BY_REFERENCE | Configuration platform strategy is corporate. |
| 0025 | Feature Flag Provider Abstraction | arc32-0025 (core) | `[]` BY_REFERENCE | Provider abstraction is standard. |
| 0028 | Self-Hosted OSS Infrastructure | arc32-0028 (core) | `[]` BY_REFERENCE | Deployment topology is corporate mandate. |
| 0030 | API Gateway (Kong) | arc32-0030 (core) | `[]` BY_REFERENCE | Gateway selection is corporate. | ### Tier 1: Runtime-Specific (Node.js/.NET/React)

| UMS ADR | Title | arc32 Source | Classification | Rationale |
|---|---|---|---|---|
| **0002** | Clean Architecture | arc32-0002 (nodejs) | `[]` ADAPTED | arc32-0002 defines NestJS architecture. UMS uses .NET. The principles are identical but implementation differs. Keep as .NET adaptation reference. |
| **0003** | Strict TypeScript Standards | arc32-0003 (nodejs) | `[]` ADAPTED | arc32-0003 covers Node.js/TS. UMS uses TS only in frontend (React). Backend is C#. Keep restricted to frontend scope. |
| **0004** | Frontend Offline Resilience | arc32-0004 (nodejs) | `[]` BY_REFERENCE | React state mgmt (Zustand + TanStack Query) matches corporate standard for frontend. |
| **0007** | Observability (OTel/Grafana) | arc32-0007 (nodejs) | `[]` ADAPTED | Corporate defines Node.js OTel. UMS uses .NET OpenTelemetry SDK. Same protocol (OTLP), different SDK. |
| **0008** | Progressive BFF Evolution | arc32-0008 (nodejs) | `[]` ADAPTED | Corporate defines NestJS BFF. UMS uses .NET 8 for BFF. Same pattern, different runtime. |
| **0012** | RBAC/ABAC Authorization | arc32-0012 (nodejs) | `[]` ADAPTED | Corporate defines NestJS Guards + Decorators. UMS implements via .NET middleware (ADR-0036, 0039). Merged into local ADRs. |
| **0021** | Auth Graph Compilation | arc32-0021 (nodejs) | `[]` ADAPTED | Merged into ADR-0039 (Policy Compilation Engine). UMS-specific implementation. |
| **0022** | Contextual Projections | arc32-0022 (nodejs) | `[]` ADAPTED | Same concept, .NET implementation. |
| **0023** | Centralized Kernel Boundary | arc32-0023 (nodejs) | `[]` ADAPTED | UMS-specific domain design, but follows corporate centralized kernel principle. |
| **0026** | MFA Adaptive Auth | arc32-0026 (nodejs) | `[]` BY_REFERENCE | MFA strategy is runtime-agnostic. Same policies apply. |
| **0027** | Dual-Protocol REST/gRPC | arc32-0027 (nodejs) | `[]` ADAPTED | Corporate defines Node.js gRPC setup. UMS uses .NET gRPC (Grpc.AspNetCore). |
| **0029** | Tactical DDD Primitives | arc32-0029 (nodejs) | `[]` ADAPTED | Corporate defines TS DDD primitives. UMS uses C# records, value objects. |
| **0033** | Minimal APIs (.NET) | arc32-0048 (dotnet) | `[]` BY_REFERENCE | Direct .NET counterpart. Identical strategy. | ### Tier 2: UMS-Local Domain Decisions

| UMS ADR | Title | arc32 Source | Classification | Rationale |
|---|---|---|---|---|
| **0031** | Identity Domain Abstraction (Subject) | arc32-0031 (core, Schema Per Context) | `[]` LOCAL | arc32-0031 defines schema-per-context. UMS-0031 defines Subject as identity abstraction. Conceptually related but domain-specific. Keep. |
| **0032** | Organization as Strategic Boundary | — | `[]` LOCAL | UMS-specific domain design. No corporate equivalent. Defines Organization as governance root. Keep. |
| **0034** | Hierarchical Multi-Tenancy (Closure + Taxonomy) | — | `[]` LOCAL | Domain-specific implementation choice. Corporate defines RLS strategy; UMS defines the hierarchical extension. Keep. |
| **0035** | Hybrid Policy Inheritance Engine | — | `[]` LOCAL | 4-mode inheritance (MANDATORY/DEFAULT/OPT_IN/NONE) is UMS-specific. Keep. |
| **0036** | Anti-Privilege Escalation | — | `[]` LOCAL | 5-invariant validation pipeline is UMS-specific. Keep. |
| **0037** | Tenant-Aware Partitioning | arc32-0031 (core, Schema Per Context) | `[]` LOCAL | Related to corporate schema-per-concept but UMS-specific implementation (LIST partitioning by root_tenant_id). Keep. |
| **0038** | Delegated Admin (Temporal Scopes) | — | `[]` LOCAL | Delegation DAG model is UMS-specific. No corporate equivalent. Keep. |
| **0039** | RBAC/ABAC Policy Compilation | arc32-0021 (nodejs) | `[]` LOCAL | Policy compilation engine design is UMS-specific. Corporate defines the concept but not the implementation. Keep. |
| **0040** | Federated Token Strategy | — | `[]` LOCAL | Dual-mode token (JWT + Opaque) is UMS-specific for hierarchical context. No corporate equivalent. Keep.
## 3. Deprecation / Consolidation Plan

### Phase 1: Reduce 18 BY_REFERENCE ADRs to Stubs

The following UMS ADRs should be reduced to a single-page stub containing only:

```markdown
# ADR-NNN: [Title]

* **Status:** Accepted (Incorporated by Reference)
* **Date:** 2026-05-08
* **Corporate Source:** [arc32-ADR-NNN](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/...)

## Decision

This project adopts the corporate standard verbatim as defined in the source above. No project-specific adaptation is required.

## Project-Specific Notes

- Implementation details: see `docs/en/artifacts/corporate-standards-baseline.md`
- Deviation tracking: any future deviation MUST be recorded as a new LOCAL ADR referencing this one.
```

**ADRs to stub:**
0001, 0005, 0006, 0009, 0011, 0013, 0014, 0015, 0017, 0018, 0019, 0020, 0024, 0025, 0028, 0030, 0004, 0033

### Phase 2: Condense 11 ADAPTED ADRs to Delta-Only

The following UMS ADRs keep their file but strip all content that duplicates the corporate source. Only the delta (.NET adaptation, UMS-specific nuance) remains.

**ADRs to condense:**
0002, 0003, 0007, 0008, 0010, 0012, 0016, 0021, 0022, 0023, 0026, 0027, 0029

Format for each condensed ADR:

```markdown
# ADR-NNN: [Title]

* **Status:** Accepted
* **Based on:** [arc32-ADR-NNN](...)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. [Runtime difference]: .NET 8 instead of NestJS — [specific change]
2. [Domain difference]: [specific change]
3. [Exclusion]: [something deferred]

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
```

### Phase 3: Keep 9 LOCAL ADRs as Full Documents

0031, 0032, 0034, 0035, 0036, 0037, 0038, 0039, 0040

---

## 4. Resulting Documentation Inventory

| State | Count | Files |
|---|---|---|
| `[]` BY_REFERENCE (stub) | 18 | 0001, 0004, 0005, 0006, 0009, 0011, 0013, 0014, 0015, 0017, 0018, 0019, 0020, 0024, 0025, 0028, 0030, 0033 |
| `[]` ADAPTED (delta) | 13 | 0002, 0003, 0007, 0008, 0010, 0012, 0016, 0021, 0022, 0023, 0026, 0027, 0029 |
| `[]` LOCAL (full) | 9 | 0031, 0032, 0034, 0035, 0036, 0037, 0038, 0039, 0040 |
| **Total UMS ADRs** | **40** | After consolidation
## 5. Governance Rules (Going Forward)

### Rule G1 — Source of Truth Hierarchy

```
arc32 (Corporate Reference)
   Mandatory compliance: ALL BY_REFERENCE
   Default compliance: ALL ADAPTED (delta-only deviations must be approved)
        UMS Project ADRs
             LOCAL ADRs are binding for UMS only
             If a LOCAL ADR later becomes corporate-wide, it must be promoted to arc32
```

### Rule G2 — New ADR Creation

Before creating any new UMS ADR:
1. Check if arc32 has a relevant corporate ADR.
2. If YES and UMS follows it verbatim → declare `BY_REFERENCE`, do NOT create a new UMS ADR.
3. If YES but UMS needs adaptation → create ADAPTED ADR with `Based-on:` header and delta-only content.
4. If NO → create LOCAL ADR, consider whether it should be proposed to arc32.

### Rule G3 — Corporate Reference Updates

When arc32 updates an ADR that UMS references:
- `BY_REFERENCE`: No action needed (the reference is live). Inform team of change.
- `ADAPTED`: Review if the adaptation delta is still valid. Update if needed.
- `LOCAL`: Not affected.

### Rule G4 — Documentation vs. Code

- This baseline document and the ADR stubs/deltas are **design governance**, not code.
- The actual implementation (code in `src/ums-workspace/apps/`) is governed by these decisions but is NOT duplicated documentation.
- If an implementation decision changes, the ADR MUST be updated FIRST (ADR-first governance).

---

## 6. Next Steps

| Step | Owner | Effort |
|---|---|---|
| 1. Approve this classification (PO + Architect) | PO + Architect | 1 session |
| 2. Stub 18 BY_REFERENCE ADRs | Tech Writer / AI | 1 day |
| 3. Condense 13 ADAPTED ADRs to delta-only | Tech Writer / AI | 2 days |
| 4. Update MASTER_INDEX.md to reflect new ADR inventory | Tech Writer | 0.5 day |
| 5. Establish sync cadence with arc32 releases | Architect | Ongoing | 