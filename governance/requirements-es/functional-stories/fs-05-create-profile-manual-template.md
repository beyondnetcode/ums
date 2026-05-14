# Functional Story 5: Crear Perfil y Asignar Manualmente Plantilla de Autorización

## 1. Propósito de Negocio

Los administradores necesitan crear perfiles que representen responsabilidades reales de trabajo y adjuntar manualmente plantillas aprobadas cuando la asignación automática no corresponde.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Seguridad** | Crea perfiles y asigna plantillas. |
| **Gestor de Operaciones de Tenant** | Administra perfiles locales dentro de su alcance delegado. |

## 3. Precondiciones de Negocio

- La organización destino existe.
- La sede destino existe cuando se requiere alcance por sede.
- Existe al menos una plantilla de autorización disponible.

## 4. Flujo Funcional Principal

1. El administrador abre la gestión de perfiles.
2. El administrador crea un perfil con nombre, organización y alcance opcional por sede.
3. El sistema crea el perfil como perfil gestionado manualmente.
4. El administrador selecciona una o más plantillas aprobadas.
5. El sistema vincula las plantillas seleccionadas al perfil.
6. Los usuarios asignados al perfil reciben los permisos efectivos derivados de esas plantillas.

## 5. Flujos Alternativos y Excepciones

### A. Conflicto de Plantilla

Si la plantilla seleccionada entra en conflicto con reglas existentes del perfil, el sistema advierte al administrador y solicita confirmación.

### B. Reemplazo de Plantilla

Si el perfil ya tiene una plantilla activa, el administrador debe confirmar si la nueva plantilla reemplaza o complementa la existente.

## 6. Reglas de Negocio

1. Un perfil debe pertenecer a una organización.
2. El alcance por sede es opcional pero debe ser explícito cuando se usa.
3. La asignación manual de plantillas debe ser trazable.
4. Las reglas de denegación explícita tienen prioridad sobre permisos permitidos.

## 7. Criterios de Aceptación

1. Un administrador puede crear un perfil en una organización permitida.
2. Una plantilla aprobada puede asignarse manualmente al perfil.
3. Las asignaciones conflictivas requieren confirmación del administrador.
4. Los usuarios afectados reciben permisos efectivos actualizados.

## 8. Requisitos Técnicos

- Persistir perfiles en `PROFILE`.
- Vincular plantillas mediante `PROFILE_PERMISSION` / relación de asignación de plantilla.
- Invalidar caché del grafo de autorización para usuarios afectados.
- Emitir `ProfileCreatedEvent` y `TemplateAssignedEvent`.
- Preservar metadata de asignación, incluyendo si fue manual.

## 9. Trazabilidad

- Entidades: `PROFILE`, `PERMISSION_TEMPLATE`, `PROFILE_PERMISSION`
- ADRs: ADR-0039, ADR-0042, ADR-0043, ADR-0035
- Technical Enabler: TE-01
