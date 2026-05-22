# IGA BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/iga/index.md)

**Bounded Context:** Identity Governance & Administration (`Ums.Domain.IGA`)  
**Aggregate Roots:** `PromotionRequest`, `RoleMaturityStatus`

---

### Identity & Access Promotion Lifecycle
Governs secure role-to-role promotion, maturity leveling, and automated toxic access risk analyses:
- [PromotionRequest](./promotion-request.md) (Aggregate Root) — Handles draft creation, manager approvals, risk assessments, security checks, and verified role executions.
- [PromotionImpactAnalysis](./promotion-impact-analysis.md) (Owned Entity) — Logs dynamic toxic-permission risk scores and affected systems.
- [RoleMaturityStatus](./role-maturity-status.md) (Aggregate Root) — Manages performance thresholds, compliance reviews, certification counters, and promotion-eligibility criteria per role maturity level (Junior $\rightarrow$ Principal).

---

**[Back to Domain Index](../index.md)**
