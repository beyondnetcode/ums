# ADR-0035: Motor de Herencia de Políticas Híbrido (Mandatory + Default + Opt-In)

*   **Estado:** Propuestao
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

En un sistema multi-inquilino jerárquico, las políticas deben fluir desde los inquilinos padres hacia sus hijos respetando la autonomía local. Cuatro escenarios de herencia surgen comúnmente en SaaS empresarial B2B:

1.  **Mandatos globales**: Políticas regulatorias/de cumplimiento (ej. "MFA obligatorio para todas las acciones de administración") deben aplicarse a todos los sub-inquilinos sin posibilidad de anulación.
2.  **Valores por defecto empresariales**: Políticas a nivel de grupo (ej. "Tiempo de sesión = 30 min") deberían aplicarse por defecto pero permitir anulaciones por subsidiaria.
3.  **Adopción opt-in**: Políticas a nivel de división deberían aplicarse solo a sucursales que las adopten explícitamente.
4.  **Soberanía local**: Una subsidiaria puede necesitar definir políticas completamente locales no relacionadas con ninguna política padre.

Un modelo binario "heredado/no heredado" es insuficiente para esté espectro.

---

## 2. Decisión Arquitectónica

Implementaremos un **motor de herencia de políticas de cuatro modos** con resolución jerárquica mediante recorrido de la closure table. El motor resuelve la política efectiva para un inquilino dado recorriendo la cadena de ancestáros y seleccionando la versión aplicable más específica.

### 2.1. Entidad Policy

```sql
CREATE TABLE policies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(128) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    effect VARCHAR(8) NOT NULL CHECK (effect IN ('ALLOW', 'DENY')),
    scope VARCHAR(16) NOT NULL DEFAULT 'TENANT_ONLY'
        CHECK (scope IN ('GLOBAL', 'TENANT_ONLY', 'SUBTREE')),
    inheritance_mode VARCHAR(16) NOT NULL DEFAULT 'DEFAULT'
        CHECK (inheritance_mode IN ('NONE', 'MANDATORY', 'DEFAULT', 'OPT_IN')),
    is_system BOOLEAN NOT NULL DEFAULT false,
    conditions JSONB,
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (code, version)
);
```

### 2.2. Policy Bindings (Relación Inquilino-Política)

```sql
CREATE TABLE policy_bindings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    policy_id UUID NOT NULL REFERENCES policies(id),
    priority INT NOT NULL DEFAULT 100,
    is_active BOOLEAN NOT NULL DEFAULT true,
    overridden_binding_id UUID REFERENCES policy_bindings(id),
    conditions_override JSONB,
    effective_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (tenant_id, policy_id),
    CONSTRAINT valid_override CHECK (
        overridden_binding_id IS NULL OR
        EXISTS (
            SELECT 1 FROM policy_bindings pb2
            WHERE pb2.id = overridden_binding_id AND pb2.tenant_id != tenant_id
)
)
);

CREATE INDEX idx_policy_bindings_tenant ON policy_bindings (tenant_id, is_active);
CREATE INDEX idx_policy_bindings_policy ON policy_bindings (policy_id);
```

### 2.3. Semántica de Modos de Herencia

| Modo | ¿Propaga a Hijos? | ¿Anulable? | Resolución |
|---|---|---|---|
| `NONE` | No | N/D | Aplica solo al inquilino vinculado. |
| `MANDATORY` | Sí, forzado | No | Todos los descendientes heredan incondicionalmente. Intentos de violación son bloqueados. |
| `DEFAULT` | Sí, aplicado | Sí | Los hijos heredan a menos que declaren un binding de anulación explícito. |
| `OPT_IN` | No, ofrecido | N/D | Los hijos ven la política pero estáá inactiva hasta que la vinculen explícitamente. | ### 2.4. Algoritmo de Resolución de Políticas

```csharp
public class PolicyResolver
{
    public async Task<ResolvedPolicy> ResolveAsync(Guid tenantId, string policyCode)
    {
        var ancestáors = await dbContext.TenantClosure
            .Where(tc => tc.DescendantId == tenantId)
            .OrderByDescending(tc => tc.Depth)
            .Join(dbContext.Tenants, tc => tc.AncestáorId, t => t.Id, (tc, t) => new { t.Id, tc.Depth })
            .ToListAsync();

        var ancestáorIds = ancestáors.Select(a => a.Id).ToList();
        var bindings = await dbContext.PolicyBindings
            .Where(pb => ancestáorIds.Contains(pb.TenantId)
                && pb.Policy.Code == policyCode
                && pb.IsActive
                && pb.EffectiveAt <= DateTime.UtcNow
                && (pb.ExpiresAt == null || pb.ExpiresAt > DateTime.UtcNow))
            .Include(pb => pb.Policy)
            .OrderByDescending(pb => ancestáorIds.IndexOf(pb.TenantId))
            .ThenBy(pb => pb.Priority)
            .ToListAsync();

        ResolvedPolicy result = null;
        foreach (var binding in bindings)
        {
            if (result == null)
            {
                result = MapToResolved(binding);
                continue;
            }

            if (binding.Policy.InheritanceMode == InheritanceMode.MANDATORY)
            {
                result = MapToResolved(binding);
                break;
            }

            if (binding.OverriddenBindingId != null || binding.Policy.InheritanceMode == InheritanceMode.DEFAULT)
            {
                result = MapToResolved(binding);
            }
        }

        return result  throw new PolicyNotFoundException(policyCode, tenantId);
    }

    public AuthorizationDecision Evaluate(ResolvedPolicy policy, EvaluationContext context)
    {
        if (policy.Conditions != null)
        {
            bool conditionsMet = EvaluateConditions(policy.Conditions, context);
            if (!conditionsMet && policy.InheritanceMode == InheritanceMode.MANDATORY)
                return AuthorizationDecision.DenyWithReason("Condiciones de política obligatoria no cumplidas");
            if (!conditionsMet)
                return AuthorizationDecision.NotApplicable;
        }
        return policy.Effect == "ALLOW" ? AuthorizationDecision.Allow : AuthorizationDecision.Deny;
    }
}
```

### 2.5. Reglas de Validación de Anulaciones (Overrides)

Un binding de anulación debe satisfacer:
1. El inquilino que anula debe ser descendiente del inquilino del binding anulado (validado mediante closure table).
2. La política anulada debe tener `inheritance_mode = DEFAULT` (las políticas MANDATORY no pueden anularse).
3. La anulación solo puede restáringir, no expandir alcance — una anulación puede agregar DENY sobre ALLOW pero no ALLOW sobre un MANDATORY DENY.
4. La anulación debe ser aprobada por un administrador con `taxonomy_rank` igual o superior al del creador del binding anulado.

---

## 3. Consecuencias

### Positivas

*   **Granularidad de nivel empresarial**: Cuatro modos de herencia cubren todos los escenarios B2B SaaS desde mandatos de cumplimiento normativo hasta compartición opt-in.
*   **Resolución determinística**: El recorrido de la cadena de ancestáros con prioridad explícita produce resultados predecibles y depurables.
*   **Preparado para ABAC**: La columna `conditions` JSONB permite condiciones basadas en atributos (tiempo, IP, geo, dispositivo) sin cambios de esquema.
*   **Anulaciones auditables**: `overridden_binding_id` crea una cadena vinculada que documenta exactamente qué política anuló a qué política padre.

### Negativas

*   **Complejidad**: Cuatro modos de herencia incrementan la carga cognitiva para los administradores de inquilinos. Mitigación: La UI de la consola debe visualizar las cadenas de herencia como un árbol.
*   **Rendimiento**: La resolución de políticas requiere una cadena de JOINs a través de closure + bindings. Mitigación: Cachear políticas resueltas por `(tenant_id, user_type)` con TTL de 300s, invalidado en mutaciones de bindings.
*   **Validación de anulaciones**: Forzar restricciónes de anulación requiere verificaciones recursivas contra los permisos del creador ancestáro.
