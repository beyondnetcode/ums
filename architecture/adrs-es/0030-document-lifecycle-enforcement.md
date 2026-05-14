# ADR 0030: Aplicación Automática de Acceso basada en Ciclo de Vida Documental

## Estatus
Aprobado

## Contexto
En un entorno altamente regulado, el acceso al sistema debe estar condicionado a la validez de la documentación de respaldo (identidad, contratos B2B, certificados). El monitoreo manual de las fechas de expiración es ineficiente y propenso a errores. Necesitamos un sistema que alerte proactivamente a los usuarios y restrinja automáticamente el acceso cuando la documentación crítica expire.

## Decisión
Implementaremos un framework de **Aplicación de Políticas y Ciclo de Vida Documental**:

1.  **Repositorio Unificado de Documentos**:
    *   `USER_DOCUMENT` sustituye a los adjuntos simples, añadiendo `IssueDate`, `ExpirationDate` y `Status`.
    *   Los documentos se clasifican mediante `DOCUMENT_TYPE`, que define si el documento es "Crítico para el Acceso".

2.  **Motor de Notificación Paramétrico**:
    *   La tabla `NOTIFICATION_RULE` permite definir alertas de N-pasos (ej. 60, 30, 15, 5 días antes de la expiración).
    *   Las alertas se filtran por Inquilino (Tenant), Categoría de Usuario y Tipo de Documento.

3.  **Políticas de Aplicación Automática**:
    *   La tabla `ACCESS_ENFORCEMENT_POLICY` define la acción a tomar tras la expiración (ej. `BLOCK_USER`).
    *   **Lógica**: Si un `DOCUMENT_TYPE` marcado como `IsAccessCritical` expira, se dispara la política asociada.

4.  **Arquitectura Desacoplada Basada en Eventos**:
    *   **Worker/Scheduler**: Escanea documentos diariamente y publica eventos (`DocumentNearExpirationEvent`, `DocumentExpiredEvent`).
    *   **Servicio de Notificación**: Consume eventos de expiración y despacha alertas multicanal.
    *   **Motor de Políticas**: Consume `DocumentExpiredEvent` e interactúa con el `IdentityService` para bloquear cuentas o restringir perfiles.

## Consecuencias
*   **Positivo**: Garantiza el cumplimiento del 100% de los requisitos de gobernanza de identidad. Reduce la carga operativa para los administradores de inquilinos. Proporciona un rastro de auditoría claro de por qué se restringió el acceso.
*   **Negativo**: Los usuarios pueden ser bloqueados inesperadamente si ignoran múltiples notificaciones. Requiere una lógica robusta de "Periodo de Gracia" y flujos de renovación sencillos en la interfaz de usuario.
