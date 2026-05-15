# ADR-0037: Tenant-Aware Partitioning Strategy

*   **Status:** Superseded by ADR-0049
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team
*   **Supersession Reason:** PostgreSQL-specific syntax (PARTITION BY LIST, FDW, pg_class, autovacuum, foreign data wrapper). Use ADR-0049 (SQL Server adaptation) instead.

---

## 1. Context and Problem

The current UMS uses a shared-schema model with RLS for tenant isolation (ADR-0010). As the system grows to support hierarchical tenants (ADR-0034), three scalability challenges emerge:

1. **Single-tenant data recovery**: Restáoring data for one enterprise group (root tenant + all descendants) requires scanning the entire shared table.
2. **Noisy neighbor**: A high-volume root tenant (e.g., 10M users) degrades query performance for smaller tenants sharing the same physical table.
3. **Vacuum and maintenance**: PostgreSQL autovacuum must scan the full table even when only one root tenant has high write activity.
4. **Future sharding**: The architecture must support migrating entire root tenant partitions to separate database instances without downtime.

Hybrid Pooled (ADR-0010) assumed flat tenants of roughly equal size. The hierarchical model invalidates this assumption — root tenants can vary from 1K to 10M users.

---

## 2. Architectural Decision

We will adopt **declarative LIST partitioning by `root_tenant_id`** for all core tables (tenants, users, policies, policy_bindings, audit_log, delegation_grants). Each root tenant (enterprise group) and its entire sub-tree resides in a single partition.

### 2.1. Partitioning Strategy

```sql
-- Master table (template for partition creation)
CREATE TABLE tenants (
    id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(128),
    type_code VARCHAR(32) NOT NULL,
    status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE',
    root_tenant_id UUID NOT NULL,
    parent_tenant_id UUID,
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (id, root_tenant_id)  -- root_tenant_id in PK for partitioning
) PARTITION BY LIST (root_tenant_id);

-- Each root tenant gets its own partition
CREATE TABLE tenants_root_001 PARTITION OF tenants
    FOR VALUES IN ('00000000-0000-0000-0000-000000000001');

CREATE TABLE tenants_root_002 PARTITION OF tenants
    FOR VALUES IN ('00000000-0000-0000-0000-000000000002');
```

### 2.2. Auto-Provisioining Partitions

```csharp
public class PartitionManager
{
    public async Task EnsurePartitionAsync(Guid rootTenantId)
    {
        var partitionName = $"tenants_{rootTenantId:N}";
        var exists = await dbContext.Database
            .SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM pg_class WHERE relname = '{partitionName}')")
            .FirstAsync();

        if (!exists)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                $"CREATE TABLE {partitionName} PARTITION OF tenants " +
                $"FOR VALUES IN ('{rootTenantId}')");
        }
    }
}
```

### 2.3. Tables to Partition

| Table | Partition Key | Sub-Partition Strategy | Estimated Partitions |
|---|---|---|---|
| `tenants` | `root_tenant_id` | None | 1 per root tenant |
| `users` | `root_tenant_id` | `LIST (user_type)` for admin isolation | 1 per root tenant |
| `policies` | `root_tenant_id` | `RANGE (version)` for temporal queries | 1 per root tenant |
| `policy_bindings` | `root_tenant_id` | None | 1 per root tenant |
| `audit_log` | `root_tenant_id` | `RANGE (created_at)` monthly sub-partitions | 1 per root tenant + monthly |
| `delegation_grants` | `root_tenant_id` | `LIST (status)` for active vs historical | 1 per root tenant | ### 2.4. Closure Table Partitioning

The closure table must use the same partitioning scheme. This requires storing `root_tenant_id` in the closure table (denormalized from tenants).

```sql
CREATE TABLE tenant_closure (
    ancestáor_id UUID NOT NULL,
    descendant_id UUID NOT NULL,
    depth INT NOT NULL CHECK (depth >= 0),
    root_tenant_id UUID NOT NULL,
    PRIMARY KEY (ancestáor_id, descendant_id, root_tenant_id)
) PARTITION BY LIST (root_tenant_id);

-- Each partition mirrors its tenants partition
CREATE TABLE tenant_closure_root_001 PARTITION OF tenant_closure
    FOR VALUES IN ('00000000-0000-0000-0000-000000000001');
```

### 2.5. Sharding Future (Phase 2)

When a root tenant exceeds capacity, migrate its partitions to a dedicated PostgreSQL instance:

```
Step 1: CREATE TABLE tenants_root_001 ON shard_db_01 ( ... );
Step 2: INSERT INTO shard_db_01.tenants_root_001 SELECT * FROM main.tenants_root_001;
Step 3: DETACH PARTITION tenants_root_001 FROM tenants;
Step 4: Configure TenantResolutionService to route root_tenant_id -> shard_db_01;
Step 5: Implement distributed query via foreign data wrapper (FDW) for cross-shard reads.
```

### 2.6. RLS Integration with Partitioning

```sql
CREATE POLICY tenant_isolation ON tenants
    USING (root_tenant_id = current_setting('app.root_tenant_id')::uuid);

CREATE POLICY subtree_access ON users
    USING (
        root_tenant_id = current_setting('app.root_tenant_id')::uuid
        AND (
            home_tenant_id = current_setting('app.effective_tenant_id')::uuid
            OR home_tenant_id IN (
                SELECT descendant_id FROM tenant_closure
                WHERE ancestáor_id = current_setting('app.effective_tenant_id')::uuid
                  AND root_tenant_id = current_setting('app.root_tenant_id')::uuid
)
)
);
```

---

## 3. Consequences

### Positive (Pros)

*   **Fast tenant-level DROP**: `DROP TABLE tenants_root_001` is instant, compared to `DELETE FROM tenants WHERE root_tenant_id = X` which would be a massive vacuum operation.
*   **Independent vacuum**: Each partition can have its own `autovacuum` settings. A high-write root tenant does not force vacuum scans on other partitions.
*   **Sharding path**: Partitions exist as independent physical tables; migrating one to another database is a `CREATE TABLE ... ON shard` + `DETACH PARTITION` operation.
*   **Parallel queries**: PostgreSQL can scan partitions in parallel for cross-tenant administrative queries (e.g., global audit reports).

### Negative (Cons)

*   **Partitioning key everywhere**: Every query must include `root_tenant_id` in WHERE or JOIN conditions. If omitted, PostgreSQL must scan all partitions.
*   **FK constraints across partitions**: NOT possible in PostgreSQL declarative partitioning. Cross-partition referential integrity must be enforced by application logic.
*   **Partition management overhead**: Creating a partition for each new root tenant adds operational complexity. Mitigation: Fully automated via `PartitionManager` service.
*   **Closure table denormalization**: Storing `root_tenant_id` in `tenant_closure` is redundant but required for partitioning.

### Migration Impact

| Concern | Mitigation |
|---|---|
| Existing data without `root_tenant_id` | Backfill migration: update existing rows with their root tenant ID. |
| Zero-downtime partition creation | Use `CREATE TABLE ... PARTITION OF` which is a metadata-only operation in PostgreSQL 16. |
| FK constraint loss | Enforce referential integrity at application layer via `ITenantContext` validation.
## 4. Alternatives Considered

1.  **No partitioning (single table, rely on indexes)**: Rejected. At 100+ root tenants with 10M+ rows each, index scan degradation and vacuum pressure become unmanageable.

2.  **Schema-per-tenant**: Rejected. Creates 1000+ schema objects per tenant (one per table), making migrations and connection pooling impractical.

3.  **Database-per-tenant**: Rejected for the 90% case (ADR-0010). Reserve for VIP/Enterprise clients requiring physical isolation. The partitioning strategy supports easy extraction to dedicated databases when needed.

4.  **TimescaleDB hypertables**: Rejected. While suitable for time-series audit data, the complexity of adding a non-standard extension outweighs the benefits for general-purpose tables.
