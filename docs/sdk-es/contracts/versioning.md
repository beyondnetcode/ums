# Política de Versionado — Resumen para Developers

> **Idioma:** [English](../../sdk/contracts/versioning.md) | Español

Este es un **resumen orientado a developers** de la política de versionado del schema. La decisión arquitectural completa está en [ADR-0074](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md).

---

## Resumen

- El `AuthorizationGraph` lleva `"schemaVersion": "MAJOR.MINOR.PATCH"`.
- Los SDKs declaran un rango soportado (ej. `>=1.0.0 <2.0.0`) en metadata del paquete.
- **El servidor emite un MAJOR que tu SDK no soporta → SDK rechaza** (`AUTH_205`).
- **El servidor emite un MINOR adelantado a tu SDK → SDK acepta con warning**, campos desconocidos preservados pero no usados.
- **El servidor emite un MINOR retrasado respecto a tu SDK → SDK acepta con warning**, puedes ver campos ausentes.

Casi nunca necesitas pensar en la versión. Bumpeas tu SDK en una cadencia normal; las reglas de arriba mantienen producción segura mientras tanto.

---

## Qué cambios cuentan como qué

### Bump MAJOR (breaking)

- Remover un campo requerido
- Renombrar un campo
- Narrowing del tipo de un campo
- Remover un valor enum que puede aparecer en payloads viejos
- Cambiar semántica de resolución (ej. invertir precedencia Allow/Deny)

### Bump MINOR (aditivo)

- Agregar un campo opcional
- Agregar un nuevo valor enum a una enumeración abierta
- Agregar una nueva sección top-level
- Agregar un nuevo tipo de criteria en `featureFlags[]`

### Bump PATCH (cosmético)

- Updates solo de documentación
- Refinamiento de patrones de validación que no rechazan payloads previamente válidos

Ante la duda, el owner del schema bumpea mayor. Falso-MAJOR es molesto; falso-MINOR es un incidente.

---

## Qué haces como consumidor del SDK

1. **Fija un rango MAJOR.** Tu package.json o csproj ya lo hace mediante `umsSchemaCompatibility`. No amplíes el rango a menos que hayas probado contra un MAJOR nuevo.
2. **Observa los logs en busca de estos eventos:**
   - `SchemaMinorMismatchEvent` — el servidor emite un MINOR más nuevo. Considera actualizar pronto.
   - `SchemaServerOlderEvent` — tu SDK es más nuevo que el servidor. Usualmente seguro pero rastrea upgrades del servidor UMS.
   - `UnknownFieldsObservedEvent` — el servidor está enviando campos que no puedes usar. Actualiza en tu próxima oportunidad.
   - `DeprecatedFieldUsageEvent` — tu código lee un campo marcado para remoción. Migra antes del próximo MAJOR.
3. **Re-autentica ante `AUTH_205`** — tu SDK no puede interpretar de forma segura lo que el servidor envió. O el servidor se actualizó (tú también deberías) o el servidor se degradó (habla con tu operador UMS).

---

## Qué haces como contribuyente del servidor UMS

Cuando propones un cambio en el grafo, documenta la categoría del bump en la descripción del PR:

> Impacto de schema: MINOR — agrega campo opcional `context.user.locale`.

CI valida que el `schemaVersion` declarado del schema coincida con la categoría del cambio corriendo golden fixtures de la versión previa contra el schema nuevo.

Para cambios MAJOR:
1. Abre un ADR separado documentando la breakage y el camino de migración.
2. Actualiza `compatibility-matrix.md`.
3. Comunica vía release notes al menos un release MINOR antes.

Para deprecaciones:
1. Marca el campo en JSON Schema con `"deprecated": true` y `"x-deprecation-removed-in": "2.0.0"`.
2. Espera al menos dos releases MINOR o seis meses.
3. Remueve en el siguiente MAJOR.

---

## Ejemplo de ciclo de vida de deprecación

| Release | Acción |
|---|---|
| Schema 1.4.0 (MINOR) | Agregar `context.tenant.legalName` (nuevo). Marcar `context.tenant.name` como `deprecated: true`, reemplazo es `legalName`. |
| Schema 1.5.0 (MINOR) | SDKs leyendo `name` emiten `DeprecatedFieldUsageEvent`. |
| Schema 1.6.0 (MINOR) | Igual — último período de gracia. |
| Schema 2.0.0 (MAJOR) | `context.tenant.name` removido. SDKs viejos (compat `<2.0.0`) recibirán `AUTH_205`. Guía de migración publicada. |

Ventana mínima: dos MINORs o seis meses, lo que sea mayor.

---

## Referencias

- [ADR-0074: Política de Versionado del Schema del Grafo](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md) — decisión completa
- [Schema Overview](./schema-overview.md) — forma actual del contrato
- [Matriz de Compatibilidad](./compatibility-matrix.md) — qué funciona con qué
- `src/libs/sdk/contracts/SCHEMA_VERSIONING.md` — resumen operacional dentro del árbol de código
