# Functional Story 13: Configurar Parámetros Jerárquicos del Sistema

## 1. Propósito de Negocio

Los administradores necesitan configurar el comportamiento del sistema sin solicitar un despliegue cada vez que un tenant, sistema o módulo requiere una regla operativa distinta. UMS debe permitir configuración controlada en múltiples alcances, preservando reglas claras de herencia y sobrescritura.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador Global** | Define valores por defecto aplicables a toda la plataforma. |
| **Administrador de Tenant** | Ajusta comportamiento para un tenant cuando la política global lo permite. |
| **Administrador de Sistema** | Ajusta comportamiento para un sistema o suite registrada. |
| **Sistema Cliente** | Consume la configuración efectiva resuelta por UMS.
## 3. Precondiciones de Negocio

- El administrador estáá autenticado.
- El administrador tiene permiso para gestionar configuración en el alcance seleccionado.
- El tenant, sistema o módulo destino estáá registrado y activo.

---

## 4. Flujo Funcional Principal

1. El administrador abre el panel de configuración.
2. El administrador selecciona el alcance donde aplicará el parámetro: Global, Tenant, System/Suite o Module.
3. El administrador registra el identificador del parámetro, su valor y una descripción clara de su propósito de negocio.
4. El administrador decide si niveles inferiores podrán sobrescribir el valor.
5. El sistema verifica si una restricción superior impide el cambio solicitado.
6. El sistema guarda el parámetro y lo deja disponible para resolver la configuración efectiva.
7. Los sistemas cliente reciben el valor más específico aplicable según la jerarquía.

---

## 5. Flujos Alternativos y Excepciones

### A. Sobrescritura No Permitida

Si un administrador de nivel superior marcó el parámetro como no sobrescribible, el sistema impide el cambio inferior e informa qué regla superior controla el comportamiento.

### B. Descripción Funcional Faltante

Si el administrador no proporciona una descripción significativa, el sistema impide la publicación y solicita documentar propósito, impacto, comportamiento esperado y alcance aplicable.

### C. Parámetro Duplicado en el Mismo Alcance

Si ya existe un parámetro con el mismo identificador en el alcance seleccionado, el sistema evita la duplicidad y orienta al administrador a editar el parámetro existente o crear un cambio versionado.

---

## 6. Reglas de Negocio

1. Gana el valor más específico aplicable: Module sobre System/Suite, System/Suite sobre Tenant, Tenant sobre Global.
2. Un valor no sobrescribible bloquea cambios en alcances inferiores.
3. Todo parámetro configurable debe incluir `code`, `value` y `description`.
4. La descripción debe explicar propósito, impacto funcional, comportamiento esperado y alcance.
5. Los valores sensibles deben tratarse como configuración protegida y no exponerse en texto plano a usuarios no autorizados.

---

## 7. Criterios de Aceptación

1. Un administrador puede crear un parámetro en un alcance permitido.
2. El sistema impide sobrescrituras inferiores cuando una regla superior las bloquea.
3. El valor efectivo entregado al sistema cliente respeta las reglas de jerarquía.
4. Un parámetro no puede publicarse sin una descripción funcional clara.
5. No se permiten identificadores duplicados dentro del mismo alcance.

---

## 8. Requisitos Técnicos

- Persistir configuraciones en `APP_CONFIGURATION`.
- Campos obligatorios: `Code`, `Value`, `Description`.
- Aplicar unicidad por alcance mediante `UX_APP_CONFIGURATION_CODE_SCOPE`.
- Soportar linaje de versiones, auditoría, eventos de trazabilidad, cacheabilidad e invalidación.
- Los parámetros sensibles deben soportar valores cifrados.
- La configuración efectiva puede cachearse, pero debe invalidarse cuando cambia un parámetro.

---

## 9. Trazabilidad

- Entidades: `APP_CONFIGURATION`, `TENANT`, `SYSTEM_SUITE`, `FUNCTIONAL_MODULE`
- ADRs: ADR-0024, ADR-0047
- Technical Enabler: TE-02 Resolve Hierarchical System Configuration
- Estándar: Estándar de Redacción de Historias Funcionales; Estándar de Catálogos Paramétricos
