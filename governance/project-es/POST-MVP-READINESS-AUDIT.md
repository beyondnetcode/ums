# Post-MVP Readiness Audit — EP-06, EP-07, EP-08

**Versión:** 1.0  
**Fecha:** 2026-05-14  
**Objetivo:** Auditar completitud de las 3 épicas post-MVP; identificar gaps y prioridades de análisis.

---

## 1. Resumen Ejecutivo

| Épica | Descripción | Historias (US) | Readiness Actual | Gap Crítico |
|-------|-------------|-----------------|------------------|------------|
| **EP-06** | Seguridad, Acceso Externo, Delegación | US-017 a US-022 | 60% | FS-09, FS-14 incompletas; ADRs claros |
| **EP-07** | Ciclo de Vida de Cumplimiento | US-023 a US-028 | 40% | FS-15, FS-16 pendientes; modelo de datos falta |
| **EP-08** | IGA Avanzada (Promoción de Roles) | US-031, US-032 | 30% | FS-12 solo 2 US; modelo estado máquina incompleto |

**Recomendación:** Expandir en orden: **EP-06 → EP-07 → EP-08** (dependencias en cascada)

---

## 2. EP-06: Seguridad, Acceso Externo y Delegación

### 2.1 Estado Actual (60% Readiness)

#### ✅ Existe

| Componente | Dónde | Detalle |
|-----------|-------|--------|
| **ADR-0044** | `/architecture/adrs/0044-delegated-admin-and-approvals.md` | Delegated admin + approval workflows definidos |
| **ADR-0026** | `/architecture/adrs/0026-mfa-passwordless-adaptive-authentication.md` | MFA & passwordless strategy |
| **ADR-0038** | `/architecture/adrs/0038-delegated-administration-temporal-scopes.md` | Temporal scopes para delegación |
| **Approvals Context** | `/architecture/blueprints/bounded-context-map.md` | Definido con entities, routes, events |
| **Historias US** | `/governance/project-es/mvp-product-backlog.md` | US-017 a US-022 presentes |

#### ⚠️ Incompleto

| Componente | Problema | Prioridad | Esfuerzo |
|-----------|----------|-----------|----------|
| **FS-09: Adaptive MFA & Passwordless** | Solo header; FS sin detalles de aceptación | ALTA | 3h |
| **FS-14: Delegated Admin Scopes** | Incomplete; falta state machine de validación | ALTA | 4h |
| **ER Model para Approvals** | No mapeado a entidades SQL; falta closure table para hierarchical approval | ALTA | 2h |
| **API Contract (OpenAPI)** | No existen rutas detalladas; DTOs pendientes | MEDIA | 2h |
| **Integration Points** | Falta claridad en cómo Approvals integra con Authorization context | MEDIA | 2h |

#### ❌ Falta Completamente

| Componente | Impacto | Esfuerzo |
|-----------|--------|----------|
| **Approval State Machine** | ¿Estados válidos? ¿Transiciones?¿Timeout? | 3h |
| **Escalation Rules** | ¿Qué pasa si approver no responde en X días? | 2h |
| **Delegation Validation Logic** | Principio de Least Privilege — cómo se implementa en código? | 2h |

---

### 2.2 Functional Stories Detail (EP-06)

#### FS-09: Adaptive MFA & Passwordless Authentication

**Historias:** US-017, US-018

**Gaps:**
- [ ] Risk scoring model (qué califica como "riesgo alto"?)
- [ ] Passwordless methods (FIDO2, magic link, etc.)
- [ ] Configuration UI/API
- [ ] Integration con Identity Context (FS-01)

**Dependencias:**
- ADR-0026 (MFA strategy)
- Identity Context (autenticación base)

**Estimado:** 8 puntos (1-2 sprints)

---

#### FS-14: Delegated Administration & Scopes

**Historias:** US-021, US-022

**Gaps:**
- [ ] Delegation table schema (qué campos definen un "scope"?)
- [ ] Scope validation (¿admin delegado puede hacer X acción?)
- [ ] Temporal constraints (expiración de delegaciones)
- [ ] Approval required para crear delegación
- [ ] Audit logging de cambios delegación

**Dependencias:**
- ADR-0044 (delegated admin)
- ADR-0038 (temporal scopes)
- Audit Context (auditoria)
- Approvals Context (workflow de aprobación)

**Estimado:** 13 puntos (2-3 sprints)

---

#### FS-10: B2B External Access Request & Approval Flow

**Historias:** US-019, US-020

**Estado:** Parcialmente definida (Approvals Context existe)

**Gaps:**
- [ ] Document attachment handling (qué documentos son requeridos?)
- [ ] Approval chain (quién aprueba? serial vs paralelo?)
- [ ] Expiration enforcement (cómo revoca acceso al expirar?)
- [ ] Integration con Compliance Context (FS-11 document validation)

**Dependencias:**
- Approvals Context (workflow)
- Audit Context (trail)
- Configuration Context (approval rules)

**Estimado:** 8 puntos (1-2 sprints)

---

### 2.3 ER Model Gaps (EP-06)

**Tablas faltantes/incompletas:**

```sql
-- Falta definición completa
CREATE TABLE [approval].[approval_workflows] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [name] VARCHAR(255),
    [trigger_type] VARCHAR(32),  -- 'USER_ONBOARDING', 'PROFILE_ASSIGNMENT', 'DELEGATION'
    [approval_type] VARCHAR(32), -- 'SERIAL', 'PARALLEL', 'QUORUM'
    [required_approvals] INT,
    [timeout_days] INT,
    -- Falta: approval_rules, escalation_config, metadata
);

-- Incompleta
CREATE TABLE [approval].[approval_requests] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [workflow_id] UNIQUEIDENTIFIER,
    [requester_id] UNIQUEIDENTIFIER,
    [target_user_id] UNIQUEIDENTIFIER,
    [requested_action] VARCHAR(255),
    [status] VARCHAR(32),  -- 'PENDING', 'APPROVED', 'REJECTED'
    [created_at] DATETIME2,
    [expires_at] DATETIME2,
    -- Falta: approval_attachments relationship, escalation tracking
);

-- No existe
CREATE TABLE [approval].[approval_approvers] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [approval_request_id] UNIQUEIDENTIFIER,
    [approver_id] UNIQUEIDENTIFIER,
    [approval_status] VARCHAR(32),  -- 'PENDING', 'APPROVED', 'REJECTED'
    [approval_order] INT,  -- Para SERIAL approvals
    [approved_at] DATETIME2,
    [decision_reason] NVARCHAR(MAX),
    [root_tenant_id] UNIQUEIDENTIFIER
);

-- No existe
CREATE TABLE [delegation].[user_management_delegations] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [delegating_admin_id] UNIQUEIDENTIFIER,  -- Quién delega
    [delegated_admin_id] UNIQUEIDENTIFIER,   -- A quién
    [scope_type] VARCHAR(32),  -- 'TENANT', 'ORGANIZATION', 'SYSTEM'
    [scope_id] UNIQUEIDENTIFIER,
    [allowed_actions] VARCHAR(MAX),  -- JSON: ['CREATE_USER', 'ASSIGN_PROFILE', ...]
    [valid_from] DATETIME2,
    [valid_until] DATETIME2,
    [approval_request_id] UNIQUEIDENTIFIER,  -- Link a aprobación
    [status] VARCHAR(32)  -- 'ACTIVE', 'PENDING', 'EXPIRED', 'REVOKED'
);
```

**Impacto:** Blockea implementación de FS-14 (delegación) y FS-10 (aprobaciones B2B).

---

## 3. EP-07: Ciclo de Vida de Cumplimiento

### 3.1 Estado Actual (40% Readiness)

#### ✅ Existe

| Componente | Dónde | Detalle |
|-----------|-------|--------|
| **ADR-0045** | `/architecture/adrs/0045-document-lifecycle-enforcement.md` | Document lifecycle defined |
| **Historias US** | MVP backlog | US-023 a US-028 presentes |

#### ⚠️ Incompleto

| Componente | Problema | Prioridad |
|-----------|----------|-----------|
| **FS-15: Expiration Notification Rules** | No existe; solo nombrada en backlog | ALTA |
| **FS-16: Access Behavior on Expiration** | No existe; modelo de enforcement falta | ALTA |
| **FS-11: Document Lifecycle** | Parcial; falta workflow de validación | MEDIA |
| **ER Model** | Tablas de documentos, notificaciones, policies no mapeadas | ALTA |

#### ❌ Falta Completamente

| Componente | Impacto |
|-----------|--------|
| **Compliance Context** | Bounded context NO definido |
| **Notification Engine** | Cómo se envían alertas (email, webhook, etc.)? |
| **Policy Enforcement Logic** | Qué ocurre cuando acceso expira (warning, suspend, revoke)? |
| **Document Validation Workflow** | Estados del documento, validadores, revalidación? |

---

### 3.2 Functional Stories Detail (EP-07)

#### FS-11: Upload & Validate User Document

**Historias:** US-023, US-024

**Gaps:**
- [ ] Document types (qué documentos se aceptan?)
- [ ] Storage system (dónde se guardan? cloud storage? encrypted?)
- [ ] Validation workflow (quién valida? qué criterios?)
- [ ] Revalidation trigger (cada cuánto se revalida?)
- [ ] Integration con expiration enforcement

**Estimado:** 8 puntos

---

#### FS-15: Expiration Notification Rules (NO EXISTE)

**Historias:** US-025, US-026

**Necesario Definir:**
- [ ] Rule model (qué es una regla de notificación?)
- [ ] Trigger conditions (¿30 días antes de expirar? configurable?)
- [ ] Notification channels (email, SMS, in-app?)
- [ ] Audience (quién se notifica: usuario, admin, ambos?)
- [ ] Frequency (una sola notificación o reiterada?)

**Estimado:** 8 puntos

---

#### FS-16: Access Behavior on Expiration (NO EXISTE)

**Historias:** US-027, US-028

**Necesario Definir:**
- [ ] Enforcement modes (warning vs suspension vs revoke?)
- [ ] Configuration per policy (diferentes comportamientos por tipo de acceso?)
- [ ] Grace periods (días antes del enforcement real?)
- [ ] Audit trail (qué se registra cuando expira?)
- [ ] Recovery path (cómo reactivar acceso expirado?)

**Estimado:** 10 puntos

---

### 3.3 ER Model Gaps (EP-07)

```sql
-- No existe
CREATE TABLE [compliance].[documents] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] UNIQUEIDENTIFIER,
    [document_type] VARCHAR(64),  -- 'IDENTITY_PROOF', 'SERVICE_AGREEMENT', etc.
    [document_name] VARCHAR(255),
    [storage_uri] VARCHAR(MAX),  -- Reference a file storage
    [uploaded_at] DATETIME2,
    [status] VARCHAR(32),  -- 'UPLOADED', 'VALIDATING', 'APPROVED', 'REJECTED'
    [valid_until] DATETIME2,  -- Revalidation deadline
    [validation_notes] NVARCHAR(MAX)
);

-- No existe
CREATE TABLE [compliance].[document_validators] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [document_id] UNIQUEIDENTIFIER,
    [validator_id] UNIQUEIDENTIFIER,
    [validation_status] VARCHAR(32),  -- 'PENDING', 'APPROVED', 'REJECTED'
    [validation_date] DATETIME2,
    [validation_reason] NVARCHAR(MAX),
    [root_tenant_id] UNIQUEIDENTIFIER
);

-- No existe
CREATE TABLE [compliance].[expiration_notification_rules] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64),
    [name] VARCHAR(255),
    [scope_type] VARCHAR(32),  -- 'GLOBAL', 'TENANT', 'ORGANIZATION'
    [target_user_category] VARCHAR(32),  -- 'INTERNAL', 'EXTERNAL', 'B2B'
    [days_before_expiration] INT,
    [notification_channels] VARCHAR(MAX),  -- JSON: ['EMAIL', 'IN_APP']
    [notify_user] BIT,
    [notify_admin] BIT,
    [notify_approver] BIT,
    [frequency] VARCHAR(32)  -- 'ONCE', 'DAILY', 'WEEKLY'
);

-- No existe
CREATE TABLE [compliance].[access_expiration_policies] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [code] VARCHAR(64),
    [name] VARCHAR(255),
    [scope_type] VARCHAR(32),  -- 'PROFILE', 'PERMISSION', 'DELEGATION'
    [on_expiration_action] VARCHAR(32),  -- 'WARNING', 'SUSPEND', 'REVOKE'
    [grace_period_days] INT,
    [allow_extension] BIT,
    [require_reapproval_on_extend] BIT
);
```

**Impacto:** Blockea todo EP-07 (compliance).

---

## 4. EP-08: IGA Avanzada (Promoción de Roles)

### 4.1 Estado Actual (30% Readiness)

#### ✅ Existe

| Componente | Dónde | Detalle |
|-----------|-------|--------|
| **ADR-0046** | `/architecture/adrs/0046-role-evolution-and-promotion.md` | Role evolution strategy |
| **Historias US** | MVP backlog | US-031, US-032 solamente |

#### ⚠️ Incompleto

| Componente | Problema | Prioridad |
|-----------|----------|-----------|
| **FS-12: Role Promotion Process** | Solo 2 US (US-031, US-032); falta detalle de workflow | ALTA |
| **State Machine** | Qué estados tiene una solicitud de promoción? | ALTA |
| **Impact Analysis** | Cómo se calcula impacto (nuevas permissions, sistemas afectados)? | MEDIA |
| **IGA Context Definition** | No existe bounded context para IGA | ALTA |

#### ❌ Falta Completamente

| Componente | Impacto |
|-----------|--------|
| **IGA Bounded Context** | Strategic domain missing |
| **Impact Analysis Engine** | Cómo se calculan permisos nuevos y riesgos? |
| **Role Maturity Model** | Qué significa "promote a role"? (evolution, lifecycle?) |
| **ER Model para Role Promotion** | Tablas de solicitudes, aprobaciones, impacto |

---

### 4.2 Functional Stories Detail (EP-08)

#### FS-12: Role Promotion Process (INCOMPLETO)

**Historias:** US-031, US-032 (solo 2 historias para épica de 13 puntos?)

**Gaps:**
- [ ] Definition: ¿"Promoción" = cambio de responsabilidades? ¿Evolución de role?
- [ ] Current state: ¿Cuál es el rol actual?
- [ ] Target state: ¿A qué rol quiere promoverse?
- [ ] Impact analysis: ¿Qué permisos nuevos? ¿Qué sistemas? ¿Riesgo?
- [ ] Approval process: ¿Quién aprueba? ¿Requiere documentación?
- [ ] Execution: ¿Cómo se aplica la promoción?
- [ ] Audit trail: ¿Qué se registra?

**Estimado:** 13 puntos (2-3 sprints)

---

### 4.3 ER Model Gaps (EP-08)

```sql
-- No existe
CREATE TABLE [iga].[role_promotion_requests] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [requesting_user_id] UNIQUEIDENTIFIER,
    [target_user_id] UNIQUEIDENTIFIER,
    [current_role_id] UNIQUEIDENTIFIER,
    [target_role_id] UNIQUEIDENTIFIER,
    [promotion_reason] NVARCHAR(MAX),
    [request_date] DATETIME2,
    [status] VARCHAR(32),  -- 'DRAFT', 'PENDING_REVIEW', 'APPROVED', 'REJECTED', 'EXECUTED'
    [approval_request_id] UNIQUEIDENTIFIER  -- Link a Approvals
);

-- No existe
CREATE TABLE [iga].[role_promotion_impact_analysis] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [promotion_request_id] UNIQUEIDENTIFIER,
    [new_permissions_count] INT,
    [affected_systems_count] INT,
    [risk_score] DECIMAL(3,2),  -- 0.0 to 1.0
    [conflicting_permissions] VARCHAR(MAX),  -- JSON array
    [analysis_timestamp] DATETIME2,
    [analyst_notes] NVARCHAR(MAX)
);

-- No existe
CREATE TABLE [iga].[role_maturity_levels] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY,
    [root_tenant_id] UNIQUEIDENTIFIER NOT NULL,
    [role_id] UNIQUEIDENTIFIER,
    [current_maturity_level] VARCHAR(32),  -- 'JUNIOR', 'SENIOR', 'LEAD', 'PRINCIPAL'
    [next_maturity_level] VARCHAR(32),
    [days_at_current_level] INT,
    [promotion_eligibility_date] DATETIME2,
    [last_review_date] DATETIME2
);
```

---

## 5. Matriz de Dependencias (Cross-Epic)

```
EP-06 (Approvals/Security)
  ├── FS-09 (MFA) → Identity Context ✅
  ├── FS-14 (Delegation) → ADR-0044 ✅, Audit Context ✅
  └── FS-10 (B2B Approval) → Approvals Context (nueva)

EP-07 (Compliance)
  ├── FS-11 (Documents) → Approvals Context (EP-06), Document storage
  ├── FS-15 (Notifications) → Configuration Context ✅
  └── FS-16 (Expiration) → Authorization Context ✅

EP-08 (IGA)
  ├── FS-12 (Role Promotion) → Approvals Context (EP-06), Impact Analysis Engine
  └── → Bounded Context nuevo (IGA)
```

**Conclusión:** EP-06 es prerequisito para EP-07 y EP-08. Secuencia: **EP-06 → EP-07 → EP-08**.

---

## 6. Plan de Completitud por Épica

### EP-06: 5 días estimados

1. **FS-09 Adaptive MFA** (3h)
   - Acceptance criteria detallado
   - Risk scoring model
   
2. **FS-14 Delegated Admin** (4h)
   - State machine (delegation states)
   - Scope validation logic
   - Temporal constraints
   
3. **FS-10 B2B Approval** (2h)
   - Document attachment rules
   - Approval chain definition
   
4. **ER Model EP-06** (2h)
   - approval_workflows
   - approval_requests, approval_approvers
   - user_management_delegations
   
5. **Integration Map** (2h)
   - Approvals ↔ Authorization
   - Approvals ↔ Audit
   - Approvals ↔ Configuration

**Total EP-06:** 13 horas

---

### EP-07: 5 días estimados

1. **FS-11 Document Lifecycle** (3h)
   - Document type catalog
   - Validation workflow
   - Storage integration
   
2. **FS-15 Notification Rules (CREATE)** (3h)
   - Rule model definition
   - Trigger conditions
   - Notification channels
   
3. **FS-16 Expiration Enforcement (CREATE)** (3h)
   - Enforcement modes
   - Grace periods
   - Recovery paths
   
4. **ER Model EP-07** (2h)
   - documents, document_validators
   - expiration_notification_rules
   - access_expiration_policies
   
5. **Compliance Context Definition** (2h)
   - Bounded context (entities, ports, events)
   - Integration con otras contexts

**Total EP-07:** 13 horas

---

### EP-08: 4 días estimados

1. **FS-12 Role Promotion (EXPAND)** (4h)
   - Break into sub-stories (5-6 historias más)
   - Impact analysis engine design
   - Execution flow
   
2. **ER Model EP-08** (2h)
   - role_promotion_requests
   - role_promotion_impact_analysis
   - role_maturity_levels
   
3. **IGA Context Definition** (2h)
   - Bounded context design
   - Port abstractions
   - Event contracts
   
4. **Integration: IGA ↔ Approvals ↔ Authorization** (2h)

**Total EP-08:** 10 horas

---

## 7. Recomendaciones & Next Steps

### 🔴 Crítico (Bloquea construcción)

1. **EP-06 ER Model** (approval_workflows, requests, delegations)
   - Requerida para Sprint 2 (después de MVP)
   - **Esfuerzo:** 2h
   - **Prioridad:** Ahora mismo
   
2. **EP-07 ER Model** (documents, notifications, policies)
   - Requerida para Sprint 2
   - **Esfuerzo:** 2h
   - **Prioridad:** Ahora mismo

3. **EP-06 FS-14 Scope Validation Logic**
   - Design document necesario para implementación correcta
   - **Esfuerzo:** 2h
   - **Prioridad:** Esta semana

### 🟡 Importante (Impacta Sprint planning)

4. **EP-07 FS-15, FS-16 (CREATE)**
   - Nuevas historias no existen
   - Esfuerzo: 6h
   - Prioridad: Esta semana

5. **EP-08 IGA Context + FS-12 Expansion**
   - Esfuerzo: 4h
   - Prioridad: Próxima semana

### 🟢 Recomendado (Mejora visibilidad)

6. **Bounded Context Maps** para Compliance + IGA
   - Visualización clara
   - Esfuerzo: 2h
   - Prioridad: Próxima semana

---

## 8. Exit Criteria (Post-MVP Readiness = 100%)

- [ ] EP-06: Todas las FS tienen acceptance criteria detallado; ER Model > 90% completo
- [ ] EP-07: Todas las FS definidas (incluyendo FS-15, FS-16 nuevas); ER Model > 90% completo
- [ ] EP-08: FS-12 expandida a 5-6 historias; IGA Context diseñado; ER Model > 80% completo
- [ ] Bounded Context Maps (3 nuevas): Approvals, Compliance, IGA
- [ ] Integration Matrix: Cómo las 8 épicas se comunican
- [ ] ADR Cross-reference completo (todos los ADRs → contextos → historias)
- [ ] Readiness Assessment final: % por épica, MVP + Post-MVP consolidado

---

**Próximo paso:** Proceder con expansión detallada de **EP-06** comenzando mañana.

**Aprobado por:** [Architect]  
**Fecha:** 2026-05-14
