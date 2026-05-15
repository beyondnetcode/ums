# ADR-0040: Federated Token Strategy for Hierarchical Context

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

In a hierarchical multi-tenant system with delegated administration and policy inheritance, the authentication token must carry enough context for downstream services to authorize requests without repeated database lookups. However, embedding the full hierarchical context in a JWT leads to token bloat.

The following claims grow with tenant hierarchy depth and delegation chain length:

- `root_tenant_id`, `effective_tenant_id`, `hierarchy_level`, `hierarchy_path`
- `user_type`, `roles[]`, `managed_tenants[]`
- `delegation_chain[]` each with scope and TTL
- `resolved_policies[]` for fast authorization

At 7 hierarchy levels with 5 delegation hops and 50 resolved policies, a self-contained JWT exceeds 8KB — exceeding HTTP header size limits in many proxy and gateway configurations.

---

## 2. Architectural Decision

We will adopt a **dual-mode token strategy**: JWT for BFF-to-Frontend communication where latency and payload size are critical, and Opaque Reference Tokens with Token Introspection for service-to-service and cross-boundary communication where full context is required.

### 2.1. Token Strategy Matrix

| Scenario | Token Type | Rationale |
|---|---|---|
| Frontend ↔ BFF (browser) | JWT (minimal claims) | Header size < 2KB, no introspection latency |
| BFF ↔ Backend API (internal) | Opaque (reference token) | Full context via introspection, header size < 200 bytes |
| Backend ↔ Backend (gRPC) | Opaque + mTLS | Service identity + full user context |
| External IdP ↔ UMS | JWT (from IdP) | Standard OAuth2/OIDC, transformed on entry |
| Long-lived machine tokens | Opaque (PAT) | Revocable, scoped, auditable | ### 2.2. JWT for Frontend (Minimal Claims)

```json
{
  "sub": "U-abc-123",
  "email": "admin@subsidiary.com",
  "root_tenant_id": "T-root-001",
  "effective_tenant_id": "T-sub-045",
  "hierarchy_level": 2,
  "user_type": "TENANT_ADMIN",
  "roles": ["subsidiary.admin"],
  "iat": 1700000000,
  "exp": 1700003600,
  "jti": "unique-token-id-456"
}
```

Size: ~450 bytes. Claims limited to identity and basic context. Authorization decisions are made server-side after token introspection or internal lookup.

### 2.3. Opaque Token for Service-to-Service

```csharp
public class OpaqueTokenService
{
    public async Task<string> CreateOpaqueTokenAsync(User user, Tenant effectiveTenant, DelegationScope? scope)
    {
        var tokenId = Guid.NewGuid();

        // Store full context in Redis with TTL matching token expiry
        var tokenContext = new TokenContext
        {
            UserId = user.Id,
            UserEmail = user.Email,
            UserType = user.UserType,
            RootTenantId = effectiveTenant.RootTenantId,
            EffectiveTenantId = effectiveTenant.Id,
            HierarchyLevel = effectiveTenant.HierarchyLevel,
            HierarchyPath = await GetHierarchyPathAsync(effectiveTenant.Id),
            Roles = await GetUserRolesAsync(user.Id, effectiveTenant.Id),
            ManagedTenantIds = user.TenantAssignments
                .Where(ta => ta.IsActive)
                .Select(ta => ta.TenantId)
                .ToList(),
            DelegationGrantId = scope?.GrantId,
            DelegationScope = scope,
            ResolvedPolicies = await GetResolvedPoliciesAsync(user.Id, effectiveTenant.Id),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _cache.SetAsync(
            $"token_context:{tokenId}",
            JsonSerializer.Serialize(tokenContext),
            TimeSpan.FromHours(1));

        // Return short reference token (no embedded claims)
        var referenceToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"ums_ref_v2:{tokenId}")
);

        return referenceToken;
    }

    public async Task<TokenContext?> IntrospectAsync(string referenceToken)
    {
        // Parse and validate token format
        if (!TryParseReferenceToken(referenceToken, out var tokenId))
            return null;

        // Retrieve from cache
        var cached = await _cache.GetAsync($"token_context:{tokenId}");
        if (cached == null) return null;

        var context = JsonSerializer.Deserialize<TokenContext>(cached);

        // Check expiry
        if (context.ExpiresAt < DateTime.UtcNow) return null;

        // Validate delegation grants are still active
        if (context.DelegationGrantId.HasValue)
        {
            var grantActive = await _cache.GetAsync($"delegation_active:{context.DelegationGrantId}");
            if (grantActive == null || grantActive == "false")
            {
                // Fallback to DB check
                var grant = await _dbContext.DelegationGrants.FindAsync(context.DelegationGrantId.Value);
                if (grant == null || grant.Status != DelegationGrantStatus.ACTIVE)
                {
                    await _cache.SetAsync($"delegation_active:{context.DelegationGrantId}", "false", TimeSpan.FromMinutes(5));
                    return null;
                }
                await _cache.SetAsync($"delegation_active:{context.DelegationGrantId}", "true", TimeSpan.FromMinutes(5));
            }
        }

        return context;
    }
}
```

### 2.4. Token Introspection Middleware

```csharp
public class TokenIntrospectionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..];

            // Detect token type
            if (IsOpaqueToken(token))
            {
                var tokenContext = await _opaqueTokenService.IntrospectAsync(token);
                if (tokenContext == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token expired, revoked, or invalid");
                    return;
                }

                // Hydrate HttpContext with full token context
                context.Items["UserId"] = tokenContext.UserId;
                context.Items["UserEmail"] = tokenContext.UserEmail;
                context.Items["UserType"] = tokenContext.UserType;
                context.Items["RootTenantId"] = tokenContext.RootTenantId;
                context.Items["EffectiveTenantId"] = tokenContext.EffectiveTenantId;
                context.Items["HierarchyLevel"] = tokenContext.HierarchyLevel;
                context.Items["HierarchyPath"] = tokenContext.HierarchyPath;
                context.Items["Roles"] = tokenContext.Roles;
                context.Items["ManagedTenantIds"] = tokenContext.ManagedTenantIds;
                context.Items["DelegationGrantId"] = tokenContext.DelegationGrantId;
                context.Items["ResolvedPolicies"] = tokenContext.ResolvedPolicies;

                // Set RLS context
                await SetRlsSessionVariables(context, tokenContext);
            }
            else
            {
                // JWT path — validate and extract minimal claims
                var principal = await _jwtValidator.ValidateAsync(token);
                if (principal == null)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                context.Items["UserId"] = principal.FindFirst("sub")?.Value;
                context.Items["RootTenantId"] = principal.FindFirst("root_tenant_id")?.Value;
                context.Items["EffectiveTenantId"] = principal.FindFirst("effective_tenant_id")?.Value;
            }
        }

        await next(context);
    }

    private async Task SetRlsSessionVariables(HttpContext context, TokenContext tokenContext)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand("SET LOCAL app.root_tenant_id = @p1", connection);
        cmd.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.Uuid, tokenContext.RootTenantId);
        await cmd.ExecuteNonQueryAsync();

        await using var cmd2 = new NpgsqlCommand("SET LOCAL app.effective_tenant_id = @p1", connection);
        cmd2.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.Uuid, tokenContext.EffectiveTenantId);
        await cmd2.ExecuteNonQueryAsync();

        await using var cmd3 = new NpgsqlCommand("SET LOCAL app.user_id = @p1", connection);
        cmd3.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.Uuid, tokenContext.UserId);
        await cmd3.ExecuteNonQueryAsync();
    }
}
```

### 2.5. Token Lifecycle

```
                Issue                    Introspect               Revoke
                                                                  
                                                                  
            
     OpaqueTokenService        TokenIntrospection        TokenRevoker 
     .Create()                 .Introspect()             .Revoke()    
                                                                      
     Store → Redis TTL 1h      Get ← Redis               Del ← Redis  
     Return ref token          Return TokenContext        + audit log  
            
```

### 2.6. Token Revocation

```csharp
public class TokenRevoker
{
    public async Task RevokeUserTokensAsync(Guid userId, string reason)
    {
        // Invalidate all token contexts for this user
        var pattern = $"token_context:*";
        // Note: in production, use Redis SCAN with a user-indexed key
        await _cache.InvalidateByPatternAsync($"token_context:*");

        // Also invalidate delegation grants associated with this user
        var activeGrants = await _dbContext.DelegationGrants
            .Where(dg => dg.GranteeUserId == userId && dg.Status == DelegationGrantStatus.ACTIVE)
            .ToListAsync();

        foreach (var grant in activeGrants)
        {
            await _cache.SetAsync($"delegation_active:{grant.Id}", "false", TimeSpan.FromMinutes(5));
        }

        _logger.LogInformation("All tokens revoked for user {UserId}: {Reason}", userId, reason);
    }
}
```

---

## 3. Consequences

### Positive (Pros)

*   **Small JWT for frontend**: ~450 bytes vs 8KB+ for self-contained tokens. Fits within HTTP header limits.
*   **Full context for services**: Opaque tokens carry the complete delegation chain, resolved policies, and managed tenants without header bloat.
*   **Immediate revocation**: Delete from Redis = token invalidated instantly. No refresh token wait, no blacklist scan.
*   **Audit trail**: Token issuance, introspection, and revocation are logged with full context.

### Negative (Cons)

*   **Introspection latency**: Each service-to-service call requires a Redis lookup (1-3ms). Mitigation: The token context is cached for the token's TTL; introspection is a single key lookup.
*   **Redis dependency**: Token validation depends on Redis availability. Mitigation: Local in-memory cache as L2 fallback; circuit breaker if Redis is down.
*   **Token state management**: Revoked tokens remain in Redis until TTL expiry. Mitigation: Set TTL to match token expiry (max 1 hour); revoke proactively deletes.
*   **Operational complexity**: Two token types increase testing surface. Mitigation: Token type detection is a simple format check (opaque starts with `ums_ref_v2:`).

---

## 4. Alternatives Considered

1.  **Self-contained JWT with all claims**: Rejected. At 8KB+, exceeds header limits in Kong, AWS ALB, and many ingress controllers. Also increases per-request bandwidth by 8KB.

2.  **JWT with distributed claims (JWT + separate claims endpoint)**: Rejected. Claims endpoint creates the same introspection dependency but with non-standard implementation. Opaque tokens are a standard pattern (RFC 7662).

3.  **Session cookies only**: Rejected. Not suitable for service-to-service or machine-to-machine scenarios. Sessions also introduce CSRF considerations.

4.  **PASETO tokens**: Rejected. While PASETO addresses some JWT security concerns, it does not solve the token bloat problem and has less ecosystem support.
