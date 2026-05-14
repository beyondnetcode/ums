# CP-04: Multi-Tenant Repository with Row-Level Security (RLS)

**Runtime:** C# / .NET 8 + SQL Server 2022  
**Backing ADR:** [ADR-0010 — Multi-Tenancy Architecture Strategy](../../adrs/0010-multi-tenancy-architecture-strategy.md) · [ADR-0037 — Tenant-Aware Partitioning](../../adrs/0037-tenant-aware-partitioning-strategy.md) · [ADR-0041 — Authoritative Database Engine](../../adrs/0041-authoritative-database-engine-strategy.md)  
**Related TE:** [TE-03 — Enforce Organization RLS](../../blueprints/technical-enablers/te-03-enforce-organization-rls-sql-server.md)

---

## The Problem This Solves

In a multi-tenant system, a missing `WHERE org_id = @orgId` clause leaks data across tenants. Manual filtering is error-prone and bypassed easily by new developers. SQL Server Row-Level Security enforces the filter **at the database engine**, so no query can bypass it — regardless of how the application is written.

---

## Architecture Overview

```
HTTP Request
    
    
TenantResolver (Middleware)
     extracts org_id from JWT claim
    
EF Core Interceptor
     injects SESSION_CONTEXT(N'org_id', @orgId)
     on every connection open
    
SQL Server RLS Security Policy
     applies predicate: org_id = CAST(SESSION_CONTEXT(N'org_id') AS UNIQUEIDENTIFIER)
    
Query results — only the current tenant's rows are visible
```

---

## 1. SQL Server RLS Setup

```sql
-- Step 1: Security predicate function
-- Returns 1 (visible) only for rows matching the session context org_id
CREATE FUNCTION rls.fn_org_filter (@row_org_id UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1 AS result
    WHERE CAST(SESSION_CONTEXT(N'org_id') AS UNIQUEIDENTIFIER) = @row_org_id;
GO

-- Step 2: Apply policy to all tenant-scoped tables
CREATE SECURITY POLICY rls.org_isolation_policy
    ADD FILTER PREDICATE rls.fn_org_filter(org_id) ON dbo.users,
    ADD FILTER PREDICATE rls.fn_org_filter(org_id) ON dbo.profiles,
    ADD FILTER PREDICATE rls.fn_org_filter(org_id) ON dbo.authorization_templates,
    ADD FILTER PREDICATE rls.fn_org_filter(org_id) ON dbo.access_requests
WITH (STATE = ON);
GO
```

---

## 2. Tenant Resolver (Application Layer Port)

```csharp
// Ums.Application/Tenancy/ITenantContext.cs
namespace Ums.Application.Tenancy;

public interface ITenantContext
{
    OrganizationId? CurrentOrganizationId { get; }
    bool IsSystemContext { get; }  // true for admin/internal operations that bypass RLS
}
```

```csharp
// Ums.Infrastructure/Tenancy/JwtTenantContext.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ums.Application.Tenancy;
using Ums.Domain.Users;

namespace Ums.Infrastructure.Tenancy;

internal sealed class JwtTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public OrganizationId? CurrentOrganizationId
    {
        get
        {
            var claim = accessor.HttpContext?.User
                .FindFirstValue("org_id");

            return claim is not null
                ? OrganizationId.From(Guid.Parse(claim))
                : null;
        }
    }

    public bool IsSystemContext =>
        accessor.HttpContext?.User.IsInRole("system") ?? false;
}
```

---

## 3. EF Core Connection Interceptor — Injects SESSION_CONTEXT

```csharp
// Ums.Infrastructure/Persistence/TenantSessionInterceptor.cs
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ums.Application.Tenancy;

namespace Ums.Infrastructure.Persistence;

internal sealed class TenantSessionInterceptor(ITenantContext tenant) : DbConnectionInterceptor
{
    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken ct = default)
    {
        await base.ConnectionOpeningAsync(connection, eventData, result, ct);
        await SetSessionContextAsync((SqlConnection)connection, ct);
        return result;
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        base.ConnectionOpening(connection, eventData, result);
        SetSessionContextAsync((SqlConnection)connection, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    private async Task SetSessionContextAsync(SqlConnection conn, CancellationToken ct)
    {
        // System context bypasses RLS — used for admin operations only
        if (tenant.IsSystemContext) return;

        if (tenant.CurrentOrganizationId is null) return;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC sp_set_session_context N'org_id', @orgId, @readonly = 1;";
        cmd.Parameters.Add(new SqlParameter("@orgId", tenant.CurrentOrganizationId.Value));
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
```

> `@readonly = 1` prevents the application itself from overwriting the session context once set — an extra guard against privilege escalation attempts.

---

## 4. DbContext Registration

```csharp
// Ums.Infrastructure/DependencyInjection.cs
services.AddScoped<ITenantContext, JwtTenantContext>();
services.AddScoped<TenantSessionInterceptor>();

services.AddDbContext<UmsDbContext>((sp, options) =>
{
    options
        .UseSqlServer(connectionString)
        .AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
});
```

---

## 5. Repository — No Manual Tenant Filtering Needed

Because RLS is enforced at the SQL engine, repositories write straightforward queries:

```csharp
// Ums.Infrastructure/Persistence/SqlUserRepository.cs
internal sealed class SqlUserRepository(UmsDbContext db) : IUserRepository
{
    // No WHERE org_id = ... needed — SQL Server applies it automatically
    public async Task<User?> FindByIdAsync(UserId id, CancellationToken ct) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken ct) =>
        await db.Users
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync(ct);
}
```

---

## 6. System-Level Bypass (Admin Operations)

For operations that need cross-tenant access (e.g., platform admin, migration scripts), inject a system-level `ITenantContext`:

```csharp
// Ums.Application/Admin/SystemTenantContext.cs — bypasses RLS
internal sealed class SystemTenantContext : ITenantContext
{
    public OrganizationId? CurrentOrganizationId => null;
    public bool IsSystemContext => true;
}
```

Register this only in system-scoped command handlers, not in the global DI container. Scope it explicitly:

```csharp
// Inside a system admin command handler
using var scope = serviceScopeFactory.CreateScope();
scope.ServiceProvider.GetRequiredService<ITenantContext>(); // override registration
```

---

## 7. Verification Testá

```csharp
// Ums.Infrastructure.Tests/RlsEnforcementTests.cs
[Fact]
public async Task User_from_org_A_cannot_see_data_from_org_B()
{
    // Arrange
    var orgA = OrganizationId.New();
    var orgB = OrganizationId.New();

    await SeedUser(orgId: orgA, email: "alice@a.com");
    await SeedUser(orgId: orgB, email: "bob@b.com");

    // Act — query as org A
    using var scope = BuildScopeForOrg(orgA);
    var repo = scope.GetRequiredService<IUserRepository>();
    var users = await repo.ListActiveAsync(CancellationToken.None);

    // Assert — only org A users are visible
    Assert.All(users, u => Assert.Equal(orgA, u.OrganizationId));
    Assert.DoesNotContain(users, u => u.Email.Value == "bob@b.com");
}
```

This test runs against a real SQL Server 2022 instance (Testácontainers) — never a mock — to validate the actual RLS policy behavior.

---

## Anti-Patterns to Reject in Code Review

| Anti-Pattern | Risk | Fix |
| :--- | :--- | :--- |
| `WHERE org_id = tenantId` in repository | Easy to forget; not enforced by engine | Use RLS session context instead |
| `SESSION_CONTEXT` set in application code (not interceptor) | Can be overwritten before query | Use `@readonly = 1` + interceptor |
| `SystemTenantContext` as default registration | All tenants' data exposed | Register only in explicit admin scope |
| Skipping RLS in integration tests | Tests pass, prod leaks data | Always use real SQL Server + Testácontainers
## Related Patterns

- [CP-01 — Hexagonal Port/Adapter](./cp-01-hexagonal-port-adapter.md)
- [CP-02 — Aggregate Root + Domain Event](./cp-02-aggregate-root-domain-event.md)
- [TE-03 — Enforce Organization RLS](../../blueprints/technical-enablers/te-03-enforce-organization-rls-sql-server.md)
