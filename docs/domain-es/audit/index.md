# Contexto de Auditoría (Audit BC) — Arquitectura de Agregados

> **Idioma:** [English](../../domain/audit/index.md) | [Español](./index.md)

**Contexto Acotado:** Auditoría de Seguridad y Cumplimiento (`Ums.Domain.Audit`)  
**Raíces de Agregado:** `AuditRecord`

---

### Auditoría de Actividades del Sistema
Coordina registros cronológicos inmutables e incrementales (append-only) que registran operaciones críticas, cambios de autorización y modificaciones de seguridad multi-inquilino:
- [AuditRecord](./audit-record.md) (Raíz de Agregado) — Repositorios que realizan el seguimiento de la huella de los actores, los datos modificados, las entidades afectadas y el resultado de las acciones. Estrictamente de solo lectura una vez registrado.

---

**[Volver al Índice de Dominio](../index.md)**
