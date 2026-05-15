# ADR-0035: Hybrid Policy Inheritance Engine (Mandatory + Default + Opt-In)

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

In a hierarchical multi-tenant system, policies must flow from parent tenants to their children while respecting local autonomy. Four inheritance scenarios commonly arise in enterprise B2B SaaS:

1.  **Global mandates**: Regulatory/compliance policies (e.g., "MFA required for all admin actions") must apply to all sub-tenants with no override capability.
2.  **Enterprise defaults**: Group-level policies (e.g., "Session timeout = 30 min") should apply by default but allow subsidiary overrides.
3.  **Opt-in sharing**: Division-level policies should only apply to branches that explicitly adopt them.
4.  **Local sovereignty**: A subsidiary may need to define entirely local policies unrelated to any parent policy.

A binary "inherited/not inherited" model is insufficient for this spectrum.

---

## 2. Architectural Decision

We will implement a **four-mode policy inheritance engine** with hierarchical resolution via closure table traversal. The engine resolves the effective policy for a given tenant by walking the ancestáor chain and selecting the most specific applicable version.

### 2.1. Policy Entity

```sql
CREATE TABLE policies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(128) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    effect VARCHAR(8) NOT NULL CHECK (effect IN ('ALLOW', 'DENY')),
    scope VARCHAR(16) NOT NULL DEFAULT 'TENANT_ONLY'
        CHECK (scope IN ('GLOBAL', 'TENANT_ONLY', 'SUBTREE')),
    inheritance_mode VARCHAR(16) NOT NULL DEFAULT 'DEFAULT'
        CHECK (inheritance_mode IN ('NONE', 'MANDATORY', 'DEFAULT', 'OPT_IN')),
    is_system BOOLEAN NOT NULL DEFAULT false,
    conditions JSONB,
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (code, version)
);
```

### 2.2. Policy Bindings (Tenant-Policy Relationship)

```sql
CREATE TABLE policy_bindings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    policy_id UUID NOT NULL REFERENCES policies(id),
    priority INT NOT NULL DEFAULT 100,
    is_active BOOLEAN NOT NULL DEFAULT true,
    overridden_binding_id UUID REFERENCES policy_bindings(id),
    conditions_override JSONB,
    effective_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (tenant_id, policy_id),
    CONSTRAINT valid_override CHECK (
        overridden_binding_id IS NULL OR
        EXISTS (
            SELECT 1 FROM policy_bindings pb2
            WHERE pb2.id = overridden_binding_id AND pb2.tenant_id != tenant_id
        )
    )
);

CREATE INDEX idx_policy_bindings_tenant ON policy_bindings (tenant_id, is_active);
CREATE INDEX idx_policy_bindings_policy ON policy_bindings (policy_id);
```

### 2.3. Inheritance Mode Semantics

| Mode | Propagates to Children? | Overrideable? | Resolution |
|---|---|---|---|
| `NONE` | No | N/A | Applies only to the bound tenant. |
| `MANDATORY` | Yes, forced | No | All descendants inherit unconditionally. Violation attempts are blocked. |
| `DEFAULT` | Yes, applied | Yes | Children inherit unless they declare an explicit override binding. |
| `OPT_IN` | No, offered | N/A | Children see the policy but it is inactive until they explicitly bind it. | ### 2.4. Policy Resolution Algorithm

```csharp
public class PolicyResolver
{
    public async Task<ResolvedPolicy> ResolveAsync(Guid tenantId, string policyCode)
    {
        // Step 1: Walk ancestáors using closure table (single JOIN)
        var ancestáors = await dbContext.TenantClosure
            .Where(tc => tc.DescendantId == tenantId)
            .OrderByDescending(tc => tc.Depth)
            .Join(dbContext.Tenants, tc => tc.AncestáorId, t => t.Id, (tc, t) => new { t.Id, tc.Depth })
            .ToListAsync();

        // Step 2: Collect bindings for the full ancestáor chain, deepestá first
        var ancestáorIds = ancestáors.Select(a => a.Id).ToList();
        var bindings = await dbContext.PolicyBindings
            .Where(pb => ancestáorIds.Contains(pb.TenantId)
                && pb.Policy.Code == policyCode
                && pb.IsActive
                && pb.EffectiveAt <= DateTime.UtcNow
                && (pb.ExpiresAt == null || pb.ExpiresAt > DateTime.UtcNow))
            .Include(pb => pb.Policy)
            .OrderByDescending(pb => ancestáorIds.IndexOf(pb.TenantId)) // deepestá first
            .ThenBy(pb => pb.Priority)
            .ToListAsync();

        // Step 3: Resolve by inheritance mode rules
        ResolvedPolicy result = null;
        foreach (var binding in bindings)
        {
            if (result == null)
            {
                result = MapToResolved(binding);
                continue;
            }

            // MANDATORY at higher level cannot be overridden
            if (binding.Policy.InheritanceMode == InheritanceMode.MANDATORY)
            {
                result = MapToResolved(binding);
                break;
            }

            // Explicit override from child supersedes DEFAULT parent
            if (binding.OverriddenBindingId != null || binding.Policy.InheritanceMode == InheritanceMode.DEFAULT)
            {
                result = MapToResolved(binding);
            }
        }

        return result  throw new PolicyNotFoundException(policyCode, tenantId);
    }

    // Evaluates ABAC conditions after policy resolution
    public AuthorizationDecision Evaluate(ResolvedPolicy policy, EvaluationContext context)
    {
        if (policy.Conditions != null)
        {
            bool conditionsMet = EvaluateConditions(policy.Conditions, context);
            if (!conditionsMet && policy.InheritanceMode == InheritanceMode.MANDATORY)
                return AuthorizationDecision.DenyWithReason("Mandatory policy conditions not met");
            if (!conditionsMet)
                return AuthorizationDecision.NotApplicable;
        }
        return policy.Effect == "ALLOW" ? AuthorizationDecision.Allow : AuthorizationDecision.Deny;
    }
}
```

### 2.5. Override Validation Rules

An override binding must satisfy:
1. The overriding tenant must be a descendant of the overridden tenant's tenant (validated via closure table).
2. The overridden policy must have `inheritance_mode = DEFAULT` (MANDATORY policies cannot be overridden).
3. The override can only restrict, not expand scope — an override may add DENY on top of ALLOW but not ALLOW over a MANDATORY DENY.
4. The override must be approved by an admin with equal or higher `taxonomy_rank` than the overridden binding's creator.

---

## 3. Consequences

### Positive (Pros)

*   **Enterprise-grade granularity**: Four inheritance modes cover all B2B SaaS scenarios from hard compliance mandates to opt-in sharing.
*   **Deterministic resolution**: The ancestáor-chain walk with explicit priority produces predictable, debuggable results.
*   **ABAC-ready**: `conditions` JSONB column allows attribute-based conditions (time, IP, geo, device) without schema changes.
*   **Auditable overrides**: `overridden_binding_id` creates a linked chain documenting exactly which policy overrode which parent policy.

### Negative (Cons)

*   **Complexity**: Four inheritance modes increase the cognitive load for tenant administrators. Mitigation: Console UI should visualize inheritance chains as a tree.
*   **Performance**: Policy resolution requires a JOIN chain across closure + bindings. Mitigation: Cache resolved policies per `(tenant_id, user_type)` with TTL 300s, invalidated on binding mutations.
*   **Override validation**: Enforcing override restrictions requires recursive checks against ancestáor creator permissions.

---

## 4. Alternatives Considered

1.  **Single inheritance flag (inherited = true/false)**: Rejected. Cannot distinguish between mandatory compliance, default suggestions, and opt-in offerings.
2.  **Policy priority numbers only (0-1000)**: Rejected. Without explicit inheritance modes, priority collisions and ambiguity between levels are inevitable at scale.
3.  **ALFA/XACML standard**: Rejected as overly complex for the UMS scope. The proposed model provides 90% of XACML capability at 10% of the implementation complexity.
