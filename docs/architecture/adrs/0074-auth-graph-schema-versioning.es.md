# ADR-0074: Política de Versionado del Schema del Grafo de Autorización

**Estado:** Aceptado
**Fecha:** 2026-05-31
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0071: Motor del Grafo de Autorización](./0071-auth-graph-engine.es.md)
- [ADR-0073: UMS SDK — Superficie de Integración Cliente Multi-Runtime](./0073-ums-sdk-multi-runtime.es.md)

---

## Contexto

ADR-0073 establece el JSON Schema del `AuthorizationGraph` como el contrato canónico entre el servidor UMS y tres runtimes de SDK (.NET, TypeScript, NestJS). El schema evoluciona: se agregan campos nuevos cuando el grafo carga información nueva, campos existentes se deprecan, ocasionalmente un campo se remueve o su semántica cambia.

Un SDK multi-runtime con cadencia de release independiente por paquete significa que el servidor puede emitir un grafo en versión N mientras un cliente todavía corre un SDK que conoce la versión N-1. Sin una política de compatibilidad documentada, cada cambio de schema se convierte en un problema de coordinación que puede romper clientes en producción.

Este ADR define cómo se versiona el schema, cómo se decide la compatibilidad por categoría de cambio, y cómo los SDKs y el servidor lo enforce.

---

## Decisión

### 1. El grafo lleva un campo explícito `schemaVersion`

Cada payload `AuthorizationGraph` emitido por `AuthorizationGraphBuilderService` incluye un string top-level `schemaVersion` conforme a versionado semántico (`MAJOR.MINOR.PATCH`).

```json
{
  "schemaVersion": "1.2.0",
  "context": { ... },
  ...
}
```

El campo es requerido y validado contra la constante `schemaVersion` del propio `auth-graph.schema.json`. La ausencia del campo es interpretada por los SDKs como "v0.x — no soportado, rechazar deserialización".

### 2. Reglas de versionado semántico — qué cuenta como MAJOR / MINOR / PATCH

| Bump | Disparador |
|---|---|
| **MAJOR** | Un cambio que un SDK targeting el major previo **no puede** deserializar o interpretar de forma segura. Ejemplos: remover un campo requerido, renombrar un campo, narrowing del tipo de un campo, remover un valor de enum que puede aparecer en payloads viejos, cambiar la semántica de resolución (ej. invertir precedencia Allow/Deny). |
| **MINOR** | Un cambio que es **backward-compatible** para lectores del major previo: agregar un campo opcional, agregar un valor nuevo a una enumeración abierta, agregar una sección nueva independiente de secciones existentes, agregar un nuevo tipo de criteria en `featureFlags[]`. |
| **PATCH** | Cambios solo de documentación al schema, refinamiento de patrones de validación que no rechazan payloads previamente válidos, clarificación de descripciones de campos. Sin cambios estructurales. |

Ante la duda, **preferir el bump mayor**. Falso-MAJOR es molesto; falso-MINOR es un incidente en producción.

### 3. Los SDKs declaran un rango de compatibilidad target

Cada paquete SDK declara su rango de schema soportado usando sintaxis de range semver en su metadata de paquete:

- **.NET (csproj):** propiedad custom `<UmsSchemaCompatibility>>=1.0.0 &lt;2.0.0</UmsSchemaCompatibility>`
- **npm (package.json):** campo custom `"umsSchemaCompatibility": ">=1.0.0 <2.0.0"`

El SDK lo lee al inicio, lo expone vía `ISdkMetadata.SchemaCompatibility` (o equivalente en cada runtime), y lo usa para validar grafos entrantes.

### 4. Estricto en MAJOR, permisivo en MINOR — el contrato runtime

| Servidor emite | SDK soporta | Comportamiento del SDK |
|---|---|---|
| `1.0.0` | `>=1.0.0 <2.0.0` | **Accept** — match exacto |
| `1.2.0` | `>=1.0.0 <2.0.0` | **Accept con warning** — el servidor es más nuevo, campos desconocidos preservados pero no usados. Logear evento estructurado `SchemaMinorMismatchEvent`. |
| `1.0.0` | `>=1.2.0 <2.0.0` | **Accept con warning** — el SDK es más nuevo, el servidor emite subset. Logear evento estructurado `SchemaServerOlderEvent`. |
| `2.0.0` | `>=1.0.0 <2.0.0` | **Reject** — error `AUTH_GRAPH_SCHEMA_UNSUPPORTED`. El SDK rechaza deserializar. |
| `0.x` o ausente | cualquiera | **Reject** — error `AUTH_GRAPH_SCHEMA_MISSING`. |

La justificación: un bump MINOR significa que lectores de la versión previa todavía pueden extraer todo lo que necesitan; el warning es apropiado. Un bump MAJOR significa que la semántica cambió; solo el rechazo duro es seguro.

### 5. Política de campos desconocidos: preservar, advertir, no errorar

Al deserializar un grafo en un MINOR adelantado al ceiling de compatibilidad del SDK, los campos desconocidos son:

- **Preservados** en la representación en memoria (un bag `extensions: Map<string, unknown>`), para que re-serialización (para cache, logging) haga round-trip fielmente.
- **Ignorados** para decisiones de autorización — el SDK no puede aplicar reglas que no entiende.
- **Logeados una vez por sesión** como un evento estructurado `UnknownFieldsObservedEvent` listando los paths de campos, para que operadores detecten staleness del SDK.

Esto rechaza explícitamente dos alternativas: strict-only (rechazar cualquier campo desconocido) que vuelve breaking los bumps MINOR, y silent-ignore que oculta staleness de operaciones.

### 6. Ciclo de vida de deprecación para remover o cambiar campos

Remover o cambiar un campo requiere la siguiente secuencia:

1. **MINOR N**: introducir el campo de reemplazo. Ambos viejo y nuevo coexisten. Marcar el viejo como `deprecated: true` en el JSON Schema con `description` apuntando al reemplazo y una versión target de remoción.
2. **MINOR N+1 o posterior**: los SDKs emiten `DeprecatedFieldUsageEvent` cada vez que código consumidor lee el campo deprecado.
3. **MAJOR N+1**: remover el campo deprecado. Los servidores dejan de emitirlo. Documentación y guía de migración actualizadas.

Ventana mínima entre pasos 1 y 3: **dos releases MINOR** o **seis meses**, lo que sea mayor.

### 7. La matriz de compatibilidad es un artefacto mantenido

`docs/sdk/contracts/compatibility-matrix.md` (bilingüe) lista cada versión publicada de SDK contra cada versión emitida de schema, marcado como `Accept`, `Accept-with-warning` o `Reject`. CI actualiza este archivo cuando se liberan SDKs. La matriz es la fuente de verdad para preguntas de soporte ("¿funciona Ums.Sdk.Authorization 1.4.2 con UMS servidor emitiendo schema 2.0?").

### 8. La versión del schema es independiente de la versión del paquete SDK

- `auth-graph.schema.json` lleva `schemaVersion: "1.0.0"`.
- `Ums.Sdk.Contracts` puede ser `1.0.0`, `1.5.3`, etc. — su versión refleja cambios de API en la superficie de DTO, que pueden bumpear independientemente (ej. para refactors de naming que no afectan el wire schema).
- Una sola versión de schema puede ser soportada por muchas versiones de paquete SDK durante su tiempo de vida.

Los dos ejes de versión **nunca** se confunden. El grafo lleva `schemaVersion`. El SDK declara `umsSchemaCompatibility`. Ninguno dice nada sobre la versión de paquete del otro.

---

## Justificación

### Por qué semver aplicado al schema y no solo a paquetes

El schema es un contrato público consumido por código que no controlamos (sistemas cliente de terceros corriendo SDKs desplegados). Semver es el vocabulario universalmente entendido para promesas de compatibilidad. Inventar un esquema custom significa que cada consumidor tiene que aprenderlo.

### Por qué estricto-en-MAJOR / permisivo-en-MINOR

Esto coincide con la promesa de semver: bumps MAJOR explícitamente señalan "debes actualizar para seguir funcionando", bumps MINOR explícitamente señalan "tu código sigue funcionando". Rechazar duro los mismatches MAJOR fuerza upgrades que de otro modo serían inseguros; aceptar MINOR con logging surface staleness sin romper producción.

### Por qué una ventana de deprecación de dos MINORs / seis meses

Una sola ventana MINOR es demasiado corta — los consumidores pueden ni notar la deprecación antes de que se vaya. Ventanas indefinidas saturan el schema. Dos MINORs / seis meses coincide con la cadencia a la que típicamente sistemas cliente refrescan sus dependencias de SDK en entornos regulados.

### Por qué preservar campos desconocidos en lugar de stripearlos

Stripear campos desconocidos rompe la serialización round-trip — si el SDK cachea el grafo y luego lo re-emite (para diagnostic dumps, debugging), el grafo cacheado difiere silenciosamente de lo que el servidor envió. Preservar con `extensions` mantiene integridad round-trip sin otorgarle peso de autorización a los campos desconocidos.

### Por qué la matriz de compatibilidad es un doc mantenido y no solo código

Ingenieros de soporte, partners de integración y revisores de seguridad preguntan "¿qué funciona con qué?" vía canales de documentación, no leyendo fixtures de test. Una matriz mantenida es la respuesta de menor fricción. El mantenimiento por CI previene drift de la realidad.

---

## Consecuencias

### Positivas

- La evolución del schema es gobernable: cada cambio tiene una implicación clara de compatibilidad comunicada por el bump de versión.
- Clientes corriendo SDKs viejos siguen funcionando a través de bumps MINOR con warnings estructurados en lugar de breakage silencioso.
- La matriz de compatibilidad da a operaciones y soporte una única fuente de verdad.
- La deprecación es un ciclo de vida explícito, no una sorpresa — los clientes tienen tiempo de migrar antes de que un campo desaparezca.

### Compromisos

- Agregar un campo al grafo deja de ser una acción gratuita — cada cambio server-side requiere una decisión de bump de schema, incluso cuando ergonómicamente el cambio es trivial.
- El bag `extensions` para campos desconocidos agrega overhead de memoria por instancia de grafo (insignificante en la práctica pero presente).
- Mantener la matriz de compatibilidad es trabajo de documentación que crece linealmente con releases. La automatización por CI mitiga pero no elimina esto.

### Neutrales

- ADR-0071 (Auth Graph Engine) no cambia — describió la estructura; este ADR gobierna su evolución.
- `schemaVersion` se vuelve un nuevo campo requerido pero es una sola adición de string — sin impacto estructural en consumidores existentes (después de un bump MAJOR a 1.0.0).

---

## Implementación

| Componente | Ubicación | Owner |
|---|---|---|
| `auth-graph.schema.json` con constante `schemaVersion` | `src/libs/sdk/contracts/` | Arquitectura |
| `SCHEMA_VERSIONING.md` resumen developer | `src/libs/sdk/contracts/` | Arquitectura |
| `compatibility-matrix.md` (EN + ES) | `docs/sdk/contracts/` | Arquitectura + Release |
| Servidor emite `schemaVersion` en el grafo | `Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs` | Backend |
| Metadata del SDK declara `umsSchemaCompatibility` | Cada paquete SDK | Equipo SDK |
| SDK rechaza mismatch MAJOR, advierte en MINOR | `Ums.Sdk.Authorization`, `@ums/sdk-authorization` | Equipo SDK |
| Eventos logeados: `UnknownFieldsObservedEvent`, `SchemaMinorMismatchEvent`, `DeprecatedFieldUsageEvent` | Todos los SDKs | Equipo SDK |
| Job CI: round-trip de cada fixture por cada SDK | `.github/workflows/sdk-contract-validation.yml` | Plataforma |

Release inicial: schema `1.0.0`. La estructura actual del grafo documentada en [`auth-graph.md` §8](../../domain-es/identity/auth-graph.md#8-ejemplo-json-de-respuesta) es el schema canónico 1.0.0.

---

**[Registro ADR](./index.md)**
