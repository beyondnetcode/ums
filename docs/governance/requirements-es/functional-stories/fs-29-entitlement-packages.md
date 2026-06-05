# FS-29: Paquetes de Entitlements para Bloques de Acceso Gobernados

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los responsables de negocio necesitan bloques de acceso reutilizables que agrupen roles, permisos y acceso a aplicaciones en un solo paquete gobernado. UMS debe permitir que los administradores definan, soliciten, aprueben y revoquen esos paquetes sin gestionar cada entitlement por separado.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Entitlements | Disena y mantiene los paquetes de acceso. |
| Solicitante | Solicita un paquete en lugar de solicitar entitlements individuales. |
| Aprobador | Decide si el paquete puede ser otorgado. |
| Auditor | Verifica quien recibio que paquete y por que. |

## 3. Precondiciones de Negocio

- El catalogo de sistemas y el catalogo de autorizacion ya estan definidos.
- El propietario del paquete sabe que roles, permisos o alcances pertenecen juntos.
- La ruta de aprobacion esta configurada para el tenant o sistema objetivo.

## 4. Flujo Funcional Principal

1. El Administrador de Entitlements crea un paquete con un nombre de negocio y un alcance.
2. El administrador agrega los roles, permisos y otros items de acceso que van juntos.
3. El solicitante elige el paquete en lugar de elegir entitlements individuales.
4. El aprobador revisa la solicitud y decide si el paquete puede otorgarse.
5. Si se aprueba, el sistema asigna todos los entitlements incluidos en una sola accion gobernada.
6. Si mas adelante se debe retirar el acceso, el sistema revoca todo el paquete o los items afectados segun la politica.

## 5. Flujos Alternativos y Excepciones

### A. El Paquete No Aplica al Alcance

Si el paquete no es valido para el tenant, sistema o sucursal, la solicitud no puede continuar.

### B. Conflicto por Inclusion Parcial

Si un item del paquete entra en conflicto con un acceso existente, el sistema avisa al aprobador antes de cerrar la decision.

### C. Paquete Vencido

Si el paquete tiene vigencia limitada y ya vencio, el sistema no lo otorga e informa al solicitante.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Un paquete debe representar un bloque de negocio gobernado, no una lista arbitraria. |
| BR-02 | Un paquete solo puede incluir entitlements permitidos por el alcance objetivo. |
| BR-03 | La aprobacion debe decidir el paquete completo, no un estado parcial ambiguo. |
| BR-04 | Cada asignacion debe ser auditable por usuario, paquete, alcance y resultado. |
| BR-05 | Los paquetes vencidos no deben permanecer activos. |
| BR-06 | Un paquete solo puede reutilizarse cuando su contenido y version siguen controlados. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un administrador puede definir un paquete con multiples entitlements gobernados. |
| 2 | Un solicitante puede pedir el paquete en lugar de seleccionar cada entitlement. |
| 3 | Un aprobador puede aprobar o denegar la solicitud del paquete. |
| 4 | Los paquetes aprobados conceden todos los entitlements incluidos en una sola accion controlada. |
| 5 | La asignacion del paquete y su resultado son auditables. |
| 6 | Los paquetes vencidos o invalidos no pueden activarse. |

## 8. Requisitos Tecnicos

- Introducir un modelo de paquete con definiciones versionadas y membresia de items.
- Persistir el alcance del paquete, los entitlements incluidos, el estado de aprobacion y el estado efectivo de asignacion.
- Reutilizar la ruta de aprobacion y el registro de auditoria para que las decisiones del paquete sigan siendo trazables.
- Soportar reglas de vencimiento y revocacion del paquete en la capa de enforcement de acceso.
- Mantener la composicion del paquete alineada con los catalogos de sistema y autorizacion.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `SystemSuite`, `Role`, `Profile`, `PermissionTemplate`, `ApprovalRequest` |
| Historias Funcionales | FS-02, FS-05, FS-24 |
| ADRs | ADR-0012, ADR-0015, ADR-0035 |
