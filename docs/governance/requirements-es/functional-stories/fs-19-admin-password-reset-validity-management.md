# Historia Funcional 19: Reset de Contraseña Admin y Gestión de Vigencia de Usuario

## 1. Propósito de Negocio

UMS debe permitir que administradores autorizados puedan restablecer contraseñas de usuarios y modificar períodos de vigencia de cuentas, con restricciones de alcance según el scope de gestión del portal, la propiedad del tenant y los permisos. El portal interno de UMS usa su propia ruta de autorización, mientras que la resolución IDP del tenant queda reservada para la API pública externa. Todas estas acciones deben quedar registradas en el historial de auditoría para garantizar responsabilidad y cumplimiento de políticas de seguridad.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Plataforma Interno** | Gestiona contraseñas y períodos de vigencia de usuarios en cualquier tenant del sistema a través del scope de gestión interna. |
| **Administrador de Tenant** | Gestiona contraseñas y períodos de vigencia de usuarios solo dentro de su propio tenant cuando ese tenant puede administrar su propio scope de UMS. |
| **Usuario Afectado** | Sujeto del reset de contraseña o modificación de período de vigencia. Recibe notificación de la acción realizada. |

## 3. Precondiciones de Negocio

- El administrador que realiza la acción está autenticado en el portal interno de UMS o en una sesión administrativa confiable y tiene un rol ADMIN en su alcance operativo.
- La cuenta de usuario objetivo existe y pertenece a un tenant dentro del alcance operativo del administrador.
- El administrador tiene el permiso requerido (`CAN_RESET_PASSWORD` y/o `CAN_MODIFY_VALIDITY_PERIOD`) asignado a su rol.
- Los feature flags que controlan estas capacidades están habilitados para el sistema o tenant.
- Las acciones administrativas limitadas al tenant solo están permitidas cuando el tenant está marcado como `IsManagementOwner=true`.

## 4. Flujo Funcional Principal

### 4.1 Flujo de Reset de Contraseña

1. El administrador navega a la sección de gestión de usuarios y selecciona el usuario objetivo.
2. UMS muestra los detalles de la cuenta del usuario incluyendo el estado actual de la credencial.
3. El administrador selecciona la acción "Restablecer Contraseña".
4. UMS valida que el administrador tiene permiso y que el usuario objetivo está dentro de su alcance.
5. El administrador confirma la acción, especificando opcionalmente una contraseña temporal o solicitando generación automática.
6. UMS genera una nueva contraseña temporal, la hashea y la almacena como la credencial activa.
7. UMS marca cualquier credencial activa previa como histórica (retenida para auditoría).
8. UMS registra la entrada de auditoría con todos los detalles requeridos.
9. UMS envía una notificación al usuario afectado (vía canal configurado) con instrucciones para cambiar la contraseña temporal.
10. UMS confirma al administrador que la contraseña fue restablecida exitosamente.

### 4.2 Flujo de Modificación de Período de Vigencia

1. El administrador navega a la sección de gestión de usuarios y selecciona el usuario objetivo.
2. UMS muestra los detalles de la cuenta del usuario incluyendo el período de vigencia actual (creado, expira, última actividad).
3. El administrador selecciona la acción "Modificar Período de Vigencia".
4. UMS valida que el administrador tiene permiso y que el usuario objetivo está dentro de su alcance.
5. El administrador especifica los nuevos parámetros de vigencia (extensión, reducción, o fecha de expiración explícita).
6. UMS valida el nuevo período contra las reglas del sistema (ej: no puede exceder la duración máxima permitida).
7. UMS actualiza el período de vigencia de la cuenta de usuario.
8. UMS registra la entrada de auditoría con todos los detalles incluyendo valores anteriores y nuevos.
9. UMS notifica al usuario afectado si el período de vigencia fue reducido o la cuenta fue desactivada.
10. UMS confirma al administrador que el período de vigencia fue actualizado.

## 5. Flujos Alternativos y Excepciones

### A. Operación Cross-Tenant (Admin de Tenant)

Si un administrador específico de tenant intenta restablecer una contraseña o modificar la vigencia de un usuario perteneciente a un tenant diferente, UMS rechaza la operación y devuelve un mensaje de acceso denegado explicando que la acción está fuera de su alcance operativo.

### B. Usuario Fuera del Alcance

Si el usuario objetivo no está dentro del alcance del administrador (ej: el usuario pertenece a un tenant al que el admin no puede acceder), UMS no revela la existencia del usuario y devuelve un mensaje genérico de "usuario no encontrado o acceso denegado".

### C. Permiso No Poseído

Si el administrador carece del permiso requerido (`CAN_RESET_PASSWORD` o `CAN_MODIFY_VALIDITY_PERIOD`), UMS no muestra el botón de acción correspondiente y devuelve error de autorización si la API es llamada directamente.

### D. Funcionalidad Deshabilitada

Si el feature flag que controla la acción está deshabilitado a nivel sistema o para el tenant específico, UMS oculta la acción y devuelve un mensaje de "funcionalidad no disponible".

### E. Usuario Federado

Si el usuario objetivo se autentica vía un proveedor de identidad externo, la acción de reset de contraseña no es aplicable y UMS redirige al administrador a la interfaz de gestión del proveedor externo.

### F. Vigencia Máxima Excedida

Si el período de vigencia solicitado excede la duración máxima permitida configurada en el sistema, UMS rechaza el cambio y explica la duración máxima allowable.

### G. Falla de Operación

Si la operación no puede completarse debido a un error del sistema, UMS muestra una razón clara cuando está disponible y un identificador de error de soporte para trazabilidad.

## 6. Reglas de Negocio

1. **Alcance de ADMIN Interno**: Usuarios con rol de ADMIN interno o scope de gestión de plataforma pueden realizar resets de contraseña y modificaciones de vigencia en cualquier cuenta de usuario del sistema.
2. **Alcance de ADMIN de Tenant**: Usuarios con roles de ADMIN específicos de tenant solo pueden realizar estas acciones en usuarios de su propio tenant cuando ese tenant está marcado como responsable de gestión.
3. **Requisitos de Permiso**: El administrador debe tener el permiso `CAN_RESET_PASSWORD` para restablecer contraseñas, y `CAN_MODIFY_VALIDITY_PERIOD` para modificar períodos de vigencia. Estos permisos se asignan vía autorización basada en roles.
4. **Requisito de Auditoría**: Cada reset de contraseña y modificación de período de vigencia DEBE generar un registro de auditoría inmutable.
5. **Denegación Cross-Tenant**: Los administradores específicos de tenant NO DEBEN poder ver, modificar o administrar usuarios de otros tenants.
6. **Sin Exposición de Secretos**: Los valores de contraseñas (temporales o permanentes) NUNCA DEBEN ser mostrados en la UI o incluidos en mensajes operativos.
7. **Requisito de Notificación**: Los usuarios afectados DEBEN ser notificados cuando su contraseña es restablecida o su período de vigencia es modificado.
8. **Reglas Configurables**: Período de vigencia máximo, longitud mínima de contraseña, y otros parámetros configurables DEBEN ser leídos de la configuración del sistema, no hardcoded.

## 7. Criterios de Aceptación

1. Un ADMIN interno puede restablecer contraseñas para usuarios en cualquier tenant.
2. Un ADMIN de tenant puede restablecer contraseñas solo para usuarios de su propio tenant.
3. Un ADMIN interno puede modificar períodos de vigencia para usuarios en cualquier tenant.
4. Un ADMIN de tenant puede modificar períodos de vigencia solo para usuarios de su propio tenant.
5. Las operaciones de reset de contraseña generan un registro de auditoría con: ID admin, ID usuario afectado, ID tenant, timestamp, y tipo de operación (`PASSWORD_RESET`).
6. Las modificaciones de período de vigencia generan un registro de auditoría con: ID admin, ID usuario afectado, ID tenant, valores de vigencia anteriores, nuevos valores de vigencia, timestamp, y tipo de operación (`VALIDITY_PERIOD_MODIFIED`).
7. La UI no muestra botones de acción para operaciones que el administrador no tiene permiso de realizar.
8. El sistema rechaza operaciones cross-tenant intentadas por administradores específicos de tenant con un mensaje de error apropiado.
9. Usuarios federados no pueden tener sus contraseñas restablecidas vía UMS (contraseña local no aplicable).
10. Todos los parámetros de configuración (vigencia máxima, reglas de contraseña, etc.) son leídos de la configuración del sistema.
11. El sistema oculta o deshabilita acciones cuando el feature flag correspondiente está deshabilitado.

## 8. Requisitos Técnicos

### 8.1 Autorización

- El flujo de gestión del portal debe resolver la autorización mediante el scope interno de UMS, no mediante el flujo IDP externo del tenant.
- `ITenantContext.OrganizationId` determina el alcance de tenant para operaciones limitadas al tenant.
- El flag `Tenant.IsManagementOwner` determina si el tenant puede realizar sus propias acciones de gestión interna.
- Las verificaciones de autorización en handlers deben validar:
  - `targetUser.TenantId == OrganizationId` para operaciones limitadas al tenant
  - el scope de plataforma permite operaciones cross-tenant desde el portal interno
  - El rol admin tiene los permisos requeridos (`CAN_RESET_PASSWORD`, `CAN_MODIFY_VALIDITY_PERIOD`)
- Feature flags para estas capacidades se almacenan en tabla `FeatureFlags` con claves `ALLOW_PASSWORD_RESET_BY_ADMIN` y `ALLOW_VALIDITY_PERIOD_MODIFICATION`.

### 8.2 Comandos

| Comando | Endpoint | Descripción |
|---|---|---|
| `ResetUserPasswordCommand` | `POST /user-accounts/{userAccountId}/passwords/reset` | Restablecer contraseña para usuario objetivo |
| `ModifyUserValidityPeriodCommand` | `PATCH /user-accounts/{userAccountId}/validity` | Modificar período de vigencia |

### 8.3 Eventos de Auditoría

| Evento | Campos |
|---|---|
| `PASSWORD_RESET` | `adminUserId`, `targetUserId`, `targetTenantId`, `timestamp`, `reason`, `effectiveImmediately` |
| `VALIDITY_PERIOD_MODIFIED` | `adminUserId`, `targetUserId`, `targetTenantId`, `timestamp`, `previousExpiresAt`, `newExpiresAt`, `reason` |

### 8.4 Configuración (Parámetros Configurables)

| Parámetro | Ubicación de Config | Default |
|---|---|---|
| `MAX_VALIDITY_PERIOD_DAYS` | `AppConfiguration` | 365 |
| `MIN_PASSWORD_LENGTH` | `AppConfiguration` | 12 |
| `PASSWORD_RESET_NOTIFICATION_CHANNEL` | `AppConfiguration` | email |
| `ALLOW_PASSWORD_RESET_BY_ADMIN` | `FeatureFlag` | true |
| `ALLOW_VALIDITY_PERIOD_MODIFICATION` | `FeatureFlag` | true |

### 8.5 Modelo de Datos

- `UserAccount` aggregate incluye `ValidityPeriod` value object con `CreatedAt`, `ExpiresAt`, `LastActivityAt`, `IsActive`.
- `PasswordCredential` es una entidad owned con `IsActive`, `CreatedAt`, `Historical` flag.
- Registros de auditoría almacenados en tabla `AuditRecords` con `OperationType`, `ActorId`, `TargetId`, `TargetType`, `TenantId`, `Timestamp`, `Details` (JSON).

### 8.6 Códigos de Error

| Código | Descripción |
|---|---|
| `AUTH_009` | Administrador carece del permiso requerido |
| `AUTH_010` | Usuario objetivo fuera del alcance del administrador |
| `USER_015` | Usuario federado no puede tener contraseña local restablecida |
| `CONFIG_003` | Período de vigencia solicitado excede el máximo permitido |

## 9. Trazabilidad

- Entidades: `UserAccount`, `PasswordCredential`, `AuditRecord`, `FeatureFlag`, `AppConfiguration`
- Historias relacionadas: FS-01 (autenticación de usuario), FS-03 (registrar organización), FS-18 (gestionar contraseña local)
- ADRs relacionadas: ADR-0012 control de acceso basado en roles, ADR-0019 requisitos de historial de auditoría
- Actualización de diagrama: `docs/domain/identity/user-account.md` - agregar gestión de período de vigencia, `docs/governance/audit/audit-events.md` - agregar nuevos tipos de eventos
