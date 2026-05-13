# Reporte Maestro: Evaluación de Gobernanza Multi-Tenant y Estructura Organizacional

*   **Estado:** Finalizado
*   **Fecha:** 2026-05-13
*   **Área:** Arquitectura Enterprise & Product Ownership

---

## 🔬 1. Evaluación del Dominio Actual y Hallazgos

Tras un análisis exhaustivo del ecosistema de UMS, hemos identificado un **acoplamiento estructural rígido** en el diseño original que impacta negativamente la evolución SaaS del producto.

### Principales Hallazgos Arquitectónicos
1.  **Acoplamiento de Identidad (Empleado vs Sujeto):** La dependencia de `identity_reference` como campo mandatorio asume una relación laboral interna. Esto bloquea escenarios B2B donde el usuario es un proveedor externo, un transportista tercero o un bot de integración (M2M) que carece de registro en la base de datos de RRHH corporativa.
2.  **Activos de Software Huérfanos:** Los sistemas (ERP, CRM, HCM) y sus componentes (Menus, APIs) operan actualmente en un catálogo global "plano". No existe una definición explícita de quién es el dueño lógico del recurso y bajo qué condiciones un tercero (Tenant) puede consumirlo.
3.  **Límites de Seguridad Ambiguos:** El modelo de permisos se enfoca en el "Qué" (acción) pero no en el "Quién es el dueño del objeto". Esto vulnera principios de Zero Trust donde la Organización debería ser la frontera física y lógica de cada bit de información.

---

## 🎯 2. Modelo de Dominio Objetivo (Enterprise-Grade)

Proponemos una reestructuración donde la **Organización** se eleva como el **Aggregate Root Estratégico** de todo el ecosistema.

### Jerarquía Recomendada
*   **Organization (Tenant):** El límite absoluto de gobernanza, aislamiento y propiedad.
    *   **Identidades (Subjects):** Personas, Actores y Service Accounts bajo tutela de la Org.
    *   **Recursos Tecnológicos:** Sistemas, Aplicaciones y APIs propiedad de la Org.
    *   **Gobierno:** Roles, Permisos, Políticas ABAC y Workflows de aprobación locales.
    *   **Trazabilidad:** Logs de auditoría inmutables aislados por Organización.

---

## 🛡️ 3. Estrategia Multi-Tenant e IAM (Identity & Access Management)

Para cumplir con requerimientos de escala empresarial y seguridad **Zero Trust**, el sistema adoptará el siguiente modelo:

### Aislamiento de Datos (RLS + Tenant-Aware APIs)
Utilizaremos un modelo **Shared Database / Shared Schema** potenciado por **PostgreSQL Row-Level Security (RLS)**. 
*   Cualquier consulta a nivel de base de datos será filtrada automáticamente por el motor de PostgreSQL usando la variable de sesión `app.current_organization_id`. 
*   Esto garantiza inmunidad total ante errores de programación (ej: olvidar un `WHERE` en el ORM), previniendo fugas de datos (*cross-tenant data leaking*).

### Modelo de Ownership de Sistemas
Los sistemas y aplicaciones pertenecen explícitamente a una organización.
*   **Owner Org:** Gestiona el código, el catálogo de menús y los endpoints de API.
*   **Consumer Org:** Suscribe el acceso al sistema mediante una **Relación Contractual de Delegación**, permitiendo que sus usuarios locales consuman el software sin que el dueño original pierda el control de la seguridad.

---

## 📊 4. Riesgos, Trade-offs y Mitigaciones

| Riesgo | Impacto | Estrategia de Mitigación |
| :--- | :--- | :--- |
| **God Entity (Organización)** | Alta complejidad en un solo objeto. | Desacoplar mediante Bounded Contexts. La Org es un ID compartido, pero su lógica está distribuida. |
| **Noisy Neighbor** | Degradación de performance. | Implementación de Rate Limiting y Quotas a nivel de API Gateway por `OrganizationId`. |
| **Complejidad de Migración** | Breaking changes en APIs existentes. | Estrategia de **Coexistencia y Deprecación** (Dual-Read/Write) detallada en el ADR-0031. |
| **Gobernanza de Service Accounts** | Acceso descontrolado M2M. | Inclusión de identidades no-humanas bajo la jerarquía de la Organización con rotación de secretos en Vault. |

---

## 🚀 5. Estrategia de Transición Incremental

1.  **Fase 01 (Cimentación):** Implementar la tabla de Organizaciones como frontera y asociar el 100% de los usuarios actuales a la "Organización Raíz".
2.  **Fase 02 (Desacoplamiento):** Migrar de `identity_reference` a `identity_reference` (Sujeto agnóstico). Inyectar `X-Org-Context` en el API Gateway.
3.  **Fase 03 (Enforcement):** Activar políticas RLS en PostgreSQL para todas las tablas de dominio.
4.  **Fase 04 (Federación):** Habilitar el módulo de Solicitudes B2B para que organizaciones externas soliciten acceso a sistemas internos de forma autónoma.

---

## ✅ 6. Conclusión de Validación End-to-End
Este modelo garantiza que el negocio pueda crecer de una plataforma corporativa cerrada a un **Ecosistema SaaS Multi-Tenant Federado**. La arquitectura cumple con:
*   **Seguridad:** Zero Trust y RLS.
*   **Negocio:** Flexibilidad para partners, clientes y proveedores.
*   **Datos:** Aislamiento físico y lógico de alta performance.
*   **Gobernanza:** Ownership claro de cada recurso y actor.
