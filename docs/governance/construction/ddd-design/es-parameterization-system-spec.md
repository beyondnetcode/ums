# Especificación del Sistema de Parametrización UMS

> **Versión:**1.0.0
> **Estado:**Propuesta
> **Creado:**2026-05-30
> **Última actualización:**2026-05-30

---

## 1. Visión General

### 1.1 Propósito

Establecer un patrón obligatorio para todos los comportamientos configurables del sistema UMS. Ninguna regla de negocio, validación, límite, vigencia, comportamiento, permiso configurable o configuración operativa puede quedar hardcodeada en la lógica web, API, servicios o componentes internos.

### 1.2 Alcance

Todos los comportamientos configurables dentro de UMS deben ser almacenados en la base de datos, cargados en memoria al inicio del sistema, y consumidos a través de un servicio proveedor de configuración centralizado.

---

## 2. Arquitectura de Parametrización

### 2.1 Zonas de Almacenamiento

```
┌─────────────────────────────────────────────────────────────┐
│ Configuración UMS │
├─────────────────────────┬───────────────────────────────────┤
│ GLOBAL (Sistema) │ POR TENANT │
│ ScopeId = 1 │ ScopeId = 2 │
├─────────────────────────┼───────────────────────────────────┤
│ • Visible solo para │ • Visible para Admin de Tenant │
│ Admin Interno │ (con permisos) │
│ • Aplica a todo el │ • Aplica solo a su tenant │
│ sistema UMS │ • Admin Interno puede ver todos │
│ • Configuración base │ • Tenant puede sobreescribir si │
│ para todos los │ el global lo permite │
│ comportamientos │ │
└─────────────────────────┴───────────────────────────────────┘
```

### 2.2 Identificadores de Alcance (Scope)

| ScopeId | Nombre | Descripción |
|---------|--------|-------------|
| 1 | Global | Parámetros del sistema UMS |
| 2 | Tenant | Parámetros específicos de tenant |
| 3 | Suite | Parámetros a nivel de System Suite (futuro) |
| 4 | Module | Parámetros a nivel de Módulo (futuro) | ---

## 3. Loader de Configuración

### 3.1 Propósito

Cargar todos los parámetros desde la base de datos a memoria al inicio del sistema para evitar consultas repetitivas a la BD durante la ejecución.

### 3.2 Comportamiento

```
Inicio del Sistema
 │
 ▼
┌────────────────────┐
│ ConfigurationLoader│
│ │
│ 1. Cargar Globales │ ← ScopeId = 1
│ │
│ 2. Cargar Por │ ← Para tenants activos
│ Tenant │
│ │
│ 3. Construir store │
│ en memoria │
│ │
│ 4. Registrar como │
│ singleton │
└────────────────────┘
 │
 ▼
┌────────────────────┐
│ConfigurationProvider│ ← Consumido por toda la lógica
│ │
│ get(key) │
│ get(key, tenantId) │
│ set(key, value) │ ← Dispara auditoría
└────────────────────┘
```

### 3.3 Estrategia de Carga

1. **Parámetros Globales**: Cargados síncronamente al inicio
2. **Parámetros de Tenant**: Cargados on-demand cuando se establece el contexto de tenant, luego cacheados
3. **Refresh de Cache**: Solo por trigger manual (sin refresh automático durante ejecución)

### 3.4 Estructura en Memoria

```typescript
interface ParameterStore {
 global: Map<string, AppConfiguration>;
 byTenant: Map<Guid, Map<string, AppConfiguration>>;
}
```

---

## 4. Modelo de Dominio de Parámetros

El sistema de configuración se modela como tres Aggregate Roots para que el esquema, el valor por defecto y el override por tenant evolucionen de forma independiente, manteniendo al mismo tiempo el contrato obligatorio `code`, `value`, `description`.

| Aggregate Root | Rol |
|---|---|
| `ParameterDefinition` | Define el esquema y el significado de negocio de un parámetro configurable |
| `ParameterGlobalValue` | Almacena el valor por defecto a nivel sistema |
| `ParameterTenantValue` | Almacena el override específico de tenant cuando la política lo permite | ### Contrato Compartido

- `code` identifica el parámetro.
- `value` almacena el contenido efectivo actual.
- `description` explica propósito, alcance e impacto de negocio.
- `ParameterTenantValue` siempre debe referenciar un `ParameterDefinition`.
- `ParameterGlobalValue` actúa como línea base cuando no existe override de tenant.

---

## 5. Servicio Proveedor de Configuración

### 4.1 Interfaz

```typescript
interface IConfigurationProvider {
 // Obtener parámetro global
 getGlobal(key: string): AppConfiguration | undefined;

 // Obtener parámetro de tenant (con revisión de precedencia)
 getForTenant(tenantId: Guid, key: string): AppConfiguration | undefined;

 // Obtener valor como tipo específico
 getValueAs<T>(key: string, tenantId?: Guid, defaultValue?: T): T;

 // Establecer parámetro (dispara auditoría)
 set(key: string, value: string, scope: Scope, tenantId?: Guid): void;

 // Recargar desde base de datos
 reload(): Promise<void>;

 // Recargar para tenant específico
 reloadTenant(tenantId: Guid): Promise<void>;
}
```

### 4.2 Reglas de Precedencia

1. **Si existe parámetro de tenant** → Usar valor del tenant
2. **Si solo existe parámetro global** → Usar valor global
3. **Los parámetros globales actúan como restricción máxima**cuando aplique
4. **Tenant no puede sobreescribir**cuando el parámetro global está marcado como no sobreescribible

```typescript
function getWithPrecedence(key: string, tenantId?: Guid): AppConfiguration | undefined {
 if (tenantId) {
 const tenantParam = store.byTenant.get(tenantId)?.get(key);
 if (tenantParam) return tenantParam;
 }

 return store.global.get(key);
}
```

---

## 6. Estructura de Parámetros

### 5.1 Entidad AppConfiguration

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | Guid | Identificador único |
| TenantId | Guid? | Propietario del tenant (null para Global) |
| SystemSuiteId | Guid? | System Suite (para nivel Suite) |
| ModuleId | Guid? | Módulo (para nivel Módulo) |
| Code | string | Código del parámetro (único dentro del scope) |
| Value | string | Valor del parámetro |
| Description | string | Descripción legible |
| ScopeId | int | 1=Global, 2=Tenant, 3=Suite, 4=Módulo |
| IsInheritable | bool | Puede ser heredado por scopes hijos |
| IsEncrypted | bool | El valor está encriptado |
| Version | string | Versión semántica |
| StatusId | int | 1=Borrador, 2=Publicado, 3=Archivado | ### 5.2 Parámetros Requeridos (Conjunto Inicial)

#### Parámetros Globales (ScopeId = 1)

| Code | Tipo | Default | Descripción |
|------|------|---------|-------------|
| SESSION_TIMEOUT_MINUTES | int | 30 | Timeout de sesión inactiva |
| MAX_LOGIN_ATTEMPTS | int | 5 | Máximos intentos de login fallidos |
| ACCESS_TOKEN_DURATION_MS | int | 3600000 | Vida útil del token de acceso (1 hora) |
| REFRESH_TOKEN_DURATION_MS | int | 604800000 | Vida útil del token refresh (7 días) |
| MIN_PASSWORD_LENGTH | int | 12 | Longitud mínima de password |
| MAX_VALIDITY_PERIOD_DAYS | int | 365 | Período de validez de cuenta |
| MFA_REQUIRED_FOR_ADMIN | bool | false | Interruptor por tenant que exige MFA verificado al iniciar sesion cuando esta habilitado |
| PASSWORD_HISTORY_COUNT | int | 5 | Tamaño del historial de passwords |
| UI_LANGUAGE_DEFAULT | string | "es" | Idioma por defecto de la UI |
| UI_TIMEZONE_DEFAULT | string | "America/Lima" | Zona horaria por defecto |
| EMAIL_FROM_ADDRESS | string | "noreply@ums.local" | Remitente de email |
| NOTIFICATION_RETRY_ATTEMPTS | int | 3 | Cantidad de reintentos de notificación | #### Parámetros de Tenant (ScopeId = 2)

| Code | Tipo | Descripción |
|------|------|-------------|
| MFA_ALLOWED_METHODS | string[] | Métodos MFA permitidos (SMS, TOTP, Email, WebAuthn) |
| PASSWORD_MAX_AGE_DAYS | int | Edad máxima de password (sobreescribe global si está establecido) |
| ACCOUNT_LOCKOUT_DURATION_MINUTES | int | Duración del bloqueo de cuenta |
| UI_CUSTOM_BRANDING_ENABLED | bool | Habilitar branding personalizado |
| SESSION_MAX_CONCURRENT | int | Máximas sesiones concurrentes por usuario |

Las respuestas de login y el grafo efectivo de autenticacion deben resolver estos valores por tenant para que el portal aplique la misma politica MFA sin asumir que es global.

## 7. Registro de Auditoría

### 6.1 Operaciones Auditadas

- Crear parámetro
- Actualizar valor del parámetro
- Actualizar estado del parámetro (Borrador → Publicado → Archivado)
- Eliminar parámetro
- Sobrecribir parámetro global a nivel de tenant

### 6.2 Estructura del Registro de Auditoría

```typescript
interface ConfigurationAuditRecord {
 id: Guid;
 parameterId: Guid;
 parameterCode: string;
 tenantId: Guid | null; // null para parámetros globales
 userId: Guid;
 userType: 'InternalAdmin' | 'TenantAdmin' | 'System';
 operation: 'Create' | 'Update' | 'Delete' | 'Override';
 previousValue: string | null;
 newValue: string | null;
 timestamp: DateTime;
 ipAddress: string;
 userAgent: string;
}
```

---

## 8. Endpoints de API

### 8.1 Parámetros Globales (Solo Admin Interno)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/configurations/global` | Listar todos los parámetros globales |
| GET | `/api/v1/configurations/global/{code}` | Obtener parámetro global por código |
| POST | `/api/v1/configurations/global` | Crear parámetro global |
| PUT | `/api/v1/configurations/global/{code}` | Actualizar parámetro global |
| DELETE | `/api/v1/configurations/global/{code}` | Archivar parámetro global | ### 7.2 Parámetros de Tenant

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/tenants/{tenantId}/configurations` | Listar parámetros de tenant |
| GET | `/api/v1/tenants/{tenantId}/configurations/{code}` | Obtener parámetro de tenant |
| POST | `/api/v1/tenants/{tenantId}/configurations` | Crear parámetro de tenant |
| PUT | `/api/v1/tenants/{tenantId}/configurations/{code}` | Actualizar parámetro de tenant |
| DELETE | `/api/v1/tenants/{tenantId}/configurations/{code}` | Archivar parámetro de tenant | ---

## 8. Integración con Frontend

### 8.1 Estructura de Pantalla de Configuración

```
┌─────────────────────────────────────────────────────────────┐
│ Parámetros del Sistema │
├─────────────────────────┬───────────────────────────────────┤
│ [Global] [Por Tenant] │ │
├─────────────────────────┤ ┌─────────────────────────────┐ │
│ Buscar... │ │ SESSION_TIMEOUT_MINUTES │ │
│ │ │ Valor: 30 │ │
│ [Global] │ │ Descripción: Timeout de │ │
│ • SESSION_TIMEOUT... │ │ sesión inactiva en minutos │ │
│ • MAX_LOGIN_ATTEMPTS │ │ Alcance: Global │ │
│ • ACCESS_TOKEN_... │ │ Estado: Publicado │ │
│ │ │ │ │
│ [Por Tenant] │ │ [Editar] [Archivar] │ │
│ ▼ TENANT: Ransa │ └─────────────────────────────┘ │
│ • MFA_ALLOWED_... │ │
│ • PASSWORD_MAX_... │ │
│ ▼ TENANT: APM │ │
│ • MFA_ALLOWED_... │ │
└─────────────────────────┴───────────────────────────────────┘
```

### 8.2 Pestaña de Configuración de Tenant

```
┌─────────────────────────────────────────────────────────────┐
│ Tenant: Ransa S.A. [Parámetros] │
├─────────────────────────────────────────────────────────────┤
│ │
│ Configuraciones específicas para este tenant │
│ │
│ [+ Agregar Parámetro] │
│ │
│ ┌──────────────────────────────────────────────────────┐ │
│ │ MFA_ALLOWED_METHODS │ │
│ │ Valor: ["TOTP", "Email"] │ │
│ │ Override del global: No establecido │ │
│ │ [Editar] [Eliminar Override] │ │
│ └──────────────────────────────────────────────────────┘ │
│ │
└─────────────────────────────────────────────────────────────┘
```

---

## 9. Implementación Técnica

### 9.1 Servicios de Backend

1. **ConfigurationLoader** (Singleton)
- Carga parámetros al inicio
- Gestiona cache en memoria
- Maneja solicitudes de recarga

2. **ConfigurationProvider** (Singleton)
- Punto de acceso central para todos los valores de configuración
- Implementa lógica de precedencia
- Notifica al servicio de auditoría ante cambios

3. **ConfigurationAuditService**
- Registra todos los cambios de configuración
- Consulta registros de auditoría

4. **ConfigurationValues**
- Fachada tipada sobre `IConfigurationProvider` para consumidores de login, sesión y contraseña
- Mantiene centralizadas las consultas de configuración en handlers y validadores

### 9.2 TODO: Migración Futura a Redis

```typescript
/**
* TODO: [TD-003] Migrar ConfigurationProvider a Redis
*
* La implementación actual almacena parámetros en memoria.
* La implementación futura debe usar Redis para cache distribuido.
*
* Pasos de migración:
* 1. Introducir abstracción IConfigurationCache
* 2. Implementar InMemoryConfigurationCache (actual)
* 3. Implementar RedisConfigurationCache (futuro)
* 4. Actualizar ConfigurationProvider para usar IConfigurationCache
* 5. Agregar estrategia de invalidación de cache
* 6. Implementar cache warming al inicio
*
* Beneficios:
* - Configuración compartida entre instancias de API
* - Invalidación automática ante actualizaciones
* - Menor carga en base de datos
* - Soporte para hot-reload de configuración
*/
```

---

## 10. Criterios de Aceptación

### 10.1 Requisitos Funcionales

- [ ] Todos los comportamientos configurables usan parámetros de la base de datos
- [ ] Parámetros globales son visibles solo para Admin Interno
- [ ] Parámetros de tenant son visibles para usuarios autorizados del tenant
- [ ] Admin Interno puede ver/editar todos los parámetros de tenant
- [ ] Regla de precedencia: Tenant sobreescribe Global cuando está permitido
- [ ] Todos los cambios de parámetros son auditados

### 10.2 Requisitos Técnicos

- [ ] Los parámetros se cargan en memoria al inicio
- [ ] No hay consultas directas a BD para parámetros durante el procesamiento de requests
- [ ] ConfigurationProvider es el único punto de acceso
- [ ] El cache puede ser refrescado manualmente
- [ ] El código está preparado para migración a Redis (capa de abstracción)

### 10.3 Requisitos de Seguridad

- [ ] La autorización valida tipo de usuario y acceso a tenant
- [ ] Los logs de auditoría incluyen usuario, timestamp y cambios
- [ ] Los parámetros encriptados se almacenan de forma segura

---

## 11. Documentos Relacionados

- [Historia Funcional: Gestión de Parámetros del Sistema](../../requirements/functional-stories/fs-20-system-parameter-management.md)
- [Registro de Deuda Técnica](../../../architecture/technical-debt.md)
- [TODO: Implementación del Sistema de Configuración](../../project/TODO.md)

---

## 12. Registro de Cambios

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0.0 | 2026-05-30 | Especificación inicial |
