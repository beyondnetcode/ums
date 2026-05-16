namespace Ums.Domain.Identity;

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

    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public OrganizationType Type => Props.Type;
    public IdpStrategy IdpStrategy => Props.IdpStrategy;
    public string? CompanyReference => Props.CompanyReference?.GetValue();
    public Guid? ParentTenantId => Props.ParentTenantId?.GetValue();
    public TenantStatus Status => Props.Status;
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    public static Result<Tenant> Create(
        string code,
        string name,
        OrganizationType type,
        IdpStrategy idpStrategy = null!,
        string? companyReference = null,
        Guid? parentTenantId = null)
    {
        idpStrategy ??= IdpStrategy.InternalBcrypt;
        
        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);
        if (string.IsNullOrWhiteSpace(name))
            return Result<Tenant>.Failure(DomainErrors.NameRequired);

        var id = IdValueObject.Create();
        var props = new TenantProps(
            id,
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            type,
            idpStrategy,
            companyReference != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(companyReference.Trim()) : null,
            parentTenantId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(parentTenantId.Value) : null);

        var tenant = new Tenant(props);
        return Result<Tenant>.Success(tenant);
    }

    public Result AddBranch(string code, string name, string? geofencingMetadata = null)
    {
        var branchResult = Branch.Create(Props.Id.GetValue(), code, name, geofencingMetadata);
        if (branchResult.IsFailure)
            return Result.Failure(branchResult.Error);

        if (_branches.Any(branch => branch.Code == branchResult.Value.Code))
            return Result.Failure("Branch code must be unique inside the tenant.");

        _branches.Add(branchResult.Value);
        DomainEvents.ApplyChange(new BranchCreatedEvent(Props.Id.GetValue(), branchResult.Value.GetId(), branchResult.Value.Code), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Suspend()
    {
        if (Props.Status == TenantStatus.Archived)
            return Result.Failure("Archived tenants cannot be suspended.");

        Props.Status = TenantStatus.Suspended;
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Activate()
    {
        if (Props.Status == TenantStatus.Archived)
            return Result.Failure("Archived tenants cannot be activated.");

        Props.Status = TenantStatus.Active;
        Props.Audit.Update("system");
        return Result.Success();
    }
}
