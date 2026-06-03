# ADR-0078: Jerarquía DDD de Recursos de Dominio — Aggregate, Entity, DomainMethod

**Estado:** Aceptado  
**Fecha:** 2026-06-03  
**Responsable:** Arquitectura  
**Relacionados:**
- [ADR-0071: Motor de Auth Graph](./0071-auth-graph-engine.md)
- [ADR-0074: Versionado de Schema del Auth Graph](./0074-auth-graph-schema-versioning.md)

---

## Contexto

El sistema de autorización modela el control de acceso a nivel de dominio mediante objetos `DomainResource`. Inicialmente solo se soportaban los tipos `Aggregate` y `Entity`. El Diseño Orientado al Dominio (DDD) también define *DomainMethods* — operaciones expuestas por un Aggregate que contienen lógica de negocio y requieren permisos explícitos (e.g. `ResetPassword()`, `ApproveOrder()`).

Surgieron tres preguntas de diseño:

1. ¿Debe `DomainMethod` ser un tipo de entidad separado o extender `DomainResource`?
2. ¿Debe la relación padre–hijo usar una tabla de unión o una columna `ParentResourceId`?
3. ¿Dónde deben aplicarse las reglas de jerarquía DDD (dominio, aplicación o persistencia)?

---

## Decisión

### 1. DomainMethod como valor de `DomainResourceType`

`DomainMethod` se agrega como tercer valor del enum `DomainResourceType` existente (`DomainMethod = new(3, ...)`), no como entidad separada. Una fila de `DomainResource` puede ser ahora `Aggregate(1) | Entity(2) | DomainMethod(3)`.

| Opción | Razón de rechazo |
|---|---|
| Entidad `DomainMethod` separada | Duplica rutas de consulta y complica el constructor del auth graph |
| Acción personalizada en el Aggregate | Pierde la direccionabilidad necesaria para el auth graph |
| Extensión mediante tabla de unión | Sobre-ingenierado para una jerarquía de dos niveles |

### 2. `ParentResourceId` en la misma tabla

Una columna nullable `Guid? ParentResourceId` en `DOMAIN_RESOURCE` provee la referencia al padre. Esto evita una tabla de unión manteniendo la jerarquía consultable en una sola lectura.

Reglas de restricción aplicadas por el agregado `SystemSuite`:
- Un `DomainMethod` **debe** tener `ParentResourceId` no nulo.
- El recurso padre **no puede** ser un `DomainMethod` (evita anidamiento más allá de un nivel).
- El recurso padre **debe** existir dentro del mismo `SystemSuite`.
- Una `Entity` puede opcionalmente referenciar un `Aggregate` padre; esto no se aplica como regla estricta.

### 3. Reglas aplicadas en el agregado de dominio

Todos los invariantes de jerarquía se aplican dentro de `SystemSuite.AddDomainResource(moduleId, parentResourceId, type, ...)`. La capa de aplicación pasa el `parentResourceId` resuelto y el agregado lo valida. Esto mantiene las reglas cerca del dato y verificables sin base de datos.

### Impacto en el auth graph

El constructor del auth graph (`IAuthorizationGraphBuilder`) incluye nodos `DomainMethod` como objetivos de permiso direccionables, vinculados a su Aggregate o Entity padre. Los templates de permisos ahora pueden otorgar acceso a operaciones de dominio específicas, no solo a recursos completos.

---

## Consecuencias

- **Positivo:** Una tabla, una ruta de consulta, extensión limpia del enum.
- **Positivo:** El auth graph puede representar permisos granulares a nivel de método.
- **Negativo:** Límite de un nivel de profundidad — si se necesita mayor anidamiento, este ADR debe revisarse.
- **Neutral:** Las filas existentes de `Aggregate` y `Entity` tienen `ParentResourceId = null`; no se requiere cambio en datos de migración.
