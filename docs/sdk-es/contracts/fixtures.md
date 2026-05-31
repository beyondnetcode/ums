# Golden Fixtures

> **Idioma:** [English](../../sdk/contracts/fixtures.md) | Español

Las golden fixtures son archivos JSON en `src/libs/sdk/contracts/fixtures/` que capturan **payloads representativos de `AuthorizationGraph`** cubriendo el espectro completo de comportamientos. Son el contrato ejecutable: cada SDK y el servidor UMS deben coincidir en cómo interpretarlas.

---

## 1. Qué garantizan las fixtures

Cada fixture pasa por round-trip a través de cada artefacto que toca el schema:

1. **Validación de JSON Schema** — la fixture conforma a `auth-graph.schema.json`.
2. **Regresión del builder del servidor** — el `AuthorizationGraphBuilderService` de UMS puede producir esta forma exacta dado el input equivalente.
3. **Deserialización del SDK** — `Ums.Sdk.Contracts`, `@ums/sdk-contracts` la parsean sin pérdida.
4. **Comportamiento del validador del SDK** — dada esta fixture y una probe (ej. "verificar `RequiresScope('STOCK_VIEW.VIEW')`"), cada runtime retorna el mismo `AuthorizationDecision`.
5. **Re-serialización** — cada SDK puede serializar el grafo deserializado de vuelta a JSON que todavía valida contra el schema.

CI corre los cinco pasos en cada PR. Una falla en cualquier paso bloquea el PR.

---

## 2. Set canónico de fixtures (v1.0.0)

| Archivo | Escenario | Usado para verificar |
|---|---|---|
| `local-auth-success.json` | Happy path auth Local (BCrypt), profile OrgWide, mezcla de Allow / NotGranted / Deny | Forma baseline, generación de scopes |
| `idp-auth-success.json` | Happy path auth IDP (Azure AD), profile BranchScoped, MFA requerido | Bloque `authentication` IDP, `branch` no nulo |
| `deny-wins.json` | La misma acción tiene Allow en un lugar y Deny en otro | Regla deny-precedence (Axioma A3) |
| `override-allow.json` | Override de profile cambia un Deny de Template a Allow | Regla override-takes-precedence |
| `empty-permissions.json` | Usuario tiene Profile pero sin permisos concedidos | `scopes[]`, `domainPermissions[]` vacíos |
| `expired-graph.json` | `validUntil < generatedAt + 1ms` (expirado deliberadamente) | Rechazo `AUTH_201` por SDKs |
| `feature-flag-matched.json` | Flag coincide vía criteria `BranchId` | `isEnabled = true`, `matchedCriteriaType` seteado |
| `feature-flag-missed-context.json` | Flag existe, pero el contexto no provee el campo requerido | `isEnabled = false`, `matchedCriteriaType = null` |
| `multi-tenant-rejection.json` | Grafo es para tenant A, usado para probe a cliente que espera tenant B | Mismatch de tenant `AUTH_109` |
| `schema-unsupported-major.json` | `schemaVersion: "2.0.0"` (MAJOR futuro) | SDK con compat `<2.0.0` rechaza con `AUTH_205` |
| `schema-minor-ahead.json` | `schemaVersion: "1.99.0"`, contiene campo opcional desconocido | Accept-with-warning, preservar en `extensions` |
| `schema-missing.json` | Payload sin `schemaVersion` | Rechazo `AUTH_204` |

---

## 3. Convenciones para escribir fixtures

- **Usa el patrón de UUID placeholder** introducido en `auth-graph.md` §8 (`11111111-1111-...` para tenants, `22222222-...` para system suites, etc.). IDs predecibles hacen diffs legibles.
- **Usa códigos de tenant ficticios pero realistas**: `LOGISTICS_CORE`, `ACME_RETAIL`, `EXAMPLE_BANK`. Nunca nombres de clientes reales.
- **Sin PII**: los emails siguen `firstname.lastname@<dominio-ficticio>.example`. Usa `.example` por RFC 2606.
- **Sin secretos, sin tokens, sin hashes** de valores reales. Los valores JWT en fixtures son literales `"PLACEHOLDER"`.
- **Una fixture, un escenario**: no apiles múltiples preocupaciones semánticas en una fixture. Si necesitas probar dos cosas, escribe dos fixtures.
- **Las fixtures son inmutables una vez publicadas**: cambiar una fixture existente es un cambio coordinado a través de todos los test suites de SDK. Agregar una fixture nueva es lo default.

---

## 4. Agregar una fixture nueva

1. Crear `src/libs/sdk/contracts/fixtures/<scenario-name>.json`.
2. Agregar una fila a la tabla de este archivo arriba.
3. Agregar casos de test correspondientes en el test suite de cada SDK, referenciando la fixture por ruta relativa.
4. Correr el job CI de contract-validation localmente para confirmar que el round-trip pasa.
5. El PR incluye: nueva fixture + 3 updates de test de SDK + fila de docs.

---

## 5. Remover una fixture

Una fixture solo puede removerse si el escenario que representa ya no es alcanzable en ninguna versión de schema soportada. La remoción requiere:

1. Confirmación en la descripción del PR que ningún schema soportado produce un payload coincidiendo con esta fixture.
2. Remoción de los casos de test correspondientes del SDK.
3. Fila borrada de este archivo.

Si una fixture queda outdated por un cambio MAJOR de schema, preferir **migrarla** (actualizarla para coincidir con el nuevo schema) en lugar de borrarla. Las fixtures históricas son útiles para análisis de regresión.

---

## 6. Referencias

- [Schema Overview](./schema-overview.md)
- [Política de Versionado](./versioning.md)
- [Grafo de Autorización (doc de dominio)](../../domain-es/identity/auth-graph.md)
- Directorio fuente de fixtures: `src/libs/sdk/contracts/fixtures/`
