# ADR-0038: Administración Delegada con Alcances Temporales

*   **Estado:** Propuestao
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

El SaaS empresarial B2B requiere delegación administrativa que sea:

1. **Temporal**: Un agente de soporte del equipo de plataforma necesita acceso de administrador por 48 horas para resolver un incidente.
2. **Acotada por tipo de recurso**: Un operador puede gestionar usuarios pero no modificar políticas ni ver registros de auditoría.
3. **Limitada en profundidad**: Un gerente de sucursal puede administrar su sucursal y los departamentos debajo de ella, pero no sucursales hermanas.
4. **Revocable en cascada**: Revocar a un administrador de nivel medio debe revocar automáticamente todas las delegaciones que concedió.
5. **Auditable**: Cada delegación (concesión, uso, revocación) debe registrarse como un evento inmutable.

El UMS actual no tiene modelo de delegación. Todos los usuarios con acceso de administrador tienen privilegios planos, permanentes y sin alcance definido.

---

## 2. Decisión Arquitectónica

Implementaremos un aggregate **DelegationGrant** que forma un grafo acíclico dirigido (DAG) de autoridades delegadas enraizado en cada MasterUser. Cada acción administrativa se valida contra la cadena de delegación del actor, no solo contra su rol.

### 2.1. Entidad DelegationGrant

```sql
CREATE TABLE delegation_grants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    root_tenant_id UUID NOT NULL REFERENCES tenants(id),

    granter_user_id UUID NOT NULL REFERENCES users(id),
    grantee_user_id UUID NOT NULL REFERENCES users(id),

    scope_tenant_id UUID NOT NULL REFERENCES tenants(id),
    max_hierarchy_depth INT NOT NULL DEFAULT 0
        CHECK (max_hierarchy_depth BETWEEN 0 AND 5),
    allowed_resource_types TEXT[] NOT NULL DEFAULT '{}',
    allowed_actions TEXT[] NOT NULL DEFAULT '{}',
    max_role VARCHAR(32) NOT NULL,

    parent_grant_id UUID REFERENCES delegation_grants(id),

    can_delegate BOOLEAN NOT NULL DEFAULT false,
    status VARCHAR(16) NOT NULL DEFAULT 'ACTIVE'
        CHECK (status IN ('ACTIVE', 'REVOKED', 'EXPIRED', 'SUSPENDED')),
    expires_at TIMESTAMPTZ,
    revoked_at TIMESTAMPTZ,
    revoked_by_user_id UUID REFERENCES users(id),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT grantee_not_granter CHECK (granter_user_id != grantee_user_id)
) PARTITION BY LIST (root_tenant_id);

CREATE INDEX idx_delegation_grantee ON delegation_grants (grantee_user_id, status);
CREATE INDEX idx_delegation_granter ON delegation_grants (granter_user_id, status);
CREATE INDEX idx_delegation_parent ON delegation_grants (parent_grant_id)
    WHERE parent_grant_id IS NOT NULL;
```

### 2.2. Reglas de Creación de Concesiones

**Regla D1 — No Expansión**: El alcance de la concesión debe ser un subconjunto del alcance efectivo del concedente.

**Regla D2 — Dominancia Jerárquica**: El concedente debe tener un `taxonomy_rank` estárictamente menor que el rango del inquilino gestionado por el concesionario.

**Regla D3 — Límite de Profundidad**: La profundidad máxima de la cadena de delegación es 5.

**Regla D4 — Límite Temporal**: Todas las delegaciones deben tener un `expires_at` a menos que las conceda un administrador de tipo `ROOT`. El TTL máximo por defecto para administradores delegados es de 90 días.

### 2.3. Resolución de Alcance Efectivo

El resolvedor calcula el alcance efectivo de un usuario recorriendo sus `DelegationGrant` activos y computando la intersección de todos ellos. Para usuarios `MASTER`, el alcance se determina por sus `TenantAssignments`.

### 2.4. Cumplimiento Temporal

Un trabajo en segundo plano (cada 5 minutos) expira las concesiones cuyo `expires_at` haya vencido, cambiando su estado a `EXPIRED` y emitiendo un evento `DelegationExpired`.

### 2.5. Eventos de Auditoría de Delegación

Cada evento del ciclo de vida de una delegación emite un evento de dominio: `DelegationGranted`, `DelegationRevoked`, `DelegationExpired`, `DelegationCascadeRevoked`, `DelegationUsed`.

---

## 3. Consecuencias

### Positivas

*   **Temporal, acotada, revocable**: Semántica completa de delegación empresarial sin concesiones de privilegios permanentes.
*   **Transparencia de cadena**: Trazabilidad completa de quién delegó qué a quién.
*   **Defensa en profundidad**: Incluso si una cuenta de administrador delegado es comprometida, el radio de explosión estáá limitado a su alcance de concesión explícito.
*   **Autoservicio**: Los administradores de inquilinos pueden delegar autoridad limitada sin intervención del equipo de plataforma.

### Negativas

*   **Sobrecarga de validación**: Cada acción de un administrador delegado requiere recorrer la cadena. Mitigación: Cachear alcance efectivo resuelto por usuario con TTL 60s.
*   **Complejidad de intersección**: Un usuario con múltiples concesiones tiene un alcance efectivo que es la intersección de todas. Mitigación: La UI de la consola muestra el alcance efectivo explícitamente.
*   **Concesiones huérfanas**: Si un concedente es desactivado, sus concesiones activas deben revisarse. Mitigación: Trabajo en segundo plano que marca concesiones de usuarios desactivados.
