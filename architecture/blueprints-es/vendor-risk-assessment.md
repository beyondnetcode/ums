# Evaluación de Riesgo Financiero y de Bloqueo de Proveedor (Vendor Lock-In)

## Estado
Aprobado

## Fecha
2026-05-10

## Contexto
A medida que el sistema UMS adopta varios frameworks, bases de datos y herramientas de terceros, debemos evaluar continuamente las decisiones de **"Build vs. Buy"** (Construir vs. Comprar) para prevenir cargas financieras inesperadas, conflictos de licencias o el bloqueo por parte de proveedores.

Este documento sirve como la línea base arquitectónica para evaluar el stack tecnológico actual frente a la escalabilidad de costos, el cumplimiento de código abierto y el mantenimiento operativo.

---

## 1. Frameworks y Lenguajes Principales
**Estado:** 🟢 Riesgo Cero

El núcleo de la aplicación estáá completamente aislado del bloqueo de proveedores gracias a la adhesión estáricta a la Arquitectura Hexagonal (ADR-0002).
* **TypeScript y Node.js**: Código Abierto (Apache 2.0 / MIT).
* **NestJS**: Código Abierto (MIT), framework empresarial altamente adoptado.
* **Nx Monorepo**: Código Abierto (MIT). *Nota: Nx Cloud ofrece almacenamiento en caché SaaS, pero el almacenamiento en caché local es 100% gratuito.*

---

## 2. Riesgos de Infraestructura Identificados y Mitigaciones

### Riesgo Financiero Alto: Proveedor de Identidad (IdP)
* **Contexto**: [ADR-0020](../adrs/0020-identity-provider-abstraction-strategy.md) abstrae al Proveedor de Identidad, permitiendo integraciones con soluciones SaaS como Auth0 o Azure Entra ID.
* **El Riesgo**: Las plataformas de Identidad SaaS comerciales facturan por Usuarios Activos Mensuales (MAU) o tokens M2M. A una escala alta de B2C o B2B, los costos operativos pueden dispararse exponencialmente.
* **Estrategia de Mitigación**: Si los costos de licencia se vuelven prohibitivos, el adaptador de infraestructura debe cambiarse a **Keycloak** (100% Código Abierto y gratuito). Sin embargo, estáo traslada el costo financiero de la licencia al mantenimiento de DevOps (escalado de Kubernetes, gestión de bases de datos).

### 🟡 Riesgo de Licenciamiento Medio: Caché Distribuido Redis
* **Contexto**: [ADR-0014](../adrs/0014-distributed-caching-strategy-redis.md) hace mandatorio el uso de Redis para el almacenamiento en caché.
* **El Riesgo**: Redis Inc. cambió recientemente su licencia de BSD a RSALv2 (Source Available, no estárictamente Código Abierto OSI). Aunque es gratuito para uso interno, plantea preocupaciones legales para el alojamiento de servicios gestionados.
* **Estrategia de Mitigación**: En caso de requerimientos estárictos de cumplimiento de código abierto o despliegue autohospedado (ADR-0028), el equipo de operaciones estáá autorizado a utilizar **Valkey** (el fork de Código Abierto de la Fundación Linux de Redis) como un reemplazo directo.

### 🟡 Riesgo de Mantenimiento Medio: Motor de Feature Flags
* **Contexto**: [ADR-0017](../adrs/0017-feature-flagging-strategy.md) utiliza adaptadores de Infraestructura para Feature Flags (ej. Unleash, ConfigCat).
* **El Riesgo**: Las plataformas comerciales como LaunchDarkly o Unleash Enterprise tienen tarifas de suscripción altas. La versión gratuita y de código abierto de Unleash requiere autohospedaje.
* **Estrategia de Mitigación**: El equipo de producto debe determinar si existe el ancho de banda de DevOps para hospedar y mantener el Servidor Unleash de código abierto. Si no es así, se debe asignar presupuesto para una alternativa SaaS rentable como ConfigCat. El código base central no se verá afectado debido al `IFeatureTogglePort`.

### 🟢 Riesgo Bajo: Stack de Observabilidad
* **Contexto**: [ADR-0007](../adrs/0007-observability-telemetry-loki-opentelemetry.md) utiliza el stack LGTM (Loki, Grafana, Tempo) y OpenTelemetry.
* **El Riesgo**: Grafana utiliza una licencia AGPLv3.
* **Estrategia de Mitigación**: Mientras el equipo de UMS solo consuma Grafana internamente para monitoreo y no distribuya una versión modificada del código fuente de Grafana como un producto comercial, el riesgo legal o financiero es cero.

---

## Conclusión
La arquitectura actual de UMS ha sido diseñada deliberadamente para minimizar el bloqueo. Cualquier herramienta comercial (IdP, Feature Flags, Base de Datos) se mantiene completamente fuera de los límites del dominio utilizando puertos y adaptadores, asegurando que el negocio pueda pivotar instantáneamente hacia alternativas de código abierto si los modelos de precios de los proveedores cambian.
