# ADR-0038: Delegated Administration with Temporal Scopes

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

Enterprise B2B SaaS requires administrative delegation that is:

1. **Temporal**: A support agent from the platform team needs admin access for 48 hours to resolve an incident.
2. **Scoped by resource type**: An operator can manage users but not modify policies or view audit logs.
3. **Depth-limited**: A branch manager can administer their branch and departments beneath it, but not sibling branches.
4. **Revocable in cascade**: Revoking a middle-tier admin must automatically revoke all delegations they granted.
5. **Auditable**: Every delegation (grant, use, revocation) must be recorded as an immutable event.

The current UMS has no delegation model. All users with admin access have flat, permanent, unscoped privileges.

---

## 2. Architectural Decision

We will implement a **DelegationGrant aggregate** that forms a directed acyclic graph (DAG) of delegated authorities rooted at each MasterUser. Every administrative action is validated against the actor's delegation chain, not just their role.

### 2.1. DelegationGrant Entity

```sql
CREATE TABLE delegation_grants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    root_tenant_id UUID NOT NULL REFERENCES tenants(id),

    -- Who
    granter_user_id UUID NOT NULL REFERENCES users(id),
    grantee_user_id UUID NOT NULL REFERENCES users(id),

    -- Scope
    scope_tenant_id UUID NOT NULL REFERENCES tenants(id),
    max_hierarchy_depth INT NOT NULL DEFAULT 0
        CHECK (max_hierarchy_depth BETWEEN 0 AND 5),
    allowed_resource_types TEXT[] NOT NULL DEFAULT '{}',
    allowed_actions TEXT[] NOT NULL DEFAULT '{}',
    max_role VARCHAR(32) NOT NULL,

    -- Chain
    parent_grant_id UUID REFERENCES delegation_grants(id),

    -- Controls
    can_delegate BOOLEAN NOT NULL DEFAULT false,
    status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE'
        CHECK (status IN ('ACTIVE', 'REVOKED', 'EXPIRED', 'SUSPENDED')),
    expires_at TIMESTAMPTZ,
    revoked_at TIMESTAMPTZ,
    revoked_by_user_id UUID REFERENCES users(id),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT grantee_not_granter CHECK (granter_user_id != grantee_user_id),
    CONSTRAINT valid_scope_tree CHECK (
        EXISTS (
            SELECT 1 FROM tenant_closure
            WHERE ancestáor_id = (
                SELECT scope_tenant_id FROM delegation_grants dg2
                WHERE dg2.id = delegation_grants.parent_grant_id
            )
            AND descendant_id = delegation_grants.scope_tenant_id
        )
    )
) PARTITION BY LIST (root_tenant_id);

CREATE INDEX idx_delegation_grantee ON delegation_grants (grantee_user_id, status);
CREATE INDEX idx_delegation_granter ON delegation_grants (granter_user_id, status);
CREATE INDEX idx_delegation_parent ON delegation_grants (parent_grant_id)
    WHERE parent_grant_id IS NOT NULL;
```

### 2.2. Grant Creation Rules

When creating a `DelegationGrant`, the system enforces:

**Rule D1 — Non-Expansion**: The grant's scope (tenant, depth, resource types, actions, max_role) must be a subset of the granter's own effective scope.

```csharp
public Result ValidateNonExpansion(DelegationGrant proposed)
{
    var granterScope = await GetEffectiveScopeAsync(proposed.GranterUserId);

    if (!granterScope.Contains(proposed.ScopeTenantId))
        return Result.Failure("Granter does not have authority over the scope tenant");

    if (proposed.MaxHierarchyDepth > granterScope.MaxHierarchyDepth)
        return Result.Failure("Cannot delegate deeper hierarchy access than possessed");

    if (!granterScope.AllowedResourceTypes.IsSupersetOf(proposed.AllowedResourceTypes))
        return Result.Failure("Cannot delegate resource types not possessed");

    // ... additional checks
}
```

**Rule D2 — Hierarchy Dominance**: The granter must have a `taxonomy_rank` strictly less than the grantee's managed tenant rank.

**Rule D3 — Delegation Depth Limit**: Maximum delegation chain depth is 5 (granter -> grantee -> sub-grantee -> ...). Enforced by the `parent_grant_id` chain walk.

**Rule D4 — Temporal Bound**: All delegations must have an `expires_at` unless granted by a `ROOT` type tenant admin. Default max TTL for delegated admins is 90 days.

### 2.3. Effective Scope Resolution

```csharp
public class EffectiveScopeResolver
{
    public async Task<EffectiveScope> ResolveAsync(Guid userId)
    {
        var user = await dbContext.Users
            .Include(u => u.TenantAssignments)
            .FirstAsync(u => u.Id == userId);

        if (user.UserType == UserType.MASTER)
        {
            // Master users have scope based on their TenantAssignments
            return new EffectiveScope(
                tenantIds: user.TenantAssignments.Select(ta => ta.TenantId).ToList(),
                maxDepth: 5,
                resourceTypes: ALL_RESOURCE_TYPES,
                actions: ALL_ACTIONS,
                maxRole: ManagedRole.TENANT_ADMIN
            );
        }

        if (user.UserType == UserType.DELEGATED_ADMIN)
        {
            // Walk delegation chain to compute effective scope
            var activeGrants = await dbContext.DelegationGrants
                .Where(dg => dg.GranteeUserId == userId && dg.Status == DelegationGrantStatus.ACTIVE)
                .ToListAsync();

            var scopes = activeGrants.Select(g => new
            {
                TenantIds = GetSubtreeIds(g.ScopeTenantId, g.MaxHierarchyDepth),
                g.AllowedResourceTypes,
                g.AllowedActions,
                g.MaxRole,
                g.CanDelegate,
                g.ExpiresAt
            });

            // Union of all active grants (intersection of restrictions)
            return EffectiveScope.Intersection(scopes);
        }

        // Regular users have no delegated scope
        return EffectiveScope.Empty;
    }
}
```

### 2.4. Temporal Enforcement

```csharp
// Background job — runs every 5 minutes
public class DelegationExpirationJob
{
    public async Task ExecuteAsync()
    {
        var expired = await dbContext.DelegationGrants
            .Where(dg => dg.Status == DelegationGrantStatus.ACTIVE
                && dg.ExpiresAt != null
                && dg.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var grant in expired)
        {
            grant.Expire();
        }

        await dbContext.SaveChangesAsync();
    }
}
```

### 2.5. Delegation Audit Events

Every delegation lifecycle event emits a domain event:

| Event | Trigger | Payload |
|---|---|---|
| `DelegationGranted` | Grant created | granter, grantee, scope, parent_grant_id, expires_at |
| `DelegationRevoked` | Grant revoked | revoker, grantee, reason |
| `DelegationExpired` | TTL reached | grant_id, grantee, original_expires_at |
| `DelegationCascadeRevoked` | Parent revoked | root_grant_id, affected_grants[] |
| `DelegationUsed` | Admin action performed via delegation | grant_id, action, target_tenant_id
## 3. Consequences

### Positive (Pros)

*   **Temporal, scoped, revocable**: Full enterprise delegation semantics without permanent privilege grants.
*   **Chain transparency**: Complete audit trail of who delegated what to whom, and who relied on which delegation.
*   **Defense in depth**: Even if a delegated admin account is compromised, the blast radius is limited to their explicit grant scope.
*   **Self-service delegation**: Tenant admins can delegate limited authority without platform team intervention.

### Negative (Cons)

*   **Delegation chain validation overhead**: Every action by a delegated admin requires a chain walk (max 5 hops). Mitigation: Cache resolved effective scope per user with TTL 60s.
*   **Scope intersection complexity**: A user with multiple grants has an effective scope that is the intersection of all grants. This can produce surprising denials. Mitigation: Console UI shows effective scope explicitly.
*   **Orphan grants**: If a granter user is deactivated, their active grants should be reviewed. Mitigation: Background job flags grants from deactivated users.

---

## 4. Alternatives Considered

1.  **Flat roles (Admin / SuperAdmin / Viewer)**: Rejected. Insufficient granularity. Cannot express temporal, scoped, depth-limited delegation.

2.  **OAuth2 delegation (token exchange)**: Rejected. Appropriate for service-to-service but not for human administrative delegation which requires audit trails, cascade revocation, and scope intersection.

3.  **LDAP-style ACL entries on each resource**: Rejected. Administrative overhead of managing individual ACLs on each tenant/policy/user object is unsustainable at enterprise scale.
