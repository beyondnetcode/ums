# ADR 0029: Administración Delegada y Flujos de Aprobación

## Estatus
Aprobado

## Contexto
A medida que la empresa escala, los Administradores de Inquilinos (Tenant Admins) no pueden gestionar manualmente cada asignación de perfil y usuario. Además, la introducción de identidades externas (B2B, EXTRANET, PARTNER) plantea un riesgo de seguridad significativo si se incorporan sin supervisión. El sistema requiere un mecanismo para que los administradores deleguen de manera segura las tareas de gestión y apliquen aprobaciones obligatorias para operaciones de identidad de alto riesgo.

## Decisión
Implementaremos un framework de **Gobernanza de Identidad y Aprobaciones** integrado directamente en el modelo de autorización:

1.  **Administración Delegada Muchos-a-Muchos**:
    *   Utilizaremos una tabla de asociación dedicada `USER_MANAGEMENT_DELEGATION` en lugar de una auto-referencia recursiva.
    *   **Regla**: Esto permite que un usuario sea gestionado por múltiples administradores (ej. un administrador local de B2B y un administrador global del sistema).
    *   **Alcance**: La delegación puede limitarse a un `SuiteId` específico, permitiendo que un administrador gestione a un usuario solo para ciertos sistemas.
    *   **Principio del Menor Privilegio**: Cuando un administrador asigna un `Profile` (Rol) a un usuario gestionado, el sistema debe verificar que los permisos efectivos del propio administrador (`ProfilePermission`) igualen o superen los permisos que se están otorgando.

2.  **Categorización de Usuarios**:
    *   Los usuarios se categorizan estrictamente (`INTERNAL`, `EXTERNAL`, `B2B`, `PARTNER`, etc.) a través del atributo `UserCategory`.

3.  **Flujos de Aprobación Configurables**:
    *   Se introduce un motor dinámico de aprobaciones (`APPROVAL_WORKFLOW`, `APPROVAL_REQUEST`, `APPROVAL_LOG`).
    *   Los flujos de trabajo se pueden configurar por `TenantId`, `SuiteId` o `TargetUserCategory`.
    *   **Aprobaciones de Doble Alcance**:
        *   *Alcance de Onboarding*: Activar a un usuario (ej. un usuario `EXTERNAL` requiere aprobación de RRHH antes de que su estado sea `ACTIVE`).
        *   *Alcance de Asignación*: Otorgar un `Profile` sensible (ej. otorgar "Administrador de Sistema" requiere doble aprobación).
    *   **Evidencia de Respaldo**: Los flujos pueden requerir documentación obligatoria (ej. "Acuerdo de Servicio B2B"). Los metadatos y punteros (URIs) se almacenan en `APPROVAL_ATTACHMENT`, mientras que los archivos residen en un servidor de archivos dedicado.

4.  **Rastro de Auditoría Inmutable**:
    *   Todos los pasos de aprobación (APPROVE, REJECT) y los cambios de delegación se registran de forma inmutable en `APPROVAL_LOG` utilizando el esquema corporativo estándar de 10 columnas.

## Consecuencias
*   **Positivo**: Reduce drásticamente el cuello de botella operativo sobre los Administradores de Inquilinos raíz. Garantiza el cumplimiento de SOX/ISO27001 al exigir rastros de aprobación para el acceso B2B/Externo. Aplica dinámicamente el Principio del Menor Privilegio.
*   **Negativo**: Aumenta la complejidad en la API de incorporación (Onboarding) y en la UI de Gestión de Usuarios, ya que deben manejar estados `PENDING` y disparadores de flujos de trabajo.
