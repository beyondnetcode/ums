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
        public const string BranchCodeNotUnique = "tenant.branch_code_not_unique";
        public const string BranchNotFound = "tenant.branch_not_found";
        public const string BranchActive = "tenant.branch_active";
        public const string ArchivedCannotSuspend = "tenant.archived_cannot_suspend";
        public const string ArchivedCannotActivate = "tenant.archived_cannot_activate";
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
        public const string PasswordHashRequired = "user_account.password_hash_required";
        public const string BlockedCannotEnrollMfa = "user_account.blocked_cannot_enroll_mfa";
        public const string MfaAlreadyEnrolled = "user_account.mfa_already_enrolled";
        public const string InternalRequiresHrId = "user_account.internal_requires_hr_id";
        public const string EmailNotUnique = "user_account.email_not_unique";
        public const string CannotRemoveLastPassword = "user_account.cannot_remove_last_password";
    }

    public static class System
    {
        public const string ConfigurationKeyTooLong = "system.configuration_key_too_long";
        public const string ConfigurationValueTooLong = "system.configuration_value_too_long";
        public const string ActionCodeTooLong = "system.action_code_too_long";
        public const string OptionCodeNotUnique = "system.option_code_not_unique";
        public const string SubMenuCodeNotUnique = "system.submenu_code_not_unique";
        public const string MenuCodeNotUnique = "system.menu_code_not_unique";
        public const string ModuleAlreadyActive = "system.module_already_active";
        public const string ModuleAlreadyInactive = "system.module_already_inactive";
        public const string ModuleInactiveCannotAddMenu = "system.module_inactive_cannot_add_menu";
        public const string ModuleCodeNotUnique = "system.module_code_not_unique";
        public const string RoleAlreadyActive = "system.role_already_active";
        public const string RoleAlreadyInactive = "system.role_already_inactive";
        public const string ActionAlreadyGranted = "system.action_already_granted";
        public const string ActionNotGranted = "system.action_not_granted";
        public const string ConfigurationKeyAlreadyExists = "system.configuration_key_already_exists";
        public const string ConfigurationKeyNotFound = "system.configuration_key_not_found";
        public const string ActionRequiresOwner = "system.action_requires_owner";
        public const string ActionXorViolation = "system.action_xor_violation";
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

    public static class ValueObject
    {
        public const string EmailRequired = "value_object.email_required";
        public const string PropertyRequired = "value_object.property_required";
        public const string DateRangeInvalid = "value_object.date_range_invalid";
    }
}
