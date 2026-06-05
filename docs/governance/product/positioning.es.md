# Posicionamiento de Producto - UMS

## Declaracion de Posicionamiento

UMS es un **Authorization and governance control plane for multi-tenant SaaS**.

## Que Significa

UMS ayuda a equipos de software y plataformas a controlar quien puede hacer que, en que tenant, en que sucursal, bajo que politica de negocio y con total trazabilidad y explicabilidad.

## Clientes Objetivo

- Proveedores SaaS con productos B2B multi-tenant.
- Equipos de plataforma que necesitan control centralizado de autorizacion y administracion delegada.
- Equipos de seguridad y cumplimiento que necesitan gobierno de acceso explicable.

## Problema Principal que Resuelve UMS

La mayoria de los productos de identidad optimizan primero la autenticacion. UMS optimiza primero la autorizacion y el gobierno, manteniendo soporte para identidad federada y credenciales nativas opcionales.

## Por Que UMS Es Diferente

1. Esta construido alrededor de un grafo de permisos, no solo de un directorio de usuarios.
2. Trata tenant, sucursal, sistema, rol y paquete como conceptos de negocio de primera clase.
3. Esta diseñado para ser explicable, para que soporte y auditoria entiendan las decisiones de acceso.
4. Es multi-tenant por diseño, no como una adaptacion posterior.
5. Puede operar de forma standalone o delegar identidad a proveedores externos.

## Pilares de Mensaje

- Control plane: un solo lugar para gobernar el acceso en un ecosistema SaaS.
- Acceso explicable: cada decision puede rastrearse y entenderse.
- Delegacion segura por tenant: los admins locales reciben solo el alcance necesario.
- Gobierno con semantica de negocio: paquetes y politicas que reflejan la operacion real.
- Extensible por diseño: IdPs e integraciones externas siguen siendo opcionales.

## Pitch Corto

UMS ofrece a los equipos SaaS una capa de autorizacion y gobierno gobernada, explicable y segura por tenant, sin atarlos a un unico proveedor de identidad.

## Pitch Largo

UMS es el control plane de autorizacion y gobierno para SaaS multi-tenant. Combina delegacion por tenant, grafos de permisos, flujos de aprobacion y paquetes de acceso explicables para que los equipos de producto gobiernen el acceso con la misma claridad con la que gobiernan sus propios dominios de negocio. UMS funciona de forma standalone o junto a proveedores de identidad externos, lo que lo hace ideal para organizaciones que quieren control fuerte sin vendor lock-in.
