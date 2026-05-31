# Catálogo de Códigos de Error — `AUTH_xxx`

> **Idioma:** [English](../../sdk/contracts/error-codes.md) | Español

Este es el **catálogo canónico** de códigos de error del dominio de autorización usado por el servidor UMS, los tres SDKs, y todos los sistemas cliente que integran vía el SDK.

El catálogo se origina en `src/libs/sdk/contracts/error-codes.yaml`. Los paquetes SDK generan constantes desde este archivo en build time, por lo que el código consumidor referencia constantes como `UmsErrorCodes.InvalidCredentials` en lugar de strings mágicos.

---

## 1. Errores de Autenticación (AUTH_001 – AUTH_010)

| Código | HTTP | Significado | Emitido por |
|---|---|---|---|
| `AUTH_001` | 400 | Error de validación — campos requeridos faltantes o malformados | Servidor, SDKs |
| `AUTH_002` | 404 | Tenant no encontrado | Servidor |
| `AUTH_003` | 403 | Tenant no activo | Servidor |
| `AUTH_004` | 401 | Usuario IDP sin UserAccount UMS correspondiente | Servidor |
| `AUTH_005` | 403 | Estado de UserAccount no es ACTIVE | Servidor |
| `AUTH_006` | 401 | Credenciales inválidas (BCrypt Local) | Servidor |
| `AUTH_007` | 423 | Cuenta bloqueada (max login attempts excedido) | Servidor |
| `AUTH_008` | 401 | Challenge MFA requerido pero no provisto | Servidor |
| `AUTH_009` | 401 | Challenge MFA falló | Servidor |
| `AUTH_010` | 401 | Sesión expirada | Servidor |

## 2. Errores de Resolución IDP (AUTH_011 – AUTH_019)

| Código | HTTP | Significado |
|---|---|---|
| `AUTH_011` | 503 | Modo IDP configurado pero sin proveedor IDP activo |
| `AUTH_012` | 501 | Sin adaptador IDP registrado para la estrategia del proveedor |
| `AUTH_013` | 502 | Llamada de autenticación IDP falló (error de red / proveedor) |
| `AUTH_014` | 401 | Validación de token IDP falló (firma, expiración, issuer) |

## 3. Errores de Autorización (AUTH_100 – AUTH_199) — emitidos por SDKs

| Código | Significado | Disparador |
|---|---|---|
| `AUTH_101` | Scope no concedido | `RequiresScope("X.Y")` y scope ausente de `scopes[]` |
| `AUTH_102` | Scope explícitamente denegado | Scope presente en denies resueltos |
| `AUTH_103` | Opción de menú no concedida | `RequiresMenuOption("CODE")` resuelve a `NotGranted` |
| `AUTH_104` | Opción de menú denegada | Resuelve a `Deny` |
| `AUTH_105` | Acceso de dominio no concedido | `RequiresDomainAccess` resuelve a `NotGranted` |
| `AUTH_106` | Acceso de dominio denegado | Resuelve a `Deny` |
| `AUTH_107` | Feature flag deshabilitado | `RequiresFeatureFlag` y `isEnabled = false` |
| `AUTH_108` | Feature flag no encontrado | Código de flag no presente en `featureFlags[]` |
| `AUTH_109` | Mismatch de tenant | Tenant esperado del request difiere del tenant del grafo |

## 4. Errores de Ciclo de Vida del Grafo (AUTH_200 – AUTH_299) — emitidos por SDKs

| Código | Significado |
|---|---|
| `AUTH_201` | `AUTH_GRAPH_EXPIRED` — `validUntil` está en el pasado |
| `AUTH_202` | `AUTH_GRAPH_MISSING` — no hay grafo disponible en el accessor |
| `AUTH_203` | `AUTH_GRAPH_MALFORMED` — grafo falló validación de JSON Schema |
| `AUTH_204` | `AUTH_GRAPH_SCHEMA_MISSING` — campo `schemaVersion` ausente |
| `AUTH_205` | `AUTH_GRAPH_SCHEMA_UNSUPPORTED` — versión MAJOR fuera del rango de compatibilidad del SDK |

## 5. Forma del Objeto de Error

Todos los errores de autorización emitidos por SDK llevan esta forma (tipada en cada runtime, semántica equivalente):

```jsonc
{
  "code":        "AUTH_101",
  "message":     "Scope 'PURCHASE_ORDER.APPROVE' no concedido",
  "primitive":   "RequiresScope",
  "target":      "PURCHASE_ORDER.APPROVE",
  "graphRequestId": "uuid",   // correlaciona con el audit trail UMS
  "validUntil":  "ISO-8601",  // ayuda a los clientes a decidir re-auth
  "occurredAt":  "ISO-8601"
}
```

Los errores emitidos por el servidor (AUTH_001–AUTH_099) siguen el [Contrato de Errores Accionables](../../architecture/adrs/0066-actionable-user-error-contract.es.md) (ADR-0066).

## 6. Política de Extensión

- Códigos nuevos se agregan mediante un bump MINOR del contrato del SDK.
- Los códigos **nunca** se reutilizan — un código retirado permanece en el catálogo marcado `deprecated: true`.
- Los códigos son strings estables — aparecen en logs, dashboards y auditorías de seguridad. Renombrar un código es un bump MAJOR.

---

## 7. Referencias

- [`error-codes.yaml`](../../../src/libs/sdk/contracts/error-codes.yaml) (fuente canónica)
- [ADR-0066: Contrato de Errores Accionables](../../architecture/adrs/0066-actionable-user-error-contract.es.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](../../architecture/adrs/0072-dynamic-auth-method-resolution.es.md)
- [Schema Overview](./schema-overview.md)
- [Política de Versionado](./versioning.md)
