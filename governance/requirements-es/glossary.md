# Glosario de Términos

Este documento establece el glosario de términos estándarizado y no ambiguo para el **Sistema de Gestión de Usuarios (UMS)** bajo la estrategia **BMAD-METHOD**.

## 1. Entidades de Identidad

| Término | Definición | Propietario SSoT |
| :--- | :--- | :--- |
| **Usuario** | Un operador humano único o cuenta de servicio registrado en el sistema. Tiene credenciales y Perfiles asignados. | `Identity.Users` |
| **Tenant (Inquilino)** | Un espacio de nombres organizativo asegurado e aislado que comparte la misma plataforma UMS física. Cada fila de base de datos está marcada con un `TenantId`. Los Tenants implementan Row-Level Security (RLS) para aislamiento a nivel de infraestructura. Mapea 1:1 a una Organización. | `Identity.Tenants` |
| **Organización** | Una empresa o entidad comercial registrada en UMS. Define un límite de seguridad y mapea a un Tenant. Puede ser la organización corporativa principal (`INTERNAL`, propietaria de la plataforma) o un actor externo (`CLIENT`, `SUPPLIER`, `PARTNER`) vinculado a identificadores ERP. El ID de Tenant de la Organización determina el aislamiento de datos. | `Identity.Organizations` |
| **Usuario Patrocinador** | Un usuario corporativo interno (de la Organización `INTERNAL`) que solicita y justifica el acceso al sistema para un usuario B2B externo a través del flujo de aprobación. | `Identity.Users` |
| **Solicitud de Acceso Externo** | Un boleto de negocio auditable que enruta una solicitud de incorporación de usuario B2B a través del flujo del Contexto de Aprobaciones. Iniciado por un Usuario Patrocinador. | `Approvals.Requests` |
| **Sede (Branch)** | Una sub-unidad física o lógica de una Organización (ej. *Terminal Portuario del Callao*, *Almacén de Lurín*). Está limitada a un único Tenant. Actúa como contexto de sede para enrutamiento de autorización jerárquica y aplicación de Row-Level Security. | `Identity.Branches` | ## 2. Entidades de Autorización

| Término | Definición | Propietario SSoT |
| :--- | :--- | :--- |
| **Sistema** | Una aplicación cliente externa registrada en el UMS (ej. TMS, WMS). | `Auth.Systems` |
| **Módulo** | Una agrupación lógica de funcionalidades dentro de un Sistema. | `Auth.Modules` |
| **Acción** | La unidad mínima de permiso (Create, Read, Update, Delete, etc.). | `Auth.Actions` |
| **Plantilla de Autorización** | Un conjunto predefinido de permisos que se puede asignar a múltiples sujetos. | `Auth.Templates` |
| **Role (Rol)** | Plano funcional nombrado que representa un conjunto de permisos dentro de un Sistema. Los Roles forman una jerarquía auto-referencial (`ParentRoleId`) que permite herencia. El Rol es la fuente desde la cual se derivan las Auth Templates; **no se asigna directamente a usuarios** — solo a través de un Profile. | `Auth.Roles`
*Este glosario es la Fuente Única de Verdad (SSoT) para el lenguaje ubicuo del proyecto.*
