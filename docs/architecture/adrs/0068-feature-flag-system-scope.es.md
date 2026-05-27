# ADR-0068: Scope de Sistema para Feature Flags — Propiedad de SystemSuite y Modelo Dinámico de Criterios

**Estado:** Aceptado
**Fecha:** 2026-05-27
**Responsable de la Decisión:** Arquitectura
**Relacionado con:**
- [ADR-0054: Aislamiento de Shell Library](./0054-shell-library-isolation.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [Dominio FeatureFlag](../../domain/configuration/feature-flag.md)
- [Modelo FeatureFlagCriteria](../../domain/configuration/feature-flag-criteria.md)
- [Contexto de Configuración BC](../../governance/construction/ddd-design/05-configuration-context.md)

---

## Contexto

El agregado original `FeatureFlag` fue diseñado como un toggle global: `FlagCode` era único en toda la plataforma sin ningún scope de propiedad. Esto generó dos problemas operacionales:

1. **Sin aislamiento de sistema.** Una bandera activada globalmente podía afectar a tenants o system suites que no deberían estar incluidos en un rollout específico. No existía ningún mecanismo para limitar una bandera a un límite de sistema particular.

2. **Modelo de targeting rígido.** La propiedad original `FlagTargets` era una cadena JSON de forma libre que describía las reglas de targeting. Esto hacía imposible consultar, validar o evolucionar las condiciones de targeting sin parsear payloads opacos. Agregar o eliminar una única condición de targeting requería reemplazar todo el JSON.

El contexto delimitado de Autorización (BC-B) ya modela `SystemSuite` como la unidad autoritativa de composición de funcionalidades — un system suite agrupa módulos, roles y permisos de un producto o sistema específico. Las feature flags pertenecen naturalmente a ese mismo límite: una bandera para "habilitar el nuevo módulo de exportación" solo tiene sentido dentro del suite que contiene ese módulo.

Se requería una refactorización para introducir `SystemSuite` como el scope obligatorio de toda feature flag y reemplazar el JSON opaco de targeting con un modelo de criterios estructurado y consultable.

---

## Decisión

### 1. FeatureFlag permanece como Aggregate Root independiente en BC-C

`FeatureFlag` permanece como Aggregate Root en el contexto delimitado de Configuración (esquema `ums_configuration`). No se convierte en entidad hija del agregado `SystemSuite` en BC-B. El vínculo entre los dos agregados se expresa mediante una clave foránea: `FeatureFlag.SystemSuiteId → ums_authorization.SystemSuites.Id`.

### 2. SystemSuiteId es el scope obligatorio e inmutable de cada bandera

Todo `FeatureFlag` debe crearse con un `SystemSuiteId` válido. Este valor se valida contra BC-B en el momento de la creación y es inmutable a partir de entonces. Una bandera no puede transferirse de un system suite a otro; si se necesita un scope diferente, debe crearse una nueva bandera.

### 3. La unicidad de FlagCode cambia de global a (SystemSuiteId, FlagCode)

La restricción de unicidad global previa sobre `FlagCode` se reemplaza por una restricción compuesta sobre `(SystemSuiteId, FlagCode)`. La misma cadena de código puede existir en diferentes system suites sin conflicto.

### 4. Los criterios reemplazan el JSON opaco de FlagTargets

Una nueva entidad propia `FeatureFlagCriteria` reemplaza el JSON de forma libre de `FlagTargets` para los propósitos de targeting. Cada criterio contiene:
- `CriteriaType` — la dimensión a evaluar (`TenantId`, `BranchId`, `UserProfileId`, `RoleCode`, `Environment`, `DateRange`, `PercentageHash`, `CustomRule`)
- `Operator` — la comparación a aplicar (`Equals`, `NotEquals`, `In`, `Between`, `LessThanOrEqual`, `Matches`)
- `Value` — el valor objetivo como cadena compatible con JSON

La colección de criterios es **opcional y dinámica**:
- Una colección vacía significa que la bandera está activa para todos los llamantes del sistema.
- Los criterios pueden agregarse o eliminarse independientemente sin modificar las propiedades de la raíz de agregado.
- Cada cambio emite un evento de dominio discreto.

### 5. Semántica de evaluación: OR dentro del tipo, AND entre tipos

El puerto `IFeatureFlagEvaluator` evalúa la colección de criterios usando las siguientes reglas:
- **Dentro del mismo CriteriaType:** los criterios se combinan con lógica **OR**.
- **Entre diferentes grupos de CriteriaType:** los grupos se combinan con lógica **AND**.

Si el contexto de evaluación no provee el dato requerido por un criterio, la evaluación retorna **`false`** (postura segura). Esto previene la activación inadvertida de funcionalidades cuando el contexto está parcialmente poblado.

---

## Fundamentos

### Por qué FeatureFlag no es hijo de SystemSuite

Convertir `FeatureFlag` en una entidad hija del agregado `SystemSuite` fue considerado pero rechazado por cuatro razones:

1. **Tamaño del agregado.** `SystemSuite` ya posee módulos, roles y plantillas de permisos. Agregar una colección no acotada de feature flags haría crecer el agregado a un tamaño inmanejable, perjudicando los tiempos de carga e incrementando el riesgo de conflictos de concurrencia.

2. **Ciclo de vida independiente.** Las feature flags transicionan a través de sus propios estados (`Inactive → Active → Archived`) en un calendario impulsado por la gestión de releases, no por el ciclo de vida del system suite. Archivar una bandera no afecta al system suite; deshabilitar un system suite no archiva sus banderas.

3. **Patrón existente.** `AppConfiguration` e `IdpConfiguration` en BC-C demuestran que los agregados de configuración referencian identificadores de BC-B y BC-A como FKs sin convertirse en hijos de esos agregados. `FeatureFlag` sigue el mismo patrón establecido.

4. **Separación de contextos delimitados.** La gestión de feature flags es una responsabilidad de configuración (BC-C). Moverla a BC-B fusionaría dos subdominios distintos y violaría el principio de responsabilidad única a nivel de contexto delimitado.

### Por qué un modelo de criterios estructurado en lugar de JSON libre

El JSON opaco de `FlagTargets` hacía imposible:
- Consultar banderas por condición de targeting (por ejemplo, "¿qué banderas tienen como objetivo al tenant T1?")
- Validar las reglas de targeting en el límite del dominio
- Emitir eventos de dominio significativos cuando cambia una regla de targeting
- Agregar o eliminar una única condición sin reemplazar todo el payload

Una colección estructurada de entidades `FeatureFlagCriteria` resuelve los cuatro problemas al costo de un esquema ligeramente más complejo.

### Por qué la postura segura es false ante contexto faltante

Un valor de contexto ausente generalmente indica un llamante anónimo, una solicitud de servicio sin contexto de tenant, o un cliente que aún no ha migrado para proveer el campo de contexto requerido. En todos los casos, activar una funcionalidad con targeting para un llamante no identificado sería incorrecto. La regla false-ante-contexto-faltante garantiza que se puedan agregar nuevos tipos de criterios a una bandera en producción sin riesgo de activación amplia no intencionada.

---

## Consecuencias

### Positivas

- Cada feature flag tiene un scope de propiedad explícito y consultable, alineado con el límite del sistema que controla.
- Las condiciones de targeting son individualmente direccionables: pueden consultarse, agregarse, eliminarse y auditarse sin modificar la raíz del agregado.
- Los eventos de dominio para cambios de criterios proporcionan un rastro de auditoría detallado para cumplimiento normativo.
- El puerto `IFeatureFlagEvaluator` mantiene la lógica de evaluación extensible y testeable de forma aislada.
- El mismo código de bandera puede reutilizarse en diferentes system suites sin conflictos de nombres.

### Compromisos (Trade-offs)

- La FK entre esquemas (`ums_configuration → ums_authorization`) introduce un acoplamiento a nivel de base de datos entre dos contextos delimitados. Se trata de una restricción intencional que refuerza la integridad referencial en la capa de persistencia.
- Crear una bandera ahora requiere un `SystemSuiteId` válido, lo que significa que el llamante debe resolver la identidad del suite antes de emitir el comando.
- La colección de criterios introduce una nueva tabla (`FeatureFlagCriteria`) y requiere operaciones JOIN para lecturas completas de banderas. Se recomienda una proyección de modelo de lectura para caminos de evaluación de alta frecuencia.

---

## Alternativas Consideradas

### Alternativa A — FeatureFlag como entidad hija de SystemSuite

`FeatureFlag` se convertiría en una entidad dentro del agregado `SystemSuite`, gestionada a través de los comandos de `SystemSuite`.

**Rechazada.** Como se argumenta en la sección de Fundamentos, esto inflaría el agregado `SystemSuite`, acoplaría dos límites de ciclo de vida diferentes y violaría la separación de contextos delimitados. El patrón de agregado independiente ya está establecido para otras entidades de configuración en BC-C.

### Alternativa B — Servicio de feature flags externo (LaunchDarkly / Unleash)

Reemplazar el modelo de dominio interno con un proveedor SaaS de feature flags y exponerlo a través de la capa anti-corrupción `IFeatureFlagPort` ya definida en el mapa de contextos.

**Fuera de alcance.** La entrada ACL `IFeatureFlagPort` en el mapa de contextos delimitados contempla esta posibilidad. La decisión de mantener las feature flags de forma interna es una decisión de alcance de producto, no arquitectónica. Este ADR rige el diseño del modelo interno; la ruta de integración externa permanece disponible a través del puerto ACL sin requerir cambios en esta decisión.

---

## Mapeo de Implementación

| Concern | Ubicación |
|---|---|
| Raíz de agregado | `Ums.Domain.Configuration.FeatureFlag` |
| Entidad de criterios | `Ums.Domain.Configuration.FeatureFlagCriteria` |
| Puerto evaluador | `Ums.Domain.Configuration.Ports.IFeatureFlagEvaluator` |
| Implementación evaluador | `Ums.Infrastructure.Configuration.FeatureFlagEvaluator` |
| Tabla `FeatureFlags` | `ums_configuration.FeatureFlags` — AGREGAR `SystemSuiteId` FK, `TenantId`; CAMBIAR UK |
| Tabla `FeatureFlagCriteria` | `ums_configuration.FeatureFlagCriteria` (nueva tabla) |
| Nuevos comandos | `AddFeatureFlagCriteriaCommand`, `RemoveFeatureFlagCriteriaCommand`, `UpdateFeatureFlagCommand` |
| Comandos modificados | `CreateFeatureFlagCommand` (+ `SystemSuiteId`, `TenantId`), `EvaluateFeatureFlagCommand` (EvaluationContext tipado) |
| Nuevas queries | `GetFeatureFlagsBySystemSuiteQuery`, `GetFeatureFlagCriteriaQuery` |

---

**[Registro de ADRs](./index.md)** | **[Dominio FeatureFlag](../../domain/configuration/feature-flag.md)** | **[FeatureFlagCriteria](../../domain/configuration/feature-flag-criteria.md)**
