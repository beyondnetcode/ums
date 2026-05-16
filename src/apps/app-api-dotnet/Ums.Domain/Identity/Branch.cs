namespace Ums.Domain.Identity;

public sealed class Branch : Entity<Branch, BranchProps>
{
    private Branch(BranchProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string? GeofencingMetadata => Props.GeofencingMetadata?.GetValue();
    public bool IsActive => Props.IsActive;

    public Guid GetId() => Props.Id.GetValue();

    public static Result<Branch> Create(Guid tenantId, string code, string name, string? geofencingMetadata = null)
    {
        if (tenantId == Guid.Empty)
            return Result<Branch>.Failure(DomainErrors.TenantRequired);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);
        if (string.IsNullOrWhiteSpace(name))
            return Result<Branch>.Failure(DomainErrors.NameRequired);

        var props = new BranchProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            geofencingMetadata != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(geofencingMetadata) : null);

        return Result<Branch>.Success(new Branch(props));
    }
}
