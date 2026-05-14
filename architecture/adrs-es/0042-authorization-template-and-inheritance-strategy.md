# ADR 0042: Estrategia de Plantillas de Autorización y Herencia

## Estatus
Propuesto

## Contexto
El Sistema de Gestión de Usuarios (UMS) actual requiere la configuración manual de roles y permisos para cada inquilino (tenant). A medida que crece el número de sistemas (Suites) e inquilinos, este enfoque se vuelve operativamente costoso y propenso a errores. Existe la necesidad de un mecanismo para definir "Plantillas de Permisos" estandarizadas y reutilizables que puedan ser heredadas o utilizadas como esquemas base para crear roles y perfiles específicos de los inquilinos.

## Decisión
Implementaremos una **Estrategia de Autorización Basada en Plantillas** basada en los siguientes principios:

1.  **Jerarquía Desacoplada**:
    *   **Sistema/Suite**: El nivel más alto de agrupación para permisos funcionales.
    *   **Plantillas de Permisos**: Conjuntos de autorizaciones predefinidas, globales o específicas de un inquilino.
    *   **Roles/Perfiles**: Implementaciones específicas de un inquilino que pueden heredar de una plantilla.

2.  **Modelo de Herencia Híbrido**:
    *   **Inicialización Estática (Esquema)**: Copia de los permisos de la plantilla a un rol en el momento de la creación.
    *   **Vinculación Dinámica (Vinculado)**: Mantenimiento de un vínculo a una versión de la plantilla donde los cambios en la plantilla pueden propagarse a los roles vinculados (gobernado por una política de "Sincronización").

3.  **Plantillas con Alcance**:
    *   **Plantillas de Sistema (Globales)**: Proporcionadas por la plataforma (ej. "Auditor Completo", "Empleado Base").
    *   **Plantillas de Inquilino (Locales)**: Creadas por un inquilino para su propia estructura interna.

4.  **Versionado**:
    *   Las plantillas admitirán el versionado semántico. Los roles pueden anclarse a una versión específica de una plantilla para garantizar la estabilidad.

## Implementación Técnica
*   **Modelo de Datos**: Introducción de las tablas `PermissionTemplates` y `TemplatePermissions`.
*   **Desacoplamiento**: Los permisos se categorizan por `SystemSuiteId`.
*   **Aislamiento**: Las plantillas globales tendrán un `TenantId = NULL` (o un UUID reservado por el sistema), mientras que las plantillas locales estarán protegidas por RLS.

## Consecuencias
*   **Positivo**: Reducción significativa en el tiempo de configuración manual, consistencia entre inquilinos y auditoría más sencilla.
*   **Negativo**: Ligero aumento en la complejidad de la base de datos y la necesidad de un proceso de sincronización en segundo plano para propagar actualizaciones de plantillas.
