# 🗄️ Modelo Entidad-Relación (E/R) - SQL Server 2022

**Tipo de Documento:** Diseño de Base de Datos  
**Estatus:** Refactorizado (Vinculado al Rol y Jerarquía Estricta)  
**Arquitectura:** Framework Maestro (Control de 5 Niveles)  
**Motor:** SQL Server 2022

## 1. Introducción
Este documento detalla el modelo de autorización **Vinculado al Rol**, aplicando estrictamente la cadena jerárquica: **Sistema -> Módulo -> Sub-módulo -> Opción -> Acción**.

> [!TIP]
> **¿Problemas de Visualización?**  
> Si los diagramas Mermaid no se renderizan correctamente, utiliza los **[🚀 Formatos de Exportación Alternativos (dbdiagram.io, DDL, D2)](./er-export-formats.md)**. Estos formatos son compatibles con herramientas profesionales como DBeaver, SSMS y dbdiagram.io.

---

## 2. Estándares Corporativos de Auditoría y Trazabilidad
Todas las entidades implementan el esquema de auditoría estándar de 10 columnas.

---

## 3. Vistas Modulares por Dominio

### 🗺️ 3.1 Mapa Global de Alto Nivel
Ruta de Resolución: `Inquilino -> Sistema -> Rol -> Plantilla -> Permiso de Perfil`.

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "posee"
    TENANT ||--o{ BRANCH : "opera"
    TENANT ||--o{ USER_ACCOUNT : "posee"
    SYSTEM_SUITE ||--o{ ROLE : "define"
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "contiene"
    
    ROLE ||--o{ ROLE : "padre_de"
    ROLE ||--o{ ROLE_PROMOTION_CRITERIA : "origen"
    ROLE ||--o{ ROLE_PROMOTION_CRITERIA : "destino"
    
    TENANT ||--o{ APP_CONFIGURATION : "configuraciones"
    SYSTEM_SUITE ||--o{ APP_CONFIGURATION : "anulaciones"
    
    ROLE ||--o{ PERMISSION_TEMPLATE : "gobierna"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "materializado"
    
    USER_ACCOUNT ||--o{ PROFILE : "actúa_como"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "administra"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "es_gestionado"
    USER_ACCOUNT ||--o{ APPROVAL_REQUEST : "onboardings/aprueba"
    
    BRANCH ||--o{ PROFILE : "contexto_de"
    PROFILE ||--o{ PROFILE_PERMISSION : "autoridad_efectiva"
    
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_SUBMODULE : "contiene"
    FUNCTIONAL_OPTION ||--o{ ACTION : "ejecuta"
    
    ACTION ||--o{ PERMISSION_TEMPLATE : "acción_autorizada"
```

---

### 🔐 3.2 Dominio: Autoridad Centrada en el Rol y Jerarquía Estricta
Este dominio garantiza que cada permiso esté limitado a un Rol y se mapee exactamente a la jerarquía funcional de 5 niveles.

```mermaid
erDiagram
    ROLE ||--o{ PERMISSION_TEMPLATE : "posee"
    PERMISSION_TEMPLATE ||--o{ PROFILE_PERMISSION : "define"
    ACTION ||--o{ PERMISSION_TEMPLATE : "autorizado"
    
    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        uniqueidentifier RoleId FK
        uniqueidentifier ActionId FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier SuiteId FK "Exclusive Arc"
        uniqueidentifier ModuleId FK "Exclusive Arc"
        uniqueidentifier SubModuleId FK "Exclusive Arc"
        uniqueidentifier OptionId FK "Exclusive Arc"
        bit IsAllowed "Estado por Defecto"
        bit IsDenied "Estado por Defecto"
        bit IsActive "Estado por Defecto"
    }
    
    PROFILE {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier UserId FK
        uniqueidentifier RoleId FK
        uniqueidentifier BranchId FK "Contexto de Sucursal"
    }
    
    PROFILE_PERMISSION {
        uniqueidentifier ProfileId PK, FK
        uniqueidentifier TemplateId PK, FK
        bit IsAllowed
        bit IsDenied
        bit IsActive
    }
```

---

### 📍 3.3 Dominio: Topología Funcional (Los 5 Niveles)
Estructura organizacional de los recursos.

```mermaid
erDiagram
    SYSTEM_SUITE ||--o{ FUNCTIONAL_MODULE : "L1: Sistema -> Módulo"
    FUNCTIONAL_MODULE ||--o{ FUNCTIONAL_SUBMODULE : "L2: Módulo -> SubMódulo"
    FUNCTIONAL_SUBMODULE ||--o{ FUNCTIONAL_OPTION : "L3: SubMódulo -> Opción"
    
    FUNCTIONAL_MODULE {
        uniqueidentifier ModuleId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_SUBMODULE {
        uniqueidentifier SubModuleId PK
        uniqueidentifier ModuleId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
    }
    
    FUNCTIONAL_OPTION {
        uniqueidentifier OptionId PK
        uniqueidentifier SubModuleId FK
        uniqueidentifier TenantId FK "RLS"
        nvarchar Name
        nvarchar Code
    }
```

---

### 🛂 3.4 Dominio: Gobernanza de Identidad y Aprobaciones
Gestión del ciclo de vida del usuario, administración delegada y flujos de trabajo de incorporación para identidades externas o de alto riesgo.

```mermaid
erDiagram
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "admin"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "gestionado"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUEST : "define_reglas_para"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUIRED_DOCUMENT : "exige"
    APPROVAL_REQUEST ||--o{ USER_DOCUMENT : "evidenciado_por"
    APPROVAL_REQUIRED_DOCUMENT ||--o{ DOCUMENT_TYPE : "tipificado_como"
    
    USER_ACCOUNT ||--o{ USER_DOCUMENT : "posee"
    DOCUMENT_TYPE ||--o{ USER_DOCUMENT : "clasifica"
    DOCUMENT_TYPE ||--o{ NOTIFICATION_RULE : "alerta_para"
    DOCUMENT_TYPE ||--o{ ACCESS_ENFORCEMENT_POLICY : "gobierna_acceso"
    
    USER_ACCOUNT ||--o{ USER_PROMOTION_PROCESS : "candidato"
    ROLE ||--o{ USER_PROMOTION_PROCESS : "objetivo"
    APPROVAL_REQUEST ||--o{ USER_PROMOTION_PROCESS : "autorizado_por"
    
    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK
        nvarchar UserCategory "INTERNAL/EXTERNAL/B2B/PARTNER"
        nvarchar Status "ACTIVE/BLOCKED/PENDING"
    }

    ROLE {
        uniqueidentifier RoleId PK
        uniqueidentifier ParentRoleId FK "Auto-Ref"
        int HierarchyLevel
        int PromotionOrder
    }

    ROLE_PROMOTION_CRITERIA {
        uniqueidentifier CriteriaId PK
        uniqueidentifier SourceRoleId FK
        uniqueidentifier TargetRoleId FK
        bit FlagSeniority
        bit FlagCompliance
        bit FlagManualApproval
    }

    USER_PROMOTION_PROCESS {
        uniqueidentifier ProcessId PK
        uniqueidentifier UserId FK
        uniqueidentifier TargetRoleId FK
        nvarchar Status "EVALUATING/CRITERIA_MET/PENDING_APPROVAL/PROMOTED"
    }

    APP_CONFIGURATION {
        uniqueidentifier SettingId PK
        uniqueidentifier TenantId FK "Anulable"
        uniqueidentifier SuiteId FK "Anulable"
        nvarchar Code "Flag / Parámetro"
        nvarchar Value
        bit IsInheritable
    }

    USER_MANAGEMENT_DELEGATION {
        uniqueidentifier DelegationId PK
        uniqueidentifier ParentAdminUserId FK
        uniqueidentifier ManagedUserId FK
        uniqueidentifier SuiteId FK "Alcance Opcional"
    }
    
    APPROVAL_WORKFLOW {
        uniqueidentifier WorkflowId PK
        uniqueidentifier TenantId FK
        uniqueidentifier SuiteId FK "Anulable"
        nvarchar TargetUserCategory
        bit RequiresApproval
    }

    APPROVAL_REQUIRED_DOCUMENT {
        uniqueidentifier DocumentTypeId PK, FK
        uniqueidentifier WorkflowId FK
        bit IsMandatory
    }
    
    APPROVAL_REQUEST {
        uniqueidentifier RequestId PK
        uniqueidentifier WorkflowId FK
        uniqueidentifier TargetUserId FK
        uniqueidentifier TargetProfileId FK "Anulable"
        nvarchar RequestStatus "PENDING/APPROVED/REJECTED"
    }

    USER_DOCUMENT {
        uniqueidentifier DocumentId PK
        uniqueidentifier UserId FK
        uniqueidentifier DocumentTypeId FK
        datetime2 IssueDate
        datetime2 ExpirationDate
        nvarchar Status "VALID/EXPIRED/PENDING_RENEWAL"
        nvarchar Criticity "LOW/MEDIUM/HIGH/CRITICAL"
        nvarchar FileStoragePath "URI/Ruta al Servidor de Archivos"
    }

    NOTIFICATION_RULE {
        uniqueidentifier RuleId PK
        uniqueidentifier TenantId FK
        uniqueidentifier DocumentTypeId FK
        int DaysBefore
        nvarchar Channel "EMAIL/IN_APP/SMS"
    }

    ACCESS_ENFORCEMENT_POLICY {
        uniqueidentifier PolicyId PK
        uniqueidentifier DocumentTypeId FK
        nvarchar ActionOnExpiration "BLOCK_USER/RESTRICT_PROFILE/LOG_ONLY"
    }
```

---

## 4. Reglas de Negocio y Restricciones Técnicas
1.  **Row-Level Security (RLS)**: `TenantId` está denormalizado en todas las entidades funcionales (Module, Option, Template, Action, Role) para permitir aislamiento O(1) vía RLS nativo de SQL Server.
2.  **Arco Exclusivo (Integridad de Plantilla)**: `PermissionTemplate` implementa 4 FKs anulables que apuntan a la jerarquía de recursos. Un constraint `CHECK` garantiza que exactamente UNO tenga valor, forzando integridad referencial estricta sobre el polimorfismo.
3.  **Propiedad de Acción XOR Estricta**: Una Acción debe pertenecer a un Sistema O a un Módulo, pero nunca a ambos: `CHECK ((SuiteId IS NOT NULL AND ModuleId IS NULL) OR (SuiteId IS NULL AND ModuleId IS NOT NULL))`.
4.  **Integridad Jerárquica**: El acceso debe rastrearse a través de `Sistema > Módulo > Sub-módulo > Opción > Acción`.
5.  **Administración Delegada (Muchos-a-Muchos)**: El alcance de administración de un usuario se define mediante la tabla `USER_MANAGEMENT_DELEGATION`. Esto permite que múltiples administradores gestionen el mismo pool de usuarios, opcionalmente restringido por `SuiteId`.
6.  **Mandatos de Aprobación**: Los usuarios Externos/B2B DEBEN pasar por un `APPROVAL_WORKFLOW` antes de alcanzar un estado `ACTIVE` o de que se les asignen perfiles de alto riesgo. Los documentos de respaldo definidos en `APPROVAL_REQUIRED_DOCUMENT` deben cargarse en `USER_DOCUMENT` antes del avance del flujo.
7.  **Aplicación Automática de Cumplimiento**: Workers en segundo plano escanean `USER_DOCUMENT`. Al expirar, se activa la `ACCESS_ENFORCEMENT_POLICY`. Los documentos críticos transicionarán automáticamente el `USER_ACCOUNT` a un estado `BLOCKED` o restringirán el contexto de un `PROFILE` específico.
8.  **Notificaciones Paramétricas**: `NOTIFICATION_RULE` permite configurar N-pasos de alerta (ej. 30, 15, 5 días antes de la expiración) por Inquilino y Tipo de Documento.
