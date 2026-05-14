# 📖 Glosario de Términos

Este documento establece el glosario de términos estandarizado y no ambiguo para el **Sistema de Gestión de Usuarios (UMS)** bajo la estrategia **BMAD-METHOD**.

## 1. Entidades de Identidad

| Término | Definición |
| :--- | :--- |
| **Sujeto (Subject)** | La representación abstracta de una identidad (Persona o Sistema) dentro del ecosistema. |
| **Organización** | El límite administrativo y de seguridad principal. Un Sujeto siempre pertenece a una Organización. |
| **Tenant** | Una instancia lógica aislada de datos y configuración dentro de una Organización. |

## 2. Entidades de Autorización

| Término | Definición | Propietario SSoT |
| :--- | :--- | :--- |
| **Sistema** | Una aplicación cliente externa registrada en el UMS (ej. TMS, WMS). | `Auth.Systems` |
| **Módulo** | Una agrupación lógica de funcionalidades dentro de un Sistema. | `Auth.Modules` |
| **Acción** | La unidad mínima de permiso (Create, Read, Update, Delete, etc.). | `Auth.Actions` |
| **Plantilla de Autorización** | Un conjunto predefinido de permisos que se puede asignar a múltiples sujetos. | `Auth.Templates` |
| **Role (Rol)** | Plano funcional nombrado que representa un conjunto de permisos dentro de un Sistema. Los Roles forman una jerarquía auto-referencial (`ParentRoleId`) que permite herencia. El Rol es la fuente desde la cual se derivan las Auth Templates; **no se asigna directamente a usuarios** — solo a través de un Profile. | `Auth.Roles` |

---
*Este glosario es la Fuente Única de Verdad (SSoT) para el lenguaje ubicuo del proyecto.*
