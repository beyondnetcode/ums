# ADR-0048: Hierarchical Multi-Tenancy Domain Model for SQL Server 2022

* **Status:** Accepted
* **Date:** 2026-05-14
* **Authors:** Architecture Team
* **Supersedes:** ADR-0034 (PostgreSQL version)
* **Related:** ADR-0010 (Core Multi-Tenancy), ADR-0041 (Database Engine Strategy)

---

## 1. Context and Problem

The current UMS multi-tenancy model (ADR-0010) treats `Organization` as a flat tenant container. For future enterprise scenarios, we need hierarchical tenant relationships (parent-child organizations, divisions, branches) while maintaining the SQL Server 2022 technology stack.

**Key Requirements:**
- Support tenant hierarchies (ROOT → ENTERPRISE → DIVISION → BRANCH)
- Enable cross-tenant administration for parent organizations
- Efficient hierarchy traversal without recursive CTEs on every query
- Type-based hierarchy validation (not all types can have children)
- Compatible with SQL Server 2022 T-SQL and .NET 8 EF Core

---

## 2. Architectural Decision

Adopt **Closure Table + Type Taxonomy** model, adapted for SQL Server 2022 native types and features.

### 2.1 Tenant Type Taxonomy

Define allowed node types and containment rules:

```sql
CREATE TABLE [ums_identity].[tenant_types] (
 code VARCHAR(32) PRIMARY KEY,
 taxonomy_rank INT NOT NULL UNIQUE CHECK (taxonomy_rank >= 0),
 can_have_children BIT NOT NULL DEFAULT 1,
 max_children INT,
 description NVARCHAR(MAX),
 created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
 CONSTRAINT chk_rank_positive CHECK (taxonomy_rank >= 0)
);

-- Seed taxonomy
INSERT INTO [ums_identity].[tenant_types] (code, taxonomy_rank, can_have_children, max_children, description) VALUES
 ('ROOT', 0, 1, NULL, 'System root (platform owner)'),
 ('ENTERPRISE', 1, 1, 1000, 'Enterprise group / holding'),
 ('SUBSIDIARY', 2, 1, 100, 'Legal subsidiary'),
 ('DIVISION', 3, 1, 50, 'Operational division'),
 ('BRANCH', 4, 0, NULL, 'Physical/logical branch (leaf)'),
 ('DEPARTMENT', 5, 0, NULL, 'Department (leaf)');
```

### 2.2 Tenants Table

Store tenant metadata and type classification:

```sql
CREATE TABLE [ums_identity].[TENANT] (
 tenant_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
 name NVARCHAR(255) NOT NULL,
 slug VARCHAR(128) UNIQUE,
 type_code VARCHAR(32) NOT NULL REFERENCES [ums_identity].[tenant_types](code),
 status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE'
 CHECK (status IN ('ACTIVE', 'SUSPENDED', 'ARCHIVED')),
 root_tenant_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
 parent_tenant_id UNIQUEIDENTIFIER NULL REFERENCES [ums_identity].[TENANT](tenant_id),
 metadata NVARCHAR(MAX), -- JSON payload

-- Audit columns (standard 10-column schema)
 created_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
 created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
 modified_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
 modified_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

 CONSTRAINT chk_metadata_json CHECK (
 metadata IS NULL OR ISJSON(metadata) = 1
 ),
 CONSTRAINT chk_root_self_reference CHECK (
 root_tenant_id <> tenant_id OR type_code = 'ROOT'
 )
);

CREATE NONCLUSTERED INDEX idx_tenant_root_type
 ON [ums_identity].[TENANT] (root_tenant_id, type_code);
CREATE NONCLUSTERED INDEX idx_tenant_parent
 ON [ums_identity].[TENANT] (parent_tenant_id)
 WHERE parent_tenant_id IS NOT NULL;
CREATE NONCLUSTERED INDEX idx_tenant_status
 ON [ums_identity].[TENANT] (status);
```

### 2.3 Tenant Closure Table

Materializes all ancestor-descendant paths for O(1) hierarchy queries:

```sql
CREATE TABLE [ums_identity].[tenant_closure] (
 ancestor_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
 descendant_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
 depth INT NOT NULL CHECK (depth >= 0),
 root_tenant_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),

 PRIMARY KEY (ancestor_id, descendant_id),
 CONSTRAINT fk_closure_root FOREIGN KEY (root_tenant_id)
 REFERENCES [ums_identity].[TENANT](tenant_id)
);

CREATE NONCLUSTERED INDEX idx_closure_descendant
 ON [ums_identity].[tenant_closure] (descendant_id, root_tenant_id);
CREATE NONCLUSTERED INDEX idx_closure_ancestor_depth
 ON [ums_identity].[tenant_closure] (ancestor_id, depth);
```

### 2.4 Tenant Closure Maintenance (T-SQL Trigger)

Automatically maintain closure table on tenant insert/delete:

```sql
CREATE OR ALTER TRIGGER [ums_identity].[tr_maintain_tenant_closure]
ON [ums_identity].[TENANT]
AFTER INSERT, DELETE
AS
BEGIN
 SET NOCOUNT ON;

-- Insert: Add self-reference and inherit parent paths
 IF EXISTS (SELECT 1 FROM inserted)
 BEGIN
-- Self-reference (depth 0)
 INSERT INTO [ums_identity].[tenant_closure]
 (ancestor_id, descendant_id, depth, root_tenant_id)
 SELECT
 i.tenant_id, i.tenant_id, 0, i.root_tenant_id
 FROM inserted i
 WHERE NOT EXISTS (
 SELECT 1 FROM [ums_identity].[tenant_closure] tc
 WHERE tc.ancestor_id = i.tenant_id
 AND tc.descendant_id = i.tenant_id
 );

-- Inherit parent paths
 INSERT INTO [ums_identity].[tenant_closure]
 (ancestor_id, descendant_id, depth, root_tenant_id)
 SELECT
 tc.ancestor_id, i.tenant_id, tc.depth + 1, i.root_tenant_id
 FROM inserted i
 INNER JOIN [ums_identity].[tenant_closure] tc
 ON tc.descendant_id = i.parent_tenant_id
 WHERE i.parent_tenant_id IS NOT NULL
 AND NOT EXISTS (
 SELECT 1 FROM [ums_identity].[tenant_closure]
 WHERE ancestor_id = tc.ancestor_id
 AND descendant_id = i.tenant_id
 );
 END

-- Delete: Remove all descendant paths
 IF EXISTS (SELECT 1 FROM deleted)
 BEGIN
 DELETE FROM [ums_identity].[tenant_closure]
 WHERE descendant_id IN (SELECT tenant_id FROM deleted);
 END
END;
```

### 2.5 Type Validation Stored Procedure

Enforce containment rules before insert/update:

```sql
CREATE OR ALTER PROCEDURE [ums_identity].[sp_validate_tenant_hierarchy]
 @tenant_id UNIQUEIDENTIFIER,
 @parent_tenant_id UNIQUEIDENTIFIER,
 @type_code VARCHAR(32),
 @result BIT OUTPUT,
 @error_message NVARCHAR(MAX) OUTPUT
AS
BEGIN
 SET NOCOUNT ON;
 SET @result = 1;
 SET @error_message = NULL;

-- Validate parent exists
 IF @parent_tenant_id IS NOT NULL
 BEGIN
 IF NOT EXISTS (
 SELECT 1 FROM [ums_identity].[TENANT]
 WHERE tenant_id = @parent_tenant_id AND status = 'ACTIVE'
 )
 BEGIN
 SET @result = 0;
 SET @error_message = 'Parent tenant does not exist or is not active.';
 RETURN;
 END

-- Validate parent can have children
 DECLARE @parent_type VARCHAR(32);
 SELECT @parent_type = t.type_code
 FROM [ums_identity].[TENANT] t
 WHERE t.tenant_id = @parent_tenant_id;

 IF NOT EXISTS (
 SELECT 1 FROM [ums_identity].[tenant_types]
 WHERE code = @parent_type AND can_have_children = 1
 )
 BEGIN
 SET @result = 0;
 SET @error_message = 'Parent type cannot have children.';
 RETURN;
 END

-- Validate child type is lower rank than parent
 DECLARE @parent_rank INT, @child_rank INT;
 SELECT @parent_rank = taxonomy_rank
 FROM [ums_identity].[tenant_types] WHERE code = @parent_type;
 SELECT @child_rank = taxonomy_rank
 FROM [ums_identity].[tenant_types] WHERE code = @type_code;

 IF @child_rank <= @parent_rank
 BEGIN
 SET @result = 0;
 SET @error_message = 'Child type must have higher rank than parent.';
 RETURN;
 END
 END
END;
```

### 2.6 Helper Views for Common Queries

```sql
-- View: Get full hierarchy path for a tenant
CREATE OR ALTER VIEW [ums_identity].[v_tenant_hierarchy_path] AS
SELECT
 tc.descendant_id,
 tc.ancestor_id,
 tc.depth,
 t.name,
 t.type_code,
 t.root_tenant_id
FROM [ums_identity].[tenant_closure] tc
INNER JOIN [ums_identity].[TENANT] t
 ON tc.ancestor_id = t.tenant_id
WHERE tc.depth > 0 -- Exclude self-references
ORDER BY tc.descendant_id, tc.depth;

-- View: Get direct children
CREATE OR ALTER VIEW [ums_identity].[v_tenant_direct_children] AS
SELECT
 parent_tenant_id,
 tenant_id,
 name,
 type_code
FROM [ums_identity].[TENANT]
WHERE parent_tenant_id IS NOT NULL
 AND status = 'ACTIVE';

-- View: Get all descendants (for admin scope)
CREATE OR ALTER VIEW [ums_identity].[v_tenant_descendants] AS
SELECT DISTINCT
 ancestor_id AS admin_tenant_id,
 descendant_id AS managed_tenant_id,
 depth
FROM [ums_identity].[tenant_closure]
WHERE depth > 0
ORDER BY admin_tenant_id, depth;
```

---

## 3. EF Core Integration (.NET 8)

### 3.1 Aggregate Root

```csharp
public class Tenant : AggregateRoot
{
 public Guid TenantId { get; private set; }
 public string Name { get; set; }
 public string Slug { get; set; }
 public TenantType Type { get; set; }
 public TenantStatus Status { get; set; }
 public Guid RootTenantId { get; private set; }
 public Guid? ParentTenantId { get; set; }
 public Tenant Parent { get; set; }
 public ICollection<Tenant> Children { get; set; } = new List<Tenant>();
 public JsonDocument Metadata { get; set; }

 // Factory method
 public static Tenant CreateRoot(string name, string slug)
 {
 var tenant = new Tenant
 {
 TenantId = Guid.NewGuid(),
 RootTenantId = Guid.NewGuid(), // Self-reference for ROOT
 Name = name,
 Slug = slug,
 Type = TenantType.Root,
 Status = TenantStatus.Active
 };
 return tenant;
 }

 // Hierarchy operations
 public void AddChild(Tenant child)
 {
 if (Type.CanHaveChildren == false)
 throw new InvalidOperationException("This tenant type cannot have children.");

 child.ParentTenantId = TenantId;
 child.RootTenantId = RootTenantId;
 Children.Add(child);
 }
}

public class TenantClosure
{
 public Guid AncestorId { get; set; }
 public Guid DescendantId { get; set; }
 public int Depth { get; set; }
 public Guid RootTenantId { get; set; }
}
```

### 3.2 EF Core Mapping

```csharp
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
 public void Configure(EntityTypeBuilder<Tenant> builder)
 {
 builder.ToTable("TENANT", "ums_identity");
 builder.HasKey(t => t.TenantId);

 builder.Property(t => t.Name).HasMaxLength(255).IsRequired();
 builder.Property(t => t.Slug).HasMaxLength(128);
 builder.Property(t => t.Status).HasConversion<string>();

 builder.HasOne(t => t.Parent)
 .WithMany(t => t.Children)
 .HasForeignKey(t => t.ParentTenantId)
 .IsRequired(false);

 // Closure table mapping
 builder.HasMany<TenantClosure>()
 .WithOne()
 .HasForeignKey(tc => tc.DescendantId);
 }
}
```

### 3.3 Repository with Hierarchy Queries

```csharp
public class TenantRepository : IRepository<Tenant>
{
 private readonly DbContext _dbContext;

 // Get all ancestors of a tenant (from root to parent)
 public async Task<IEnumerable<Tenant>> GetAncestorsAsync(Guid tenantId)
 {
 return await _dbContext.Set<TenantClosure>()
 .Where(tc => tc.DescendantId == tenantId && tc.Depth > 0)
 .OrderByDescending(tc => tc.Depth)
 .Select(tc => new { tc.AncestorId })
 .Join(
 _dbContext.Set<Tenant>(),
 tc => tc.AncestorId,
 t => t.TenantId,
 (tc, t) => t
 )
 .ToListAsync();
 }

 // Get all descendants (for admin scope)
 public async Task<IEnumerable<Tenant>> GetDescendantsAsync(Guid tenantId)
 {
 return await _dbContext.Set<TenantClosure>()
 .Where(tc => tc.AncestorId == tenantId && tc.Depth > 0)
 .Select(tc => tc.DescendantId)
 .Join(
 _dbContext.Set<Tenant>(),
 id => id,
 t => t.TenantId,
 (id, t) => t
 )
 .ToListAsync();
 }
}
```

---

## 4. Multi-Tenancy & RLS Integration

Layer 1 (Primary): EF Core applies `HasQueryFilter` with `root_tenant_id`

```csharp
modelBuilder.Entity<Tenant>().HasQueryFilter(
 t => t.RootTenantId == _tenantContext.CurrentRootTenantId
);
```

Layer 2 (Failsafe): SQL Server RLS enforces via SESSION_CONTEXT

```sql
CREATE FUNCTION [ums_identity].[fn_tenant_filter_predicate](@RootTenantId UNIQUEIDENTIFIER)
RETURNS TABLE WITH SCHEMABINDING
AS RETURN
 SELECT 1 AS result
 WHERE @RootTenantId = CAST(SESSION_CONTEXT(N'root_tenant_id') AS UNIQUEIDENTIFIER);

CREATE SECURITY POLICY [ums_identity].[TenantIsolationPolicy]
 ADD FILTER PREDICATE [ums_identity].[fn_tenant_filter_predicate](root_tenant_id)
 ON [ums_identity].[TENANT],
 ADD BLOCK PREDICATE [ums_identity].[fn_tenant_filter_predicate](root_tenant_id)
 ON [ums_identity].[TENANT] AFTER INSERT;
```

---

## 5. Consequences

### Positive
- O(1) hierarchy queries via closure table
- Type-safe hierarchy (taxonomy rank validation)
- Native SQL Server NVARCHAR/UNIQUEIDENTIFIER types
- Compatible with EF Core .NET 8
- Trigger-based closure maintenance (no application overhead)
- Ready for future partitioning (ADR-0049)

### Negative
- Closure table doubles storage (but acceptable for typical org depth 5-7)
- Trigger overhead on tenant creation (minimal, <1ms)
- Requires discipline: all hierarchy queries use closure table, not parent_id

---

## 6. Implementation Notes

- **Phase:** Post-MVP (Phase 2) or parallel development if resources allow
- **Dependencies:** ADR-0010, ADR-0041, ADR-0049 (partitioning strategy)
- **Testing:** Closure table consistency checks, hierarchy traversal correctness
- **Rollout:** Blue-green migration from flat model (keep parent_id during transition)

---

## 7. References

- ADR-0010: Core Multi-Tenancy Architecture Strategy
- ADR-0041: Authoritative Database Engine Strategy (SQL Server 2022)
- ADR-0049: Tenant-Aware Partitioning Strategy for SQL Server
- [SQL Server Documentation: Hierarchical Data](https://learn.microsoft.com/en-us/sql/relational-databases/hierarchical-data-sql-server)
- [EF Core: Entity Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)

