using System.Reflection;
using System.Text.Json;
using Ums.Domain.Enums;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branding;
using Ums.Domain.Identity.Tenant.Branch;
using Ums.Domain.Identity.Tenant.IdentityProvider;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Identity.UserAccount.MfaEnrollment;
using Ums.Domain.Identity.UserAccount.PasswordCredential;
using Ums.Domain.Identity.UserManagementDelegation;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;
using UserManagementDelegationAggregate = Ums.Domain.Identity.UserManagementDelegation.UserManagementDelegation;

internal static class IdentityAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static TenantAggregate RehydrateTenant(
        TenantRecord tenantRecord,
        IReadOnlyCollection<TenantBranchRecord> branchRecords,
        IReadOnlyCollection<TenantIdentityProviderRecord> providerRecords,
        TenantBrandingRecord? brandingRecord)
    {
        var props = new TenantProps(
            TenantId.Load(tenantRecord.Id),
            Code.Create(tenantRecord.Code),
            Name.Create(tenantRecord.Name),
            DomainEnumerationMapper.FromValue<OrganizationType>(tenantRecord.OrganizationTypeId),
            DomainEnumerationMapper.FromValue<IdpStrategy>(tenantRecord.IdpStrategyId),
            string.IsNullOrWhiteSpace(tenantRecord.CompanyReference) ? null : CompanyReference.Create(tenantRecord.CompanyReference),
            tenantRecord.ParentTenantId.HasValue ? TenantId.Load(tenantRecord.ParentTenantId.Value) : null,
            ActorId.Create(tenantRecord.CreatedBy));

        props.Status = DomainEnumerationMapper.FromValue<TenantStatus>(tenantRecord.StatusId);
        SetAudit(props, tenantRecord.CreatedBy, tenantRecord.CreatedAtUtc, tenantRecord.UpdatedBy, tenantRecord.UpdatedAtUtc, tenantRecord.AuditTimeSpan);

        var tenant = Construct<TenantAggregate, TenantProps>(props);

        var branches = branchRecords.Select(RehydrateBranch).ToList();
        var providers = providerRecords.Select(RehydrateIdentityProvider).ToList();
        var branding = brandingRecord is null ? null : RehydrateBranding(brandingRecord);

        SetField(tenant, "_branches", branches);
        SetField(tenant, "_identityProviders", providers);
        SetField(tenant, "_branding", branding);
        tenant.DomainEvents.MarkChangesAsCommitted();
        tenant.BrokenRules.Clear();

        return tenant;
    }

    public static UserAccountAggregate RehydrateUserAccount(
        UserAccountRecord record,
        IReadOnlyCollection<UserAccountMfaEnrollmentRecord> enrollmentRecords,
        IReadOnlyCollection<UserAccountPasswordCredentialRecord> passwordRecords)
    {
        var props = new UserAccountProps(
            UserAccountId.Load(record.Id),
            TenantId.Load(record.TenantId),
            Email.Create(record.Email),
            DomainEnumerationMapper.FromValue<UserCategory>(record.CategoryId),
            string.IsNullOrWhiteSpace(record.IdentityReference) ? null : IdentityReference.Create(record.IdentityReference),
            record.IdentityReferenceTypeId.HasValue ? DomainEnumerationMapper.FromValue<IdentityReferenceType>(record.IdentityReferenceTypeId.Value) : null,
            ActorId.Create(record.CreatedBy));

        props.BranchId = record.BranchId.HasValue ? BranchId.Load(record.BranchId.Value) : null;
        props.Status = DomainEnumerationMapper.FromValue<UserStatus>(record.StatusId);
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var account = Construct<UserAccountAggregate, UserAccountProps>(props);

        var enrollments = enrollmentRecords.Select(RehydrateEnrollment).ToList();
        var passwords = passwordRecords.Select(RehydratePasswordCredential).ToList();

        SetField(account, "_mfaEnrollments", enrollments);
        SetField(account, "_passwordCredentials", passwords);
        account.DomainEvents.MarkChangesAsCommitted();
        account.BrokenRules.Clear();

        return account;
    }

    public static UserManagementDelegationAggregate RehydrateUserManagementDelegation(UserManagementDelegationRecord record)
    {
        var actionIds = JsonSerializer.Deserialize<List<int>>(record.AllowedActionsJson) ?? [];
        var allowedActions = actionIds
            .Select(id => DomainEnumerationMapper.FromValue<DelegatedAction>(id))
            .ToList();

        var props = new UserManagementDelegationProps(
            DelegationId.Load(record.Id),
            TenantId.Load(record.TenantId),
            UserAccountId.Load(record.DelegatingAdminId),
            UserAccountId.Load(record.DelegatedAdminId),
            DomainEnumerationMapper.FromValue<DelegationScopeType>(record.ScopeTypeId),
            record.ScopeId,
            allowedActions,
            record.ValidFrom,
            record.ValidUntil,
            record.MaxDurationDays,
            record.RequiresApproval,
            ActorId.Create(record.CreatedBy));

        props.Status = DomainEnumerationMapper.FromValue<DelegationStatus>(record.StatusId);
        props.ApprovalRequestId = record.ApprovalRequestId;
        props.RevokedAt = record.RevokedAt;
        props.RevokedBy = record.RevokedBy;
        props.RevocationReason = record.RevocationReason;
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var delegation = Construct<UserManagementDelegationAggregate, UserManagementDelegationProps>(props);
        delegation.DomainEvents.MarkChangesAsCommitted();
        delegation.BrokenRules.Clear();

        return delegation;
    }

    private static Branch RehydrateBranch(TenantBranchRecord record)
    {
        var props = new BranchProps(
            BranchId.Load(record.Id),
            TenantId.Load(record.TenantId),
            Code.Create(record.Code),
            Name.Create(record.Name),
            string.IsNullOrWhiteSpace(record.GeofencingMetadata) ? null : Value.Create(record.GeofencingMetadata),
            ActorId.Create(record.CreatedBy));

        props.IsActive = record.IsActive;
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<Branch, BranchProps>(props);
    }

    private static IdentityProvider RehydrateIdentityProvider(TenantIdentityProviderRecord record)
    {
        var props = new IdentityProviderProps(
            IdentityProviderId.Load(record.Id),
            TenantId.Load(record.TenantId),
            Code.Create(record.Code),
            Name.Create(record.Name),
            Description.Create(record.Description),
            DomainEnumerationMapper.FromValue<IdpStrategy>(record.StrategyId),
            ActorId.Create(record.CreatedBy));

        props.IsActive = record.IsActive;
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<IdentityProvider, IdentityProviderProps>(props);
    }

    private static Branding RehydrateBranding(TenantBrandingRecord record)
    {
        var props = new BrandingProps(
            BrandingId.Load(record.Id),
            TenantId.Load(record.TenantId),
            Logo.Create(record.Logo),
            DomainEnumerationMapper.FromValue<LogoFormat>(record.LogoFormatId),
            HexColor.Create(record.PrimaryColor),
            DomainEnumerationMapper.FromValue<BackgroundStyle>(record.BackgroundStyleId),
            LoginText.Create(record.HeadlineText),
            LoginText.Create(record.SecondaryText),
            LoginText.Create(record.PrimaryButtonLabel),
            LoginText.Create(record.FooterText),
            string.IsNullOrWhiteSpace(record.CustomDomain) ? null : CustomDomain.Create(record.CustomDomain),
            DnsCnameTarget.Create(),
            record.MagicLinkFallbackEnabled,
            ActorId.Create(record.CreatedBy));

        props.DnsVerificationStatus = DomainEnumerationMapper.FromValue<DnsVerificationStatus>(record.DnsVerificationStatusId);
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<Branding, BrandingProps>(props);
    }

    private static MfaEnrollment RehydrateEnrollment(UserAccountMfaEnrollmentRecord record)
    {
        var props = new MfaEnrollmentProps(
            MfaEnrollmentId.Load(record.Id),
            UserAccountId.Load(record.UserAccountId),
            DomainEnumerationMapper.FromValue<MfaMethod>(record.MethodId),
            ActorId.Create(record.CreatedBy));

        props.Status = DomainEnumerationMapper.FromValue<MfaEnrollmentStatus>(record.StatusId);
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<MfaEnrollment, MfaEnrollmentProps>(props);
    }

    private static PasswordCredential RehydratePasswordCredential(UserAccountPasswordCredentialRecord record)
    {
        var props = new PasswordCredentialProps(
            PasswordCredentialId.Load(record.Id),
            UserAccountId.Load(record.UserAccountId),
            PasswordHash.Create(record.PasswordHash),
            ActorId.Create(record.CreatedBy));

        props.IsActive = record.IsActive;
        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<PasswordCredential, PasswordCredentialProps>(props);
    }

    private static TEntity Construct<TEntity, TProps>(TProps props)
        where TEntity : class
        where TProps : class
    {
        var ctor = typeof(TEntity).GetConstructor(InstanceFlags, null, [typeof(TProps)], null)
            ?? throw new InvalidOperationException($"Constructor for {typeof(TEntity).Name} not found.");

        return (TEntity)ctor.Invoke([props]);
    }

    private static void SetField<TTarget>(object target, string fieldName, TTarget value)
    {
        var field = target.GetType().GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field {fieldName} not found on {target.GetType().Name}.");

        field.SetValue(target, value);
    }

    private static void SetAudit(object props, string createdBy, DateTime createdAtUtc, string? updatedBy, DateTime? updatedAtUtc, string auditTimeSpan)
    {
        var property = props.GetType().GetProperty("Audit", InstanceFlags)
            ?? throw new InvalidOperationException($"Audit property not found on {props.GetType().Name}.");

        property.SetValue(props, AuditValueObject.Load(new AuditProps
        {
            CreatedBy = createdBy,
            CreatedAt = createdAtUtc,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAtUtc,
            TimeSpan = auditTimeSpan,
        }));
    }
}
