# EP-06: Diseño Detallado — Seguridad, Acceso Externo y Delegación

**Versión:** 1.0  
**Fecha:** 2026-05-14  
**Épica:** EP-06 (Post-MVP)  
**Historias:** US-017 a US-022  
**Functional Stories:** FS-09, FS-10, FS-14

---

## PARTE 1: FS-09 — Adaptive MFA & Passwordless Authentication

### 1.1 Definición

**FS-09** implementa autenticación adaptativa donde:
- **MFA**: Multi-Factor Authentication (requisito condicional basado en riesgo)
- **Passwordless**: Métodos sin contraseña (FIDO2, magic links, biometría)

El sistema calcula un **Risk Score** en tiempo real y decide automáticamente si MFA es requerido.

### 1.2 Risk Scoring Model

#### 1.2.1 Factores de Riesgo

| Factor | Rango | Peso | Ejemplo |
|--------|-------|------|---------|
| **Login Frequency Anomaly** | 0-30 | 0.20 | User nunca ha hecho login a esta hora |
| **Geographic Anomaly** | 0-30 | 0.25 | User está en país diferente al usual |
| **Device Reputation** | 0-20 | 0.15 | Device nuevo o no reconocido |
| **Network Anomaly** | 0-10 | 0.10 | IP sospechosa, VPN, proxy |
| **Failed Attempts** | 0-10 | 0.10 | N intentos fallidos recientes |
| **Tenant Risk Level** | 0-30 | 0.20 | Tenant categorizado como "high-risk" |

**Risk Score = Σ(Factor × Weight)**

Rango: 0 (bajo riesgo) a 100 (alto riesgo)

#### 1.2.2 Thresholds de Decisión

```csharp
public class MFADecisionEngine
{
    // Risk Score → MFA Requirement
    public MFARequirement CalculateMFARequirement(decimal riskScore, User user, Tenant tenant)
    {
        return (riskScore, user.Category, tenant.RiskLevel) switch
        {
            // Bajo riesgo: Sin MFA requerido
            (< 20, _, _) => MFARequirement.NotRequired,
            
            // Riesgo medio: MFA recomendado (opcional)
            (20 to 40, UserCategory.INTERNAL, _) => MFARequirement.Recommended,
            (20 to 40, _, _) => MFARequirement.Required,
            
            // Riesgo alto: MFA obligatorio
            (40 to 70, _, _) => MFARequirement.Required,
            
            // Riesgo crítico: MFA + intervención de admin
            (> 70, _, _) => MFARequirement.RequiredWithSecurityReview,
            
            _ => MFARequirement.Required
        };
    }
}

public enum MFARequirement
{
    NotRequired,                  // User puede skipear MFA
    Recommended,                  // Mostrar prompt pero permitir skip
    Required,                     // MFA obligatorio
    RequiredWithSecurityReview    // MFA + manual admin review
}
```

#### 1.2.3 Cálculo de Riesgos por Factor

```csharp
public class RiskScoringEngine
{
    // Factor 1: Login Frequency Anomaly (0-30 puntos)
    public int CalculateFrequencyAnomaly(User user, DateTime loginAttemptTime)
    {
        var userLoginHistory = _auditRepository.GetLoginsByUser(user.Id, lastDays: 30);
        var usualLoginHours = userLoginHistory
            .GroupBy(l => l.Timestamp.Hour)
            .Select(g => (hour: g.Key, frequency: g.Count()))
            .OrderByDescending(g => g.frequency)
            .Take(5)  // Top 5 horas
            .Select(g => g.hour)
            .ToList();
        
        if (!usualLoginHours.Contains(loginAttemptTime.Hour))
            return 30;  // Anomalía total
        
        return 0;  // Patrón conocido
    }
    
    // Factor 2: Geographic Anomaly (0-30 puntos)
    public int CalculateGeographicAnomaly(User user, string ipAddress)
    {
        var userLocation = _geoIpService.GetLocation(ipAddress);
        var usualCountries = _auditRepository.GetLoginsByUser(user.Id, lastDays: 90)
            .Select(l => _geoIpService.GetLocation(l.IpAddress).Country)
            .Distinct()
            .ToList();
        
        if (!usualCountries.Contains(userLocation.Country))
        {
            // Check si geográficamente POSIBLE viajar en el tiempo
            var lastLoginLocation = _auditRepository.GetLastLogin(user.Id);
            var travelTime = CalculateTravelTime(lastLoginLocation, userLocation);
            
            if (travelTime.TotalMinutes < 120)  // Imposible viajar en 2h
                return 30;  // Muy sospechoso
            
            return 20;  // Viaje posible pero raro
        }
        
        return 0;
    }
    
    // Factor 3: Device Reputation (0-20 puntos)
    public int CalculateDeviceReputation(User user, string deviceFingerprint)
    {
        var knownDevices = _deviceRepository.GetDevicesByUser(user.Id)
            .Where(d => d.Status == DeviceStatus.TRUSTED)
            .Select(d => d.Fingerprint)
            .ToList();
        
        if (!knownDevices.Contains(deviceFingerprint))
            return 20;  // Device desconocido
        
        return 0;
    }
    
    // Factor 4: Network Anomaly (0-10 puntos)
    public int CalculateNetworkAnomaly(string ipAddress)
    {
        var threatIntel = _threatIntelService.CheckIP(ipAddress);
        
        return threatIntel switch
        {
            { IsMalicious: true } => 10,
            { IsVPN: true } => 5,      // VPN = algo sospechoso
            { IsProxy: true } => 5,
            { IsTor: true } => 10,
            _ => 0
        };
    }
    
    // Factor 5: Failed Attempts (0-10 puntos)
    public int CalculateFailedAttempts(User user, string ipAddress)
    {
        var recentFailures = _auditRepository
            .GetFailedLoginAttempts(user.Id, ipAddress, lastMinutes: 60)
            .Count;
        
        return recentFailures switch
        {
            0 => 0,
            1 to 3 => 3,
            4 to 6 => 7,
            >= 7 => 10
        };
    }
    
    // Factor 6: Tenant Risk Level (0-30 puntos)
    public int CalculateTenantRiskLevel(Tenant tenant)
    {
        return tenant.RiskLevel switch
        {
            TenantRiskLevel.LOW => 0,
            TenantRiskLevel.MEDIUM => 10,
            TenantRiskLevel.HIGH => 25,
            TenantRiskLevel.CRITICAL => 30,
            _ => 10
        };
    }
}
```

---

### 1.3 Acceptance Criteria (FS-09)

#### US-017: Adaptive MFA
**Como:** Administrador de Seguridad  
**Quiero:** Reglas de MFA adaptativo para exigir verificación en accesos de riesgo  
**Para que:** La postura de seguridad mejore sin fricción uniforme

**Criteria:**

```gherkin
Feature: Adaptive MFA Requirements

  Scenario: Low-risk login (interno, dispositivo conocido, hora usual)
    Given User "alice@corp.com" (INTERNAL) intenta login a las 9am
    And desde su dispositivo conocido
    And desde su país usual
    When Risk Score calculado = 15
    Then MFA no es requerido
    And login completa sin MFA
    
  Scenario: Medium-risk login (hora inusual)
    Given User "bob@corp.com" intenta login a las 3am
    And Risk Score calculado = 35
    When User category = INTERNAL
    Then MFA es "Recommended" (optional)
    And se muestra prompt "Verificación adicional?" con skip button
    
  Scenario: High-risk login (país diferente)
    Given User "charlie@corp.com" (EXTERNAL) intenta login desde Brasil
    And su último login fue desde USA hace 1 hora (viaje imposible)
    When Risk Score calculado = 75
    Then MFA es "Required"
    And login BLOQUEADO hasta completar MFA
    
  Scenario: Critical risk login (múltiples factores)
    Given User intenta login con Risk Score = 85
    And factores: país desconocido + 5 intentos fallidos + IP maliciosa
    When Risk Score > 70
    Then MFA es "RequiredWithSecurityReview"
    And login bloqueado + security team notificado
    And auditoría registra intent malicioso
    
  Scenario: Tenant High-Risk Category
    Given Tenant "HighRiskCorp" categorizado como HIGH_RISK
    And User es de ese tenant
    When cualquier login
    Then Risk Score recibe +25 puntos automáticamente
    And MFA es más probable (threshold más bajo)
```

---

### 1.4 Métodos Passwordless Soportados

#### FS-09 Scope: Métodos Disponibles

| Método | Descripción | Seguridad | UX | Requisitos |
|--------|-------------|-----------|-----|------------|
| **FIDO2 / WebAuthn** | Biometría o security key | 🔒🔒🔒 (Alta) | ⭐⭐⭐ (Excelente) | Device con soporte FIDO2 |
| **Magic Link** | Link por email con token temporal | 🔒🔒 (Media) | ⭐⭐⭐ (Excelente) | Email access |
| **App Notification** | Push a app móvil (similar a Microsoft/Google Authenticator) | 🔒🔒🔒 (Alta) | ⭐⭐⭐ (Excelente) | Authenticator app instalada |
| **SMS OTP** | Código temporal por SMS | 🔒 (Baja) | ⭐⭐ (Buena) | Número teléfono verificado |
| **TOTP** | Time-based OTP (Google Authenticator, Authy) | 🔒🔒 (Media) | ⭐⭐ (Buena) | Authenticator app |

**MVP FS-09 Scope:** FIDO2 + Magic Link + App Notification

```csharp
public interface IPasswordlessMethod
{
    string MethodName { get; }  // "fido2", "magic_link", "app_notification"
    Task<PasswordlessChallenge> InitiateAsync(User user);
    Task<bool> VerifyAsync(PasswordlessChallenge challenge, string response);
}

public class FIDO2Method : IPasswordlessMethod
{
    public string MethodName => "fido2";
    
    public async Task<PasswordlessChallenge> InitiateAsync(User user)
    {
        // 1. Generar challenge (random bytes)
        var challenge = GenerateSecureChallenge(32);
        
        // 2. Recuperar credential IDs registrados del usuario
        var credentials = await _credentialRepository.GetFIDO2CredentialsByUser(user.Id);
        
        // 3. Construir WebAuthn PublicKeyCredentialRequestOptions
        var options = new PublicKeyCredentialRequestOptions
        {
            Challenge = challenge,
            Timeout = 60000,  // 60 segundos
            UserVerification = UserVerificationRequirement.Preferred,
            AllowCredentials = credentials.Select(c => new PublicKeyCredentialDescriptor
            {
                Type = PublicKeyCredentialType.PublicKey,
                Id = Convert.FromBase64String(c.CredentialId)
            }).ToList()
        };
        
        // 4. Guardar challenge en cache temporal (expiración 5 min)
        await _challengeCache.SetAsync($"fido2:{user.Id}", challenge, TimeSpan.FromMinutes(5));
        
        return new PasswordlessChallenge
        {
            Method = "fido2",
            Options = JsonSerializer.Serialize(options),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
    }
    
    public async Task<bool> VerifyAsync(PasswordlessChallenge challenge, string response)
    {
        // 1. Parsear respuesta WebAuthn del cliente
        var assertion = JsonSerializer.Deserialize<AuthenticatorAssertionResponse>(response);
        
        // 2. Validar signature usando credential público
        var credential = await _credentialRepository.GetCredential(assertion.Id);
        var isValid = VerifySignature(assertion, credential.PublicKey);
        
        // 3. Validar counter (prevenir replay attacks)
        if (assertion.SignCount <= credential.SignCount)
            return false;  // Posible cloning attack
        
        credential.SignCount = assertion.SignCount;
        await _credentialRepository.UpdateAsync(credential);
        
        return isValid;
    }
}

public class MagicLinkMethod : IPasswordlessMethod
{
    public string MethodName => "magic_link";
    
    public async Task<PasswordlessChallenge> InitiateAsync(User user)
    {
        // 1. Generar token único (40 caracteres aleatorios)
        var token = GenerateSecureToken(40);
        
        // 2. Crear "passwordless session" en BD
        var session = new PasswordlessSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Method = "magic_link",
            Token = HashToken(token),  // Store hash, no plaintext
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Status = PasswordlessSessionStatus.PENDING
        };
        await _sessionRepository.AddAsync(session);
        
        // 3. Enviar email con link
        var magicLink = $"https://ums.example.com/auth/passwordless/verify?token={token}&session={session.Id}";
        await _emailService.SendAsync(user.Email, new PasswordlessMagicLinkEmail
        {
            UserName = user.Name,
            MagicLink = magicLink,
            ExpiresIn = "15 minutos"
        });
        
        return new PasswordlessChallenge
        {
            Method = "magic_link",
            SessionId = session.Id.ToString(),
            ExpiresAt = session.ExpiresAt,
            Message = $"Link enviado a {MaskEmail(user.Email)}"
        };
    }
    
    public async Task<bool> VerifyAsync(PasswordlessChallenge challenge, string response)
    {
        // response = token del user
        var session = await _sessionRepository.GetAsync(Guid.Parse(challenge.SessionId));
        
        if (session == null || session.ExpiresAt < DateTime.UtcNow)
            return false;  // Session no existe o expiró
        
        // Timing-safe comparison para evitar timing attacks
        var isValid = TimingSafeEquals(HashToken(response), session.Token);
        
        if (isValid)
        {
            session.Status = PasswordlessSessionStatus.VERIFIED;
            session.VerifiedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);
        }
        
        return isValid;
    }
}
```

#### Magic Link Flow Detallado

```
┌─────────┐                    ┌──────────┐                    ┌───────┐
│ Browser │                    │ UMS API  │                    │ Email │
└────┬────┘                    └────┬─────┘                    └───┬───┘
     │                              │                              │
     │─ POST /auth/passwordless ──> │                              │
     │ { email: "user@corp.com" }   │                              │
     │                              │                              │
     │                         Generar token                       │
     │                         Crear session                       │
     │                         Hash token                          │
     │                              │ ──── Enviar magic link ────> │
     │                              │ <──── Email sent ───────── │
     │                              │                              │
     │ <─ 202 Accepted ────────────│                              │
     │   { sessionId, expiresAt }  │                              │
     │                              │                              │
     │ [Usuario click magic link]   │                              │
     │─ GET /auth/passwordless     │                              │
     │   /verify?token=XXX         │                              │
     │ &session=YYY                 │                              │
     │                              │                              │
     │                         Recuperar session                   │
     │                         Validar token                       │
     │                         Crear JWT                           │
     │                              │                              │
     │ <─ 302 Redirect + ──────────│                              │
     │    Set-Cookie: session_jwt  │                              │
     │                              │                              │
     │ [Usuario autenticado]        │                              │
```

---

### 1.5 Configuration (FS-09)

Dónde y cómo se configuran las reglas MFA:

```sql
-- Nueva tabla en Configuration Context
CREATE TABLE [configuration].[mfa_policies] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64),  -- "default", "high-risk-users", etc.
    [name] VARCHAR(255),
    [enabled] BIT,
    [scope_type] VARCHAR(32),  -- 'GLOBAL', 'TENANT', 'ORGANIZATION'
    [applies_to_user_category] VARCHAR(32),  -- 'INTERNAL', 'EXTERNAL', 'B2B'
    
    -- Risk-based thresholds
    [risk_score_required_threshold] INT,  -- Ej: 40
    [risk_score_review_threshold] INT,   -- Ej: 70
    
    -- Enabled methods
    [allow_fido2] BIT,
    [allow_magic_link] BIT,
    [allow_app_notification] BIT,
    [allow_sms_otp] BIT,
    [allow_totp] BIT,
    
    -- Passwordless-only mode (no password auth)
    [passwordless_only] BIT,
    
    [created_at] DATETIME2,
    [modified_at] DATETIME2,
    [root_tenant_id] UNIQUEIDENTIFIER
);

-- Tabla de Risk Scoring customization por tenant
CREATE TABLE [configuration].[risk_scoring_weights] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [frequency_anomaly_weight] DECIMAL(3,2),  -- Default: 0.20
    [geographic_anomaly_weight] DECIMAL(3,2), -- Default: 0.25
    [device_reputation_weight] DECIMAL(3,2),  -- Default: 0.15
    [network_anomaly_weight] DECIMAL(3,2),    -- Default: 0.10
    [failed_attempts_weight] DECIMAL(3,2),    -- Default: 0.10
    [tenant_risk_weight] DECIMAL(3,2)         -- Default: 0.20
);
```

---

## PARTE 2: FS-14 — Delegated Administration & Scopes

### 2.1 Definición

**FS-14** permite que administradores deleguen autoridad de gestión a otros con límites controlados:

- **Delegating Admin** (A) → **Delegated Admin** (B): "Puedes gestionar usuarios en mi división"
- **Scope Limiting**: "Solo en ORGANIZATION X", "Solo acciones CREATE_USER y ASSIGN_PROFILE"
- **Temporal Constraints**: "Válido hasta 2026-12-31"
- **Approval Required**: Crear delegación puede requerir aprobación (si es sensitive)

### 2.2 State Machine (Delegación)

```
┌─────────────────────────────────────────────────────────┐
│                 DELEGATION LIFECYCLE                    │
└─────────────────────────────────────────────────────────┘

                    [DRAFT]
                      │
                      │ (Admin completa configuración)
                      ▼
              ┌─ [PENDING_APPROVAL] ──┐
              │                       │
    (Si requiere approval)      (Si no requiere)
              │                       │
              │ (Approver aprueba)    │ (Admin confirma)
              │                       │
              └──────────┬────────────┘
                         ▼
                   [ACTIVE]
                         │
        ┌────────────────┼────────────────┐
        │                │                │
   (Revocado)      (Expirado)        (Finaliza)
        │                │                │
        ▼                ▼                ▼
   [REVOKED]        [EXPIRED]        [COMPLETED]
        │                │                │
        └────────────────┴────────────────┘
                         │
                         ▼
                   [ARCHIVED]
```

#### 2.2.1 Estados Detallados

| Estado | Descripción | Transiciones Válidas | Eventos |
|--------|-------------|---------------------|--------|
| **DRAFT** | Delegación en creación, no visible | → PENDING_APPROVAL, → ACTIVE | Created |
| **PENDING_APPROVAL** | Esperando aprobación (si config lo requiere) | → ACTIVE (approved), → REJECTED | SubmittedForApproval |
| **ACTIVE** | Delegación operativa | → REVOKED, → EXPIRED | Activated |
| **REVOKED** | Revocado manualmente por admin | → ARCHIVED | Revoked |
| **EXPIRED** | Expiró por fecha (valid_until) | → ARCHIVED | Expired |
| **COMPLETED** | Finalizado naturalmente (fin de período) | → ARCHIVED | Completed |
| **REJECTED** | Rechazado en aprobación | → ARCHIVED | Rejected |
| **ARCHIVED** | Histórico (no visible en operaciones) | (ninguna) | Archived |

#### 2.2.2 Transiciones Bloqueadas

```csharp
public class DelegationStateValidator
{
    public bool IsValidTransition(DelegationStatus from, DelegationStatus to)
    {
        var validTransitions = new Dictionary<DelegationStatus, HashSet<DelegationStatus>>
        {
            { DelegationStatus.DRAFT, new() { DelegationStatus.PENDING_APPROVAL, DelegationStatus.ACTIVE } },
            { DelegationStatus.PENDING_APPROVAL, new() { DelegationStatus.ACTIVE, DelegationStatus.REJECTED } },
            { DelegationStatus.ACTIVE, new() { DelegationStatus.REVOKED, DelegationStatus.EXPIRED } },
            { DelegationStatus.REVOKED, new() { DelegationStatus.ARCHIVED } },
            { DelegationStatus.EXPIRED, new() { DelegationStatus.ARCHIVED } },
            { DelegationStatus.COMPLETED, new() { DelegationStatus.ARCHIVED } },
            { DelegationStatus.REJECTED, new() { DelegationStatus.ARCHIVED } },
            { DelegationStatus.ARCHIVED, new() { } }  // Terminal
        };
        
        return validTransitions.ContainsKey(from) && validTransitions[from].Contains(to);
    }
}
```

---

### 2.3 Scope Model (Límites de Delegación)

Una delegación define qué acciones puede hacer el delegated admin.

#### 2.3.1 Scope Types

```csharp
public enum ScopeType
{
    TENANT,          // Toda la organización (root tenant)
    ORGANIZATION,    // Una organización específica (child tenant)
    DEPARTMENT,      // Un departamento
    SYSTEM,          // Un sistema/aplicación específico
    TEAM             // Un equipo
}

public record DelegationScope
{
    public ScopeType Type { get; init; }
    public Guid? ScopeId { get; init; }  // ID de la organización, sistema, etc.
    public List<string> AllowedActions { get; init; }  // ["CREATE_USER", "ASSIGN_PROFILE"]
}
```

#### 2.3.2 Allowed Actions (¿Qué puede hacer el delegated admin?)

```csharp
public enum DelegatedAction
{
    // User Management
    CREATE_USER,
    VIEW_USER,
    UPDATE_USER,
    DEACTIVATE_USER,
    DELETE_USER,
    RESET_PASSWORD,
    
    // Profile/Role Assignment
    ASSIGN_PROFILE,
    REVOKE_PROFILE,
    APPROVE_PROFILE_REQUEST,
    
    // Delegation
    CREATE_DELEGATION,
    REVOKE_DELEGATION,
    VIEW_DELEGATION,
    
    // Approvals
    APPROVE_EXTERNAL_ACCESS,
    REJECT_EXTERNAL_ACCESS,
    
    // Audit/Reporting
    VIEW_AUDIT_LOG,
    EXPORT_USERS,
    
    // Configuration
    CONFIGURE_ORGANIZATION,
    MANAGE_ORGANIZATION_POLICIES
}
```

#### 2.3.3 Principle of Least Privilege Validation

**Regla crítica:** Un admin delegado NO puede otorgar permisos mayores a los que posee.

```csharp
public class DelegationPermissionValidator
{
    /// <summary>
    /// Valida que los permisos siendo delegados no excedan los del delegating admin.
    /// </summary>
    public async Task<ValidationResult> ValidateDelegationAsync(
        User delegatingAdmin,
        User delegatedAdmin,
        DelegationScope requestedScope)
    {
        // 1. Obtener permisos efectivos del delegating admin
        var delegatingAdminPermissions = await _authorizationService
            .GetEffectivePermissionsAsync(delegatingAdmin.Id);
        
        // 2. Validar que requested actions están en delegatingAdminPermissions
        var unauthorizedActions = requestedScope.AllowedActions
            .Except(delegatingAdminPermissions.Select(p => p.ActionCode))
            .ToList();
        
        if (unauthorizedActions.Any())
            return ValidationResult.Failure(
                $"Admin no puede delegar acciones: {string.Join(", ", unauthorizedActions)}");
        
        // 3. Validar scope: delegating admin no puede delegar fuera de su propio scope
        var delegatingAdminScope = await _delegationRepository
            .GetDelegationScopeAsync(delegatingAdmin.Id);
        
        if (!IsWithinScope(requestedScope, delegatingAdminScope))
            return ValidationResult.Failure(
                "Delegación solicitada excede el scope del admin delegante");
        
        // 4. Validar que delegated admin no tenga conflictos de interés
        // (ej: no delegar a admin de un competidor dentro mismo tenant)
        if (HasConflictOfInterest(delegatedAdmin, requestedScope))
            return ValidationResult.Failure("Conflicto de interés detectado");
        
        return ValidationResult.Success();
    }
    
    private bool IsWithinScope(DelegationScope requested, DelegationScope delegatingAdmin)
    {
        return requested.Type switch
        {
            ScopeType.TENANT when delegatingAdmin.Type == ScopeType.TENANT 
                => requested.ScopeId == delegatingAdmin.ScopeId,
            
            ScopeType.ORGANIZATION when delegatingAdmin.Type == ScopeType.TENANT 
                => true,  // Tenant-level admin puede delegar a org-level
            
            ScopeType.ORGANIZATION when delegatingAdmin.Type == ScopeType.ORGANIZATION
                => requested.ScopeId == delegatingAdmin.ScopeId,
            
            _ => false
        };
    }
}
```

---

### 2.4 Temporal Constraints

Delegaciones pueden tener validez limitada.

```csharp
public record DelegationTemporalConstraints
{
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public TimeSpan? MaxDuration { get; init; }  // Máximo duración permitida (ej: 90 días)
    public DayOfWeek[]? AllowedDaysOfWeek { get; init; }  // Ej: solo business days
    public TimeSpan? AllowedTimeRange { get; init; }  // Ej: 9am-6pm solo
}

public class DelegationExpirationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Cada hora, buscar delegaciones que expiraron
            var expiredDelegations = await _delegationRepository
                .GetExpiredAsync(DateTime.UtcNow);
            
            foreach (var delegation in expiredDelegations)
            {
                // Transicionar a EXPIRED state
                delegation.Status = DelegationStatus.EXPIRED;
                delegation.ModifiedAt = DateTime.UtcNow;
                
                await _delegationRepository.UpdateAsync(delegation);
                
                // Registrar en auditoria
                await _auditService.LogAsync(new AuditEvent
                {
                    EventType = "DELEGATION_EXPIRED",
                    DelegationId = delegation.Id,
                    RootTenantId = delegation.RootTenantId,
                    Timestamp = DateTime.UtcNow
                });
                
                // Notificar al delegating admin
                await _notificationService.NotifyAsync(
                    delegation.DelegatingAdminId,
                    "Delegación expirada",
                    $"Delegación a {delegation.DelegatedAdmin.Name} expiró");
            }
            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

---

### 2.5 Acceptance Criteria (FS-14)

```gherkin
Feature: Delegated Administration with Scope Control

  Scenario: Create delegation within scope
    Given Admin "alice@corp.com" (TENANT-level)
    When crea delegación a "bob@corp.com"
    And scope: ORGANIZATION "Sales Division"
    And allowed_actions: [CREATE_USER, ASSIGN_PROFILE]
    And valid_from: 2026-05-15
    And valid_until: 2026-12-31
    Then Delegation creada en estado DRAFT
    And Audit registra: DELEGATION_CREATED
    
  Scenario: Approve delegation that requires review
    Given Delegation en estado PENDING_APPROVAL
    When Approver aprueba
    Then Delegation transiciona a ACTIVE
    And Delegated admin puede gestionar usuarios
    And Audit registra: DELEGATION_APPROVED
    
  Scenario: Prevent escalation of privilege
    Given Admin "charlie@corp.com" (ORG-level, permisos limitados)
    When intenta crear delegación con permisos > sus propios
    Then Validación falla
    And Error: "Cannot delegate permissions you don't possess"
    And Audit registra: DELEGATION_VALIDATION_FAILED
    
  Scenario: Auto-expire delegation on valid_until
    Given Delegation con valid_until: 2026-12-31
    When Sistema alcanza 2027-01-01
    Then Delegation transiciona automáticamente a EXPIRED
    And Delegated admin pierde acceso
    And Audit registra: DELEGATION_EXPIRED
    
  Scenario: Manual revocation by delegating admin
    Given Delegation en estado ACTIVE
    When Delegating admin ejecuta "Revoke delegation"
    Then Delegation transiciona a REVOKED
    And Razón de revocación registrada
    And Delegated admin recibe notificación
    And Audit registra: DELEGATION_REVOKED
    
  Scenario: Delegated admin operates within scope
    Given Delegated admin "bob" con scope: ORG "Sales"
    And allowed_actions: [CREATE_USER]
    When intenta crear user en Sales org
    Then Operación permitida
    When intenta crear user en Engineering org (fuera scope)
    Then Operación bloqueada
    And Error: "Outside delegated scope"
```

---

## PARTE 3: ER Model Completo (EP-06)

### 3.1 Tablas Nuevas

```sql
-- ============================================
-- APPROVALS CONTEXT TABLES
-- ============================================

CREATE TABLE [approval].[approval_workflows] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64) NOT NULL,
    [name] VARCHAR(255) NOT NULL,
    [description] NVARCHAR(MAX),
    
    -- Trigger que inicia el workflow
    [trigger_type] VARCHAR(32) NOT NULL,  -- 'USER_ONBOARDING', 'PROFILE_ASSIGNMENT', 'DELEGATION_CREATION', 'B2B_ACCESS_REQUEST'
    
    -- Tipo de aprobación
    [approval_type] VARCHAR(32) NOT NULL,  -- 'SERIAL' (uno después de otro), 'PARALLEL' (todos simultáneamente), 'QUORUM' (mayoría)
    [required_approvals] INT NOT NULL DEFAULT 1,  -- Cuántas aprobaciones se necesitan
    
    -- Timing
    [timeout_days] INT DEFAULT 7,  -- Cuántos días antes de auto-reject
    [escalate_after_days] INT,  -- Cuándo escalar a superior si no aprueba
    
    -- Scope
    [scope_type] VARCHAR(32),  -- 'GLOBAL', 'TENANT', 'ORGANIZATION'
    [applies_to_user_category] VARCHAR(32),  -- 'INTERNAL', 'EXTERNAL', 'B2B' (NULL = all)
    
    -- Audit
    [enabled] BIT NOT NULL DEFAULT 1,
    [created_by] VARCHAR(255),
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [modified_by] VARCHAR(255),
    [modified_at] DATETIME2,
    [is_deleted] BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT pk_approval_workflows PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_approval_workflows_tenant FOREIGN KEY (root_tenant_id) REFERENCES [identity].[tenants](id)
);

CREATE TABLE [approval].[approval_rules] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [workflow_id] UNIQUEIDENTIFIER NOT NULL,
    [rule_order] INT NOT NULL,  -- Orden de evaluación
    
    -- Condición que gatilla esta regla
    [condition_json] NVARCHAR(MAX),  -- JSON: { "riskScore": "> 50", "userCategory": "EXTERNAL" }
    
    -- Quién aprueba si esta regla aplica
    [approver_role] VARCHAR(64),  -- 'SECURITY_ADMIN', 'DEPARTMENT_HEAD', 'COMPLIANCE_OFFICER'
    [approver_count] INT DEFAULT 1,
    
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [is_deleted] BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT pk_approval_rules PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_approval_rules_workflow FOREIGN KEY (workflow_id, root_tenant_id) REFERENCES [approval].[approval_workflows](id, root_tenant_id)
);

CREATE TABLE [approval].[approval_requests] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [workflow_id] UNIQUEIDENTIFIER NOT NULL,
    
    -- Quién solicita
    [requester_id] UNIQUEIDENTIFIER NOT NULL,
    
    -- Target de la solicitud
    [target_user_id] UNIQUEIDENTIFIER,
    [target_entity_type] VARCHAR(32),  -- 'USER', 'PROFILE', 'DELEGATION', 'B2B_ACCESS'
    [target_entity_id] UNIQUEIDENTIFIER,
    
    -- Descripción
    [requested_action] VARCHAR(255) NOT NULL,
    [request_reason] NVARCHAR(MAX),
    [business_justification] NVARCHAR(MAX),
    
    -- Timing
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [submitted_at] DATETIME2,
    [expires_at] DATETIME2,
    [completed_at] DATETIME2,
    
    -- Estado
    [status] VARCHAR(32) NOT NULL DEFAULT 'DRAFT',  -- DRAFT, SUBMITTED, PENDING, APPROVED, REJECTED, ESCALATED
    [final_decision] VARCHAR(32),  -- APPROVED, REJECTED
    [final_decision_reason] NVARCHAR(MAX),
    
    -- Metadata
    [priority] VARCHAR(32),  -- LOW, MEDIUM, HIGH, CRITICAL
    [risk_score] DECIMAL(5,2),
    
    CONSTRAINT pk_approval_requests PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_approval_requests_workflow FOREIGN KEY (workflow_id, root_tenant_id) REFERENCES [approval].[approval_workflows](id, root_tenant_id),
    CONSTRAINT fk_approval_requests_requester FOREIGN KEY (requester_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id),
    CONSTRAINT fk_approval_requests_target FOREIGN KEY (target_user_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id)
);

CREATE TABLE [approval].[approval_approvers] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [approval_request_id] UNIQUEIDENTIFIER NOT NULL,
    
    -- Quién aprueba
    [approver_id] UNIQUEIDENTIFIER NOT NULL,
    [approver_role] VARCHAR(64),
    
    -- Orden de aprobación (para SERIAL workflows)
    [approval_order] INT,
    
    -- Decisión
    [status] VARCHAR(32) NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, REJECTED, ESCALATED
    [approved_at] DATETIME2,
    [decision_reason] NVARCHAR(MAX),
    [decision_notes] NVARCHAR(MAX),
    
    -- Escalación
    [escalated_to_id] UNIQUEIDENTIFIER,  -- Superior si escalado
    [escalated_at] DATETIME2,
    
    CONSTRAINT pk_approval_approvers PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_approval_approvers_request FOREIGN KEY (approval_request_id, root_tenant_id) REFERENCES [approval].[approval_requests](id, root_tenant_id),
    CONSTRAINT fk_approval_approvers_approver FOREIGN KEY (approver_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id)
);

CREATE TABLE [approval].[approval_attachments] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [approval_request_id] UNIQUEIDENTIFIER NOT NULL,
    
    [document_name] VARCHAR(255) NOT NULL,
    [document_type] VARCHAR(64),  -- 'SERVICE_AGREEMENT', 'IDENTITY_PROOF', etc.
    [storage_uri] VARCHAR(MAX) NOT NULL,  -- URL a archivo en Azure Blob Storage, S3, etc.
    [file_size_bytes] BIGINT,
    [uploaded_by] UNIQUEIDENTIFIER,
    [uploaded_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT pk_approval_attachments PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_approval_attachments_request FOREIGN KEY (approval_request_id, root_tenant_id) REFERENCES [approval].[approval_requests](id, root_tenant_id)
);

-- ============================================
-- DELEGATION CONTEXT TABLES
-- ============================================

CREATE TABLE [delegation].[user_management_delegations] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    
    -- Admin roles
    [delegating_admin_id] UNIQUEIDENTIFIER NOT NULL,  -- Quién delega
    [delegated_admin_id] UNIQUEIDENTIFIER NOT NULL,   -- A quién se delega
    
    -- Scope
    [scope_type] VARCHAR(32) NOT NULL,  -- TENANT, ORGANIZATION, DEPARTMENT, SYSTEM, TEAM
    [scope_id] UNIQUEIDENTIFIER,  -- ID de org, dept, etc.
    
    -- Acciones permitidas
    [allowed_actions] NVARCHAR(MAX) NOT NULL,  -- JSON array: ["CREATE_USER", "ASSIGN_PROFILE", ...]
    
    -- Temporal validity
    [valid_from] DATETIME2 NOT NULL,
    [valid_until] DATETIME2 NOT NULL,
    [max_duration_days] INT,  -- Máxima duración permitida (para validación)
    
    -- Approval
    [requires_approval] BIT NOT NULL DEFAULT 0,
    [approval_request_id] UNIQUEIDENTIFIER,  -- Link a approval request si fue requerido
    
    -- Estado
    [status] VARCHAR(32) NOT NULL DEFAULT 'DRAFT',  -- DRAFT, PENDING_APPROVAL, ACTIVE, REVOKED, EXPIRED, REJECTED, COMPLETED, ARCHIVED
    [revoked_at] DATETIME2,
    [revoked_by] UNIQUEIDENTIFIER,
    [revocation_reason] NVARCHAR(MAX),
    
    -- Restricciones adicionales
    [restricted_to_user_category] VARCHAR(32),  -- Ej: solo usuarios EXTERNAL
    [restricted_to_organization_id] UNIQUEIDENTIFIER,  -- Ej: solo en esta org
    
    -- Audit
    [created_by] UNIQUEIDENTIFIER NOT NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [modified_by] VARCHAR(255),
    [modified_at] DATETIME2,
    
    CONSTRAINT pk_user_management_delegations PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_delegation_delegating_admin FOREIGN KEY (delegating_admin_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id),
    CONSTRAINT fk_delegation_delegated_admin FOREIGN KEY (delegated_admin_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id),
    CONSTRAINT fk_delegation_approval FOREIGN KEY (approval_request_id, root_tenant_id) REFERENCES [approval].[approval_requests](id, root_tenant_id)
);

-- ============================================
-- INDICES para Performance
-- ============================================

CREATE INDEX idx_approval_requests_workflow ON [approval].[approval_requests] (workflow_id, root_tenant_id)
    WHERE status NOT IN ('APPROVED', 'REJECTED');

CREATE INDEX idx_approval_requests_target ON [approval].[approval_requests] (target_user_id, root_tenant_id);

CREATE INDEX idx_approval_approvers_request ON [approval].[approval_approvers] (approval_request_id, root_tenant_id);

CREATE INDEX idx_approval_approvers_approver ON [approval].[approval_approvers] (approver_id, root_tenant_id)
    WHERE status = 'PENDING';

CREATE INDEX idx_delegations_delegated_admin ON [delegation].[user_management_delegations] (delegated_admin_id, root_tenant_id)
    WHERE status = 'ACTIVE';

CREATE INDEX idx_delegations_scope ON [delegation].[user_management_delegations] (scope_type, scope_id, root_tenant_id)
    WHERE status IN ('ACTIVE', 'PENDING_APPROVAL');
```

---

### 3.2 Modification to Existing Tables

```sql
-- Agregar columnas a users table para track delegated admin status
ALTER TABLE [identity].[users] ADD 
    [is_delegated_admin] BIT NOT NULL DEFAULT 0,
    [delegated_admin_scopes] NVARCHAR(MAX);  -- JSON: cached scopes for performance

-- Agregar columnas a approval_requests para link a MFA/passwordless decisions
ALTER TABLE [approval].[approval_requests] ADD
    [risk_score] DECIMAL(5,2),
    [mfa_required] BIT,
    [passwordless_allowed] BIT;
```

---

## PARTE 4: Integration Map (EP-06)

### 4.1 Approvals ↔ Authorization Context

```
┌──────────────────────────────────────┐
│   APPROVALS CONTEXT                  │
│ ┌─ approval_requests                 │
│ ├─ approval_workflows                │
│ ├─ approval_approvers                │
│ └─ approval_attachments              │
└──────────────────┬───────────────────┘
                   │
         [Requires user permission?]
                   │
                   ▼
        ┌──────────────────────────────┐
        │ AUTHORIZATION CONTEXT        │
        │ ┌─ policies                  │
        │ ├─ policy_bindings           │
        │ └─ permissions               │
        └──────────────────────────────┘
         
  Flow: Approver debe tener permiso APPROVE_PROFILE_ASSIGNMENT
        para aprobar un approval_request de asignación de profile.
```

**Queries de integración:**

```csharp
public interface IApprovalAuthorizationValidator
{
    /// <summary>
    /// Valida que approver tiene permiso para aprobar esta request.
    /// </summary>
    Task<bool> CanApproveAsync(User approver, ApprovalRequest request);
}

public class ApprovalAuthorizationValidator : IApprovalAuthorizationValidator
{
    public async Task<bool> CanApproveAsync(User approver, ApprovalRequest request)
    {
        // 1. Determinar qué permission se necesita basado en el tipo de request
        var requiredPermission = request.TargetEntityType switch
        {
            "PROFILE" => "APPROVE_PROFILE_ASSIGNMENT",
            "USER_ONBOARDING" => "APPROVE_USER_ONBOARDING",
            "B2B_ACCESS" => "APPROVE_B2B_ACCESS",
            "DELEGATION" => "APPROVE_DELEGATION",
            _ => throw new InvalidOperationException()
        };
        
        // 2. Check si el approver tiene esa permission
        var permissions = await _authorizationService
            .GetEffectivePermissionsAsync(approver.Id);
        
        return permissions.Any(p => p.ActionCode == requiredPermission);
    }
}
```

### 4.2 Approvals ↔ Audit Context

```
┌─────────────────────────────────────┐
│  APPROVALS CONTEXT                  │
│ (Generates events)                  │
└────────────┬────────────────────────┘
             │
    [APPROVAL_REQUEST_CREATED]
    [APPROVAL_SUBMITTED]
    [APPROVAL_APPROVED]
    [APPROVAL_REJECTED]
    [APPROVAL_ESCALATED]
             │
             ▼
┌──────────────────────────────────────┐
│ AUDIT CONTEXT                        │
│ ┌─ audit_log (receives events)       │
│ └─ (stores immutable trail)          │
└──────────────────────────────────────┘

  Cada decisión de aprobación se registra en audit_log
  con: approver, timestamp, decision, reason.
```

### 4.3 Approvals ↔ Configuration Context

```
┌──────────────────────────────────────┐
│ CONFIGURATION CONTEXT                │
│ ┌─ approval_workflows  (configurable)│
│ ├─ approval_rules      (configurable)│
│ ├─ mfa_policies        (configurable)│
│ └─ risk_scoring_weights (tunable)    │
└──────────┬───────────────────────────┘
           │
    [Defines approval behavior]
           │
           ▼
┌──────────────────────────────────────┐
│ APPROVALS CONTEXT                    │
│ ┌─ Uses workflows from config        │
│ ├─ Applies rules from config         │
│ └─ Evaluates risk scores per config  │
└──────────────────────────────────────┘
```

---

## Summary EP-06 Deliverables

### ✅ Completed in This Document

1. **FS-09 Adaptive MFA**
   - Risk Scoring Model (6 factors, weighted)
   - Decision Engine (thresholds)
   - Passwordless Methods (FIDO2, Magic Link, App Notification)
   - Configuration Model
   - Acceptance Criteria (5 scenarios)

2. **FS-14 Delegated Admin**
   - State Machine (8 states)
   - Scope Model (5 scope types, allowed actions)
   - Principle of Least Privilege Validation
   - Temporal Constraints & Auto-Expiration
   - Acceptance Criteria (6 scenarios)

3. **ER Model (Complete)**
   - approval_workflows
   - approval_rules
   - approval_requests
   - approval_approvers
   - approval_attachments
   - user_management_delegations
   - Indices for performance

4. **Integration Map**
   - Approvals ↔ Authorization
   - Approvals ↔ Audit
   - Approvals ↔ Configuration

---

**Próximo: EP-07 Compliance (Documento separado)**

---

**Aprobado por:** Arquitecto Principal  
**Fecha:** 2026-05-14
