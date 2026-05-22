# TE-07: API Gateway con YARP — Punto de Entrada Multi-Cliente SaaS

| Campo | Valor |
|-------|-------|
| **TE ID** | TE-07 |
| **Estado** | Propuesto |
| **Referencia ADR** | ADR-0058 (Evolución API Gateway), ADR-0053 (OpenTelemetry) |
| **Satisface** | NFR: Enrutamiento multi-cliente, Aplicación de seguridad, Limitación de tasa |
| **Responsable** | Equipo de Plataforma |
| **Fecha** | 2026-05-22 |

---

## Problema

El MVP de UMS utiliza nginx embebido en el contenedor de la web-app como reverse proxy. Cuando se incorpore el cliente móvil, no existirá un punto de entrada centralizado para aplicar políticas de seguridad, límites de tasa ni enrutamiento por tenant en todos los clientes. nginx no es extensible en C# y no puede compartir el pipeline de middleware existente de UMS.

## Solución: Aplicación Gateway Dedicada con YARP

Introducir `ums.gateway` — un proyecto ASP.NET Core independiente que usa Microsoft YARP como motor de reverse proxy. El gateway se convierte en el único punto de entrada para todas las solicitudes entrantes. nginx conserva únicamente la responsabilidad de servir archivos estáticos.

```
Internet
    │
    ▼
┌───────────────────────────────────────────┐
│  ums.gateway  (ASP.NET Core + YARP)        │
│                                           │
│  ┌─────────────────────────────────────┐  │
│  │  Pipeline de Middleware             │  │
│  │  1. Limitación de tasa (por tenant) │  │
│  │  2. Cabeceras de seguridad          │  │
│  │  3. Validación JWT (opcional)       │  │
│  │  4. Reverse Proxy YARP              │  │
│  └────────────────┬────────────────────┘  │
└───────────────────┼───────────────────────┘
                    │
         ┌──────────┴──────────┐
         ▼                     ▼
  ┌────────────┐        ┌───────────────┐
  │  ums.api   │        │  ums.web-app  │
  │  :8080     │        │  nginx :80    │
  │            │        │  (solo static)│
  └────────────┘        └───────────────┘
         ▲
  ums.mobile-app
  (enruta a través del gateway)
```

---

## Estructura del Proyecto

```
ums/src/apps/ums.gateway/
├── Ums.Gateway.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Middleware/
│   ├── SecurityHeadersMiddleware.cs
│   └── TenantRateLimitMiddleware.cs
└── Dockerfile
```

---

## Plano de Implementación

### 1. Configuración del Proyecto

```xml
<!-- Ums.Gateway.csproj -->
<PackageReference Include="Yarp.ReverseProxy" Version="2.*" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="*" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="*" />
```

### 2. Configuración de Rutas YARP (`appsettings.json`)

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "ums-api",
        "Match": { "Path": "/api/{**catch-all}" }
      },
      "graphql-route": {
        "ClusterId": "ums-api",
        "Match": { "Path": "/graphql" }
      },
      "web-route": {
        "ClusterId": "ums-web",
        "Match": { "Path": "/{**catch-all}" }
      }
    },
    "Clusters": {
      "ums-api": {
        "Destinations": {
          "primary": { "Address": "http://ums-api:8080" }
        }
      },
      "ums-web": {
        "Destinations": {
          "primary": { "Address": "http://ums-web:80" }
        }
      }
    }
  }
}
```

### 3. Program.cs — Pipeline de Middleware

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("tenant-policy", context =>
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// OpenTelemetry — reutiliza configuración del ADR-0053
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation());

var app = builder.Build();

app.UseRateLimiter();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data: blob: https:; connect-src 'self' https:; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
    context.Response.Headers["Strict-Transport-Security"] =
        "max-age=31536000; includeSubDomains";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";
    await next();
});

app.MapReverseProxy();
app.Run();
```

### 4. nginx.conf Después de la Migración

```nginx
server {
    listen 80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
        try_files $uri $uri/ /index.html;
    }

    error_page 500 502 503 504 /50x.html;
    location = /50x.html {
        root /usr/share/nginx/html;
    }
}
```

Todas las cabeceras de seguridad y la configuración de proxy se eliminan de nginx.

### 5. Adición al Docker Compose

```yaml
ums-gateway:
  build:
    context: .
    dockerfile: apps/ums.gateway/Dockerfile
  container_name: ums-gateway
  ports:
    - "80:8090"
    - "443:8091"
  depends_on:
    - ums-api
    - ums-web
  networks:
    - ums-network
```

---

## Ruta de Migración desde el Estado Actual

| Paso | Acción | Riesgo |
|------|--------|--------|
| 1 | Crear proyecto `ums.gateway` con YARP | Bajo — no se toca código existente |
| 2 | Mover NuGet `Yarp.ReverseProxy` de `Ums.Presentation` al gateway | Bajo |
| 3 | Portar cabeceras de seguridad de `nginx.conf` al middleware del gateway | Bajo — traducción directa |
| 4 | Simplificar `nginx.conf` a modo solo-estático | Bajo |
| 5 | Actualizar `docker-compose.yml` para agregar gateway y redirigir puertos | Medio — cambios de puerto |
| 6 | Actualizar Dockerfile de `ums.web-app` (nginx simplificado) | Bajo |

---

## Disparador de Implementación

**No implementar** hasta que se inicie el proyecto del cliente móvil (`ums.mobile-app`) o hasta que el enrutamiento multi-cliente se convierta en un requisito de producto confirmado. El enfoque actual con nginx embebido es completamente válido para el MVP con un solo cliente web.

---

## Criterios de Aceptación

- [ ] Todas las solicitudes a `/api/**` y `/graphql` se enrutan a través del gateway hacia `ums-api`.
- [ ] Todas las solicitudes a `/**` (no-API) se enrutan a través del gateway hacia `ums-web`.
- [ ] Las cabeceras de seguridad están presentes en cada respuesta independientemente del tipo de cliente.
- [ ] La limitación de tasa se aplica por claim `tenant_id`; las solicitudes anónimas usan una partición compartida.
- [ ] Las trazas de OpenTelemetry se propagan a través del gateway hacia los servicios downstream.
- [ ] nginx.conf no contiene directivas `proxy_pass` ni `add_header` de seguridad.
- [ ] El cliente móvil puede consumir `ums-api` a través del gateway sin configuración especial.

---

**[Índice TE](./index.md)** | **[ADR-0058](../../adrs/0058-api-gateway-yarp-evolution.md)** | **[Matriz de Trazabilidad](../../traceability-matrix.md)**
