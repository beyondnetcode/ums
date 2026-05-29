# ADR-0058: Evolución de API Gateway — YARP para SaaS Multi-Cliente

## Estado

Propuesto

## Fecha

2026-05-22

## Contexto

UMS actualmente usa nginx embebido dentro de la imagen Docker `ums.web-app` como servidor de archivos estáticos y proxy reverso al backend API. Este enfoque es cohesivo y suficiente para un único cliente web.

La evolución SaaS planeada introduce al menos dos superficies de cliente distintas:

- `ums.web-app` — browser SPA (React + Vite)
- `ums.mobile-app` — aplicación móvil nativa (futuro)

Con múltiples clientes, el proxy reverso nginx embebido se convierte en un pasivo:

- Los headers de seguridad (CSP, HSTS, X-Frame-Options) están duplicados o ausentes por cliente.
- Rate limiting, validación de token de autenticación, y routing de tenant no pueden centralizarse.
- Los clientes móviles bye-passean nginx enteramente, perdiendo enforcement cross-cutting.
- Cualquier cambio en routing o política de seguridad requiere rebuild del contenedor frontend.

UMS ya tiene `Yarp.ReverseProxy` como dependencia declarada en `Ums.Presentation`, y la abstracción existente `IAuthenticationPort` es gateway-compatible. La tecnología ya está presente en el stack.

---

## Decisión

**Introducir `ums.gateway` como una aplicación ASP.NET Core dedicada usando YARP como API Gateway centralizado para todos los clientes UMS.**

El gateway será el único punto de entrada para todo el tráfico inbound. nginx se reducirá a un servidor de archivos estático puro sin responsabilidad de proxy.

### Alcance de Responsabilidades

| Concern | Dueño Actual | Dueño Objetivo |
|---|---|---|
| Servición de archivos estáticos (SPA) | nginx | nginx (sin cambios) |
| Proxy reverso a API | nginx (por cliente) | YARP gateway (centralizado) |
| Headers de seguridad | nginx.conf | YARP middleware |
| Rate limiting | No implementado | YARP + `IPartitionedRateLimiter` |
| Routing de tenant | No implementado | YARP gateway |
| Acceso API móvil | Directo a API | YARP gateway |
| Validación de token auth | Capa API | YARP gateway (pre-routing) |

### Arquitectura Objetivo

```
Internet
    │
    ▼
┌──────────────────────────────┐
│  ums.gateway  (YARP)         │  port 443 / 80
│  ASP.NET Core                │  Security headers, Rate limit,
│                              │  Tenant routing, Auth check
└────────────┬─────────────────┘
             │
    ┌────────┴──────────┐
    ▼                   ▼
┌──────────┐      ┌──────────────┐
│ ums.api  │      │ ums.web-app  │  nginx: static only
│ :8080    │      │ :80          │  no proxy logic
└──────────┘      └──────────────┘
                        ▲
                  ums.mobile-app
                  (future client,
                   routes via gateway)
```

### Lo que nginx.conf Se Convierte

```nginx
server {
    listen 80;
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
}
```

Todos los headers de seguridad, políticas CSP, y configuración de proxy migran al YARP gateway.

---

## Trigger para Implementación

Este ADR es **Propuesto** y no debe implementarse hasta que comience el desarrollo del cliente móvil. El enfoque actual de nginx-embedded permanece válido para el MVP con un único cliente web.

La implementación se activa cuando **cualquiera de las siguientes** ocurre:

1. El proyecto `ums.mobile-app` es creado.
2. Un segundo consumidor de la API requiere routing independiente o política de seguridad.
3. Rate limiting o routing aware de tenant se convierte en requerimiento de producto.

---

## Consecuencias

### Positivas

- Punto único de enforcement para headers de seguridad, rate limiting, y routing de tenant a través de todos los clientes.
- Clientes móvil y web comparten el mismo contrato de gateway sin duplicar configuración.
- El gateway está escrito en C#, co-localizado con el modelo de dominio, y comparte la instrumentación OpenTelemetry ya en uso (ADR-0053).
- `IPartitionedRateLimiter` se integra nativamente con el contexto de tenant de UMS, habilitando rate limiting por tenant sin herramientas externas.
- nginx se vuelve stateless y reemplazable — el hosting estático podría moverse a CDN sin tocar la capa API.

### Negativas

- Añade una unidad desplegable (`ums.gateway`) al Docker Compose y futuros manifiestos de Kubernetes.
- Requiere migración de nginx security headers a YARP middleware (esfuerzo one-time).
- El gateway se convierte en single point of failure si no se escala o health-checkea apropiadamente.

### Neutral

- No requiere cambios en el modelo de dominio.
- Dependencia `Yarp.ReverseProxy` ya presente en `Ums.Presentation` — debería moverse al nuevo proyecto gateway.

---

## Referencias

- [TE-07: YARP API Gateway — Blueprint de Implementación](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)
- [ADR-0053: Estrategia de Observabilidad OpenTelemetry](./0053-opentelemetry-observability.md)
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md)
- Configuración nginx actual: `ums/src/apps/ums.web-app/nginx.conf`

---

**[Índice ADR](./index.md)** | **[Portal de Arquitectura](../index.md)**