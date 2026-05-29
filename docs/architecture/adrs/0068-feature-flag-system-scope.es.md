# ADR-0068: Alcance del Sistema de Feature Flags — Propiedad de SystemSuite y Modelo de Criterios Dinámicos

**Estado:** Aceptado
**Fecha:** 2026-05-27
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [FeatureFlag Domain](../../domain/configuration/feature-flag.md)
- [FeatureFlagCriteria Model](../../domain/configuration/feature-flag-criteria.md)
- [BC Configuration Context](../../governance/construction/ddd-design/05-configuration-context.md)

---

## Contexto

El aggregate `FeatureFlag` original fue diseñado como un toggle global: `FlagCode` era único a través de toda la plataforma sin ningún scope de propiedad. Esto creó dos problemas operacionales:

1. **Sin aislamiento de sistema.** Un flag activado globalmente podía afectar a tenants o system suites que no deberían incluirse en un rollout específico. No había mecanismo para constrain un flag a una frontera de sistema particular.

2. **Modelo de targeting rígido.** La propiedad `FlagTargets` original era un string JSON de forma libre que describía reglas de targeting. Esto hacía imposible querying, validar, o evolucionar condiciones de targeting sin parsear payloads opacos. Agregar o remover una única condición de targeting requería reemplazar todo el blob JSON.

El Authorization bounded context (BC-B) ya modela `SystemSuite` como la unidad autoritativa de composición de features — un system suite agrupa modules, roles, y permissions para un producto o sistema específico. Los feature flags pertenecen naturalmente a la misma frontera: un flag para "habilitar nuevo módulo de exportación" solo tiene sentido dentro del suite que contiene el módulo de exportación.

Se necesitaba un refactoring para introducir `SystemSuite` como el scope obligatorio de cada feature flag y reemplazar el JSON de targeting opaco con un modelo de criterios estructurado y queryable.

---

## Decisión

### 1. FeatureFlag permanece como un Aggregate Root independiente en BC-C

`FeatureFlag` permanece como un Aggregate Root en el Configuration bounded context (`ums_configuration` schema). No se convierte en una entidad hija del aggregate `SystemSuite` en BC-B. El enlace entre los dos aggregates se expresa a través de una foreign key: `FeatureFlag.SystemSuiteId → ums_authorization.SystemSuites.Id`.

### 2. SystemSuiteId es el scope obligatorio e inmutable de cada flag

Cada `FeatureFlag` debe crearse con un `SystemSuiteId` válido. Este valor se valida contra BC-B en tiempo de creación y es inmutable después de eso. Un flag no puede transferirse de un system suite a otro; si se necesita un scope diferente, debe crearse un nuevo flag.

### 3. La unicidad de FlagCode cambia de global a (SystemSuiteId, FlagCode)

La restricción de unicidad global previa en `FlagCode` se reemplaza por una restricción de unicidad compuesta en `(SystemSuiteId, FlagCode)`. La misma string de código puede existir en diferentes system suites sin conflicto.

### 4. Los Criterios reemplazan el JSON opaco de FlagTargets

Una nueva entidad propia `FeatureFlagCriteria` reemplaza el JSON `FlagTargets` de forma libre para propósitos de targeting. Cada criterio lleva:
- `CriteriaType` — la dimensión siendo evaluada (`TenantId`, `BranchId`, `UserProfileId`, `RoleCode`, `Environment`, `DateRange`, `PercentageHash`, `CustomRule`)
- `Operator` — la comparación a aplicar (`Equals`, `NotEquals`, `In`, `Between`, `LessThanOrEqual`, `Matches`)
- `Value` — el valor objetivo como string compatible con JSON

La colección de criterios es **opcional y dinámica**:
- Una colección vacía significa que el flag está activo para todos los llamadores en el sistema.
- Los criterios pueden agregarse o removerse independientemente sin modificar las propiedades del aggregate root.
- Cada cambio emite un evento de dominio discreto.

### 5. Semántica de evaluación: OR dentro del tipo, AND entre tipos

El port `IFeatureFlagEvaluator` evalúa la colección de criterios usando las siguientes reglas:
- **Dentro del mismo CriteriaType:** los criterios se combinan con lógica **OR**.
- **Entre grupos de CriteriaType diferentes:** los grupos se combinan con lógica **AND**.

Si el contexto de evaluación no provee los datos requeridos por un criterio, la evaluación retorna **`false`** (postura segura). Esto previene la activación inadvertida de features cuando el contexto está parcialmente poblado.

---

## Justificación

### Por qué FeatureFlag no es hijo de SystemSuite

Convertir `FeatureFlag` en una entidad hija del aggregate `SystemSuite` fue considerado pero rechazado por cuatro razones:

1. **Tamaño del aggregate.** `SystemSuite` ya posee modules, roles, y permission templates. Agregar una colección no acotada de feature flags crecería el aggregate a un tamaño inmanejable, dañando tiempos de carga e incrementando el riesgo de conflictos de concurrencia.

2. **Ciclo de vida independiente.** Los feature flags transicionan a través de sus propios estados (`Inactive → Active → Archived`) en un schedule驱动 por release management, no por el ciclo de vida del system suite. Archivar un flag no afecta al system suite; deshabilitar un system suite no archiva sus flags.

3. **Patrón existente.** `AppConfiguration` e `IdpConfiguration` en BC-C demuestran que los aggregates de configuración referencian identificadores de BC-B y BC-A como FKs sin convertirse en hijos de esos aggregates. `FeatureFlag` sigue el mismo patrón establecido.

4. **Separación de bounded context.** La gestión de feature flags es una responsabilidad de configuración (BC-C). Moverla a BC-B mergearía dos subdominios distintos y violaría el principio de responsabilidad única a nivel de bounded context.

### Por qué un modelo de criterios estructurado en lugar de JSON de forma libre

El JSON opaco `FlagTargets` hacía imposible:
- Query flags por condición de targeting (ej., "¿qué flags targetean al tenant T1?")
- Validar reglas de targeting en el límite de dominio
- Emitir eventos de dominio significativos cuando una regla de targeting cambia
- Agregar o remover una única condición sin reemplazar todo el payload

Una colección de entidad `FeatureFlagCriteria` estructurada aborda las cuatro preocupaciones al costo de un schema ligeramente más complejo.

### Por qué la postura segura es false en contexto faltante

Un valor de contexto ausente más comúnmente indica un llamador anónimo, un request de servicio sin contexto de tenant, o un cliente que aún no ha migrado para proporcionar el campo de contexto requerido. En todos los casos, activar una feature targeted para un llamador no identificado sería incorrecto. La regla false-on-missing-context asegura que nuevos tipos de criterios puedan agregarse a un flag live sin riesgo de activación broad inadvertida.

---

## Consecuencias

### Positivas

- Cada feature flag tiene un scope de propiedad explícito y queryable alineado con la frontera de sistema que controla.
- Las condiciones de targeting son individualmente direccionables: pueden queryarse, agregarse, removerse, y auditarse sin tocar el aggregate root.
- El modelo de criterios estructurado permite dashboards de "flags por tenant" o "flags por environment" sin parsing de JSON.
- La inmutabilidad de SystemSuiteId simplifica el razonamiento sobre el alcance de un flag — no hay riesgo de drift accidental de scope.
- Los eventos de dominio discretos por cambio de criterio permiten auditoría completa de cambios de targeting.

### Negativas

- El composite unique constraint `(SystemSuiteId, FlagCode)` requiere que los migrations validen duplicados antes de aplicar.
- El modelo de criterios con operadores definidos requiere que el `FeatureFlagEvaluator` implemente la lógica de evaluación completa — no es un simple JSON parse.
- La postura false-on-missing-context puede causar que flags activos no se activen si el llamador no proporciona contexto completo — requiere que los clientes proporcionen context fields completos.

### Neutrales

- `FeatureFlag` sigue siendo un aggregate root, no un entity hijo — el código existente que crea o usa `FeatureFlag` no requiere cambios de estructura.
- El foreign key `SystemSuiteId` ya existe en el schema; la restricción unique es el único cambio de schema requerido.

---

## Referencias

- [FeatureFlag Domain Documentación](../../domain/configuration/feature-flag.md)
- [FeatureFlagCriteria Model](../../domain/configuration/feature-flag-criteria.md)
- [BC Configuration Context DDD Design](../../governance/construction/ddd-design/05-configuration-context.md)