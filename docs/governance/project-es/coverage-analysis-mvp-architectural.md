# 📊 Análisis de Cobertura: MVP Backlog vs Arquitectura

**Documento:** Validación de alineamiento entre Épicas, Bounded Contexts, ER Model, y Functional Stories  
**Fecha:** 2026-05-14  
**Estado:** Revisión Técnica  

---

## 🎯 Propósito

Este documento verifica que **cada Épica en el MVP Backlog** está completamente cubierta por:
1. Un **Bounded Context** que la gobierna
2. **Entidades en el ER Model** que la soportan
3. **Functional Stories** que la especifican
4. Un **Service Owner** (UMS Core API .NET 8 o Satellite NestJS) asignado

---

## 📋 1. Matriz de Cobertura: Épica → Contexto DDD → ER Model

| Épica | Misión | Bounded Context(s) | Entidades Clave | Schema SQL | Service Owner | MVP Status |
|-------|--------|-------------------|-----------------|------------|---------------|-----------|
| **EP-01** | Tenant e Identidad | Identity | TENANT, BRANCH, USER_ACCOUNT, ORGANIZATION | `[ums_identity]` | UMS Core API (.NET 8) | ✅ Core |
| **EP-02** | Catálogo de Sistemas | Authorization | SYSTEM_SUITE, FUNCTIONAL_MODULE, FUNCTIONAL_SUBMODULE, FUNCTIONAL_OPTION, ACTION | `[ums_authz]` | UMS Core API (.NET 8) | ✅ Core |
| **EP-03** | Autorización & Perfiles | Authorization | ROLE, PROFILE, PERMISSION_TEMPLATE, PROFILE_PERMISSION | `[ums_authz]` | UMS Core API (.NET 8) | ✅ Core |
| **EP-04** | Configuración Jerárquica | Configuration | APP_CONFIGURATION | `[ums_config]` | UMS Core API (.NET 8) | ✅ Core |
| **EP-05** | Diagnóstico & UX | Console, Authorization | Reusa: PROFILE, PERMISSION_TEMPLATE, ACTION | `[ums_authz]` + SPA | UMS Core API + React | ✅ Estab. |
| **EP-06** | Seguridad & B2B & Delegación | Identity, Approvals | USER_MANAGEMENT_DELEGATION, APPROVAL_WORKFLOW, APPROVAL_REQUEST | `[ums_identity]`, `[ums_approval]` | UMS Core API (.NET 8) | 🔄 Post-MVP |
| **EP-07** | Cumplimiento & Ciclo Vida | Compliance | USER_DOCUMENT, DOCUMENT_TYPE, NOTIFICATION_RULE, ACCESS_ENFORCEMENT_POLICY | `[ums_compliance]` | Compliance Satellite (NestJS) | 🔄 Post-MVP |
| **EP-08** | IGA & Promoción de Roles | IGA | ROLE_PROMOTION_CRITERIA, USER_PROMOTION_PROCESS | `[ums_iga]` | IGA Satellite (NestJS) | 🔄 Post-MVP |

---

## 📐 2. Matriz: Functional Stories → Épicas → Historias de Usuario

| FS | Historia Funcional | Épica(s) | Historias Usuario | Contexto(s) DDD | Entidades ER | Fase |
|----|-------------------|---------|------------------|-----------------|-------------|------|
| FS-01 | Autenticación corporativa | EP-01 | US-005, US-006 | Identity | USER_ACCOUNT, TENANT | Core |
| FS-02 | Crear plantillas autorización | EP-03 | US-007, US-008 | Authorization | PERMISSION_TEMPLATE, PROFILE | Core |
| FS-03 | Registro de tenant + IdP | EP-01 | US-001, US-002 | Identity, Config | TENANT, APP_CONFIGURATION | Core |
| FS-04 | Topología funcional (M→M→O→A) | EP-02 | US-003, US-004 | Authorization | SYSTEM_SUITE, FUNCTIONAL_MODULE, FUNCTIONAL_SUBMODULE, FUNCTIONAL_OPTION, ACTION | Core |
| FS-05 | Crear perfil + asignación manual | EP-03 | US-009, US-010 | Authorization | PROFILE, PROFILE_PERMISSION, PERMISSION_TEMPLATE | Core |
| FS-06 | Autoasignación de plantillas | EP-03 | US-029, US-030 | Authorization | PERMISSION_TEMPLATE, PROFILE, PROFILE_PERMISSION | Posterior |
| FS-07 | Diagnóstico por grafo | EP-05 | US-013, US-014 | Authorization, Console | PROFILE, PERMISSION_TEMPLATE, ACTION | Estab. |
| FS-08 | Login hospedado & branding | EP-05 | US-015, US-016 | Identity, Console | TENANT, BRANCH | Estab. |
| FS-09 | MFA adaptativo & passwordless | EP-06 | US-017, US-018 | Identity, Config | USER_ACCOUNT, APP_CONFIGURATION | Seg. Post-MVP |
| FS-10 | Solicitud & aprobación B2B | EP-06 | US-019, US-020 | Identity, Approvals | USER_ACCOUNT, APPROVAL_REQUEST, APPROVAL_WORKFLOW | Exp. Post-MVP |
| FS-11 | Carga de documentos | EP-07 | US-023, US-024 | Compliance | USER_DOCUMENT, DOCUMENT_TYPE | Cum. Post-MVP |
| FS-12 | Promoción de roles IGA | EP-08 | US-031, US-032 | IGA | USER_PROMOTION_PROCESS, ROLE_PROMOTION_CRITERIA | IGA Post-MVP |
| FS-13 | Configuración jerárquica | EP-04 | US-011, US-012 | Configuration | APP_CONFIGURATION | Core |
| FS-14 | Administración delegada | EP-06 | US-021, US-022 | Identity | USER_MANAGEMENT_DELEGATION | Gov. Post-MVP |
| FS-15 | Reglas de notificación | EP-07 | US-025, US-026 | Compliance | NOTIFICATION_RULE, USER_DOCUMENT | Cum. Post-MVP |
| FS-16 | Políticas de enforcement | EP-07 | US-027, US-028 | Compliance | ACCESS_ENFORCEMENT_POLICY, USER_DOCUMENT | Cum. Post-MVP |

---

## ✅ 3. Validación de Cobertura por Épica

### ✅ EP-01: Base de Tenant e Identidad (MVP CORE)
- **Bounded Context Cobertor:** Identity Context
  - ✅ User Registration & Lifecycle
  - ✅ Organization (Tenant) Management
  - ✅ Branch (Sedes) Registry
  - ✅ IdP Strategy Adapters
- **ER Model:** ✅ TENANT, BRANCH, USER_ACCOUNT, ORGANIZATION
- **Functional Stories:** FS-01, FS-03, FS-08
- **Historias Usuario:** US-001, US-002, US-005, US-006
- **Service Owner:** UMS Core API (.NET 8) con DbConnectionInterceptor para SESSION_CONTEXT
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-02: Catálogo de Sistemas Gobernados (MVP CORE)
- **Bounded Context Cobertor:** Authorization Context
  - ✅ System Registry
  - ✅ Module / Menu / Option / Action Topology
- **ER Model:** ✅ SYSTEM_SUITE, FUNCTIONAL_MODULE, FUNCTIONAL_SUBMODULE, FUNCTIONAL_OPTION, ACTION
- **Functional Stories:** FS-04
- **Historias Usuario:** US-003, US-004
- **Service Owner:** UMS Core API (.NET 8)
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-03: Diseño y Asignación de Autorización (MVP CORE)
- **Bounded Context Cobertor:** Authorization Context
  - ✅ Profile & Template Engine
  - ✅ Permission Graph Compiler (PDP)
  - ✅ Explicit-Deny Precedence Resolver
- **ER Model:** ✅ ROLE, PROFILE, PERMISSION_TEMPLATE, PROFILE_PERMISSION
- **Functional Stories:** FS-02, FS-05, FS-06
- **Historias Usuario:** US-007, US-008, US-009, US-010, US-029, US-030
- **Service Owner:** UMS Core API (.NET 8)
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-04: Base de Configuración (MVP CORE)
- **Bounded Context Cobertor:** Configuration & Feature Management Context
  - ✅ System Behavioral Configuration Model
  - ✅ Config Cache Manager
- **ER Model:** ✅ APP_CONFIGURATION (schema: [ums_config])
- **Functional Stories:** FS-13
- **Historias Usuario:** US-011, US-012
- **Service Owner:** UMS Core API (.NET 8)
- **Contracts:** GET /v1/config/system/{system_id}?tenant_id → returns active system config
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-05: Experiencia y Diagnóstico de Acceso (ESTABILIZACIÓN MVP)
- **Bounded Contexts Cotitulares:** 
  - ✅ Console Context (PAP Web Portal)
  - ✅ Authorization Context (diagnosis engine)
- **ER Model:** Reutiliza PROFILE, PERMISSION_TEMPLATE, ACTION (sin nuevas entidades)
- **Functional Stories:** FS-07, FS-08
- **Historias Usuario:** US-013, US-014, US-015, US-016
- **Service Owners:** 
  - Backend: UMS Core API (.NET 8) 
  - Frontend: React SPA en Console Context
- **Contracts:** GET /v1/authorization/graph → returns HierarchicalJsonGraph (for diagnosis)
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-06: Seguridad, Acceso Externo y Delegación (POST-MVP)
- **Bounded Contexts Cotitulares:** 
  - ✅ Identity Context (user provisioning)
  - ✅ Approvals Context (nueva entidad: APPROVAL_WORKFLOW, APPROVAL_REQUEST)
- **ER Model:** ✅ USER_MANAGEMENT_DELEGATION, APPROVAL_WORKFLOW, APPROVAL_REQUEST, APPROVAL_LOG
- **Functional Stories:** FS-09, FS-10, FS-14
- **Historias Usuario:** US-017, US-018, US-019, US-020, US-021, US-022
- **Service Owner:** UMS Core API (.NET 8)
- **Nota Arquitectónica:** Falta definir un Bounded Context explícito "Approvals" en el BCM. Ver Gap #1 abajo.
- **Status:** ⚠️ **COBERTURA CON GAP ARQUITECTÓNICO**

### ✅ EP-07: Ciclo de Vida de Cumplimiento (POST-MVP)
- **Bounded Context Cobertor:** Compliance Context (DEFINIDO)
  - ✅ User Document Lifecycle
  - ✅ Access Enforcement Policy Engine
  - ✅ Pre-Expiration Notification Rules
  - ✅ Notification Dispatcher
- **ER Model:** ✅ USER_DOCUMENT, DOCUMENT_TYPE, NOTIFICATION_RULE, ACCESS_ENFORCEMENT_POLICY
- **Functional Stories:** FS-11, FS-15, FS-16
- **Historias Usuario:** US-023, US-024, US-025, US-026, US-027, US-028
- **Service Owner:** Compliance Satellite (NestJS)
- **Status:** ✅ **COBERTURA COMPLETA**

### ✅ EP-08: Automatización IGA Avanzada (POSTERIOR)
- **Bounded Context Cobertor:** IGA Context (Identity Governance & Administration)
  - ✅ Role Promotion Criteria Engine
  - ✅ User Promotion Process Manager
  - ✅ Delegated Administration Registry
- **ER Model:** ✅ ROLE_PROMOTION_CRITERIA, USER_PROMOTION_PROCESS
- **Functional Stories:** FS-12
- **Historias Usuario:** US-031, US-032
- **Service Owner:** IGA Satellite (NestJS)
- **Status:** ✅ **COBERTURA COMPLETA**

---

## 🚨 4. Gaps Identificados

### Gap #1: Approvals Context NO está definido en el Bounded Context Map
**Severidad:** 🔴 MEDIA  
**Ubicación:** `bounded-context-map.md`  
**Descripción:**
- Las Épicas EP-06 (B2B) y EP-07 (Cumplimiento) requieren flujos de aprobación (APPROVAL_WORKFLOW, APPROVAL_REQUEST).
- El ER Model define correctamente las entidades en schema `[ums_approval]`.
- El Service-Entity Map mapea correctamente a UMS Core API (.NET 8).
- **Pero** el Bounded Context Map NO incluye un contexto "Approvals".

**Impacto:** 
- Unclear responsibility & ownership between Identity, Compliance, and the approval orchestrator.
- Contex relationship lines missing from BCM Mermaid diagram.

**Acción Requerida:**
- [ ] Agregar Bounded Context "Approvals" al BCM con misión: "Govern approval workflows for B2B access, document validation, and role promotions"
- [ ] Mapear relaciones: Approvals ← Customer-Supplier desde Identity, Compliance, IGA
- [ ] Incluir entidades: APPROVAL_WORKFLOW, APPROVAL_REQUIRED_DOCUMENT, APPROVAL_REQUEST, APPROVAL_LOG

---

### Gap #2: ER Model está incompleto — Falta ORGANIZATION aggregate en Identity
**Severidad:** 🟡 BAJA  
**Ubicación:** `database-design-er.md`  
**Descripción:**
- El Bounded Context Map referencia "Organization (Tenant) aggregate" bajo Identity Context.
- El ER Model defines TENANT y BRANCH, pero **no muestra ORGANIZATION como entidad separada**.
- Preguntas: ¿Es ORGANIZATION = TENANT (alias)? ¿O es un parent para multi-tenant orgs?

**Impacto:**
- Ambigüedad en el diseño, especialmente para "B2B External Organization" (FS-10).
- Developer confusion al implementar provisioning externo.

**Acción Requerida:**
- [ ] Clarificar en glossary: ¿TENANT vs ORGANIZATION vs BRANCH vs Client?
- [ ] Si ORGANIZATION es un agregado separado, dibujarlo en el ER Model con relaciones a TENANT.
- [ ] Verificar con el User Story US-001 ("Registrar una organización/tenant") qué semántica se espera.

---

### Gap #3: Configuration Context contracts no menciona App Configuration routes
**Severidad:** 🟡 BAJA  
**Ubicación:** `bounded-context-map.md` Section E  
**Descripción:**
- El Configuration Context está bien definido para IdP, System Config, y Feature Flags.
- **Pero** App Configuration (APP_CONFIGURATION entity) no está documentado en los contracts.
- El ER Model sabe que APP_CONFIGURATION → [ums_config] schema, pero no hay endpoint specification.

**Impacto:**
- Unclear contract for FS-13 (Configuración Jerárquica).
- No hay route definition para PUT/GET /v1/config/app/{code}.

**Acción Requerida:**
- [ ] Agregar al Configuration Context contracts:
  ```
  - GET /v1/config/app?tenant_id&code → returns app configuration value (with inheritance)
  - POST /v1/config/app → creates app config (versioned, tenant/system-scoped)
  - GET /v1/config/app/hierarchy → explains inheritance chain
  ```

---

### Gap #4: Audit Context integration contracts incomplete for all event types
**Severidad:** 🟡 BAJA  
**Ubicación:** `bounded-context-map.md` Section D  
**Descripción:**
- El Audit Context lists events from Identity, Authorization, and Configuration.
- **Falta:** Events from Compliance, IGA, y Approvals.
- El ER Model no incluye la entidad AUDIT_RECORD (implícita pero no dibujada).

**Impacto:**
- Auditors cannot trace Approval decisions, Document validations, Role promotions.
- Regulatory compliance gaps for EP-06, EP-07, EP-08.

**Acción Requerida:**
- [ ] Expand Audit Context integration: Add published events from Compliance, IGA, Approvals
  ```
  - ComplianceContext → DocumentExpiredEvent, DocumentValidatedEvent, NotificationSentEvent
  - IGAContext → PromotionCriteriaMetEvent, PromotionApprovedEvent
  - ApprovalsContext → ApprovalRequestCreatedEvent, ApprovalRequestResolvedEvent
  ```
- [ ] Add AUDIT_RECORD entity to ER Model with fields: audit_id, aggregate_root, event_type, user_id, timestamp, details
- [ ] Diagram audit ledger schema.

---

## 🎯 5. Recomendaciones Prioritarias

### 🔴 Crítico — Implementar antes de Coding Sprint
1. **Definir Approvals Bounded Context** (Gap #1)
   - Service owner: UMS Core API (.NET 8)
   - Schema: [ums_approval]
   - Contracts: POST /v1/approvals/request, PATCH /v1/approvals/{requestId}
   - Events: ApprovalRequestCreatedEvent, ApprovalRequestResolvedEvent
   - Impacto: Sin esto, B2B flow (FS-10) y Document validation (FS-11) no tienen dueño claro.

2. **Clarify Organization vs Tenant semantics** (Gap #2)
   - Impact: FS-10 "External B2B Organization" depende de esto.
   - Add to glossary with Entity-Relationship diagram.

### 🟡 Importante — Completar antes del Lanzamiento MVP
3. **Document App Configuration routes** (Gap #3)
   - Impacto: FS-13 (Configuración Jerárquica) necesita especificación de API.

4. **Expand Audit Context to cover all 8 domains** (Gap #4)
   - Impacto: Compliance, IGA, Approvals events deben ser auditados.
   - Add event types to Audit Context definition.

---

## 📊 6. Tabla Resumen: Alineamiento Completo

| Épica | MVP Status | Bounded Contexts | ER Entities | Functional Stories | Historias | Service Owner | ¿Completo? |
|-------|-----------|-----------------|-------------|-------------------|-----------|---------------|-----------|
| EP-01 | Core | Identity | TENANT, BRANCH, USER_ACCOUNT, ORG | FS-01, FS-03, FS-08 | 4 | UMS Core API | ✅ Sí |
| EP-02 | Core | Authorization | SYSTEM_SUITE, FMODULE, FSUBMODULE, FOPTION, ACTION | FS-04 | 2 | UMS Core API | ✅ Sí |
| EP-03 | Core | Authorization | ROLE, PROFILE, PERMISSION_TEMPLATE, PROFILE_PERMISSION | FS-02, FS-05, FS-06 | 6 | UMS Core API | ✅ Sí |
| EP-04 | Core | Configuration | APP_CONFIGURATION | FS-13 | 2 | UMS Core API | ✅ Sí |
| EP-05 | Estab. | Console, Authorization | Reutiliza EP-02, EP-03 | FS-07, FS-08 | 4 | UMS Core API + React | ✅ Sí |
| EP-06 | Post-MVP | Identity, **Approvals** ⚠️ | USER_MGT_DELEGATION, **APPROVAL_*** ⚠️ | FS-09, FS-10, FS-14 | 6 | UMS Core API | ⚠️ Gap #1 |
| EP-07 | Post-MVP | Compliance | USER_DOCUMENT, NOTIFICATION_RULE, ACCESS_POLICY | FS-11, FS-15, FS-16 | 6 | Compliance Sat. | ✅ Sí |
| EP-08 | Post-MVP | IGA | ROLE_PROMOTION_CRITERIA, USER_PROMOTION_PROCESS | FS-12 | 2 | IGA Satellite | ✅ Sí |

---

## 📝 7. Conclusión

✅ **87.5% de cobertura arquitectónica** (7 de 8 épicas tienen cobertura completa)

⚠️ **1 Gap crítico (EP-06 Approvals Context)** — Impacta B2B y documento workflows  
⚠️ **3 Gaps secundarios (Organization semantics, App Config routes, Audit coverage)**

### Recomendación: 
**Antes de iniciar development sprints:**
1. ✅ Aceptar cobertura de EP-01 a EP-05 para MVP Core
2. ⚠️ Resolver Gap #1 (Approvals Context) y Gap #2 (Organization semantics) para EP-06
3. ⚠️ Documentar App Configuration routes (Gap #3) y Audit Context expansion (Gap #4)

**Siguiente acción:** Refinar bounded-context-map.md y database-design-er.md con los 4 gaps y re-validar con arquitecto principal.

