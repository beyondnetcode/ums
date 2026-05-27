# FeatureFlagCriteria — Modelo de Criterios de Evaluación

> **Idioma:** [English](../../domain/configuration/feature-flag-criteria.md) | [Español](./feature-flag-criteria.md)

**Contexto Delimitado:** Configuración (`Ums.Domain.Configuration`)
**Agregado Propietario:** `FeatureFlag`
**Tipo de Entidad:** Entidad Propia (sin ciclo de vida independiente)
**Estado:** Producción

---

## 1. Introducción

`FeatureFlagCriteria` son los bloques fundamentales que determinan cuándo un `FeatureFlag` se considera activo para un contexto de evaluación específico. Cada criterio expresa una condición: un tipo específico de dato (por ejemplo, un tenant, un rol, un rango de fechas), un operador y un valor objetivo.

La colección de criterios es **opcional y dinámica**:

- Una bandera con una colección de criterios **vacía** está activa para todos los llamantes del sistema, independientemente del contexto.
- Una bandera con **uno o más** criterios está activa únicamente cuando el contexto de evaluación satisface las condiciones definidas.
- Los criterios pueden agregarse o eliminarse en cualquier momento mientras la bandera esté en estado `Inactive` o `Active`.

Los criterios no tienen estados propios de ciclo de vida. Existen mientras están vinculados a la bandera y se eliminan mediante el `RemoveFeatureFlagCriteriaCommand`.

---

## 2. Tipos de Criterio (CriteriaTypes)

| CriteriaType | Descripción | Tipo JSON del Value | Operadores Soportados | Ejemplo de Value |
|---|---|---|---|---|
| `TenantId` | Coincide por identificador de tenant | `string` (GUID) | `Equals`, `NotEquals` | `"a3f2e1d0-..."` |
| `BranchId` | Coincide por identificador de sucursal | `string` (GUID) | `Equals`, `NotEquals` | `"b7c9a2e1-..."` |
| `UserProfileId` | Coincide por identificador de perfil de usuario | `string` (GUID) | `Equals`, `NotEquals` | `"c1d4f5a2-..."` |
| `RoleCode` | Coincide por código de rol | `string` | `Equals`, `NotEquals`, `In` | `"ADMIN"` o `["ADMIN","MANAGER"]` |
| `Environment` | Coincide por ambiente de despliegue | `string` | `Equals`, `NotEquals`, `In` | `"Production"` o `["Staging","Production"]` |
| `DateRange` | Activo dentro de un rango de fechas UTC | `{ "from": "ISO8601", "to": "ISO8601" }` | `Between` | `{"from":"2026-06-01T00:00:00Z","to":"2026-09-01T00:00:00Z"}` |
| `PercentageHash` | Activa para un porcentaje determinístico de usuarios | `string` (entero 0–100) | `LessThanOrEqual` | `"30"` |
| `CustomRule` | Regla nombrada resuelta por un handler registrado | `string` (identificador de regla) | `Matches` | `"beta-tester-group"` |

**Notas sobre el Value JSON:**

- Para el operador `In`, el Value debe ser un array JSON de strings: `["VALOR_A","VALOR_B"]`.
- Para `DateRange`, la fecha de inicio debe ser estrictamente anterior a la de fin (INV-FF5).
- Para `PercentageHash`, el hash se calcula a partir de una identidad de usuario estable (por ejemplo, `UserProfileId`) en módulo 100; el criterio pasa cuando `hash % 100 <= umbral`.
- Para `CustomRule`, la capa de infraestructura resuelve el nombre de la regla a un evaluador concreto registrado en el contenedor de dependencias.

---

## 3. Reglas de Combinación

La colección de criterios sigue un modelo booleano de dos niveles:

| Nivel | Regla |
|---|---|
| Dentro del mismo `CriteriaType` | **OR** — la condición pasa si cualquier criterio individual de ese tipo coincide |
| Entre diferentes grupos de `CriteriaType` | **AND** — todos los grupos de tipos deben pasar para que el resultado global sea `true` |

**Ejemplo:** una bandera tiene:
- `TenantId Equals T1`
- `TenantId Equals T2`
- `RoleCode Equals ADMIN`

Lógica de evaluación: `(TenantId == T1 OR TenantId == T2) AND (RoleCode == ADMIN)`

Esto significa que la bandera está activa únicamente para los tenants T1 o T2, **y** solo cuando el llamante tiene el rol ADMIN.

---

## 4. Postura Segura — Dato Faltante en el Contexto

Si el contexto de evaluación no provee un valor para un `CriteriaType` requerido por algún criterio, la evaluación completa retorna **`false`**.

**Fundamento:** Es más seguro denegar el acceso a la funcionalidad que concederlo con información incompleta. Esto evita activaciones inadvertidas cuando el contexto está parcialmente poblado (por ejemplo, un llamante anónimo o una solicitud entre servicios sin contexto de tenant).

| Escenario | Resultado |
|---|---|
| El contexto provee todos los campos requeridos | Evaluar normalmente |
| El contexto omite un campo requerido | `false` (postura segura) |
| La colección de criterios está vacía | `true` (activa para todos) |
| La bandera está Archivada | Evaluación rechazada por invariante |

---

## 5. Algoritmo de Evaluación (Pseudocódigo)

```
función Evaluate(flag: FeatureFlag, context: EvaluationContext) -> bool:

    si flag.Criteria está vacío:
        retornar true

    grupos = AgruparPor(flag.Criteria, c => c.CriteriaType)

    para cada grupo en grupos:
        valorContexto = context.GetValue(grupo.CriteriaType)

        si valorContexto es null o está ausente:
            retornar false   // postura segura

        resultadoGrupo = false

        para cada criterio en grupo:
            si AplicarOperador(criterio.Operator, valorContexto, criterio.Value):
                resultadoGrupo = true
                romper       // OR dentro del grupo — cortocircuito

        si resultadoGrupo es false:
            retornar false   // AND entre grupos — cortocircuito

    retornar true
```

El puerto `IFeatureFlagEvaluator` expone este algoritmo. Su implementación reside en la capa de infraestructura y se inyecta en el handler de aplicación.

```csharp
public interface IFeatureFlagEvaluator
{
    bool Evaluate(FeatureFlag flag, EvaluationContext context);
}
```

---

## 6. Escenarios de Evaluación de Extremo a Extremo

### Escenario 1 — Bandera sin criterios (activa para todos)

**Configuración:** La bandera `NEW_DASHBOARD` no tiene criterios.
**Contexto:** Cualquier llamante.
**Resultado:** `true` — la bandera está activa para todos en el sistema.

---

### Escenario 2 — Criterio único de tenant

**Configuración:** La bandera `BETA_EXPORT` tiene un criterio: `TenantId Equals acme-corp-id`.
**Contexto A:** `{ TenantId: "acme-corp-id" }` → `true`
**Contexto B:** `{ TenantId: "other-tenant-id" }` → `false`
**Contexto C:** `{ TenantId: null }` → `false` (postura segura)

---

### Escenario 3 — Combinación AND de múltiples tipos

**Configuración:** La bandera `ADVANCED_REPORTS` tiene:
- `TenantId Equals acme-corp-id`
- `RoleCode In ["ADMIN", "MANAGER"]`

**Contexto A:** `{ TenantId: "acme-corp-id", RoleCode: "ADMIN" }` → `true`
**Contexto B:** `{ TenantId: "acme-corp-id", RoleCode: "VIEWER" }` → `false` (rol no está en la lista)
**Contexto C:** `{ TenantId: "other-tenant-id", RoleCode: "ADMIN" }` → `false` (tenant incorrecto)
**Contexto D:** `{ TenantId: "acme-corp-id" }` (RoleCode ausente) → `false` (postura segura)

---

### Escenario 4 — Rango de fechas (rollout temporal)

**Configuración:** La bandera `SEASONAL_PROMOTION` tiene un criterio: `DateRange Between { from: "2026-06-01", to: "2026-09-01" }`.
**Contexto A (evaluado el 2026-07-15):** `true` — dentro del rango.
**Contexto B (evaluado el 2026-10-01):** `false` — pasada la fecha de fin.
**Contexto C:** `{ CurrentDateUtc: null }` → `false` (postura segura).

---

### Escenario 5 — OR dentro del mismo tipo

**Configuración:** La bandera `MULTI_TENANT_PILOT` tiene:
- `TenantId Equals tenant-A`
- `TenantId Equals tenant-B`
- `Environment Equals Staging`

**Contexto A:** `{ TenantId: "tenant-A", Environment: "Staging" }` → `true` (tenant coincide con T-A o T-B; ambiente coincide)
**Contexto B:** `{ TenantId: "tenant-C", Environment: "Staging" }` → `false` (ningún tenant coincide)
**Contexto C:** `{ TenantId: "tenant-B", Environment: "Production" }` → `false` (ambiente no coincide)

---

## 7. Referencia

- Ver [`FeatureFlag`](./feature-flag.md) para el agregado propietario, ciclo de vida y comandos.
- Ver [ADR-0068](../../architecture/adrs/0068-feature-flag-system-scope.md) para la decisión arquitectónica que introdujo el modelo de criterios.
- `IFeatureFlagEvaluator` se registra en `Ums.Infrastructure.Configuration`.

---

**[Volver al Índice de Configuración](./index.md)** | **[FeatureFlag](./feature-flag.md)**
