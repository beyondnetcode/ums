namespace Ums.Domain.Identity.Tenant.Branch;

public sealed class Branch : Entity<Branch, BranchProps>
{
    private Branch(BranchProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Value? GeofencingMetadata => Props.GeofencingMetadata;
    public bool IsActive => Props.IsActive;

    public BranchId GetId() => BranchId.Load(Props.Id.GetValue());

    public static Result<Branch> Create(TenantId tenantId, Code code, Name name, ActorId createdBy, Value? geofencingMetadata = null)
    {
        var props = new BranchProps(IdValueObject.Create(), tenantId, code, name, geofencingMetadata, createdBy);
        var branch = new Branch(props);

        if (!branch.IsValid())
        {
            return Result<Branch>.Failure(branch.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Branch>.Success(branch);
    }

    internal Result CanDeactivate()
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Common.Invalid));
        }

        return IsValid()
            ? Result.Success()
            : Result.Failure(BrokenRules.GetBrokenRulesAsString());
    }

    internal Result CanReactivate()
    {
        if (IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Common.Invalid));
        }

        return IsValid()
            ? Result.Success()
            : Result.Failure(BrokenRules.GetBrokenRulesAsString());
    }

    internal void DeactivateInternal()
    {
        Props.IsActive = false;
    }

    internal void ReactivateInternal()
    {
        Props.IsActive = true;
    }
}
