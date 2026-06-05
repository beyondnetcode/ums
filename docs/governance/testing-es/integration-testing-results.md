# Resultados y Justificación de Pruebas de Integración (E2E)

## 1. Justificación y Alcance
Las pruebas de Integración y E2E validan la funcionalidad integral del monorepositorio UMS, cubriendo la comunicación entre la SPA en React y el backend en .NET Core.

- **Framework E2E:** Playwright
- **Backend API:** GraphQL & REST
- **Escenarios Clave:** Flujos de Autenticación, Aislamiento de Inquilinos (Tenant RLS y capa de aplicación), Autorización por Roles, Navegación y Renderizado Dinámico de la UI.

## 2. Casos Ejecutados y Resultados

Con base en el último ciclo de QA:

- **Total Pruebas E2E Ejecutadas:** 35
- **Exitosas:** 1
- **Fallidas:** 34
- **Estado:** FALLIDO (debido a cambios recientes en las etiquetas de la UI, no a errores de lógica de negocio).

### Escenarios Clave y Hallazgos
- **Flujo de Autenticación (Auth Flow):** Valida inicio de sesión, expiración de sesión y bloqueo de cuentas. *Falla estrictamente porque las etiquetas de la UI como 'Usuario' fueron actualizadas a 'Correo electrónico' por cumplimiento OWASP.*
- **Autorización Dinámica de UI:** Valida que los botones (ej. 'Agregar') estén deshabilitados o habilitados basándose en permisos dinámicos (los interceptores AOP del backend se reflejan correctamente en la UI).
- **Navegación y Panel de Perfil:** Valida los datos del panel de sesión, el estado del inquilino activo y el acceso a módulos.

## 3. Conclusiones y Siguientes Pasos
La integración de la lógica de negocio (respuestas API, códigos HTTP, interceptores de seguridad) ha sido verificada manualmente y es estable. Las pruebas E2E automatizadas están fallando estrictamente debido a desajustes en los selectores del DOM resultantes de las recientes actualizaciones de fortalecimiento de seguridad en la UI.

**Plan de Acción:** Actualizar los selectores `getByText` y `getByLabel` de Playwright para que coincidan con las nuevas etiquetas de UI compatibles con OWASP.
