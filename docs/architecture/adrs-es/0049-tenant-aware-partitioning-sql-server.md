# ADR-0049: Estrategia de Particionamiento Consciente de Tenants para SQL Server 2022

* **Estado:** Aceptado
* **Fecha:** 2026-05-14
* **Autores:** Equipo de Arquitectura
* **Reemplaza:** ADR-0037 (versión PostgreSQL)
* **Relacionado:** ADR-0010 (Multi-Tenancy Básica), ADR-0041 (Motor de BD), ADR-0048 (Modelo Jerárquico)

---

## 1. Contexto y Problema

A medida que UMS adopta tenants jerárquicos (ADR-0048), emergen desafíos de escalabilidad:

1. **Recuperación de tenant único**: Restaurar un tenant raíz requiere escanear tablas compartidas
2. **Vecino ruidoso**: Tenants de alto volumen degradan rendimiento para otros
3. **Mantenimiento de índices**: Estadísticas deben escanear toda la tabla
4. **Sharding futuro**: Necesidad de migrar particiones de tenant raíz a instancias separadas

SQL Server 2022 soporta **particionamiento nativo LIST por `root_tenant_id`**, habilitando aislamiento físico por tenant sin separación de esquema.

---

## 2. Decisión Arquitectónica

Adoptar **particionamiento LIST por `root_tenant_id`** para tablas centrales. Cada tenant raíz y su subárbol reside en una sola partición.

### 2.1 Esquema de Particionamiento (SQL Server)

```sql
-- Tabla maestra (plantilla)
CREATE TABLE [ums_identity].[TENANT] (
 tenant_id UNIQUEIDENTIFIER NOT NULL,
 root_tenant_id UNIQUEIDENTIFIER NOT NULL,
 name NVARCHAR(255) NOT NULL,
 type_code VARCHAR(32) NOT NULL,
 status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE',
 parent_tenant_id UNIQUEIDENTIFIER NULL,

 PRIMARY KEY CLUSTERED (root_tenant_id, tenant_id) -- Clave de partición primero
) ON ps_tenant_by_root(root_tenant_id);

-- Función de partición: Lista de IDs de tenant raíz
CREATE PARTITION FUNCTION pf_tenant_by_root (UNIQUEIDENTIFIER)
 AS PARTITION RANGE LEFT
 FOR VALUES (
 '11111111-1111-1111-1111-111111111111',
 '22222222-2222-2222-2222-222222222222'
 );

-- Esquema de partición: Mapear particiones a grupos de archivos
CREATE PARTITION SCHEME ps_tenant_by_root
 AS PARTITION pf_tenant_by_root
 TO (fg_tenant_001, fg_tenant_002, [PRIMARY]);
```

### 2.2 Tablas Centrales a Particionar

| Tabla | Clave de Partición |
|-------|-------------------|
| `TENANT` | `root_tenant_id` |
| `USER_ACCOUNT` | `root_tenant_id` |
| `AUTHORIZATION_TEMPLATE` | `root_tenant_id` |
| `PROFILE_PERMISSION` | `root_tenant_id` |
| `AUDIT_RECORD` | `root_tenant_id` |
| `APPROVAL_REQUEST` | `root_tenant_id` |

### 2.3 Gestión Dinámica de Particiones (C#)

Crear automáticamente particiones para nuevos tenants raíz:

```csharp
public class PartitionManager
{
 private readonly DbContext _dbContext;

 public async Task EnsurePartitionAsync(Guid rootTenantId)
 {
 var exists = await _dbContext.Database
 .SqlQueryRaw<bool>(
 @"SELECT CASE WHEN EXISTS (
 SELECT 1 FROM sys.partitions
 WHERE partition_number > 1
 AND object_id = OBJECT_ID('[ums_identity].[TENANT]')
 ) THEN 1 ELSE 0 END"
 )
 .FirstAsync();

 if (!exists)
 {
 // Crear grupo de archivos
 await _dbContext.Database.ExecuteSqlRawAsync(
 $"ALTER DATABASE [ums] ADD FILEGROUP fg_tenant_{rootTenantId:N}");

 // Alterar función de partición
 await _dbContext.Database.ExecuteSqlRawAsync(
 $"ALTER PARTITION FUNCTION pf_tenant_by_root() SPLIT RANGE ('{rootTenantId}')");
 }
 }
}
```

### 2.4 Optimización de Consultas

Siempre incluir `root_tenant_id` en cláusula WHERE para pruning de partición:

```csharp
// BIEN: Pruning de partición habilitado
var tenants = await _dbContext.Tenants
 .Where(t => t.RootTenantId == rootTenantId)
 .ToListAsync();

// MALO: Sin pruning (escanea todas las particiones)
var all = await _dbContext.Tenants.ToListAsync();
```

---

## 3. Integración con RLS

Layer 1 (Primary): EF Core aplica filtros conscientes de partición

Layer 2 (Failsafe): SQL Server RLS previene fugas cross-partition

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

## 4. Camino de Migración: Plano → Jerárquico + Particionado

**Fase 1:** Crear infraestructura de particionamiento (sin movimiento de datos)
**Fase 2:** Reconstruir tablas existentes en esquema de partición
**Fase 3:** Verificar pruning de partición en producción

---

## 5. Futuro: Consultas Cross-Shard (Fase 3)

Si un tenant raíz excede capacidad de instancia única:

1. Copiar partición a base de datos shard
2. Actualizar enrutamiento de aplicación
3. Usar Linked Server para consultas que necesitan datos de múltiples shards

---

## 6. Consecuencias

### Positivas
- Recuperación rápida de tenant (DROP partición en vez de DELETE)
- Aislamiento de vecino ruidoso
- Optimización de índices por partición
- Rendimiento mejorado vía pruning de partición
- Listo para sharding futuro

### Negativas
- Clave de partición en todas las consultas
- Restricciones FK limitadas entre particiones (validar en app)
- Complejidad adicional en mantenimiento de particiones

---

## 7. Referencias

- ADR-0048: Modelo Jerárquico Multi-Tenant para SQL Server 2022
- ADR-0041: Estrategia Autorizada de Motor de Base de Datos
- [Guía de Particionamiento SQL Server](https://learn.microsoft.com/es-es/sql/relational-databases/partitions/partitioned-tables-and-indexes)

