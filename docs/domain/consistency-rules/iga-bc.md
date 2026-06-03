# IGA BC — Consistency Rules

> **Bounded Context:** `Ums.Domain.IGA`
> **Aggregates:** `PromotionRequest`, `RoleMaturityStatus`

---

## PromotionRequest

### State Machine

```
Draft ──Submit()──► PendingManagerApproval
PendingManagerApproval ──ManagerApprove()──► PendingSecurityReview
PendingManagerApproval ──ManagerReject()──► Rejected (terminal)
PendingSecurityReview ──SecurityReviewLowRisk()──► PendingSecurityApproval
PendingSecurityReview ──SecurityReviewHighRisk()──► PendingSecurityApproval
PendingSecurityApproval ──SecurityApprove()──► ApprovedReadyToExecute
PendingSecurityApproval ──SecurityReject()──► Rejected (terminal)
ApprovedReadyToExecute ──Execute()──► Executed
Executed ──Verify()──► Verified (terminal)
Executed ──MarkVerificationFailed()──► VerificationFailed
VerificationFailed ──(Reject)──► Rejected (terminal) [planned]
```

**Note:** `Cancel()` from `Draft` is a planned operation, not yet implemented.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Submit()` | Status must be `Draft` | `iga.promotion_not_in_draft` |
| `ManagerApprove()` / `ManagerReject()` | Status must be `PendingManagerApproval` | `iga.promotion_not_pending_manager` |
| `SecurityReviewLowRisk()` / `SecurityReviewHighRisk()` / `SecurityApprove()` / `SecurityReject()` | Status must be in pending-security states | `iga.promotion_not_pending_security` |
| `Execute()` | Status must be `ApprovedReadyToExecute` | `iga.promotion_not_approved` |
| `Execute()` | Must not already be executed | `iga.promotion_already_executed` |
| `AddImpactAnalysis()` | Only one impact analysis allowed | `iga.impact_analysis_already_exists` | ### Cross-Aggregate Guards (application layer)

| Validation | Notes |
|------------|-------|
| Manager and Security reviewer must be different actors | Application layer validates before calling security operations |
| High-risk promotions require ImpactAnalysis before Execute | Application layer pre-validates | ### Child Entity Cascade Rules

| Event | Cascade |
|-------|---------|
| Request rejected | `PromotionImpactAnalysis` is retained for audit purposes — not deleted. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Executed without Verification | `Execute()` completes but `Verify()` never called | Background monitoring; `VerificationFailed` is a valid terminal path |
| `VerificationFailed` with no defined next step | State reached but no transition to rejection | Application layer should auto-reject after timeout |
| Pending request ages indefinitely | No timeout mechanism | Planned: background job auto-rejects after 30 days in any Pending state | ---

## RoleMaturityStatus

### State Machine

Role maturity evolves through scoring and performance records. No fixed status enum — status is derived from maturity level and score.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| Level change | Must be different from current | `iga.maturity_level_unchanged` |
| Performance score | Must be in valid range | `iga.invalid_performance_score` | ---

*Part of [consistency-rules/index.md](./index.md)*
