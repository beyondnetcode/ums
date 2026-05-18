# Decisiones de Diseno y Vacios de Validacion

**Tipo:** DDD — Design Decisions & Validation Gaps  
**Version:** 2.0 | **Fecha:** 2026-05-15

---

## Decisiones de Diseno Justificadas

| Decision | Justificacion |
|----------|---------------|
| `Branch` como Entidad dentro de `Tenant` | Para garantizar límites transaccionales robustos y que las ramas dependan enteramente de la raíz organizativa del Tenant |
| `PermissionTemplate` AR separado de `Profile` | Un template puede reutilizarse por multiples Profiles; ciclo de vida independiente (ADR-0042) |
| `Role` AR separado de `PermissionTemplate` | Role pertenece a Authorization; criterios de promocion pertenecen a IGA; no pueden cohabitar (ADR-0046) |
| `DocumentType` AR con `NotificationRule` y `AccessEnforcementPolicy` embebidas | Las reglas son configuracion del catalogo, no ciclo de vida independiente; siempre se leen juntas |
| `UserDocument` AR separado de `DocumentType` | DocumentType es catalogo (admin); UserDocument es instancia de cumplimiento (usuario/revisor) |
| `TenantClosure` excluida del dominio | Es proyeccion de infraestructura mantenida por trigger SQL Server; el dominio la consume como servicio de repositorio |
| Eventos de dominio con ID como referencia (no navigation properties) | Desacoplamiento entre contextos; los agregados solo conocen IDs de otros contextos |
| `TemplateAssignmentRule` como AR | Tiene su propio ciclo de vida, prioridad y estado; no es parte de Profile ni de Template |

---

## Vacios de Validacion Pendientes

Estos puntos requieren resolucion en workshop tecnico antes del Sprint 1.

| ID | Pregunta | Impacto | Responsable |
|----|---------|---------|-------------|
| V1 | `TemplateAssignmentRule` no existe en `database-design-er.md`. Se necesita agregar la entidad al modelo fisico para implementar FS-06 | Alto — bloquea el sprint que incluya FS-06 | Arquitecto + DBA |
| V2 | `MfaEnrollment` no aparece en el E/R actual. Se necesita tabla para persistir metodos MFA enrolados por usuario (FS-09) | Resuelto | Implementado en el código dentro de UserAccount como Entidad hija |
| V3 | `PROFILE_INTERNAL_ONLY` flag no esta en el modelo actual. Sin el es imposible aplicar INV-P8 de FS-10 (externos no reciben profiles internos) | Alto — riesgo de seguridad si no se implementa | Product Owner + Arquitecto |
| V4 | La herencia de politicas de ADR-0035 (MANDATORY/DEFAULT/OPT_IN/NONE) no tiene entidades `Policy` / `PolicyBinding` en el E/R. El modelo actual solo cubre permisos a nivel de template/profile. ¿Se incluye en el producto completo o es una evolucion futura? | Medio — afecta complejidad de Authorization Context | Arquitecto Principal |
| V5 | `ApprovalWorkflow` no especifica si tiene stages multi-paso (stage 1: manager, stage 2: CISO). El modelo actual asume un solo aprobador. ¿Es multi-stage? | Medio — cambia estructura de `APPROVAL_LOG` y routing | Product Owner |
| V6 | El modelo de autenticacion M2M para `SERVICE_ACCOUNT` (client_credentials, API key) no esta documentado en ninguna FS. Solo esta mencionado en el glosario. ¿Como se autentica y que flujo sigue? | Medio — afecta `IAuthenticationPort` y `UserAccount` | Arquitecto de Seguridad |

---

## Proximos Pasos

Workshop tecnico de 3 horas con el equipo de ingenieria para resolver V1-V6 y aprobar el diseno antes del Sprint 1.

Prioridad de resolucion:
1. **V1, V2, V3** — bloquean implementacion directa; alta severidad
2. **V4** — clarificar alcance del producto vs. evolucion futura
3. **V5** — impacta schema de base de datos; resolver antes del DDL final
4. **V6** — necesario para SERVICE_ACCOUNT en scope si lo hay

---

**[Anterior: DDD Primitives](./11-ddd-primitives.md)** | **[Indice DDD](./index.md)**
