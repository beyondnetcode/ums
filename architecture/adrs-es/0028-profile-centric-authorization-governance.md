# ADR 0028: Modelo de Gobernanza de Acciones con Alcance y Autorización Impulsada por Plantillas

## Estatus
Refactorizado (Propiedad de Acciones Jerárquicas)

## Contexto
Iteraciones anteriores permitían acciones que potencialmente podían existir fuera de un contexto funcional claro. Para garantizar una gobernanza de nivel empresarial, cada acción autorizada debe estar estrictamente vinculada a un **Sistema** (Acciones globales) o a un **Módulo Funcional** (Acciones específicas).

## Decisión
Implementaremos la **Gobernanza de Acciones con Alcance** dentro del Framework de Plantillas Maestras:

1.  **Propiedad Estricta de Acciones**:
    *   Cada `ACTION` debe pertenecer a un `SYSTEM_SUITE` (Global) o a un `FUNCTIONAL_MODULE` (Específica).
    *   Las acciones huérfanas (sin un padre de sistema o módulo) están estrictamente prohibidas.

2.  **Ruta de Resolución**:
    *   La autorización sigue una ruta jerárquica obligatoria:
        `Tenant -> Sistema -> Módulo -> Recurso -> Acción -> Plantilla -> PermisoDePerfil`.

3.  **Unión de Plantilla (Junction)**:
    *   `PermissionTemplate` actúa como la unión autorizada entre un **Recurso** (Sistema, Módulo, Menú, SubMenú, Opción) y una **Acción con Alcance**.

4.  **Sin Deriva Efectiva**:
    *   `ProfilePermission` es una instancia materializada de una `PermissionTemplate`.
    *   La asignación manual de acciones a perfiles sin una plantilla es imposible por diseño.

## Implementación Técnica
*   **Entidad Action**: Implementa `SystemId` y `ModuleId` (al menos uno debe ser no nulo).
*   **Mapeo de Recursos**: Las plantillas mapean recursos a acciones con alcance.
*   **Auditoría**: Todos los cambios de propiedad se auditan completamente con el esquema corporativo de 10 columnas.

## Consecuencias
*   **Positivo**: Elimina la ambigüedad funcional, garantiza que cada permiso tenga un propietario claro y simplifica la auditoría.
*   **Negativo**: Esquema ligeramente más complejo debido a la propiedad condicional de las acciones.
