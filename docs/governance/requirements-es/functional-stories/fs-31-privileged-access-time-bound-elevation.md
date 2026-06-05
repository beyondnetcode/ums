# FS-31: Acceso Privilegiado con Elevacion Limitada en el Tiempo

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

El acceso privilegiado debe otorgarse solo durante el tiempo necesario y luego retirarse automaticamente. UMS debe permitir que los equipos de seguridad aprueben una elevacion temporal, limiten su duracion y conserven una trazabilidad clara de la justificacion.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Solicitante de Acceso Privilegiado | Solicita acceso elevado temporal. |
| Aprobador de Seguridad | Revisa y aprueba o deniega la elevacion. |
| Administrador de Seguridad | Configura la politica de acceso privilegiado. |
| Auditor | Revisa el motivo, la duracion y el resultado de la elevacion. |

## 3. Precondiciones de Negocio

- El usuario ya existe en UMS y tiene una identidad base.
- El rol privilegiado o el alcance elevado ya esta definido.
- La organizacion tiene una politica de aprobacion para elevacion temporal.

## 4. Flujo Funcional Principal

1. El solicitante pide acceso privilegiado y explica por que lo necesita.
2. El aprobador revisa la solicitud, el alcance objetivo y la duracion pedida.
3. Si se aprueba, el sistema concede la elevacion solo por la ventana de tiempo permitida.
4. El sistema elimina la elevacion automaticamente cuando termina la ventana.
5. La decision y la duracion siguen siendo auditables aun despues de retirar el acceso.

## 5. Flujos Alternativos y Excepciones

### A. Falta Justificacion

Si el solicitante no proporciona una razon suficiente, la solicitud no puede aprobarse.

### B. Duracion Demasiado Larga

Si la duracion solicitada excede la politica, el aprobador debe reducirla o denegar la solicitud.

### C. Alcance de Alto Riesgo

Si el alcance solicitado es especialmente sensible, el sistema puede requerir una aprobacion mas fuerte antes de otorgarlo.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | El acceso privilegiado debe ser temporal y estar aprobado de forma explicita. |
| BR-02 | Cada elevacion debe tener hora de inicio, hora de fin y motivo. |
| BR-03 | El acceso debe eliminarse automaticamente cuando la elevacion vence. |
| BR-04 | Los alcances de mayor riesgo pueden requerir aprobacion adicional o limites mas estrictos. |
| BR-05 | Cada decision debe ser auditable y explicable. |
| BR-06 | Una elevacion privilegiada no debe volverse permanente por accidente. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un solicitante puede pedir acceso privilegiado temporal con motivo y duracion. |
| 2 | Un aprobador puede aprobar o denegar la solicitud. |
| 3 | El acceso aprobado expira automaticamente al final de la ventana aprobada. |
| 4 | El sistema registra la justificacion, el revisor, el alcance y la duracion. |
| 5 | El acceso privilegiado vencido deja de estar activo. |
| 6 | Las solicitudes de alto riesgo pueden requerir revision mas estricta antes de aprobarse. |

## 8. Requisitos Tecnicos

- Introducir un modelo de elevacion con limite de tiempo que pueda guardar solicitante, aprobador, alcance, inicio, fin y estado.
- Integrar las decisiones de elevacion con el flujo de aprobacion y la politica de enforcement de acceso.
- Emitir eventos auditables para otorgamiento, vencimiento y retiro del acceso elevado.
- Soportar limites de duracion y umbrales de aprobacion definidos por politica.
- Mantener el modelo de acceso privilegiado separado de la asignacion permanente de roles.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `ApprovalWorkflow`, `ApprovalRequest`, `AccessEnforcementPolicy`, `Role`, `Profile` |
| Historias Funcionales | FS-10, FS-16, FS-24 |
| ADRs | ADR-0012, ADR-0016, ADR-0035 |
