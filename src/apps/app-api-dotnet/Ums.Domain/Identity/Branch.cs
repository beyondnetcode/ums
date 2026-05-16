namespace Ums.Domain.Identity;

using Ums.Domain.Kernel.ValueObjects;

public sealed class Branch : Entity<Branch, BranchProps>
{
    private Branch(BranchProps props) : base(props) { }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Value? GeofencingMetadata => Props.GeofencingMetadata;
    public bool IsActive => Props.IsActive;

    public Guid GetId() => Props.Id.GetValue();

    public static Result<Branch> Create(TenantId tenantId, Code code, Name name, string createdBy, Value? geofencingMetadata = null)
    {
        var props = new BranchProps(
            IdValueObject.Create(),
            tenantId,
            code,
            name,
            geofencingMetadata,
            createdBy);

        var branch = new Branch(props);

        if (!branch.IsValid())
        {
            return Result<Branch>.Failure(branch.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Branch>.Success(branch);
    }

}

