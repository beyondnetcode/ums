# ADR-0036: Anti-Escalada de Privilegios mediante Validación Recursiva de Alcance

*   **Estado:** Propuestao
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

En un sistema multi-inquilino jerárquico con administración delegada, la escalada de privilegios es el riesgo de seguridad principal. Un atacante que comprometa a un administrador delegado de bajo nivel no debería poder:

1. Acceder a recursos en un inquilino del mismo nivel o superior.
2. Conceder permisos que no posee a otros usuarios.
3. Delegar poderes administrativos más allá del alcance que recibió.
4. Modificar políticas que restáringen su propio inquilino o sus ancestáros.
5. Crear usuarios en inquilinos fuera de su ámbito gestionado.

Las verificaciones RBAC tradicionales ("¿tiene el usuario el rol X?") son insuficientes porque no consideran la **relación jerárquica** entre el alcance del actor y el recurso objetivo.

---

## 2. Decisión Arquitectónica

Implementaremos un **Pipeline de Validación Recursiva de Alcance** que verifica cada operación administrativa contra cinco invariantes. Este pipeline se ejecuta como una cadena de middlewares .NET, cada uno aplicando una dimensión del contrato de seguridad.

### 2.1. Las Cinco Invariantes

| # | Invariante | Regla | Validación |
|---|---|---|---|
| I1 | **Dominancia Jerárquica** | El `taxonomy_rank` del actor debe ser estárictamente menor que el del objetivo | 1 consulta DB a `tenant_types` |
| I2 | **Contención de Alcance** | El inquilino objetivo debe estáar dentro del subárbol `managed_tenants[]` del actor | 1 JOIN en `tenant_closure` |
| I3 | **No Expansión de Concesión** | El actor no puede delegar poderes que no posee | Recorrido de cadena de `DelegationGrant` |
| I4 | **Restáricción de Políticas** | El actor no puede modificar/eliminar una política MANDATORY de nivel superior | Verificación de `inheritance_mode` |
| I5 | **Integridad de Revocación** | Si cualquier concesión en la cadena se revoca, todas las concesiones derivadas son inválidas | Verificación recursiva de estado de `DelegationGrant` | ### 2.2. Pipeline de Middleware de Validación de Alcance

```csharp
app.UseMiddleware<TenantResolutionMiddleware>()
   .UseMiddleware<HierarchyDominanceMiddleware>()     // I1
   .UseMiddleware<ScopeContainmentMiddleware>()        // I2
   .UseMiddleware<DelegationIntegrityMiddleware>()     // I3 + I5
   .UseMiddleware<PolicyConstraintMiddleware>()        // I4
   .UseMiddleware<AuthorizationDecisionMiddleware>();  // Decisión final
```

### 2.3. Entidad DelegationGrant

```csharp
public class DelegationGrant : Entity
{
    public Guid GranterUserId { get; private set; }
    public User Granter { get; private set; } = null!;
    public Guid GranteeUserId { get; private set; }
    public User Grantee { get; private set; } = null!;

    public Guid RootTenantScopeId { get; private set; }
    public int MaxHierarchyDepth { get; private set; }
    public string[] AllowedResourceTypes { get; private set; }
    public string[] AllowedActions { get; private set; }
    public ManagedRole MaxRole { get; private set; }

    public Guid? ParentGrantId { get; private set; }
    public DelegationGrant? ParentGrant { get; private set; }

    public bool CanDelegate { get; private set; }
    public DelegationGrantStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedByUserId { get; private set; }

    public enum DelegationGrantStatus { ACTIVE, REVOKED, EXPIRED, SUSPENDED }

    public Result<DelegationGrant> Create(
        User granter, User grantee, DelegationScope scope, bool canDelegate, Guid? parentGrantId)
    {
        // I3: Validar que granter posee el alcance que estáá delegando
    }

    public Result Revoke(Guid revokedByUserId)
    {
        Status = DelegationGrantStatus.REVOKED;
        RevokedAt = DateTime.UtcNow;
        RevokedByUserId = revokedByUserId;
        AddDomainEvent(new DelegationRevokedEvent(Id, revokedByUserId, "Cascade from parent revocation"));
    }
}
```

### 2.4. Invariante I1: Dominancia Jerárquica

```csharp
public class HierarchyDominanceMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var actorTenant = context.Items["ActorTenant"] as Tenant;
        var targetTenant = context.Items["TargetTenant"] as Tenant;

        if (targetTenant != null)
        {
            var actorRank = await dbContext.TenantTypes
                .Where(tt => tt.Code == actorTenant.TypeCode)
                .Select(tt => tt.TaxonomyRank)
                .FirstAsync();

            var targetRank = await dbContext.TenantTypes
                .Where(tt => tt.Code == targetTenant.TypeCode)
                .Select(tt => tt.TaxonomyRank)
                .FirstAsync();

            if (actorRank >= targetRank)
            {
                context.Items["AuthorizationResult"] = AuthorizationResult.Deny(
                    "I1", $"El rango del actor {actorRank} no es menor que el rango del objetivo {targetRank}");
                await WriteDeniedResponse(context);
                return;
            }
        }

        await next(context);
    }
}
```

### 2.5. Invariante I2: Contención de Alcance

```csharp
public class ScopeContainmentMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var actorUser = context.Items["ActorUser"] as User;
        var targetTenantId = (Guid?)context.Items["TargetTenantId"];

        if (targetTenantId.HasValue && actorUser.UserType == UserType.DELEGATED_ADMIN)
        {
            bool isWithinScope = await dbContext.TenantClosure
                .AnyAsync(tc =>
                    tc.DescendantId == targetTenantId.Value
                    && dbContext.TenantAssignments
                        .Where(ta => ta.UserId == actorUser.Id && ta.IsActive)
                        .Select(ta => ta.TenantId)
                        .Contains(tc.AncestáorId));

            if (!isWithinScope)
            {
                context.Items["AuthorizationResult"] = AuthorizationResult.Deny(
                    "I2", $"El inquilino objetivo {targetTenantId} estáá fuera del alcance gestionado del actor");
                await WriteDeniedResponse(context);
                return;
            }
        }

        await next(context);
    }
}
```

### 2.6. Invariante I5: Revocación en Cascada

```sql
CREATE OR REPLACE FUNCTION fn_cascade_delegation_revocation()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status = 'REVOKED' AND OLD.status != 'REVOKED' THEN
        UPDATE delegation_grants
        SET status = 'REVOKED',
            revoked_at = NEW.revoked_at,
            revoked_by_user_id = NEW.revoked_by_user_id
        WHERE parent_grant_id = NEW.id
          AND status = 'ACTIVE';

        INSERT INTO audit_log (root_tenant_id, actor_user_id, action_type, resource_type, resource_id, metadata)
        SELECT t.root_tenant_id, NEW.revoked_by_user_id, 'delegation.cascade_revoked', 'delegation_grant',
               dg.id, jsonb_build_object('parent_grant_id', NEW.id, 'reason', 'Cascade from parent revocation')
        FROM delegation_grants dg
        JOIN tenants t ON t.id = dg.root_tenant_scope_id
        WHERE dg.parent_grant_id = NEW.id AND dg.status = 'REVOKED';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_cascade_delegation_revocation
    AFTER UPDATE ON delegation_grants
    FOR EACH ROW
    WHEN (NEW.status = 'REVOKED' AND OLD.status != 'REVOKED')
    EXECUTE FUNCTION fn_cascade_delegation_revocation();
```

---

## 3. Consecuencias

### Positivas

*   **Defensa en profundidad**: Cinco invariantes independientemente testeables previenen puntos únicos de fallo en la lógica de seguridad.
*   **Validación de jerarquía O(1)**: El rango de taxonomía elimina recorridos recursivos del árbol para la verificación más frecuente (I1).
*   **Cadena de confianza explícita**: Cada delegación estáá vinculada a su padre, permitiendo trazabilidad completa y revocación en cascada.
*   **Aislamiento de middleware**: Cada invariante se ejecuta como middleware independiente, permitiendo deshabilitación selectiva por endpoint.

### Negativas

*   **Sobrecarga por petición**: 3-5 consultas DB por petición administrativa. Mitigación: Cachear rangos de taxonomía y alcances validados con TTL 60s.
*   **Costo de revocación en cascada**: Revocar una cadena de delegación profundamente anidada requiere actualizaciones recursivas. Mitigación: Limitar profundidad de delegación a 5; procesar por lotes con CTE.
*   **Riesgo de falsos positivos**: Invariantes demasiado estárictas podrían bloquear compartición legítima de datos cross-tenant. Mitigación: `tenant_edges` con `edge_type = 'data_sharing'` puede crear alcances de exclusión explícitos.
