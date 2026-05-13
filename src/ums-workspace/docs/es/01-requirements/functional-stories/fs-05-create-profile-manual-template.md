# ðŸ§ª Functional Story 5: Crear Perfil y Asignar Manualmente Plantilla de AutorizaciÃ³n

Este caso de uso especifica el flujo para crear un Perfil de usuario dentro de un contexto de OrganizaciÃ³n/Sede y **asignar manualmente** una o mÃ¡s Plantillas de AutorizaciÃ³n a este a travÃ©s de la Consola de AdministraciÃ³n.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Crear Perfil y Asignar Manualmente Plantilla de AutorizaciÃ³n |
| **Actor Principal** | Administrador de Seguridad Global (SuperAdmin) o Gestor de Operaciones de Tenant (LocalAdmin) |
| **Precondiciones** | La OrganizaciÃ³n destino, Sede y al menos una Plantilla de AutorizaciÃ³n estÃ¡n registradas en el sistema. |
| **Postcondiciones** | El Perfil es creado y se encuentra activo. La Plantilla es vinculada. Todos los usuarios asignados a este perfil heredan el grafo de permisos compilado inmediatamente. Las claves de cachÃ© de Redis para los usuarios afectados son desalojadas. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador de Seguridad
    participant Console as Consola Admin UMS
    participant API as API .NET 8 UMS
    participant DB as PostgreSQL
    participant Cache as CachÃ© Redis
    participant Audit as Registro de AuditorÃ­a

    Admin->>Console: Navegar a Perfiles > Crear Nuevo Perfil
    Admin->>Console: Seleccionar OrganizaciÃ³n, Sede (opcional), ingresar nombre
    Console->>API: POST /api/v1/profiles { organization_id, branch_id, name }
    API->>DB: Insertar registro PROFILE (auto_assigned = false)
    API->>Audit: Registrar Evento ProfileCreatedEvent
    API-->>Console: 201 Created { profileId }
    Admin->>Console: Navegar a Perfil > Plantillas > Asignar Plantilla
    Admin->>Console: Seleccionar plantilla de la lista (ej. SCM_Analyst_Baseline_v1)
    Console->>API: POST /api/v1/profiles/{profileId}/templates { template_id }
    API->>DB: Vincular TEMPLATE a PROFILE
    API->>Cache: Desalojar todas las claves de cachÃ© del grafo para usuarios en este perfil
    API->>Audit: Registrar Evento TemplateAssignedEvent { profileId, templateId, assignedBy, manual: true }
    API-->>Console: 200 OK - Plantilla vinculada, cachÃ© desalojada
    Console-->>Admin: Mostrar notificaciÃ³n de Ã©xito
```

### A. Flujo Principal
1. El Administrador navega al mÃ³dulo de **Perfiles** y hace clic en **Crear Nuevo Perfil**.
2. Selecciona la OrganizaciÃ³n de destino (obligatorio) y opcionalmente una Sede para establecer el contexto. Ingresa un nombre descriptivo para el perfil (ej. `TransportationAnalyst_Callao`).
3. El Perfil se crea y se guarda con `auto_assigned = false`.
4. El Administrador navega al panel de **AsignaciÃ³n de Plantillas** del perfil y selecciona una o mÃ¡s Plantillas de AutorizaciÃ³n disponibles en un menÃº desplegable con bÃºsqueda.
5. La API persiste el enlace de la plantilla, desaloja todas las claves de la cachÃ© de Redis correspondientes a los grafos de los usuarios actualmente asignados a este perfil, y escribe un registro inmutable en auditorÃ­a marcado como `manual: true`.
6. Todos los usuarios en el perfil reciben inmediatamente el grafo de permisos actualizado en su prÃ³xima solicitud (un fallo en cachÃ© fuerza la recompilaciÃ³n).

---

## ðŸ›¡ï¸ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Conflicto en la VersiÃ³n de la Plantilla
- Si la versiÃ³n de la plantilla seleccionada introduce reglas de DENEGACIÃ“N explÃ­citas (DENY) que entran en conflicto con las entradas de PERMITIR (ALLOW) personalizadas localmente en el perfil, la Consola muestra una advertencia de compatibilidad requiriendo la confirmaciÃ³n del administrador antes de persistir los cambios.

### Flujo Alternativo B: El Perfil ya tiene una Plantilla
- Si el perfil ya tiene asignada una plantilla activa, la nueva asignaciÃ³n **reemplaza** a la anterior tras la confirmaciÃ³n. El enlace de la plantilla previa es archivado en el registro de auditorÃ­a.

### Flujo Alternativo C: Sin Usuarios Activos Afectados
- Si no hay usuarios asignados actualmente al perfil, la plantilla se vincula de forma inmediata sin requerir desalojo de cachÃ©. De igual manera se escribe el registro de auditorÃ­a.
