namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;

public sealed class Tenant : AggregateRoot<Tenant, TenantProps>
{
    private readonly List<Branch> _branches = new();

    private Tenant(TenantProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new TenantCreatedEvent(Props.Id.GetValue(), Props.Code.GetValue(), Props.Name.GetValue()), true);
        }
    }

    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public OrganizationType Type => Props.Type;
    public IdpStrategy IdpStrategy => Props.IdpStrategy;
    public Value? CompanyReference => Props.CompanyReference;
    public TenantId? ParentTenantId => Props.ParentTenantId;
    public TenantStatus Status => Props.Status;
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    public static Result<Tenant> Create(
        Code code,
        Name name,
        OrganizationType type,
        string createdBy,
        IdpStrategy idpStrategy = null!,
        Value? companyReference = null,
        TenantId? parentTenantId = null)
    {
        idpStrategy ??= IdpStrategy.InternalBcrypt;
        
        var id = IdValueObject.Create();
        var props = new TenantProps(
            id,
            code,
            name,
            type,
            idpStrategy,
            companyReference,
            parentTenantId,
            createdBy);

        var tenant = new Tenant(props);

        if (!tenant.IsValid())
        {
            return Result<Tenant>.Failure(tenant.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Tenant>.Success(tenant);
    }


    public Result AddBranch(Code code, Name name, string updatedBy, Value? geofencingMetadata = null)
    {
        var branchResult = Branch.Create(TenantId.Load(Props.Id.GetValue()), code, name, updatedBy, geofencingMetadata);
        if (branchResult.IsFailure)
            return Result.Failure(branchResult.Error);

        if (_branches.Any(branch => branch.Code == branchResult.Value.Code))
        {
            BrokenRules.Add(new BrokenRule(nameof(Branches), DomainErrors.Tenant.BranchCodeNotUnique));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _branches.Add(branchResult.Value);
        DomainEvents.ApplyChange(new BranchCreatedEvent(Props.Id.GetValue(), branchResult.Value.GetId(), branchResult.Value.Code.GetValue()), true);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }

    public Result Suspend(string updatedBy)
    {
        if (Props.Status == TenantStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.ArchivedCannotSuspend));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = TenantStatus.Suspended;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }

    public Result Activate(string updatedBy)
    {
        if (Props.Status == TenantStatus.Archived)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Tenant.ArchivedCannotActivate));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = TenantStatus.Active;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        return Result.Success();
    }
}
