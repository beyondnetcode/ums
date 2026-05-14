# ADR-0034: Modelo de Dominio Multi-Inquilino Jerárquico (Closure Table + Taxonomía)

*   **Estado:** Reemplazado por ADR-0048
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners
*   **Razón de Reemplazo:** La sintaxis PostgreSQL es incompatible con SQL Server 2022. Usar ADR-0048 (adaptación para SQL Server) en su lugar.

---

## 1. Contexto y Problema

El modelo actual de multi-inquilino del UMS (ADR-0010) trata a `Organization` como un contenedor plano de inquilino. Cada `Organization` es un límite aislado sin relación jerárquica con otros. Los `Subject` estáán vinculados a una sola `Organization` mediante `OrganizationId`, y la seguridad a nivel de fila (RLS) filtra por un único `organization_id`.

Este modelo plano presenta limitaciones críticas para escenarios SaaS empresariales:

1.  **Sin anidamiento de inquilinos**: Un grupo empresarial global (ej. "Logistics Corp") con subsidiarias, divisiones y sucursales no puede representarse como una jerarquía de contención.
2.  **Sin administración cross-tenant**: Un administrador maestáro no puede gestionar usuarios o políticas en múltiples sub-inquilinos sin que se le conceda acceso a cada uno individualmente.
3.  **Alcance de políticas plano**: Las políticas no pueden definirse a nivel padre y heredarse por hijos con anulaciones (overrides) locales.
4.  **Sin cadena de delegación**: No existe forma de modelar "El Usuario A recibió poderes de administrador delegado del Usuario B, quien los recibió del Usuario C."
5.  **Consultas recursivas lentas**: Usar listas de adyacencia `parent_id` con CTE recursivos en cada verificación de autorización introduce una latencia inaceptable a escala.

Debemos elegir un modelo de persistencia que soporte relaciones jerárquicas entre inquilinos, recorrido eficiente del árbol y gobernanza de jerarquía basada en tipos.

---

## 2. Decisión Arquitectónica

Adoptaremos un modelo de **Closure Table + Taxonomía de Tipos** para multi-inquilino jerárquico, rechazando listas de adyacencia puras y conjuntos anidados (nested sets).

### 2.1. Taxonomía de Tipos de Inquilino

Una tabla de referencia `tenant_types` define los tipos de nodo permitidos, su orden jerárquico y reglas de contención. El `taxonomy_rank` permite la validación del nivel jerárquico en O(1) sin recorrer el árbol.

```sql
CREATE TABLE tenant_types (
    code VARCHAR(32) PRIMARY KEY,
    taxonomy_rank INT NOT NULL UNIQUE,
    can_have_children BOOLEAN NOT NULL DEFAULT true,
    max_children INT,
    description TEXT
);

INSERT INTO tenant_types (code, taxonomy_rank, can_have_children, max_children) VALUES
    ('ROOT',        0, true,  NULL),
    ('ENTERPRISE',  1, true,  1000),
    ('SUBSIDIARY',  2, true,  100),
    ('DIVISION',    3, true,  50),
    ('BRANCH',      4, false, NULL),
    ('DEPARTMENT',  5, false, NULL);
```

### 2.2. Tabla de Inquilinos

Cada inquilino almacena su tipo, ID del inquilino raíz (para particionamiento) y metadatos — sin puntero padre para consultas de jerarquía.

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

ALTER TABLE tenants ADD COLUMN parent_tenant_id UUID REFERENCES tenants(id);

CREATE INDEX idx_tenants_root_type ON tenants (root_tenant_id, type_code);
CREATE INDEX idx_tenants_parent ON tenants (parent_tenant_id) WHERE parent_tenant_id IS NOT NULL;
```

### 2.3. Closure Table de Inquilinos

La closure table materializa todas las rutas ancestáro-descendiente. Cualquier consulta de jerarquía se resuelve con un solo JOIN.

```sql
CREATE TABLE tenant_closure (
    ancestáor_id UUID NOT NULL REFERENCES tenants(id),
    descendant_id UUID NOT NULL REFERENCES tenants(id),
    depth INT NOT NULL CHECK (depth >= 0),
    PRIMARY KEY (ancestáor_id, descendant_id)
);

CREATE INDEX idx_closure_descendant ON tenant_closure (descendant_id);
CREATE INDEX idx_closure_ancestáor_depth ON tenant_closure (ancestáor_id, depth);
```

### 2.4. Aristas de Inquilinos (Relaciones Extensibles)

Una tabla de aristas separada captura relaciones no jerárquicas como federación, acuerdos de compartición de datos y confianza cross-tenant.

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

### 2.5. Trigger: Mantenimiento de Closure Table

```sql
CREATE OR REPLACE FUNCTION fn_maintain_tenant_closure()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO tenant_closure (ancestáor_id, descendant_id, depth)
        VALUES (NEW.id, NEW.id, 0);
        IF NEW.parent_tenant_id IS NOT NULL THEN
            INSERT INTO tenant_closure (ancestáor_id, descendant_id, depth)
            SELECT tc.ancestáor_id, NEW.id, tc.depth + 1
            FROM tenant_closure tc
            WHERE tc.descendant_id = NEW.parent_tenant_id
              AND NOT EXISTS (
                  SELECT 1 FROM tenant_closure
                  WHERE ancestáor_id = tc.ancestáor_id AND descendant_id = NEW.id
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

### 2.6. Validación de Jerarquía por Tipo

Un trigger de validación fuerza que el tipo de un inquilino hijo tenga un `taxonomy_rank` estárictamente mayor que el de su padre.

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
            RAISE EXCEPTION 'El tipo de inquilino hijo % (rango %) debe tener taxonomy_rank mayor que el tipo padre (rango %)',
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

## 3. Patrones de Consulta Clave

| Operación | Consulta | Complejidad |
|---|---|---|
| Obtener subárbol completo | `SELECT t.* FROM tenants t JOIN tenant_closure tc ON t.id = tc.descendant_id WHERE tc.ancestáor_id = :id AND tc.depth > 0` | 1 JOIN |
| Obtener hijos directos | Misma con `AND tc.depth = 1` | 1 JOIN |
| Obtener ancestáros (resolución de políticas) | `SELECT t.*, tc.depth FROM tenant_closure tc JOIN tenants t ON t.id = tc.ancestáor_id WHERE tc.descendant_id = :id ORDER BY tc.depth DESC` | 1 JOIN |
| Validar nivel jerárquico | `SELECT taxonomy_rank FROM tenant_types WHERE code = :type_code` | O(1) |
| Obtener inquilino raíz | `SELECT * FROM tenants WHERE id = :root_tenant_id` | O(1) |
| Verificar descendencia | `SELECT 1 FROM tenant_closure WHERE ancestáor_id = :b_id AND descendant_id = :a_id` | 1 index lookup
## 4. Consecuencias

### Positivas

*   **Validación de jerarquía O(1)**: El rango de taxonomía reemplaza recorridos recursivos del árbol para comparación de niveles.
*   **Un solo JOIN para subárbol**: Sin CTE recursivo en la ruta crítica de las verificaciones de autorización.
*   **Relaciones extensibles**: `tenant_edges` soporta federación, confianza y compartición de datos sin cambios de esquema.
*   **Preparado para particionamiento**: `root_tenant_id` permite particionamiento LIST en todas las tablas relacionadas.
*   **SQL portátil**: No se requieren extensiones específicas de PostgreSQL (LTREE no es necesario).

### Negativas

*   **Amplificación de escritura**: Crear un inquilino requiere N+1 inserts (1 inquilino + N filas closure donde N = profundidad).
*   **Complejidad de triggers**: Los triggers de closure y validación incrementan la superficie de lógica en base de datos.
*   **Riesgo de desnormalización**: `parent_tenant_id` estáá desnormalizado y debe mantenerse sincronizado con la closure table.

### Riesgos y Mitigaciones

| Riesgo | Mitigación |
|---|---|
| Amplificación de escritura en árboles profundos | Profundidad máxima limitada a 7 por taxonomía; las escrituras closure son insignificantes (máximo 7 filas por insert). |
| Closure table fuera de sincronía | Trabajo de reconciliación periódico; `parent_tenant_id` es la fuente de verdad, la closure es regenerable. |
| Sobrecarga de triggers | Los triggers son ligeros; toda la lógica central estáá en la capa de aplicación para testabilidad. |
| Complejidad al mover inquilinos | Mover inquilinos es una operación administrativa rara; costo aceptable por el rendimiento de lectura.
## 5. Alternativas Consideradas

1.  **Lista de Adyacencia (solo parent_id)**: Rechazada. Los CTE recursivos en cada verificación de autenticación introducen 5-50ms de latencia que se acumulan bajo carga. Probado con 10K inquilinos a profundidad 6: tiempo promedio de CTE = 18ms vs closure JOIN = 0.4ms.

2.  **Conjuntos Anidados (Nested Sets)**: Rechazados. Las escrituras (insert/mover) requieren re-indexar grandes porciones del árbol. No es adecuado para un sistema multi-inquilino donde la creación de inquilinos es frecuente.

3.  **LTREE (extensión PostgreSQL)**: Rechazada para mantener portabilidad de base de datos. Si la portabilidad no es una preocupación, LTREE puede adoptarse como capa de optimización sobre la closure table.

4.  **Base de datos de Grafos (BD separada)**: Rechazada. La sobrecarga de consistencia de doble escritura y la complejidad operativa superan los beneficios para datos de inquilinos con estructura de árbol.

---

## 6. Estrategia de Migración

1.  **Fase 1**: Crear tablas `tenant_types`, `tenants`, `tenant_closure` y `tenant_edges` junto a la tabla `organizations` existente.
2.  **Fase 2**: Migrar filas existentes de `Organization` a `tenants` con `type_code = 'ENTERPRISE'` y `root_tenant_id = self`.
3.  **Fase 3**: Crear nueva entidad `User` con `home_tenant_id` reemplazando `Subject.OrganizationId`. Dual-write durante la transición.
4.  **Fase 4**: Eliminar tabla `organizations`, renombrar `tenants`, habilitar triggers de closure para nuevas operaciones jerárquicas.
5.  **Rollback**: Mantener `organizations` como una vista sobre `tenants` durante 2 ciclos de release.
