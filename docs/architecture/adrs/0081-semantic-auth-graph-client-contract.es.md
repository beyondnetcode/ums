# ADR-0081: Contrato Semantico del Auth Graph para Clientes — Code-First, ID-Optional

**Estado:** Aceptado  
**Fecha:** 2026-06-04  
**Responsable de Decisión:** Arquitectura  
**Relacionados:**
- [ADR-0071: Motor de Auth Graph](./0071-auth-graph-engine.es.md)
- [ADR-0074: Politica de Versionado del Schema del Auth Graph](./0074-auth-graph-schema-versioning.es.md)
- [ADR-0080: Previsualizacion del Auth Graph — Pipeline Interno vs Externo](./0080-auth-graph-preview-internal-pipeline.es.md)

---

## Contexto

El objetivo de largo plazo de UMS es actuar como un **proveedor claro de grafo de autorizacion** para los sistemas cliente. Los clientes deben poder activar, desactivar, ejecutar o denegar opciones usando un contrato estable, legible por negocio y semantico.

El grafo actual ya contiene datos ricos de autorizacion, pero tambien expone identificadores tecnicos internos (`Guid`) en varias secciones. Esos identificadores son utiles dentro del backend, para correlacion y para flujos de diagnostico, pero no son ideales como contrato principal para sistemas cliente descendentes.

La direccion del producto es volver el grafo expuesto al cliente:

1. Semantico
2. Estable a traves de la evolucion del schema
3. Independiente de identificadores internos de base de datos
4. Seguro para integraciones de larga duracion
5. Facil de consumir por codigo de producto, guards de UI y politicas del lado del servicio

---

## Decision

UMS tratara el grafo de autorizacion para clientes como el contrato semantico actual y mantendra los IDs fuera de la superficie cliente por defecto.

### 1. El contrato cliente por defecto es code-first

El grafo expuesto a sistemas cliente debe apoyarse principalmente en:

- `code`
- `value`
- `description`
- `effect`
- `source`
- `scope`
- `validUntil`
- `schemaVersion`

Los GUID tecnicos permanecen como detalles internos de implementacion salvo que un escenario de diagnostico o migracion los requiera.

### 2. Los identificadores internos no forman parte de la superficie normal de integracion

Los siguientes identificadores no deberian ser requeridos por clientes estandar:

- `user.id`
- `tenant.id`
- `systemSuite.id`
- `role.id`
- `profile.id`
- `branch.id`
- GUIDs de permisos y acciones

Estos valores pueden seguir existiendo en modelos internos, registros de auditoria y vistas administrativas internas, pero no deberian ser necesarios para decisiones normales de autorizacion.

### 3. El modo de diagnostico puede incluir IDs de forma explicita

Un contrato diagnostico opcional puede exponer GUIDs solo cuando:

- el llamador es un administrador interno o un operador de soporte autorizado
- la solicitud se hace mediante un endpoint interno de previsualizacion o diagnostico
- el llamador solicita explicitamente `includeIds=true` o una bandera equivalente de soporte

Este modo existe para troubleshooting, correlacion y soporte de migracion, no para consumo normal de clientes.

### 4. El significado de negocio se expresa mediante codigos estables

Los sistemas cliente deben usar codigos de negocio estables para interpretar el grafo:

- codigo de tenant
- codigo de system suite
- codigo de rol
- codigo de branch
- codigo de recurso
- codigo de accion
- efecto de permiso
- string de scope

Esto mantiene la logica del cliente independiente de identificadores generados por la base de datos.

### 5. El refresh sigue siendo por sesion, no por un token de grafo

UMS no introduce un contrato separado de refresh token para el grafo semantico. El grafo sigue siendo valido hasta `validUntil`. Cuando expira, el cliente se autentica de nuevo y recibe un snapshot fresco del grafo.

Esto evita estados parciales donde un token se renueva pero el modelo de autorizacion queda desfasado o inconsistente.

### 6. El grafo semantico es la fuente de verdad del cliente para decisiones de acceso

Para los sistemas cliente, el grafo debe ser la carga util autorizada durante su ventana de validez. El cliente no deberia inferir acceso desde GUIDs de backend ni reconsultar UMS para cada accion.

---

## Forma del Payload

```json
{
  "schemaVersion": "1.0.0",
  "graphId": "optional-support-correlation-id",
  "context": {
    "tenant": {
      "code": "TECHNO",
      "value": "Techno Logistics",
      "status": "ACTIVE",
      "isManagementOwner": false
    },
    "systemSuite": {
      "code": "WMS_SUITE",
      "value": "Warehouse Management Suite",
      "status": "PUBLISHED"
    },
    "role": {
      "code": "WAREHOUSE_SUPERVISOR",
      "value": "Warehouse Supervisor",
      "hierarchyLevel": 3
    },
    "profile": {
      "scope": "BranchScoped",
      "isActive": true
    },
    "branch": {
      "code": "LIM-01",
      "value": "Lima Main Branch"
    }
  },
  "permissions": [
    {
      "resourceCode": "INVENTORY",
      "actionCode": "VIEW",
      "effect": "Allow",
      "source": "Template"
    }
  ],
  "scopes": [
    "INVENTORY.VIEW"
  ],
  "featureFlags": [
    {
      "flagCode": "NEW_MENU_EXPERIMENT",
      "isEnabled": true
    }
  ],
  "effectiveConfig": {
    "sessionTimeoutMinutes": 60,
    "accessTokenDurationMs": 3600000
  },
  "generatedAt": "2026-06-04T12:00:00Z",
  "validUntil": "2026-06-04T13:00:00Z"
}
```

La estructura exacta puede evolucionar, pero el contrato debe permanecer semantico y estable para los sistemas cliente.

---

## Estrategia de Implementacion

### Baseline actual

- Usar el contrato semantico del cliente como payload por defecto para todas las integraciones nuevas.
- Mantener un modo diagnostico disponible para soporte y herramientas de previsualizacion.
- Reservar los GUID internos solo para modelos backend y flujos de soporte.

---

## Consecuencias

### Positivas

- Las integraciones cliente seran mas faciles de entender y mantener
- Producto y negocio podran razonar sobre accesos usando lenguaje estable
- Los identificadores de base de datos ya no se filtraran como dependencias de integracion
- La evolucion del schema sera mas segura porque los clientes dependeran de campos semanticos y no de claves sustitutas

### Compromisos

- Las herramientas de soporte pueden necesitar soporte explicito para modo diagnostico
- El backend debe seguir conservando identificadores internos aunque el cliente no dependa de ellos

---

## Notas de Implementacion

| Area | Guia |
|---|---|
| Graph builder | Producir campos semanticos como proyeccion cliente por defecto |
| Modelo interno | Mantener GUIDs en agregados backend y registros de auditoria |
| Endpoints cliente | Preferir respuestas basadas en code/value/description |
| Modo diagnostico | Exponer IDs solo a flujos internos autenticados de soporte |
| SDKs | Consumir campos semanticos como superficie canonica de decision |
| Documentacion | Mantener sincronizados los documentos de contrato en ingles y espanol |

---

**[Registro ADR](./index.md)**
