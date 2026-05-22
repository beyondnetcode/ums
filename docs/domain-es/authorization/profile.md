# Profile — Arquitectura de Agregados

**Contexto Delimitado:** Autorización  
**Raíz de Agregado:** `Profile`  
**Módulo:** `Ums.Domain.Authorization.Profile`  
**Estado:** Producción

---

## 1. Visión General del Agregado

### Propósito
El agregado `Profile` representa un rol de seguridad dinámico asignado a los usuarios del sistema. Orquesta las asignaciones de permisos mediante el mapeo de operaciones de suite (acciones) a alcances de acceso específicos (GLOBAL, TENANT o BRANCH). Esto determina qué acciones puede ejecutar un usuario y precisamente qué segmentos de datos (inquilinos/sucursales) tiene permitido ver o modificar.

### Responsabilidad de Negocio
- Actuar como el mecanismo central de roles de autorización.
- Hacer cumplir los límites de los alcances de seguridad (niveles Global vs Inquilino vs Sucursal).
- Gestionar los permisos dinámicos del perfil a través de entidades propias `ProfilePermission`.
- Controlar las reglas de asignación y ciclos de vida de los roles.

### Raíz de Agregado
`Profile` es la raíz del agregado. Todos los ajustes de permisos o transiciones de estado deben pasar por comandos de `Profile`.

### Invariantes y Reglas de Consistencia
1. El `Name` de un perfil debe ser único dentro de su ámbito de `TenantId`.
2. Un perfil marcado con `Scope = GLOBAL` no puede tener una restricción de alcance de `TenantId` o `BranchId`.
3. Un perfil marcado con `Scope = TENANT` debe tener un `TenantId` válido.
4. Un perfil marcado con `Scope = BRANCH` debe tener un `TenantId` y un `BranchId` válidos.
5. Si el inquilino propietario se suspende, todos los perfiles asignados a ese inquilino se suspenden implícitamente (R-10).

### Entidades Relacionadas / Objetos de Valor
| Entidad / VO | Tipo | Propietario |
|---|---|---|
| `ProfilePermission` | Entidad | Propia (ver [profile-permission.md](./profile-permission.md)) |
| `ProfileScope` | Enum | GLOBAL · TENANT · BRANCH |
| `ProfileName` | Objeto de Valor | Nombre del rol de visualización alfanumérico |

### Eventos de Dominio
- `ProfileCreatedEvent`
- `ProfileScopeAdjustedEvent`
- `ProfilePermissionGrantedEvent`
- `ProfilePermissionRevokedEvent`
- `ProfileDeactivatedEvent`

---

## 2. Modelo de Dominio

### Clases / Entidades / Objetos de Valor
```
Profile (Raíz de Agregado)
├── Props: ProfileProps
│   ├── Id: IdValueObject
│   ├── TenantId?: TenantId
│   ├── BranchId?: BranchId
│   ├── Name: ProfileName
│   ├── Scope: ProfileScope
│   ├── IsActive: bool
│   └── Audit: AuditValueObject
└── Hijos
    └── IReadOnlyList<ProfilePermission>
```

---

## 3. Diagramas de Modelo de Objetos

```mermaid
classDiagram
    class Profile {
        +Guid Id
        +Guid? TenantId
        +Guid? BranchId
        +ProfileName Name
        +ProfileScope Scope
        +bool IsActive
        +List~ProfilePermission~ Permissions
        +Create()
        +GrantPermission()
        +RevokePermission()
        +Deactivate()
    }
    class ProfilePermission {
        +Guid Id
        +Guid ProfileId
        +Guid ActionId
        +string PermissionKey
    }
    Profile "1" *-- "0..*" ProfilePermission
```

---

## 4. Diagramas de Secuencia

### Flujo para Crear un Perfil
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as CreateProfileHandler
    participant R as IProfileRepository
    participant P as Profile (AR)

    C->>H: CreateProfileCommand(tenantId, branchId, name, scope)
    H->>R: ExistsByName(tenantId, name)
    R-->>H: false
    H->>P: Profile.Create(id, tenantId, branchId, name, scope)
    P->>P: Validar invariantes basados en el alcance
    P->>P: Levantar ProfileCreatedEvent
    H->>R: Add(profile)
    R-->>H: ok
    H-->>C: ProfileId
```

---

## 5. Modelo ER

```mermaid
erDiagram
    PROFILE ||--o{ PROFILE_PERMISSION : "contiene"
    TENANT ||--o{ PROFILE : "posee"
    BRANCH ||--o{ PROFILE : "limita"

    PROFILE {
        uniqueidentifier ProfileId PK
        uniqueidentifier TenantId FK "Nullable"
        uniqueidentifier BranchId FK "Nullable"
        nvarchar Name "Unique per TenantId"
        nvarchar Scope "GLOBAL-TENANT-BRANCH"
        bit IsActive
    }
    PROFILE_PERMISSION {
        uniqueidentifier GrantId PK
        uniqueidentifier ProfileId FK
        uniqueidentifier ActionId FK
        nvarchar PermissionKey
    }
```

### Reglas de Aislamiento de Inquilinos
- Los perfiles globales (`TenantId IS NULL`) se comparten en todo el sistema.
- Los perfiles delimitados por Inquilino y Sucursal se particionan estrictamente por `TenantId`. Todas las consultas de base de datos dirigidas a operaciones de inquilinos deben aplicar el filtro de inquilinos correspondiente.

---

## 6. Integración de Contexto Delimitado
- **Aguas Arriba**: Consume `TenantId` y `BranchId` del Contexto de Identidad (Identity BC).
- Consume `ActionId` de los agregados `SystemSuite`.
- Consumido por los contextos de Aprobaciones e IGA para validar solicitudes de sesión y propuestas de elevación.

---

## 7. Capa de Aplicación
- `CreateProfileCommand` -> Entradas: `TenantId?, BranchId?, Name, Scope` -> Retorna: `Guid`
- `GrantPermissionToProfileCommand` -> Entradas: `ProfileId, ActionId` -> Retorna: `void`

---

## 8. Infraestructura/Persistencia
- Índice: Índice único en `TenantId, Name` (para asegurar la unicidad del nombre dentro del inquilino).
- Transacción: Las modificaciones a `Profile` y sus elementos secundarios `ProfilePermission` se confirman dentro de una sola transacción de unidad de trabajo de EF Core.

---

## 9. Seguridad y Cumplimiento
- Diseñar / crear perfiles globales: Restringido al rol `Platform:Admin`.
- Configuración de perfiles de Inquilino / Sucursal: Restringido al rol `Tenant:Admin` (para su propio inquilino).
- Cumplimiento: La modificación de perfiles es un punto caliente de auditoría de seguridad. Todos los cambios desencadenan de inmediato registros de auditoría e invalidaciones de sesión.

---

## 10. Decisiones Técnicas
- Las restricciones de alcance (Global vs Inquilino vs Sucursal) se evalúan dentro de la lógica del agregado raíz de dominio en lugar de restricciones de base de datos para asegurar la pureza arquitectónica de la capa de DDD.

---

**[Volver al Índice de Autorización](./index.md)**
