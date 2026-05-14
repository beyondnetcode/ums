# Guía Rápida del Stack Tecnológico Empresarial Polyglot (Referencia Rápida)

Esta guía sirve como referencia autoritativa y de alta densidad de herramientas por capa arquitectónica para desarrolladores y agentes autónomos que trabajan en la Arquitectura de Referencia Enterprise.

---

### 1. Tiempo de Ejecución y Lenguaje
*   **Núcleo Empresarial:** .NET 8 LTS (C#)
*   **Servicios Satélite:** Node.js v20 LTS (TypeScript v5.4+)
*   **Motor del Compilador:** Roslyn (.NET) / SWC (Node)
*   **Calidad del Código:** SonarJS / SonarLint / ESLint v8 + Prettier v3
*   **Puertas de Calidad Git:** Husky + lint-staged / Hooks de pre-commit .NET

### 2. Capa de API
*   **Protocolos Internos:** gRPC (gRPC-dotnet / NestJS Microservices)
*   **Protocolos Externos:** REST API (ASP.NET Core / NestJS Express)
*   **Estándar de Validación:** FluentValidation / class-validator + class-transformer
*   **Documentación de API:** OpenAPI v3 (Swagger) vía Swashbuckle / decoradores de NestJS

### 3. Capa de Gateway
*   **API Gateway:** Kong Gateway (Edición Open Source)
*   **Gestión de Sesiones:** JSON Web Tokens (JWT) firmados con RS256
*   **Seguridad Interna:** TLS mutuo (mTLS) vía Istio Service Mesh
*   **Limitación de Tasa:** Limitador de tasa de ventana deslizante (plugin Kong Redis)

### 4. Capa de Dominio y Aplicación
*   **Patrón Arquitectónico:** Arquitectura Hexagonal (Puertos y Adaptadores)
*   **Estrategia de Monorepo:** Nx Monorepo
*   **Patrón de Ejecución:** Monolito Modular (Preparado para Dapr)
*   **Patrón de Segregación:** CQRS interno (MediatR / Módulo CQRS de NestJS)
*   **Inyección de Dependencias:** Contenedor Nativo (.NET / NestJS)

### 5. Capa de Datos
*   **Base de Datos Primaria (.NET):** SQL Server 2022
*   **Base de Datos Primaria (Node):** SQL Server 2022 *(decisión UMS: todos los servicios usan SQL Server 2022 según ADR-0041)*
*   **Mapeo Relacional (ORM):** EF Core (.NET) / TypeORM (Node)
*   **Motor de Migración de Esquema:** Migraciones de EF / Migraciones de TypeORM vía K8s Init-Containers
*   **Caché en Memoria:** Redis v7.2 (Sentinel / Cluster)
*   **Almacén de Objetos y Activos:** MinIO (Compatible con S3, Autohospedado)
*   **Broker de Mensajería Asíncrona:** RabbitMQ (AMQP v0.9.1, Autohospedado)

### 6. Estrategia Multi-tenancy
*   **Modelo de Aislamiento de Datos:** Base de Datos Compartida con Seguridad a Nivel de Fila (RLS)
*   **Implementación (.NET):** SQL Server SESSION_CONTEXT + Políticas de Seguridad
*   **Implementación (Node):** SQL Server SESSION_CONTEXT + Security Policies *(alineado con el núcleo .NET; según ADR-0041)*
*   **Resolución de Contexto:** Extracción de claims JWT vía Middleware / Guards

### 7. Infraestructura y Despliegue
*   **Motor de Contenedores:** Docker v25 (Imágenes Distroless multi-etapa)
*   **Plataforma Orquestadora:** Kubernetes (K8s v1.28+)
*   **Gestión de Secretos y Claves:** HashiCorp Vault (OSS, Autohospedado)
*   **Empaquetador de Despliegue:** Charts parametrizados de Helm v3

### 8. Observabilidad
*   **Estándar de Instrumentación:** OpenTelemetry (SDKs neutros de proveedor)
*   **Agregador de Logs:** Grafana Loki (OSS)
*   **Trazas Distribuidas:** Jaeger (OSS)
*   **Servidor de Métricas:** Motor de Pull de Prometheus

### 9. Seguridad
*   **Registros de Auth:** OIDC y SAML Federados + Almacén BCrypt Nativo de UMS
*   **Control de Acceso:** RBAC Jerárquico + Control de Acceso Basado en Atributos (ABAC)
*   **Auditoría de Dependencias:** Snyk CLI + `npm audit` / `dotnet list package --vulnerable`

### 10. Experiencia del Desarrollador (DevEx)
*   **Servicios Locales:** Especificación Docker Compose (SQL Server, Redis, etc.)
*   **Framework de Pruebas Unitarias:** xUnit (.NET) / Jestá (Node)
*   **Pruebas de Integración:** Testácontainers (SQL Server / PostgreSQL / Redis)
*   **Pruebas de Extremo a Extremo (E2E):** Playwright
