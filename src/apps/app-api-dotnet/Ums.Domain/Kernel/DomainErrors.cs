namespace Ums.Domain.Kernel;

public static class DomainErrors
{
    public static class Common
    {
        public const string Required = "common.required";
        public const string Invalid = "common.invalid";
        public const string NotFound = "common.not_found";
        public const string Duplicate = "common.duplicate";
        public const string Unauthorized = "common.unauthorized";
        public const string Forbidden = "common.forbidden";
    }

    public static class User
    {
        public const string TerminatedCannotActivate = "user.account.terminated_cannot_activate";
        public const string BlockReasonRequired = "user.account.block_reason_required";
        public const string ProfileIdRequired = "user.account.profile_id_required";
        public const string ProfileAlreadyAssigned = "user.account.profile_already_assigned";
        public const string DuplicateEmail = "user.account.duplicate_email";
        public const string NotFound = "user.account.not_found";
        public const string InvalidCredentials = "user.account.invalid_credentials";
        public const string Locked = "user.account.locked";
        public const string Disabled = "user.account.disabled";
    }

    public static class Tenant
    {
        public const string Required = "user.tenant.required";
        public const string Invalid = "user.tenant.invalid";
        public const string NotFound = "user.tenant.not_found";
        public const string BranchCodeNotUnique = "tenant.branch_code_not_unique";
        public const string ArchivedCannotSuspend = "tenant.archived_cannot_suspend";
        public const string ArchivedCannotActivate = "tenant.archived_cannot_activate";
    }

    public static class Profile
    {
        public const string Required = "user.profile.required";
        public const string NotFound = "user.profile.not_found";
        public const string AlreadyAssigned = "user.profile.already_assigned";
        public const string CodeRequired = "authorization.profile.required";
        public const string NotFoundAuth = "authorization.profile.not_found";
        public const string Duplicate = "authorization.profile.duplicate";
        public const string RoleIdRequired = "authorization.profile.role_id_required";
        public const string RoleAlreadyAssigned = "authorization.profile.role_already_assigned";
        public const string FunctionalActionIdRequired = "authorization.profile.functional_action_id_required";
    }

    public static class Role
    {
        public const string Required = "authorization.role.required";
        public const string NotFound = "authorization.role.not_found";
        public const string Duplicate = "authorization.role.duplicate";
        public const string TenantAndSystemRequired = "authorization.role.tenant_and_system_required";
    }

    public static class Authorization
    {
        public const string GrantExpired = "authorization.grant.expired";
        public const string GrantInvalid = "authorization.grant.invalid";
        public const string GrantInsufficient = "authorization.grant.insufficient";
        public const string PermissionDenied = "authorization.permission.denied";
        public const string TemplateNotFound = "authorization.template.not_found";
        public const string FunctionalModuleNotFound = "authorization.functional_module.not_found";
        public const string FunctionalActionNotFound = "authorization.functional_action.not_found";
        public const string FunctionalSubmoduleNotFound = "authorization.functional_submodule.not_found";
        public const string FunctionalOptionNotFound = "authorization.functional_option.not_found";
        public const string SystemSuiteNotFound = "authorization.system_suite.not_found";
    }

    public static class FunctionalModule
    {
        public const string TenantAndSystemRequired = "authorization.functional_module.tenant_and_system_required";
        public const string MenuCodeNotUnique = "authorization.functional_module.menu_code_not_unique";
    }

    public static class FunctionalSubmodule
    {
        public const string TenantSystemModuleRequired = "authorization.functional_submodule.tenant_system_module_required";
        public const string MenuLabelRequired = "authorization.functional_submodule.menu_label_required";
        public const string OptionCodeNotUnique = "authorization.functional_submodule.option_code_not_unique";
    }

    public static class FunctionalOption
    {
        public const string IdentifiersRequired = "authorization.functional_option.identifiers_required";
        public const string LabelRequired = "authorization.functional_option.label_required";
        public const string RouteRequired = "authorization.functional_option.route_required";
    }

    public static class FunctionalAction
    {
        public const string TenantAndSystemRequired = "authorization.functional_action.tenant_and_system_required";
        public const string LevelRequired = "authorization.functional_action.level_required";
    }

    public static class SystemSuite
    {
        public const string BaseUrlRequired = "authorization.system_suite.base_url_required";
        public const string ModuleCodeNotUnique = "authorization.system_suite.module_code_not_unique";
        public const string ActionCodeNotUnique = "authorization.system_suite.action_code_not_unique";
    }

    public static class PermissionTemplate
    {
        public const string IdentifiersRequired = "authorization.permission_template.identifiers_required";
        public const string FunctionalActionIdRequired = "authorization.permission_template.functional_action_id_required";
        public const string GrantAlreadyExists = "authorization.permission_template.grant_already_exists";
        public const string CannotPublishWithoutGrants = "authorization.permission_template.cannot_publish_without_grants";
    }

    public static class TemplateAssignmentRule
    {
        public const string TemplateIdRequired = "authorization.template_assignment_rule.template_id_required";
        public const string PredicateRequired = "authorization.template_assignment_rule.predicate_required";
    }

    public static class CompiledPolicyGraph
    {
        public const string IdentifiersRequired = "authorization.compiled_policy_graph.identifiers_required";
        public const string GraphHashRequired = "authorization.compiled_policy_graph.graph_hash_required";
    }

    public static class Iga
    {
        public const string DelegationIdentifiersRequired = "iga.delegation.identifiers_required";
        public const string DelegatorDelegateMustDiffer = "iga.delegation.delegator_delegate_must_differ";
        public const string DelegationScopeRequired = "iga.delegation.scope_required";
        public const string PromotionIdentifiersRequired = "iga.promotion.identifiers_required";
        public const string ApprovalIdRequired = "iga.promotion.approval_id_required";
        public const string PromotionOnlyCompleteAfterCriteria = "iga.promotion.only_complete_after_criteria";
        public const string SourceTargetRolesRequired = "iga.role_promotion.source_target_roles_required";
        public const string SourceTargetMustDiffer = "iga.role_promotion.source_target_must_differ";
        public const string EvaluationExpressionRequired = "iga.role_promotion.evaluation_expression_required";
    }

    public static class Approval
    {
        public const string RequestIdRequired = "approval.request.id_required";
        public const string IdentifiersRequired = "approval.request.identifiers_required";
        public const string OnlyDraftCanSubmit = "approval.request.only_draft_can_submit";
        public const string OnlyPendingCanResolve = "approval.request.only_pending_can_resolve";
        public const string JustificationRequired = "approval.request.justification_required";
        public const string ResolutionCommentRequired = "approval.request.resolution_comment_required";
        public const string ResolverIdRequired = "approval.request.resolver_id_required";
        public const string MinimumApprovalsRequired = "approval.request.minimum_approvals_required";
        public const string ExternalIdentifiersRequired = "approval.request.external_identifiers_required";
        public const string RequestTypeRequired = "approval.request.type_required";
    }

    public static class Audit
    {
        public const string FlagLogIdentifiersRequired = "audit.flag_log.identifiers_required";
        public const string FlagLogSubjectRequired = "audit.flag_log.subject_required";
        public const string RecordIdentifiersRequired = "audit.record.identifiers_required";
    }

    public static class Compliance
    {
        public const string ResourceScopeRequired = "compliance.access_enforcement.resource_scope_required";
        public const string NotificationTriggerRequired = "compliance.notification.trigger_required";
        public const string UserDocumentIdentifiersRequired = "compliance.user_document.identifiers_required";
        public const string StorageReferenceRequired = "compliance.user_document.storage_reference_required";
        public const string ReviewCommentRequired = "compliance.user_document.review_comment_required";
    }

    public static class Configuration
    {
        public const string CreatorRequired = "configuration.feature_flag.creator_required";
        public const string SystemSuiteIdRequired = "configuration.app_configuration.system_suite_id_required";
        public const string PublisherRequired = "configuration.app_configuration.publisher_required";
    }

    public static class ParametricCatalog
    {
        public const string TenantIdRequired = "parametric_catalog.tenant_id_required";
        public const string ValueRequired = "parametric_catalog.value_required";
        public const string DescriptionRequired = "parametric_catalog.description_required";
        public const string VersionRequired = "parametric_catalog.version_required";
    }

    public static class ValueObject
    {
        public const string EmailRequired = "value_object.email_required";
        public const string PropertyRequired = "value_object.property_required";
        public const string DateRangeInvalid = "value_object.date_range_invalid";
    }

    public static class System
    {
        public const string InternalError = "system.internal_error";
        public const string ValidationFailed = "system.validation_failed";
        public const string ConfigurationInvalid = "system.configuration_invalid";
    }
}
