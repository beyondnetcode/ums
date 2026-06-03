# ADR-0079: Política de Guardia de Dependencias — Bloqueo de Operaciones con Dependencias Activas

**Estado:** Aceptado  
**Fecha:** 2026-06-03  
**Responsable:** Arquitectura  
**Relacionados:**
- [ADR-0077: Límite de Autorización para Gestión del Portal de Tenant](./0077-tenant-portal-management-authorization-boundary.md)

---

## Contexto

Varias operaciones de cambio de estado en UMS son inseguras cuando existen dependencias activas. Por ejemplo, suspender un tenant mientras usuarios activos siguen autenticados, o bloquear un usuario mientras sus perfiles se usan activamente en decisiones de autorización.

Sin una capa de guardia, estas operaciones dejarían silenciosamente datos activos huérfanos o requerirían flujos de trabajo complejos con verificaciones manuales. Se necesita un contrato consistente y descubrible que:

1. Prevenga el cambio de estado peligroso.
2. Devuelva una respuesta estructurada explicando *por qué* fue bloqueado y *qué* debe resolverse primero.
3. No coloque la lógica de guardia en el modelo de dominio (que requeriría lecturas costosas entre agregados) ni en la UI (que sería poco confiable).

---

## Decisión

### Ubicación: handlers de comandos de aplicación

Las guardias de dependencias viven en los handlers de comandos de aplicación, no en el agregado de dominio ni en la UI. El handler consulta los repositorios relevantes antes de despachar el comando de cambio de estado y rechaza la operación si se encuentran dependencias bloqueantes.

Esto permite:
- Lecturas entre agregados sin violar los límites de agregado DDD.
- Verificabilidad mediante tests de integración sin cargar el modelo de dominio completo.
- Aplicación consistente independientemente de qué superficie dispara el comando.

### Codificación de errores: `BlockedOperationError` con separador `|`

Los errores de bloqueo se codifican como cadena usando `BlockedOperationError.Encode(errorCode, deps)` donde múltiples entradas de dependencia se separan con `|`. El decodificador `BlockedOperationError.TryDecode(error, out errorCode, out deps)` reconstruye la lista en la capa de presentación.

### Contrato HTTP: 409 Conflict con `BlockedOperationResponse`

Cuando una guardia se activa, el endpoint devuelve **HTTP 409 Conflict** con el siguiente cuerpo JSON:

```json
{
  "errorCode": "TENANT_HAS_ACTIVE_USERS",
  "message": "Mensaje legible en el idioma configurado",
  "brokenRule": "A tenant cannot be suspended while active users exist.",
  "blockingDependencies": [
    { "entityType": "UserAccount", "status": "Active", "count": 3 }
  ]
}
```

Los valores de `errorCode` están definidos en `DomainErrors` y mapeados a mensajes/reglas en `BlockedOperationMessages`.

### Operaciones con guardia

| Operación | Código de error | Dependencia bloqueante |
|---|---|---|
| Suspender Tenant | `TENANT_HAS_ACTIVE_USERS` | Registros `UserAccount` activos |
| Suspender Tenant | `TENANT_HAS_ACTIVE_BRANCHES` | Registros `Branch` activos |
| Suspender Tenant | `TENANT_HAS_ACTIVE_IDP_CONFIG` | Registros `IdpConfiguration` activos |
| Bloquear / Eliminar Usuario | `USER_HAS_ACTIVE_PROFILES` | Registros `Profile` activos |
| Desactivar Rol | `ROLE_HAS_ACTIVE_PROFILES` | Registros `Profile` activos |
| Desactivar Rol | `ROLE_HAS_ACTIVE_CHILD_ROLES` | Registros de `Role` hijo activos |
| Deprecar Template | `TEMPLATE_HAS_ACTIVE_PROFILES` | Registros `Profile` activos |
| Eliminar DomainResource | `DOMAIN_RESOURCE_HAS_TEMPLATE_ITEMS` | Registros `PermissionTemplateItem` activos |
| Eliminar Módulo | `MODULE_HAS_ACTIVE_MENUS` | Registros `Menu` activos |

---

## Consecuencias

- **Positivo:** Respuesta 409 consistente y legible por máquina — la UI puede mostrar información accionable de bloqueo al usuario.
- **Positivo:** La lógica de guardia está centralizada y es testeable independientemente del modelo de dominio.
- **Negativo:** La capa de aplicación debe emitir consultas adicionales antes de cada operación con guardia; aceptable dada la baja frecuencia de mutaciones.
- **Neutral:** Agregar nuevas operaciones con guardia requiere: una nueva constante en `DomainErrors`, una nueva entrada en `BlockedOperationMessages`, y una nueva verificación en el handler de comando correspondiente.
