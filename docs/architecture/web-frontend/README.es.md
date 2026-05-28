# Referencia Aplicada Web Frontend React de UMS

> Idioma: [English](./README.md) | [Espanol](./README.es.md)

Esta seccion documenta como la implementacion React Web de UMS aplica el Estandar Web Frontend React de Evolith.

UMS es una referencia aplicada, no la fuente de estandares frontend universales. Las reglas reutilizables pertenecen a Evolith. UMS conserva rutas concretas de producto, headers locales de API, modulos de producto, evidencia de codigo fuente y adaptaciones especificas de implementacion.

## Documentos

| Documento | Proposito |
|---|---|
| [Referencia Aplicada React UMS](./ums-react-applied-reference.es.md) | Mapeo basado en evidencia entre topicos React de Evolith y archivos fuente de UMS. |

## Limite de autoridad

| Aspecto | Evolith | UMS |
|---|---|---|
| Estandares reutilizables | Posee principios, reglas boilerplate, quality gates y criterios de promocion | Consume estandares |
| Implementacion de producto | Referencia ejemplos solamente | Posee codigo fuente concreto y decisiones locales |
| Sistema de diseno UI | Posee gobierno de tokens y reglas de accesibilidad | Posee valores de tokens y componentes UMS concretos |
| Acceso a datos | Posee requerimientos reutilizables de frontera | Posee URLs, headers, comportamiento CSRF y estrategia de tenant de UMS |

## Alcance de evidencia actual

Esta referencia se basa en la app React actual ubicada en:

```text
src/apps/ums.web-app
```

Las fuentes observadas incluyen bootstrap de aplicacion, routing, layout shell, contexto/cliente HTTP, configuracion Tailwind y definiciones de tokens Material Design 3.

---
[Volver al Portal de Arquitectura](../index.es.md)
