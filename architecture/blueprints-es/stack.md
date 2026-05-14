# Definición Autoritativa del Stack Tecnológico — UMS

**Tipo de Documento:** Plano de Arquitectura (Blueprint)  
**Estado:** Aprobado  
**Framework:** Fase 02 de bMAD (Arquitectura)  
**Soberanía:** 100% Agnóstico de la Nube / Capaz de On-Premise  

---

## Referencia de Contexto Ejecutivo

*   **Nombre del Producto:** Sistema de Gestión de Usuarios (UMS)
*   **Tipo de Producto:** Híbrido (Despliegues SaaS y On-Premise localizados)
*   **Usuarios Principales:** Usuarios de Aplicaciones Integradas (Operadores, Analistas de Negocio), Admins de Tenant B2B
*   **Escala Esperada (Inicial):** < 1,000 tenants, ~50 usuarios concurrentes por tenant (~50,000 conexiones concurrentes activas totales)
*   **Escala Esperada (Objetivo):** > 10,000 tenants, ~500 usuarios concurrentes por tenant (~5,000,000 conexiones concurrentes activas totales)
*   **Tamaño del Equipo:** ~5–10 Ingenieros
*   **Experiencia del Equipo:** Sólida en NestJS y TypeScript/JavaScript, algo de DevOps (Docker, Kubernetes), sin experiencia en Java
*   **Restáricciones Existentes:** Transición a .NET 8 para la API Core, motor relacional SQL Server, caché Redis de alto rendimiento, arquitectura lista para Dapr, capacidad de despliegue estáricta en K8s on-premise.
*   **No Negociables:** Absolutamente cero dependencias de SDKs de proveedores de nube en la capa de dominio principal (Arquitectura Hexagonal estáricta); alternativas de infraestructura de código abierto 100% autohospedables.
*   **Estrategia Polyglot (ADR 0026):** .NET 8 -> SQL Server; Node.js -> PostgreSQL/MongoDB.

---

## 1. Runtime y Lenguaje

### 1.1 Lenguaje + Versión
*   **Núcleo Empresarial Principal:** **C# ejecutándose en .NET 8 LTS**
    *   **Por qué:** Rendimiento superior para tareas de cómputo intensivo, herramientas de grado empresarial y seguridad de tipos estáricta.
*   **Servicios Satélite/Ligeros:** **TypeScript v5.4+ ejecutándose en Node.js v20 LTS**
    *   **Por qué:** Alta velocidad de desarrollo y ecosistema rico para módulos no críticos.
*   **Alternativas Rechazadas:**
    *   *Golang*: Rechazado por falta de experiencia del equipo; introducir un nuevo lenguaje retrasaría el tiempo de salida al mercado.
    *   *Java (Spring Boot)*: Rechazado por mayor huella de memoria y falta de experiencia del equipo.

### 1.2 Sistema de Tipos / Configuración del Compilador
*   **Herramienta Elegida:** **Compilación estáricta de TypeScript mediante SWC (`@swc/core`) dentro de Nx Monorepo**
*   **Por qué se eligió:** `strict: true` obliga a evitar el `any` implícito y realiza chequeos estárictos de nulos, previniendo errores comunes en tiempo de ejecución. SWC compila TypeScript hasta 20 veces más rápido que el `tsc` tradicional, acelerando significativamente los ciclos de desarrollo local y las ejecuciones de CI/CD.
*   **Alternativas Rechazadas:**
    *   *tsc (compilador estándar de TypeScript)*: Rechazado como motor de compilación principal debido a los tiempos de compilación lentos en configuraciones de monorepo de alta concurrencia, pero se mantiene únicamente para el chequeo de tipos (`tsc --noEmit`).

### 1.3 Toolchain de Linting y Formateo
*   **Herramienta Elegida:** **ESLint v8 + Prettier v3 integrados mediante Husky y lint-staged**
*   **Por qué se eligió:** Garantiza un formateo de código uniforme y automatizado, además de un análisis estáático en la etapa de pre-commit, evitando que entre al repositorio código no formateado o con errores detectables.
*   **Alternativas Rechazadas:**
    *   *TSLint*: Depreciado; ya no tiene soporte.

---

## 2. Capa de API

### 2.1 Protocolo de API Principal y Framework
*   **Herramienta Elegida:** **Motor de Doble Protocolo: REST (vía NestJS Express / ASP.NET Core) + gRPC (vía NestJS Microservices / gRPC-dotnet)**
*   **Por qué se eligió:**
    *   *REST (JSON)*: Sirve como la API pública para integraciones de clientes aguas abajo y la página de Login de UMS debido a su compatibilidad universal.
    *   *gRPC (Protocol Buffers)*: Obliga a una comunicación interna de alto rendimiento, baja latencia y tipos seguros (BFF a UMS, o servicio a servicio) para garantizar que los grafos de permisos se resuelvan en **menos de 5ms**.
*   **Alternativas Rechazadas:**
    *   *GraphQL*: Rechazado debido a la alta complejidad de caché, la sobrecarga de complejidad de consultas y la dificultad de mantener límites de latencia predecibles de sub-5ms bajo un anidamiento multi-tenant B2B masivo.

### 2.2 Estándar de Documentación de API
*   **Herramienta Elegida:** **OpenAPI v3 (Swagger) generado dinámicamente mediante decoradores de NestJS / Swashbuckle para .NET**
*   **Por qué se eligió:** Asegura que las APIs REST públicas se documenten automáticamente, sean interactivas y estáén completamente sincronizadas con la base de código, con cero mantenimiento manual de documentos.
*   **Alternativas Rechazadas:**
    *   *Colecciones manuales de Postman*: Alta carga de mantenimiento y propensas a desincronizarse con las APIs de producción.

### 2.3 Librería de Validación
*   **Herramienta Elegida:** **`class-validator` + `class-transformer` (Node) / Data Annotations + FluentValidation (.NET)**
*   **Por qué se eligió:** Se integra nativamente con los Pipes de NestJS o el Pipeline de Middleware de ASP.NET para aplicar validación declarativa basada en decoradores directamente en los DTOs en el ingreso a la red.
*   **Alternativas Rechazadas:**
    *   *Joi / Zod*: Aunque son altamente performantes, no se integran tan perfectamente con la inyección de dependencias basada en clases y los decoradores como las soluciones nativas.

---

## 3. Capa de Gateway

### 3.1 Solución de Gateway por Cliente
*   **Herramienta Elegida:** **Kong API Gateway (Edición Open Source)**
*   **Por qué se eligió:** Un API gateway agnóstico de la nube, ligero y de rendimiento extremadamente alto (construido sobre Nginx) que maneja rate-limiting B2B, listas blancas de IP y enrutamiento. Se ejecuta nativamente en Kubernetes y es completamente autohospedable on-premise.
*   **Alternativas Rechazadas:**
    *   *AWS API Gateway / Azure API Management*: Rechazados porque son propietarios, estáán bloqueados por la nube y no pueden ejecutarse on-premise dentro de las redes locales de los clientes.

### 3.2 Mecanismo de Autenticación
*   **Herramienta Elegida:** **Tokens Web JSON (JWT) firmados con RS256 + TLS mutuo (mTLS)**
*   **Por qué se eligió:** Los JWT permiten sesiones de usuario sin estado y verificadas criptográficamente. mTLS (gestionado por Istio/Linkerd o Kong) asegura todo el tráfico interno de contenedor a contenedor, adhiriéndose a las directrices de Zero Trust.
*   **Alternativas Rechazadas:**
    *   *Cookies de Sesión con Estado*: Restáringe el escalado horizontal al requerir sincronización de sesiones o sesiones pegajosas (sticky sessions) a través de instancias de contenedores distribuidas.

### 3.3 Estrategia de Rate Limiting
*   **Herramienta Elegida:** **Rate Limiting de ventana deslizante aplicado en Kong Gateway usando Redis**
*   **Por qué se eligió:** Previene intentos de fuerza bruta o de denegación de servicio. Aplicar estáo en la capa de Gateway usando Redis ahorra ciclos de CPU a nivel de aplicación al descartar el tráfico malicioso antes de que llegue a los pods.
*   **Alternativas Rechazadas:**
    *   *Rate-limiting en memoria a nivel de aplicación*: Riesgo de agotamiento de memoria y no comparte el estado del rate-limiting entre múltiples pods horizontales.

---

## 4. Capa de Dominio y Aplicación

### 4.1 Patrón Arquitectónico
*   **Herramienta Elegida:** **Arquitectura Hexagonal (Puertos y Adaptadores) / Arquitectura Limpia**
*   **Por qué se eligió:** Obligatorio para asegurar que la capa de Dominio central tenga **absolutamente cero dependencias** de NestJS, EF Core, TypeORM o SDKs de nube externos. La lógica central se comunica exclusivamente con interfaces (Puertos), haciendo que el kernel sea completamente soberano y preparado para el futuro.
*   **Alternativas Rechazadas:**
    *   *Arquitectura de 3 capas estándar*: Crea un fuerte acoplamiento entre la lógica de negocio, los ORMs de base de datos y los frameworks de red, violando nuestára restricción de soberanía no negociable.

### 4.2 Estrategia de Módulo / Contexto Acotado
*   **Herramienta Elegida:** **Monolito Modular dentro de Nx Monorepo (preparado para Dapr)**
*   **Por qué se eligió:** Minimiza la complejidad operativa y de despliegue inicial para nuestáro equipo de 5-10 ingenieros. Todos los contextos (`Identity`, `Authorization`, `Configuration`, `Audit`) estáán aislados dentro de límites estárictos de librerías, lo que permite dividirlos en microservicios independientes de Dapr sin refactorizar los modelos de dominio centrales.
*   **Alternativas Rechazadas:**
    *   *Microservicios distribuidos desde el Día 1*: Alta complejidad operativa, sobrecarga de despliegue y latencia de red que abrumaría a un equipo de ingeniería pequeño.

### 4.3 Enfoque CQRS
*   **Herramienta Elegida:** **CQRS interno a través de librerías (MediatR en .NET / Módulo CQRS en NestJS)**
*   **Por qué se eligió:** Desacopla la compilación pesada del grafo de permisos (Consultas) de las mutaciones de identidad básicas (Comandos), optimizando el rendimiento de lectura.
*   **Alternativas Rechazadas:**
    *   *CRUD directo*: Consultar y compilar directamente tablas relacionales en cada solicitud de lectura degrada el rendimiento de la base de datos bajo altas cargas concurrentes.

---

## 5. Capa de Datos

### 5.1 Base de Datos Primaria + ORM/Query Builder
*   **Para Servicios .NET 8:** **SQL Server 2022 + Entity Framework Core (EF Core)**
    *   **Por qué:** Soporte oficial de Microsoft, integración profunda con el ecosistema y RLS optimizado vía `SESSION_CONTEXT`.
*   **Para Servicios Relacionales Node.js:** **PostgreSQL v16 + TypeORM / Drizzle**
    *   **Por qué:** Soporte nativo de JSONB robusto y alta madurez en la comunidad Node.js.
*   **Para Servicios NoSQL Node.js:** **MongoDB (Atlas / Enterprise)**
    *   **Por qué:** Alta flexibilidad para documentos no estructurados y prototipado rápido.

### 5.2 Estrategia de Migración
*   **Herramienta Elegida:** **Migraciones de TypeORM ejecutadas mediante Init-Containers de K8s**
*   **Por qué se eligió:** Garantiza que los esquemas de base de datos estáén versionados y las migraciones se ejecuten secuencial y exitosamente antes de que los pods de la aplicación se activen, previniendo la desincronización de esquemas durante actualizaciones continuas (rolling updates).
*   **Alternativas Rechazadas:**
    *   *TypeORM `synchronize: true`*: Extremadamente peligroso para entornos de producción, ya que puede causar la pérdida accidental de datos.

### 5.3 Capa de Caché
*   **Herramienta Elegida:** **Redis v7.2 (Sentinel / Cluster autohospedado)**
*   **Por qué se eligió:** Proporciona un caché distribuido en memoria, de latencia ultra baja y autohospedable. Las configuraciones de Sentinel/Cluster replicadas aseguran alta disponibilidad y tiempos de lectura de sub-3ms para grafos de autorización compilados.
*   **Alternativas Rechazadas:**
    *   *Memcached*: Carece de soporte robusto para estructuras de datos (hashes, sets, sorted sets) y failover de replicación nativa.

### 5.4 Almacenamiento de Objetos / Archivos
*   **Herramienta Elegida:** **MinIO (Autohospedado, Compatible con S3)**
*   **Por qué se eligió:** Almacenamiento de objetos de alto rendimiento y completamente autohospedable. Implementa exactamente el contrato de API de AWS S3, permitiendo despliegues locales sin bloqueo de nube.
*   **Alternativas Rechazadas:**
    *   *AWS S3*: Rechazado como opción principal porque es un servicio de nube propietario que no puede ejecutarse on-premise para despliegues localizados.

### 5.5 Cola de Mensajes / Bus de Eventos
*   **Herramienta Elegida:** **RabbitMQ (Autohospedado, AMQP v0.9.1)**
*   **Por qué se eligió:** Broker de alto rendimiento, ligero y completamente autohospedable con capacidades de enrutamiento robustas. Se adapta perfectamente a las redes on-premise y conlleva una baja sobrecarga administrativa.
*   **Alternativas Rechazadas:**
    *   *Apache Kafka*: Ofrece un mayor rendimiento pero conlleva una carga administrativa e infraestructura masiva (Zookeeper/KRaft) que es innecesaria para nuestára escala inicial.

---

## 6. Estrategia Multi-tenancy

### 6.1 Modelo de Aislamiento
*   **Estrategia:** **Base de Datos Compartida con Seguridad a Nivel de Fila (RLS) Nativa**
*   **Implementación (SQL Server):** Utiliza `SESSION_CONTEXT('TenantId', @value)` y Políticas de Seguridad con predicados iTVF.
*   **Implementación (PostgreSQL):** Utiliza `SET LOCAL app.current_tenant = 'value'` y `CREATE POLICY` nativo.
*   **Por qué se eligió:** Alta densidad de empaquetado y baja sobrecarga administrativa manteniendo el aislamiento absoluto a nivel de motor.
*   **Alternativas Rechazadas:**
    *   *Base de datos por tenant*: Alto costo de infraestructura y grave sobrecarga administrativa al gestionar miles de bases de datos.
    *   *Esquema por tenant*: Se vuelve difícil de escalar y migrar cuando el número de tenants supera los 1,000, causando el agotamiento del pool de conexiones.

### 6.2 Mecanismo de Resolución de Tenant
*   **Herramienta Elegida:** **Interceptor de NestJS / Middleware de ASP.NET + Contexto de Sesión de Conexión de Base de Datos**
*   **Por qué se eligió:** Resuelve el `tenant_id` desde los claims de JWT o cabeceras `X-Tenant-ID` en el ingreso, y utiliza un wrapper de transacción de base de datos para inyectar dinámicamente el contexto del tenant en la sesión activa.
*   **Alternativas Rechazadas:**
    *   *Filtrado a nivel de aplicación*: Propenso a omisiones de desarrolladores (olvidar una cláusula `WHERE tenant_id = x`), lo que lleva a vulnerabilidades críticas de filtración de datos. RLS previene estáo a nivel de base de datos.

---

## 7. Infraestructura y Despliegue

### 7.1 Contenedorización
*   **Herramienta Elegida:** **Docker v25 con builds distroless multi-etapa**
*   **Por qué se eligió:** Reduce el tamaño de la imagen del contenedor al mínimo absoluto y elimina las utilidades estándar de shell, endureciendo significativamente los contenedores de producción contra exploits de ejecución remota.

### 7.2 Orquestáación
*   **Herramienta Elegida:** **Kubernetes (K8s v1.28+)**
*   **Por qué se eligió:** Estandariza el despliegue, el escalado y la auto-recuperación. Funciona de manera idéntica en nubes públicas (EKS, GKE) y clusters locales on-premise (MicroK8s, Rancher K3s, OpenShift).

### 7.3 Gestión de Configuración y Secretos
*   **Herramienta Elegida:** **HashiCorp Vault (OSS, Autohospedado)**
*   **Por qué se eligió:** Un almacén de secretos de grado empresarial, altamente seguro y autohospedable. Los secretos se inyectan dinámicamente en los pods de K8s a través de sidecars Vault Agent Injector, asegurando que las credenciales nunca se expongan en texto plano.
*   **Alternativas Rechazadas:**
    *   *Secretos estándar de K8s*: Almacenados en texto plano base64 dentro de etcd, lo cual es inseguro sin una integración compleja de KMS.

### 7.4 Helm Chart / Estrategia de Despliegue
*   **Herramienta Elegida:** **Charts parametrizados de Helm v3**
*   **Por qué se eligió:** Permite despliegues basados en paquetes, parametrizando todos los recursos de UMS mientras habilita intercambios fáciles de parámetros (por ejemplo, alternar MinIO local versus S3 en la nube) entre entornos.

---

## 8. Observabilidad

### 8.1 Estándar de Instrumentación
*   **Herramienta Elegida:** **OpenTelemetry (estándar W3C Trace Context)**
*   **Por qué se eligió:** Mandatorio por nuestáros no negociables. Garantiza que el código de la aplicación permanezca completamente neutral respecto al proveedor. Si cambiamos de Jaeger/Loki a Datadog o New Relic, no tenemos que modificar una sola línea de código de la aplicación.

### 8.2 Métricas
*   **Herramienta Elegida:** **Prometheus recolectando desde OpenTelemetry Collector**
*   **Por qué se eligió:** Agregador de métricas estándar, autohospedable y de rendimiento extremadamente alto para entornos Kubernetes.

### 8.3 Tracing Distribuido
*   **Herramienta Elegida:** **Jaeger (OSS, Autohospedado)**
*   **Por qué se eligió:** Motor de tracing distribuido altamente confiable y autohospedable que recibe spans de traza estándar de OpenTelemetry.

### 8.4 Agregación de Logs
*   **Herramienta Elegida:** **Grafana Loki (OSS, Autohospedado)**
*   **Por qué se eligió:** Sistema de agregación de logs extremadamente eficiente que utiliza las mismas etiquetas de metadatos que Prometheus, permitiendo una correlación fluida entre métricas y logs dentro de los dashboards de Grafana.

---

## 9. Seguridad

### 9.1 Auth e Identidad
*   **Herramienta Elegida:** **Resolutores OIDC/SAML federados con UMS Native BCrypt Store Fallback**
*   **Por qué se eligió:** Asegura la federación de identidad de grado empresarial (Okta, Keycloak, Azure AD) lista para usar a través de configuraciones OIDC/SAML, mientras mantiene una tabla de usuarios local segura con hash BCrypt para soportar operaciones locales on-premise.

### 9.2 Enfoque RBAC / ABAC
*   **Herramienta Elegida:** **RBAC jerárquico compilado en grafos de permisos de grano fino con evaluación de contexto ABAC**
*   **Por qué se eligió:** Satisface tanto el enrutamiento basado en roles (para el renderizado de UI) como la evaluación precisa de atributos (geofencing, umbrales de acción) requeridos por los portales de clientes integrados modernos.

### 9.3 Herramientas de Auditoría de Dependencias
*   **Herramienta Elegida:** **CLI de Snyk Open Source + `npm audit` / `dotnet list package --vulnerable` ejecutado en el pipeline de CI/CD**
*   **Por qué se eligió:** Bloquea las construcciones automáticamente si se detectan vulnerabilidades críticas (CVEs) en los paquetes npm o nuget, protegiendo la cadena de suministro de software.

---

## 10. Experiencia del Desarrollador

### 10.1 Configuración de Desarrollo Local
*   **Herramienta Elegida:** **Especificación Docker Compose**
*   **Por qué se eligió:** Permite a los desarrolladores levantar toda la suite de dependencias de UMS (**SQL Server**, **PostgreSQL**, Redis, RabbitMQ, MinIO) localmente con un solo comando (`docker compose up -d`).

### 10.2 Monorepo vs. Multi-repo
*   **Herramienta Elegida:** **Nx Monorepo**
*   **Por qué se eligió:** Simplifica la gestión de dependencias, permite compartir tipos de TypeScript entre el frontend y el backend instantáneamente, y utiliza el almacenamiento en caché de construcción avanzado para minimizar los tiempos de compilación de CI.

### 10.3 Pirámide de Pruebas
*   **Herramienta Elegida:**
    *   *Pruebas Unitarias*: xUnit (.NET) / Jestá (Node) (objetivo >80% de cobertura).
    *   *Pruebas de Integración*: Testácontainers (levantando instancias efímeras de SQL Server/PostgreSQL en Docker local para pruebas realistas).
    *   *End-to-End*: Playwright para pruebas de regresión de la Consola Web.

---

## 11. Servicios de Terceros

Para evitar el bloqueo de proveedores de nube y soportar entornos fuera de línea (offline) y on-premise, **es mandatorio el uso de cero integraciones SaaS externas**. Las integraciones opcionales estáán completamente abstraídas tras Puertos de Dominio.

| Nombre del Servicio | Propósito | Por qué NO Internamente | Alternativa Agnóstica de Nube | Interfaz de Dominio |
| :--- | :--- | :--- | :--- | :--- |
| **Twilio** | Entrega de SMS OTP | Los gateways de operadoras de telecomunicaciones requieren acuerdos globales complejos. | Gateway local SMTP-a-SMS o Módem SMS autohospedado | `ISmsPort` |
| **SendGrid** | Emails Transaccionales | Gestionar la reputación de IP y las colas de entrega de correo es una carga operativa masiva. | Servidor SMTP Postfix / Haraka autohospedado | `IEmailPort`
## 12. Registro de Riesgo de Bloqueo de Proveedor (Vendor Lock-in)

| Componente | Solución Elegida | Riesgo de Bloqueo | Estrategia de Mitigación | Disparador de Re-evaluación |
| :--- | :--- | :--- | :--- | :--- |
| **Base de Datos** | SQL Server / PostgreSQL | **Medio** | Cumplimiento estándar de SQL. La capa de dominio estáá desacoplada mediante Puertos. | Cambios drásticos en el licenciamiento |
| **Almacén de Objetos** | MinIO | **Bajo** | MinIO utiliza exactamente el contrato de API de AWS S3. | Cuellos de botella de rendimiento |
| **Almacén de Secretos**| HashiCorp Vault | **Bajo** | La resolución de secretos se abstrae mediante inyección de secretos de K8s. | Cambios en el modelo de licenciamiento |
| **Gateway** | Kong Gateway | **Bajo** | La configuración se gestiona mediante recursos estándar de Ingress de K8s. | Restáricciones de enrutamiento personalizadas
## 13. Log de Decisiones

### Decisión 1: Estrategia de Backend Polyglot
*   **Decisión:** **.NET 8 para el Núcleo Empresarial; Node.js para Servicios Satélite.**
*   **Racional:** Equilibra los requisitos de alto rendimiento/seguridad para el núcleo con la velocidad de desarrollo para utilidades.
*   **Revisar cuando:** Las habilidades organizativas cambien significativamente o se requiera un modelo de runtime único unificado.

### Decisión 2: Selección de Base de Datos Impulsada por el Lenguaje (ADR 0026)
*   **Decisión:** **SQL Server para .NET; PostgreSQL/Mongo para Node.**
*   **Racional:** Aprovecha las fortalezas nativas del driver y el ecosistema de cada plataforma.
*   **Revisar cuando:** El intercambio de datos entre servicios requiera un único motor unificado o los costos de licencia excedan el presupuesto.

---

## 14. Preguntas Abiertas

1.  **Gateways de SMS On-premise:** ¿Qué hardware de SMS local o proveedores de telecomunicaciones estáán pre-aprobados por clientes empresariales localizados?
    *   *Información Necesaria:* Contratos activos de SMS locales o especificaciones de hardware de gateway SMS.
    *   *Resolutor BMAD:* **Agente de Dev / Agente de Infra** durante el onboarding de la Fase 05.
2.  **Registro Dinámico de OIDC:** ¿Deberían las instalaciones on-premise soportar registros dinámicos de clientes OIDC, o deben configurarse estááticamente mediante Helm?
    *   *Información Necesaria:* Capacidades de onboarding de la infraestructura de TI del cliente.
    *   *Resolutor BMAD:* **Product Owner / Arquitecto de Soluciones** durante los despliegues piloto.
