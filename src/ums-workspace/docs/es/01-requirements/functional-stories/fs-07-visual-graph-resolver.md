# ðŸ§ª Functional Story 7: Diagnosticar Permisos vÃ­a Visualizador de Grafos

Este caso de uso especifica el flujo para que los ingenieros de SRE y los administradores de seguridad diagnostiquen y visualicen el grafo de autorizaciÃ³n compilado para un usuario especÃ­fico dentro de una organizaciÃ³n, sede y contexto de sistema objetivo.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Diagnosticar Permisos vÃ­a Visualizador de Grafos |
| **Actor Principal** | SRE / Ingeniero de Soporte |
| **Precondiciones** | El actor estÃ¡ autenticado con rol SRE o SuperAdmin en la Consola Admin UMS. El usuario destino existe y tiene al menos un perfil asignado. |
| **Postcondiciones** | El grafo de autorizaciÃ³n compilado es renderizado visualmente. El actor puede identificar rutas permitidas (verde), denegadas (rojo) y la razÃ³n de cada decisiÃ³n. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

```mermaid
sequenceDiagram
    autonumber
    actor SRE as SRE / Ingeniero Soporte
    participant Console as Consola Admin UMS
    participant API as API .NET 8 UMS
    participant Engine as Motor Auth (PDP)
    participant Cache as CachÃ© Redis
    participant DB as PostgreSQL

    SRE->>Console: Navegar al mÃ³dulo Visualizador de Grafos
    SRE->>Console: Buscar usuario por email o user_id
    Console->>API: GET /api/v1/users?email=analyst@logisticscorp.com
    API-->>Console: Retornar registro de usuario coincidente
    SRE->>Console: Seleccionar OrganizaciÃ³n, Sede y Sistema Objetivo
    Console->>API: POST /v1/authorization/graph/diagnostic { user_id, tenant_id, branch_id, system_id }
    Note over API: Omite cachÃ© para diagnÃ³stico - siempre re-compila desde DB
    API->>Engine: Disparar compilaciÃ³n fresca del grafo
    Engine->>DB: Consultar todos los perfiles y plantillas para el usuario
    Engine->>Engine: Aplicar reglas de Precedencia de DenegaciÃ³n ExplÃ­cita
    Engine-->>API: Retornar grafo anotado con razones de decisiÃ³n por nodo
    API-->>Console: Retornar payload diagnÃ³stico { graph, decisions, source_rules }
    Console->>SRE: Renderizar Ãrbol Visual interactivo
    Note over Console: Verde = PERMITIDO, Rojo = DENEGADO, Gris = No Asignado
    SRE->>Console: Clic en nodo para ver razÃ³n de decisiÃ³n
    Console->>SRE: Mostrar fuente de regla (template_id, profile_id, efecto, razÃ³n)
```

### A. Flujo Principal
1. El SRE navega al mÃ³dulo **Visualizador de Grafos** en la Consola de AdministraciÃ³n.
2. Escribe el correo del usuario o `user_id` en el buscador. El sistema devuelve los registros de usuarios coincidentes.
3. Selecciona la **OrganizaciÃ³n**, **Sede** y **Sistema** a travÃ©s de listas desplegables en cascada.
4. Hace clic en **Resolver Grafo**. La API llama al **endpoint de diagnÃ³stico** del Motor de AutorizaciÃ³n, el cual **siempre ignora la cachÃ© de Redis** y recompila directamente desde PostgreSQL para mostrar el estado actual real (ground-truth).
5. El motor devuelve un grafo anotado que incluye, para cada nodo de MenÃº/SubmenÃº/OpciÃ³n/AcciÃ³n:
    - El `efecto` (`ALLOW`, `DENY` o `NOT_ASSIGNED`).
    - La `fuente de regla` (`template_id` o `profile_id` que produjo el efecto).
    - La `razÃ³n` (ej., `Concedido por Template_SCM_Analyst_Baseline_v1`, `Bloqueado por DENY explÃ­cito en perfil PortSupervisor_Callao`).
6. La Consola renderiza el Ã¡rbol interactivamente: **nodos verdes** para PERMITIR (ALLOW), **nodos rojos** para DENEGAR (DENY), **nodos grises** para NO ASIGNADO.
7. El SRE puede hacer clic en cualquier nodo para expandir su panel de justificaciÃ³n de decisiÃ³n.

---

## ðŸ›¡ï¸ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Usuario sin Asignaciones de Perfil
- Si el usuario no tiene perfiles activos para el contexto seleccionado, el Ã¡rbol se renderiza completamente gris con el mensaje: *"No se encontraron asignaciones de perfil activas para este usuario en el contexto seleccionado. Asigne un perfil o plantilla para conceder acceso."*

### Flujo Alternativo B: Sede no Seleccionada (Alcance de Toda la OrganizaciÃ³n)
- Si el campo sede se deja en blanco, el diagnÃ³stico resuelve permisos a nivel de toda la organizaciÃ³n, excluyendo sobreescrituras (overrides) de perfiles con alcance especÃ­fico de sede.

### Flujo Alternativo C: Presencia de PolÃ­tica de Geocercado (Geofencing)
- Si el grafo compilado contiene metadatos de geocercado ABAC en cualquier nodo, el visualizador muestra la restricciÃ³n de geocercado en lÃ­nea (ej., `callao_port_radius_10km`) como una anotaciÃ³n informativa sin evaluar la ubicaciÃ³n en tiempo de ejecuciÃ³n.
