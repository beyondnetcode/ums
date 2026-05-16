namespace Ums.Domain.Authorization;

public sealed class CompiledPolicyGraph : Entity<CompiledPolicyGraph, CompiledPolicyGraphProps>
{
    private CompiledPolicyGraph(CompiledPolicyGraphProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserId => Props.UserId.GetValue();
    public string GraphHash => Props.GraphHash.GetValue();
    public string Payload => Props.Payload.GetValue();
    public DateTimeOffset CompiledAt => Props.CompiledAt;

    public static Result<CompiledPolicyGraph> Create(Guid tenantId, Guid userId, string graphHash, string payload)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty)
            return Result<CompiledPolicyGraph>.Failure(DomainErrors.CompiledPolicyGraph.IdentifiersRequired);

        if (string.IsNullOrWhiteSpace(graphHash))
            return Result<CompiledPolicyGraph>.Failure(DomainErrors.CompiledPolicyGraph.GraphHashRequired);

        var props = new CompiledPolicyGraphProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(userId),
            global::Ums.Domain.Authorization.ValueObjects.GraphHash.Create(graphHash.Trim()),
            global::Ums.Domain.Authorization.ValueObjects.Payload.Create(string.IsNullOrWhiteSpace(payload) ? "{}" : payload.Trim()));

        return Result<CompiledPolicyGraph>.Success(new CompiledPolicyGraph(props));
    }
}
