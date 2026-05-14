# ADR 0028: Modelo de Gobernanza y Autorización Centrado en el Perfil

## Estatus
Propuesto

## Contexto
El modelo de autorización anterior dependía de una asociación directa entre Usuarios, Roles y Permisos. Esto generaba inconsistencias conceptuales donde las anulaciones de permisos para un usuario específico requerían la creación de un nuevo rol o la gestión de asignaciones ad-hoc, complicando la gobernanza y la auditoría. Existe la necesidad de una entidad central que encapsule todo el contexto de una autorización (Usuario, Rol, Sistema, Sucursal e Inquilino) y sirva como contenedor para las "Autorizaciones Efectivas".

## Decisión
Haremos la transición a un **Modelo de Autorización Centrado en el Perfil**.

1.  **El Perfil como Eje de Gobernanza**:
    *   Un **Perfil** es la unidad atómica de autorización.
    *   Es una composición contextual de: `UserId` + `RoleId` + `SystemId` + `BranchId` + `TenantId`.
    *   Un usuario puede tener múltiples perfiles (ej. "Gerente en Sucursal A" y "Auditor en Sucursal B").

2.  **Persistencia de Autorizaciones Efectivas**:
    *   En lugar de resolver los permisos al vuelo desde los roles, el conjunto final de autorizaciones se persistirá a nivel de **Perfil** en una tabla `ProfilePermissions`.
    *   Esto permite **Anulaciones (Overrides)** granulares (agregar o eliminar permisos) para un perfil específico sin afectar a otros usuarios que comparten el mismo Rol.

3.  **Estándares de Trazabilidad y Auditoría**:
    *   Todos los cambios de autorización deben rastrearse con un `TransactionId` o `AuditId`.
    *   Cada entidad en el sistema seguirá el **Esquema de Auditoría Corporativa Estándar** (Created, Updated, Deleted, Version, Status).

4.  **Desacoplamiento**:
    *   **Rol**: Un esquema base de permisos.
    *   **Perfil**: Una implementación contextual de un Rol para un Usuario y una Sucursal específicos.

## Implementación Técnica
*   La tabla `Users` ya no estará vinculada directamente a `Roles`.
*   La tabla `Profiles` se convierte en el punto de unión central.
*   El `Motor de Autorización` resolverá los permisos consultando el **ID del Perfil Activo** en el contexto de ejecución.

## Consecuencias
*   **Positivo**: Granularidad perfecta para anulaciones, auditoría simplificada (un solo lugar para ver qué puede hacer un usuario en un contexto específico) y mejor alineación con estructuras organizacionales complejas.
*   **Negativo**: Mayor número de filas en las tablas de permisos (`ProfilePermissions`) y necesidad de lógica para mantener los perfiles sincronizados con sus roles/plantillas de origen cuando sea necesario.
