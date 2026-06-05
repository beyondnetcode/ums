# FS-32: Guardrails de Confiabilidad Operativa para Acciones de Gobierno

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los administradores necesitan operaciones de gobierno predecibles incluso cuando las solicitudes se repiten, concurren o se reintentan. UMS debe evitar estados duplicados, prevenir errores entre tenants y mantener visibles los fallos para que la administracion de accesos siga siendo confiable.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Plataforma | Realiza acciones de gobierno de tenant y sistema. |
| Administrador de Tenant | Realiza administracion con alcance limitado al tenant. |
| Ingeniero de Soporte | Investiga acciones fallidas o duplicadas. |
| Auditor | Revisa si el estado final es confiable y completo. |

## 3. Precondiciones de Negocio

- El actor esta autenticado con el alcance correcto de tenant o plataforma.
- El elemento objetivo existe y es elegible para la accion solicitada.
- El contexto de tenant esta disponible antes de enviar la operacion.

## 4. Flujo Funcional Principal

1. El actor envia una accion de gobierno como crear, actualizar, aprobar o revocar.
2. El sistema verifica si la misma accion ya fue enviada.
3. El sistema verifica si una edicion concurrente sobrescribiria un cambio mas reciente.
4. Si la accion es valida, el sistema la completa una sola vez y registra el resultado.
5. Si ocurre un fallo durante la entrega o el reintento, el sistema mantiene visible el resultado en lugar de perderlo en silencio.
6. El actor ve un resultado claro de exito, conflicto o reintento.

## 5. Flujos Alternativos y Excepciones

### A. Envio Duplicado

Si la misma accion se envia otra vez, el sistema reutiliza el resultado previo o rechaza el duplicado en lugar de crear estado duplicado.

### B. Conflicto por Actualizacion Concurrente

Si otro actor cambio el mismo registro primero, el sistema pide una lectura fresca en lugar de sobrescribir el valor mas reciente.

### C. Falta Contexto de Tenant

Si la solicitud no trae un contexto de tenant valido, la accion se rechaza y no se aplica globalmente.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Los envios repetidos no deben crear estado de gobierno duplicado. |
| BR-02 | Las actualizaciones concurrentes no deben sobrescribir silenciosamente cambios mas nuevos. |
| BR-03 | El alcance del tenant siempre debe conocerse antes de aceptar un cambio. |
| BR-04 | Los reintentos de entrega no deben perder el resultado de negocio ni la auditoria. |
| BR-05 | Los fallos operativos deben ser visibles para soporte y auditoria. |
| BR-06 | Los datos entre tenants nunca deben mezclarse por error. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Repetir la misma accion no crea estado de negocio duplicado. |
| 2 | Un cambio concurrente se detecta en lugar de sobrescribirse en silencio. |
| 3 | Una solicitud sin contexto de tenant se rechaza. |
| 4 | Las entregas fallidas siguen visibles hasta resolverse. |
| 5 | Los usuarios de soporte y auditoria pueden explicar el resultado final de la accion. |
| 6 | El estado final permanece consistente despues de los reintentos. |

## 8. Requisitos Tecnicos

- Introducir deduplicacion de solicitudes para los flujos de comando soportados.
- Agregar verificacion de concurrencia optimista para registros mutables que pueden editarse al mismo tiempo.
- Aplicar tenant scoping antes de que la operacion entre en el flujo de aplicacion.
- Mantener observable la entrega de outbox o eventos para que operaciones vea cuando un cambio sigue pendiente.
- Exponer feedback accionable de conflicto y reintento en lugar de fallos genericos.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `Tenant`, `UserAccount`, `Profile`, `ApprovalRequest`, `AppConfiguration`, `AuditRecord` |
| Historias Funcionales | FS-03, FS-05, FS-13, FS-24 |
| ADRs | ADR-0010, ADR-0033, ADR-0063, ADR-0066 |
