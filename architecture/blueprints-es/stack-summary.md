# ⚡ Guía Rápida del Stack Tecnológico Progresivo de Node.js (Referencia Rápida)

Esta guía sirve como referencia autoritativa y de alta densidad de herramientas por capa arquitectónica para desarrolladores y agentes autónomos que trabajan en la Arquitectura de Referencia Progresiva de Node.js.

---

### 1. Runtime y Lenguaje
*   **Entorno de Ejecución:** Node.js v20 LTS
*   **Lenguaje:** TypeScript v5.4+ (Modo Estricto)
*   **Motor de Compilación:** SWC (`@swc/core`) dentro de Nx Monorepo
*   **Calidad de Código:** ESLint v8 + Prettier v3
*   **Quality Gates de Git:** Husky + lint-staged

### 2. Capa de API
*   **Protocolos Internos:** gRPC (NestJS Microservices)
*   **Protocolos Externos:** API REST (NestJS Express)
*   **Estándar de Validación:** `class-validator` + `class-transformer`
*   **Documentación de API:** OpenAPI v3 (Swagger) vía decoradores de NestJS

### 3. Capa de Gateway
*   **API Gateway:** Kong Gateway (Edición Open Source)
*   **Gestión de Sesión:** Tokens Web JSON (JWT) firmados con RS256
*   **Seguridad Interna:** TLS mutuo (mTLS) vía Istio Service Mesh
*   **Rate Limiting:** Limitador de ventana deslizante (plugin Kong Redis)

### 4. Capa de Dominio y Aplicación
*   **Patrón Arquitectónico:** Arquitectura Hexagonal (Puertos y Adaptadores)
*   **Estrategia de Monorepo:** Nx Monorepo
*   **Patrón de Ejecución:** Monolito Modular (Preparado para Dapr)
*   **Patrón de Segregación:** CQRS interno (Módulo CQRS de NestJS)
*   **Inyección de Dependencias:** Contenedor DI nativo de NestJS

### 5. Capa de Datos
*   **Base de Datos Relacional Principal:** PostgreSQL v16
*   **Mapeo Relacional (ORM):** TypeORM (TypeScript)
*   **Consultas de Alto Rendimiento:** Driver nativo `pg`
*   **Motor de Migración de Esquema:** Migraciones de TypeORM vía Init-Containers de Kubernetes
*   **Caché en Memoria:** Redis v7.2 (Replicaciones Sentinel / Cluster)
*   **Almacén de Objetos y Activos:** MinIO (Compatible con S3, Autohospedado)
*   **Broker de Mensajería Asíncrona:** RabbitMQ (AMQP v0.9.1, Autohospedado)

### 6. Estrategia Multi-tenancy
*   **Modelo de Aislamiento de Datos:** Base de datos compartida con Seguridad a Nivel de Fila (RLS)
*   **Contexto de Resolución de Tenant:** Extracción de claims JWT vía Guards de NestJS
*   **Aplicación del Aislamiento:** Inyección dinámica de sesión de transacción de base de datos (`SET LOCAL app.current_tenant`)

### 7. Infraestructura y Despliegue
*   **Motor de Contenedores:** Docker v25 (Imágenes node Distroless multi-etapa)
*   **Plataforma Orquestadora:** Kubernetes (K8s v1.28+)
*   **Gestión de Secretos y Claves:** HashiCorp Vault (OSS, Autohospedado)
*   **Empaquetador de Despliegue:** Charts parametrizados de Helm v3

### 8. Observabilidad
*   **Estándar de Instrumentación:** OpenTelemetry (SDKs agnósticos del proveedor)
*   **Agregador de Logs:** Grafana Loki (OSS)
*   **Trazas Distribuidas:** Jaeger (OSS)
*   **Servidor de Métricas:** Motor de recolección Prometheus

### 9. Seguridad
*   **Registros de Auth:** OIDC y SAML federados + UMS Native BCrypt Store
*   **Control de Acceso:** RBAC jerárquico + Control de Acceso Basado en Atributos (ABAC)
*   **Auditoría de Dependencias:** CLI de Snyk + `npm audit` dentro de los pipelines de CI/CD

### 10. Experiencia del Desarrollador (DevEx)
*   **Servicios Locales:** Docker Compose Spec
*   **Framework de Pruebas Unitarias:** Jest
*   **Pruebas de Integración:** Jest + Supertest con **Testcontainers**
*   **Pruebas de Extremo a Extremo (E2E):** Playwright
