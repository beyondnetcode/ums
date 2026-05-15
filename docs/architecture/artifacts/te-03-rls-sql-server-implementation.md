# TE-03: Enforce Row-Level Security by Organization (SQL Server 2022 Implementation)

**Versión:** 2.0  
**Fecha:** 2026-05-14  
**Estado:** Accepted (replaces preliminary TE-03 with complete error handling, SESSION_CONTEXT cleanup, and failover scenarios)  
**Autores:** Arquitecto Principal, DBA Senior  
**Referencias:** ADR-0041 (SQL Server), ADR-0048 (Hierarchical Multi-Tenancy), ADR-0010 (Multi-Tenancy Strategy)

---

## 1. Objetivo

Definir la implementación completa de Row-Level Security (RLS) en SQL Server 2022 para asegurar aislamiento de datos por inquilino/organización. Este documento cubre:

1. **Two-Layer Isolation Model**: Layer 1 (Aplicación) vs. Layer 2 (Base de Datos)
2. **SQL Server Security Policies** con `SESSION_CONTEXT`
3. **Error Handling & Retry Logic**
4. **SESSION_CONTEXT Cleanup** (connection pooling, failover, timeouts)
5. **Failover Scenarios** (replica failures, network partitions)
6. **Validation Queries** para testing y troubleshooting

---

## 2. Dos Capas de Aislamiento (Two-Layer Model)

### 2.1. Layer 1: Application-Level Filtering (PRIMARY)

**Responsable:** Aplicación .NET 8 (EF Core Global Query Filter)

**Mecanismo:** Todas las queries de lectura/escritura incluyen un filtro `WHERE root_tenant_id = @currentTenantId` mediante `HasQueryFilter`.

**Ventajas:**
- ✅ Independiente de la base de datos (motor agnóstico)
- ✅ Rápido de depurar (código de aplicación, no triggers)
- ✅ Sin sobrecarga de sesión SQL Server

**Obligaciones del Desarrollador:**
- NUNCA hacer query sin el filtro RLS (debe estar en el DbContext global filter)
- Validar en code review que TODAS las IRepository implementaciones usan el global filter
- Unit tests deben verificar que el filtro está presente

```csharp
// Layer 1: Application Filter (PRIMARIO)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Aplica a TODAS las entidades con root_tenant_id
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (entityType.FindProperty("root_tenant_id") != null)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, "root_tenant_id");
            var currentTenant = Expression.Call(
                typeof(CurrentTenantContext).GetMethod(nameof(CurrentTenantContext.GetTenantId)));
            var equal = Expression.Equal(property, currentTenant);
            var lambda = Expression.Lambda(equal, parameter);
            
            entityType.SetQueryFilter(lambda);
        }
    }
}

// Test: Verificar que el filtro está presente
[Fact]
public async Task GetTenants_ShouldOnlyReturnCurrentTenantData()
{
    // Arrange: Create 2 tenants with different root_tenant_id
    var tenant1 = new Tenant(Guid.NewGuid(), "Corp A", ...);
    var tenant2 = new Tenant(Guid.NewGuid(), "Corp B", ...);
    
    _context.Tenants.AddRange(tenant1, tenant2);
    await _context.SaveChangesAsync();
    
    // Act: Query as tenant1
    CurrentTenantContext.Set(tenant1.RootTenantId);
    var result = await _context.Tenants.ToListAsync();
    
    // Assert: Only tenant1 is returned (Layer 1 filter applied)
    result.Should().HaveCount(1);
    result.First().RootTenantId.Should().Be(tenant1.RootTenantId);
}
```

### 2.2. Layer 2: SQL Server RLS Policy (FAILSAFE)

**Responsable:** SQL Server 2022 Security Policy con `SESSION_CONTEXT`

**Mecanismo:** Si el desarrollador olvida el filtro Layer 1, la política RLS a nivel de BD bloquea la fila.

**Propósito:** Defensa en profundidad contra errores de desarrollador.

**Limitaciones:**
- ⚠️ Solo activa si se implementa correctamente (ver sección 3)
- ⚠️ Sobrecarga mínima pero no cero
- ⚠️ NO reemplaza Layer 1 (Layer 1 es el mecanismo primario)

---

## 3. SQL Server RLS Implementation (Layer 2)

### 3.1. Crear Security Policy Predicate

```sql
-- Step 1: Create inline table-valued function (predicate)
CREATE OR ALTER FUNCTION [security].[fn_tenants_rls_predicate] (
    @root_tenant_id UNIQUEIDENTIFIER
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1
    WHERE @root_tenant_id = CAST(SESSION_CONTEXT(N'root_tenant_id') AS UNIQUEIDENTIFIER);

-- Step 2: Create SECURITY POLICY
CREATE SECURITY POLICY [security].[sp_tenants_rls]
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [identity].[tenants],
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [identity].[users],
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [authorization].[policies],
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [authorization].[policy_bindings],
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [audit].[audit_log],
    ADD FILTER PREDICATE [security].[fn_tenants_rls_predicate]([root_tenant_id]) 
        ON [approval].[approval_workflows];

-- Step 3: Enable the policy
ALTER SECURITY POLICY [security].[sp_tenants_rls] WITH (STATE = ON);
```

### 3.2. SESSION_CONTEXT Setup (Obligatorio en Conexión)

```sql
-- Executed IMMEDIATELY after connection opened (before ANY query)
EXEC sp_set_session_context @key = N'root_tenant_id', 
                             @value = @tenantId,  -- UNIQUEIDENTIFIER
                             @read_only = 0;      -- Allow cleanup logic

-- Validate it was set
SELECT SESSION_CONTEXT(N'root_tenant_id') AS current_tenant_id;
-- Returns: @tenantId (or NULL if not set)
```

### 3.3. EF Core Integration (.NET 8)

```csharp
// UMS.Infrastructure.Security/RLSSessionContextInterceptor.cs
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class RLSSessionContextInterceptor : DbConnectionInterceptor
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<RLSSessionContextInterceptor> _logger;
    
    public RLSSessionContextInterceptor(
        ICurrentUserContext currentUserContext,
        ILogger<RLSSessionContextInterceptor> logger)
    {
        _currentUserContext = currentUserContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Ejecutado cuando se abre una conexión de BD.
    /// CRÍTICO: Debe ejecutar ANTES de cualquier query.
    /// </summary>
    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (connection is not SqlConnection sqlConn)
        {
            _logger.LogWarning("Non-SQL Server connection detected; RLS Layer 2 not available");
            return result;
        }
        
        try
        {
            // 1. Abrir la conexión primero
            await sqlConn.OpenAsync(cancellationToken);
            
            // 2. Obtener tenant ID del contexto actual
            var rootTenantId = _currentUserContext.RootTenantId;
            if (rootTenantId == Guid.Empty)
            {
                _logger.LogWarning("RootTenantId is empty; rejecting connection");
                throw new InvalidOperationException(
                    "RootTenantId must be set before database access. " +
                    "Ensure TenantResolutionMiddleware is configured.");
            }
            
            // 3. Ejecutar sp_set_session_context
            using (var cmd = sqlConn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_set_session_context";
                cmd.CommandTimeout = 10;  // Timeout para prevenir deadlocks
                
                cmd.Parameters.Add(new SqlParameter("@key", SqlDbType.NVarChar, 128)
                {
                    Value = "root_tenant_id"
                });
                cmd.Parameters.Add(new SqlParameter("@value", SqlDbType.UniqueIdentifier)
                {
                    Value = rootTenantId
                });
                cmd.Parameters.Add(new SqlParameter("@read_only", SqlDbType.Bit)
                {
                    Value = 0  // Permitir que cleanup después limpia el contexto
                });
                
                try
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    _logger.LogDebug("SESSION_CONTEXT set for tenant {TenantId}", rootTenantId);
                }
                catch (SqlException ex) when (ex.Number == 50000)  // User-defined error
                {
                    _logger.LogError(ex, 
                        "Failed to set SESSION_CONTEXT for tenant {TenantId}. " +
                        "Check sp_set_session_context exists.", rootTenantId);
                    throw new DataAccessException(
                        "Failed to initialize tenant isolation. Database misconfigured.", ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CONNECTION_OPENING_ASYNC timeout; cancellation token triggered");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting RLS SESSION_CONTEXT");
            throw new DataAccessException("RLS initialization failed", ex);
        }
        
        return result;
    }
    
    /// <summary>
    /// Ejecutado cuando se cierra una conexión.
    /// CRÍTICO: Limpiar SESSION_CONTEXT para evitar que se reutilice en otro tenant.
    /// </summary>
    public override async ValueTask<InterceptionResult> ConnectionClosingAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (connection is not SqlConnection sqlConn || sqlConn.State != ConnectionState.Open)
        {
            return result;
        }
        
        try
        {
            // Limpiar SESSION_CONTEXT ANTES de devolver la conexión al pool
            using (var cmd = sqlConn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_set_session_context";
                cmd.CommandTimeout = 5;
                
                cmd.Parameters.Add(new SqlParameter("@key", SqlDbType.NVarChar, 128)
                {
                    Value = "root_tenant_id"
                });
                cmd.Parameters.Add(new SqlParameter("@value", SqlDbType.UniqueIdentifier)
                {
                    Value = DBNull.Value  // Limpiar contexto
                });
                
                try
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    _logger.LogDebug("SESSION_CONTEXT cleared on connection close");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, 
                        "Failed to clear SESSION_CONTEXT on close. " +
                        "Consider restarting connection pool.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CONNECTION_CLOSING; connection may be corrupted");
        }
        
        return result;
    }
}

// Registrar el interceptor en DI
public static IServiceCollection AddRLSSecurity(this IServiceCollection services)
{
    services.AddScoped<RLSSessionContextInterceptor>();
    services.AddDbContext<IdentityDbContext>((provider, options) =>
    {
        var rls = provider.GetRequiredService<RLSSessionContextInterceptor>();
        options
            .UseSqlServer("Server=...;Database=ums_identity;...")
            .AddInterceptors(rls);
    });
    
    return services;
}
```

---

## 4. Error Handling & Retry Logic

### 4.1. Categorizar Excepciones de RLS

```csharp
// UMS.Shared.Infrastructure/Exceptions/DataAccessException.cs
public class DataAccessException : Exception
{
    public DataAccessException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

public class RLSException : DataAccessException
{
    public RLSException(string message, Exception? inner = null)
        : base(message, inner) { }
}

public class TenantIsolationException : DataAccessException
{
    public string? TenantId { get; }
    public TenantIsolationException(string message, string? tenantId = null)
        : base(message)
    {
        TenantId = tenantId;
    }
}

// UMS.Shared.Infrastructure/Exceptions/SqlExceptionExtensions.cs
public static class SqlExceptionExtensions
{
    private const int SESSION_CONTEXT_TIMEOUT = -2;      // Timeout
    private const int SESSION_CONTEXT_NOT_FOUND = 16389; // Session variable not found
    private const int PERMISSION_DENIED = 262;           // Login failed (RLS blocking)
    private const int DEADLOCK = 1205;                   // Deadlock
    private const int CONNECTION_INTERRUPTED = 64;       // Communication link broken
    
    public static bool IsRLSRelated(this SqlException ex)
    {
        return ex.Errors.Cast<SqlError>().Any(e =>
            e.Number == PERMISSION_DENIED ||
            e.Number == SESSION_CONTEXT_NOT_FOUND ||
            e.Message.Contains("SESSION_CONTEXT"));
    }
    
    public static bool IsRetryable(this SqlException ex)
    {
        return ex.Errors.Cast<SqlError>().Any(e =>
            e.Number == DEADLOCK ||
            e.Number == CONNECTION_INTERRUPTED ||
            e.Number == -1);  // Network error
    }
}
```

### 4.2. Retry Policy con Polly

```csharp
// UMS.Shared.Infrastructure/Resilience/RLSRetryPolicy.cs
using Polly;
using Polly.CircuitBreaker;

public class RLSRetryPolicy
{
    private readonly IAsyncPolicy<T> _policy;
    private readonly ILogger<RLSRetryPolicy> _logger;
    
    public RLSRetryPolicy(ILogger<RLSRetryPolicy> logger)
    {
        _logger = logger;
        
        _policy = Policy<T>
            .Handle<SqlException>(ex => ex.IsRetryable())
            .Or<InvalidOperationException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        "RLS retry {Attempt}/3 after {Delay}ms due to {Exception}",
                        attempt, delay.TotalMilliseconds, outcome.Exception?.Message);
                })
            .WrapAsync(Policy<T>
                .Handle<RLSException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        _logger.LogError(
                            "RLS circuit breaker opened for {Duration}s due to repeated failures",
                            duration.TotalSeconds);
                    }));
    }
    
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        return await _policy.ExecuteAsync(action, cancellationToken);
    }
}

// Uso en repositorio
public class TenantEfCoreRepository : ITenantRepository
{
    private readonly IdentityDbContext _context;
    private readonly RLSRetryPolicy _retryPolicy;
    
    public async Task<Tenant?> GetByIdAsync(TenantId id)
    {
        return await _retryPolicy.ExecuteAsync(async ct =>
        {
            try
            {
                return await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == id, ct);
            }
            catch (SqlException ex) when (ex.IsRLSRelated())
            {
                throw new RLSException(
                    $"RLS violation detected querying tenant {id}. " +
                    "Check SESSION_CONTEXT configuration.", ex);
            }
        });
    }
}
```

---

## 5. SESSION_CONTEXT Cleanup (Critical for Connection Pooling)

### 5.1. El Problema: Connection Pool Reuse

```
Escenario problemático:
1. Conexión #1: Cliente A abre conexión, set SESSION_CONTEXT root_tenant_id = Corp-A
2. Cliente A cierra la conexión → devuelve a pool
3. Conexión #1: Cliente B obtiene la MISMA conexión del pool
4. BUG: CLIENT B puede ver datos de CORP-A si SESSION_CONTEXT no se limpió
```

### 5.2. Solución: Cleanup on Connection Return

```csharp
// UMS.Shared.Infrastructure/Resilience/RLSConnectionPoolManager.cs
public class RLSConnectionPoolManager
{
    private readonly SqlConnectionStringBuilder _connString;
    private readonly ILogger<RLSConnectionPoolManager> _logger;
    
    /// <summary>
    /// Ejecutar después de cambios en arquitectura de pooling.
    /// Limpia todas las conexiones activas.
    /// </summary>
    public async Task ClearAllSessionContexts(CancellationToken cancellationToken = default)
    {
        try
        {
            // Opción 1: Cerrar todo el pool (nuclear, para emergencias)
            SqlConnection.ClearAllPools();
            _logger.LogWarning("Cleared all SQL connection pools");
            
            // Opción 2: Reconectar y limpiar individual
            using (var conn = new SqlConnection(_connString.ConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "EXEC sp_set_session_context @key=N'root_tenant_id', @value=NULL";
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear SESSION_CONTEXT across pools");
        }
    }
    
    /// <summary>
    /// Validar que pool no contiene "stale" SESSION_CONTEXT.
    /// Ejecutar en health check periódico.
    /// </summary>
    public async Task<bool> ValidatePoolHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using (var conn = new SqlConnection(_connString.ConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT SESSION_CONTEXT(N'root_tenant_id')";
                    var result = await cmd.ExecuteScalarAsync(cancellationToken);
                    
                    // Debería ser NULL si conexión nueva
                    if (result != null && result != DBNull.Value)
                    {
                        _logger.LogWarning(
                            "Pool health check FAILED: SESSION_CONTEXT not cleared. " +
                            "Found value: {Value}", result);
                        return false;
                    }
                    
                    _logger.LogInformation("Pool health check PASSED: SESSION_CONTEXT is clean");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pool health check error");
            return false;
        }
    }
}

// Registrar en DI
services.AddHostedService<RLSPoolHealthCheckService>();

public class RLSPoolHealthCheckService : BackgroundService
{
    private readonly RLSConnectionPoolManager _poolManager;
    private readonly ILogger<RLSPoolHealthCheckService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Verificar pool cada 5 minutos
                await _poolManager.ValidatePoolHealthAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RLS pool health check failed");
            }
        }
    }
}
```

---

## 6. Failover Scenarios

### 6.1. Replica Failover (Always-On Availability Groups)

```sql
-- SQL Server AG listener: ums-listener.database.windows.net
-- Automatic failover con SESSION_CONTEXT preservation

-- Scenario: Primary node fails → Secondary takes over
-- Expected behavior: SESSION_CONTEXT LOST (connection dropped)
-- Action: EF Core reconnects → RLSSessionContextInterceptor re-sets context

-- Verificar que réplica tiene la política de RLS
SELECT * FROM sys.security_policies 
WHERE name = 'sp_tenants_rls';
-- Debería retornar 1 fila en CADA réplica
```

### 6.2. Implementar Reconnection Logic

```csharp
// UMS.Shared.Infrastructure/Resilience/FailoverAwareRLSInterceptor.cs
public class FailoverAwareRLSInterceptor : RLSSessionContextInterceptor
{
    private readonly ICircuitBreakerPolicy _failoverPolicy;
    
    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Intenta set SESSION_CONTEXT con retry en failover
            return await _failoverPolicy.ExecuteAsync(async () =>
            {
                return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
            });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("failover"))
        {
            _logger.LogWarning(
                ex,
                "Database failover detected. SESSION_CONTEXT reset on new connection.");
            throw;
        }
    }
}
```

### 6.3. Network Partition Handling

```csharp
// UMS.Shared.Infrastructure/Health/RLSHealthCheck.cs
public class RLSHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var conn = new SqlConnection(_connString.ConnectionString))
            {
                // Timeout corto para detectar particiones rápido
                conn.ConnectionTimeout = 3;
                await conn.OpenAsync(cancellationToken);
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";  // Ping simple
                    cmd.CommandTimeout = 2;
                    await cmd.ExecuteScalarAsync(cancellationToken);
                }
            }
            
            return HealthCheckResult.Healthy("RLS layer operational");
        }
        catch (SqlException ex) when (ex.Number == -1)  // Network error
        {
            return HealthCheckResult.Unhealthy(
                "Network partition detected; RLS layer unavailable", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("RLS health check error", ex);
        }
    }
}

// Registrar en Program.cs
builder.Services
    .AddHealthChecks()
    .AddCheck<RLSHealthCheck>("rls-health");
```

---

## 7. Validation Queries (Testing & Troubleshooting)

### 7.1. Verificar RLS Policy Está Activa

```sql
-- Step 1: Confirmar que la policy existe y está enabled
SELECT 
    sp.name AS policy_name,
    sp.state,
    COUNT(*) AS predicate_count
FROM sys.security_policies sp
LEFT JOIN sys.security_predicates sp2 ON sp.security_policy_id = sp2.security_policy_id
WHERE sp.name = 'sp_tenants_rls'
GROUP BY sp.name, sp.state;

-- Expected: policy_name='sp_tenants_rls', state=1 (enabled), predicate_count >= 6

-- Step 2: Listar todas las tablas protegidas
SELECT 
    sp.name AS policy_name,
    OBJECT_NAME(sp2.target_object_id) AS table_name,
    sp2.predicate_name,
    sp2.predicate_type_desc
FROM sys.security_policies sp
JOIN sys.security_predicates sp2 ON sp.security_policy_id = sp2.security_policy_id
WHERE sp.name = 'sp_tenants_rls'
ORDER BY table_name;
```

### 7.2. Validar SESSION_CONTEXT Setup

```sql
-- Test 1: Verificar que sp_set_session_context existe y es callable
EXEC sp_set_session_context 
    @key = N'root_tenant_id',
    @value = '00000000-0000-0000-0000-000000000001';
    
SELECT SESSION_CONTEXT(N'root_tenant_id') AS current_tenant;
-- Expected: 00000000-0000-0000-0000-000000000001

-- Test 2: Limpiar contexto
EXEC sp_set_session_context 
    @key = N'root_tenant_id',
    @value = NULL;
    
SELECT SESSION_CONTEXT(N'root_tenant_id') AS current_tenant;
-- Expected: NULL
```

### 7.3. Test RLS Isolation (Integration Test Script)

```csharp
// tests/Integration/UMS.Contexts.Identity.IntegrationTests/RLSIsolationTests.cs
public class RLSIsolationTests : IAsyncLifetime
{
    private readonly IdentityDbContext _context1;  // Tenant A
    private readonly IdentityDbContext _context2;  // Tenant B
    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();
    
    public async Task InitializeAsync()
    {
        // Crear contextos con diferentes tenants
        _context1 = CreateContextForTenant(_tenantA);
        _context2 = CreateContextForTenant(_tenantB);
        
        // Setup datos
        await SetupTestData();
    }
    
    [Fact]
    public async Task RLS_Layer1_ShouldIsolateTenants()
    {
        // Act: Query como Tenant A
        CurrentTenantContext.Set(_tenantA);
        var resultA = await _context1.Tenants.ToListAsync();
        
        // Act: Query como Tenant B
        CurrentTenantContext.Set(_tenantB);
        var resultB = await _context2.Tenants.ToListAsync();
        
        // Assert: Cada tenant ve solo sus datos
        resultA.Should().AllSatisfy(t => t.RootTenantId.Should().Be(_tenantA));
        resultB.Should().AllSatisfy(t => t.RootTenantId.Should().Be(_tenantB));
        resultA.Should().NotEqual(resultB);
    }
    
    [Fact]
    public async Task RLS_Layer2_Should_Block_Unfiltered_Query()
    {
        // Act: Intentar ejecutar raw SQL sin filtro RLS
        // (Simular que desarrollador olvidó Layer 1)
        var sql = "SELECT * FROM [identity].[tenants]";
        
        // Assert: SQL Server RLS debería bloquear
        var ex = await Assert.ThrowsAsync<SqlException>(async () =>
        {
            using (var cmd = _context1.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sql;
                await cmd.ExecuteReaderAsync();
            }
        });
        
        ex.Message.Should().Contain("RLS");
    }
    
    [Fact]
    public async Task SESSION_CONTEXT_Should_Persist_Across_Operations()
    {
        // Act: Set context una vez
        CurrentTenantContext.Set(_tenantA);
        
        // Act: Múltiples operaciones
        var tenant1 = await _context1.Tenants.FirstOrDefaultAsync();
        var users1 = await _context1.Users.ToListAsync();
        var policies1 = await _context1.Policies.ToListAsync();
        
        // Assert: Todas las queries respetan el mismo tenant
        tenant1?.RootTenantId.Should().Be(_tenantA);
        users1.Should().AllSatisfy(u => u.RootTenantId.Should().Be(_tenantA));
        policies1.Should().AllSatisfy(p => p.RootTenantId.Should().Be(_tenantA));
    }
    
    private IdentityDbContext CreateContextForTenant(Guid tenantId)
    {
        CurrentTenantContext.Set(tenantId);
        return new IdentityDbContext(_options, _services, _currentUserContext);
    }
    
    private async Task SetupTestData()
    {
        // Insertar datos para ambos tenants
        _context1.Tenants.Add(new Tenant(_tenantA, "Tenant A", ...));
        _context2.Tenants.Add(new Tenant(_tenantB, "Tenant B", ...));
        
        await _context1.SaveChangesAsync();
        await _context2.SaveChangesAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _context1.DisposeAsync();
        await _context2.DisposeAsync();
    }
}
```

### 7.4. Troubleshooting Script (DBA Diagnostics)

```sql
-- Ejecutar si sospechas RLS issues

-- 1. Verificar que las políticas están habilitadas
SELECT 
    name,
    type_desc,
    is_ms_shipped,
    state_desc
FROM sys.security_policies
WHERE name LIKE 'sp_%';

-- 2. Verificar que las funciones predicate existen
SELECT 
    SCHEMA_NAME(schema_id) AS schema_name,
    name,
    type_desc
FROM sys.objects
WHERE name LIKE 'fn_%_rls_%'
ORDER BY name;

-- 3. Listar auditoría de accesos bloqueados por RLS (si habilitado)
SELECT TOP 100
    event_time,
    session_id,
    server_principal_name,
    database_name,
    object_name,
    statement
FROM fn_get_audit_file('\\path\\to\\audit\*.sqlaudit', DEFAULT, DEFAULT)
WHERE action_name LIKE 'RLS%'
ORDER BY event_time DESC;

-- 4. Verificar que la columna root_tenant_id existe en todas las tablas core
SELECT 
    OBJECT_NAME(column_id) AS table_name,
    name AS column_name,
    user_type_name(user_type_id) AS data_type
FROM sys.all_columns
WHERE name = 'root_tenant_id'
  AND OBJECT_NAME(object_id) IN (
    'tenants', 'users', 'policies', 'policy_bindings', 
    'audit_log', 'approval_workflows')
ORDER BY table_name;

-- 5. Performance: Índices en root_tenant_id (para partition pruning)
SELECT 
    OBJECT_NAME(i.object_id) AS table_name,
    i.name AS index_name,
    STRING_AGG(c.name, ',') AS columns
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id 
  AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id 
  AND ic.column_id = c.column_id
WHERE c.name = 'root_tenant_id'
GROUP BY i.object_id, i.name
ORDER BY table_name;
```

---

## 8. Performance Considerations

### 8.1. Predicate Execution Cost

| Scenario | Impact | Mitigation |
|----------|--------|-----------|
| Predicate on non-partitioned table | ~1-2% overhead | Use composite index (root_tenant_id, query_column) |
| Predicate with NULL SESSION_CONTEXT | Query blocked (good!) | Ensure RLSSessionContextInterceptor always executes |
| Multiple joins across RLS tables | Predicates applied per table | Use filtered indexes per tenant |

### 8.2. Query Plan Optimization

```sql
-- Enable RLS query statistics (SQL Server 2019+)
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- Test query with RLS
EXEC sp_set_session_context @key = N'root_tenant_id', 
                             @value = '00000000-0000-0000-0000-000000000001';

SELECT * FROM [identity].[tenants]
WHERE [type_code] = 'ENTERPRISE';

-- Expected: Predicate applied; partition pruning if partitioned table
```

---

## 9. Deployment & Monitoring

### 9.1. TE-03 Deployment Checklist

- [ ] `sp_set_session_context` stored procedure created
- [ ] RLS predicates (fn_*_rls_predicate) created
- [ ] Security policy created and ENABLED
- [ ] RLSSessionContextInterceptor registered in DI
- [ ] RLSConnectionPoolManager health check configured
- [ ] RLS validation queries tested manually
- [ ] Integration tests passing (RLSIsolationTests)
- [ ] Failover scenarios tested (replica failover simulation)
- [ ] Monitoring alerts set for RLS errors

### 9.2. Alerts & Logging

```csharp
// Log levels for RLS events
logger.LogInformation("RLS: SESSION_CONTEXT set for tenant {TenantId}", tenantId);
logger.LogWarning("RLS: SESSION_CONTEXT cleanup failed on connection close");
logger.LogError("RLS: Predicate evaluation failed; tenant data may be exposed!");

// Alerting rules
- ERROR level → Immediate PagerDuty
- WARNING level → Slack notification
- Metrics: RLS predicate evaluation time, failed context setups
```

---

## 10. Rollback & Disable Plan

Si Layer 2 RLS causa problemas inesperados:

```sql
-- Disable security policy (mantiene las definiciones)
ALTER SECURITY POLICY [security].[sp_tenants_rls] WITH (STATE = OFF);

-- Drop if needed (después de validar que Layer 1 solo es suficiente)
DROP SECURITY POLICY [security].[sp_tenants_rls];
DROP FUNCTION [security].[fn_tenants_rls_predicate];

-- Layer 1 aplicación filters permanecen activos
```

---

## 11. Success Criteria

✅ **TE-03 Complete cuando:**

1. SQL Server RLS policies ejecutadas sin errores
2. SESSION_CONTEXT se configura en CADA apertura de conexión
3. SESSION_CONTEXT se limpia en CADA cierre de conexión
4. RLSIsolationTests pasen (Layer 1 + Layer 2 ambas activas)
5. Failover scenario validado (replica failure recovery)
6. Health check monitoreando pool continuamente
7. Documentado para DBA (troubleshooting guide incluido)

---

**Aprobado por:** Arquitecto Principal, DBA  
**Fecha:** 2026-05-14  
**Próxima Revisión:** 2026-06-11 (Post-Sprint 1)
