# ADR-0036: Anti-Privilege Escalation via Recursive Scope Validation

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

In a hierarchical multi-tenant system with delegated administration, privilege escalation is the primary security risk. An attacker who compromises a low-level delegated admin should not be able to:

1. Access resources in a tenant at the same or higher hierarchy level.
2. Grant permissions they do not possess to other users.
3. Delegate administration powers beyond the scope they received.
4. Modify policies that constrain their own tenant or its ancestors.
5. Create users in tenants outside their managed scope.

Traditional RBAC checks ("does user have role X?") are insufficient because they do not consider the **hierarchical relationship** between the actor's scope and the target resource.

---

## 2. Architectural Decision

We will implement a **Recursive Scope Validation Pipeline** that verifies every administrative operation against five invariant rules. This pipeline executes as a chain of .NET middleware components, each enforcing one dimension of the security contract.

### 2.1. The Five Invariants

| # | Invariant | Rule | Validation |
|---|---|---|---|
| I1 | **Hierarchy Dominance** | Actor's `taxonomy_rank` must be strictly less than target's `taxonomy_rank` | 1 DB lookup on `tenant_types` |
| I2 | **Scope Containment** | Target tenant must be within actor's `managed_tenants[]` subtree | 1 JOIN on `tenant_closure` |
| I3 | **Grant Non-Expansion** | Actor cannot delegate powers it does not possess | Chain walk of `DelegationGrant` |
| I4 | **Policy Constraint** | Actor cannot modify or delete a MANDATORY policy from a higher level | Policy `inheritance_mode` check |
| I5 | **Revocation Integrity** | If any grant in the delegation chain is revoked, all downstream grants are invalid | Recursive `DelegationGrant` status check |

### 2.2. Scope Validation Middleware Pipeline

```csharp
// Registration in Program.cs
app.UseMiddleware<TenantResolutionMiddleware>()
   .UseMiddleware<HierarchyDominanceMiddleware>()     // I1
   .UseMiddleware<ScopeContainmentMiddleware>()        // I2
   .UseMiddleware<DelegationIntegrityMiddleware>()     // I3 + I5
   .UseMiddleware<PolicyConstraintMiddleware>()        // I4
   .UseMiddleware<AuthorizationDecisionMiddleware>();  // Final decision
```

### 2.3. DelegationGrant Entity

```csharp
public class DelegationGrant : Entity
{
    public Guid GranterUserId { get; private set; }
    public User Granter { get; private set; } = null!;
    public Guid GranteeUserId { get; private set; }
    public User Grantee { get; private set; } = null!;

    // Scope definition
    public Guid RootTenantScopeId { get; private set; }
    public int MaxHierarchyDepth { get; private set; } // 0 = current only, 1 = +children, etc.
    public string[] AllowedResourceTypes { get; private set; } // ["users", "policies", "tenants"]
    public string[] AllowedActions { get; private set; } // ["create", "read", "update", "delete", "delegate"]
    public ManagedRole MaxRole { get; private set; }

    // Chain
    public Guid? ParentGrantId { get; private set; }
    public DelegationGrant? ParentGrant { get; private set; }

    // Control
    public bool CanDelegate { get; private set; }
    public DelegationGrantStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedByUserId { get; private set; }

    public enum DelegationGrantStatus { ACTIVE, REVOKED, EXPIRED, SUSPENDED }

    public Result<DelegationGrant> Create(
        User granter, User grantee, DelegationScope scope, bool canDelegate, Guid? parentGrantId)
    {
        // I3: Validate granter possesses the scope being delegated
        // I1: Validate granter.taxonomy_rank < grantee's applicable rank
    }

    public Result Revoke(Guid revokedByUserId)
    {
        Status = DelegationGrantStatus.REVOKED;
        RevokedAt = DateTime.UtcNow;
        RevokedByUserId = revokedByUserId;
        // I5: Cascade revocation to all child grants
        AddDomainEvent(new DelegationRevokedEvent(Id, revokedByUserId, "Cascade from parent revocation"));
    }
}
```

### 2.4. Invariant I1: Hierarchy Dominance

```csharp
public class HierarchyDominanceMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var actorTenant = context.Items["ActorTenant"] as Tenant;
        var targetTenant = context.Items["TargetTenant"] as Tenant;

        if (targetTenant != null)
        {
            // O(1) rank comparison via taxonomy lookup
            var actorRank = await dbContext.TenantTypes
                .Where(tt => tt.Code == actorTenant.TypeCode)
                .Select(tt => tt.TaxonomyRank)
                .FirstAsync();

            var targetRank = await dbContext.TenantTypes
                .Where(tt => tt.Code == targetTenant.TypeCode)
                .Select(tt => tt.TaxonomyRank)
                .FirstAsync();

            if (actorRank >= targetRank)
            {
                context.Items["AuthorizationResult"] = AuthorizationResult.Deny(
                    "I1", $"Actor rank {actorRank} is not less than target rank {targetRank}");
                await WriteDeniedResponse(context);
                return;
            }
        }

        await next(context);
    }
}
```

### 2.5. Invariant I2: Scope Containment

```csharp
public class ScopeContainmentMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var actorUser = context.Items["ActorUser"] as User;
        var targetTenantId = (Guid?)context.Items["TargetTenantId"];

        if (targetTenantId.HasValue && actorUser.UserType == UserType.DELEGATED_ADMIN)
        {
            // Verify target is within managed subtree using closure table (single JOIN)
            bool isWithinScope = await dbContext.TenantClosure
                .AnyAsync(tc =>
                    tc.DescendantId == targetTenantId.Value
                    && dbContext.TenantAssignments
                        .Where(ta => ta.UserId == actorUser.Id && ta.IsActive)
                        .Select(ta => ta.TenantId)
                        .Contains(tc.AncestorId));

            if (!isWithinScope)
            {
                context.Items["AuthorizationResult"] = AuthorizationResult.Deny(
                    "I2", $"Target tenant {targetTenantId} is outside actor's managed scope");
                await WriteDeniedResponse(context);
                return;
            }
        }

        await next(context);
    }
}
```

### 2.6. Invariant I5: Cascade Revocation

```sql
-- Cascading revocation trigger
CREATE OR REPLACE FUNCTION fn_cascade_delegation_revocation()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status = 'REVOKED' AND OLD.status != 'REVOKED' THEN
        UPDATE delegation_grants
        SET status = 'REVOKED',
            revoked_at = NEW.revoked_at,
            revoked_by_user_id = NEW.revoked_by_user_id
        WHERE parent_grant_id = NEW.id
          AND status = 'ACTIVE';

        -- Log cascade event
        INSERT INTO audit_log (root_tenant_id, actor_user_id, action_type, resource_type, resource_id, metadata)
        SELECT t.root_tenant_id, NEW.revoked_by_user_id, 'delegation.cascade_revoked', 'delegation_grant',
               dg.id, jsonb_build_object('parent_grant_id', NEW.id, 'reason', 'Cascade from parent revocation')
        FROM delegation_grants dg
        JOIN tenants t ON t.id = dg.root_tenant_scope_id
        WHERE dg.parent_grant_id = NEW.id AND dg.status = 'REVOKED';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_cascade_delegation_revocation
    AFTER UPDATE ON delegation_grants
    FOR EACH ROW
    WHEN (NEW.status = 'REVOKED' AND OLD.status != 'REVOKED')
    EXECUTE FUNCTION fn_cascade_delegation_revocation();
```

---

## 3. Consequences

### Positive (Pros)

*   **Defense in depth**: Five independently testable invariants prevent single-point-of-failure in security logic.
*   **O(1) hierarchy validation**: Taxonomy rank eliminates recursive tree walks for the most frequent check (I1).
*   **Explicit chain of trust**: Every delegation is linked to its parent, enabling full traceability and cascade revocation.
*   **Middleware isolation**: Each invariant runs as an independent middleware, allowing selective disabling per endpoint (e.g., public registration).

### Negative (Cons)

*   **Per-request overhead**: 3-5 DB queries per administrative request (types lookup, closure check, delegation chain). Mitigation: Cache taxonomy ranks and validated scopes with TTL 60s.
*   **Cascade revocation cost**: Revoking a deeply nested delegation chain requires recursive updates. Mitigation: Limit delegation depth to 5; batch process with CTE.
*   **False positives risk**: Overly strict invariants could block legitimate cross-tenant data sharing (e.g., federation reports). Mitigation: `tenant_edges` with `edge_type = 'data_sharing'` can create explicit bypass scopes.

---

## 4. Alternatives Considered

1.  **Centralized Policy Decision Point (PDP) with XACML**: Rejected. XACML complexity is disproportionate for the UMS scope. The five-invariant pipeline provides equivalent guarantees at lower implementation cost.

2.  **JWT claim-based validation only**: Rejected. Privilege escalation detection requires evaluating the delegation chain state at request time, which cannot be pre-encoded in a token.

3.  **Application-level checks only (no DB validation)**: Rejected. Defense in depth requires both application middleware and database-level RLS enforcement.
