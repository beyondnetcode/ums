namespace Ums.Domain.Kernel;

public static class DomainErrors
{
    public static class Common
    {
        public const string Required = "common.required";
        public const string Invalid = "common.invalid";
        public const string NotFound = "common.not_found";
        public const string Duplicate = "common.duplicate";
    }

    public static class Tenant
    {
        public const string Required = "user.tenant.required";
        public const string NotFound = "user.tenant.not_found";
        public const string SignupRequestNotPending = "tenant.signup_request_not_pending";
        public const string SignupRequestAlreadyProcessed = "tenant.signup_request_already_processed";
        public const string SignupRequestAlreadyExists = "tenant.signup_request_already_exists";
        public const string BranchCodeNotUnique = "tenant.branch_code_not_unique";
        public const string BranchNotFound = "tenant.branch_not_found";
        public const string BranchActive = "tenant.branch_active";
        public const string ArchivedCannotSuspend = "tenant.archived_cannot_suspend";
        public const string ArchivedCannotActivate = "tenant.archived_cannot_activate";
        public const string AlreadyActive = "tenant.already_active";
        public const string AlreadySuspended = "tenant.already_suspended";
        public const string IdpCodeNotUnique = "tenant.idp_code_not_unique";
        public const string IdpNotFound = "tenant.idp_not_found";
        public const string IdpAlreadyActive = "tenant.idp_already_active";
        public const string IdpAlreadyInactive = "tenant.idp_already_inactive";
        public const string NoActiveIdp = "tenant.no_active_idp";
        public const string BrandingAlreadyExists = "tenant.branding_already_exists";
        public const string BrandingNotFound = "tenant.branding_not_found";
    }

    public static class Branding
    {
        public const string InvalidHexColor = "branding.invalid_hex_color";
        public const string InvalidCustomDomain = "branding.invalid_custom_domain";
        public const string InvalidCnameTarget = "branding.invalid_cname_target";
        public const string LoginTextTooLong = "branding.login_text_too_long";
        public const string LogoRequired = "branding.logo_required";
        public const string LogoFormatRequired = "branding.logo_format_required";
        public const string InvalidLogoFormat = "branding.invalid_logo_format";
        public const string DnsVerificationRequired = "branding.dns_verification_required";
    }

    public static class UserAccount
    {
        public const string InvalidEmail = "user_account.invalid_email";
        public const string CannotActivate = "user_account.cannot_activate";
        public const string AlreadyBlocked = "user_account.already_blocked";
        public const string CannotRestore = "user_account.cannot_restore";
        public const string BlockedCannotUpdateCredentials = "user_account.blocked_cannot_update_credentials";
        public const string FederatedCannotUseLocalPassword = "user_account.federated_cannot_use_local_password";
        public const string PasswordHashRequired = "user_account.password_hash_required";
        public const string BlockedCannotEnrollMfa = "user_account.blocked_cannot_enroll_mfa";
        public const string MfaAlreadyEnrolled = "user_account.mfa_already_enrolled";
        public const string MfaAlreadyVerified = "user_account.mfa_already_verified";
        public const string InternalRequiresHrId = "user_account.internal_requires_hr_id";
        public const string EmailNotUnique = "user_account.email_not_unique";
        public const string CannotRemoveLastPassword = "user_account.cannot_remove_last_password";
        // REC-16: soft-delete + GDPR
        public const string AlreadyDeleted = "user_account.already_deleted";
    }

    public static class SystemSuite
    {
        public const string ConfigurationKeyTooLong = "system_suite.configuration_key_too_long";
        public const string ConfigurationValueTooLong = "system_suite.configuration_value_too_long";
        public const string ActionCodeTooLong = "system_suite.action_code_too_long";
        public const string OptionCodeNotUnique = "system_suite.option_code_not_unique";
        public const string SubMenuCodeNotUnique = "system_suite.submenu_code_not_unique";
        public const string MenuCodeNotUnique = "system_suite.menu_code_not_unique";
        public const string ModuleAlreadyActive = "system_suite.module_already_active";
        public const string ModuleAlreadyInactive = "system_suite.module_already_inactive";
        public const string ModuleInactiveCannotAddMenu = "system_suite.module_inactive_cannot_add_menu";
        public const string ModuleCodeNotUnique = "system_suite.module_code_not_unique";
        public const string RoleAlreadyActive = "system_suite.role_already_active";
        public const string RoleAlreadyInactive = "system_suite.role_already_inactive";
        public const string ActionAlreadyGranted = "system_suite.action_already_granted";
        public const string ActionNotGranted = "system_suite.action_not_granted";
        public const string ConfigurationKeyAlreadyExists = "system_suite.configuration_key_already_exists";
        public const string ConfigurationKeyNotFound = "system_suite.configuration_key_not_found";
        public const string ActionRequiresOwner = "system_suite.action_requires_owner";
        public const string ActionXorViolation = "system_suite.action_xor_violation";
    }

    public static class Authorization
    {
        public const string InvalidTemplateVersion = "authorization.invalid_template_version";
        public const string TemplateNotDraft = "authorization.template_not_draft";
        public const string TemplateNotPublished = "authorization.template_not_published";
        public const string TemplateAlreadyPublished = "authorization.template_already_published";
        public const string TemplateAlreadyDeprecated = "authorization.template_already_deprecated";
        public const string TemplateItemTargetAlreadyExists = "authorization.template_item_target_already_exists";
        public const string InvalidPermissionEffect = "authorization.invalid_permission_effect";
        public const string ProfileAlreadyActive = "authorization.profile_already_active";
        public const string ProfileAlreadyInactive = "authorization.profile_already_inactive";
        public const string PermissionAlreadyExists = "authorization.permission_already_exists";
        public const string PermissionNotFound = "authorization.permission_not_found";
        public const string TemplateNotPublishedForProfile = "authorization.template_not_published_for_profile";
        public const string TemplateTenantMismatch = "authorization.template_tenant_mismatch";
        public const string ProfileTemplateAlreadyLinked = "authorization.profile_template_already_linked";
        public const string RoleAlreadyActive = "authorization.role_already_active";
        public const string RoleAlreadyInactive = "authorization.role_already_inactive";
        public const string RoleHierarchyLevelInvalid = "authorization.role_hierarchy_level_invalid";
        public const string RootRoleHierarchyLevelInvalid = "authorization.root_role_hierarchy_level_invalid";
        public const string ChildRoleHierarchyLevelInvalid = "authorization.child_role_hierarchy_level_invalid";
        public const string RolePromotionOrderInvalid = "authorization.role_promotion_order_invalid";
        public const string ActionAlreadyGranted = "authorization.action_already_granted";
        public const string ActionNotGranted = "authorization.action_not_granted";
    }

    public static class Approvals
    {
        public const string DocumentTypeAlreadyRequired = "approvals.document_type_already_required";
        public const string RequestNotPending = "approvals.request_not_pending";
        public const string DocumentAlreadyExpired = "approvals.document_already_expired";
        public const string PolicyRequiresProfileOrRole = "approvals.policy_requires_profile_or_role";
        public const string PolicyAlreadyInactive = "approvals.policy_already_inactive";
        public const string RuleAlreadyInactive = "approvals.rule_already_inactive";
    }

    public static class Configuration
    {
        public const string IdpConfigNotDraft           = "configuration.idp_config_not_draft";
        public const string IdpConfigNotActive          = "configuration.idp_config_not_active";
        public const string IdpConfigAlreadyActive      = "configuration.idp_config_already_active";
        public const string IdpConfigAlreadyInactive    = "configuration.idp_config_already_inactive";
        public const string IdpConfigPayloadInvalid     = "configuration.idp_config_payload_invalid";
        public const string AppConfigNotDraft           = "configuration.app_config_not_draft";
        public const string AppConfigNotPublished       = "configuration.app_config_not_published";
        public const string AppConfigAlreadyArchived    = "configuration.app_config_already_archived";
        public const string FlagArchivedCannotChange    = "configuration.flag_archived_cannot_change";
        public const string FlagAlreadyActive           = "configuration.flag_already_active";
        public const string FlagAlreadyInactive         = "configuration.flag_already_inactive";
        public const string FlagPercentageOutOfRange    = "configuration.flag_percentage_out_of_range";
        public const string SystemSuiteIdRequired       = "configuration.system_suite_id_required";
        public const string DuplicateCriteria           = "configuration.duplicate_criteria";
        public const string CriteriaNotFound            = "configuration.criteria_not_found";
        public const string CriteriaTypeRequired        = "configuration.criteria_type_required";
        public const string CriteriaOperatorRequired    = "configuration.criteria_operator_required";
        public const string CriteriaValueRequired       = "configuration.criteria_value_required";
    }

    public static class Audit
    {
        public const string WhatChangedRequired = "audit.what_changed_required";
        public const string AffectedEntityRequired = "audit.affected_entity_required";
    }

    public static class Compliance
    {
        public const string ExpirationBeforeIssueDate          = "compliance.expiration_before_issue_date";
        public const string DocumentCannotTransition            = "compliance.document_cannot_transition";
        public const string DocumentNotPendingReview            = "compliance.document_not_pending_review";
        public const string DocumentAlreadyExpired              = "compliance.document_already_expired";
    }

    public static class Delegation
    {
        public const string SelfDelegationNotAllowed         = "delegation.self_delegation_not_allowed";
        public const string ValidUntilMustBeAfterValidFrom   = "delegation.valid_until_must_be_after_valid_from";
        public const string AllowedActionsRequired           = "delegation.allowed_actions_required";
        public const string CannotActivateFromCurrentStatus  = "delegation.cannot_activate_from_current_status";
        public const string CannotRevokeFromCurrentStatus    = "delegation.cannot_revoke_from_current_status";
        public const string RevocationReasonRequired         = "delegation.revocation_reason_required";
        public const string CannotArchiveFromCurrentStatus   = "delegation.cannot_archive_from_current_status";
        public const string NotActive                        = "delegation.not_active";
    }

    public static class IGA
    {
        public const string MaturityLevelUnchanged = "iga.maturity_level_unchanged";
        public const string InvalidPerformanceScore = "iga.invalid_performance_score";
        public const string PromotionNotInDraft = "iga.promotion_not_in_draft";
        public const string PromotionNotPendingManager = "iga.promotion_not_pending_manager";
        public const string PromotionNotPendingSecurity = "iga.promotion_not_pending_security";
        public const string PromotionNotApproved = "iga.promotion_not_approved";
        public const string PromotionAlreadyExecuted = "iga.promotion_already_executed";
        public const string PromotionAlreadyRejected = "iga.promotion_already_rejected";
        public const string ImpactAnalysisAlreadyExists = "iga.impact_analysis_already_exists";
    }

    public static class TenantParameter
    {
        public const string CodeNotUnique = "tenant_parameter.code_not_unique";
        public const string NotFound = "tenant_parameter.not_found";
        public const string InvalidValueType = "tenant_parameter.invalid_value_type";
        public const string ValueNotInAllowedList = "tenant_parameter.value_not_in_allowed_list";
        public const string CannotDeactivateActive = "tenant_parameter.cannot_deactivate_active";
        public const string CannotDeleteWithChildren = "tenant_parameter.cannot_delete_with_children";
    }

    public static class ValueObject
    {
        public const string EmailRequired = "value_object.email_required";
        public const string PropertyRequired = "value_object.property_required";
        public const string DateRangeInvalid = "value_object.date_range_invalid";
    }
}
