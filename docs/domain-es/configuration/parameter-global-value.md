# ParameterGlobalValue — Arquitectura de Agregado

**Contexto Delimitado:** Configuración  
**Raíz de Agregado:** `ParameterGlobalValue`  
**Módulo:** `Ums.Domain.Configuration.Parameter`  
**Estado:** Producción

---

`ParameterGlobalValue` almacena el valor base global de un parámetro. Actúa como valor por defecto para todos los tenants salvo override explícito.

### Responsabilidad de Negocio
- Publicar el valor global del parámetro.
- Servir como baseline para resolución jerárquica.

### Invariantes
- Debe referenciar una `ParameterDefinition`.
- El valor debe ser compatible con el `DataType` de la definición.
- No puede archivarse si todavía existen overrides activos por tenant.
