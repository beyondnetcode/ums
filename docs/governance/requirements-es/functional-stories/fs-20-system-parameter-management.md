# Historia Funcional 20: Gestión de Parámetros del Sistema

## 1. Propósito de Negocio

UMS debe proporcionar un mecanismo seguro, configurable y auditable para gestionar parámetros a nivel de sistema y específicos de tenant. Los administradores de plataforma internos pueden gestionar parámetros globales y específicos de tenant a través del scope interno de gestión de UMS, mientras que los administradores de tenant solo pueden gestionar parámetros dentro de su propio scope cuando el tenant es responsable de su propia gestión. Todos los cambios de parámetros deben ser rastreados para cumplimiento y depuración.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Plataforma Interno** | Gestiona parámetros globales del sistema y parámetros específicos de cualquier tenant a través del scope interno del portal. |
| **Administrador de Tenant** | Gestiona parámetros dentro de su propio scope de tenant únicamente cuando el tenant puede gestionar su propio scope de UMS. No puede acceder a configuraciones globales ni de otros tenants. |
| **Sistema** | Consume los parámetros configurados para determinar comportamiento, reglas, límites y permisos. |

## 3. Precondiciones de Negocio

- El administrador está autenticado y tiene una sesión válida en el portal interno de UMS o en una sesión administrativa confiable.
- El administrador tiene el permiso `CAN_MANAGE_GLOBAL_CONFIGURATION` (para config global) o `CAN_MANAGE_TENANT_CONFIGURATION` (para config específica de tenant).
- Para operaciones de tenant gestionadas desde el portal, `Tenant.IsManagementOwner=true`.
- Para admins de tenant, `OrganizationId` coincide con el tenant objetivo para operaciones específicas de tenant.

## 4. Flujo Funcional Principal

### 4.1 Acceder a Configuración Global (Solo Admin Interno)

1. El administrador del portal navega a la sección de Configuración del Sistema.
2. UMS valida que el administrador tiene scope interno del portal y el usuario tiene `CAN_MANAGE_GLOBAL_CONFIGURATION`.
3. UMS muestra todos los parámetros globales (donde `Scope = Global`).
4. El administrador puede crear, modificar, publicar o archivar parámetros globales.
5. Los cambios son auditados con trazabilidad completa.

### 4.2 Acceder a Configuración de Tenant

1. Un administrador (interno o de tenant) navega a la sección de Configuración de un tenant.
2. Para administradores del portal interno: UMS permite ver/gestionar cualquier parámetro de cualquier tenant.
3. Para admins de tenant: UMS valida que `OrganizationId == targetTenantId` y que el tenant puede gestionar su propio scope.
4. UMS muestra parámetros donde `Scope = Tenant` y `TenantId = targetTenant`.
5. El administrador puede crear, modificar, publicar o archivar parámetros específicos de tenant.

### 4.3 Crear/Modificar Parámetro

1. El administrador selecciona "Agregar Parámetro" o selecciona un parámetro existente.
2. UMS muestra un formulario con: Code, Value, Description, Scope, IsInheritable, IsEncrypted.
3. Para parámetros globales: Scope se establece automáticamente como Global.
4. Para parámetros de tenant: Scope se establece como Tenant y TenantId se auto-popula.
5. El administrador completa los detalles y envía.
6. UMS valida los datos y guarda como Draft.
7. UMS registra la entrada de auditoría.
8. UMS indica al administrador que debe publicar el parámetro.

### 4.4 Publicar Parámetro

1. El administrador selecciona un parámetro en Draft y elige "Publicar".
2. UMS valida que el parámetro es válido.
3. UMS cambia el estado de Draft a Published.
4. UMS registra la entrada de auditoría.
5. El parámetro se activa y entra en efecto inmediatamente.

## 5. Flujos Alternativos y Excepciones

### A. Intento de Acceso No Autorizado

Si un administrador de tenant intenta acceder a configuración global, UMS devuelve un error 403 con mensaje "La configuración global solo es accesible mediante el scope interno de gestión."

### B. Intento de Acceso Cross-Tenant

Si un administrador de tenant intenta acceder a la configuración de otro tenant, UMS devuelve un error 403 con mensaje "No tiene permiso para acceder a la configuración de este tenant."

### C. Parámetro No Encontrado

Si el administrador solicita un parámetro que no existe, UMS devuelve un error 404.

### D. Modificación Concurrente

Si dos administradores modifican el mismo parámetro simultáneamente, UMS devuelve un error 409 Conflict con información de conflicto de ETag.

### E. Modificaciones Solo en Draft

Los parámetros solo pueden ser modificados cuando están en estado Draft. Los parámetros publicados requieren crear una nueva versión Draft.

### F. Scope Inválido para Tipo de Usuario

Si un admin interno intenta crear un parámetro con Scope Tenant sin especificar TenantId, UMS devuelve un error de validación.

## 6. Reglas de Negocio

1. **Scope de Configuración Global**: Parámetros sin `TenantId`, sin `SystemSuiteId` y sin `ModuleId` son Globales. Solo administradores del portal interno pueden gestionarlos.
2. **Scope de Configuración de Tenant**: Parámetros con `TenantId` son específicos de tenant. Tanto admins internos como de tenant pueden gestionarlos cuando el tenant está marcado como responsable de gestión (admins limitados a su tenant).
3. **Herencia**: Si `IsInheritable=true`, configuraciones hijas pueden sobrescribir el parámetro.
4. **Encriptación**: Si `IsEncrypted=true`, el valor se almacena encriptado y se desencripta solo cuando se consume.
5. **Versionado**: Los parámetros usan versionado semántico (`Major.Minor.Patch`). Las actualizaciones a Draft incrementan la versión menor.
6. **Ciclo de Vida del Estado**: Draft → Published → Archived. Solo parámetros en Draft pueden ser modificados.
7. **Matriz de Autorización**:
   | Scope de Configuración | Admin Interno | Admin de Tenant |
   |---------------------|----------------|----------------|
   | Global | Puede gestionar | No puede acceder |
   | Tenant (propio) | Puede gestionar | Puede gestionar |
   | Tenant (otro) | Puede gestionar | No puede acceder |
   | Suite | Puede gestionar | No puede acceder |
   | Module | Puede gestionar | No puede acceder |

## 7. Criterios de Aceptación

1. Los administradores internos pueden ver y gestionar todos los parámetros globales a través del scope interno del portal.
2. Los administradores de tenant no pueden ver ni acceder a configuración global.
3. Los administradores de tenant pueden ver y gestionar parámetros dentro de su propio tenant.
4. Los administradores de tenant no pueden acceder a configuraciones de otros tenants.
5. Los administradores internos pueden acceder a la configuración de cualquier tenant a través del scope interno del portal.
6. Todos los cambios de parámetros (crear, actualizar, publicar, archivar) generan registros de auditoría.
7. El sistema usa valores de parámetros de AppConfiguration en lugar de constantes hardcodeadas.
8. Los parámetros soportan versionado y ciclo de vida de estado (Draft → Published → Archived).
9. El flag de encriptación protege valores sensibles de parámetros.
10. El flag de herencia permite que configuraciones hijas sobrescriban parámetros.
11. La concurrencia optimista previene actualizaciones conflictivas.

## 8. Requisitos Técnicos

### 8.1 Verificaciones de Autorización

En todos los endpoints de AppConfiguration, agregar inyección de `ITenantContext` y verificar:

```csharp
// Para configs globales
if (scope == "Global" && !hasInternalPortalScope)
    return Results.Forbid();

// Para configs de tenant
if (targetTenantId != organizationId && !hasInternalPortalScope)
    return Results.Forbid();
```

### 8.2 Endpoints

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|---------------|
| GET | `/app-configurations` | Listar configuraciones (filtradas por scope y permisos de usuario) | Basado en scope |
| GET | `/app-configurations/{id}` | Obtener una configuración | Basado en scope y propiedad |
| POST | `/app-configurations` | Crear nueva configuración | Admin interno o admin de tenant para su propio scope de gestión |
| PUT | `/app-configurations/{id}` | Actualizar configuración draft | Igual que crear |
| POST | `/app-configurations/{id}/publish` | Publicar configuración | Igual que crear |
| POST | `/app-configurations/{id}/archive` | Archivar configuración | Igual que crear |

### 8.3 Filtros de Consulta

| Parámetro | Comportamiento |
|-----------|----------|
| `scope=Global` | Solo administradores del portal interno pueden consultar; retorna todas las configs globales |
| `scope=Tenant&tenantId={id}` | Administradores del portal interno obtienen todas las configs de tenant; admins de tenant obtienen solo su tenant cuando está permitido |
| Sin filtro de scope | Retorna configs basadas en los permisos del usuario |

### 8.4 Valores Hardcodeados a Migrar

| Hardcode Actual | Código de Parámetro | Valor Default |
|-----------------|-------------------|---------------|
| `ACCESS_TOKEN_DURATION` (frontend) | `ACCESS_TOKEN_DURATION_MS` | 3600000 (1 hora) |
| `REFRESH_TOKEN_DURATION` (frontend) | `REFRESH_TOKEN_DURATION_MS` | 604800000 (7 días) |
| `MIN_PASSWORD_LENGTH` | `MIN_PASSWORD_LENGTH` | 12 |
| `MAX_VALIDITY_PERIOD_DAYS` | `MAX_VALIDITY_PERIOD_DAYS` | 365 |

### 8.5 Eventos de Auditoría

| Evento | Campos |
|---|---|
| `APP_CONFIG_CREATED` | adminUserId, configId, code, scope, timestamp |
| `APP_CONFIG_UPDATED` | adminUserId, configId, code, oldVersion, newVersion, timestamp |
| `APP_CONFIG_PUBLISHED` | adminUserId, configId, code, version, timestamp |
| `APP_CONFIG_ARCHIVED` | adminUserId, configId, code, version, timestamp |

### 8.6 Feature Flags

| Flag | Descripción | Default |
|------|-------------|---------|
| `ALLOW_GLOBAL_CONFIG_MANAGEMENT` | Habilitar gestión de configuración global | `true` |
| `ALLOW_TENANT_CONFIG_MANAGEMENT` | Habilitar gestión de configuración específica de tenant | `true` |

## 9. Trazabilidad

- Entidades: `AppConfiguration`, `AuditRecord`
- Historias relacionadas: FS-01 (autenticación), FS-17 (roles de sistema), FS-19 (reset de contraseña admin)
- ADRs relacionadas: ADR-0012 (RBAC), ADR-0019 (trail de auditoría)
- Actualización de diagrama: `docs/domain/identity/tenant.md` sección 10 - agregar reglas de gestión de parámetros
