# UserAccount — Arquitectura del Agregado

> **Idioma:** [English](../../domain/identity/user-account.md) | [Español](./user-account.md)

**Bounded Context:** Identity
**Aggregate Root:** `UserAccount`
**Modulo:** `Ums.Domain.Identity.UserAccount`
**Estado:** Produccion

---

## 1. Descripcion del Agregado

### Proposito
`UserAccount` representa la identidad digital de un usuario dentro de un Tenant. Es el punto central de autenticacion, gestion del ciclo de vida de credenciales y configuracion de MFA. Posee `PasswordCredential` y `MfaEnrollment` como entidades propias.

### Responsabilidad de Negocio
- Gestionar el ciclo de vida del usuario: registro, activacion, bloqueo y restauracion.
- Proveer la identidad central para autenticacion local y federada.
- Controlar credenciales de contrasena (`PasswordCredential`) con historial de rotacion y asegurar rotacion segura.
- Administrar metodos MFA (`MfaEnrollment`) enrollados por el usuario de forma independiente (TOTP, SMS, Email, WebAuthn).

**PasswordCredential**: Almacena el hash BCrypt de la contrasena para autenticacion local. Soporta rotacion de credenciales con registros historicos (inactivos).
**MfaEnrollment**: Registra el enrolamiento de un usuario en un metodo MFA especifico. Se pueden enrolar multiples metodos por usuario, cada uno con su propio ciclo de vida (Enrolled, Pending, Revoked).

### Aggregate Root
`UserAccount` es su propio aggregate root. Todas las mutaciones de `PasswordCredential` y `MfaEnrollment` pasan por comandos de `UserAccount`.

### Invariantes y Reglas de Consistencia
1. **UserAccount**: `Email` debe ser unico dentro del mismo `TenantId`.
2. **UserAccount/PasswordCredential**: Un usuario `FEDERATED` (con `IdentityReference`) no debe tener `PasswordCredential` activa.
3. **PasswordCredential**: A lo sumo una `PasswordCredential` con `IsActive = true` por usuario.
4. **PasswordCredential**: Establecer una nueva contrasena desactiva automaticamente la credencial activa anterior.
5. **PasswordCredential**: `PasswordHash` debe ser un hash BCrypt valido. Las credenciales historicas se conservan para auditoria y no se eliminan.
6. **MfaEnrollment**: Un usuario puede enrolar cada `MfaMethod` a lo sumo una vez — sin metodos duplicados.
7. **MfaEnrollment**: Transiciones de estado del enrolamiento: `Pending -> Enrolled -> Revoked`.
8. **MfaEnrollment**: `UserAccount.Status` debe ser `Active` para enrolar un nuevo metodo MFA.
9. **MfaEnrollment**: Al menos un metodo enrollado debe permanecer si el tenant requiere MFA.

### Entidades Relacionadas / Value Objects
| Entidad / VO | Tipo | Notas |
|---|---|---|
| `TenantId` | Value Object | FK al Tenant propietario |
| `BranchId` | Value Object | FK opcional a Branch |
| `Email` | Value Object | Unico por TenantId |
| `UserCategory` | Enum | EMPLOYEE · CONTRACTOR · EXTERNAL |
| `UserStatus` | Enum | Pending · Active · Blocked · Inactive |
| `IdentityReference` | Value Object | Sub de IdP externo (nullable) |
| `IdentityReferenceType` | Enum | OIDC · SAML2 · WS_FED (nullable) |
| `AuditValueObject` | Value Object | CreatedAt/By, UpdatedAt/By |

### Eventos de Dominio
| Evento | Disparador |
|---|---|
| `UserRegisteredEvent` | Usuario registrado en el sistema |
| `UserActivatedEvent` | Usuario activado (Pending o Blocked -> Active) |
| `UserBlockedEvent` | Usuario bloqueado |
| `UserRestoredEvent` | Usuario restaurado desde bloqueado |
| `MfaEnrolledEvent` | Nuevo metodo MFA enrollado |
| `MfaVerifiedEvent` | Desafio MFA completado exitosamente |
| `AuthenticationAttemptedEvent` | Intento de autenticacion registrado |

*(Nota: Las operaciones de contrasena alimentan la auditoria y no tienen un evento dedicado propio)*

### Comandos / Casos de Uso
| Comando | Descripcion |
|---|---|
| `RegisterUserCommand` | Registrar nuevo usuario |
| `ActivateUserCommand` | Activar usuario pendiente o bloqueado |
| `BlockUserCommand` | Bloquear usuario activo |
| `RestoreUserCommand` | Restaurar usuario bloqueado |
| `SetPasswordCommand` | Crear o rotar credencial de contrasena activa |
| `DeactivatePasswordCommand` | Desactivar credencial (ej. en federacion de cuenta) |
| `EnrollMfaCommand` | Enrolar nuevo metodo MFA |
| `VerifyMfaCommand` | Confirmar desafio MFA (Pending -> Enrolled) |
| `RevokeMfaEnrollmentCommand` | Revocar metodo MFA enrollado |
| `LinkExternalIdentityCommand` | Vincular identidad federada |

---

## 2. Modelo de Objetos

```
UserAccount (Aggregate Root)
├── Props: UserAccountProps
│   ├── Id: IdValueObject
│   ├── TenantId: TenantId
│   ├── BranchId?: BranchId
│   ├── Email: Email
│   ├── UserCategory: UserCategory
│   ├── Status: UserStatus
│   ├── IdentityReference?: IdentityReference
│   ├── IdentityReferenceType?: IdentityReferenceType
│   └── Audit: AuditValueObject
├── PasswordCredential (Entidad Propia, 0..N almacenadas, 0..1 activa)
│   └── Props: PasswordCredentialProps
│       ├── Id: IdValueObject
│       ├── UserAccountId: UserAccountId
│       ├── PasswordHash: PasswordHash
│       ├── IsActive: bool
│       └── Audit: AuditValueObject
└── MfaEnrollment (Entidad Propia, 0..N)
    └── Props: MfaEnrollmentProps
        ├── Id: IdValueObject
        ├── UserAccountId: UserAccountId
        ├── Method: MfaMethod
        ├── Status: MfaEnrollmentStatus
        └── Audit: AuditValueObject
```

### Ciclo de Vida
**UserAccount**:
```
Pending ──► Active ──► Blocked ──► Active
Active ──► Inactive (terminal)
```
**PasswordCredential**:
```
Nueva Credencial (IsActive = true)
    ↓ (en SetPassword)
Credencial Anterior (IsActive = false) — retenida para historial
```
**MfaEnrollment**:
```
Pending ──► Enrolled ──► Revoked
```

---

## 3. Diagramas de Secuencia

### Flujo: Registrar Usuario
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as RegisterUserHandler
    participant R as IUserAccountRepository

    C->>H: RegisterUserCommand(tenantId, email, category, createdBy)
    H->>R: ExistsByEmail(tenantId, email)
    R-->>H: false
    H->>H: Crear UserAccount (Status = Pending)
    H->>H: Emitir UserRegisteredEvent
    H->>R: Add(userAccount)
    H-->>C: userId
```

### Flujo: Bloquear Usuario
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as BlockUserHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)

    C->>H: BlockUserCommand(userId, reason, actorId)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.Block(reason, actorId)
    U->>U: Guardia: Status debe ser Active
    U->>U: Status = Blocked
    U->>U: Emitir UserBlockedEvent
    H->>R: Update(userAccount)
    H-->>C: void
```

### Flujo: Establecer Contrasena
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as SetPasswordHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)
    participant P as IPasswordHashingService

    C->>H: SetPasswordCommand(userId, plainPassword, actorId)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>P: Hash(plainPassword)
    P-->>H: bcryptHash
    H->>U: userAccount.SetPassword(credentialId, bcryptHash, actorId)
    U->>U: Buscar PasswordCredential activa
    U->>U: Establecer IsActive = false en existente
    U->>U: Crear nueva PasswordCredential (IsActive = true)
    H->>R: Update(userAccount)
    H-->>C: void
```

### Flujo: Desactivar Credencial (en federacion)
```mermaid
sequenceDiagram
    participant H as LinkExternalIdentityHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)

    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.LinkExternalIdentity(ref, refType, actorId)
    U->>U: Establecer IdentityReference + IdentityReferenceType
    U->>U: Buscar PasswordCredential activa
    U->>U: Establecer PasswordCredential.IsActive = false
    H->>R: Update(userAccount)
```

### Flujo: Enrolar MFA
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as EnrollMfaHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)
    participant MFA as IMfaChallengeService

    C->>H: EnrollMfaCommand(userId, method, actorId)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.EnrollMfa(enrollmentId, method, actorId)
    U->>U: Guardia: metodo no ya enrollado
    U->>U: Guardia: usuario activo
    U->>U: Crear MfaEnrollment (Status = Enrolled)
    U->>U: Emitir MfaEnrolledEvent
    H->>R: Update(userAccount)
    H->>MFA: InitiateSetup(userId, method)
    MFA-->>H: setupToken
    H-->>C: enrollmentId, setupToken
```

### Flujo: Verificar MFA
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as VerifyMfaHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)
    participant MFA as IMfaChallengeService

    C->>H: VerifyMfaCommand(userId, enrollmentId, otp, actorId)
    H->>MFA: Validate(userId, method, otp)
    MFA-->>H: valido
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.VerifyMfa(enrollmentId, actorId)
    U->>U: Enrollment.Status = Enrolled
    U->>U: Emitir MfaVerifiedEvent
    H->>R: Update(userAccount)
    H-->>C: void
```

### Flujo: Revocar MFA
```mermaid
sequenceDiagram
    participant C as Cliente
    participant H as RevokeMfaHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)

    C->>H: RevokeMfaEnrollmentCommand(userId, enrollmentId, actorId)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.RevokeMfa(enrollmentId, actorId)
    U->>U: Guardia: ultimo enrolamiento no eliminado si tenant requiere MFA
    U->>U: Enrollment.Status = Revoked
    H->>R: Update(userAccount)
    H-->>C: void
```

---

## 4. Modelo Entidad-Relacion

```mermaid
erDiagram
    TENANT ||--o{ USER_ACCOUNT : "tiene"
    USER_ACCOUNT ||--o{ PASSWORD_CREDENTIAL : "autenticado_con"
    USER_ACCOUNT ||--o{ MFA_ENROLLMENT : "enrolla_mfa"

    USER_ACCOUNT {
        uniqueidentifier UserId PK
        uniqueidentifier TenantId FK
        uniqueidentifier BranchId "Nullable FK"
        nvarchar Email "Unico por TenantId"
        nvarchar UserCategory "EMPLOYEE-CONTRACTOR-EXTERNAL"
        nvarchar Status "PENDING-ACTIVE-BLOCKED-INACTIVE"
        nvarchar IdentityReference "Nullable"
        nvarchar IdentityReferenceType "Nullable"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }

    PASSWORD_CREDENTIAL {
        uniqueidentifier CredentialId PK
        uniqueidentifier UserAccountId FK
        nvarchar PasswordHash "Hash BCrypt - solo escritura"
        bit IsActive "Solo uno true por UserAccount"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }

    MFA_ENROLLMENT {
        uniqueidentifier MfaEnrollmentId PK
        uniqueidentifier UserAccountId FK
        nvarchar Method "TOTP-SMS-EMAIL-WEBAUTHN"
        nvarchar Status "ENROLLED-PENDING-REVOKED"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }
```

---

## 5. Modelo de Bounded Context

```mermaid
flowchart TD
    subgraph Identity["Identity BC"]
        T[Tenant AR]
        UA[UserAccount AR]
        PC[PasswordCredential Entity]
        MFA[MfaEnrollment Entity]
        UA -->|TenantId| T
        UA --> PC
        UA --> MFA
    end

    subgraph Authorization["Authorization BC"]
        PROF[Profile AR]
    end

    subgraph Infra["Infrastructure"]
        AUTH[Authentication Service]
        HASH[Password Hashing Service]
        TOTP[TOTP Service]
        SMS[SMS Gateway]
        WA[WebAuthn Authenticator]
    end

    subgraph Audit["Audit BC"]
        AUD[AuditRecord]
    end

    PROF -->|UserId| UA
    UA -->|eventos de dominio| AUD
    HASH -->|Hash BCrypt| PC
    AUTH -->|Verificar contrasena| PC
    PC -->|Evento PASSWORD_SET| AUD
    TOTP -->|Validacion OTP| MFA
    SMS -->|Entrega OTP| MFA
    WA -->|Verificacion de Asercion| MFA
    MFA -->|MFA_ENROLLED| AUD
    MFA -->|MFA_VERIFIED| AUD
```

---

## 6. Contrato de Capa de Aplicacion

### Comandos
| Comando | Entrada | Salida |
|---|---|---|
| `RegisterUserCommand` | `tenantId, email, category, createdBy` | `Guid userId` |
| `ActivateUserCommand` | `userId, actorId` | `void` |
| `BlockUserCommand` | `userId, reason, actorId` | `void` |
| `SetPasswordCommand` | `userId, plainPassword, actorId` | `void` |
| `EnrollMfaCommand` | `userId, method, actorId` | `Guid enrollmentId, setupToken` |
| `VerifyMfaCommand` | `userId, enrollmentId, otp, actorId` | `void` |
| `RevokeMfaEnrollmentCommand` | `userId, enrollmentId, actorId` | `void` |

### Consultas
| Consulta | Retorna |
|---|---|
| `GetUserMfaEnrollmentsQuery(userId)` | `List<MfaEnrollmentDto>` |

### Casos de Error
| Codigo | Condicion |
|---|---|
| `USER_EMAIL_DUPLICATE` | Email ya existe en el tenant |
| `USER_NOT_FOUND` | userId desconocido |
| `USER_NOT_ACTIVE` | Operacion requiere usuario activo |
| `USER_IS_FEDERATED` | No se puede establecer contrasena en usuario federado |
| `PASSWORD_HASH_INVALID` | Fallo en validacion del hash |
| `MFA_METHOD_ALREADY_ENROLLED` | Metodo MFA ya enrollado |
| `MFA_ENROLLMENT_NOT_FOUND` | enrollmentId desconocido |
| `MFA_LAST_ENROLLMENT` | Revocar dejaria al usuario sin MFA (requerido) |
| `MFA_VERIFICATION_FAILED` | OTP invalido |

---

## 7. Notas de Persistencia

### Indices
| Indice | Columnas | Tipo |
|---|---|---|
| `IX_UserAccount_TenantId_Email` | `TenantId, Email` | Unico |
| `IX_UserAccount_TenantId` | `TenantId` | No unico |
| `IX_PasswordCredential_UserAccountId_IsActive` | `UserAccountId, IsActive` | No unico |
| `IX_MfaEnrollment_UserAccountId_Method` | `UserAccountId, Method` | Unico (solo activos) |

### Seguridad y Restricciones Unicas
- `(UserAccountId, Method)` — solo un enrolamiento por metodo por usuario activo.
- La columna `PasswordHash` nunca debe aparecer en proyecciones de consultas retornadas a clientes.
- `PasswordHash` nunca debe aparecer en payloads `AuditRecord.WhatChanged`.
- La columna debe estar encriptada en reposo (SQL Server Always Encrypted o TDE).

---

## 8. Seguridad y Auditoria

### Reglas de Autorizacion
| Operacion | Rol Requerido |
|---|---|
| Registrar Usuario | Tenant:Admin · Tenant:UserManager |
| Bloquear / Restaurar | Tenant:Admin |
| Establecer Contrasena | Usuario mismo o Tenant:Admin |
| Leer Credencial (solo IsActive) | Tenant:Admin |
| Leer Hash | Nadie — solo escritura |
| Enrolar MFA | Usuario mismo |
| Revocar MFA | Usuario mismo o Tenant:Admin |
| Verificar MFA | Usuario mismo |

### Datos Sensibles
- `PasswordHash` es el campo mas sensible del sistema. El acceso de lectura debe ser bloqueado a nivel de repositorio.
- `Email` es PII — enmascarado en logs.

### Eventos de Auditoria
- `USER_REGISTERED`, `USER_ACTIVATED`, `USER_BLOCKED`, `USER_RESTORED`
- `PASSWORD_SET` — registrado con `actorId`, `userId`, timestamp. Hash nunca registrado.
- `MFA_ENROLLED`, `MFA_VERIFIED`, `MFA_REVOKED`

### Cumplimiento
- GDPR: El hash no es PII, pero la presencia de un registro de credencial implica cuenta local. Al borrar cuenta, el hash debe ser anulado.
- NIST 800-63B: BCrypt con factor de costo apropiado requerido.
- Los registros de enrolamiento MFA deben conservarse para trazabilidad de auditoria incluso despues de la revocacion.
- Las credenciales WebAuthn (passkeys) nunca deben almacenar datos raw de atestigamiento FIDO en el modelo de dominio — eso pertenece a la capa de infraestructura.
