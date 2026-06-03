# Broken Rules Registry

> Complete list of every domain broken rule in UMS, with implementation status.
> Legend: Implemented | Defined in DomainErrors but not wired | Missing

---

## Identity BC

### Tenant

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `tenant.archived_cannot_suspend` | `DomainErrors.Tenant.ArchivedCannotSuspend` | `Suspend()` when Archived | |
| `tenant.archived_cannot_activate` | `DomainErrors.Tenant.ArchivedCannotActivate` | `Activate()` when Archived | |
| `tenant.already_active` | `DomainErrors.Tenant.AlreadyActive` | `Activate()` when already Active | |
| `tenant.already_suspended` | `DomainErrors.Tenant.AlreadySuspended` | `Suspend()` when already Suspended | |
| `tenant.branch_code_not_unique` | `DomainErrors.Tenant.BranchCodeNotUnique` | `AddBranch()` duplicate code | |
| `tenant.branch_active` | `DomainErrors.Tenant.BranchActive` | `RemoveBranch()` on active branch | |
| `tenant.idp_code_not_unique` | `DomainErrors.Tenant.IdpCodeNotUnique` | `RegisterIdentityProvider()` | |
| `tenant.idp_already_active` | `DomainErrors.Tenant.IdpAlreadyActive` | `ActivateIdentityProvider()` | |
| `tenant.idp_already_inactive` | `DomainErrors.Tenant.IdpAlreadyInactive` | `DeactivateIdentityProvider()` | |
| `tenant.branding_already_exists` | `DomainErrors.Tenant.BrandingAlreadyExists` | `SetBranding()` | |
| `tenant.branding_not_found` | `DomainErrors.Tenant.BrandingNotFound` | `UpdateBranding()`, `RemoveBranding()` | |
| `TENANT_HAS_ACTIVE_USERS` | `DomainErrors.Tenant.HasActiveUsers` | `Suspend(activeUserCount > 0)` | |
| `TENANT_HAS_ACTIVE_BRANCHES` | `DomainErrors.Tenant.HasActiveBranches` | `Suspend(activeBranchCount > 0)` | |
| `TENANT_HAS_ACTIVE_IDP` | `DomainErrors.Tenant.HasActiveIdpConfig` | `Archive()` with active IdP | |
| `tenant.archived_cannot_archive` | `DomainErrors.Tenant.ArchivedCannotArchive` | `Archive()` when already Archived | | ### UserAccount

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `user_account.cannot_activate` | `DomainErrors.UserAccount.CannotActivate` | `Activate()` invalid state | |
| `user_account.already_blocked` | `DomainErrors.UserAccount.AlreadyBlocked` | `Block()` when already Blocked | |
| `user_account.cannot_restore` | `DomainErrors.UserAccount.CannotRestore` | `Restore()` invalid state | |
| `user_account.blocked_cannot_update_credentials` | `DomainErrors.UserAccount.BlockedCannotUpdateCredentials` | `AddPassword()` when Blocked | |
| `user_account.federated_cannot_use_local_password` | `DomainErrors.UserAccount.FederatedCannotUseLocalPassword` | `AddPassword()` federated user | |
| `user_account.password_hash_required` | `DomainErrors.UserAccount.PasswordHashRequired` | `AddPassword()` missing hash | |
| `user_account.blocked_cannot_enroll_mfa` | `DomainErrors.UserAccount.BlockedCannotEnrollMfa` | `EnrollMfa()` when Blocked | |
| `user_account.mfa_already_enrolled` | `DomainErrors.UserAccount.MfaAlreadyEnrolled` | `EnrollMfa()` duplicate | |
| `user_account.cannot_remove_last_password` | `DomainErrors.UserAccount.CannotRemoveLastPassword` | `RemovePassword()` last one | |
| `user_account.already_deleted` | `DomainErrors.UserAccount.AlreadyDeleted` | `Delete()` when Deleted | |
| `user_account.cannot_deny` | `DomainErrors.UserAccount.CannotDeny` | `Deny()` invalid state | |
| `USER_HAS_ACTIVE_PROFILES` | `DomainErrors.UserAccount.HasActiveProfiles` | `Delete(activeProfileCount > 0)` | Defined, not wired | ### UserManagementDelegation

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `delegation.self_delegation_not_allowed` | `DomainErrors.Delegation.SelfDelegationNotAllowed` | `Create()` same actor | |
| `delegation.valid_until_must_be_after_valid_from` | `DomainErrors.Delegation.ValidUntilMustBeAfterValidFrom` | `Create()` | |
| `delegation.allowed_actions_required` | `DomainErrors.Delegation.AllowedActionsRequired` | `Create()` empty actions | |
| `delegation.cannot_activate_from_current_status` | `DomainErrors.Delegation.CannotActivateFromCurrentStatus` | `Activate()` wrong state | |
| `delegation.cannot_revoke_from_current_status` | `DomainErrors.Delegation.CannotRevokeFromCurrentStatus` | `Revoke()` wrong state | |
| `delegation.revocation_reason_required` | `DomainErrors.Delegation.RevocationReasonRequired` | `Revoke()` no reason | |
| `delegation.cannot_archive_from_current_status` | `DomainErrors.Delegation.CannotArchiveFromCurrentStatus` | `Archive()` wrong state | | ---

## Authorization BC

### Role

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `authorization.role_already_active` | `DomainErrors.Authorization.RoleAlreadyActive` | `Activate()` when active | |
| `authorization.role_already_inactive` | `DomainErrors.Authorization.RoleAlreadyInactive` | `Deactivate()` when inactive | |
| `authorization.role_hierarchy_level_invalid` | `DomainErrors.Authorization.RoleHierarchyLevelInvalid` | `Create()/Update()` level < 0 | |
| `authorization.root_role_hierarchy_level_invalid` | `DomainErrors.Authorization.RootRoleHierarchyLevelInvalid` | `Create()/Update()` root≠0 | |
| `authorization.child_role_hierarchy_level_invalid` | `DomainErrors.Authorization.ChildRoleHierarchyLevelInvalid` | `Create()/Update()` child=0 | |
| `authorization.role_promotion_order_invalid` | `DomainErrors.Authorization.RolePromotionOrderInvalid` | `Create()/Update()` order < 0 | |
| `ROLE_HAS_ACTIVE_PROFILES` | `DomainErrors.Authorization.RoleHasActiveProfiles` | `Deactivate(activeProfileCount > 0)` | |
| `ROLE_HAS_ACTIVE_CHILD_ROLES` | `DomainErrors.Authorization.RoleHasActiveChildRoles` | `Deactivate(activeChildRoleCount > 0)` | | ### PermissionTemplate

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `authorization.template_not_draft` | `DomainErrors.Authorization.TemplateNotDraft` | `AddItem()`, `Publish()` wrong state | |
| `authorization.template_not_published` | `DomainErrors.Authorization.TemplateNotPublished` | `Deprecate()` wrong state | |
| `authorization.template_already_published` | `DomainErrors.Authorization.TemplateAlreadyPublished` | `Publish()` already published | |
| `authorization.template_already_deprecated` | `DomainErrors.Authorization.TemplateAlreadyDeprecated` | `Deprecate()` already deprecated | |
| `authorization.template_item_target_already_exists` | `DomainErrors.Authorization.TemplateItemTargetAlreadyExists` | `AddItem()` duplicate | |
| `authorization.invalid_permission_effect` | `DomainErrors.Authorization.InvalidPermissionEffect` | `AddItem()` invalid effect | |
| `TEMPLATE_HAS_ACTIVE_PROFILES` | `DomainErrors.Authorization.TemplateHasActiveProfiles` | `Delete(activeProfileCount > 0)` | |
| `authorization.template_not_deletable` | `DomainErrors.Authorization.TemplateNotDeletable` | `Delete()` when status is Published | |
| `authorization.template_items_required` | `DomainErrors.Authorization.TemplateItemsRequired` | `Publish()` with no items | | ### Profile

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `authorization.profile_already_active` | `DomainErrors.Authorization.ProfileAlreadyActive` | `Activate()` when active | |
| `authorization.profile_already_inactive` | `DomainErrors.Authorization.ProfileAlreadyInactive` | `Deactivate()` when inactive | |
| `authorization.template_not_published_for_profile` | `DomainErrors.Authorization.TemplateNotPublishedForProfile` | `AssignTemplate()` not published | |
| `authorization.template_tenant_mismatch` | `DomainErrors.Authorization.TemplateTenantMismatch` | `AssignTemplate()` wrong tenant | |
| `authorization.profile_template_already_linked` | `DomainErrors.Authorization.ProfileTemplateAlreadyLinked` | `AssignTemplate()` duplicate | |
| `authorization.permission_not_found` | `DomainErrors.Authorization.PermissionNotFound` | Override/Activate/Deactivate | | ### SystemSuite

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `system_suite.module_already_active` | `DomainErrors.SystemSuite.ModuleAlreadyActive` | `ActivateModule()` | |
| `system_suite.module_already_inactive` | `DomainErrors.SystemSuite.ModuleAlreadyInactive` | `DeactivateModule()` | |
| `system_suite.module_inactive_cannot_add_menu` | `DomainErrors.SystemSuite.ModuleInactiveCannotAddMenu` | `AddMenu()` inactive module | |
| `system_suite.module_code_not_unique` | `DomainErrors.SystemSuite.ModuleCodeNotUnique` | `AddModule()` duplicate | |
| `system_suite.menu_code_not_unique` | `DomainErrors.SystemSuite.MenuCodeNotUnique` | `AddMenu()` duplicate | |
| `system_suite.submenu_code_not_unique` | `DomainErrors.SystemSuite.SubMenuCodeNotUnique` | `AddSubMenu()` duplicate | |
| `system_suite.option_code_not_unique` | `DomainErrors.SystemSuite.OptionCodeNotUnique` | `AddOption()` duplicate | |
| `authorization.domain_method_requires_parent` | `DomainErrors.Authorization.DomainMethodRequiresParent` | `AddDomainResource()` DomainMethod without parent | |
| `authorization.parent_resource_not_found` | `DomainErrors.Authorization.ParentResourceNotFound` | `AddDomainResource()` parent missing | |
| `authorization.domain_method_cannot_be_parent` | `DomainErrors.Authorization.DomainMethodCannotBeParent` | `AddDomainResource()` invalid hierarchy | |
| `DOMAIN_RESOURCE_HAS_TEMPLATE_ITEMS` | `DomainErrors.Authorization.DomainResourceHasTemplateItems` | `RemoveDomainResource(templateItemCount > 0)` | |
| `MODULE_HAS_ACTIVE_MENUS` | `DomainErrors.Authorization.ModuleHasActiveMenus` | `RemoveModule(activeMenuCount > 0)` | | ---

## Configuration BC

### FeatureFlag

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `configuration.flag_archived_cannot_change` | `DomainErrors.Configuration.FlagArchivedCannotChange` | Any mutation when Archived | |
| `configuration.flag_already_active` | `DomainErrors.Configuration.FlagAlreadyActive` | `Activate()` when active | |
| `configuration.flag_already_inactive` | `DomainErrors.Configuration.FlagAlreadyInactive` | `Deactivate()` when inactive | |
| `configuration.flag_percentage_out_of_range` | `DomainErrors.Configuration.FlagPercentageOutOfRange` | `Create()/UpdateTargeting()` % < 0 or > 100 | |
| `configuration.duplicate_criteria` | `DomainErrors.Configuration.DuplicateCriteria` | `AddCriteria()` duplicate | |
| `configuration.criteria_not_found` | `DomainErrors.Configuration.CriteriaNotFound` | `RemoveCriteria()` missing | | ### ParameterDefinition

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `configuration.parameter_code_not_unique` | `DomainErrors.Configuration.ParameterCodeNotUnique` | `Create()` duplicate code in scope | |
| `configuration.parameter_has_active_values` | `DomainErrors.Configuration.ParameterHasActiveValues` | `Archive()` when active values exist | | ### ParameterGlobalValue

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `configuration.parameter_value_invalid_type` | `DomainErrors.Configuration.ParameterValueInvalidType` | `Create()/UpdateValue()` wrong data type | |
| `configuration.parameter_global_value_in_use` | `DomainErrors.Configuration.ParameterGlobalValueInUse` | `Archive()` while tenant overrides remain active | | ### ParameterTenantValue

| Error Code | DomainErrors key | Trigger operation | Status |
|------------|-----------------|-------------------|--------|
| `configuration.parameter_value_invalid_type` | `DomainErrors.Configuration.ParameterValueInvalidType` | `Create()/UpdateValue()` wrong data type | |
| `configuration.parameter_override_not_allowed` | `DomainErrors.Configuration.ParameterOverrideNotAllowed` | `Create()/UpdateValue()` when scope disallows tenant overrides | | ### IdpConfiguration

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `configuration.idp_config_not_draft` | `DomainErrors.Configuration.IdpConfigNotDraft` | Mutation on non-Draft | |
| `configuration.idp_config_already_active` | `DomainErrors.Configuration.IdpConfigAlreadyActive` | `Activate()` when active | |
| `configuration.idp_config_already_inactive` | `DomainErrors.Configuration.IdpConfigAlreadyInactive` | `Deactivate()` when inactive | | ### AppConfiguration

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `configuration.app_config_not_draft` | `DomainErrors.Configuration.AppConfigNotDraft` | Mutation when not Draft | |
| `configuration.app_config_already_archived` | `DomainErrors.Configuration.AppConfigAlreadyArchived` | `Archive()` when Archived | | ---

## Approvals BC

### ApprovalWorkflow

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `approvals.document_type_already_required` | `DomainErrors.Approvals.DocumentTypeAlreadyRequired` | `AddRequiredDocument()` duplicate | |
| `approvals.requires_documents_if_approval_required` | `DomainErrors.Approvals.RequiresDocumentsIfApprovalRequired` | `Create()/RemoveRequiredDocument()` leaves 0 docs when RequiresApproval=true | | ### ApprovalRequest

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `approvals.request_not_pending` | `DomainErrors.Approvals.RequestNotPending` | `Approve()/Reject()` when not Pending | | ### NotificationRule

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `approvals.rule_already_inactive` | `DomainErrors.Approvals.RuleAlreadyInactive` | `Deactivate()` when already inactive | | ### AccessEnforcementPolicy

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `approvals.policy_requires_profile_or_role` | `DomainErrors.Approvals.PolicyRequiresProfileOrRole` | `Create()` no target | |
| `approvals.policy_already_inactive` | `DomainErrors.Approvals.PolicyAlreadyInactive` | `Deactivate()` when inactive | | ### UserDocument

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `compliance.expiration_before_issue_date` | `DomainErrors.Compliance.ExpirationBeforeIssueDate` | `Upload()` date validation | |
| `compliance.document_cannot_transition` | `DomainErrors.Compliance.DocumentCannotTransition` | Invalid status transition | |
| `compliance.document_not_pending_review` | `DomainErrors.Compliance.DocumentNotPendingReview` | `Validate()/Reject()` wrong state | |
| `compliance.document_already_expired` | `DomainErrors.Compliance.DocumentAlreadyExpired` | `Expire()` when already expired | | ---

## IGA BC

### PromotionRequest

| Error Code | DomainErrors key | Trigger | Status |
|------------|-----------------|---------|--------|
| `iga.promotion_not_in_draft` | `DomainErrors.IGA.PromotionNotInDraft` | `Submit()` wrong state | |
| `iga.promotion_not_pending_manager` | `DomainErrors.IGA.PromotionNotPendingManager` | `ManagerApprove()/ManagerReject()` | |
| `iga.promotion_not_pending_security` | `DomainErrors.IGA.PromotionNotPendingSecurity` | `SecurityReview*()` | |
| `iga.promotion_not_approved` | `DomainErrors.IGA.PromotionNotApproved` | `Execute()` wrong state | |
| `iga.promotion_already_executed` | `DomainErrors.IGA.PromotionAlreadyExecuted` | `Execute()` repeat | |
| `iga.impact_analysis_already_exists` | `DomainErrors.IGA.ImpactAnalysisAlreadyExists` | `AddImpactAnalysis()` duplicate | | ---

## Summary

| Status | Count |
|--------|-------|
| Implemented | ~65 |
| Defined but not wired | 0 |
| Missing entirely | 0 | *Update this table whenever you implement or add a broken rule.*
