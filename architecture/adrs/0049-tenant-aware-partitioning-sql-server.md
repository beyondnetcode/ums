# ADR-0049: Tenant-Aware Partitioning Strategy for SQL Server 2022

*   **Status:** Accepted
*   **Date:** 2026-05-14
*   **Authors:** Architecture Team
*   **Supersedes:** ADR-0037 (PostgreSQL version)
*   **Related:** ADR-0010 (Core Multi-Tenancy), ADR-0041 (Database Engine), ADR-0048 (Hierarchical Model)

---

## 1. Context and Problem

As UMS adopts hierarchical tenants (ADR-0048), scalability challenges emerge:

1. **Single-tenant recovery**: Restoring a root tenant requires scanning shared tables
2. **Noisy neighbor**: High-volume tenants degrade performance for smaller ones
3. **Index maintenance**: Statistics and index fragmentation must scan entire table
4. **Future sharding**: Need to migrate root tenant partitions to separate instances

SQL Server 2022 supports native **RANGE and LIST partitioning** by `root_tenant_id`, enabling per-tenant physical isolation without schema separation.

---

## 2. Architectural Decision

Adopt **LIST partitioning by `root_tenant_id`** for core tables. Each root tenant and its entire sub-tree resides in a single partition.

### 2.1 Partitioning Scheme (SQL Server)

```sql
-- Master table template (RANGE by LIST not needed in SQL Server)
CREATE TABLE [ums_identity].[TENANT] (
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    root_tenant_id UNIQUEIDENTIFIER NOT NULL,
    name NVARCHAR(255) NOT NULL,
    type_code VARCHAR(32) NOT NULL,
    status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE',
    parent_tenant_id UNIQUEIDENTIFIER NULL,
    metadata NVARCHAR(MAX),
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    created_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
    modified_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    modified_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
    
    PRIMARY KEY CLUSTERED (root_tenant_id, tenant_id)  -- Partition key first
) ON ps_tenant_by_root(root_tenant_id);

-- Partition function: List of root tenant IDs
CREATE PARTITION FUNCTION pf_tenant_by_root (UNIQUEIDENTIFIER)
    AS PARTITION RANGE LEFT 
    FOR VALUES (
        '11111111-1111-1111-1111-111111111111',
        '22222222-2222-2222-2222-222222222222',
        '33333333-3333-3333-3333-333333333333'
        -- Add more as needed
    );

-- Partition scheme: Map partitions to filegroups
CREATE PARTITION SCHEME ps_tenant_by_root
    AS PARTITION pf_tenant_by_root
    TO (fg_tenant_001, fg_tenant_002, fg_tenant_003, [PRIMARY]);

-- Filegroups (can be on different disks for isolation)
ALTER DATABASE [ums] ADD FILEGROUP fg_tenant_001;
ALTER DATABASE [ums] ADD FILE 
    (NAME = N'ums_tenant_001', FILENAME = N'D:\data\ums_tenant_001.ndf')
    TO FILEGROUP fg_tenant_001;

ALTER DATABASE [ums] ADD FILEGROUP fg_tenant_002;
ALTER DATABASE [ums] ADD FILE 
    (NAME = N'ums_tenant_002', FILENAME = N'E:\data\ums_tenant_002.ndf')
    TO FILEGROUP fg_tenant_002;
```

### 2.2 Core Tables to Partition

| Table | Partition Key | Sub-Partition | TTL |
|-------|---------------|----------------|-----|
| `TENANT` | `root_tenant_id` | None | Permanent |
| `USER_ACCOUNT` | `root_tenant_id` | Optional: by `user_category` (INTERNAL, EXTERNAL, B2B) | Permanent |
| `AUTHORIZATION_TEMPLATE` | `root_tenant_id` | None | Permanent |
| `PROFILE_PERMISSION` | `root_tenant_id` | None | Permanent |
| `AUDIT_RECORD` | `root_tenant_id` | Optional: RANGE by `created_at` (monthly) | Permanent |
| `APPROVAL_REQUEST` | `root_tenant_id` | None | Permanent |
| `USER_DOCUMENT` | `root_tenant_id` | Optional: by `document_status` | Permanent |

### 2.3 Closure Table Partitioning

Must include `root_tenant_id` for partition pruning:

```sql
CREATE TABLE [ums_identity].[tenant_closure] (
    ancestor_id UNIQUEIDENTIFIER NOT NULL,
    descendant_id UNIQUEIDENTIFIER NOT NULL,
    depth INT NOT NULL CHECK (depth >= 0),
    root_tenant_id UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY CLUSTERED (root_tenant_id, ancestor_id, descendant_id)
) ON ps_tenant_by_root(root_tenant_id);

-- Indexes optimized for closure table queries
CREATE NONCLUSTERED INDEX idx_closure_descendant 
    ON [ums_identity].[tenant_closure] (root_tenant_id, descendant_id, ancestor_id);

CREATE NONCLUSTERED INDEX idx_closure_ancestor_depth 
    ON [ums_identity].[tenant_closure] (root_tenant_id, ancestor_id, depth);
```

### 2.4 Dynamic Partition Management (C#)

Automatically create partitions for new root tenants:

```csharp
public class PartitionManager
{
    private readonly DbContext _dbContext;
    private readonly ILogger<PartitionManager> _logger;
    
    public async Task EnsurePartitionAsync(Guid rootTenantId)
    {
        // Check if partition exists (via sys.partitions)
        var exists = await _dbContext.Database
            .SqlQueryRaw<bool>(
                @"SELECT CASE 
                    WHEN EXISTS (
                        SELECT 1 FROM sys.partitions 
                        WHERE partition_number > 1 
                          AND object_id = OBJECT_ID('[ums_identity].[TENANT]')
                          AND CONVERT(NVARCHAR(MAX), 
                            (SELECT value FROM sys.partition_parameters WHERE partition_id = 1))
                            = @RootTenantId
                    ) THEN 1 ELSE 0 END",
                new SqlParameter("@RootTenantId", rootTenantId)
            )
            .FirstAsync();
        
        if (!exists)
        {
            _logger.LogInformation("Creating partition for root tenant {TenantId}", rootTenantId);
            
            // Create filegroup if needed
            await _dbContext.Database.ExecuteSqlRawAsync(
                "ALTER DATABASE [ums] ADD FILEGROUP fg_tenant_" + rootTenantId.ToString("N").Substring(0, 8));
            
            // Add file to filegroup
            await _dbContext.Database.ExecuteSqlRawAsync(
                $"ALTER DATABASE [ums] ADD FILE " +
                $"(NAME = N'ums_tenant_{rootTenantId:N}'.Substring(0, 8), " +
                $"FILENAME = N'D:\\data\\ums_tenant_{rootTenantId:N}.Substring(0, 8).ndf') " +
                $"TO FILEGROUP fg_tenant_{rootTenantId:N}.Substring(0, 8)");
            
            // Alter partition function and scheme
            await _dbContext.Database.ExecuteSqlRawAsync(
                $"ALTER PARTITION FUNCTION pf_tenant_by_root() SPLIT RANGE ('{rootTenantId}')");
            
            await _dbContext.Database.ExecuteSqlRawAsync(
                $"ALTER PARTITION SCHEME ps_tenant_by_root NEXT USED fg_tenant_{rootTenantId:N}.Substring(0, 8)");
            
            _logger.LogInformation("Partition created successfully for {TenantId}", rootTenantId);
        }
    }
}
```

### 2.5 Query Optimization with Partition Pruning

Always include `root_tenant_id` in WHERE clause for partition pruning:

```csharp
// ✅ GOOD: Partition pruning enabled
var tenants = await _dbContext.Tenants
    .Where(t => t.RootTenantId == rootTenantId && t.Status == "ACTIVE")
    .ToListAsync();

// ❌ BAD: No partition pruning (scans all partitions)
var allActive = await _dbContext.Tenants
    .Where(t => t.Status == "ACTIVE")
    .ToListAsync();
```

### 2.6 Monitoring & Maintenance

```sql
-- View partition usage
SELECT 
    OBJECT_NAME(ps.object_id) AS TableName,
    ps.partition_number,
    fg.name AS FileGroupName,
    ps.row_count,
    (ps.reserved_page_count * 8) / 1024 AS SizeMB
FROM sys.dm_db_partition_stats ps
INNER JOIN sys.filegroups fg 
    ON ps.hobt_id IN (
        SELECT hobt_id FROM sys.partitions 
        WHERE partition_number = ps.partition_number
    )
WHERE OBJECT_NAME(ps.object_id) LIKE 'TENANT%'
ORDER BY ps.partition_number;

-- Rebuild fragmented partitions
ALTER INDEX ALL ON [ums_identity].[TENANT] REBUILD PARTITION = 1;
```

---

## 3. RLS Integration with Partitioning

Layer 1 (Primary): EF Core applies partition-aware queries

```csharp
var query = _dbContext.Tenants
    .Where(t => t.RootTenantId == _tenantContext.CurrentRootTenantId
            && t.TenantId == _tenantContext.CurrentTenantId);
    // Implicit partition pruning via root_tenant_id in WHERE
```

Layer 2 (Failsafe): SQL Server RLS prevents cross-partition leaks

```sql
CREATE FUNCTION [ums_identity].[fn_tenant_partition_filter](@RootTenantId UNIQUEIDENTIFIER)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT 1 AS result 
    WHERE @RootTenantId = CAST(SESSION_CONTEXT(N'root_tenant_id') AS UNIQUEIDENTIFIER);

CREATE SECURITY POLICY [ums_identity].[TenantPartitionPolicy]
    ADD FILTER PREDICATE [ums_identity].[fn_tenant_partition_filter](root_tenant_id)
        ON [ums_identity].[TENANT];
```

---

## 4. Migration Path: Flat → Hierarchical + Partitioned

### Phase 1: Add partitioning infrastructure (no table changes)
```sql
-- Create partition function, scheme, filegroups (no data movement)
CREATE PARTITION FUNCTION pf_tenant_by_root ...
CREATE PARTITION SCHEME ps_tenant_by_root ...
```

### Phase 2: Rebuild existing tables on partition scheme
```sql
-- Rebuild TENANT and related tables onto partitions
ALTER TABLE [ums_identity].[TENANT] REBUILD ON ps_tenant_by_root(root_tenant_id);
```

### Phase 3: Verify partition pruning
```sql
-- Enable plan comparison
SET STATISTICS IO ON;
SELECT * FROM [ums_identity].[TENANT] WHERE root_tenant_id = @id;
-- Check "Pages read" should be partition-size, not entire table
```

---

## 5. Future: Cross-Shard Queries (Phase 3)

If a root tenant exceeds single-instance capacity:

1. **Copy partition to shard database:**
   ```sql
   BACKUP DATABASE [ums_tenant_001] TO DISK = '\\shard_db_01_backup.bak';
   RESTORE DATABASE [ums_tenant_001] FROM DISK = '\\shard_db_01_backup.bak';
   ```

2. **Update application routing:**
   ```csharp
   var shardConnection = _shardRouter.GetConnection(rootTenantId);
   // Route queries to shard_db_01 for this tenant
   ```

3. **Linked Server fallback (for queries needing current + shard data):**
   ```sql
   SELECT * FROM [ums].[ums_identity].[TENANT]  -- Local
   UNION ALL
   SELECT * FROM [shard_db_01].[ums_identity].[TENANT]  -- Remote via linked server
   WHERE root_tenant_id IN (
       SELECT root_tenant_id FROM _shard_registry 
       WHERE shard_instance = 'shard_db_01'
   );
   ```

---

## 6. Consequences

### Positive
- ✅ **Fast tenant recovery**: DROP single partition instead of DELETE query
- ✅ **Noisy neighbor isolation**: Each tenant on separate disk/filegroup/maintenance schedule
- ✅ **Index optimization**: Per-partition statistics and rebuild
- ✅ **Query performance**: Partition pruning reduces scanned pages
- ✅ **Sharding ready**: Partitions exist as independent physical units

### Negative
- ⚠️ **Partition key everywhere**: All queries must include `root_tenant_id` for pruning
- ⚠️ **FK constraints**: Cannot easily reference across partitions (enforce in app logic)
- ⚠️ **Partition maintenance overhead**: Filegroup/partition management adds complexity (mitigated by PartitionManager automation)
- ⚠️ **Growth planning**: Must pre-allocate filegroups; dynamic creation requires careful resource management

---

## 7. Implementation Notes

- **Phase:** Post-MVP (Phase 2) or parallel development
- **Prerequisite:** ADR-0048 (Hierarchical Model) - need `root_tenant_id` denormalized
- **Testing:** Partition pruning verification (via SET STATISTICS IO), failover scenarios
- **Monitoring:** sys.dm_db_partition_stats, index fragmentation, filegroup usage

---

## 8. References

- ADR-0048: Hierarchical Multi-Tenancy for SQL Server 2022
- ADR-0041: Authoritative Database Engine Strategy
- [SQL Server Table Partitioning Guide](https://learn.microsoft.com/en-us/sql/relational-databases/partitions/partitioned-tables-and-indexes)
- [SQL Server RLS with Partitioning](https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security)
- [Partition Pruning Optimization](https://learn.microsoft.com/en-us/sql/relational-databases/partitions/partitioned-table-indexes)

