# ADR-0048: Modelo Jerárquico Multi-Tenant para SQL Server 2022

*   **Estado:** Aceptado
*   **Fecha:** 2026-05-14
*   **Autores:** Equipo de Arquitectura
*   **Reemplaza:** ADR-0034 (versión PostgreSQL)
*   **Relacionado:** ADR-0010 (Multi-Tenancy Básica), ADR-0041 (Estrategia de Motor de BD)

---

## 1. Contexto y Problema

El modelo actual de multi-tenancy en UMS (ADR-0010) trata `Organization` como un contenedor plano sin relaciones jerárquicas. Para escenarios empresariales futuros, necesitamos jerarquías de tenants (parent-child organizations, divisiones, sedes) mientras mantenemos la pila tecnológica SQL Server 2022.

**Requisitos clave:**
- Soportar jerarquías de tenants (ROOT → ENTERPRISE → DIVISION → BRANCH)
- Permitir administración cross-tenant para organizaciones padres
- Recorrido eficiente de jerarquía sin CTEs recursivos en cada consulta
- Validación de jerarquía basada en tipos (no todos los tipos pueden tener hijos)
- Compatible con T-SQL y .NET 8 EF Core

---

## 2. Decisión Arquitectónica

Adoptar modelo de **Closure Table + Taxonomy de Tipos**, adaptado para tipos nativos de SQL Server 2022 y características.

### 2.1 Taxonomy de Tipos de Tenant

Definir tipos de nodos permitidos y reglas de contención:

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

-- Poblar taxonomy
INSERT INTO [ums_identity].[tenant_types] (code, taxonomy_rank, can_have_children, max_children, description) VALUES
    ('ROOT',        0, 1, NULL,   'Raíz del sistema (propietario de plataforma)'),
    ('ENTERPRISE',  1, 1, 1000,   'Grupo empresarial / holding'),
    ('SUBSIDIARY',  2, 1, 100,    'Filial legal'),
    ('DIVISION',    3, 1, 50,     'División operativa'),
    ('BRANCH',      4, 0, NULL,   'Sede física/lógica (hoja)'),
    ('DEPARTMENT',  5, 0, NULL,   'Departamento (hoja)');
```

### 2.2 Tabla Tenants

Almacenar metadatos de tenant y clasificación de tipo:

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
    metadata NVARCHAR(MAX), -- Payload JSON
    
    -- Columnas de auditoría (esquema estándar de 10 columnas)
    created_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    modified_by NVARCHAR(128) NOT NULL DEFAULT SYSTEM_USER,
    modified_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE NONCLUSTERED INDEX idx_tenant_root_type 
    ON [ums_identity].[TENANT] (root_tenant_id, type_code);
CREATE NONCLUSTERED INDEX idx_tenant_parent 
    ON [ums_identity].[TENANT] (parent_tenant_id) 
    WHERE parent_tenant_id IS NOT NULL;
```

### 2.3 Tabla Closure de Tenants

Materializa todos los caminos ancestro-descendiente para consultas O(1):

```sql
CREATE TABLE [ums_identity].[tenant_closure] (
    ancestor_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
    descendant_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
    depth INT NOT NULL CHECK (depth >= 0),
    root_tenant_id UNIQUEIDENTIFIER NOT NULL REFERENCES [ums_identity].[TENANT](tenant_id),
    
    PRIMARY KEY (ancestor_id, descendant_id)
);

CREATE NONCLUSTERED INDEX idx_closure_descendant 
    ON [ums_identity].[tenant_closure] (descendant_id, root_tenant_id);
CREATE NONCLUSTERED INDEX idx_closure_ancestor_depth 
    ON [ums_identity].[tenant_closure] (ancestor_id, depth);
```

### 2.4 Trigger para Mantenimiento de Closure (T-SQL)

Mantener automáticamente tabla closure en insert/delete de tenant:

```sql
CREATE OR ALTER TRIGGER [ums_identity].[tr_maintain_tenant_closure]
ON [ums_identity].[TENANT]
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Insert: Agregar auto-referencia y heredar rutas del padre
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- Auto-referencia (depth 0)
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
        
        -- Heredar rutas del padre
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
    
    -- Delete: Remover todas las rutas descendientes
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        DELETE FROM [ums_identity].[tenant_closure]
        WHERE descendant_id IN (SELECT tenant_id FROM deleted);
    END
END;
```

---

## 3. Integración EF Core (.NET 8)

```csharp
public class Tenant : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; set; }
    public TenantType Type { get; set; }
    public Guid RootTenantId { get; private set; }
    public Guid? ParentTenantId { get; set; }
    public Tenant Parent { get; set; }
    public ICollection<Tenant> Children { get; set; } = new List<Tenant>();
    
    public static Tenant CreateRoot(string name)
    {
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            RootTenantId = Guid.NewGuid(),
            Name = name,
            Type = TenantType.Root
        };
        return tenant;
    }
    
    public void AddChild(Tenant child)
    {
        if (!Type.CanHaveChildren)
            throw new InvalidOperationException("Este tipo de tenant no puede tener hijos.");
        
        child.ParentTenantId = TenantId;
        child.RootTenantId = RootTenantId;
        Children.Add(child);
    }
}
```

---

## 4. Referencias

- ADR-0010: Estrategia Arquitectónica Multi-Tenancy Básica
- ADR-0041: Estrategia de Motor de Base de Datos Autorizada
- ADR-0049: Estrategia de Particionamiento Consciente de Tenants para SQL Server

