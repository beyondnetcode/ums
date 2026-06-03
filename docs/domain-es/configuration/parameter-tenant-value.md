# ParameterTenantValue — Arquitectura de Agregado

**Contexto Delimitado:** Configuración  
**Raíz de Agregado:** `ParameterTenantValue`  
**Módulo:** `Ums.Domain.Configuration.Parameter`  
**Estado:** Producción

---

`ParameterTenantValue` almacena el override específico de un tenant. Solo puede existir cuando la política del sistema permite sobreescribir el valor global.

### Responsabilidad de Negocio
- Ajustar el valor de un parámetro para un tenant específico.
- Respetar la precedencia frente al valor global.

### Invariantes
- Debe referenciar una `ParameterDefinition`.
- El valor debe ser compatible con el `DataType` de la definición.
- El `ParameterDefinition` debe permitir overrides por tenant.
