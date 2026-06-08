# Interesados y Usuarios del Proyecto - Sistema de Gestión de Usuarios (UMS)

Para asegurar el éxito del UMS, se han mapeado los siguientes **Interesados (Stakeholders)** internos y externos con sus respectivos roles, responsabilidades y expectativas principales:

---

## 1. Interesados Internos

| Rol del Interesado | Responsabilidad Principal | Expectativa Principal del UMS |
| :--- | :--- | :--- |
| **Arquitecto de Software Principal** | Diseño arquitectónico, gobernanza de límites hexagonales, seguridad y preparación para microservicios con Dapr. | Alto desacoplamiento, interfaces limpias (Puertos), cero vulnerabilidades y alineación perfecta con el modelo C4. |
| **Product Owner / Analista de Negocio** | Recolección de requisitos, definición de alcance, priorización de características e historias de usuario. | Onboarding de clientes (tenants) autoservicio, inyección dinámica de menús y lógica de asignación RBAC/ABAC intuitiva. |
| **Líder de Desarrollo** | Implementación de código en .NET 10/React 18 y pruebas unitarias. | Interfaces claras, seguridad de tipos, excelente soporte de Nx y endpoints de API bien documentados. |
| **Analista de QA / Seguridad** | Pruebas de contrato, pruebas de penetración locales, verificación de cobertura y quality gates. | Alta testabilidad del código, cumplimiento de contratos y registros de auditoría de negocio inmutables. |
| **Ingeniero de DevOps / SRE** | Topología de infraestructura, orquestación con Docker y pipelines de telemetría con Grafana Loki. | Builds de CI/CD rápidos (< 5m), alta observabilidad (OpenTelemetry) y aislamiento de datos fiable mediante PostgreSQL row-level security.
## 2. Usuarios Externos

| Persona de Usuario | Contexto | Beneficio Clave del UMS |
| :--- | :--- | :--- |
| **Admin de Tenant Cliente** | Administrador de TI en una empresa cliente B2B integrada (Tenant). | Autonomía completa de autoservicio para gestionar perfiles de empleados, roles y alcances de autorización sin tickets de soporte. |
| **Usuario Final B2B** | Empleado en una empresa cliente (ej. operador de montacargas, planificador de fletes). | Login rápido y sin fricciones (Passkey/SSO) y un portal dinámico que muestra solo sus aplicaciones permitidas. | 
