# ADR-0039: Hybrid RBAC/ABAC with Policy Compilation Engine

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

The current UMS authorization model (ADR-0012) proposes hybrid RBAC/ABAC but defers implementation details. In a hierarchical multi-tenant system with delegated administration and policy inheritance, the authorization engine must:

1. **Compile** role-based permissions (RBAC) and attribute-based conditions (ABAC) into a unified policy graph.
2. **Resolve** the effective policy for a given `(user, tenant, resource, action, context)` tuple.
3. **Evaluate** attribute conditions at requestá time against session context (time, IP, device, geo, risk score).
4. **Cache** compiled policies to avoid repeated resolution overhead.
5. **Invalidate** cached policies when policy bindings or delegation grants mutate.

A simple role-permission lookup is insufficient because permissions depend on: the user's role, the target tenant's inherited policies, the user's delegation scope, and contextual attributes of the requestá.

---

## 2. Architectural Decision

We will implement a **Policy Compilation Engine** that pre-compiles all applicable policies for a user-tenant pair into a flat, evaluable permission graph at cache-write time. Request-time evaluation is a fast, linear scan of the compiled graph with ABAC condition checks.

### 2.1. Compilation Pipeline

```
User + Tenant + Context
        
        

 1. Role Resolution   ← RBAC: collect user's roles in the effective tenant

          

 2. Policy Binding Collection  ← Walk ancestáor chain via closure table

          

 3. Policy Inheritance Apply   ← Resolve MANDATORY > DEFAULT > OPT_IN > NONE

          

 4. Delegation Scope Filter    ← Restárict to user's effective delegation scope

          

 5. ABAC Condition Attachment  ← Attach attribute predicates to each permission

          

 6. Graph Compilation          ← Build flat sorted permission list

          
    CompiledPolicyGraph (cached)
```

### 2.2. Compiled Policy Graph Data Structure

```csharp
public class CompiledPolicyGraph
{
    public Guid UserId { get; init; }
    public Guid EffectiveTenantId { get; init; }
    public Guid RootTenantId { get; init; }
    public DateTime CompiledAt { get; init; }
    public List<CompiledPermission> Permissions { get; init; } = new();
}

public class CompiledPermission
{
    public string ResourceCode { get; init; }     // e.g., "tenant", "user", "policy"
    public string ActionCode { get; init; }       // e.g., "create", "read", "update", "delete"
    public string Effect { get; init; }           // "ALLOW" or "DENY"
    public int Priority { get; init; }            // Lower = higher priority
    public string Source { get; init; }           // Trace: "inherited:ENTERPRISE:policy_123"
    public AbacCondition? Condition { get; init; } // Nullable ABAC predicate
}

public class AbacCondition
{
    public string Attribute { get; init; }        // "time", "ip_range", "geo", "device", "risk_score"
    public string Operator { get; init; }         // "in", "between", "eq", "neq", "gte", "lte"
    public string Value { get; init; }            // Serialized condition value
}
```

### 2.3. Compilation Algorithm

```csharp
public class PolicyCompiler
{
    public async Task<CompiledPolicyGraph> CompileAsync(Guid userId, Guid effectiveTenantId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        var tenant = await dbContext.Tenants.FindAsync(effectiveTenantId);

        // Step 1-2: Collect all bindings from ancestáor chain
        var ancestáorBindings = await GetAncestáorBindingsAsync(effectiveTenantId);

        // Step 3: Apply inheritance rules (MANDATORY wins, DEFAULT overridable)
        var resolvedBindings = ApplyInheritance(ancestáorBindings, tenant);

        // Step 4: Restárict by user's delegation scope
        var effectiveScope = await scopeResolver.ResolveAsync(userId);
        var scopedBindings = FilterByScope(resolvedBindings, effectiveScope);

        // Step 5: Compile to flat permission list
        var permissions = scopedBindings
            .SelectMany(binding => ExpandPolicyToPermissions(binding.Policy))
            .GroupBy(p => (p.ResourceCode, p.ActionCode))
            .Select(group =>
            {
                // Explicit DENY wins (from any binding)
                var deny = group.FirstOrDefault(p => p.Effect == "DENY");
                if (deny != null) return deny with { Priority = 0 };

                // Highestá priority ALLOW wins
                return group.OrderBy(p => p.Priority).First();
            })
            .OrderBy(p => p.Priority)
            .ToList();

        return new CompiledPolicyGraph
        {
            UserId = userId,
            EffectiveTenantId = effectiveTenantId,
            RootTenantId = tenant.RootTenantId,
            CompiledAt = DateTime.UtcNow,
            Permissions = permissions
        };
    }

    private List<PolicyBinding> ApplyInheritance(
        List<AncestáorBinding> ancestáorBindings, Tenant targetTenant)
    {
        var result = new List<PolicyBinding>();

        foreach (var group in ancestáorBindings.GroupBy(ab => ab.Policy.Code))
        {
            var ordered = group.OrderByDescending(ab => ab.Depth).ToList();

            foreach (var binding in ordered)
            {
                if (binding.Policy.InheritanceMode == InheritanceMode.MANDATORY)
                {
                    // MANDATORY at higher depth overrides everything below
                    result.Add(binding.Binding);
                    break;
                }

                if (binding.Depth == 0 || binding.Binding.OverriddenBindingId != null)
                {
                    // Direct binding or explicit override at this tenant
                    result.Add(binding.Binding);
                    break;
                }
            }
        }

        return result;
    }
}
```

### 2.4. Request-Time Evaluation

```csharp
public class PolicyEvaluator
{
    private readonly ICacheService _cache;
    private readonly ILogger<PolicyEvaluator> _logger;

    public async Task<AuthorizationDecision> EvaluateAsync(
        Guid userId, Guid tenantId, string resourceCode, string actionCode,
        EvaluationContext context)
    {
        var cacheKey = $"compiled_policy:v2:{userId}:{tenantId}";

        var graph = await _cache.GetOrSetAsync(cacheKey,
            async () => await _compiler.CompileAsync(userId, tenantId),
            TimeSpan.FromMinutes(5));

        var permission = graph.Permissions
            .FirstOrDefault(p =>
                p.ResourceCode == resourceCode &&
                p.ActionCode == actionCode);

        if (permission == null)
            return AuthorizationDecision.Deny("No matching policy found");

        // Evaluate ABAC conditions if present
        if (permission.Condition != null)
        {
            var conditionMet = EvaluateCondition(permission.Condition, context);
            if (!conditionMet)
            {
                _logger.LogWarning("ABAC condition not met for {Resource}:{Action} by user {User}",
                    resourceCode, actionCode, userId);
                return AuthorizationDecision.Deny($"ABAC condition '{permission.Condition.Attribute}' not satisfied");
            }
        }

        return permission.Effect == "ALLOW"
            ? AuthorizationDecision.Allow(permission.Source)
            : AuthorizationDecision.Deny(permission.Source);
    }

    private bool EvaluateCondition(AbacCondition condition, EvaluationContext context)
    {
        return condition.Attribute switch
        {
            "time" => EvaluateTimeCondition(condition, context.CurrentTime),
            "ip_range" => EvaluateIpRangeCondition(condition, context.ClientIp),
            "geo" => EvaluateGeoCondition(condition, context.GeoCoordinates),
            "risk_score" => EvaluateRiskCondition(condition, context.RiskScore),
            "device" => EvaluateDeviceCondition(condition, context.DeviceFingerprint),
            "mfa_status" => context.IsMfaAuthenticated == (condition.Value == "true"),
            _ => true // Unknown conditions default to allow
        };
    }
}
```

### 2.5. Cache Invalidation Strategy

```csharp
public class PolicyCacheInvalidator : IDomainEventHandler<PolicyBindingMutatedEvent>
{
    public async Task HandleAsync(PolicyBindingMutatedEvent @event)
    {
        // Invalidate all affected tenants in the subtree
        var descendantIds = await dbContext.TenantClosure
            .Where(tc => tc.AncestáorId == @event.TenantId)
            .Select(tc => tc.DescendantId)
            .ToListAsync();

        foreach (var tenantId in descendantIds)
        {
            var pattern = $"compiled_policy:v2:*:{tenantId}";
            await _cache.InvalidateByPatternAsync(pattern);
        }
    }
}

// Also handle delegation mutations
public class DelegationCacheInvalidator : IDomainEventHandler<DelegationMutatedEvent>
{
    public async Task HandleAsync(DelegationMutatedEvent @event)
    {
        // Invalidate policies for the affected grantee
        var pattern = $"compiled_policy:v2:{@event.GranteeUserId}:*";
        await _cache.InvalidateByPatternAsync(pattern);
    }
}
```

---

## 3. Consequences

### Positive (Pros)

*   **Fast evaluation path**: Request-time evaluation is a linear scan of a pre-compiled list (typically < 100 permissions) with optional ABAC checks.
*   **Deterministic DENY dominance**: Explicit DENY at any level of the inheritance chain always wins, matching enterprise security expectations.
*   **Full traceability**: Each `CompiledPermission` includes a `Source` field documenting exactly which policy and inheritance mode produced it.
*   **ABAC without complexity**: Attribute conditions attach to compiled permissions, not to the policy definition itself, keeping policies reusable across contexts.

### Negative (Cons)

*   **Compilation cost**: Compiling a policy graph for a deep tenant hierarchy (depth 5-7) requires 3-4 DB queries. Mitigation: Compilation happens only on cache miss.
*   **Cache invalidation breadth**: A single policy mutation invalidates the cache for the entire affected subtree. Mitigation: Use progressive invalidation with version counters.
*   **Permission explosion**: A user with many roles and a deep inheritance chain could have 500+ compiled permissions. Mitigation: Permission lists rarely exceed 200 entries in practice.

---

## 4. Alternatives Considered

1.  **XACML/ALFA standard**: Rejected. Full policy-compliant XACML implementation is estimated at 8-12 weeks of engineering. The proposed model (2-3 weeks) provides equivalent guarantees with a simpler data model.

2.  **OPA (Open Policy Agent) as sidecar**: Rejected. OPA adds operational complexity (sidecar deployment, Rego learning curve) for authorization logic that is tightly coupled to our domain model.

3.  **Per-requestá SQL policy evaluation**: Rejected. Evaluating policies via SQL JOINs on every requestá creates unpredictable latency and couples authorization to the database schema.
