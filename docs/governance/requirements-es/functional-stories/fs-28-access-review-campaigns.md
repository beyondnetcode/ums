# FS-28: Campanas de Revision de Acceso para Recertificacion de Roles y Permisos

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los responsables de seguridad y negocio necesitan campanas recurrentes para confirmar que los usuarios siguen necesitando el acceso que tienen. UMS debe permitir que los administradores revisen roles, perfiles y plantillas activas, registren decisiones de revision y eliminen el acceso que ya no tenga justificacion.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Gobierno de Acceso | Crea y administra las campanas de revision. |
| Propietario del Recurso / Jefe | Revisa el acceso asignado a los usuarios dentro del alcance. |
| Revisor | Confirma, reduce o elimina el acceso durante la campana. |
| Usuario | Se ve afectado por el resultado de la revision y puede recibir una notificacion. |
| Auditor | Revisa el historial de la campana y las decisiones finales. |

## 3. Precondiciones de Negocio

- El tenant tiene asignaciones activas que requieren revision periodica.
- Los revisores y propietarios del recurso estan definidos para el alcance objetivo.
- La organizacion tiene una cadencia de revision o una regla de revision disparada por evento.

## 4. Flujo Funcional Principal

1. El Administrador de Gobierno de Acceso crea una campana de revision para un tenant, sistema, rol o perfil.
2. El sistema prepara la lista de elementos de acceso que deben revisarse.
3. El revisor asignado abre la campana y ve cada elemento con su estado actual.
4. El revisor confirma el acceso, lo reduce, lo elimina o lo escala para una revision adicional.
5. El sistema registra cada decision y aplica el cambio de acceso resultante.
6. Cuando todos los elementos se cierran, la campana se completa y queda auditable.

## 5. Flujos Alternativos y Excepciones

### A. El Revisor No Responde

Si un revisor no completa la campana a tiempo, el sistema escala o cierra la revision segun la politica configurada.

### B. No Se Permite Auto-Revision

Si la politica prohíbe la auto-revision, el usuario no puede aprobar su propio acceso y otro revisor debe completar el item.

### C. El Acceso Ya No Existe

Si el elemento de acceso ya fue eliminado o esta inactivo, el sistema lo omite y mantiene consistente el resultado de la campana.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | El acceso sensible debe recertificarse periodicamente. |
| BR-02 | Los revisores solo pueden actuar sobre items dentro de su alcance asignado. |
| BR-03 | Una decision de revision debe cerrarse con un resultado final. |
| BR-04 | Las decisiones deben ser auditables con revisor, fecha, alcance y motivo. |
| BR-05 | El acceso eliminado no debe seguir apareciendo como activo al cerrar la campana. |
| BR-06 | Las politicas pueden exigir escalamiento o retiro automatico cuando la revision vence. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un administrador puede crear una campana de revision para un alcance definido. |
| 2 | El revisor puede ver los items de acceso que pertenecen a la campana. |
| 3 | El revisor puede aprobar, reducir o eliminar acceso para cada item. |
| 4 | El sistema registra la decision del revisor y el resultado final. |
| 5 | El acceso eliminado durante la campana ya no esta activo al cerrar la campana. |
| 6 | La campana llega a un estado terminal y auditable. |

## 8. Requisitos Tecnicos

- Introducir un modelo de campana de revision que pueda registrar alcance, asignacion de revisores, estado de items y resultado final.
- Persistir cada decision con revisor, fecha, motivo y accion de acceso resultante.
- Soportar reglas de escalamiento a nivel de campana o cierre automatico para revisiones vencidas.
- Emitir eventos auditables para la creacion de campana, la decision del item, el cierre y los cambios de enforcement.
- Mantener un tenant scoping estricto para que un revisor solo vea items dentro del tenant y del alcance de negocio asignado.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `Role`, `Profile`, `PermissionTemplate`, `AccessEnforcementPolicy`, `AuditRecord` |
| Historias Funcionales | FS-16, FS-24 |
| ADRs | ADR-0016, ADR-0033, ADR-0035 |
