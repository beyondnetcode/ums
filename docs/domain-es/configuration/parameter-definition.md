# ParameterDefinition — Arquitectura de Agregado

**Contexto Delimitado:** Configuración  
**Raíz de Agregado:** `ParameterDefinition`  
**Módulo:** `Ums.Domain.Configuration.Parameter`  
**Estado:** Producción

---

`ParameterDefinition` define el esquema canónico de un parámetro configurable. Controla el `code`, el tipo de valor permitido, la descripción funcional y el scope donde puede existir.

### Responsabilidad de Negocio
- Definir el contrato del parámetro.
- Establecer el significado funcional del valor.
- Proteger la consistencia del catálogo de configuración.

### Invariantes
- El `code` debe ser único dentro de su scope.
- El `DataType` determina qué valores son válidos en `ParameterGlobalValue` y `ParameterTenantValue`.
- Los overrides por tenant solo son válidos cuando el scope lo permite.
- La definición no puede archivarse si aún existen valores globales o de tenant activos.
