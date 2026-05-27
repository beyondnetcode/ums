# PermissionTemplate — Arquitectura de Agregados

**Contexto Delimitado:** Autorización  
**Raíz de Agregado:** `PermissionTemplate`  
**Módulo:** `Ums.Domain.Authorization.PermissionTemplate`  
**Estado:** Producción

---

## 1. Visión General del Agregado

### Propósito
El agregado `PermissionTemplate` define paquetes de derechos de acceso (permisos) estándar y reutilizables mapeados a varios roles del sistema (ej. "Empleado Estándar", "Administrador de Sucursal", "Administrador Financiero del Inquilino"). Actúa como un plano estandarizado que simplifica y automatiza el aprovisionamiento de perfiles (roles) dinámicos cuando se registran nuevos inquilinos o se incorporan nuevos usuarios.

### Responsabilidad de Negocio
- Crear y mantener plantillas de seguridad preempaquetadas.
- Vincular las Acciones de suite granulares a una plantilla con nombre mediante `PermissionTemplateItem`.
- Facilitar configuraciones de seguridad consistentes y reproducibles entre inquilinos.

### Raíz de Agregado
`PermissionTemplate` es la raíz del agregado. Los detalles de configuración de elementos secundarios se administran dentro de la colección de entidades propias `PermissionTemplateItem`.

### Invariantes y Reglas de Consistencia
1. El `Code` de la plantilla debe ser único en todo el sistema.
2. Una plantilla debe contener al menos un `PermissionTemplateItem` para estar activa.
3. Si una `Action` subyacente en `SystemSuite` se elimina, el `PermissionTemplateItem` correspondiente se elimina automáticamente en cascada.
4. Para un `PermissionTemplateItem`, una plantilla no puede contener mapeos duplicados de `ActionId`.
5. La `PermissionKey` almacenada debe coincidir exactamente con la clave calculada dentro del catálogo `Action` en el momento de la validación.

### Entidades Relacionadas / Objetos de Valor
| Entidad / VO | Tipo | Propietario |
|---|---|---|
| `PermissionTemplateItem` | Entidad | Propia |
| `TemplateCode` | Objeto de Valor | Código de plantilla alfanumérico |
| `TemplateName` | Objeto de Valor | Descripción y etiqueta de visualización |

### Eventos de Dominio
| Evento | Desencadenante |
|---|---|
| `PermissionTemplateCreatedEvent` | Nueva plantilla creada |
| `PermissionTemplateUpdatedEvent` | Detalles de plantilla modificados |
| `PermissionTemplateDeletedEvent` | Plantilla eliminada |
| `PermissionTemplateItemAddedEvent` | Elemento mapeado agregado a la plantilla |
| `PermissionTemplateItemRemovedEvent` | Elemento mapeado removido de la plantilla |

### Comandos / Casos de Uso
| Comando | Descripción |
|---|---|
| `CreatePermissionTemplateCommand` | Crear una nueva plantilla |
| `AddTemplateItemCommand` | Agrega un mapeo de acción (elemento) a la plantilla |

---

## 2. Modelo de Dominio

### Clases / Entidades / Objetos de Valor
```
PermissionTemplate (Raíz de Agregado)
├── Props: PermissionTemplateProps
│   ├── Id: IdValueObject
│   ├── Code: TemplateCode
│   ├── Name: TemplateName
│   ├── Description: string
│   └── IsActive: bool
└── Hijos
    └── IReadOnlyList<PermissionTemplateItem>
        └── PermissionTemplateItem
            ├── Props: ItemProps
            │   ├── Id: IdValueObject
            │   ├── TemplateId: TemplateId
            │   ├── ActionId: Guid
            │   └── PermissionKey: string
```

### Atributos Principales
| Entidad | Atributo | Tipo | Notas |
|---|---|---|---|
| `PermissionTemplate` | `Id` | `Guid` | PK |
| `PermissionTemplate` | `Code` | `string` | Único |
| `PermissionTemplate` | `IsActive` | `bool` | Flag de estado |
| `PermissionTemplateItem` | `Id` | `Guid` | PK |
| `PermissionTemplateItem` | `TemplateId` | `Guid` | FK a Template |
| `PermissionTemplateItem` | `ActionId` | `Guid` | FK a Action del sistema |
| `PermissionTemplateItem` | `PermissionKey` | `string` | Clave de caché copiada |

---

## 3. Diagramas de Modelo de Objetos

```mermaid
classDiagram
    direction TB
    class PermissionTemplate {
        +Guid Id
        +TemplateCode Code
        +TemplateName Name
        +string Description
        +bool IsActive
        +List~PermissionTemplateItem~ Items
        +Create()
        +AddItem()
        +RemoveItem()
    }
    class PermissionTemplateItem {
        +Guid Id
        +Guid TemplateId
        +Guid ActionId
        +string PermissionKey
    }
    PermissionTemplate "1" *-- "0..*" PermissionTemplateItem : contiene
```

---

## 4. Diagramas de Secuencia

### Flujo para Crear una Plantilla
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as CreateTemplateHandler
    participant R as IPermissionTemplateRepository
    participant T as PermissionTemplate (AR)

    C->>H: CreatePermissionTemplateCommand(code, name, description)
    H->>R: ExistsByCode(code)
    R-->>H: false
    H->>T: PermissionTemplate.Create(id, code, name, description)
    T->>T: Levantar PermissionTemplateCreatedEvent
    H->>R: Add(template)
    R-->>H: ok
    H-->>C: TemplateId
```

---

## 5. Modelo ER

```mermaid
erDiagram
    PERMISSION_TEMPLATE ||--o{ PERMISSION_TEMPLATE_ITEM : "contiene"
    SYSTEM_SUITE ||--o{ PERMISSION_TEMPLATE_ITEM : "apunta a"
    MODULE ||--o{ PERMISSION_TEMPLATE_ITEM : "apunta a"
    DOMAIN_RESOURCE ||--o{ PERMISSION_TEMPLATE_ITEM : "apunta a"
    ACTION ||--o{ PERMISSION_TEMPLATE_ITEM : "referencia"

    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        nvarchar Code "Unique"
        nvarchar Name
        nvarchar Description
        bit IsActive
    }
    PERMISSION_TEMPLATE_ITEM {
        uniqueidentifier ItemId PK
        uniqueidentifier TemplateId FK
        int TargetTypeId "1=SystemSuite 2=Module 3=Submodule 4=Option 5=Aggregate 6=Entity"
        uniqueidentifier TargetId "Exclusive Arc"
        uniqueidentifier ActionId FK
        nvarchar PermissionKey
    }
```

### Reglas de Aislamiento de Inquilinos
- Las plantillas se pueden configurar como **Globales** (disponibles en toda la plataforma para todos los inquilinos) o **Delimitadas por Inquilino** (disponibles solo para el inquilino que las creó). Las tablas delimitadas por inquilino incluyen una columna `TenantId` que admite valores nulos.
- `PermissionTemplateItem` hereda el alcance de aislamiento del agregado padre `PermissionTemplate`.

---

## 6. Integración de Contexto Delimitado
- Consume metadatos de `Action` del agregado `SystemSuite` y mapea sus identificadores dinámicos.
- Los perfiles de seguridad aguas abajo consumen estas plantillas para inicializar permisos de perfiles predeterminados.

---

## 7. Capa de Aplicación
- `CreatePermissionTemplateCommand` -> Entradas: `Code, Name, Description` -> Retorna: `Guid`
- `AddTemplateItemCommand` -> Entradas: `TemplateId, ActionId, PermissionKey` -> Retorna: `Guid`

---

## 8. Infraestructura/Persistencia
- Límite de transacción: `PermissionTemplateItem` se guarda como parte de la persistencia de `PermissionTemplate`.
- Índice: Índice único en `Code` e índice en `TenantId` para la plantilla. Índice único en `TemplateId, ActionId` para los elementos.

---

## 9. Seguridad y Cumplimiento
- La edición de plantillas globales está restringida al rol `Platform:Admin`.
- La creación de plantillas delimitadas por inquilino está restringida al rol `Tenant:Admin`.
- El alcance administrativo sobre los elementos coincide con las reglas de la plantilla padre.

---

## 10. Decisiones Técnicas
- El uso de plantillas de inicialización estándar evita la fatiga de configuración manual durante el registro de nuevas organizaciones.
- Duplicar la `PermissionKey` calculada directamente dentro de la tabla de elementos sirve como una optimización de caché desnormalizada para los cálculos de permisos de alta velocidad.

---

**[Volver al Índice de Autorización](./index.md)**
