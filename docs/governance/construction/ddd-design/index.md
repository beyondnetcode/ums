# DDD Domain Layer Design

**Tipo:** Portal de Diseno DDD  
**Version:** 2.0 | **Fecha:** 2026-05-15 | **Estado:** Propuesto  
**Alcance:** Producto completo — FS-01 a FS-16  
**Owner:** Arquitecto Principal

> **Visualizacion interactiva:** [interactive-ddd-viewer.html](./interactive-ddd-viewer.html)  
> Bounded Context Map · 5 Maquinas de Estado · 3 Flujos Cross-Contexto

---

## Documentos

### Arquitectura de Contextos

| # | Documento | Contenido |
|---|-----------|-----------|
| 01 | [Bounded Context Map](./01-bounded-context-map.md) | 9 contextos · relaciones Customer-Supplier / Conformist · ACL ports |
| 02 | [Lenguaje Ubicuo](./02-ubiquitous-language.md) | 25 terminos canonicos · estados por agregado |

### Modelo de Dominio por Contexto

| # | Documento | Contexto | Agregados | FS |
|---|-----------|----------|-----------|----|
| 03 | [Identity Context](./03-identity-context.md) | BC-A `ums_identity` | Tenant, UserAccount, Branch | FS-01, FS-03, FS-08, FS-09 |
| 04 | [Authorization Context](./04-authorization-context.md) | BC-B `ums_authz` | SystemSuite, Role, PermissionTemplate, TemplateAssignmentRule, Profile | FS-02, FS-04, FS-05, FS-06, FS-07 |
| 05 | [Configuration Context](./05-configuration-context.md) | BC-C `ums_config` | IdpConfiguration, AppConfiguration, FeatureFlag | FS-08, FS-09, FS-13 |
| 06 | [Audit Context](./06-audit-context.md) | BC-D `ums_audit` | AuditRecord | Todos |
| 07 | [Approvals Context](./07-approvals-context.md) | BC-F `ums_approval` | ApprovalWorkflow, ApprovalRequest | FS-10, FS-11, FS-12 |
| 08 | [IGA Context](./08-iga-context.md) | BC-H `ums_iga` | RolePromotionCriteria, UserPromotionProcess, UserManagementDelegation | FS-12, FS-14 |
| 09 | [Compliance Context](./09-compliance-context.md) | BC-I `ums_compliance` | DocumentType, UserDocument | FS-11, FS-15, FS-16 |

### Artefactos Transversales

| # | Documento | Contenido |
|---|-----------|-----------|
| 10 | [Cross-Context Flows](./10-cross-context-flows.md) | B2B Onboarding · Document Expiration · Role Promotion · Tabla de enrutamiento |
| 11 | [DDD Primitives](./11-ddd-primitives.md) | Entity / AggregateRoot / ValueObject / Result&lt;T&gt; · Estructura monorepo |
| 12 | [Design Decisions & Gaps](./12-design-decisions.md) | 8 decisiones justificadas · 6 vacios de validacion (V1-V6) |

---

## Resumen del Modelo

| Metrica | Valor |
|---------|-------|
| Bounded Contexts | 9 (7 con entidades propias + Cache + Console) |
| Aggregate Roots | 19 |
| Value Objects | 60+ |
| Invariantes totales | 55+ |
| Comandos totales | 60+ |
| Eventos de Dominio | 55+ |
| Maquinas de Estado | 5 (UserAccount, PermissionTemplate, ApprovalRequest, UserPromotionProcess, UserDocument) |
| Flujos cross-contexto | 3 diagramas de secuencia |
| Historias funcionales cubiertas | 16 (FS-01 a FS-16) |

---

## Fuentes

| Documento fuente | Uso |
|-----------------|-----|
| `bounded-context-map.md` | Contratos publicados, patrones de integracion, ACL |
| `database-design-er.md` | Entidades canonicas, arco exclusivo, reglas 1-9 |
| `glossary.md` | Lenguaje ubicuo, maquinas de estado, enumeraciones |
| `conceptual-data-model.md` | Atributos, tipos y nullabilidad |
| ADR-0029 al 0049 | Invariantes, decisiones de diseno, restricciones tecnicas |
| FS-01 a FS-16 | Criterios de aceptacion, reglas de negocio por historia |

---

**[Volver a Construction](../index.md)** | **[Volver a Governance](../../index.md)**
