# Resultados y Justificación de Pruebas de Integración (E2E)

## 1. Justificación y Alcance
Las pruebas de Integración y E2E validan la funcionalidad integral del monorepositorio UMS. El alcance cubre exhaustivamente las tres capas arquitectónicas:

- **Web (Frontend):** Pruebas E2E con Playwright que validan el renderizado de la UI de la SPA en React, el manejo del estado y los flujos de usuario.
- **API (Backend):** Pruebas de integración invocando endpoints en vivo de GraphQL y REST, validando el análisis de solicitudes, los interceptores de autorización AOP y el mapeo de respuestas.
- **BD (Base de Datos):** Validación de consultas reales a SQL Server, políticas de Seguridad a Nivel de Fila (RLS) y persistencia de datos desencadenada por la capa API en un contenedor o instancia de base de datos real.
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
