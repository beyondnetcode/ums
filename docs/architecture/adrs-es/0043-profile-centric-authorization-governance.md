# ADR 0043: Gobernanza de Plantillas Maestáras Vinculadas al Rol y Jerarquía Profunda

## Estatus
Refactorizado (Gobernanza Vinculada al Rol)

## Contexto
Para garantizar la integridad funcional absoluta, las plantillas de permisos no deben existir como esquemas desconectados. Deben estáar estárictamente vinculadas a un **Rol** dentro de un **Sistema**, asegurando que la autoridad siempre se defina dentro de un contexto organizacional y funcional válido.

## Decisión
Implementaremos la **Gobernanza de Plantillas Vinculadas al Rol** con una jerarquía funcional profunda:

1.  **Cadena de Relación Estricta**:
    *   `Sistema (1:N) Rol (1:N) Plantilla (1:N) PermisoDePerfil`.
    *   Las plantillas son ahora una extensión de la definición de autoridad del Rol.

2.  **Jerarquía Funcional Profunda**:
    *   El modelo admite explícitamente la autorización en 6 niveles:
        1. **Sistema/Suite**
        2. **Módulo Funcional**
        3. **Menú/ÍtemMenú**
        4. **SubMenú** (si aplica)
        5. **Opción Funcional**
        6. **Acción** (Ver, Crear, etc.)

3.  **Propiedad de Acciones con Alcance**:
    *   Las acciones DEBEN pertenecer a un **Sistema** o **Módulo**. Sin huérfanos.
    *   Las acciones se reutilizan dentro de las plantillas en el contexto del Rol.

4.  **Materialización Efectiva**:
    *   `ProfilePermission` es un vínculo materializado a una **Plantilla Vinculada al Rol**, admitiendo anulaciones explícitas de `IsAllowed/IsDenied`.

## Implementación Técnica
*   **PermissionTemplate**: Clave foránea a `RoleId`.
*   **Mapeo de Jerarquía**: La identificación de recursos admite estructuras recursivas de menús y opciones.
*   **Auditoría**: Esquema corporativo obligatorio de 10 columnas para todas las entidades.

## Consecuencias
*   **Positivo**: Elimina conjuntos de autoridad desconectados, garantiza que los roles sean el pivote principal de gobernanza y admite requerimientos empresariales extremadamente granulares.
*   **Negativo**: Requerimientos de entrada de datos más estárictos (las plantillas deben crearse por Rol).
