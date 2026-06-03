# Identity BC — Consistency Rules

> **Bounded Context:** `Ums.Domain.Identity`
> **Aggregates:** `Tenant`, `UserAccount`, `UserManagementDelegation`, `TenantSignupRequest`

---

## Tenant

### State Machine

```
Active ──Suspend()──► Suspended ──Activate()──► Active
Active ──Archive()──► Archived (terminal)
Suspended ──Archive()──► Archived (terminal)
```

**Forbidden transitions:**
- `Archived → Suspend` → broken rule `tenant.archived_cannot_suspend`
- `Archived → Activate` → broken rule `tenant.archived_cannot_activate`
- `Active → Active` → broken rule `tenant.already_active`
- `Suspended → Suspended` → broken rule `tenant.already_suspended`
- `Archived → Archive` → broken rule `tenant.archived_cannot_archive`

### Intra-aggregate Guards (enforced by the aggregate itself)

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `AddBranch()` | Branch code must be unique within tenant | `tenant.branch_code_not_unique` |
| `RemoveBranch()` | Branch must be inactive before removal | `tenant.branch_active` |
| `RegisterIdentityProvider()` | IdP code must be unique within tenant | `tenant.idp_code_not_unique` |
| `ActivateIdentityProvider()` | IdP must not already be active | `tenant.idp_already_active` |
| `DeactivateIdentityProvider()` | IdP must not already be inactive | `tenant.idp_already_inactive` |
| `SetBranding()` | Branding must not already exist | `tenant.branding_already_exists` |
| `UpdateBranding() / RemoveBranding() / VerifyBrandingDns() / FailBrandingDns()` | Branding must exist | `tenant.branding_not_found` |
| `AddParameter()` | Parameter code must be unique among active parameters | `tenant_parameter.code_not_unique` | ### Cross-Aggregate Dependency Guards (application layer passes counts)

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `Suspend(activeUserCount, activeBranchCount)` | `activeUserCount > 0` | `TENANT_HAS_ACTIVE_USERS` |
| `Suspend(activeUserCount, activeBranchCount)` | `activeBranchCount > 0` | `TENANT_HAS_ACTIVE_BRANCHES` |
| `Archive(activeIdpCount)` | `activeIdpCount > 0` | `TENANT_HAS_ACTIVE_IDP` | ### Child Entity Cascade Rules

| Event | Cascade |
|-------|---------|
| Tenant suspended | No automatic cascade. Application layer must deactivate branches/users before calling `Suspend()`. |
| Tenant archived | No automatic cascade. Archive is a terminal state. All active children must be resolved before archive. |
| `DeactivateBranch()` | Branch state changes to Inactive. No cascade to users — application layer handles. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active Branch in Suspended Tenant | `Suspend()` called without first resolving active branches | Pass `activeBranchCount` → application layer enforces |
| Active Users in Suspended Tenant | `Suspend()` called without first blocking users | Pass `activeUserCount` → application layer enforces |
| Active IdP in Archived Tenant | `Archive()` called without deactivating IdP | Pass `activeIdpCount` → application layer enforces | ---

## UserAccount

### State Machine

```
Pending ──Activate()──► Active ──Block()──► Blocked ──Restore()──► Active
Pending ──Deny()──► Denied (terminal)
Active ──Delete()──► Deleted (terminal, GDPR soft-delete)
Blocked ──Delete()──► Deleted (terminal)
```

**Forbidden transitions:**
- Any state → `Active` without going through `Restore()` from `Blocked`
- `Deleted → any` → broken rule `user_account.already_deleted`
- `Denied → any` → terminal state

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Activate()` | Status must be `Pending` or `Suspended` | `user_account.cannot_activate` |
| `Block()` | Must not already be `Blocked` | `user_account.already_blocked` |
| `Restore()` | Must be `Blocked` | `user_account.cannot_restore` |
| `Delete()` | Must not already be `Deleted` | `user_account.already_deleted` |
| `Deny()` | Must be in deniable state | `user_account.cannot_deny` |
| `AddPassword()` | User must not be `Blocked` | `user_account.blocked_cannot_update_credentials` |
| `AddPassword()` | User must not be federated | `user_account.federated_cannot_use_local_password` |
| `AddPassword()` | Password hash required | `user_account.password_hash_required` |
| `RemovePassword()` | Must not be last active password | `user_account.cannot_remove_last_password` |
| `EnrollMfa()` | User must not be `Blocked` | `user_account.blocked_cannot_enroll_mfa` |
| `EnrollMfa()` | Method must not already be enrolled | `user_account.mfa_already_enrolled` | ### Cross-Aggregate Dependency Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Delete(activeProfileCount)` | `activeProfileCount > 0` | `USER_HAS_ACTIVE_PROFILES` | ### Child Entity Cascade Rules

| Event | Cascade |
|-------|---------|
| `Block()` | All PasswordCredentials must be deactivated. All MfaEnrollments must be suspended. |
| `Delete()` | PII anonymisation triggered via domain event `UserDeletedEvent`. Application layer must respond to event. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active Profile referencing Deleted User | `Delete()` without removing profiles | Pass `activeProfileCount` → application layer deactivates profiles first |
| Active credentials on Blocked user | `Block()` without deactivating credentials | Application layer responds to `UserBlockedEvent` | ---

## UserManagementDelegation

### State Machine

```
Draft ──SubmitForApproval()──► PendingApproval ──Approve()──► Active
Draft ──Activate()──► Active (if AutoApprove)
Active ──Revoke()──► Revoked (terminal)
Active ──Expire()──► Expired (terminal, triggered by background job)
Active ──Complete()──► Completed (terminal)
PendingApproval ──Reject()──► Rejected (terminal)
Revoked/Expired/Completed/Rejected ──Archive()──► Archived (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` | Delegating and Delegated actors must differ | `delegation.self_delegation_not_allowed` |
| `Create()` | `ValidUntil > ValidFrom` | `delegation.valid_until_must_be_after_valid_from` |
| `Create()` | At least one `AllowedAction` required | `delegation.allowed_actions_required` |
| `Activate()` | Status must be `Draft` or `PendingApproval` | `delegation.cannot_activate_from_current_status` |
| `Revoke()` | Status must be `Active` | `delegation.cannot_revoke_from_current_status` |
| `Revoke()` | Reason required | `delegation.revocation_reason_required` |
| `Archive()` | Status must be terminal | `delegation.cannot_archive_from_current_status` |
| `CanExecuteAction()` | Status must be `Active` AND `ValidUntil >= now` | returns `false`, no broken rule | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active Delegation past ValidUntil | Background job not running | Background job must call `Expire()` daily |
| Delegation.Active with deleted DelegatingAdmin | DelegatingAdmin UserAccount deleted | Application layer listens to `UserDeletedEvent` and revokes delegation | ---

## TenantSignupRequest

### State Machine

```
Pending ──Approve()──► Approved (terminal)
Pending ──Reject()──► Rejected (terminal)
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Approve()` / `Reject()` | Status must be `Pending` | `tenant.signup_request_not_pending` |
| `Create()` | No duplicate active request for same email | `tenant.signup_request_already_exists` | ---

*Part of [consistency-rules/index.md](./index.md)*
