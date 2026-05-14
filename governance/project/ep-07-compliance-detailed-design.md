# EP-07: Detailed Design — Compliance Lifecycle

**Version:** 1.0  
**Date:** 2026-05-14  
**Epic:** EP-07 (Post-MVP)  
**User Stories:** US-023 to US-028  
**Functional Stories:** FS-11, FS-15 (NEW), FS-16 (NEW)

---

## PART 1: FS-11 — Upload & Validate User Document

### 1.1 Definition

**FS-11** allows users and admins to upload documents (identity, certificates, agreements) for compliance.

Workflow:
1. **Upload**: User uploads document → secure storage
2. **Validation**: Validator reviews → APPROVED / REJECTED
3. **Lifecycle**: Document valid until revalidation date
4. **Enforcement**: If expires, access can be affected (integration with FS-16)

### 1.2 Document Type Taxonomy

```csharp
public enum DocumentType
{
    // Identity Verification
    IDENTITY_PROOF,           // Passport, DNI, Driver License
    ADDRESS_VERIFICATION,     // Utility bill, bank statement
    CORPORATE_REGISTRATION,   // Articles of incorporation
    
    // Authorization
    SERVICE_AGREEMENT,        // B2B contract
    DATA_PROCESSING_AGREEMENT, // DPA
    NON_DISCLOSURE_AGREEMENT, // NDA
    
    // Compliance
    BACKGROUND_CHECK,         // Criminal record clearance
    INSURANCE_CERTIFICATE,    // Liability, D&O
    SECURITY_CLEARANCE,       // Government clearance
    
    // Role-specific
    CERTIFICATION,            // Professional cert (CPA, CISSP)
    TRAINING_COMPLETION,      // Mandatory training proof
    MEDICAL_CLEARANCE,        // For certain roles
    
    // Custom (tenant-specific)
    CUSTOM_DOCUMENT           // Tenant-defined
}
```

### 1.3 Acceptance Criteria (FS-11)

```gherkin
Feature: Document Upload & Validation

  Scenario: Upload identity document
    Given User "alice@corp.com" is EXTERNAL
    When uploads document type: IDENTITY_PROOF
    And document: passport.pdf (500KB, valid PDF)
    Then document stored in secure location
    And document status = UPLOADED
    And validator notified for review
    
  Scenario: Validate document - APPROVED
    Given Document in UPLOADED status
    When Compliance Officer reviews
    Then document status = APPROVED
    And valid_until = now + 365 days
    And user notified: "Document approved"
    
  Scenario: Validate document - REJECTED
    Given Document in UPLOADED status
    When Compliance Officer reviews and rejects
    Then document status = REJECTED
    And user notified: "Document rejected"
    And user can re-upload
```

---

### 1.4 Storage & Security

```csharp
public class SecureDocumentStorageService : IDocumentStorageService
{
    public async Task<StorageResult> UploadDocumentAsync(
        User uploader,
        DocumentUploadRequest request,
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        // 1. Validate file
        if (!IsValidFileType(request.DocumentType, request.FileName))
            throw new InvalidDocumentException("File type not allowed");
        
        // 2. Encrypt document
        var encryptedStream = await _encryption.EncryptAsync(fileStream);
        
        // 3. Store in secure storage
        var documentId = Guid.NewGuid();
        var storagePath = $"documents/{uploader.RootTenantId}/{uploader.Id}/{documentId}/{request.FileName}";
        var storageUri = await _storage.UploadAsync(storagePath, encryptedStream);
        
        // 4. Create document record
        var document = new UserDocument
        {
            Id = documentId,
            RootTenantId = uploader.RootTenantId,
            UserId = uploader.Id,
            Type = request.DocumentType,
            FileName = request.FileName,
            StorageUri = storageUri,
            Status = DocumentStatus.UPLOADED,
            UploadedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(document);
        
        // 5. Notify validators
        var validators = await _userRepository.GetUsersByRoleAsync(
            uploader.RootTenantId, "COMPLIANCE_OFFICER");
        
        foreach (var validator in validators)
        {
            await _notificationService.NotifyAsync(
                validator.Id,
                $"Document requiring validation: {request.DocumentType}",
                $"User {uploader.Name} uploaded {request.DocumentType}");
        }
        
        return new StorageResult { DocumentId = documentId, Status = "UPLOADED" };
    }
}
```

---

## PART 2: FS-15 — Expiration Notification Rules (NEW)

### 2.1 Definition

**FS-15** defines when and how to notify users/admins about expiring access.

**Key concept:** Configurable rules per tenant to alert BEFORE access is revoked.

### 2.2 Notification Rule Model

```csharp
public record ExpirationNotificationRule
{
    public Guid Id { get; init; }
    public Guid RootTenantId { get; init; }
    public string Code { get; init; }  // "expiry_30d", "expiry_7d"
    
    // What access expires
    public string ScopeType { get; init; }  // 'PROFILE', 'PERMISSION', 'DELEGATION'
    public string? TargetUserCategory { get; init; }  // INTERNAL, EXTERNAL, B2B
    
    // When to notify BEFORE expiration
    public int DaysBeforeExpiration { get; init; }  // 30, 7, 1
    
    // Who to notify
    public bool NotifyUser { get; init; }
    public bool NotifyAdmin { get; init; }
    public bool NotifyApprover { get; init; }
    
    // How to notify
    public List<NotificationChannel> Channels { get; init; }  // EMAIL, IN_APP, SMS, WEBHOOK
    
    // Notification frequency
    public NotificationFrequency Frequency { get; init; }  // ONCE, DAILY, WEEKLY
    
    public bool Enabled { get; init; }
}

public enum NotificationChannel
{
    EMAIL,
    IN_APP,
    SMS,
    WEBHOOK,
    SLACK
}

public enum NotificationFrequency
{
    ONCE,      // Single notification
    DAILY,     // Every day until expiration
    WEEKLY,    // Weekly
    ON_LOGIN   // Each login attempt
}
```

### 2.3 Notification Engine

```csharp
public class ExpirationNotificationEngine : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Run hourly
            await ProcessExpiringAccessAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
    
    private async Task ProcessExpiringAccessAsync(CancellationToken cancellationToken)
    {
        var rules = await _ruleRepository.GetEnabledRulesAsync();
        
        foreach (var rule in rules)
        {
            var expiringAccess = await _expirationRepo.GetExpiringAccessAsync(
                ruleScope: rule.ScopeType,
                daysUntilExpiration: rule.DaysBeforeExpiration);
            
            foreach (var access in expiringAccess)
            {
                var lastNotification = await _notificationService.GetLastNotificationAsync(
                    access.UserId, rule.Id);
                
                if (ShouldSendNotification(lastNotification, rule.Frequency))
                {
                    await SendNotificationAsync(access, rule);
                }
            }
        }
    }
}
```

### 2.4 Acceptance Criteria (FS-15)

```gherkin
Feature: Expiration Notification Rules

  Scenario: Configure notification rule
    Given Admin creates rule:
      - Days: 30
      - Notify: User + Admin
      - Channels: EMAIL, IN_APP
      - Frequency: ONCE
    Then rule saved and enabled
    
  Scenario: Auto-notify user before expiry
    Given Notification rule for 30 days
    And User access expiring in 30 days
    When background job executes
    Then email sent to user
    And in-app notification created
    
  Scenario: Customizable channels per rule
    Given Rule with Channels: [EMAIL, SLACK, WEBHOOK]
    When access expiring in 10 days
    Then notification sent via EMAIL
    And slack message to #compliance
    And webhook POST to compliance endpoint
```

---

## PART 3: FS-16 — Access Behavior on Expiration (NEW)

### 3.1 Definition

**FS-16** defines what happens to access when it expires.

**Modes:** WARNING (alert), SUSPEND (temporary), REVOKE (permanent)

### 3.2 Access Expiration Policy Model

```csharp
public record AccessExpirationPolicy
{
    public Guid Id { get; init; }
    public Guid RootTenantId { get; init; }
    
    // What access this controls
    public string ScopeType { get; init; }  // 'PROFILE', 'PERMISSION', 'DELEGATION'
    
    // What happens on expiration
    public ExpirationAction OnExpirationAction { get; init; }  // WARNING, SUSPEND, REVOKE
    
    // Grace period: days after expiration before enforcement
    public int GracePeriodDays { get; init; }
    
    // Extension permissions
    public bool AllowExtension { get; init; }
    public int MaxExtensionDays { get; init; }
    public bool RequireReapprovalOnExtend { get; init; }
    
    public bool Enabled { get; init; }
}

public enum ExpirationAction
{
    WARNING,    // Notification only
    SUSPEND,    // Temporary suspension
    REVOKE      // Permanent revocation
}
```

### 3.3 Enforcement Engine

```csharp
public class AccessExpirationEnforcementEngine : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await EnforceExpiredAccessAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
    
    private async Task EnforceExpiredAccessAsync(CancellationToken stoppingToken)
    {
        var policies = await _policyRepository.GetEnabledPoliciesAsync();
        
        foreach (var policy in policies)
        {
            var expiredAccess = await _accessRepository.GetExpiredAccessAsync(
                policyScope: policy.ScopeType,
                expiredBeforeDays: policy.GracePeriodDays);
            
            foreach (var access in expiredAccess)
            {
                await EnforceAccessAsync(access, policy);
            }
        }
    }
    
    private async Task EnforceAccessAsync(UserAccess access, AccessExpirationPolicy policy)
    {
        switch (policy.OnExpirationAction)
        {
            case ExpirationAction.WARNING:
                // Audit only
                await _auditService.LogAsync(new AuditEvent
                {
                    EventType = "ACCESS_EXPIRED_WARNING",
                    UserId = access.UserId
                });
                break;
            
            case ExpirationAction.SUSPEND:
                // Suspend access
                access.Status = AccessStatus.SUSPENDED;
                await _accessRepository.UpdateAsync(access);
                await _authorizationService.RevokePermissionsAsync(access.UserId, access.Id);
                break;
            
            case ExpirationAction.REVOKE:
                // Revoke access permanently
                access.Status = AccessStatus.REVOKED;
                await _accessRepository.UpdateAsync(access);
                await _authorizationService.RevokePermissionsAsync(access.UserId, access.Id);
                break;
        }
    }
}
```

### 3.4 Acceptance Criteria (FS-16)

```gherkin
Feature: Access Behavior on Expiration

  Scenario: WARNING mode
    Given Policy with OnExpiration: WARNING
    When access expires
    Then notification sent
    And access remains ACTIVE
    
  Scenario: SUSPEND mode
    Given Policy with OnExpiration: SUSPEND, GracePeriod: 7
    And access expired 7 days ago
    When enforcement runs
    Then access status = SUSPENDED
    And permissions revoked
    
  Scenario: REVOKE mode
    Given Policy with OnExpiration: REVOKE, GracePeriod: 3
    And access expired 3 days ago
    When enforcement runs
    Then access status = REVOKED
    And permissions permanently removed
    
  Scenario: Request extension
    Given User has suspended access
    And Policy with AllowExtension: true
    When user requests extension
    Then extension request created
    And if approved: access ExpiresAt extended
```

---

## PART 4: Compliance Context Definition

```
┌─────────────────────────────────────────┐
│      COMPLIANCE CONTEXT                 │
├─────────────────────────────────────────┤
│                                         │
│ AGGREGATES:                             │
│  - UserDocument                         │
│  - ExpirationNotificationRule           │
│  - AccessExpirationPolicy               │
│  - AccessExtensionRequest               │
│                                         │
│ PORTS:                                  │
│  - IDocumentStorageService              │
│  - IExpirationNotificationEngine        │
│  - IAccessExpirationEnforcement         │
│                                         │
│ ADAPTERS:                               │
│  - SecureDocumentStorageService         │
│  - SqlServerComplianceRepository        │
│  - EmailNotificationAdapter             │
│                                         │
│ EVENTS:                                 │
│  - DocumentUploadedEvent                │
│  - DocumentApprovedEvent                │
│  - ExpirationNotificationSentEvent      │
│  - AccessSuspendedEvent                 │
│  - AccessRevokedEvent                   │
│                                         │
└─────────────────────────────────────────┘
```

---

## PART 5: ER Model (EP-07)

```sql
-- ============================================
-- COMPLIANCE CONTEXT TABLES
-- ============================================

CREATE TABLE [compliance].[documents] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [document_type] VARCHAR(64) NOT NULL,
    [document_name] VARCHAR(255) NOT NULL,
    [storage_uri] VARCHAR(MAX) NOT NULL,
    [file_size_bytes] BIGINT,
    [file_hash] VARCHAR(256),
    
    [uploaded_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [uploaded_by] UNIQUEIDENTIFIER,
    [status] VARCHAR(32) NOT NULL DEFAULT 'UPLOADED',
    [valid_until] DATETIME2,
    
    CONSTRAINT pk_documents PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_documents_user FOREIGN KEY (user_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id)
);

CREATE TABLE [compliance].[document_validators] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [document_id] UNIQUEIDENTIFIER NOT NULL,
    [validator_id] UNIQUEIDENTIFIER NOT NULL,
    
    [validation_status] VARCHAR(32),
    [validation_date] DATETIME2,
    [validation_notes] NVARCHAR(MAX),
    
    CONSTRAINT pk_document_validators PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_doc_validators_doc FOREIGN KEY (document_id, root_tenant_id) REFERENCES [compliance].[documents](id, root_tenant_id)
);

CREATE TABLE [configuration].[expiration_notification_rules] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64) NOT NULL,
    [name] VARCHAR(255) NOT NULL,
    
    [scope_type] VARCHAR(32),
    [target_user_category] VARCHAR(32),
    [days_before_expiration] INT NOT NULL,
    
    [notify_user] BIT,
    [notify_admin] BIT,
    [notify_approver] BIT,
    [notification_channels] NVARCHAR(MAX),
    [notification_frequency] VARCHAR(32),
    
    [enabled] BIT NOT NULL DEFAULT 1,
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT pk_expiration_notification_rules PRIMARY KEY (id, root_tenant_id)
);

CREATE TABLE [configuration].[access_expiration_policies] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64) NOT NULL,
    [name] VARCHAR(255) NOT NULL,
    
    [scope_type] VARCHAR(32) NOT NULL,
    [on_expiration_action] VARCHAR(32) NOT NULL,
    [grace_period_days] INT DEFAULT 0,
    
    [allow_extension] BIT,
    [max_extension_days] INT,
    [require_reapproval_on_extend] BIT,
    
    [enabled] BIT NOT NULL DEFAULT 1,
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT pk_access_expiration_policies PRIMARY KEY (id, root_tenant_id)
);

CREATE TABLE [compliance].[access_extension_requests] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [access_id] UNIQUEIDENTIFIER NOT NULL,
    
    [requested_by] UNIQUEIDENTIFIER NOT NULL,
    [requested_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [current_expiration_date] DATETIME2 NOT NULL,
    [proposed_new_expiration_date] DATETIME2 NOT NULL,
    [justification] NVARCHAR(MAX),
    
    [status] VARCHAR(32) NOT NULL DEFAULT 'PENDING',
    [approval_request_id] UNIQUEIDENTIFIER,
    [approved_at] DATETIME2,
    
    CONSTRAINT pk_access_extension_requests PRIMARY KEY (id, root_tenant_id)
);

-- Indices
CREATE INDEX idx_documents_user ON [compliance].[documents] (user_id, root_tenant_id)
    WHERE status IN ('UPLOADED', 'VALIDATING');
```

---

## Summary EP-07 Completed

- ✅ **FS-11**: Document Upload & Validation
- ✅ **FS-15** (NEW): Expiration Notification Rules
- ✅ **FS-16** (NEW): Access Expiration Enforcement (WARNING, SUSPEND, REVOKE)
- ✅ **Compliance Context**: Defined
- ✅ **ER Model**: Complete with all tables

---

**Approved by:** Principal Architect  
**Date:** 2026-05-14
