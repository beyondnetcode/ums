# DocumentType — Arquitectura de Agregados

**Contexto Delimitado:** Aprobaciones  
**Raíz de Agregado:** `DocumentType`  
**Módulo:** `Ums.Domain.Approvals.DocumentType`  
**Estado:** Producción

---

## 1. Visión General del Agregado

### Propósito
El agregado `DocumentType` gobierna las clasificaciones, reglas y esquemas de políticas para los documentos subidos por los usuarios (ej., Pasaportes, Documentos de certificación). Declara umbrales críticos (`Criticity`) y configura acciones proactivas de cumplimiento de seguridad (entidad `EnforcementPolicy`) para ejecutarse automáticamente cuando un documento obligatorio expira o se elimina. `NotificationRule` es un Aggregate Root independiente reutilizable para reglas de notificación y solo puede ser referenciado por `DocumentType`.

### Responsabilidad de Negocio
- Registrar y clasificar documentos de verificación corporativa.
- Establecer pautas de criticidad (Baja, Media, Alta, Crítica).
- Referenciar reglas de notificación dinámicas sin acoplar el ciclo de vida de `NotificationRule` al catálogo documental.
- Definir canales de transmisión de alertas (correo electrónico, SMS, etc.).
- Definir bloqueos automáticos de cumplimiento (ej., Bloqueo de acceso, restricción de perfiles) cuando fallan los elementos críticos de cumplimiento.

### Raíz de Agregado
`DocumentType` es la raíz del agregado. Definir acciones de cumplimiento debe realizarse a través de él para aplicar las invariantes. `NotificationRule` no forma parte de su ciclo de vida y no puede modelarse como entidad hija.

### Invariantes y Reglas de Consistencia
1. Cada `DocumentType` debe poseer un `Code` único dentro de su espacio de nombres de `TenantId`.
2. **Unicidad de DaysBefore (INV-DT2)**: Los umbrales de alerta (`DaysBefore`) en las `NotificationRules` referenciadas deben ser únicos en la lista de reglas asociadas.
3. **Mandatos Críticos (INV-DT1)**: Si un `DocumentType` se establece en `Critical`, debe tener exactamente una `EnforcementPolicy` activa definida para garantizar el cumplimiento del sistema.
4. **Política Única (INV-DT3)**: Solo se permite una `EnforcementPolicy` activa por `DocumentType`.
5. **Coincidencia de Criticidad (INV-DT4)**: Los tipos de documentos que no son críticos/altos no pueden aplicar acciones de cumplimiento como `BlockUser` o `RestrictProfile`.
6. **Días Antes Válidos (INV-NR1)**: En `NotificationRule`, `DaysBefore` debe ser un número entero positivo estrictamente mayor que cero.
7. **Canales Válidos (INV-NR2)**: La colección `Channels` en `NotificationRule` debe contener al menos un canal de notificación válido (Email, SMS, WebPortal) y no puede ser nula ni vacía.

### Entidades Relacionadas / Objetos de Valor
| Entidad / VO | Tipo | Propietario | Descripción |
|---|---|---|---|
| `DocumentTypeId` | Objeto de Valor | | Identificador de raíz de agregado |
| `DocumentCriticity` | Enumerado | | LOW · MEDIUM · HIGH · CRITICAL |
| `NotificationRule` | Agregado Raíz | Referencia externa | Define el umbral de advertencia reactivo de expiración |
| `NotificationRuleId` | Objeto de Valor | Referencia externa | Identificador único de la regla de notificación |
| `NotificationChannel` | Enumerado | Referencia externa | EMAIL · SMS · IN_APP · WEB_PUSH |
| `EnforcementPolicy` | Entidad | Propia | Detalla períodos de gracia y bloqueos |
| `Code` | Objeto de Valor | Entidad Hija | Identificador de tipo de notificación |
| `AuditValueObject` | Objeto de Valor | | Rastrea metadatos de creación y modificación |

### Eventos de Dominio
| Evento | Desencadenante |
|---|---|
| `DocumentTypeRegisteredEvent` | Se registra con éxito una nueva categoría de documento |
| `NotificationRuleConfiguredEvent` | Se configura una regla de pre-alerta de vencimiento |
| `NotificationRuleRemovedEvent` | Se elimina una regla de pre-alerta |
| `EnforcementPolicyDefinedEvent` | Se define un bloqueo de cumplimiento |
| `EnforcementPolicyUpdatedEvent` | Se actualizan los parámetros de cumplimiento |

### Comandos / Casos de Uso
| Comando | Descripción |
|---|---|
| `CreateDocumentTypeCommand` | Registrar un nuevo tipo de documento con parámetros por defecto |
| `ConfigureNotificationRuleCommand` | Configurar un nuevo umbral de alerta y sus canales en el agregado independiente `NotificationRule` |
| `RemoveNotificationRuleCommand` | Eliminar una regla de pre-alerta de notificación existente |
| `DefineEnforcementPolicyCommand` | Añadir una política de cumplimiento de bloqueo o degradación de perfil |
| `UpdateEnforcementPolicyCommand` | Modificar las acciones o períodos de gracia de una política |

### Límites de Repositorio / Servicio
- `IDocumentTypeRepository` — Persiste los esquemas de clasificación.
- Acotado estrictamente por `TenantId` para evitar cruces de configuración entre inquilinos.

---

## 2. Modelo de Dominio

### Clases / Entidades / Objetos de Valor
```
DocumentType (Raíz de Agregado)
├── Props: DocumentTypeProps
│   ├── Id: DocumentTypeId
│   ├── TenantId: TenantId
│   ├── Code: Code
│   ├── Name: Name
│   ├── Description: Description
│   ├── Criticity: DocumentCriticity
│   └── Audit: AuditValueObject
└── Hijo (Anulable)
    └── EnforcementPolicy
```

---

## 3. Diagramas de Modelo de Objetos

```mermaid
classDiagram
    direction LR
    class DocumentType {
        +Guid Id
        +Guid TenantId
        +Code Code
        +Name Name
        +Description Description
        +DocumentCriticity Criticity
        +EnforcementPolicy EnforcementPolicy
        +Create()
        +DefineEnforcementPolicy()
    }
    class DocumentCriticity {
        <<enumeration>>
        LOW
        MEDIUM
        HIGH
        CRITICAL
    }
    class EnforcementPolicy {
        +Guid Id
        +AccessEnforcementAction ActionOnExpiration
        +int? GracePeriodDays
    }
    DocumentType "1" *-- "1" DocumentCriticity
    DocumentType "1" *-- "0..1" EnforcementPolicy
```

---

## 4. Diagramas de Secuencia

### Flujo para Definir Política de Cumplimiento
```mermaid
sequenceDiagram
    participant C as AdministradorInquilino
    participant H as DefinePolicyHandler
    participant R as IDocumentTypeRepository
    participant D as DocumentType (AR)

    C->>H: DefineEnforcementPolicyCommand(docTypeId, action, graceDays)
    H->>R: GetById(docTypeId)
    R-->>H: DocumentType (AR)
    H->>D: DefineEnforcementPolicy(action, graceDays, actorId)
    D->>D: Validar INV-DT4 (restricciones no críticas)
    D->>D: Validar INV-DT3 (límite de política única)
    D->>D: Levantar EnforcementPolicyDefinedEvent
    H->>R: Save(docType)
    R-->>H: ok
    H-->>C: ok
```

---

## 5. Modelo ER

```mermaid
erDiagram
    TENANT ||--o{ DOCUMENT_TYPE : "configura"
    DOCUMENT_TYPE ||--o{ NOTIFICATION_RULE : "define"
    DOCUMENT_TYPE ||--o| ENFORCEMENT_POLICY : "restringe"

    DOCUMENT_TYPE {
        uniqueidentifier DocumentTypeId PK
        uniqueidentifier TenantId FK
        nvarchar Code "Único por TenantId"
        nvarchar Name
        nvarchar Description
        nvarchar Criticity "LOW-MEDIUM-HIGH-CRITICAL"
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }
    NOTIFICATION_RULE {
        uniqueidentifier RuleId PK
        uniqueidentifier DocumentTypeId FK
        int DaysBefore "Único por DocumentTypeId"
        nvarchar ChannelsJson "Matriz serializada de canales"
        nvarchar Code
        nvarchar Description
    }
    ENFORCEMENT_POLICY {
        uniqueidentifier PolicyId PK
        uniqueidentifier DocumentTypeId FK
        nvarchar ActionOnExpiration "BlockUser-RestrictProfile-Notify"
        int GracePeriodDays "Nullable"
    }
```

### Reglas de Aislamiento de Inquilinos
- Los esquemas clasificados están particionados estrictamente por `TenantId`. Todas las consultas de enrutamiento de verificación imponen límites de aislamiento.
- `NotificationRule` es un Aggregate Root independiente y conserva su propio ciclo de vida.

---

## 6. Integración de Contexto Delimitado
- **Aguas Arriba**: Hereda las reglas de contexto de `Identidad` (validando registros de inquilinos).
- **Aguas Abajo**: Consultado por `UserDocument` para verificar los umbrales de alerta, y por `AccessEnforcementPolicy` durante los pases de verificación de cumplimiento. Las alertas configuradas a través de `NotificationRule` son procesadas por ejecutores en segundo plano para notificar a los usuarios.

---

## 7. Capa de Aplicación
- `CreateDocumentTypeCommand` -> Entradas: `TenantId, Code, Name, Description, Criticity` -> Retorna: `Guid`
- `ConfigureNotificationRuleCommand` -> Entradas: `NotificationRuleId, DaysBefore, Channels, Code, Description` -> Retorna: `void`
- `RemoveNotificationRuleCommand` -> Entradas: `NotificationRuleId` -> Retorna: `void`
- `DefineEnforcementPolicyCommand` -> Entradas: `DocumentTypeId, Action, GracePeriodDays?` -> Retorna: `void`

---

## 8. Infraestructura/Persistencia
- Índice: Índice único en `TenantId, Code`. En `NotificationRule`, índice compuesto en `TenantId, Code` para asegurar unicidad.
- Transacción: Las actualizaciones de hijos (políticas y entradas de reglas de pre-alerta) se almacenan de forma atómica dentro de la transacción de base de datos del padre `DOCUMENT_TYPE`.

---

## 9. Seguridad y Cumplimiento
- Ajustar la clasificación o reglas críticas: Restringido estrictamente a los roles de `Tenant:Admin`.
- Cumplimiento: Alterar las reglas de cumplimiento representa un alto impacto de seguridad y desencadena un registro de auditoría de alta gravedad.

---

## 10. Decisiones Técnicas
- Consolidar `DocumentType` como catálogo y `NotificationRule` como agregado independiente protege los límites del dominio contra restricciones divididas.
- Almacenar los canales de comunicación permitidos como una matriz serializada (`ChannelsJson`) garantiza la flexibilidad sin sobrecargar de consultas complejas.

---

**[Volver al Índice de Aprobaciones](./index.md)**
