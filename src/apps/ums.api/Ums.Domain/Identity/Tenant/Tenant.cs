namespace Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.Tenant.Branch;
using Ums.Domain.Identity.Tenant.IdentityProvider;
using Ums.Domain.Identity.Tenant.Branding;
using Ums.Domain.Identity.Tenant.TenantParameter;
using Ums.Domain.Identity.Tenant.Events;
using BranchEntity = Ums.Domain.Identity.Tenant.Branch.Branch;
using IdentityProviderEntity = Ums.Domain.Identity.Tenant.IdentityProvider.IdentityProvider;
using BrandingEntity = Ums.Domain.Identity.Tenant.Branding.Branding;
using TenantParameterEntity = Ums.Domain.Identity.Tenant.TenantParameter.TenantParameter;

public sealed class Tenant : AggregateRoot<Tenant, TenantProps>
{
    private readonly List<BranchEntity> _branches = new();
    private readonly List<IdentityProviderEntity> _identityProviders = new();
    private readonly List<TenantParameterEntity> _parameters = new();
    private BrandingEntity? _branding;

    public new TenantDomainEventsManager DomainEvents { get; }

    private Tenant(TenantProps props) : base(props)
    {
        DomainEvents = new TenantDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new TenantCreatedEvent(Props.Id.GetValue(), Props.Code.GetValue(), Props.Name.GetValue()));
        }
    }

    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public OrganizationType Type => Props.Type;
    public IdpStrategy IdpStrategy => Props.IdpStrategy;
    public CompanyReference? CompanyReference => Props.CompanyReference;
    public TenantId? ParentTenantId => Props.ParentTenantId;
    public TenantStatus Status => Props.Status;

    public IReadOnlyCollection<BranchEntity> Branches => _branches.AsReadOnly();
    public IReadOnlyCollection<IdentityProviderEntity> IdentityProviders => _identityProviders.AsReadOnly();
    public BrandingEntity? Branding => _branding;
    public IReadOnlyCollection<TenantParameterEntity> Parameters => _parameters.AsReadOnly();

    public IdentityProviderEntity? GetActiveIdentityProvider()
    {
        return _identityProviders.FirstOrDefault(ip => ip.IsActive);
    }

    public static Result<Tenant> Create(
        Code code,
        Name name,
        OrganizationType type,
        ActorId createdBy,
        IdpStrategy idpStrategy = null!,
        CompanyReference? companyReference = null,
        TenantId? parentTenantId = null,
        TenantId? tenantId = null)
    {
        idpStrategy ??= IdpStrategy.InternalBcrypt;

        var id = tenantId ?? IdValueObject.Create();
        var props = new TenantProps(id, code, name, type, idpStrategy, companyReference, parentTenantId, createdBy);

        var tenant = new Tenant(props);

        if (!tenant.IsValid())
        {
            return Result<Tenant>.Failure(tenant.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Tenant>.Success(tenant);
    }

    public Result AddBranch(Code code, Name name, ActorId createdBy, Value? geofencingMetadata = null)
    {
        if (_branches.Any(b => b.Code.Equals(code)))
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Tenant.BranchCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var branchResult = BranchEntity.Create(TenantId.Load(Props.Id.GetValue()), code, name, createdBy, geofencingMetadata);
        if (branchResult.IsFailure)
        {
            return Result.Failure(branchResult.Error);
        }

        _branches.Add(branchResult.Value);
        DomainEvents.RaiseEvent(new BranchCreatedEvent(Props.Id.GetValue(), branchResult.Value.GetId().GetValue(), code.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result RemoveBranch(IdValueObject branchId, ActorId updatedBy)
    {
        var branch = FindBranch(branchId);
        if (branch.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.NotFound));
        }

        if (branch.IsSuccess && branch.Value.IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.Invalid));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _branches.Remove(branch.Value);
        DomainEvents.RaiseEvent(new BranchRemovedEvent(Props.Id.GetValue(), branch.Value.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result DeactivateBranch(IdValueObject branchId, ActorId updatedBy)
    {
        var branch = FindBranch(branchId);
        if (branch.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.NotFound));
        }

        if (branch.IsSuccess)
        {
            var canDeactivate = branch.Value.CanDeactivate();
            if (canDeactivate.IsFailure)
            {
                BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.Invalid));
            }
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        branch.Value.DeactivateInternal();
        DomainEvents.RaiseEvent(new BranchDeactivatedEvent(Props.Id.GetValue(), branch.Value.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result ReactivateBranch(IdValueObject branchId, ActorId updatedBy)
    {
        var branch = FindBranch(branchId);
        if (branch.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.NotFound));
        }

        if (branch.IsSuccess)
        {
            var canReactivate = branch.Value.CanReactivate();
            if (canReactivate.IsFailure)
            {
                BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Common.Invalid));
            }
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        branch.Value.ReactivateInternal();
        DomainEvents.RaiseEvent(new BranchReactivatedEvent(Props.Id.GetValue(), branch.Value.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RegisterIdentityProvider(Code code, Name name, Description description, IdpStrategy strategy, ActorId createdBy)
    {
        if (_identityProviders.Any(ip => ip.Code == code))
        {
            BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var idpResult = IdentityProviderEntity.Create(TenantId.Load(Props.Id.GetValue()), code, name, description, strategy, createdBy);
        if (idpResult.IsFailure)
        {
            return Result.Failure(idpResult.Error);
        }

        _identityProviders.Add(idpResult.Value);
        DomainEvents.RaiseEvent(new IdentityProviderRegisteredEvent(Props.Id.GetValue(), idpResult.Value.GetId().GetValue(), code.GetValue(), strategy.Name));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result ActivateIdentityProvider(IdValueObject identityProviderId, ActorId updatedBy)
    {
        var identityProvider = FindIdentityProvider(identityProviderId);
        if (identityProvider.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpNotFound));
        }

        if (identityProvider.IsSuccess)
        {
            var canActivate = identityProvider.Value.CanActivate();
            if (canActivate.IsFailure)
            {
                BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpAlreadyActive));
            }
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        foreach (var ip in _identityProviders)
        {
            ip.DeactivateInternal();
        }

        identityProvider.Value.ActivateInternal();
        DomainEvents.RaiseEvent(new IdentityProviderActivatedEvent(Props.Id.GetValue(), identityProvider.Value.GetId().GetValue(), identityProvider.Value.Code.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result DeactivateIdentityProvider(IdValueObject identityProviderId, ActorId updatedBy)
    {
        var identityProvider = FindIdentityProvider(identityProviderId);
        if (identityProvider.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpNotFound));
        }

        if (identityProvider.IsSuccess)
        {
            var canDeactivate = identityProvider.Value.CanDeactivate();
            if (canDeactivate.IsFailure)
            {
                BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpAlreadyInactive));
            }
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        identityProvider.Value.DeactivateInternal();
        DomainEvents.RaiseEvent(new IdentityProviderDeactivatedEvent(Props.Id.GetValue(), identityProvider.Value.GetId().GetValue(), identityProvider.Value.Code.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveIdentityProvider(IdValueObject identityProviderId, ActorId updatedBy)
    {
        var identityProvider = FindIdentityProvider(identityProviderId);
        if (identityProvider.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Tenant.IdpNotFound));
        }

        if (identityProvider.IsSuccess && identityProvider.Value.IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IdentityProviders), DomainErrors.Common.Invalid));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _identityProviders.Remove(identityProvider.Value);
        DomainEvents.RaiseEvent(new IdentityProviderRemovedEvent(Props.Id.GetValue(), identityProvider.Value.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result SetBranding(BrandingSettings settings, ActorId createdBy)
    {
        if (_branding is not null)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branding), DomainErrors.Tenant.BrandingAlreadyExists));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var brandingResult = BrandingEntity.Create(TenantId.Load(Props.Id.GetValue()), settings, createdBy);

        if (brandingResult.IsFailure)
        {
            return Result.Failure(brandingResult.Error);
        }

        _branding = brandingResult.Value;
        DomainEvents.RaiseEvent(new BrandingCreatedEvent(Props.Id.GetValue(), _branding.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateBranding(BrandingSettings settings, ActorId updatedBy)
    {
        if (_branding is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branding), DomainErrors.Tenant.BrandingNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = _branding!.Update(settings, updatedBy);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        DomainEvents.RaiseEvent(new BrandingUpdatedEvent(Props.Id.GetValue(), _branding.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result VerifyBrandingDns(ActorId updatedBy)
    {
        if (_branding is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branding), DomainErrors.Tenant.BrandingNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var verifyResult = _branding!.VerifyDns(updatedBy);
        if (verifyResult.IsFailure)
        {
            return Result.Failure(verifyResult.Error);
        }

        DomainEvents.RaiseEvent(new BrandingDnsVerifiedEvent(Props.Id.GetValue(), _branding.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result FailBrandingDns(ActorId updatedBy)
    {
        if (_branding is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branding), DomainErrors.Tenant.BrandingNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var failResult = _branding!.FailDnsVerification(updatedBy);
        if (failResult.IsFailure)
        {
            return Result.Failure(failResult.Error);
        }

        DomainEvents.RaiseEvent(new BrandingDnsFailedEvent(Props.Id.GetValue(), _branding.GetId().GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RemoveBranding(ActorId updatedBy)
    {
        if (_branding is null)
        {
            BrokenRules.Add(new BrokenRule(nameof(Branding), DomainErrors.Tenant.BrandingNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var brandingId = _branding!.GetId().GetValue();
        _branding = null;
        DomainEvents.RaiseEvent(new BrandingRemovedEvent(Props.Id.GetValue(), brandingId));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result AddParameter(
        string code,
        string description,
        string value,
        TenantParameterValueType valueType,
        TenantParameterCategory category,
        bool isSensitive,
        string? defaultValue,
        string? allowedValues,
        ActorId createdBy)
    {
        if (_parameters.Any(p => p.Code.GetValue() == code && p.IsActive))
        {
            BrokenRules.Add(new BrokenRule(nameof(Parameters), DomainErrors.TenantParameter.CodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var parameterResult = TenantParameterEntity.Create(
            TenantId.Load(Props.Id.GetValue()),
            code,
            description,
            value,
            valueType,
            category,
            isSensitive,
            defaultValue,
            allowedValues,
            createdBy);

        if (parameterResult.IsFailure)
        {
            return Result.Failure(parameterResult.Error);
        }

        _parameters.Add(parameterResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateParameter(string code, string newValue, ActorId updatedBy)
    {
        var parameter = FindParameter(code);
        if (parameter.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Parameters), DomainErrors.TenantParameter.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var updateResult = parameter.Value.UpdateValue(newValue, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result DeactivateParameter(string code, ActorId updatedBy)
    {
        var parameter = FindActiveParameter(code);
        if (parameter.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Parameters), DomainErrors.TenantParameter.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var deactivateResult = parameter.Value.Deactivate(updatedBy);
        if (deactivateResult.IsFailure)
        {
            return Result.Failure(deactivateResult.Error);
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public TenantParameterEntity? GetParameter(string code)
    {
        return _parameters.FirstOrDefault(p => p.Code.GetValue() == code && p.IsActive);
    }

    public T? GetTypedParameter<T>(string code, Func<string, T?> parser, T? defaultValue = default)
    {
        var parameter = GetParameter(code);
        if (parameter is null) return defaultValue;

        return parser(parameter.Value);
    }

    private Result<TenantParameterEntity> FindParameter(string code)
    {
        var parameter = _parameters.FirstOrDefault(p => p.Code.GetValue() == code);
        return parameter is null
            ? Result<TenantParameterEntity>.Failure(DomainErrors.TenantParameter.NotFound)
            : Result<TenantParameterEntity>.Success(parameter);
    }

    private Result<TenantParameterEntity> FindActiveParameter(string code)
    {
        var parameter = _parameters.FirstOrDefault(p => p.Code.GetValue() == code && p.IsActive);
        return parameter is null
            ? Result<TenantParameterEntity>.Failure(DomainErrors.TenantParameter.NotFound)
            : Result<TenantParameterEntity>.Success(parameter);
    }

    public Result Suspend(ActorId updatedBy)
    {
        if (Props.Status == TenantStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.ArchivedCannotSuspend));
        }

        if (Props.Status == TenantStatus.Suspended)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.AlreadySuspended));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithStatus(TenantStatus.Suspended));
        DomainEvents.RaiseEvent(new TenantSuspendedEvent(Props.Id.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Activate(ActorId updatedBy)
    {
        if (Props.Status == TenantStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.ArchivedCannotActivate));
        }

        if (Props.Status == TenantStatus.Active)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.AlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithStatus(TenantStatus.Active));
        DomainEvents.RaiseEvent(new TenantActivatedEvent(Props.Id.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<BranchEntity> FindBranch(IdValueObject branchId)
    {
        var branch = _branches.FirstOrDefault(b => b.Props.Id.GetValue() == branchId.GetValue());
        if (branch is null)
            return Result<BranchEntity>.Failure(DomainErrors.Common.NotFound);

        return Result<BranchEntity>.Success(branch);
    }

    private Result<IdentityProviderEntity> FindIdentityProvider(IdValueObject identityProviderId)
    {
        var identityProvider = _identityProviders.FirstOrDefault(ip => ip.Props.Id.GetValue() == identityProviderId.GetValue());
        if (identityProvider is null)
            return Result<IdentityProviderEntity>.Failure(DomainErrors.Common.NotFound);

        return Result<IdentityProviderEntity>.Success(identityProvider);
    }
}
