# 📘 Functional Story 13: Configurar Parámetros Jerárquicos del Sistema

Este documento especifica el flujo para gestionar configuraciones y feature flags bajo un modelo de herencia y anulaciones (overrides).

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Configurar Parámetros Jerárquicos del Sistema |
| **Actor Principal** | Administrador Global / de Tenant / de Sistema |
| **Precondiciones** | El actor tiene permisos de administración sobre el alcance (Scope) seleccionado. |
| **Postcondiciones** | El parámetro es persistido y aplicado dinámicamente según la regla de herencia. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El administrador accede al panel de configuración.
2.  Selecciona el alcance (Scope) de la configuración: Global, Tenant, Sistema (Suite) o Módulo.
3.  Ingresa el código del parámetro (ej. `ENABLE_DOC_VALIDATION`) y su valor.
4.  Define si el parámetro es heredable (`IsInheritable`) por niveles inferiores.
5.  El sistema valida si existe una restricción de "No Heredable" en un nivel superior que impida la anulación.
6.  El sistema persiste el registro en `APP_CONFIGURATION`.
7.  Los servicios consumen el valor resuelto siguiendo la lógica: "El valor más específico (Módulo > Sistema > Tenant > Global) prevalece".

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Intento de Override Bloqueado
*   Si un administrador de Tenant intenta cambiar un valor que fue marcado como `IsInheritable = FALSE` a nivel Global, el sistema rechaza el cambio y muestra la política de cumplimiento superior.

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `APP_CONFIGURATION`
- `TENANT`
- `SYSTEM_SUITE`

### Criterios de Aceptación
1.  Un cambio en el nivel Global debe propagarse a todos los Tenants que no tengan un override específico.
2.  El sistema debe soportar valores cifrados para parámetros marcados como sensibles.
3.  La resolución del parámetro debe ocurrir en tiempo real o mediante caché invalidable.
