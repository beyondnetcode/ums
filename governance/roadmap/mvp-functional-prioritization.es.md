# Priorizacion de Historias Funcionales para MVP

## Proposito

Este documento define el orden recomendado de construccion de las Functional Stories de UMS desde una perspectiva de Product Owner, Analista de Negocio y Direccion Ejecutiva.

La prioridad `1` representa la capacidad funcional mas critica para un producto minimo viable de UMS. El numero de prioridad mas alto representa la capacidad menos core u opcional, que puede entregarse despues del MVP salvo que exista una restriccion contractual, regulatoria o de cliente que cambie el contexto de negocio.

## Criterios de Priorizacion

La priorizacion considera:

- Dependencia funcional: capacidades que otras historias necesitan para operar.
- Valor MVP: capacidades requeridas para demostrar que UMS funciona de punta a punta.
- Reduccion de riesgo de negocio: capacidades que reducen incertidumbre en onboarding, acceso y gobierno.
- Preparacion operativa: capacidades necesarias para soportar y diagnosticar el primer uso productivo.
- Secuencia de entrega: capacidades que deben implementarse solo cuando su base funcional ya existe.

## Supuesto de MVP

El MVP debe demostrar que UMS puede:

- Registrar una organizacion/tenant y su estrategia de identidad.
- Registrar un sistema y su estructura funcional de acceso.
- Autenticar usuarios mediante el proveedor de identidad configurado.
- Definir plantillas reutilizables de autorizacion.
- Asignar acceso a usuarios mediante perfiles.
- Soportar configuracion base y diagnostico operativo.

La automatizacion de cumplimiento, la administracion delegada, los flujos B2B externos, las notificaciones y la automatizacion avanzada de IGA son valiosas, pero dependen de la base core de identidad y autorizacion.

## Orden de Prioridad Recomendado

| Prioridad | Historia | Capacidad Funcional | Clasificacion MVP | Razon Ejecutiva / Producto | Dependencia Clave |
|---:|---|---|---|---|---|
| 1 | FS-03 | Registrar organizacion y configurar estrategia IdP | MVP Core | Establece el tenant, la organizacion y el limite de identidad. Sin esto, UMS no tiene un alcance de negocio controlado. | Ninguna |
| 2 | FS-04 | Registrar sistema y definir topologia de menus | MVP Core | Define los sistemas, modulos, opciones y acciones que UMS va a gobernar. La autorizacion no es significativa sin este catalogo. | FS-03 |
| 3 | FS-01 | Autenticacion de usuario corporativo mediante IdP externo | MVP Core | Permite que los usuarios ingresen a UMS usando el proveedor de identidad configurado. Valida la base de identidad. | FS-03 |
| 4 | FS-02 | Crear e instanciar plantilla de autorizacion | MVP Core | Crea plantillas reutilizables de permisos para sistemas y opciones funcionales. Es la base para una gestion de acceso consistente. | FS-04 |
| 5 | FS-05 | Crear perfil y asignar manualmente una plantilla de autorizacion | MVP Core | Convierte el diseno de autorizacion en acceso real para usuarios. Completa el primer flujo MVP de acceso de punta a punta. | FS-02, FS-03 |
| 6 | FS-13 | Configurar parametros jerarquicos del sistema | MVP Core | Provee la base de configuracion para comportamiento por tenant, sistema, feature, seguridad, workflow y reglas de negocio. | FS-03, FS-04 |
| 7 | FS-07 | Diagnosticar permisos mediante visualizador de grafo | Estabilizacion MVP | Da visibilidad a administradores y soporte sobre por que un usuario tiene o no tiene acceso. Reduce riesgo de salida productiva. | FS-02, FS-05 |
| 8 | FS-08 | Autenticarse mediante pagina de login hospedada y personalizable | Preparacion de Lanzamiento MVP | Mejora la experiencia de login, branding y redirecciones cuando la autenticacion base ya esta probada. | FS-01, FS-03 |
| 9 | FS-09 | MFA adaptativo y autenticacion passwordless | Seguridad Post-MVP | Fortalece la autenticacion con controles adaptativos, pero debe seguir a una autenticacion IdP externa estable. | FS-01 |
| 10 | FS-10 | Solicitud y aprobacion de acceso externo B2B | Expansion de Negocio Post-MVP | Permite acceso externo controlado y onboarding patrocinado cuando el modelo interno de acceso ya es estable. | FS-03, FS-05 |
| 11 | FS-14 | Delegar gestion de usuarios entre administradores | Escalamiento de Gobierno Post-MVP | Soporta administracion distribuida cuando la organizacion supera la gestion centralizada. | FS-05 |
| 12 | FS-11 | Cargar y validar documento de usuario | Base de Cumplimiento Post-MVP | Introduce evidencia documental de cumplimiento cuando el modelo core de usuarios y acceso ya existe. | FS-03, FS-05 |
| 13 | FS-15 | Configurar reglas de notificacion de expiracion | Operacion de Cumplimiento Post-MVP | Agrega control operativo proactivo sobre documentos, permisos o eventos de ciclo de vida proximos a expirar. | FS-11, FS-13 |
| 14 | FS-16 | Definir politica de acceso por expiracion | Ejecucion de Cumplimiento Post-MVP | Ejecuta el comportamiento de acceso cuando expiran condiciones de cumplimiento. Debe seguir a documentos y notificaciones. | FS-11, FS-15 |
| 15 | FS-06 | Autoasignar plantilla de autorizacion al crear perfil | Automatizacion Posterior | Optimiza administracion repetitiva, pero primero debe existir y validarse la asignacion manual. | FS-05 |
| 16 | FS-12 | Ejecutar proceso de promocion de rol | IGA Avanzado Posterior | Automatiza evolucion y promocion de roles. Aporta madurez, pero no es requerido para el primer MVP funcional. | FS-05, FS-06 |

## Fases de Entrega Recomendadas

| Fase | Prioridades | Alcance | Intencion de Producto |
|---|---:|---|---|
| MVP Core | 1-6 | Tenant, catalogo de sistemas, autenticacion, plantillas de autorizacion, asignacion de perfiles, configuracion jerarquica | Demostrar la capacidad minima de UMS de punta a punta. |
| Estabilizacion MVP | 7-8 | Diagnostico de permisos y experiencia de login hospedada | Hacer que el MVP sea usable, soportable y listo para lanzamiento. |
| Seguridad y Expansion Post-MVP | 9-11 | Autenticacion adaptativa, acceso B2B, administracion delegada | Expandir control, alcance y gobierno operativo. |
| Cumplimiento Post-MVP | 12-14 | Documentos, notificaciones de expiracion, politica de acceso por expiracion | Agregar gestion del ciclo de vida de cumplimiento. |
| IGA Avanzado Posterior | 15-16 | Autoasignacion y promocion de roles | Automatizar y madurar el modelo de gobierno. |

## Guia para Product Owner

El MVP no debe iniciar por historias altamente automatizadas. UMS primero necesita una base funcional clara: quien es dueno del tenant, que sistema se gobierna, como se autentican los usuarios, que accesos se pueden otorgar y como se asignan esos accesos.

Si un cliente de lanzamiento requiere acceso condicionado por documentos, entonces FS-11, FS-15 y FS-16 deben promoverse al alcance MVP despues de FS-05 y FS-13. En caso contrario, es mejor entregarlas cuando el modelo core de acceso ya este estable.

