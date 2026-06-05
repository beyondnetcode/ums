# Contrato Semantico del Auth Graph

> **Idioma:** [English](../../sdk/contracts/semantic-client-contract.md) | Español

Este documento define el contrato cliente semantico para `AuthorizationGraph`.

El objetivo es hacer las integraciones cliente estables, legibles por negocio y libres de dependencia por defecto de GUID internos.

El contrato cliente canonico actual es el documentado aqui. El schema estructural heredado sigue disponible como referencia en [Schema Overview](./schema-overview.md).

---

## Objetivos del Contrato

Los sistemas cliente deben poder:

1. Decidir acceso sin depender de identificadores de base de datos.
2. Renderizar UI de negocio usando codigos estables.
3. Cachear el grafo con seguridad hasta `validUntil`.
4. Reautenticarse cuando el grafo expire en lugar de hacer refresh parcial del token.
5. Consumir el mismo contrato en web, mobile, backend y servicios de integracion.

---

## Forma Top-Level

```jsonc
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
  "authentication": {
    "method": "Local",
    "provider": {
      "code": "AZURE_AD_LOGISTICS",
      "value": "Azure AD - Logistics",
      "strategy": "AZURE_AD"
    },
    "mfaRequired": true,
    "issuedAt": "2026-06-04T12:00:00Z",
    "sessionExpiresAt": "2026-06-04T13:00:00Z"
  },
  "permissions": [
    {
      "resourceCode": "INVENTORY",
      "actionCode": "VIEW",
      "effect": "Allow",
      "source": "Template"
    }
  ],
  "featureFlags": [
    {
      "flagCode": "NEW_MENU_EXPERIMENT",
      "isEnabled": true
    }
  ],
  "effectiveConfig": {
    "sessionTimeoutMinutes": 60,
    "accessTokenDurationMs": 3600000,
    "authUseExternalIdp": false
  },
  "scopes": [
    "INVENTORY.VIEW"
  ],
  "generatedAt": "2026-06-04T12:00:00Z",
  "validUntil": "2026-06-04T13:00:00Z"
}
```

La forma exacta puede llevar secciones semanticas adicionales, pero el contrato cliente por defecto debe permanecer basado en codigos y sin IDs.

---

## Campos Permitidos

### Contexto

- `tenant.code`
- `tenant.value`
- `tenant.status`
- `tenant.isManagementOwner`
- `systemSuite.code`
- `systemSuite.value`
- `systemSuite.status`
- `role.code`
- `role.value`
- `role.hierarchyLevel`
- `profile.scope`
- `profile.isActive`
- `branch.code`
- `branch.value`

### Autenticacion

- `authentication.method`
- `authentication.provider.code`
- `authentication.provider.value`
- `authentication.provider.strategy`
- `authentication.mfaRequired`
- `authentication.issuedAt`
- `authentication.sessionExpiresAt`

### Autorizacion

- `permissions[].resourceCode`
- `permissions[].actionCode`
- `permissions[].effect`
- `permissions[].source`
- `featureFlags[].flagCode`
- `featureFlags[].isEnabled`
- `effectiveConfig.sessionTimeoutMinutes`
- `effectiveConfig.accessTokenDurationMs`
- `effectiveConfig.authUseExternalIdp`
- `scopes[]`
- `generatedAt`
- `validUntil`

### Campo solo para soporte

- `graphId` es opcional y existe para correlacion de soporte, no para logica de autorizacion.

---

## Campos Prohibidos en la Superficie Cliente por Defecto

Los siguientes campos no deberian formar parte del contrato externo normal:

- GUIDs crudos para tenant, user, role, profile, branch, system suite, resource o action
- claves primarias de base de datos
- claves foraneas
- nombres internos de tablas
- nombres de schemas SQL
- hashes de contrasenas
- claves de firma
- refresh tokens de IDPs externos
- access tokens de IDPs externos
- assertions SAML de IDPs externos
- secretos de infraestructura
- connection strings

Si un flujo de soporte necesita un identificador para correlacion, debe usar el modo diagnostico explicito y no el payload por defecto.

---

## Modo Diagnostico

El modo diagnostico se reserva para soporte interno y flujos de administracion.

Cuando esta habilitado, puede incluir:

- GUIDs internos
- metadatos de correlacion
- diffs solo de previsualizacion
- marcadores de traza utiles para depuracion

El modo diagnostico nunca debe convertirse en el contrato cliente por defecto.

Triggers recomendados:

- `includeIds=true`
- `mode=diagnostic`
- `mode=preview`

---

## Ruta de Migracion

### Fase 1

- Mantener disponible el payload actual del grafo.
- Agregar la proyeccion semantica en paralelo al grafo actual.
- Permitir que los flujos internos de soporte pidan modo diagnostico.

### Fase 2

- Actualizar los clientes externos para consumir el contrato semantico por `code` y `value`.
- Eliminar cualquier dependencia de GUIDs en la logica de decision del cliente.
- Mantener identificadores de correlacion solo internamente en backend.

### Fase 3

- Hacer del contrato semantico el valor por defecto para integraciones externas.
- Reservar la exposicion de GUIDs para herramientas de soporte y previsualizacion.

---

## Reglas de Decision del Cliente

Los clientes deben evaluar acceso en este orden:

1. Verificar `validUntil`.
2. Verificar alcance y estado del tenant.
3. Revisar permisos denegados primero.
4. Revisar permisos permitidos.
5. Evaluar feature flags solo cuando el grafo siga siendo valido.

Ejemplo:

```ts
function canExecute(graph, resourceCode, actionCode) {
  if (new Date(graph.validUntil) <= new Date()) return false;

  const deny = graph.permissions.find(
    p => p.resourceCode === resourceCode && p.actionCode === actionCode && p.effect === 'Deny'
  );
  if (deny) return false;

  return graph.permissions.some(
    p => p.resourceCode === resourceCode && p.actionCode === actionCode && p.effect === 'Allow'
  );
}
```

---

## Notas de Implementacion

| Area | Guia |
|---|---|
| Contrato publico | Preferir campos semanticos y codigos estables |
| Modelo interno | Mantener GUIDs dentro de agregados backend y eventos de auditoria |
| SDKs | Deserializar primero los campos semanticos, IDs solo en modo diagnostico |
| Diagnostico | Usar banderas explicitas de soporte para exponer IDs |
| Versionado | `schemaVersion` debe reflejar la revision del contrato semantico |

---

## Referencias

- [Resumen del Auth Graph Schema](./schema-overview.md)
- [Politica de Versionado del Auth Graph](./versioning.md)
- [Auth Graph (doc de dominio)](../../domain-es/identity/auth-graph.md)
- [ADR-0081: Contrato Semantico del Auth Graph para Clientes](../../architecture/adrs/0081-semantic-auth-graph-client-contract.es.md)
