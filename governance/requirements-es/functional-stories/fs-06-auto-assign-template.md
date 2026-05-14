# Functional Story 6: Auto-Asignar Plantilla de Autorización al Crear Perfil

## 1. Propósito de Negocio

UMS debe reducir administración manual asignando la plantilla correcta cuando un nuevo perfil coincide con reglas de negocio aprobadas.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Seguridad** | Configura reglas de asignación. |
| **Motor de Reglas UMS** | Aplica reglas coincidentes durante la creación del perfil. |

## 3. Precondiciones de Negocio

- Existe al menos una regla de asignación activa.
- El perfil creado contiene atributos evaluables.
- Existe una plantilla coincidente activa.

## 4. Flujo Funcional Principal

1. El administrador define una regla que vincula atributos de perfil con una plantilla.
2. Se crea un nuevo perfil.
3. UMS evalúa el perfil contra reglas de asignación activas.
4. Si una regla coincide, UMS asigna automáticamente la plantilla correspondiente.
5. El perfil queda marcado como asignado automáticamente.
6. Los usuarios afectados reciben los permisos resultantes.

## 5. Flujos Alternativos y Excepciones

### A. Ninguna Regla Coincide

Si ninguna regla activa coincide con el perfil, el perfil queda sin asignación automática y puede gestionarse manualmente.

### B. Múltiples Reglas Coinciden

Si más de una regla coincide, UMS aplica la regla de mayor prioridad y registra por qué fue seleccionada.

## 6. Reglas de Negocio

1. La asignación automática debe ser explicable.
2. La prioridad de reglas debe ser determinística.
3. La asignación manual permanece disponible cuando la automatización no coincide.
4. Las asignaciones automáticas deben ser auditables.

## 7. Criterios de Aceptación

1. Una regla coincidente asigna una plantilla automáticamente.
2. Un perfil sin regla coincidente queda disponible para asignación manual.
3. La prioridad resuelve múltiples coincidencias consistentemente.
4. La razón de asignación es visible para administradores.

## 8. Requisitos Técnicos

- Evaluar reglas activas durante la creación del perfil.
- Persistir estado de asignación en la relación perfil/plantilla.
- Invalidar caché del grafo de autorización para usuarios afectados.
- Emitir evento de auditoría con `auto: true` y referencia de regla seleccionada.

## 9. Trazabilidad

- Entidades: `PROFILE`, `PERMISSION_TEMPLATE`, `PROFILE_PERMISSION`
- ADRs: ADR-0042, ADR-0043, ADR-0035
- Technical Enabler: TE-01
