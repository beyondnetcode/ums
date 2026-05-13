# ðŸ§ª Functional Story 6: Auto-Asignar Plantilla de AutorizaciÃ³n al Crear Perfil

Este caso de uso especifica el flujo del **Motor AutomÃ¡tico de AsignaciÃ³n de Plantillas Basado en Reglas**, el cual se activa al momento de la creaciÃ³n de un perfil para adjuntar automÃ¡ticamente la Plantilla de AutorizaciÃ³n correspondiente segÃºn reglas de coincidencia configurables.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Auto-Asignar Plantilla de AutorizaciÃ³n al Crear Perfil |
| **Actor Principal** | Motor de Reglas UMS (Actor del Sistema â€” disparado automÃ¡ticamente) |
| **Actor Secundario** | Administrador de Seguridad Global (SuperAdmin â€” configura las reglas) |
| **Precondiciones** | Al menos una Regla de AsignaciÃ³n estÃ¡ configurada y activa. El perfil desencadenado tiene atributos de metadatos que coinciden con una regla. |
| **Postcondiciones** | El Perfil es creado con `auto_assigned = true`. La plantilla coincidente es vinculada. Las claves de cachÃ© de Redis son desalojadas si existen usuarios. Se escribe el registro de auditorÃ­a marcado como `auto: true`. |

---

## ðŸ”„ 2. Flujo de ConfiguraciÃ³n de Reglas (Admin â€” ConfiguraciÃ³n Ãšnica)

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador de Seguridad
    participant Console as Consola Admin UMS
    participant API as API .NET 8 UMS
    participant DB as PostgreSQL

    Admin->>Console: Navegar a Plantillas > Reglas de AsignaciÃ³n > Crear Regla
    Admin->>Console: Definir condiciones de la regla (role_code, organization_id, branch_id) y target template_id
    Console->>API: POST /api/v1/assignment-rules { conditions, template_id, priority }
    API->>DB: Persistir registro ASSIGNMENT_RULE (estado ACTIVE)
    API-->>Console: 201 Created { ruleId }
    Console-->>Admin: ConfirmaciÃ³n de regla activa
```

---

## ðŸ”„ 3. Flujo Disparador de AsignaciÃ³n AutomÃ¡tica (Sistema â€” Al Crear Perfil)

```mermaid
sequenceDiagram
    autonumber
    participant Trigger as Evento de CreaciÃ³n de Perfil
    participant Engine as Motor de Reglas (Servicio de Dominio)
    participant DB as PostgreSQL
    participant Cache as CachÃ© Redis
    participant Audit as Registro de AuditorÃ­a

    Trigger->>Engine: Evento ProfileCreatedEvent { profileId, role_code, organization_id, branch_id }
    Engine->>DB: Consultar todas las reglas ASSIGNMENT_RULES activas ordenadas por prioridad
    Engine->>Engine: Evaluar condiciÃ³n de cada regla contra atributos del perfil
    alt Regla Coincidente Encontrada
        Engine->>DB: Vincular template_id al perfil (auto_assigned = true)
        Engine->>Cache: Desalojar claves del grafo en cachÃ© (si hay usuarios asignados)
        Engine->>Audit: Registrar Evento TemplateAutoAssignedEvent { profileId, templateId, ruleId, auto: true }
    else No hay Coincidencia de Regla
        Engine->>Audit: Registrar Evento ProfileCreatedNoRuleMatchEvent { profileId }
        Note over Engine: Perfil creado sin plantilla - requiere asignaciÃ³n manual
    end
```

### A. LÃ³gica de Coincidencia de Reglas
Las reglas son evaluadas en **orden de prioridad** (nÃºmero menor = mayor prioridad). La **primera regla coincidente** gana (evaluaciÃ³n de cortocircuito).

Una regla es coincidente cuando **todas** sus condiciones son satisfechas:

| Campo CondiciÃ³n | Tipo de Coincidencia | Ejemplo de Valor |
| :--- | :--- | :--- |
| `role_code` | Coincidencia exacta | `TransportationAnalyst` |
| `organization_id` | Coincidencia exacta o comodÃ­n `*` | `tenant_logistics_corp` |
| `branch_id` | Coincidencia exacta o comodÃ­n `*` | `branch_callao_terminal` |
| `system_id` | Coincidencia exacta o comodÃ­n `*` | `scm_route_planner` |

**Ejemplo de Regla:**
> *Si `role_code = "TransportationAnalyst"` Y `organization_id = "tenant_logistics_corp"` ENTONCES asignar `Template_SCM_Analyst_Baseline_v1`*

---

## ðŸ›¡ï¸ 4. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: MÃºltiples Reglas Coinciden
- Si mÃ¡s de una regla coincide con los atributos del perfil, solamente se aplica la **regla de mayor prioridad** (el nÃºmero de prioridad mÃ¡s bajo). Las demÃ¡s coincidencias se registran en el rastro de auditorÃ­a como candidatas.

### Flujo Alternativo B: Plantilla Objetivo Desactivada
- Si la plantilla correspondiente ha sido desactivada o eliminada desde que se creÃ³ la regla, el motor registra una advertencia `RULE_TEMPLATE_UNAVAILABLE` en el rastro de auditorÃ­a y pasa a evaluar la siguiente regla coincidente. Si no existe alternativa, el perfil se crea sin asignaciÃ³n de plantilla.

### Flujo Alternativo C: Fallo del Motor de Reglas
- Si el Motor de Reglas lanza una excepciÃ³n no controlada, **NO se revierte la creaciÃ³n del Perfil** (el perfil se guarda). El fallo del motor se registra como un evento de nivel `CRITICAL` y el perfil se marca para una revisiÃ³n de asignaciÃ³n manual de plantilla.
