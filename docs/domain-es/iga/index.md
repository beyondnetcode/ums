# Contexto IGA (Identity Governance & Administration) — Arquitectura de Agregados

> **Idioma:** [English](../../domain/iga/index.md) | [Español](./index.md)

**Contexto Acotado:** Identity Governance & Administration (`Ums.Domain.IGA`)  
**Raíces de Agregado:** `PromotionRequest`, `RoleMaturityStatus`

---

### Ciclo de Vida de Ascenso de Accesos e Identidad
Gobierna el ascenso seguro de un rol a otro, la nivelación de madurez y los análisis automáticos de riesgo de accesos tóxicos:
- [PromotionRequest](./promotion-request.md) (Raíz de Agregado) — Maneja la creación en borrador, aprobaciones de gerentes, evaluaciones de riesgo, controles de seguridad y ejecuciones verificadas de roles.
- [PromotionImpactAnalysis](./promotion-impact-analysis.md) (Entidad Propia) — Registra puntuaciones dinámicas de riesgo de permisos tóxicos y sistemas afectados.
- [RoleMaturityStatus](./role-maturity-status.md) (Raíz de Agregado) — Administra umbrales de rendimiento, revisiones de cumplimiento, contadores de certificación y criterios de elegibilidad para ascensos según el nivel de madurez del rol (Junior $\rightarrow$ Principal).

---

**[Volver al Índice de Dominio](../index.md)**
