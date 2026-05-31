# Authorization Graph — Estructura y Semántica

> **Language:** English | [Español](#)

**Bounded Context:** Cross-cutting — Identity (auth) + Authorization (graph)
**Owner:** `AuthorizationGraphBuilderService` (Application layer)
**Status:** Production

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
│   ├── tenant  { id, code, name, status }
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
Para el frontend web UMS. Retorna cookie de sesión + respuesta enriquecida con el grafo.

**Request:**
```json
{ "tenantCode": "LOGISTICS_CORE", "username": "user@ransa.pe", "password": "...", "rememberMe": false }
```

**Response:** `LoginSuccessResponse` con campo `authorizationGraph`.

---

### `POST /api/v1/client/authenticate`
Para sistemas cliente externos. Sin cookie. JWT inline con claims del grafo.

**Request:**
```json
{
  "tenantCode": "LOGISTICS_CORE",
  "username": "user@ransa.pe",
  "password": "...",
  "format": "JSON"          // opcional — override del default del tenant
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
  "graph": "{...}",          // grafo serializado
  "requestId": "uuid"        // correlaciona con el registro de auditoría
}
```

---

## 6. Vigencia del Grafo

El grafo es válido hasta `validUntil = generatedAt + SESSION_TIMEOUT_MINUTES`. El cliente debe re-autenticar al expirar. Los cambios de permisos realizados mientras el grafo está en uso toman efecto en el siguiente login.

---

## 7. Referencias

- [ADR-0071: Auth Graph Engine](../../architecture/adrs/0071-auth-graph-engine.md)
- [ADR-0072: Dynamic Auth Method Resolution](../../architecture/adrs/0072-dynamic-auth-method-resolution.md)
- [AuthorizationGraphBuilderService](../../../src/apps/ums.api/Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs)
- [ClientAuthEndpoints](../../../src/apps/ums.api/Ums.Presentation/Endpoints/Identity/Auth/ClientAuthEndpoints.cs)
