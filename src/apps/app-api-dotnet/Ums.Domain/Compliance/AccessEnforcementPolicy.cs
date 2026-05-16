namespace Ums.Domain.Compliance;

public sealed class AccessEnforcementPolicy : ParametricCatalogEntity<AccessEnforcementPolicy, AccessEnforcementPolicyProps>
{
    private AccessEnforcementPolicy(AccessEnforcementPolicyProps props) : base(props) { }

    public EnforcementEffect Effect => Props.Effect;
    public string ResourceScope => Props.ResourceScope.GetValue();

    public static Result<AccessEnforcementPolicy> Create(Guid tenantId, string code, string value, string description, EnforcementEffect effect, string resourceScope, string version = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(resourceScope))
            return Result<AccessEnforcementPolicy>.Failure(DomainErrors.Compliance.ResourceScopeRequired);

        var props = new AccessEnforcementPolicyProps
        {
            Effect = effect,
            ResourceScope = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(resourceScope.Trim())
        };

        var policy = new AccessEnforcementPolicy(props);
        var result = policy.SetCatalogFields(tenantId, code, value, description, version);
        if (result.IsFailure)
            return Result<AccessEnforcementPolicy>.Failure(result.Error);

        policy.DomainEvents.ApplyChange(new AccessEnforcementPolicyChangedEvent(tenantId, policy.GetId(), policy.Code), true);
        return Result<AccessEnforcementPolicy>.Success(policy);
    }
}
