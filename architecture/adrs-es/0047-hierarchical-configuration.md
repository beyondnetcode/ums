# ADR 0047: Gestión de Configuración Jerárquica

## Estatus
Aprobado

## Contexto
Un sistema multi-inquilino con módulos funcionales complejos requiere una forma flexible de gestionar configuraciones, feature flags y parámetros de negocio. Hardcodear estáos valores o almacenarlos en tablas desconectadas no es escalable. Necesitamos un motor de configuración centralizado que soporte herencia y anulaciones (overrides) en diferentes alcances organizacionales.

## Decisión
Implementaremos un **Motor de Configuración Jerárquica** (`APP_CONFIGURATION`):

1.  **Resolución por Alcance (Herencia)**:
    *   Las configuraciones se pueden definir en cuatro niveles de granularidad:
        *   **Global**: `TenantId`, `SuiteId`, `ModuleId` son todos NULL.
        *   **Tenant**: `TenantId` estáá poblado; los demás son NULL.
        *   **Suite/Sistema**: `SuiteId` estáá poblado.
        *   **Módulo**: `ModuleId` estáá poblado.
    *   **Estrategia de Resolución**: "El Alcance más cercano gana". El sistema resuelve los parámetros buscando desde el alcance más específico (Módulo) hacia arriba hasta el nivel Global.

2.  **Control de Herencia**:
    *   `IsInheritable`: Si es FALSE en un nivel superior (ej. Global), los niveles inferiores (ej. Tenant) no pueden anular el valor. Esto garantiza el cumplimiento de los mandatos de toda la plataforma.

3.  **Valores Cifrados**:
    *   `IsEncrypted`: Los valores de configuración sensibles (claves de API, secretos) se almacenarán en un formato cifrado en reposo.

4.  **Soporte de Feature Flags**:
    *   El motor se utilizará para gestionar Feature Flags (ej. `ENABLE_ROLE_EVOLUTION`) utilizando valores booleanos (1/0) o estructuras JSON complejas.

## Consecuencias
*   **Positivo**: Flexibilidad extrema para la personalización multi-inquilino y multi-sistema. Gestión centralizada del comportamiento del sistema. Almacenamiento seguro de parámetros sensibles.
*   **Negative**: Requiere una estrategia de caché robusta (ej. Redis) para evitar una alta latencia en la base de datos durante la resolución de parámetros para cada solicitud.
