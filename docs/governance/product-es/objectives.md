# Objetivos Estratégicos (OKRs) - Sistema de Gestión de Usuarios (UMS)

Para alinear la entrega técnica con los objetivos corporativos, el desarrollo del Sistema de Gestión de Usuarios (UMS) se rige por los siguientes **Objetivos y Resultados Clave (OKRs)** medibles:

---

## Objetivo 1: Entregar Seguridad de Grado Empresarial y Sin Contraseñas
Asegurar que la plataforma elimine los riesgos de seguridad tradicionales asociados con la gestión de identidad.

- **KR 1.1**: Lograr **Cero hashes de contraseñas locales almacenados** para usuarios federados, manteniendo un **Almacén de Credenciales Nativas** cifrado con bcrypt como respaldo interno opcional.
- **KR 1.2**: Soportar **Autenticación Multifactor (MFA) OBLIGATORIA** y WebAuthn (Passkeys) para el 100% de las cuentas administrativas en la salida a producción.
- **KR 1.3**: Superar todas las pruebas de penetración externas con **Cero vulnerabilidades Altas o Críticas** (cumpliendo con OWASP Top 10).

---

## Objetivo 2: Maximizar el Rendimiento de Construcción y Latencia de Autorización
Garantizar que las verificaciones de permisos centralizadas no degraden la experiencia del usuario en los sistemas cliente.

- **KR 2.1**: Mantener la latencia de recuperación del grafo de permisos compilado **por debajo de 50ms** usando Caché Redis con TTL < 1 hora.
- **KR 2.2**: Asegurar que el tiempo de construcción del monorepo sea **menor a 5 minutos** utilizando el almacenamiento en caché de tareas de alto rendimiento de Nx.
- **KR 2.3**: Lograr una **sobrecarga de RLS con EF Core / SQL Server de < 5ms** por ejecución de consulta SQL.

---

## Objetivo 3: Habilitar la Escalabilidad B2B mediante Autoservicio
Delegar tareas administrativas a los clientes, reduciendo la sobrecarga de soporte.

- **KR 3.1**: Reducir el tiempo de onboarding de nuevos clientes (tenants) de **días a < 10 minutos** a través de portales de registro de autoservicio.
- **KR 3.2**: Lograr **Cero tickets de soporte** para restablecimiento de contraseñas, ya que los usuarios gestionan sus propias credenciales a través de portales de autoservicio nativos o externos.
- **KR 3.3**: Habilitar la **Personalización Dinámica de Menús**, permitiendo a los administradores de clientes crear y asignar plantillas de permisos en tiempo real.

---

## Objetivo 4: Lograr una Extensibilidad Plug-and-Play Real (Núcleo Neutral)
Diseñar el sistema para que todas las infraestructuras externas (IdPs y Gestores de Feature Flags) sean completamente opcionales y conectables.

- **KR 4.1**: Entregar un **Motor de Respaldo Nativo** funcional tanto para Identidad (bcrypt/DB interna) como para Feature Flags (SQL Server como base de targets/evaluador) de fábrica, sin requerir dependencias SaaS externas.
- **KR 4.2**: Estandarizar el **100% de las integraciones externas** detrás de puertos hexagonales (`IAuthenticationPort` e `IFeatureFlagPort`), permitiendo la adición o cambio de proveedores (Zitadel, Azure AD, Okta, LaunchDarkly, Unleash) solo mediante configuración.
- **KR 4.3**: Hacer cumplir el uso de **Cero importaciones de SDK de proveedores externos** dentro de las capas puras de `core/` o `application/` de la aplicación.
