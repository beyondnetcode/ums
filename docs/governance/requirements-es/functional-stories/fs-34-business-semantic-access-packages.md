# FS-34: Paquetes de Acceso con Semantica de Negocio y Compositor de Politicas

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los responsables de negocio necesitan paquetes de acceso definidos en lenguaje de negocio, no solo en entitlements tecnicos. UMS debe permitir componer paquetes usando conceptos como tenant, sucursal, nivel de socio, sistema y rol, y luego publicar paquetes gobernados con versionado y reglas de politica.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Arquitecto de Entitlements | Disena la composicion del paquete. |
| Propietario de Negocio | Valida que el paquete coincide con la necesidad del negocio. |
| Aprobador | Confirma si el paquete puede publicarse o asignarse. |
| Administrador de Tenant | Reutiliza paquetes para patrones de acceso repetitivos. |

## 3. Precondiciones de Negocio

- Ya existen el catalogo de sistemas y el catalogo de autorizacion.
- El paquete puede mapearse a alcances de negocio reales como tenant, sucursal, sistema o tipo de socio.
- La aprobacion y la politica de expiracion estan configuradas.

## 4. Flujo Funcional Principal

1. El Arquitecto de Entitlements crea un paquete usando terminos de negocio y versionado.
2. El arquitecto mapea el paquete a roles, permisos y reglas de alcance.
3. El Propietario de Negocio revisa el paquete y confirma que coincide con el proceso o modelo de socio.
4. El paquete se publica y queda disponible para solicitud o asignacion.
5. Cuando el paquete cambia, se crea una nueva version en lugar de reescribir el historial en silencio.

## 5. Flujos Alternativos y Excepciones

### A. El Alcance No Coincide

Si el paquete no coincide con el tenant, sucursal o alcance de socio objetivo, el sistema bloquea la publicacion.

### B. Composicion Invalida

Si un paquete incluye entitlements en conflicto, el sistema avisa al autor antes de que el paquete pueda publicarse.

### C. Version Expirada

Si una version anterior ya no es valida, el sistema no permite reutilizarla sin una politica explicita.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | Los paquetes deben nombrarse y modelarse en lenguaje de negocio. |
| BR-02 | Las versiones del paquete deben ser inmutables una vez publicadas. |
| BR-03 | El paquete debe reflejar solo el alcance de negocio permitido. |
| BR-04 | Un paquete puede solicitarse, aprobarse, asignarse y revocarse como una unidad gobernada. |
| BR-05 | Los cambios de composicion deben conservar trazabilidad entre versiones. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un paquete puede modelarse con alcance de negocio y entitlements tecnicos juntos. |
| 2 | Un paquete puede versionarse y publicarse sin perder historial. |
| 3 | Un propietario de negocio puede confirmar que el paquete coincide con el patron de acceso esperado. |
| 4 | El sistema bloquea composiciones invalidas o en conflicto. |
| 5 | El paquete puede reutilizarse para escenarios de negocio repetitivos. |

## 8. Requisitos Tecnicos

- Agregar metadatos del paquete para terminos de negocio, alcance, version y estado de ciclo de vida.
- Mantener inmutable la composicion del paquete despues de su publicacion.
- Soportar la composicion por roles, permisos y reglas de acceso.
- Reutilizar aprobacion y auditoria para que la publicacion y asignacion del paquete sean trazables.
- Permitir que la resolucion del paquete se evalúe por alcance de negocio sin filtrar detalles de implementacion interna.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `SystemSuite`, `Role`, `PermissionTemplate`, `Profile`, `ApprovalRequest` |
| Historias Funcionales | FS-02, FS-24, FS-29 |
| ADRs | ADR-0012, ADR-0015, ADR-0035 |
