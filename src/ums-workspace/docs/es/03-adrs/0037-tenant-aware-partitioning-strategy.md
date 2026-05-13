# ADR-0037: Estrategia de Particionamiento por Inquilino

*   **Estado:** Propuesto
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

El UMS actual usa un modelo de esquema compartido con RLS para aislamiento de inquilinos (ADR-0010). A medida que el sistema crece para soportar inquilinos jerárquicos (ADR-0034), surgen tres desafíos de escalabilidad:

1. **Recuperación de datos por inquilino**: Restaurar datos para un grupo empresarial completo (root tenant + todos los descendientes) requiere escanear toda la tabla compartida.
2. **Vecino ruidoso**: Un root tenant de alto volumen (ej. 10M usuarios) degrada el rendimiento de consultas para inquilinos más pequeños que comparten la misma tabla física.
3. **Vacío y mantenimiento**: El autovacuum de PostgreSQL debe escanear la tabla completa incluso si solo un root tenant tiene alta actividad de escritura.
4. **Fragmentación futura**: La arquitectura debe soportar la migración de particiones completas de root tenant a instancias de base de datos separadas sin tiempo de inactividad.

El modelo híbrido agrupado (ADR-0010) asumía inquilinos planos de tamaño aproximadamente igual. El modelo jerárquico invalida esta suposición — los root tenants pueden variar desde 1K hasta 10M de usuarios.

---

## 2. Decisión Arquitectónica

Adoptaremos **particionamiento declarativo LIST por `root_tenant_id`** para todas las tablas core (tenants, users, policies, policy_bindings, audit_log, delegation_grants). Cada root tenant (grupo empresarial) y todo su subárbol residen en una sola partición.

### 2.1. Estrategia de Particionamiento

```sql
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
    PRIMARY KEY (id, root_tenant_id)
) PARTITION BY LIST (root_tenant_id);

CREATE TABLE tenants_root_001 PARTITION OF tenants
    FOR VALUES IN ('00000000-0000-0000-0000-000000000001');

CREATE TABLE tenants_root_002 PARTITION OF tenants
    FOR VALUES IN ('00000000-0000-0000-0000-000000000002');
```

### 2.2. Auto-Provisión de Particiones

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

### 2.3. Tablas a Particionar

| Tabla | Clave de Partición | Sub-Particionamiento | Particiones Estimadas |
|---|---|---|---|
| `tenants` | `root_tenant_id` | Ninguno | 1 por root tenant |
| `users` | `root_tenant_id` | `LIST (user_type)` | 1 por root tenant |
| `policies` | `root_tenant_id` | `RANGE (version)` | 1 por root tenant |
| `policy_bindings` | `root_tenant_id` | Ninguno | 1 por root tenant |
| `audit_log` | `root_tenant_id` | `RANGE (created_at)` sub-particiones mensuales | 1 por root tenant + mensual |
| `delegation_grants` | `root_tenant_id` | `LIST (status)` | 1 por root tenant |

### 2.4. Particionamiento de Closure Table

La closure table debe usar el mismo esquema de particionamiento. Esto requiere almacenar `root_tenant_id` en la closure table (desnormalizado desde tenants).

```sql
CREATE TABLE tenant_closure (
    ancestor_id UUID NOT NULL,
    descendant_id UUID NOT NULL,
    depth INT NOT NULL CHECK (depth >= 0),
    root_tenant_id UUID NOT NULL,
    PRIMARY KEY (ancestor_id, descendant_id, root_tenant_id)
) PARTITION BY LIST (root_tenant_id);

CREATE TABLE tenant_closure_root_001 PARTITION OF tenant_closure
    FOR VALUES IN ('00000000-0000-0000-0000-000000000001');
```

### 2.5. Fragmentación Futura (Sharding)

Cuando un root tenant excede la capacidad, migrar sus particiones a una instancia PostgreSQL dedicada:

```
Paso 1: CREATE TABLE tenants_root_001 ON shard_db_01 ( ... );
Paso 2: INSERT INTO shard_db_01.tenants_root_001 SELECT * FROM main.tenants_root_001;
Paso 3: DETACH PARTITION tenants_root_001 FROM tenants;
Paso 4: Configurar TenantResolutionService para rutear root_tenant_id -> shard_db_01;
Paso 5: Implementar consulta distribuida mediante FDW para lecturas cross-shard.
```

---

## 3. Consecuencias

### Positivas

*   **DROP rápido por inquilino**: `DROP TABLE tenants_root_001` es instantáneo, comparado con `DELETE FROM tenants WHERE root_tenant_id = X`.
*   **Vacío independiente**: Cada partición puede tener su propia configuración de autovacuum.
*   **Camino a sharding**: Las particiones existen como tablas físicas independientes; migrar a otra base de datos es una operación `CREATE TABLE ... ON shard` + `DETACH PARTITION`.
*   **Consultas paralelas**: PostgreSQL puede escanear particiones en paralelo para consultas administrativas cross-tenant.

### Negativas

*   **Clave de partición en todas partes**: Cada consulta debe incluir `root_tenant_id` en WHERE o JOIN. Si se omite, PostgreSQL debe escanear todas las particiones.
*   **FK entre particiones**: NO es posible en particionamiento declarativo de PostgreSQL. La integridad referencial cross-partición debe ser aplicada por la capa de aplicación.
*   **Sobrecarga de gestión**: Crear una partición para cada nuevo root tenant añade complejidad operativa. Mitigación: Automatizado mediante `PartitionManager`.
*   **Desnormalización en closure table**: Almacenar `root_tenant_id` en `tenant_closure` es redundante pero necesario para particionamiento.
