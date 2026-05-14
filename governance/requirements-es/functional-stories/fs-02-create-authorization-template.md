# Functional Story 2: Crear e Instanciar Plantilla de Autorización

## 1. Propósito de Negocio

Los administradores necesitan plantillas de autorización reutilizables para gobernar patrones comunes de acceso de forma consistente entre perfiles y sistemas. UMS debe permitir crear, versionar y asignar plantillas sin redefinir permisos manualmente para cada perfil.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador Global de TI** | Crea y mantiene plantillas reutilizables de autorización. |
| **Administrador de Tenant/Sistema** | Asigna plantillas aprobadas a perfiles cuando estáá permitido. | ## 3. Precondiciones de Negocio

- La topología del sistema destino estáá registrada.
- Las acciones disponibles estáán definidas.
- El administrador tiene permiso para gestionar plantillas.

## 4. Flujo Funcional Principal

1. El administrador abre el gestáor de plantillas.
2. El administrador crea una nueva plantilla con nombre, versión y propósito.
3. El administrador selecciona los sistemas, menús, opciones y acciones que la plantilla permitirá o denegará.
4. El sistema valida que los permisos seleccionados pertenezcan a recursos registrados válidos.
5. El administrador publica la plantilla.
6. El administrador asigna la plantilla a un perfil existente o nuevo.
7. Los usuarios vinculados a ese perfil reciben el comportamiento de autorización efectivo desde la plantilla asignada.

## 5. Flujos Alternativos y Excepciones

### A. Selección de Permiso Inválida

Si el administrador selecciona un recurso o acción inválida, el sistema impide la publicación y explica qué debe corregirse.

### B. Actualización de Plantilla Incompatible

Si una nueva versión de plantilla entra en conflicto con sobrescrituras locales en perfiles, el sistema advierte al administrador y exige confirmación explícita antes de aplicar el cambio.

## 6. Reglas de Negocio

1. Las plantillas deben estáar versionadas.
2. Las plantillas solo deben referenciar recursos registrados válidos.
3. La asignación de plantillas debe ser auditable.
4. Los cambios que afectan usuarios existentes deben ser trazables.

## 7. Criterios de Aceptación

1. Un administrador puede crear y publicar una plantilla válida.
2. Los recursos inválidos no pueden incluirse en una plantilla.
3. Una plantilla puede asignarse a un perfil.
4. Los cambios de plantilla quedan auditables y trazables.

## 8. Requisitos Técnicos

- Persistir plantillas y permisos usando `PERMISSION_TEMPLATE`, `PROFILE` y `PROFILE_PERMISSION`.
- Validar integridad de jerarquía de recursos antes de publicar.
- Invalidar caché del grafo de autorización compilado para usuarios afectados tras asignación o actualización.
- Emitir eventos de auditoría por creación, publicación, asignación y cambio de versión.
- Preservar linaje de versionado semántico.

## 9. Trazabilidad

- Entidades: `PERMISSION_TEMPLATE`, `PROFILE`, `PROFILE_PERMISSION`, `ACTION`
- ADRs: ADR-0039, ADR-0042, ADR-0021
- Technical Enabler: TE-01
