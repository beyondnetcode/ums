# ADR 0028: Modelo de Gobernanza y Autorización Centrado en el Perfil Enterprise

## Estatus
Refactorizado (Estándares Enterprise)

## Contexto
Los modelos RBAC estándar fallan en entornos empresariales multi-inquilino y multi-sistema donde los permisos están contextualizados por la estructura organizacional (Sucursales) y los límites funcionales (Sistemas). La asignación directa usuario-rol crea brechas de gobernanza. Un modelo robusto debe aislar los roles dentro de los sistemas y consolidar la autoridad en un único pivote contextual: el **Perfil**.

## Decisión
Implementaremos un **Modelo de Autorización Centrado en el Perfil Enterprise** gobernado por las siguientes reglas estrictas:

1.  **Propiedad Jerárquica Estricta**:
    *   Un **Inquilino (Tenant)** posee sus **Usuarios**, **Sistemas (Suites)** y **Sucursales (Branches)**.
    *   Un **Sistema** pertenece a un único Inquilino.
    *   Un **Rol** pertenece a un único Sistema. Los roles globales están prohibidos.
    *   Un **Permiso** pertenece al contexto funcional de un Sistema.

2.  **El Perfil como Nexo Contextual**:
    *   El **Perfil** es la intersección única de: `Tenant` + `Sistema` + `Sucursal` + `Usuario` + `Rol`.
    *   Las autorizaciones se resuelven y persisten a nivel de **Perfil** como "Permisos Efectivos".

3.  **Estándares de Gobernanza y Auditoría**:
    *   Cada entidad debe implementar el **Esquema de Auditoría Corporativa** (más de 10 columnas).
    *   El soporte para **Soft Delete**, **Bloqueo Optimista** y **Pistas de Auditoría** es obligatorio para todas las operaciones de persistencia.
    *   **Trazabilidad**: Las operaciones críticas de seguridad deben rastrear `CorrelationId`, `AuditId` y `TransactionId`.

4.  **Motor de Autorización**:
    *   El motor resuelve los permisos consultando la tabla de **Permisos Efectivos** para el `ProfileId` actual.
    *   Admite **Anulaciones (Overrides)** (Conceder/Denegar) a nivel de Perfil para una granularidad máxima.

## Implementación Técnica
*   **Cardinalidad**: `Tenant (1:N) Sistema (1:N) Rol (1:N) Perfil`.
*   **Persistencia**: Los permisos efectivos se proyectan desde `Role -> ProfilePermission` al crear/sincronizar el perfil.

## Consecuencias
*   **Positivo**: Aislamiento perfecto, resolución contextual simplificada, auditabilidad total y escalabilidad masiva para entornos multi-organización.
*   **Negativo**: Mayor gestión de metadatos y requerimiento de una lógica de sincronización robusta entre Roles y Perfiles.
