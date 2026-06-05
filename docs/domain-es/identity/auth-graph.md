# Grafo de Autorización — Estructura y Semántica

> **Idioma:** [English](../../domain/identity/auth-graph.md) | [Español](./auth-graph.md)

**Bounded Context:** Transversal — Identity (auth) + Authorization (grafo)
**Propietario:** `AuthorizationGraphBuilderService` (capa Application)
**Estado:** Producción

---

## 1. Introducción

El `AuthorizationGraph` es el artefacto central que UMS entrega al sistema cliente tras autenticar un usuario. Es un snapshot completo e inmutable del universo de autorización del usuario: quién es, en qué contexto opera, qué puede hacer, qué feature flags están activos y qué configuraciones efectivas aplica su tenant.

El cliente **no necesita volver a consultar UMS** para tomar decisiones de acceso durante la vigencia del grafo (`validUntil`).

---

## 2. Estructura del Grafo

```
AuthorizationGraph
│
├── context                          ← Contexto del principal
│   ├── user    { id, email, username, displayName, status }
│   ├── tenant  { id, code, name, status, isManagementOwner }
│   ├── systemSuite { id, code, name, status }
│   ├── role    { id, code, name, hierarchyLevel, parentRoleId? }
│   ├── profile { id, scope: "OrgWide"|"BranchScoped", isActive }
│   └── branch  { id, code, name } | null
│
├── authentication                   ← Cómo se autenticó
│   ├── method           "Local" | "IDP"
│   ├── provider         { name, code, strategy } | null
│   ├── mfaRequired      bool
│   ├── issuedAt         DateTime (UTC)
│   └── sessionExpiresAt DateTime
│
├── actions[]                        ← Catálogo completo de acciones del SystemSuite
│   └── { id, code, name }
│
├── menuAccess[]                     ← Árbol de menús con permisos efectivos
│   └── module { id, code, name, sortOrder, status }
│       └── menus[]  { id, code, label, sortOrder }
│           └── subMenus[] { id, code, label, sortOrder }
│               └── options[] { id, code, label, actionCode,
│                                effect: "Allow"|"Deny"|"NotGranted",
│                                source: "Template"|"Override" }
│
├── domainPermissions[]              ← Recursos de dominio con acciones autorizadas
│   └── resource { id, type: "Aggregate"|"Entity", code, name, moduleId? }
│       └── actions[] { actionId, actionCode, actionName,
│                        effect: "Allow"|"Deny"|"NotGranted",
│                        source: "Template"|"Override" }
│
├── featureFlags[]                   ← Flags evaluados al momento de autenticación
│   └── { flagCode, systemSuiteId, isEnabled, matchedCriteriaType? }
│
├── effectiveConfig                  ← Configuración efectiva del tenant
│   ├── sessionTimeoutMinutes
│   ├── maxLoginAttempts
│   ├── minPasswordLength
│   ├── mfaRequiredForAdmin
│   ├── accessTokenDurationMs
│   └── authUseExternalIdp
│
├── scopes[]                         ← OAuth2 scopes: "resourceCode.actionCode"
│
├── generatedAt  DateTime (UTC)
└── validUntil   DateTime (= generatedAt + sessionTimeoutMinutes)
```

---

## 3. Reglas de Resolución de Permisos

Para cada par `(TargetId, ActionId)` en el SystemSuite:

| Prioridad | Regla |
|---|---|
| 1 | `ProfilePermission.IsActive = false` → **EXCLUIR** |
| 2 | `IsOverride = true` → usar `IsAllowed`/`IsDenied` del PP (Source: Override) |
| 3 | `IsOverride = false` → usar valores del TemplateItem original (Source: Template) |
| 4 | `IsDenied = true` → **Effect: Deny** (siempre gana sobre Allow) |
| 5 | `IsAllowed = true` → **Effect: Allow** |
| 6 | Sin entrada → **Effect: NotGranted** (denegación implícita) |

---

## 4. Formatos de Serialización

El grafo se serializa según el parámetro `AUTH_GRAPH_DEFAULT_FORMAT` del tenant (default: JSON).

| Formato | ContentType | Override |
|---|---|---|
| JSON (default) | `application/json` | `?format=json` o `Accept: application/json` |
| XML | `application/xml` | `?format=xml` o `Accept: application/xml` |
| YAML | `application/x-yaml` | `?format=yaml` o `Accept: text/yaml` |
| CSV | `text/csv` | `?format=csv` o `Accept: text/csv` |

Nuevos formatos se agregan registrando un `IAuthorizationGraphSerializer` en `AuthorizationGraphSerializerFactorySetup`.

---

## 5. Endpoints

### `POST /api/v1/auth/login`
Para el frontend web UMS. Retorna cookie de sesión + respuesta enriquecida con el grafo. Este flujo se resuelve con `AuthAccessScope.PortalManagement`, por lo que siempre usa autenticación local incluso cuando la API externa del tenant está federada.

**Request:**
```json
{ "tenantCode": "INTERNAL_ADMIN", "username": "admin@ums.local", "password": "...", "rememberMe": false }
```

**Response:** `LoginSuccessResponse` con campo `authorizationGraph`.

---

### `POST /api/v1/client/authenticate`
Para sistemas cliente externos. Sin cookie. JWT inline con claims del grafo. Este flujo se resuelve con `AuthAccessScope.ExternalApi`, por lo que puede honrar el IDP configurado por tenant sin afectar el acceso de gestión del portal.

**Request:**
```json
{
  "tenantCode": "TECHNO",
  "username": "user@ransa.pe",
  "password": "...",
  "format": "JSON"
}
```

**Response:**
```json
{
  "token": "eyJ...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "issuedAt": "2026-05-31T12:00:00Z",
  "format": "JSON",
  "graph": "{...}",
  "requestId": "uuid"
}
```

---

## 6. Vigencia del Grafo

El grafo es válido hasta `validUntil = generatedAt + SESSION_TIMEOUT_MINUTES`. El cliente debe re-autenticar al expirar. Los cambios de permisos realizados mientras el grafo está en uso toman efecto en el siguiente login.

---

## 7. Ejemplo Conceptual — Cómo se Relacionan los Componentes

El grafo es un **snapshot autocontenido** del universo de autorización de un usuario al momento de la autenticación. Las relaciones entre sus piezas son las siguientes:

```
                    ┌──────────────────────┐
                    │       Tenant         │  ← raíz organizacional
                    │   (INTERNAL_ADMIN)   │     define aislamiento
                    └─────────┬────────────┘
                              │
            ┌─────────────────┼──────────────────┐
            │                 │                  │
            ▼                 ▼                  ▼
     ┌───────────┐     ┌────────────┐    ┌──────────────────┐
     │  Usuario  │     │ AuthMethod │    │ effectiveConfig  │
     │ (autenti- │     │  (Local /  │    │  global + tenant │
     │  cado)    │     │   IDP)     │    │   (resuelto)     │
     └─────┬─────┘     └────────────┘    └──────────────────┘
           │
           │  pertenece a 1 Profile activo por (Tenant, SystemSuite)
           ▼
     ┌───────────┐
     │  Profile  │  ── enlaza a ──►  Role  (jerarquía, code, level)
     │  (scope:  │
     │  OrgWide  │  ── materializa ──►  ProfilePermission[]
     │  o Branch │
     │  Scoped)  │
     └─────┬─────┘
           │
           │ reglas de resolución (override > template, deny > allow)
           ▼
     ┌────────────────────────────────────────────────────┐
     │  Permisos efectivos                                │
     │    • menuAccess[]      (árbol UI)                  │
     │    • domainPermissions[] (Aggregate/Entity)        │
     │    • scopes[]          ("resourceCode.actionCode") │
     └────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌──────────────────────┐
                    │   featureFlags[]     │  ← reglas dinámicas
                    │ (evaluados con el    │     evaluadas en auth-time
                    │  contexto completo)  │     contra el principal
                    └──────────────────────┘
```

| Componente | Rol en el grafo | Origen |
|---|---|---|
| **Tenant** | Define el alcance organizacional. Todos los demás datos se resuelven dentro de su frontera. | `context.tenant` |
| **Usuario autenticado** | Principal cuya identidad ya fue verificada por el `AuthMethod`. | `context.user` |
| **Profile** | Enlaza al usuario con un `Role` y un conjunto de `ProfilePermission` materializados. Es el pivote desde el cual se resuelven todos los permisos. | `context.profile` |
| **Método de autenticación** | Registra cómo se verificó la identidad (`Local` BCrypt o `IDP` federado). Determinado por `IAuthMethodResolver` desde `AUTH_USE_EXTERNAL_IDP`. | `authentication` |
| **Roles** | Catálogo jerárquico del SystemSuite que define el "qué puede ser" el usuario. El Profile referencia uno. | `context.role` |
| **Permisos** | Resultado de aplicar las reglas Deny-wins/Override-precedence sobre los `ProfilePermission` del Profile. Se exponen en dos vistas: `menuAccess[]` (UI) y `domainPermissions[]` (recursos de dominio). | `menuAccess[]`, `domainPermissions[]` |
| **Scopes** | Traducción OAuth2-style de los permisos `Allow` a strings `resourceCode.actionCode`, listos para validación rápida en el cliente. | `scopes[]` |
| **Reglas de autorización** | Feature flags evaluados contra el contexto del usuario al momento de autenticación (no después). | `featureFlags[]` |
| **Parametrización global efectiva** | Defaults de plataforma para parámetros del `ParameterCatalog` con `TenantId = NULL`. | `effectiveConfig` (merge) |
| **Parametrización específica del tenant** | Overrides del tenant sobre los defaults globales. Mayor precedencia. | `effectiveConfig` (merge) |

El cliente recibe **un único documento** que es suficiente para tomar todas sus decisiones de acceso durante la vigencia de la sesión.

---

## 8. Ejemplo JSON de Respuesta

Ejemplo representativo de `POST /api/v1/client/authenticate` para una autenticación **Local (BCrypt)** exitosa. Datos ficticios.

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.PLACEHOLDER.SIGNATURE",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "issuedAt": "2026-05-31T14:30:00Z",
  "format": "JSON",
  "requestId": "8c3f1b2a-9e44-4d7a-b8c1-2a1f4e5d6c7b",
  "graph": {
    "context": {
      "user": {
        "id": "7a1d4e22-0001-4f00-9a00-100000000001",
        "email": "ana.flores@logistics-corp.example",
        "username": "ana.flores",
        "displayName": "Ana Flores",
        "status": "ACTIVE"
      },
      "tenant": {
        "id": "11111111-1111-4111-8111-111111111111",
        "code": "INTERNAL_ADMIN",
        "name": "Logistics Corp",
        "status": "ACTIVE"
      },
      "systemSuite": {
        "id": "22222222-2222-4222-8222-222222222222",
        "code": "WMS_SUITE",
        "name": "Warehouse Management Suite",
        "status": "PUBLISHED"
      },
      "role": {
        "id": "33333333-3333-4333-8333-333333333333",
        "code": "WAREHOUSE_SUPERVISOR",
        "name": "Warehouse Supervisor",
        "hierarchyLevel": 3,
        "parentRoleId": "33333333-3333-4333-8333-333333333330"
      },
      "profile": {
        "id": "44444444-4444-4444-8444-444444444444",
        "scope": "BranchScoped",
        "isActive": true
      },
      "branch": {
        "id": "55555555-5555-4555-8555-555555555555",
        "code": "CALLAO_DC",
        "name": "Callao Distribution Center"
      }
    },
    "authentication": {
      "method": "Local",
      "provider": null,
      "mfaRequired": false,
      "issuedAt": "2026-05-31T14:30:00Z",
      "sessionExpiresAt": "2026-05-31T15:30:00Z"
    },
    "actions": [
      { "id": "a0000001-0000-4000-8000-000000000001", "code": "VIEW", "name": "View" },
      { "id": "a0000001-0000-4000-8000-000000000002", "code": "CREATE", "name": "Create" },
      { "id": "a0000001-0000-4000-8000-000000000003", "code": "UPDATE", "name": "Update" },
      { "id": "a0000001-0000-4000-8000-000000000004", "code": "DELETE", "name": "Delete" },
      { "id": "a0000001-0000-4000-8000-000000000005", "code": "APPROVE", "name": "Approve" }
    ],
    "menuAccess": [
      {
        "module": {
          "id": "m0000001-0000-4000-8000-000000000001",
          "code": "INVENTORY",
          "name": "Inventory",
          "sortOrder": 1,
          "status": "PUBLISHED"
        },
        "menus": [
          {
            "id": "n0000001-0000-4000-8000-000000000001",
            "code": "STOCK",
            "label": "Stock Management",
            "sortOrder": 1,
            "subMenus": [
              {
                "id": "s0000001-0000-4000-8000-000000000001",
                "code": "STOCK_OPS",
                "label": "Operations",
                "sortOrder": 1,
                "options": [
                  {
                    "id": "o0000001-0000-4000-8000-000000000001",
                    "code": "STOCK_VIEW",
                    "label": "View Stock",
                    "actionCode": "VIEW",
                    "effect": "Allow",
                    "source": "Template"
                  },
                  {
                    "id": "o0000001-0000-4000-8000-000000000002",
                    "code": "STOCK_ADJUST",
                    "label": "Adjust Stock",
                    "actionCode": "UPDATE",
                    "effect": "Allow",
                    "source": "Override"
                  },
                  {
                    "id": "o0000001-0000-4000-8000-000000000003",
                    "code": "STOCK_DELETE",
                    "label": "Delete Stock Record",
                    "actionCode": "DELETE",
                    "effect": "Deny",
                    "source": "Override"
                  }
                ]
              }
            ]
          }
        ]
      }
    ],
    "domainPermissions": [
      {
        "resource": {
          "id": "r0000001-0000-4000-8000-000000000001",
          "type": "Aggregate",
          "code": "PURCHASE_ORDER",
          "name": "Purchase Order",
          "moduleId": "m0000001-0000-4000-8000-000000000001"
        },
        "actions": [
          {
            "actionId": "a0000001-0000-4000-8000-000000000001",
            "actionCode": "VIEW",
            "actionName": "View",
            "effect": "Allow",
            "source": "Template"
          },
          {
            "actionId": "a0000001-0000-4000-8000-000000000005",
            "actionCode": "APPROVE",
            "actionName": "Approve",
            "effect": "NotGranted",
            "source": "Template"
          }
        ]
      }
    ],
    "featureFlags": [
      {
        "flagCode": "WMS_NEW_PICKING_UI",
        "systemSuiteId": "22222222-2222-4222-8222-222222222222",
        "isEnabled": true,
        "matchedCriteriaType": "BranchId"
      },
      {
        "flagCode": "WMS_BULK_EXPORT",
        "systemSuiteId": "22222222-2222-4222-8222-222222222222",
        "isEnabled": false,
        "matchedCriteriaType": null
      }
    ],
    "effectiveConfig": {
      "sessionTimeoutMinutes": 60,
      "maxLoginAttempts": 5,
      "minPasswordLength": 12,
      "mfaRequiredForAdmin": true,
      "accessTokenDurationMs": 3600000,
      "authUseExternalIdp": false
    },
    "scopes": [
      "STOCK_VIEW.VIEW",
      "STOCK_ADJUST.UPDATE",
      "PURCHASE_ORDER.VIEW"
    ],
    "generatedAt": "2026-05-31T14:30:00Z",
    "validUntil": "2026-05-31T15:30:00Z"
  }
}
```

> **Variante IDP**: para una autenticación federada, la sección `authentication` cambia a:
> ```json
> "authentication": {
>   "method": "IDP",
>   "provider": { "name": "Azure AD - Logistics", "code": "AZURE_AD_LOGISTICS", "strategy": "AZURE_AD" },
>   "mfaRequired": true,
>   "issuedAt": "2026-05-31T14:30:00Z",
>   "sessionExpiresAt": "2026-05-31T15:30:00Z"
> }
> ```
> El resto del grafo es idéntico — la fuente de la identidad es transparente para los consumidores del grafo.

---

## 9. Ejemplo Simplificado para Consumo del Cliente

El cliente típicamente extrae sólo lo necesario para tomar decisiones rápidas. Un fragmento mínimo cacheado podría verse así:

```json
{
  "tenantCode": "TECHNO",
  "userId": "7a1d4e22-0001-4f00-9a00-100000000001",
  "roleCode": "WAREHOUSE_SUPERVISOR",
  "scopes": [
    "STOCK_VIEW.VIEW",
    "STOCK_ADJUST.UPDATE",
    "PURCHASE_ORDER.VIEW"
  ],
  "denies": [
    "STOCK_DELETE.DELETE"
  ],
  "validUntil": "2026-05-31T15:30:00Z"
}
```

### Patrones de validación en el cliente

**Validar si el usuario tiene un permiso:**

```ts
function canPerform(scopes: string[], denies: string[], scope: string): boolean {
  // Deny gana siempre (Axioma A3)
  if (denies.includes(scope)) return false;
  return scopes.includes(scope);
}

// Uso:
canPerform(graph.scopes, graph.denies, "STOCK_ADJUST.UPDATE"); // true
canPerform(graph.scopes, graph.denies, "STOCK_DELETE.DELETE"); // false (deny)
canPerform(graph.scopes, graph.denies, "STOCK_PURGE.DELETE");  // false (NotGranted)
```

**Validar pertenencia a un scope (resource-level):**

```ts
function hasAnyActionOn(scopes: string[], resourceCode: string): boolean {
  return scopes.some(s => s.startsWith(`${resourceCode}.`));
}

hasAnyActionOn(graph.scopes, "PURCHASE_ORDER"); // true
```

**Validar restricciones por tenant antes de procesar una request:**

```ts
function assertTenant(graph: AuthGraph, expectedTenantCode: string): void {
  if (graph.context.tenant.code !== expectedTenantCode) {
    throw new Error("Tenant mismatch — request rechazado");
  }
  if (graph.context.tenant.status !== "ACTIVE") {
    throw new Error("Tenant no activo");
  }
  if (new Date(graph.validUntil) < new Date()) {
    throw new Error("Auth graph expirado — re-autenticar");
  }
}
```

**Validar un feature flag al renderizar UI:**

```ts
function isFlagEnabled(graph: AuthGraph, code: string): boolean {
  const flag = graph.featureFlags.find(f => f.flagCode === code);
  return flag?.isEnabled === true;
}
```

---

## 10. Notas de Seguridad

El grafo está diseñado para ser **seguro de exponer al sistema cliente**, pero debe respetar las siguientes reglas de contenido:

| Regla | Detalle |
|---|---|
| **No exponer secretos** | El grafo NUNCA debe incluir `PasswordHash`, `ApiCredentialHash`, claves de firma JWT, secretos de cliente OAuth, ni ningún material criptográfico. |
| **No exponer tokens de proveedores IDP** | `id_token`, `access_token`, `refresh_token` o assertions SAML del IDP externo se consumen únicamente dentro de UMS durante la autenticación y se descartan. No deben aparecer en `authentication.provider`. |
| **No exponer credenciales** | Ni en plano ni hasheadas. El campo `authentication.method` indica `Local` o `IDP` pero nunca acompaña el secreto utilizado. |
| **No exponer configuración sensible** | `effectiveConfig` sólo expone parámetros operacionales del sistema cliente (timeouts, longitudes mínimas, flags booleanos). Connection strings, claves de servicios externos, endpoints internos y secretos de infraestructura están explícitamente excluidos. |
| **Datos personales mínimos** | El grafo expone los datos de identidad mínimos necesarios para que el sistema cliente atribuya acciones (`id`, `email`, `displayName`). Datos sensibles de RRHH, dirección, documentos de identidad u otros campos PII NO forman parte del grafo. |
| **Sin información de otros tenants** | Cada grafo está scopeado a un único tenant. Resolución, configuración y permisos de otros tenants nunca se incluyen, incluso si el usuario tiene acceso a múltiples organizaciones (cada tenant requiere su propia autenticación). |
| **Sin metadatos internos de UMS** | IDs internos de infraestructura, nombres de schemas SQL, nombres de tablas, hashes de partición y otros detalles de implementación no se exponen. |
| **Auditoría** | El `requestId` correlaciona la respuesta con un `AuthenticationAttemptedEvent` en el audit trail. El cliente puede registrar este ID para soporte sin exponer detalles internos. |

**Principio rector:** el grafo contiene solo lo necesario para que el sistema cliente tome decisiones de **autenticación, autorización y control de acceso**. Cualquier campo adicional debe justificarse contra este principio antes de incluirse.

---

## 11. Referencias

- [ADR-0071: Motor del Grafo de Autorización](../../architecture/adrs/0071-auth-graph-engine.es.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](../../architecture/adrs/0072-dynamic-auth-method-resolution.es.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.es.md) — superficie oficial de consumo client-side
- [ADR-0074: Política de Versionado del Schema del Grafo](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- [Contrato Semantico del Auth Graph](../../sdk-es/contracts/semantic-client-contract.md)
- [Resolución del Método de Autenticación](./auth-method-resolution.md)
- [Portal UMS SDK](../../sdk-es/index.md) — deserialización tipada, validador, atributos/decorators para .NET, TypeScript y NestJS
- [Schema Overview](../../sdk-es/contracts/schema-overview.md)
- [AuthorizationGraphBuilderService](../../../src/apps/ums.api/Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs)
- [ClientAuthEndpoints](../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Auth/ClientAuthEndpoints.cs)
