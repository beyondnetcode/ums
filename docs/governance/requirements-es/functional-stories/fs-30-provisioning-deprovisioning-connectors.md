# FS-30: Conectores de Provisioning y Deprovisioning para Sistemas Descendentes

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Cuando el acceso cambia en UMS, los sistemas descendentes deben recibir automaticamente la instruccion correspondiente de crear, actualizar o eliminar. UMS debe proporcionar conectores de provisioning gobernados para mantener sincronizadas las operaciones de identidad en aplicaciones externas.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Operaciones de Identidad | Configura el conector y sus reglas de mapeo. |
| Propietario del Sistema Externo | Confirma el destino descendente y el modelo de acceso. |
| Conector de Integracion | Aplica la accion de provisioning o deprovisioning. |
| Auditor | Revisa el historial de provisioning y los fallos. |

## 3. Precondiciones de Negocio

- Un sistema o aplicacion descendente ya fue registrado como destino.
- Existen reglas de mapeo para los datos que deben sincronizarse.
- El tenant tiene un flujo aprobado para provisioning y deprovisioning.

## 4. Flujo Funcional Principal

1. El Administrador de Operaciones de Identidad registra un destino descendente y define el mapeo.
2. Cuando un usuario, perfil o entitlement cambia en UMS, el sistema genera una accion de provisioning.
3. El conector entrega el cambio al sistema descendente.
4. Si el cambio no puede entregarse de inmediato, el sistema mantiene la accion trazable hasta que se resuelva o tenga exito.
5. Cuando el acceso se elimina, el sistema envia la accion de deprovisioning para que el sistema descendente no conserve acceso obsoleto.

## 5. Flujos Alternativos y Excepciones

### A. El Sistema Destino No Esta Disponible

Si el sistema descendente no esta disponible, la accion se retiene y se reintenta segun la politica.

### B. Error de Mapeo

Si los datos mapeados estan incompletos o invalidos, el sistema marca la accion como fallida y requiere correccion.

### C. Se Requiere Retiro de Acceso

Si un usuario pierde el acceso, la accion de deprovisioning debe intentarse y seguirse hasta que el sistema descendente deje de otorgar ese acceso.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Todo cambio de acceso gobernado debe reflejarse en el sistema descendente cuando el conector este habilitado. |
| BR-02 | El deprovisioning es obligatorio cuando el acceso se revoca o expira. |
| BR-03 | Los fallos deben ser visibles y reintentables; no pueden mantener acceso en silencio. |
| BR-04 | Las reglas de mapeo deben ser explicitas y versionadas para cada destino. |
| BR-05 | El mismo cambio de usuario o perfil no debe crear operaciones duplicadas en el sistema descendente. |
| BR-06 | El resultado de acceso en el sistema descendente debe seguir siendo auditable. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Se puede registrar un conector para un sistema descendente. |
| 2 | Los cambios de acceso generan la accion de provisioning o deprovisioning esperada. |
| 3 | La accion permanece trazable hasta que el sistema descendente confirma el cambio o se corrige. |
| 4 | El retiro de acceso dispara una accion de deprovisioning. |
| 5 | Los fallos son visibles para operaciones y pueden reintentarse. |
| 6 | Se previenen acciones descendentes duplicadas para el mismo cambio. |

## 8. Requisitos Tecnicos

- Introducir un modelo de conector para destinos de provisioning descendente y sus mapeos.
- Persistir acciones de provisioning, estado de entrega, conteo de reintentos y resultado final.
- Emitir y consumir eventos de outbox para cambios de perfil, rol y entitlement.
- Soportar reintento manual y visibilidad operacional para entregas fallidas.
- Mantener los conectores aislados mediante adaptadores tipo ACL para que los esquemas externos no contaminen el dominio.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `UserAccount`, `Profile`, `Role`, `SystemSuite`, `IdentityProvider`, `AuditRecord` |
| Historias Funcionales | FS-03, FS-05, FS-24 |
| ADRs | ADR-0015, ADR-0033, ADR-0072 |
