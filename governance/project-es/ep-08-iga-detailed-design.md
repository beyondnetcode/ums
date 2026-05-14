# EP-08: Diseño Detallado — IGA Avanzada (Identity Governance & Administration)

**Versión:** 1.0  
**Fecha:** 2026-05-14  
**Épica:** EP-08 (Post-MVP)  
**Historias:** US-031, US-032 (EXPAND a 5-6 historias)  
**Functional Story:** FS-12 (Role Promotion & Maturity)

---

## PARTE 1: IGA Strategic Domain & Role Maturity

### 1.1 Definición de IGA

**Identity Governance & Administration** es la práctica de:
- Mapear identidades a roles y responsabilidades
- Supervisar y evaluar la evolución de roles en el tiempo
- Autorizar cambios de responsabilidad (promociones)
- Auditar decisiones de gobernanza

**En UMS:** Esto significa definir un modelo donde roles y permisos evolucionan en el tiempo, y las transiciones son gobernadas, auditadas y auditables.

### 1.2 Role Maturity Model

Cada role tiene un nivel de madurez que refleja responsabilidad y seniority:

```csharp
public enum RoleMaturityLevel
{
    JUNIOR = 1,      // Aprendiz (0-6 meses)
    INTERMEDIATE = 2, // Contribuidor (6-18 meses)
    SENIOR = 3,      // Experto (18+ meses)
    LEAD = 4,        // Líder de equipo
    PRINCIPAL = 5    // Arquitecto/Estratega
}

public record RoleMaturityStatus
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public RoleMaturityLevel CurrentLevel { get; init; }
    public RoleMaturityLevel EligibleNextLevel { get; init; }
    
    // Timeline
    public DateTime AssignedAt { get; init; }
    public DateTime CurrentLevelSince { get; init; }  // Cuándo alcanzó este nivel
    public DateTime? EligibleForPromotionAt { get; init; }  // Cuándo es eligible
    
    // Cumplimiento
    public int CompletedCertifications { get; init; }
    public int CompletedTrainings { get; init; }
    public decimal PerformanceScore { get; init; }  // 0.0 a 5.0
    public bool HasNoComplianceIssues { get; init; }
    
    public string? BlockingFactor { get; init; }  // Ej: "Pending CISSP certification"
}
```

---

## PARTE 2: FS-12 — Role Promotion Process (EXPANDIDO)

### 2.1 Definición Expandida

**FS-12** gestiona el ciclo de vida completo de una promoción:

1. **Eligibility Check** → Verificar que user es eligible
2. **Impact Analysis** → Calcular qué permisos nuevos, riesgos
3. **Approval** → Gerente + Security aprueba
4. **Execution** → Aplicar nueva role
5. **Verification** → Auditar los cambios

### 2.2 Sub-Historias Expandidas (5-6 historias)

#### US-031: Request Role Promotion (Requestor)
**Como:** Usuario Senior con 2 años en rol  
**Quiero:** Solicitar promoción a Lead  
**Para que:** Mi compensación y responsabilidades se alineen

**Aceptación:**
- Usuario puede ver cuál es su rol actual y siguiente eligible
- Puede describir motivos + logros
- Request guardado en DRAFT status
- Audit registra: PROMOTION_REQUESTED

---

#### US-032: Review Promotion Impact (Reviewer)
**Como:** Security Administrator  
**Quiero:** Ver impacto de una promoción (nuevas permisos, sistemas afectados)  
**Para que:** No apruebe cambios que causen riesgos

**Aceptación:**
- Impacto muestra: permisos actuales vs nuevos
- Sistemas afectados listados
- Risk score calculado (0-100)
- Conflicting permissions identificados (si es posible)
- Reviewer puede comentar

---

#### US-033: Approve/Reject Promotion (Manager)
**Como:** Manager directo  
**Quiero:** Aprobar o rechazar solicitud de promoción  
**Para que:** Mi equipo esté alineado

**Aceptación:**
- Manager ve solicitud con impact analysis
- Puede aprobar o rechazar
- Debe escribir motivo
- Si aprueba → workflow a siguiente approver
- Si rechaza → solicitud cerrada

---

#### US-034: Execute Promotion (Admin)
**Como:** Admin IGA  
**Quiero:** Ejecutar una promoción aprobada  
**Para que:** Los permisos nuevos sean aplicados

**Aceptación:**
- Solo disponible si approval chain completa
- Al ejecutar:
  - Role en usuario actualizado
  - Permisos nuevos asignados
  - Permisos viejos removidos (si aplica)
  - Maturity level actualizado
  - Audit event creado
- Notificación a usuario

---

#### US-035: Monitor Promotion Metrics (Analytics)
**Como:** HR Analytics  
**Quiero:** Ver metrics de promociones (tiempo promedio, aprobación rates)  
**Para que:** Identifique bottlenecks

**Aceptación:**
- Dashboard mostrando:
  - Promociones pendientes (count, edad)
  - Approval time (avg, median, P95)
  - Rejection rate por approver
  - Blocked reasons (reasons por qué se rechaza)
  - Impact score distribution

---

#### US-036: Promotion Eligibility Engine (Automated)
**Como:** IGA System  
**Quiero:** Auto-calcular cuándo usuario es eligible  
**Para que:** Notificaciones automáticas

**Aceptación:**
- Cada noche, recalcular eligibility
- Si usuario es nuevamente eligible:
  - Notificar user + manager
  - Crear "eligible for promotion" badge
- Considerar:
  - Tiempo en rol actual
  - Certifications completadas
  - Training completadas
  - Performance score
  - Compliance issues

---

### 2.3 Impact Analysis Engine

```csharp
public class RolePromotionImpactAnalysis
{
    public Guid UserId { get; set; }
    public Guid CurrentRoleId { get; set; }
    public Guid TargetRoleId { get; set; }
    
    // Permisos
    public List<Permission> CurrentPermissions { get; set; }
    public List<Permission> TargetPermissions { get; set; }
    public List<Permission> PermissionsAdded { get; set; }
    public List<Permission> PermissionsRemoved { get; set; }
    public List<Permission> ConflictingPermissions { get; set; }
    
    // Sistemas afectados
    public List<SystemImpact> AffectedSystems { get; set; }
    
    // Riesgo
    public decimal RiskScore { get; set; }  // 0-100
    public List<string> RiskFactors { get; set; }
    public List<string> MitigationsSuggested { get; set; }
    
    // Auditoría
    public DateTime AnalyzedAt { get; set; }
    public string AnalyzedBy { get; set; }
}

public record SystemImpact
{
    public string SystemName { get; init; }
    public int NewPermissionsCount { get; init; }
    public string ImpactLevel { get; init; }  // LOW, MEDIUM, HIGH, CRITICAL
    public string? Details { get; init; }
}

public class PromotionImpactAnalysisService
{
    public async Task<RolePromotionImpactAnalysis> AnalyzeAsync(
        User user, 
        Role currentRole, 
        Role targetRole)
    {
        // 1. Get permisos actuales
        var currentPermissions = await _authorizationService
            .GetEffectivePermissionsAsync(user.Id);
        
        // 2. Get permisos del target role
        var targetPermissions = await _authorizationService
            .GetPermissionsByRoleAsync(targetRole.Id);
        
        // 3. Calcular diferencias
        var added = targetPermissions
            .Except(currentPermissions, new PermissionComparer())
            .ToList();
        
        var removed = currentPermissions
            .Except(targetPermissions, new PermissionComparer())
            .ToList();
        
        // 4. Detectar conflicting permissions (ej: create + delete same resource = risky)
        var conflicting = DetectConflictingPermissions(added);
        
        // 5. Identificar sistemas afectados
        var affectedSystems = added
            .GroupBy(p => p.System)
            .Select(g => new SystemImpact
            {
                SystemName = g.Key,
                NewPermissionsCount = g.Count(),
                ImpactLevel = CalculateImpactLevel(g),
                Details = string.Join(", ", g.Select(p => p.ActionCode))
            })
            .ToList();
        
        // 6. Calcular risk score
        var riskScore = CalculateRiskScore(added, removed, targetRole, user);
        
        return new RolePromotionImpactAnalysis
        {
            UserId = user.Id,
            CurrentRoleId = currentRole.Id,
            TargetRoleId = targetRole.Id,
            CurrentPermissions = currentPermissions.ToList(),
            TargetPermissions = targetPermissions.ToList(),
            PermissionsAdded = added,
            PermissionsRemoved = removed,
            ConflictingPermissions = conflicting,
            AffectedSystems = affectedSystems,
            RiskScore = riskScore,
            RiskFactors = IdentifyRiskFactors(riskScore, added, user),
            AnalyzedAt = DateTime.UtcNow,
            AnalyzedBy = "PromotionImpactAnalysisEngine"
        };
    }
    
    private decimal CalculateRiskScore(
        List<Permission> added, 
        List<Permission> removed, 
        Role targetRole, 
        User user)
    {
        decimal score = 0;
        
        // Factor 1: Permissions sensitivity (0-40)
        var sensitivePermissions = added.Count(p => p.RiskLevel == "CRITICAL");
        score += Math.Min(40, sensitivePermissions * 10);
        
        // Factor 2: Role seniority jump (0-30)
        var seniority = targetRole.MaturityLevel - (user.CurrentRole.MaturityLevel ?? 0);
        if (seniority > 2) score += 30;  // Jumping more than 2 levels = risky
        else if (seniority > 1) score += 15;
        
        // Factor 3: Time in current role (0-20)
        var timeInRole = (DateTime.UtcNow - user.RoleAssignedAt).TotalDays;
        if (timeInRole < 180) score += 20;  // Less than 6 months = risky
        else if (timeInRole < 365) score += 10;
        
        // Factor 4: User compliance history (0-10)
        var compliance = await _auditService.GetComplianceScoreAsync(user.Id);
        if (compliance < 0.9) score += 10;
        
        return Math.Min(100, score);
    }
    
    private List<string> DetectConflictingPermissions(List<Permission> newPermissions)
    {
        var conflicts = new List<string>();
        
        // Anti-patterns
        var hasCreate = newPermissions.Any(p => p.ActionCode.Contains("CREATE"));
        var hasDelete = newPermissions.Any(p => p.ActionCode.Contains("DELETE"));
        var hasApprove = newPermissions.Any(p => p.ActionCode.Contains("APPROVE"));
        var hasExecute = newPermissions.Any(p => p.ActionCode.Contains("EXECUTE"));
        
        if (hasApprove && hasExecute)
            conflicts.Add("APPROVAL_EXECUTION_CONFLICT: User can approve and execute same action");
        
        if (newPermissions.Count > 15)
            conflicts.Add("HIGH_PRIVILEGE_COUNT: More than 15 new permissions");
        
        return conflicts;
    }
}
```

### 2.4 Promotion Workflow

```
┌──────────────────────────────────────────────────────┐
│     ROLE PROMOTION WORKFLOW (State Machine)          │
└──────────────────────────────────────────────────────┘

                [DRAFT]
                  │
         User submits request
                  ▼
          [PENDING_MANAGER_APPROVAL]
                  │
        Manager approves/rejects
         ✗        │        ✓
        [REJECTED]│   [PENDING_SECURITY_REVIEW]
                  │        │
             Security reviews & analyzes
                  │
        ┌─────────┴─────────┐
        │                   │
    (Risky)             (Safe)
        │                   │
        ▼                   │
  [PENDING_SECURITY_      [SECURITY_APPROVED]
   APPROVAL]              │
        │                 │
    Security       (No additional approval needed)
    approves            │
        │               │
        └───────┬───────┘
                ▼
        [APPROVED_READY_TO_EXECUTE]
                │
          Admin executes
                ▼
        [EXECUTED]
                │
          System verifies
                ▼
        [VERIFIED] ── or ── [VERIFICATION_FAILED]
                                   │
                            (Rollback & notify)
```

---

## PARTE 3: IGA Bounded Context

```
┌─────────────────────────────────────────────┐
│        IGA BOUNDED CONTEXT                  │
├─────────────────────────────────────────────┤
│                                             │
│ AGGREGATES:                                 │
│  - RoleMaturityStatus                       │
│  - PromotionRequest                         │
│  - PromotionImpactAnalysis                  │
│                                             │
│ PORTS (Abstractions):                       │
│  - IPromotionApprovalService                │
│  - IPromotionImpactAnalyzer                 │
│  - IEligibilityCalculator                   │
│  - IPromotionExecutor                       │
│                                             │
│ ADAPTERS:                                   │
│  - SqlServerIGARepository                   │
│  - RolePromotionApprovalAdapter             │
│  - PromotionImpactAnalysisAdapter           │
│                                             │
│ EVENTS:                                     │
│  - PromotionRequestedEvent                  │
│  - PromotionEligibilityCalculatedEvent      │
│  - PromotionApprovedEvent                   │
│  - PromotionRejectedEvent                   │
│  - PromotionExecutedEvent                   │
│  - PromotionVerifiedEvent                   │
│                                             │
└─────────────────────────────────────────────┘
```

---

## PARTE 4: ER Model (EP-08)

```sql
-- ============================================
-- IGA CONTEXT TABLES
-- ============================================

CREATE TABLE [iga].[role_maturity_levels] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [role_id] UNIQUEIDENTIFIER NOT NULL,
    
    [current_maturity_level] VARCHAR(32) NOT NULL,  -- JUNIOR, INTERMEDIATE, SENIOR, LEAD, PRINCIPAL
    [next_eligible_maturity_level] VARCHAR(32),
    
    [assigned_at] DATETIME2 NOT NULL,
    [current_level_since] DATETIME2 NOT NULL,
    [eligible_for_promotion_at] DATETIME2,
    
    -- Cumplimiento
    [completed_certifications_count] INT DEFAULT 0,
    [completed_trainings_count] INT DEFAULT 0,
    [performance_score] DECIMAL(3,2),  -- 0.0 to 5.0
    [has_no_compliance_issues] BIT DEFAULT 1,
    
    [blocking_factor] NVARCHAR(MAX),
    [last_reviewed_at] DATETIME2,
    
    CONSTRAINT pk_role_maturity_levels PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_role_maturity_user FOREIGN KEY (user_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id)
);

CREATE TABLE [iga].[promotion_requests] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    
    [current_role_id] UNIQUEIDENTIFIER NOT NULL,
    [target_role_id] UNIQUEIDENTIFIER NOT NULL,
    
    [requested_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [requested_by] UNIQUEIDENTIFIER NOT NULL,
    [request_reason] NVARCHAR(MAX),
    
    -- Approval chain
    [manager_id] UNIQUEIDENTIFIER NOT NULL,
    [manager_approval_status] VARCHAR(32),  -- PENDING, APPROVED, REJECTED
    [manager_decision_at] DATETIME2,
    [manager_decision_reason] NVARCHAR(MAX),
    
    [security_approval_status] VARCHAR(32),
    [security_decision_at] DATETIME2,
    
    -- Overall status
    [status] VARCHAR(32) NOT NULL DEFAULT 'DRAFT',  -- DRAFT, PENDING_MANAGER_APPROVAL, PENDING_SECURITY_REVIEW, APPROVED, REJECTED, EXECUTED, VERIFIED, FAILED
    [final_status] VARCHAR(32),  -- PROMOTED, REJECTED, ROLLED_BACK
    
    -- Execution
    [executed_at] DATETIME2,
    [executed_by] UNIQUEIDENTIFIER,
    [verified_at] DATETIME2,
    
    CONSTRAINT pk_promotion_requests PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_promotion_requests_user FOREIGN KEY (user_id, root_tenant_id) REFERENCES [identity].[users](id, root_tenant_id)
);

CREATE TABLE [iga].[promotion_impact_analysis] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [promotion_request_id] UNIQUEIDENTIFIER NOT NULL,
    
    [risk_score] DECIMAL(5,2),
    [risk_level] VARCHAR(32),  -- LOW, MEDIUM, HIGH, CRITICAL
    [new_permissions_count] INT,
    [removed_permissions_count] INT,
    [affected_systems_count] INT,
    
    [conflicting_permissions] NVARCHAR(MAX),  -- JSON array
    [risk_factors] NVARCHAR(MAX),  -- JSON array
    [suggested_mitigations] NVARCHAR(MAX),  -- JSON array
    
    [analyzed_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [analyzed_by] VARCHAR(255),
    
    CONSTRAINT pk_promotion_impact_analysis PRIMARY KEY (id, root_tenant_id),
    CONSTRAINT fk_promotion_impact_request FOREIGN KEY (promotion_request_id, root_tenant_id) REFERENCES [iga].[promotion_requests](id, root_tenant_id)
);

CREATE TABLE [iga].[promotion_eligible_notifications] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    
    [eligible_for_next_level] VARCHAR(32),
    [eligible_at] DATETIME2 NOT NULL,
    [notification_sent_at] DATETIME2,
    [user_acknowledged_at] DATETIME2,
    
    CONSTRAINT pk_promotion_eligible_notifications PRIMARY KEY (id, root_tenant_id)
);

-- Indices
CREATE INDEX idx_role_maturity_user ON [iga].[role_maturity_levels] (user_id, root_tenant_id);
CREATE INDEX idx_promotion_requests_user ON [iga].[promotion_requests] (user_id, root_tenant_id)
    WHERE status IN ('PENDING_MANAGER_APPROVAL', 'PENDING_SECURITY_REVIEW');
CREATE INDEX idx_promotion_requests_manager ON [iga].[promotion_requests] (manager_id, root_tenant_id)
    WHERE manager_approval_status = 'PENDING';
```

---

## PARTE 5: Integration (IGA con otras contexts)

### 5.1 IGA ↔ Approvals

Promotion requests pueden requerir formal approval workflow si target role es sensitive:

```csharp
// IGA → Approvals: Crear approval request
if (targetRole.RiskLevel == "CRITICAL")
{
    var approvalRequest = await _approvalService.CreateApprovalRequestAsync(
        workflow: "ROLE_PROMOTION_APPROVAL",
        requester: user.Id,
        targetUser: user.Id,
        requestedAction: $"Promote from {currentRole.Name} to {targetRole.Name}",
        linkedEntity: promotionRequest.Id);
    
    promotionRequest.ApprovalRequestId = approvalRequest.Id;
}
```

### 5.2 IGA ↔ Authorization

Cuando promotion se ejecuta, permisos del usuario se actualizan:

```csharp
// IGA → Authorization: Update user permissions
await _authorizationService.RevokePermissionsAsync(user.Id, currentRole.Id);
await _authorizationService.AssignPermissionsAsync(user.Id, targetRole.Id);
```

### 5.3 IGA ↔ Audit

Todos los eventos auditados:

```csharp
await _auditService.LogAsync(new AuditEvent
{
    EventType = "PROMOTION_EXECUTED",
    UserId = user.Id,
    ResourceId = promotionRequest.Id.ToString(),
    Details = new 
    { 
        FromRole = currentRole.Name, 
        ToRole = targetRole.Name,
        RiskScore = impactAnalysis.RiskScore 
    }
});
```

---

## Summary EP-08 Completado

- ✅ **FS-12 Expanded**: 6 sub-historias (US-031 a US-036)
- ✅ **IGA Bounded Context**: Definido con agregados, puertos, adaptadores, eventos
- ✅ **Role Maturity Model**: 5 niveles (JUNIOR a PRINCIPAL)
- ✅ **Promotion Impact Analysis**: Risk scoring, permission analysis, affected systems
- ✅ **ER Model**: Tables para maturity, requests, analysis
- ✅ **Integration**: IGA ↔ Approvals, Authorization, Audit

---

**Aprobado por:** Arquitecto Principal  
**Fecha:** 2026-05-14
