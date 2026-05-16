namespace Ums.Domain.Enums;

using Ums.Shell.Ddd;

public class TenantStatus : DomainEnumeration
{
    public static readonly TenantStatus Active = new(1, nameof(Active));
    public static readonly TenantStatus Suspended = new(2, nameof(Suspended));
    public static readonly TenantStatus Archived = new(3, nameof(Archived));

    private TenantStatus(int id, string name) : base(id, name) { }
}

public class UserAccountStatus : DomainEnumeration
{
    public static readonly UserAccountStatus Pending = new(1, nameof(Pending));
    public static readonly UserAccountStatus Active = new(2, nameof(Active));
    public static readonly UserAccountStatus Suspended = new(3, nameof(Suspended));
    public static readonly UserAccountStatus Blocked = new(4, nameof(Blocked));
    public static readonly UserAccountStatus Terminated = new(5, nameof(Terminated));

    private UserAccountStatus(int id, string name) : base(id, name) { }
}

public class IdpStrategy : DomainEnumeration
{
    public static readonly IdpStrategy InternalBcrypt = new(1, nameof(InternalBcrypt));
    public static readonly IdpStrategy Zitadel = new(2, nameof(Zitadel));
    public static readonly IdpStrategy AzureAd = new(3, nameof(AzureAd));
    public static readonly IdpStrategy Okta = new(4, nameof(Okta));
    public static readonly IdpStrategy Keycloak = new(5, nameof(Keycloak));
    public static readonly IdpStrategy Auth0 = new(6, nameof(Auth0));
    public static readonly IdpStrategy Google = new(7, nameof(Google));
    public static readonly IdpStrategy Ldap = new(8, nameof(Ldap));
    public static readonly IdpStrategy Saml2 = new(9, nameof(Saml2));
    public static readonly IdpStrategy GenericOidc = new(10, nameof(GenericOidc));

    private IdpStrategy(int id, string name) : base(id, name) { }
}

public class LifecycleStatus : DomainEnumeration
{
    public static readonly LifecycleStatus Draft = new(1, nameof(Draft));
    public static readonly LifecycleStatus Active = new(2, nameof(Active));
    public static readonly LifecycleStatus Published = new(3, nameof(Published));
    public static readonly LifecycleStatus Inactive = new(4, nameof(Inactive));
    public static readonly LifecycleStatus Archived = new(5, nameof(Archived));
    public static readonly LifecycleStatus Retired = new(6, nameof(Retired));
    public static readonly LifecycleStatus Deprecated = new(7, nameof(Deprecated));

    private LifecycleStatus(int id, string name) : base(id, name) { }
}

public class PermissionEffect : DomainEnumeration
{
    public static readonly PermissionEffect Allow = new(1, nameof(Allow));
    public static readonly PermissionEffect Deny = new(2, nameof(Deny));

    private PermissionEffect(int id, string name) : base(id, name) { }
}

public class FunctionalActionLevel : DomainEnumeration
{
    public static readonly FunctionalActionLevel System = new(1, nameof(System));
    public static readonly FunctionalActionLevel Module = new(2, nameof(Module));
    public static readonly FunctionalActionLevel Menu = new(3, nameof(Menu));
    public static readonly FunctionalActionLevel Option = new(4, nameof(Option));

    private FunctionalActionLevel(int id, string name) : base(id, name) { }
}

public class FeatureFlagType : DomainEnumeration
{
    public static readonly FeatureFlagType Boolean = new(1, nameof(Boolean));
    public static readonly FeatureFlagType Variant = new(2, nameof(Variant));
    public static readonly FeatureFlagType Percentage = new(3, nameof(Percentage));

    private FeatureFlagType(int id, string name) : base(id, name) { }
}

public class ApprovalRequestStatus : DomainEnumeration
{
    public static readonly ApprovalRequestStatus Draft = new(1, nameof(Draft));
    public static readonly ApprovalRequestStatus Pending = new(2, nameof(Pending));
    public static readonly ApprovalRequestStatus Approved = new(3, nameof(Approved));
    public static readonly ApprovalRequestStatus Rejected = new(4, nameof(Rejected));
    public static readonly ApprovalRequestStatus Cancelled = new(5, nameof(Cancelled));
    public static readonly ApprovalRequestStatus Expired = new(6, nameof(Expired));

    private ApprovalRequestStatus(int id, string name) : base(id, name) { }
}

public class ApprovalDecision : DomainEnumeration
{
    public static readonly ApprovalDecision Approved = new(1, nameof(Approved));
    public static readonly ApprovalDecision Rejected = new(2, nameof(Rejected));
    public static readonly ApprovalDecision Cancelled = new(3, nameof(Cancelled));

    private ApprovalDecision(int id, string name) : base(id, name) { }
}

public class UserPromotionStatus : DomainEnumeration
{
    public static readonly UserPromotionStatus Evaluating = new(1, nameof(Evaluating));
    public static readonly UserPromotionStatus CriteriaMet = new(2, nameof(CriteriaMet));
    public static readonly UserPromotionStatus PendingApproval = new(3, nameof(PendingApproval));
    public static readonly UserPromotionStatus Promoted = new(4, nameof(Promoted));
    public static readonly UserPromotionStatus Rejected = new(5, nameof(Rejected));

    private UserPromotionStatus(int id, string name) : base(id, name) { }
}

public class DelegationStatus : DomainEnumeration
{
    public static readonly DelegationStatus Active = new(1, nameof(Active));
    public static readonly DelegationStatus Revoked = new(2, nameof(Revoked));
    public static readonly DelegationStatus Expired = new(3, nameof(Expired));

    private DelegationStatus(int id, string name) : base(id, name) { }
}

public class UserDocumentStatus : DomainEnumeration
{
    public static readonly UserDocumentStatus PendingReview = new(1, nameof(PendingReview));
    public static readonly UserDocumentStatus Valid = new(2, nameof(Valid));
    public static readonly UserDocumentStatus Expired = new(3, nameof(Expired));
    public static readonly UserDocumentStatus Rejected = new(4, nameof(Rejected));

    private UserDocumentStatus(int id, string name) : base(id, name) { }
}

public class EnforcementEffect : DomainEnumeration
{
    public static readonly EnforcementEffect BlockAccess = new(1, nameof(BlockAccess));
    public static readonly EnforcementEffect WarnOnly = new(2, nameof(WarnOnly));
    public static readonly EnforcementEffect RequireApproval = new(3, nameof(RequireApproval));
    public static readonly EnforcementEffect Notify = new(4, nameof(Notify));

    private EnforcementEffect(int id, string name) : base(id, name) { }
}
