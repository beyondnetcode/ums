# ðŸ§ª Functional Story 4: Registrar Sistema y Definir TopologÃ­a de MenÃº

Este caso de uso especifica el flujo para registrar una nueva aplicaciÃ³n cliente (Sistema) en el UMS y definir su jerarquÃ­a de recursos de navegaciÃ³n (MenÃºs, SubmenÃºs, Opciones y Acciones).

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Registrar Sistema y Definir TopologÃ­a de MenÃº |
| **Actor Principal** | Administrador de Seguridad Global (SuperAdmin) |
| **Precondiciones** | El actor estÃ¡ autenticado como SuperAdmin en la Consola de AdministraciÃ³n UMS. |
| **Postcondiciones** | El Sistema se registra con una credencial de API M2M segura. La topologÃ­a de menÃº se define y estÃ¡ disponible para la asignaciÃ³n de plantillas. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador de Seguridad
    participant Console as Consola Admin UMS
    participant API as API .NET 8 UMS
    participant DB as PostgreSQL
    participant Audit as Registro de AuditorÃ­a

    Admin->>Console: Navegar a Sistemas > Registrar Nuevo Sistema
    Admin->>Console: Llenar nombre del sistema, cÃ³digo, URL base
    Console->>API: POST /api/v1/systems { name, system_code, base_url }
    API->>DB: Insertar registro SYSTEM, generar api_credential_hash
    API->>Audit: Registrar Evento SystemRegisteredEvent
    API-->>Console: 201 Created { systemId, apiCredential }
    Console-->>Admin: Mostrar credencial del sistema (una sola vez, pedir copiar)
    Admin->>Console: Navegar a Sistemas > TopologÃ­a > Agregar MenÃº
    loop Construir Ãrbol de MenÃºs
        Admin->>Console: Agregar MenÃº â†’ SubmenÃºs â†’ Opciones â†’ Acciones
        Console->>API: POST /api/v1/systems/{id}/menus (batch recursivo)
        API->>DB: Insertar registros MENU / SUBMENU / OPTION / ACTION
    end
    API-->>Console: ConfirmaciÃ³n de topologÃ­a guardada
```

### A. Flujo Principal
1. El SuperAdmin navega a **Sistemas** y hace clic en **Registrar Nuevo Sistema**.
2. Llena el nombre del sistema (`SCM Route Planner`), cÃ³digo de mÃ¡quina (`scm_route_planner`) y la URL base.
3. La API genera una credencial de API M2M Ãºnica y hasheada que las aplicaciones cliente utilizarÃ¡n en los encabezados `Authorization: Bearer` al llamar a `POST /v1/authorization/graph`. Esta credencial se muestra **una sola vez** y debe ser guardada.
4. El administrador navega al **Constructor de TopologÃ­a** para el sistema registrado y construye el Ã¡rbol de navegaciÃ³n: `MenÃºs â†’ SubmenÃºs â†’ Opciones â†’ Acciones`.
5. Cada nodo especifica una etiqueta, un Ã­ndice de orden y (para las Acciones) un mapeo de endpoint de la API y cÃ³digo de acciÃ³n (`create`, `read`, `update`, `delete`, `export`, `approve`).

---

## ðŸ›¡ï¸ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: CÃ³digo de Sistema Duplicado
- Si el `system_code` ya existe, la API devuelve un `409 Conflict` con el cÃ³digo de error `ERR_DUPLICATE_SYSTEM_CODE`.

### Flujo Alternativo B: TopologÃ­a Incompleta
- Si una OpciÃ³n no tiene Acciones definidas, la topologÃ­a se guarda como borrador pero no puede ser referenciada en Plantillas de AutorizaciÃ³n hasta que se vincule al menos una AcciÃ³n.
