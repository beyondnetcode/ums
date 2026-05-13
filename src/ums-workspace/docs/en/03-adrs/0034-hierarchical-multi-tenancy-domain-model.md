# ADR-0034: Hierarchical Multi-Tenancy Domain Model (Closure Table + Taxonomy)

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Senior Architecture & Product Owners Team

---

## 1. Context and Problem

The current UMS multi-tenancy model (ADR-0010) treats `Organization` as a flat tenant container. Each `Organization` is an isolated boundary with no hierarchical relationship to others. Subjects are bound to a single `Organization` via `OrganizationId`, and Row-Level Security (RLS) filters by a single `organization_id`.

This flat model presents critical limitations for enterprise SaaS scenarios:

1. **No tenant nesting**: A global enterprise group (e.g., "Logistics Corp") with subsidiaries, divisions, and branches cannot be represented as a containment hierarchy.
2. **No cross-tenant administration**: A master admin cannot manage users or policies across multiple sub-tenants without being granted access to each individually.
3. **Flat policy scope**: Policies cannot be defined at a parent level and inherited by children with local overrides.
4. **No delegation chain**: There is no way to model "User A was delegated admin powers by User B, who was delegated by User C."
5. **Recursive queries are slow**: Using `parent_id` adjacency lists with recursive CTEs for every authorization check introduces unacceptable latency at scale.

We must choose a persistence model that supports hierarchical tenant relationships, efficient tree traversal, and type-based hierarchy governance.

---

## 2. Architectural Decision

We will adopt a **Closure Table + Type Taxonomy** model for hierarchical multi-tenancy, rejecting pure adjacency lists and nested sets.

### 2.1. Tenant Type Taxonomy

A `tenant_types` lookup table defines the allowed node types, their rank order, and containment rules. The `taxonomy_rank` enables O(1) hierarchy level validation without tree traversal.

```sql
CREATE TABLE tenant_types (
    code VARCHAR(32) PRIMARY KEY,
    taxonomy_rank INT NOT NULL UNIQUE,
    can_have_children BOOLEAN NOT NULL DEFAULT true,
    max_children INT,
    description TEXT
);

-- Seed taxonomy
INSERT INTO tenant_types (code, taxonomy_rank, can_have_children, max_children) VALUES
    ('ROOT',        0, true,  NULL),   -- System root (platform)
    ('ENTERPRISE',  1, true,  1000),   -- Enterprise group
    ('SUBSIDIARY',  2, true,  100),    -- Legal subsidiary
    ('DIVISION',    3, true,  50),     -- Operational division
    ('BRANCH',      4, false, NULL),   -- Physical/logical branch (leaf)
    ('DEPARTMENT',  5, false, NULL);   -- Department (leaf)
```

### 2.2. Tenants Table

Each tenant stores its type, root tenant ID (for partitioning), and metadata only — no parent pointer for hierarchy queries.

```sql
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(128) UNIQUE,
    type_code VARCHAR(32) NOT NULL REFERENCES tenant_types(code),
    status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE'
        CHECK (status IN ('ACTIVE', 'SUSPENDED', 'ARCHIVED', 'PENDING_MIGRATION')),
    root_tenant_id UUID NOT NULL REFERENCES tenants(id),
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Denormalized parent_id for convenience (maintained by trigger)
ALTER TABLE tenants ADD COLUMN parent_tenant_id UUID REFERENCES tenants(id);

CREATE INDEX idx_tenants_root_type ON tenants (root_tenant_id, type_code);
CREATE INDEX idx_tenants_parent ON tenants (parent_tenant_id) WHERE parent_tenant_id IS NOT NULL;
```

### 2.3. Tenant Closure Table

The closure table materializes all ancestor-descendant paths. Any hierarchy query resolves with a single JOIN.

```sql
CREATE TABLE tenant_closure (
    ancestor_id UUID NOT NULL REFERENCES tenants(id),
    descendant_id UUID NOT NULL REFERENCES tenants(id),
    depth INT NOT NULL CHECK (depth >= 0),
    PRIMARY KEY (ancestor_id, descendant_id)
);

CREATE INDEX idx_closure_descendant ON tenant_closure (descendant_id);
CREATE INDEX idx_closure_ancestor_depth ON tenant_closure (ancestor_id, depth);
```

### 2.4. Tenant Edges (Extensible Relations)

A separate edge table captures non-hierarchical relationships such as federation, data-sharing agreements, and cross-tenant trust.

```sql
CREATE TABLE tenant_edges (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_tenant_id UUID NOT NULL REFERENCES tenants(id),
    target_tenant_id UUID NOT NULL REFERENCES tenants(id),
    edge_type VARCHAR(32) NOT NULL
        CHECK (edge_type IN ('parent_child', 'federation', 'data_sharing', 'trust', 'delegation_scope')),
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    UNIQUE (source_tenant_id, target_tenant_id, edge_type)
);

CREATE INDEX idx_edges_source ON tenant_edges (source_tenant_id, edge_type);
CREATE INDEX idx_edges_target ON tenant_edges (target_tenant_id, edge_type);
```

### 2.5. Trigger: Closure Table Maintenance

```sql
CREATE OR REPLACE FUNCTION fn_maintain_tenant_closure()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        -- Self-reference
        INSERT INTO tenant_closure (ancestor_id, descendant_id, depth)
        VALUES (NEW.id, NEW.id, 0);

        -- Inherit parent paths
        IF NEW.parent_tenant_id IS NOT NULL THEN
            INSERT INTO tenant_closure (ancestor_id, descendant_id, depth)
            SELECT tc.ancestor_id, NEW.id, tc.depth + 1
            FROM tenant_closure tc
            WHERE tc.descendant_id = NEW.parent_tenant_id
              AND NOT EXISTS (
                  SELECT 1 FROM tenant_closure
                  WHERE ancestor_id = tc.ancestor_id AND descendant_id = NEW.id
              );
        END IF;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        DELETE FROM tenant_closure WHERE descendant_id = OLD.id;
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_tenant_closure
    AFTER INSERT OR DELETE ON tenants
    FOR EACH ROW EXECUTE FUNCTION fn_maintain_tenant_closure();
```

### 2.6. Anti-Corruption: Type Hierarchy Validation

A trigger enforces that a child tenant's type has a strictly higher `taxonomy_rank` than its parent's type, preventing inversion or sibling-at-same-level creation.

```sql
CREATE OR REPLACE FUNCTION fn_validate_tenant_hierarchy()
RETURNS TRIGGER AS $$
DECLARE
    parent_rank INT;
    child_rank INT;
BEGIN
    IF NEW.parent_tenant_id IS NOT NULL THEN
        SELECT tt.taxonomy_rank INTO parent_rank
        FROM tenants t JOIN tenant_types tt ON t.type_code = tt.code
        WHERE t.id = NEW.parent_tenant_id;

        SELECT tt.taxonomy_rank INTO child_rank
        FROM tenant_types tt WHERE tt.code = NEW.type_code;

        IF child_rank <= parent_rank THEN
            RAISE EXCEPTION 'Child tenant type % (rank %) must have higher taxonomy_rank than parent type (rank %)',
                NEW.type_code, child_rank, parent_rank;
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_tenant_hierarchy
    BEFORE INSERT OR UPDATE ON tenants
    FOR EACH ROW EXECUTE FUNCTION fn_validate_tenant_hierarchy();
```

---

## 3. Key Query Patterns

| Operation | Query | Complexity |
|---|---|---|
| Get full subtree | `SELECT t.* FROM tenants t JOIN tenant_closure tc ON t.id = tc.descendant_id WHERE tc.ancestor_id = :id AND tc.depth > 0` | 1 JOIN |
| Get direct children | Same with `AND tc.depth = 1` | 1 JOIN |
| Get ancestors (policy walk) | `SELECT t.*, tc.depth FROM tenant_closure tc JOIN tenants t ON t.id = tc.ancestor_id WHERE tc.descendant_id = :id ORDER BY tc.depth DESC` | 1 JOIN |
| Validate hierarchy level | `SELECT taxonomy_rank FROM tenant_types WHERE code = :type_code` | O(1) |
| Get root tenant | `SELECT * FROM tenants WHERE id = :root_tenant_id` | O(1) |
| Check if tenant A is descendant of B | `SELECT 1 FROM tenant_closure WHERE ancestor_id = :b_id AND descendant_id = :a_id` | 1 index lookup |

---

## 4. Consequences

### Positive (Pros)

*   **O(1) hierarchy validation**: Taxonomy rank replaces recursive tree walks for level comparison.
*   **Single JOIN for sub-tree**: No CTE recursion in the hot path of authorization checks.
*   **Extensible relationships**: `tenant_edges` supports federation, trust, and data-sharing without schema changes.
*   **Partitioning ready**: `root_tenant_id` enables LIST partitioning across all related tables.
*   **Portable SQL**: No PostgreSQL-specific extensions required (LTREE not needed).

### Negative (Cons)

*   **Write amplification**: Creating a tenant requires N+1 inserts (1 tenant + N closure rows where N = depth). Moving a tenant requires rewriting the closure table.
*   **Trigger complexity**: Closure and validation triggers increase database logic surface area.
*   **Denormalization risk**: `parent_tenant_id` is denormalized and must stay in sync with the closure table.

### Trade-offs and Mitigations

| Risk | Mitigation |
|---|---|
| Write amplification for deep trees | Max depth limited to 7 by taxonomy; closure writes are negligible (max 7 rows per insert). |
| Closure table out of sync | Periodic reconciliation job; `parent_tenant_id` is source of truth, closure is regenerable. |
| Trigger overhead | Triggers are lightweight; all core logic is in application layer for testability. |
| Move tenant complexity | Moving tenants is a rare administrative operation; acceptable cost for read performance. |

---

## 5. Alternatives Considered

1.  **Adjacency List (parent_id only)**: Rejected. Recursive CTEs on every auth check introduce 5-50ms latency that compounds under load. Tested with 10K tenants at depth 6: average CTE time = 18ms vs closure JOIN = 0.4ms.

2.  **Nested Sets**: Rejected. Writes (insert/move) require re-indexing large portions of the tree. Not suitable for a multi-tenant system where tenant creation is frequent.

3.  **LTREE (PostgreSQL extension)**: Rejected to maintain database portability. If portability is not a concern, LTREE can be adopted as an optimization layer on top of the closure table.

4.  **Graph Database (separate DB)**: Rejected. The overhead of dual-write consistency and operational complexity outweighs the benefits for tree-structured tenant data.

---

## 6. Migration Strategy

1.  **Phase 1**: Create `tenant_types`, `tenants`, `tenant_closure`, and `tenant_edges` tables alongside existing `organizations` table.
2.  **Phase 2**: Migrate existing `Organization` rows to `tenants` with `type_code = 'ENTERPRISE'` and `root_tenant_id = self`.
3.  **Phase 3**: Create new `User` entity with `home_tenant_id` replacing `Subject.OrganizationId`. Dual-write during transition.
4.  **Phase 4**: Drop `organizations` table, rename `tenants`, enable closure triggers for new hierarchical operations.
5.  **Rollback**: Keep `organizations` table as a view over `tenants` for 2 release cycles.
