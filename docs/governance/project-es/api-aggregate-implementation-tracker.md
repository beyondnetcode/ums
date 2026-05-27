# Seguimiento de Implementacion de Agregados en la API

Este documento registra el estado actual de implementacion de la API UMS por agregado para poder retomar el trabajo sin reconstruir contexto.

## 1. Resumen Actual

| Agregado | Dominio | Aplicacion | REST | Queries GraphQL | Persistencia SQL Server | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| Tenant | Si | Si | Si | Si | Si | Parcial |
| UserAccount | Si | Si | Si | Si | Si | Parcial |
| Profile | Si | Si | Si | Si | Si | Parcial |
| SystemSuite | Si | Si | Si | Si | No | Parcial |
| PermissionTemplate | Si | Si | Si | Si | No | Parcial |
| ApprovalWorkflow | Si | Si | Si | Si | No | Parcial |
| ApprovalRequest | Si | Si | Si | Si | No | Parcial |
| DocumentType | Si | Si | Si | Si | No | Parcial |
| UserDocument | Si | Si | Si | Si | No | Parcial |
| AccessEnforcementPolicy | Si | Si | Si | Si | No | Parcial |
| NotificationRule | Si | Si | Si | Si | No | Parcial |
| PromotionRequest | Si | Si | Si | Si | No | Parcial |
| RoleMaturityStatus | Si | Si | Si | Si | No | Parcial |
| AuditRecord | Si | Si | Si | Si | No | Parcial |
| AppConfiguration | Si | No | No | No | No | Faltante |
| FeatureFlag | Si | No | No | No | No | Faltante |
| IdpConfiguration | Si | No | No | No | No | Faltante |

## 2. Gaps de Mayor Prioridad

1. Completar el contexto Configuration en la API:
   - `AppConfiguration`
   - `FeatureFlag`
   - `IdpConfiguration`
2. Terminar la exposicion de comportamientos de dominio en agregados ya presentes en REST y GraphQL:
   - `UserAccount`
   - `Profile`
   - `SystemSuite`
   - `PermissionTemplate`
   - `PromotionRequest`
   - `RoleMaturityStatus`
   - `DocumentType`
   - `UserDocument`
3. Migrar los repositorios restantes de in-memory a SQL Server.
4. Extender la cobertura de outbox transaccional mas alla de los agregados ya migrados a SQL.

## 3. Detalle de Seguimiento por Agregado

### 3.1 Identity

#### Tenant
- Capacidades API faltantes:
  - actualizar datos base del tenant
  - editar datos de branch
  - editar datos de identity provider
- Persistencia:
  - SQL Server implementado
- Siguiente paso recomendado:
  - agregar comandos de actualizacion para tenant, branch e identity provider

#### UserAccount
- Capacidades API faltantes:
  - enrolar MFA
  - verificar challenge MFA
- Capacidades API implementadas:
  - establecer o rotar password local con hash BCrypt generado en servidor
  - exponer estado de password activa y fecha de ultima rotacion sin retornar hashes
  - registrar intento de autenticacion
- Persistencia:
  - SQL Server implementado
- Siguiente paso recomendado:
  - exponer el ciclo de vida MFA; activar o remover passwords historicas permanece inhabilitado por diseno de auditoria

### 3.2 Authorization

#### Profile
- Capacidades API faltantes:
  - asignar template
  - override allow/deny/neutral
  - activar o desactivar permisos individuales
- Persistencia:
  - SQL Server implementado

#### SystemSuite
- Capacidades API faltantes:
  - ciclo de vida de modules
  - ciclo de vida de menus
  - ciclo de vida de submenus
  - ciclo de vida de options
  - ciclo de vida de actions
  - ciclo de vida de app settings
- Persistencia:
  - sigue en in-memory

#### PermissionTemplate
- Capacidades API faltantes:
  - deprecate template
  - add item
  - actualizar estado allow/deny/neutral del item
  - activar o desactivar item
  - remover item
- Persistencia:
  - sigue en in-memory

### 3.3 Approvals

#### ApprovalWorkflow
- Capacidades API faltantes:
  - agregar required document
  - remover required document
- Persistencia:
  - sigue en in-memory

#### ApprovalRequest
- Capacidades API faltantes:
  - revision de ciclo de vida avanzado si el negocio requiere expiracion o cancelacion
- Persistencia:
  - sigue en in-memory

#### DocumentType
- Capacidades API faltantes:
  - actualizar document type
  - configurar notification rules
  - remover notification rules
  - definir enforcement policy
  - actualizar enforcement policy
- Persistencia:
  - sigue en in-memory

#### UserDocument
- Capacidades API faltantes:
  - rechazar documento
  - expirar documento
  - recargar documento
  - registrar notificacion enviada
  - registrar enforcement ejecutado
- Persistencia:
  - sigue en in-memory

#### AccessEnforcementPolicy
- Capacidades API faltantes:
  - actualizar action
- Persistencia:
  - sigue en in-memory

#### NotificationRule
- Capacidades API faltantes:
  - actualizar recipient o configuracion de la regla
- Persistencia:
  - sigue en in-memory

### 3.4 IGA

#### PromotionRequest
- Capacidades API faltantes:
  - aprobacion o rechazo de manager
  - revision de seguridad low-risk o high-risk
  - aprobacion o rechazo de seguridad
  - execute
  - verify
  - mark verification failed
  - add impact analysis
- Persistencia:
  - sigue en in-memory

#### RoleMaturityStatus
- Capacidades API faltantes:
  - registrar certificacion completada
  - registrar entrenamiento completado
  - actualizar performance score
  - marcar issue de compliance
  - resolver issue de compliance
  - revisar elegibilidad
- Persistencia:
  - sigue en in-memory

### 3.5 Audit

#### AuditRecord
- Implementacion faltante:
  - repositorio SQL Server
  - revision completa de alineamiento con outbox y ledger inmutable

### 3.6 Configuration

#### AppConfiguration
- Implementacion faltante:
  - repositorios
  - comandos y queries de aplicacion
  - endpoints REST
  - queries GraphQL
  - persistencia SQL Server

#### FeatureFlag
- Implementacion faltante:
  - repositorios
  - comandos y queries de aplicacion
  - endpoints REST
  - queries GraphQL
  - persistencia SQL Server

#### IdpConfiguration
- Implementacion faltante:
  - repositorios
  - comandos y queries de aplicacion
  - endpoints REST
  - queries GraphQL
  - persistencia SQL Server

## 4. Orden Recomendado de Continuacion

1. Completar el contexto Configuration en la API
2. Terminar la exposicion de comportamiento de `UserAccount` y `Profile`
3. Terminar `SystemSuite` y `PermissionTemplate`
4. Terminar `PromotionRequest` y `RoleMaturityStatus`
5. Terminar los comportamientos pendientes de Approvals
6. Migrar los agregados restantes de in-memory a SQL Server

---
**Ultima actualizacion:** 2026-05-21
