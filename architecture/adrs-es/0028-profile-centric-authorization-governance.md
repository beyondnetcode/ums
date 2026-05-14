# ADR 0028: Modelo de Gobernanza y Autorización Impulsado por Plantillas Maestras

## Estatus
Refactorizado (Gobernanza de Plantillas Maestras)

## Contexto
Los modelos de autorización estándar a menudo permiten permisos "ad-hoc" a nivel de usuario o perfil, lo que lleva a una deriva en la gobernanza y pesadillas de auditoría. Los entornos empresariales requieren una única fuente inmutable de verdad para toda autoridad posible: la **Plantilla de Permisos**.

## Decisión
Implementaremos un **Framework de Autorización Impulsado por Plantillas Maestras** gobernado por las siguientes reglas inmutables:

1.  **Plantilla de Permisos como Fuente Maestra**:
    *   Ningún permiso puede existir en un Perfil si no ha sido definido previamente en el catálogo maestro `PERMISSION_TEMPLATE`.
    *   Las plantillas definen la intersección granular de un **Recurso** (Módulo/Menú/Opción) y una **Acción** (Ver/Crear/Exportar/etc.).

2.  **Materialización vía ProfilePermission**:
    *   La autoridad se "materializa" desde una Plantilla hacia una entrada de **ProfilePermission**.
    *   `ProfilePermission` almacena el **Estado Efectivo** utilizando una lógica de triple estado: `IsAllowed`, `IsDenied`, `IsActive`.
    *   Esto admite tanto RBAC (herencia) como ABAC-lite (anulaciones/denegaciones).

3.  **Jerarquía Estricta**:
    *   Cada permiso pertenece a un **Sistema (Suite)**.
    *   Las plantillas se agrupan por **Módulos Funcionales**.

4.  **Gobernanza y Auditoría**:
    *   Cada entidad implementa el **Esquema de Auditoría Corporativa** (más de 10 columnas).
    *   El `Motor de Autorización` resuelve la autoridad efectiva consultando la tabla materializada `ProfilePermission`, garantizando un rendimiento de resolución de un solo salto.

## Implementación Técnica
*   **Cardinalidad**: `Sistema (1:N) Módulo (1:N) Plantilla (1:N) PermisoDePerfil`.
*   **Persistencia**: `ProfilePermission` es una copia física de la autoridad de la plantilla para un Perfil específico, lo que permite anulaciones no destructivas.

## Consecuencias
*   **Positivo**: Control absoluto sobre el catálogo de permisos, historial 100% auditable, soporte para denegaciones explícitas y resolución de alto rendimiento.
*   **Negativo**: Requiere un proceso de sincronización cuando se actualiza la Plantilla Maestra.
