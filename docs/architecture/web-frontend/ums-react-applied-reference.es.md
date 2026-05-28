# Referencia Aplicada React UMS

> Idioma: [English](./ums-react-applied-reference.md) | [Espanol](./ums-react-applied-reference.es.md)

## 1. Proposito

Este documento mapea la implementacion React Web de UMS contra el Estandar Web Frontend React de Evolith. Es evidencia de la implementacion actual del producto, no un estandar universal.

Las reglas reutilizables pertenecen a Evolith. Los detalles especificos de UMS permanecen aqui salvo que se promuevan mediante un ADR de Evolith, estandar de gobierno o patron canonico.

## 2. Alcance de fuente

La referencia aplicada cubre:

```text
src/apps/ums.web-app
```

Perfil de implementacion observado:

| Aspecto | Implementacion UMS |
|---|---|
| Runtime | React 18.3.1 |
| Build | Vite 5.4.10 |
| Lenguaje | TypeScript |
| Routing | React Router DOM 6.30.3 |
| Estado servidor | TanStack React Query 5.100.9 |
| Estado cliente | Zustand 5.0.13 |
| Validacion runtime | Zod 4.4.3 |
| HTTP | Axios 1.16.0 |
| Estilos | TailwindCSS 3.4.19 mas variables CSS |
| Mocking | MSW 2.14.6 |
| Tests unitarios/componentes | Vitest y Testing Library |
| E2E | Playwright |

## 3. Mapeo Evolith hacia UMS

| Topico Evolith | Evidencia fuente UMS | Clasificacion |
|---|---|---|
| Bootstrap de aplicacion | `src/apps/ums.web-app/src/main.tsx` centraliza React StrictMode, provider React Query, sincronizacion de locale, configuracion de request context y arranque opcional de MSW. | Evidencia aplicada |
| Provider de server-state | `main.tsx` crea un `QueryClient` con opciones compartidas de query. | Perfil candidato Evolith; defaults exactos permanecen locales |
| Routing | `src/apps/ums.web-app/src/App.tsx` usa `BrowserRouter`, imports lazy por ruta, `Suspense`, `RouteLoader` y redirecciones fallback. | Evidencia aplicada |
| Error boundary | `App.tsx` envuelve la aplicacion ruteada con `AppErrorBoundary`. | Patron reutilizable |
| Layout shell | `src/apps/ums.web-app/src/presentation/shared/layouts/MainLayout.tsx` compone top app bar, nav rail, region de contenido, comportamiento de notificaciones e integracion idle timeout. | Patron reutilizable; componentes concretos y timeout son locales |
| Tokens Material Design 3 | `src/apps/ums.web-app/src/index.css` define tokens de roles Material Design 3 light/dark como variables CSS. | Candidato de gobierno de tokens; valores concretos son locales |
| Puente Tailwind de tokens | `src/apps/ums.web-app/tailwind.config.js` expone colores semanticos `m3.*` mediante `hsl(var(--token))`. | Candidato boilerplate |
| Frontera HTTP | `src/apps/ums.web-app/src/infrastructure/http/httpClient.ts` centraliza configuracion Axios, headers, CSRF y errores normalizados. | Patron de frontera reutilizable; headers son locales |
| Request context | `src/apps/ums.web-app/src/infrastructure/http/request-context.ts` centraliza contexto de usuario e idioma y base URL por entorno. | Patron reutilizable; placeholder de tenant es deuda tecnica local |
| Perfil de pruebas | `src/apps/ums.web-app/package.json` declara Vitest, Testing Library, MSW y Playwright. | Perfil candidato de quality gates |

## 4. Items que deben permanecer locales en UMS

| Item | Razon |
|---|---|
| Rutas de producto como `/tenants`, `/users`, `/delegations`, `/profiles` y `/feature-flags` | Navegacion de dominio UMS, no boilerplate reutilizable. |
| Nombres concretos de pantallas dashboard | Detalles de implementacion de bounded contexts UMS. |
| Headers `X-User-Id`, `X-Language` y `X-Tenant-Id` | Contrato especifico de API. Evolith debe definir el patron, no los nombres. |
| `DEV_TENANT_ID` | Placeholder de desarrollo y no apto como estandar empresarial. |
| Valores Indigo/Violet de tokens | Decision de branding del producto. Evolith posee roles de token, no estos valores. |
| Timeout idle de 15 minutos | Politica de seguridad/sesion UMS. Evolith puede exigir configurabilidad. |
| Nombres de componentes TopAppBar y NavRail | Implementacion de componentes UMS. Evolith posee el patron application shell. |

## 5. Items a promover a Evolith

| Candidato | Destino de promocion |
|---|---|
| Estructura React con `domain`, `application`, `infrastructure` y `presentation` | Estandar boilerplate React Evolith |
| Patron de composicion de providers raiz | Guia de bootstrap React Evolith |
| Lazy loading de pantallas a nivel ruta | Estandar de routing React Evolith |
| Roles de tokens Material Design 3 via variables CSS | Estandar de sistema de diseno UI Evolith |
| Mapeo de tokens semanticos Tailwind | Estandar boilerplate React Evolith |
| Frontera centralizada HTTP/request context | Estandar de acceso a datos Evolith |
| Perfil de pruebas con Vitest, Testing Library, MSW y Playwright | Quality gates frontend Evolith |

## 6. Items que requieren ADR o promocion formal

| Item | Accion requerida |
|---|---|
| React + Vite como baseline frontend empresarial por defecto | ADR Evolith |
| TanStack React Query como baseline de server-state | ADR Evolith o estandar de ingenieria |
| Zustand como perfil liviano de client-state | ADR Evolith o perfil opcional |
| Material Design 3 basado en tokens como estandar UI | Estandar UI/design-system Evolith y posible ADR |
| Zod como perfil de validacion runtime | ADR Evolith o estandar de contratos de datos |
| MSW mas Playwright como perfil estandar de testing | Estandar de pruebas Evolith o ADR |

## 7. Brechas actuales y acciones de seguimiento

| Brecha | Recomendacion |
|---|---|
| Identificador de tenant de desarrollo hardcodeado en request context | Reemplazar por resolucion de tenant autenticada o adaptador de desarrollo seguro por entorno. |
| La referencia aplicada UMS depende de convenciones de fuente aun no totalmente automatizadas | Extender la auditoria documental con checks de estructura frontend si se requiere. |
| Los valores de tokens MD3 son locales pero aun no enlazan a un ADR de sistema de diseno Evolith | Promover gobierno de tokens a Evolith antes de hacerlo obligatorio. |
| Patron de rutas protegidas no documentado en esta referencia aplicada | Agregar cuando los authorization guards se formalicen en fuente. |

## 8. Checklist de validacion

Antes de cambiar la arquitectura React Web de UMS, validar:

- El cambio se clasifica como estandar Evolith, decision local UMS o candidato de promocion.
- Los patrones compartidos permanecen independientes del lenguaje de producto UMS.
- Rutas, headers, tenant IDs y branding especificos del producto permanecen en UMS.
- Toda regla reutilizable se propone primero en Evolith.
- Documentacion en ingles y espanol se actualiza en conjunto.
- Markdown permanece UTF-8 limpio y sin iconos decorativos.

---
[Volver al Portal Web Frontend](./README.es.md)
