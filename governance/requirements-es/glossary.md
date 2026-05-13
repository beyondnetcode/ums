# 📖 Glosario de Términos

Este documento establece el glosario de términos estandarizado y no ambiguo para el **Sistema de Gestión de Usuarios (UMS)** bajo la estrategia **BMAD-METHOD**.

## 1. Entidades de Identidad

| Término | Definición |
| :--- | :--- |
| **Sujeto (Subject)** | La representación abstracta de una identidad (Persona o Sistema) dentro del ecosistema. |
| **Organización** | El límite administrativo y de seguridad principal. Un Sujeto siempre pertenece a una Organización. |
| **Tenant** | Una instancia lógica aislada de datos y configuración dentro de una Organización. |

## 2. Entidades de Autorización

| Término | Definición |
| :--- | :--- |
| **Sistema** | Una aplicación cliente externa registrada en el UMS (ej. TMS, WMS). |
| **Módulo** | Una agrupación lógica de funcionalidades dentro de un Sistema. |
| **Acción** | La unidad mínima de permiso (Create, Read, Update, Delete, etc.). |
| **Plantilla de Autorización** | Un conjunto predefinido de permisos que se puede asignar a múltiples sujetos. |

---
*Este glosario es la Fuente Única de Verdad (SSoT) para el lenguaje ubicuo del proyecto.*
