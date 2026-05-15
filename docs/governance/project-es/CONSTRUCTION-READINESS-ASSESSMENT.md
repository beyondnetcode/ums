# 🏗️ Evaluación de Readiness para Construcción (Development)

**Fecha:** 2026-05-14  
**Evaluador:** Arquitectura + Producto  
**Estado Global:** ⚠️ **PARCIALMENTE LISTO** (77% ready)

---

## 📋 Resumen Ejecutivo

| Área | Estado | % Listo | Bloqueador | Acción |
|------|--------|---------|-----------|--------|
| **Épicas & Backlog** | ✅ Completo | 100% | No | Listo para dev |
| **Bounded Contexts** | ✅ Completo | 100% | No | Listo para dev |
| **ER Model** | ✅ Completo | 100% | No | Listo para dev |
| **Functional Stories** | ✅ Completo | 100% | No | Listo para dev |
| **ADR Core (0001-0025)** | ✅ Completo | 100% | No | Listo para dev |
| **ADR UMS-Specific (0026-0047)** | ⚠️ Parcial | 70% | **SÍ** | **Revisar antes de dev** |
| **Technical Enablers** | ⚠️ Parcial | 85% | **SÍ** | **Actualizar** |
| **Service Implementation Plan** | ❌ Falta | 0% | **SÍ** | **Crear** |
| **API Spec (OpenAPI 3.0)** | ❌ Falta | 0% | Recomendado | **Crear para C1** |
| **DB Migration Scripts** | ❌ Falta | 0% | Recomendado | **Crear para C1** |

---

## 🔴 Problemas Críticos Identificados

### 1️⃣ **ADR-0034 & ADR-0037: PostgreSQL sin adaptación a SQL Server**

**Status:** Proposed (no implementado aún)  
**Severidad:** 🔴 CRÍTICA  
**Impacto:** Si se implementan tal cual, fallarán en SQL Server 2022

#### Problemas en ADR-0034 (Hierarchical Multi-Tenancy):

| Línea | PostgreSQL | SQL Server Equivalente | Impacto |
|-------|-----------|----------------------|---------|
| 43, 68 | `gen_random_uuid()` | `NEWID()` | Function no existe en T-SQL |
| 66, 67 | `TIMESTAMPTZ` | `DATETIME2` | Tipo no compatible |
| 65 | `JSONB` | `NVARCHAR(MAX)` JSON | Tipo no compatible |
| 79 | comentario "ancestáor" | error mojibake | Visibilidad (no funcional) |
| 117-147 | `$$ LANGUAGE plpgsql` | Procedimiento T-SQL | Sintaxis incompatible |
| 145 | `EXECUTE FUNCTION` | `EXECUTE PROCEDURE` | Sintaxis distinta |

**Código problemático:**
```sql
-- PostgreSQL (ADR-0034:118)
$$ LANGUAGE plpgsql;

-- SQL Server equivalente necesario:
AS BEGIN ... END;
```

#### Problemas en ADR-0037 (Tenant Partitioning):

| Sección | PostgreSQL | Incompatibilidad SQL Server |
|---------|-----------|---------------------------|
| 2.1 | `PARTITION BY LIST (root_tenant_id)` | SQL Server es `PARTITION BY LIST`, pero sintaxis es distinta |
| 2.2 (línea 61) | `pg_class` (tabla del sistema) | `sys.tables` en SQL Server |
| 2.2 (línea 67) | `CREATE TABLE ... PARTITION OF` | Sintaxis distinta en SQL Server |
| 2.5 (línea 103) | `foreign data wrapper (FDW)` | No existe en SQL Server (usar Linked Servers) |
| 2.6 (línea 117) | `current_setting()` | `SESSION_CONTEXT()` en SQL Server |

**Código problemático:**
```sql
-- PostgreSQL (ADR-0037:117)
USING (root_tenant_id = current_setting('app.root_tenant_id')::uuid);

-- SQL Server equivalente:
USING (root_tenant_id = CAST(SESSION_CONTEXT(N'root_tenant_id') AS uniqueidentifier));
```

**Recomendación:**
- 🚫 NO implementar ADR-0034 y ADR-0037 tal como están
- ✅ Crear ADR-0034v2 y ADR-0037v2 adaptados a SQL Server 2022
- ⚠️ Marcar originals como `Status: Superseded — Requires SQL Server Adaptation`

---

### 2️⃣ **Technical Enablers Incompletos**

**TE-03 ("Enforce Organization RLS - SQL Server")** — Parcialmente correcto

**Estado:** Escrito en 2026-05 pero sin verificación de sintaxis SQL Server completa

**Gaps encontrados:**
- ✅ DbConnectionInterceptor conceptualmente correcto
- ✅ SESSION_CONTEXT setup correcto
- ⚠️ Falta: Manejo de failover (replication scenarios)
- ⚠️ Falta: Limpieza de SESSION_CONTEXT en error
- ⚠️ Falta: Test cases para validar RLS sin app-layer filter

**Recomendación:**
- Expandir TE-03 con:
  ```csharp
  // Error handling
  try {
    await _dbConnection.SetSessionContextAsync(tenantId);
  } catch {
    // Log + clean SESSION_CONTEXT before throwing
  }
  
  // Verification query
  SELECT CAST(SESSION_CONTEXT(N'current_tenant_id') AS uniqueidentifier);
  ```

---

### 3️⃣ **Falta: Service Implementation Plan**

**Status:** ❌ DOCUMENTO FALTANTE  
**Criticidad:** 🔴 BLOQUEADOR para fase de "Architecture → Implementation"

**Qué debe incluir:**
```
Servicio: UMS Core API (.NET 8)
├─ Modules (por Bounded Context)
│  ├─ Identity.Module
│  ├─ Authorization.Module
│  ├─ Configuration.Module
│  ├─ Audit.Module
│  └─ Approvals.Module
├─ Package structure
│  ├─ Domain layer (aggregates, value objects)
│  ├─ Application layer (use cases, commands, queries)
│  ├─ Infrastructure layer (repos, ports, adapters)
│  └─ API layer (controllers, filters)
├─ CI/CD pipeline
├─ Test strategy
└─ Deployment stages
```

---

### 4️⃣ **Falta: OpenAPI 3.0 Specification**

**Status:** ❌ DOCUMENTO FALTANTE  
**Criticidad:** 🟡 RECOMENDADO antes de C1 (Sprint 1)

**Endpoints documentados en BCM but NOT in OpenAPI format:**

Ejemplo de lo que falta:
```yaml
paths:
  /v1/approvals/request:
    post:
      summary: Submit approval request
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ApprovalRequest'
      responses:
        '201':
          description: Approval request created
        '400':
          $ref: '#/components/responses/BadRequest'
        '403':
          $ref: '#/components/responses/Forbidden'
```

---

## 🟡 Problemas Secundarios (No bloqueadores)

### 5️⃣ **Mojibake Characters en algunos documentos (residual)**

**Status:** ⚠️ Parcialmente solucionado  
**Documentos:** 
- ✅ Functional Stories (FS-01 a FS-16): Reparados
- ✅ Technical Enablers (TE-01 a TE-03): Reparados
- ⚠️ ADRs (0034, 0037): "ancestáor" aparece aún (mojibake residual)
- ⚠️ Compliance ER Diagram: "Orquestáación" en glossary (mojibake residual)

**Impacto:** Visual/minor — no afecta desarrollo  
**Acción:** Limpiar antes de hacer PR a main

---

### 6️⃣ **ADR-0010 (Core Multi-Tenancy) — Layer Inversion Resuelta**

**Status:** ✅ CORRECTA después de análisis anterior

Confirmación:
- ✅ Layer 1 (Primary): EF Core `HasQueryFilter` → aplicación controla aislamiento
- ✅ Layer 2 (Failsafe): SQL Server RLS → infraestructura como red de seguridad
- ✅ DbConnectionInterceptor conecta ambas capas

**Listo para implementación.**

---

## ✅ Áreas Listas para Desarrollo

### Épicas MVP (US-001 a US-012): **100% LISTO**

| Épica | ER | BCM | FS | Listo |
|-------|-----|----|----|-------|
| EP-01 (Tenant & Identity) | ✅ | ✅ | FS-01, FS-03, FS-08 | ✅ |
| EP-02 (System Catalog) | ✅ | ✅ | FS-04 | ✅ |
| EP-03 (Authorization) | ✅ | ✅ | FS-02, FS-05, FS-06 | ✅ |
| EP-04 (Configuration) | ✅ | ✅ | FS-13 | ✅ |
| EP-05 (Experience & Diagnostics) | ✅ | ✅ | FS-07, FS-08 | ✅ |

**Estos 5 están listos para sprint 1.**

---

### Post-MVP Épicas (US-013+): **90% LISTO**

| Épica | Estado | Bloqueador |
|-------|--------|-----------|
| EP-06 (B2B & Delegación) | ✅ Arquitectura OK | Approvals Context definido ✅ |
| EP-07 (Cumplimiento) | ✅ Arquitectura OK | Service plan + TE-03 expansión |
| EP-08 (IGA) | ✅ Arquitectura OK | ADR-0046/0047 + Service plan |

**Pueden iniciar una semana después de EP-01-05, pero necesitan:**
- ADR-0034v2, ADR-0037v2 (SQL Server)
- Service Implementation Plan
- TE-03 expanded

---

## 🚀 Plan de Acción Recomendado

### Fase 0: Pre-Construcción (esta semana)

| Tarea | Dueño | Duración | Bloquea |
|-------|-------|----------|--------|
| 1. Crear ADR-0034v2 (SQL Server Hierarchical) | Arq. | 2h | EP-06+ |
| 2. Crear ADR-0037v2 (SQL Server Partitioning) | Arq. | 2h | EP-06+ |
| 3. Expandir TE-03 (RLS Error Handling) | Arq.+Dev | 1.5h | Sprint 1 |
| 4. Crear Service Implementation Plan | Arq. | 3h | Sprint 1 |
| 5. Crear OpenAPI 3.0 spec | Dev Lead | 4h | Recomendado |
| 6. Limpiar mojibake residual en ADRs | Dev | 0.5h | Pulido |

**Total: ~12.5 horas** (1-2 días de trabajo)

### Fase 1: Construcción (Sprint 1 - Semanas 1-2)

**Épicas en Scope:** EP-01, EP-02, EP-03, EP-04, EP-05 (US-001 a US-016)

**Deliverables:**
- [ ] UMS Core API (.NET 8) scaffolding + Identity.Module
- [ ] SQL Server database init scripts
- [ ] EF Core migrations (ums_identity, ums_authz, ums_config)
- [ ] First authentication flow (FS-01: Corporate user login)

### Fase 1.5: Post-MVP Planning (Semana 2)

**Decisión crítica:** ¿Implementar ADR-0034/0037 (Hierarchical + Partitioning)?

**Opciones:**
- A) **Sí, ahora:** Requiere ADR-0034v2/0037v2 + 2 sprints extras
- B) **Post-MVP Phase 2:** Mantener flat model, escalar después
- C) **Híbrido:** Arquitectura lista para hierarchical, pero no implementada hasta Phase 2

**Recomendación:** C (Híbrido) — Arquitectura soporta jerarquía (Tenant.ParentTenantId), pero RLS permanece flat por ahora.

---

## 📊 Matriz de Readiness por Contexto

| Bounded Context | ER Design | API Contracts | Service Plan | Test Plan | Listo |
|-----------------|-----------|---------------|--------------|-----------|-------|
| **Identity** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 85% |
| **Authorization** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 85% |
| **Configuration** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 85% |
| **Audit** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 85% |
| **Console (PAP)** | ✅ | ✅ | ❌ | ❌ | 🟡 70% (React SPA falta) |
| **Approvals** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 80% |
| **Compliance** | ✅ | ✅ | ❌ | ❌ | 🟡 70% (NestJS Sat.) |
| **IGA** | ✅ | ✅ | ❌ | ❌ | 🟡 70% (NestJS Sat.) |
| **Cache** | ✅ | ✅ | ⚠️ Partial | ❌ | 🟡 75% |

---

## 🎯 Recomendación Final

### ✅ **PUEDES INICIAR CONSTRUCCIÓN CON ESTAS CONDICIONES:**

1. **Esta semana (Pre-Construcción):**
   - ✅ Crear ADR-0034v2 y ADR-0037v2 (SQL Server)
   - ✅ Expandir TE-03 con error handling
   - ✅ Crear Service Implementation Plan documento
   - ✅ (Opcional pero recomendado) Crear OpenAPI 3.0 spec

2. **Sprint 1 puede empezar CON:**
   - ✅ EP-01-05 (Épicas MVP Core)
   - ✅ US-001 a US-016

3. **NO EMPEZAR AÚN:**
   - ❌ EP-06 (B2B + Approvals) sin ADR-0034v2 claridad
   - ❌ EP-07 (Compliance) sin Service plan consolidado
   - ❌ EP-08 (IGA) sin decisión de hierarchical tenants

### 📈 **Readiness Progression:**

```
Hoy (2026-05-14):          77% ready
Después de Pre-Construcción: 95% ready → LISTO PARA SPRINT 1
Sprint 1 completo:         Épicas 1-5 en producción
Sprint 2:                  Épicas 6-8 (si ADR-0034v2/0037v2 aprobados)
```

---

## ❓ Preguntas para Equipo

Antes de finalizr Pre-Construcción, necesitas responder:

1. **¿Implementar jerarquía de tenants (ADR-0034/0037) en fase 1 o fase 2?**
   - Impacta duración del proyecto: +2 sprints si es fase 1

2. **¿Usar React SPA para PAP (Console Context) o API-only en fase 1?**
   - Afecta US-015/016 scope

3. **¿NestJS satélites (Compliance, IGA) listos para desarrollo simultáneo a .NET 8 Core?**
   - Requiere coordinación de equipos

4. **¿Necesitas OpenAPI 3.0 spec generado automáticamente o es documentación suficiente?**
   - Recomendado para frontend/testing

---

## 🏁 Conclusión

**Status: ADELANTE CON CONSTRUCCIÓN (con condiciones)**

✅ Arquitectura está sólida y madura  
✅ Épicas MVP están completamente especificadas  
⚠️ 2 ADRs necesitan adaptación SQL Server (pero no son bloqueadores para Sprint 1)  
⚠️ Service Implementation Plan es RECOMENDADO crear esta semana  

**Riesgo de cambio arquitectónico durante desarrollo:** 🟡 BAJO  
**Riesgo de descubrimientos en ER Model:** 🟢 MUY BAJO  
**Riesgo de scope creep:** 🟡 MEDIO (si no se aclaran Épicas 6-8)

