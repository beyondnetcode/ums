# Auth Graph Schema — Vista General

> **Idioma:** [English](../../sdk/contracts/schema-overview.md) | Español

Este documento describe el **JSON Schema canónico** para el payload `AuthorizationGraph`. Es el contrato entre el servidor UMS y todos los runtimes del SDK (.NET, TypeScript, NestJS).

Para el ejemplo runtime completo y la semántica de autorización, ver [`auth-graph.md`](../../domain-es/identity/auth-graph.md). Esta página se enfoca en la **forma del schema, tipos y garantías de contrato**.

---

## 1. Ubicación

| Artefacto | Ruta |
|---|---|
| JSON Schema | `src/libs/sdk/contracts/auth-graph.schema.json` |
| Catálogo de códigos de error | `src/libs/sdk/contracts/error-codes.yaml` |
| Golden fixtures | `src/libs/sdk/contracts/fixtures/*.json` |
| Política de versionado | `src/libs/sdk/contracts/SCHEMA_VERSIONING.md` |

El schema es **JSON Schema 2020-12**.

---

## 2. Forma Top-Level

```jsonc
{
  "schemaVersion": "1.0.0",          // semver, requerido, gobernado por ADR-0074
  "context":            { ... },     // quién es el principal, en qué tenant/branch/role
  "authentication":     { ... },     // cómo se verificó la identidad
  "actions":            [ ... ],     // catálogo completo de acciones del SystemSuite
  "menuAccess":         [ ... ],     // árbol de navegación UI con permisos resueltos
  "domainPermissions":  [ ... ],     // recursos de dominio (Aggregate/Entity) con permisos resueltos
  "featureFlags":       [ ... ],     // flags evaluados al momento de autenticación
  "effectiveConfig":    { ... },     // parámetros resueltos del tenant
  "scopes":             [ ... ],     // strings "RESOURCE.ACTION" estilo OAuth2
  "generatedAt":        "...",       // ISO-8601 UTC
  "validUntil":         "..."        // ISO-8601 UTC; = generatedAt + sessionTimeoutMinutes
}
```

Todos los campos arriba son **requeridos** en v1.0.0. Agregar campos opcionales en el futuro es un bump MINOR según [ADR-0074](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md).

---

## 3. Referencia Campo por Campo

### 3.1 `schemaVersion` (string, requerido)

- Formato: semver `MAJOR.MINOR.PATCH`
- Pattern: `^\d+\.\d+\.\d+$`
- Valor inicial: `"1.0.0"`
- Gobierna la aceptación del SDK — ver [versioning.md](./versioning.md).

### 3.2 `context` (objeto, requerido)

```jsonc
"context": {
  "user":        { "id": "uuid", "email": "string", "username": "string",
                   "displayName": "string", "status": "ACTIVE|PENDING|BLOCKED" },
  "tenant":      { "id": "uuid", "code": "string", "name": "string",
                   "status": "ACTIVE|SUSPENDED|ARCHIVED" },
  "systemSuite": { "id": "uuid", "code": "string", "name": "string",
                   "status": "DRAFT|PUBLISHED|RETIRED" },
  "role":        { "id": "uuid", "code": "string", "name": "string",
                   "hierarchyLevel": 0, "parentRoleId": "uuid|null" },
  "profile":     { "id": "uuid", "scope": "OrgWide|BranchScoped", "isActive": true },
  "branch":      { "id": "uuid", "code": "string", "name": "string" } | null
}
```

Notas:
- `branch` es `null` cuando el scope del profile es `OrgWide`.
- Todos los valores enum son cerrados en v1.0.0 — agregar nuevos es un bump MINOR pero los SDKs tratan valores enum desconocidos como strings opacos (y logean un warning).

### 3.3 `authentication` (objeto, requerido)

```jsonc
"authentication": {
  "method":           "Local" | "IDP",
  "provider":         { "name": "string", "code": "string", "strategy": "string" } | null,
  "mfaRequired":      true|false,
  "issuedAt":         "ISO-8601",
  "sessionExpiresAt": "ISO-8601"
}
```

`provider` es `null` cuando `method = "Local"`. Cuando `method = "IDP"`, `provider` es requerido y `strategy` coincide con un valor de `IdpStrategyHint` de BC-A (ej. `AZURE_AD`, `OKTA`, `SAML2`, `GENERIC_OIDC`).

El SDK NO recibe tokens IDP — ver [Notas de Seguridad en auth-graph.md §10](../../domain-es/identity/auth-graph.md#10-notas-de-seguridad).

### 3.4 `actions` (array, requerido, puede estar vacío)

Catálogo de todas las entidades `Action` registradas en el `SystemSuite`. Usado por clientes para resolver referencias `actionCode` en `menuAccess` y `domainPermissions`.

```jsonc
"actions": [
  { "id": "uuid", "code": "VIEW", "name": "View" }
]
```

### 3.5 `menuAccess` (array, requerido, puede estar vacío)

Árbol de navegación UI: `Module → Menu → SubMenu → Option`, con el `AccessEffect` resuelto y `source` a nivel de hoja.

```jsonc
"menuAccess": [
  {
    "module": { "id": "uuid", "code": "string", "name": "string",
                "sortOrder": 0, "status": "PUBLISHED" },
    "menus": [
      {
        "id": "uuid", "code": "string", "label": "string", "sortOrder": 0,
        "subMenus": [
          {
            "id": "uuid", "code": "string", "label": "string", "sortOrder": 0,
            "options": [
              {
                "id": "uuid", "code": "string", "label": "string",
                "actionCode": "VIEW",
                "effect": "Allow" | "Deny" | "NotGranted",
                "source": "Template" | "Override"
              }
            ]
          }
        ]
      }
    ]
  }
]
```

### 3.6 `domainPermissions` (array, requerido, puede estar vacío)

Recursos de dominio (Aggregate o Entity) con permisos resueltos por acción.

```jsonc
"domainPermissions": [
  {
    "resource": { "id": "uuid", "type": "Aggregate" | "Entity",
                  "code": "string", "name": "string", "moduleId": "uuid|null" },
    "actions": [
      { "actionId": "uuid", "actionCode": "string", "actionName": "string",
        "effect": "Allow" | "Deny" | "NotGranted",
        "source": "Template" | "Override" }
    ]
  }
]
```

### 3.7 `featureFlags` (array, requerido, puede estar vacío)

Feature flags **evaluados al momento de autenticación** contra el contexto completo del usuario.

```jsonc
"featureFlags": [
  {
    "flagCode": "string",
    "systemSuiteId": "uuid",
    "isEnabled": true|false,
    "matchedCriteriaType": "TenantId|BranchId|UserProfileId|RoleCode|Environment|DateRange|PercentageHash|CustomRule" | null
  }
]
```

`isEnabled = false` y `matchedCriteriaType = null` significa que el flag existe pero ningún criteria coincidió con el contexto (false-on-missing-context según [ADR-0068](../../architecture/adrs/0068-feature-flag-system-scope.es.md)).

### 3.8 `effectiveConfig` (objeto, requerido)

Parámetros de configuración resueltos por tenant desde el `ParameterCatalog`.

```jsonc
"effectiveConfig": {
  "sessionTimeoutMinutes":  60,
  "maxLoginAttempts":       5,
  "minPasswordLength":      12,
  "mfaRequiredForAdmin":    true,
  "accessTokenDurationMs":  3600000,
  "authUseExternalIdp":     false
}
```

Esto **no** es el `ParameterCatalog` completo — solo los parámetros relevantes para autenticación, autorización y gestión de sesión client-side. Parámetros sensibles (connection strings, secretos) **nunca** se incluyen.

### 3.9 `scopes` (array de strings, requerido, puede estar vacío)

Scopes estilo OAuth2 derivados de las entradas **Allow** en `menuAccess` y `domainPermissions`. Formato: `"{resourceCode}.{actionCode}"`.

```jsonc
"scopes": ["STOCK_VIEW.VIEW", "STOCK_ADJUST.UPDATE", "PURCHASE_ORDER.VIEW"]
```

Las entradas `Deny` **no** están presentes en `scopes` — se exponen solo vía los árboles resueltos para que el SDK pueda aplicar deny-precedence. Algunos SDKs surface un `denies` derivado para checks ergonómicos (ver guías de runtime).

### 3.10 `generatedAt` / `validUntil` (string, requerido)

Timestamps ISO-8601 UTC. El SDK DEBE verificar `validUntil > now` antes de aplicar cualquier decisión de autorización; si `validUntil <= now`, la decisión es `Expired` y el cliente debe re-autenticarse.

---

## 4. Enumeraciones Abiertas vs Cerradas

El schema trata las enumeraciones en dos categorías:

| Cerradas (rechazo ante desconocido) | Abiertas (preservadas con warning) |
|---|---|
| `effect`: `Allow`/`Deny`/`NotGranted` | `tenant.status`, `user.status` (pueden agregar categorías) |
| `source`: `Template`/`Override` | `authentication.method` (Local/IDP hoy, más a futuro) |
| `profile.scope`: `OrgWide`/`BranchScoped` | `resource.type` (Aggregate/Entity hoy) |

Las enumeraciones cerradas afectan decisiones de autorización directamente — un valor desconocido sería inseguro de interpretar. Las enumeraciones abiertas son informativas; valores desconocidos se preservan y surface vía `UnknownFieldsObservedEvent`.

---

## 5. Garantía Round-Trip

Cada fixture bajo `src/libs/sdk/contracts/fixtures/` debe:
1. Validar contra `auth-graph.schema.json`.
2. Ser deserializada exitosamente por los tres SDKs.
3. Ser re-serializada por cada SDK produciendo un payload que re-valide contra el schema.

CI lo enforce en cada PR — ver [fixtures.md](./fixtures.md).

---

## 6. Referencias

- [ADR-0071: Motor del Grafo de Autorización](../../architecture/adrs/0071-auth-graph-engine.es.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
- [ADR-0074: Política de Versionado de Schema](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- [Grafo de Autorización (doc de dominio)](../../domain-es/identity/auth-graph.md)
- [Catálogo de Códigos de Error](./error-codes.md)
- [Política de Versionado (resumen developer)](./versioning.md)
- [Fixtures](./fixtures.md)
- [Matriz de Compatibilidad](./compatibility-matrix.md)
