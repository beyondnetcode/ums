namespace Ums.Domain.Authorization;

public class CompiledPolicyGraphProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId UserId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.GraphHash GraphHash { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.Payload Payload { get; private set; }
    public DateTimeOffset CompiledAt { get; private set; }

    public CompiledPolicyGraphProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId userId, global::Ums.Domain.Authorization.ValueObjects.GraphHash graphHash, global::Ums.Domain.Authorization.ValueObjects.Payload payload)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        GraphHash = graphHash;
        Payload = payload;
        CompiledAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
