# TE-07: YARP API Gateway — Multi-Client SaaS Entry Point

| Field | Value |
|-------|-------|
| **TE ID** | TE-07 |
| **Status** | Proposed |
| **ADR Reference** | ADR-0058 (API Gateway Evolution), ADR-0053 (OpenTelemetry) |
| **Satisfies** | NFR: Multi-client routing, Security enforcement, Rate limiting |
| **Owner** | Platform Team |
| **Date** | 2026-05-22 |

---

## Problem

UMS MVP uses nginx embedded in the web-app container as a reverse proxy. When the mobile client is introduced, there is no centralized entry point to enforce security policy, rate limits, or tenant routing across all clients. nginx is not extensible in C# and cannot share the existing UMS middleware pipeline.

## Solution: Dedicated YARP Gateway Application

Introduce `ums.gateway` — a standalone ASP.NET Core project using Microsoft YARP as the reverse proxy engine. The gateway becomes the only entry point for all inbound requests. nginx retains responsibility for static file serving only.

```
Internet
    │
    ▼
┌───────────────────────────────────────────┐
│  ums.gateway  (ASP.NET Core + YARP)        │
│                                           │
│  ┌─────────────────────────────────────┐  │
│  │  Middleware Pipeline                │  │
│  │  1. Rate Limiting (per tenant)      │  │
│  │  2. Security Headers                │  │
│  │  3. JWT Validation (optional)       │  │
│  │  4. YARP Reverse Proxy              │  │
│  └────────────────┬────────────────────┘  │
└───────────────────┼───────────────────────┘
                    │
         ┌──────────┴──────────┐
         ▼                     ▼
  ┌────────────┐        ┌───────────────┐
  │  ums.api   │        │  ums.web-app  │
  │  :8080     │        │  nginx :80    │
  │            │        │  (static only)│
  └────────────┘        └───────────────┘
         ▲
  ums.mobile-app
  (routes via gateway)
```

---

## Project Structure

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

## Implementation Blueprint

### 1. Project Setup

```xml
<!-- Ums.Gateway.csproj -->
<PackageReference Include="Yarp.ReverseProxy" Version="2.*" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="*" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="*" />
```

### 2. YARP Route Configuration (`appsettings.json`)

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

### 3. Program.cs — Middleware Pipeline

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

// OpenTelemetry — reuses ADR-0053 configuration
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

### 4. nginx.conf After Migration

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

All security headers and proxy configuration are removed from nginx.

### 5. Docker Compose Addition

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

## Migration Path from Current State

| Step | Action | Risk |
|------|--------|------|
| 1 | Create `ums.gateway` project with YARP | Low — no existing code touched |
| 2 | Move `Yarp.ReverseProxy` NuGet from `Ums.Presentation` to gateway | Low |
| 3 | Port security headers from `nginx.conf` to gateway middleware | Low — direct translation |
| 4 | Simplify `nginx.conf` to static-only | Low |
| 5 | Update `docker-compose.yml` to add gateway and redirect ports | Medium — port changes |
| 6 | Update `ums.web-app` Dockerfile (nginx config simplified) | Low |

---

## Implementation Trigger

Do **not** implement until the mobile client (`ums.mobile-app`) project is initiated or until multi-client routing becomes a confirmed product requirement. The current nginx-embedded approach is fully valid for single-client MVP.

---

## Acceptance Criteria

- [ ] All requests to `/api/**` and `/graphql` are routed through the gateway to `ums-api`.
- [ ] All requests to `/**` (non-API) are routed through the gateway to `ums-web`.
- [ ] Security headers are present on every response regardless of client type.
- [ ] Rate limiting applies per `tenant_id` claim; anonymous requests use a shared partition.
- [ ] OpenTelemetry traces propagate through the gateway to downstream services.
- [ ] nginx.conf contains no `proxy_pass`, no `add_header` security directives.
- [ ] Mobile client can consume `ums-api` through the gateway without special configuration.

---

**[TE Index](./index.md)** | **[ADR-0058](../../adrs/0058-api-gateway-yarp-evolution.md)** | **[Traceability Matrix](../../traceability-matrix.md)**
