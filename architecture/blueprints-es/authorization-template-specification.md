# Especificación de Plantillas de Autorización

## 1. Descripción General
El módulo de Plantillas de Autorización proporciona un mecanismo para definir conjuntos reutilizables de permisos (Blueprints) que pueden ser heredados por Roles y Perfiles en todo el ecosistema de UMS. Esto reduce la carga operativa y garantiza la consistencia en la asignación de permisos.

## 2. Modelo de Dominio y Desacoplamiento
El sistema impone un desacoplamiento estricto entre las áreas funcionales y los roles de implementación:

1.  **Sistema/Suite**: Define el límite funcional propiedad de un Inquilino.
2.  **Plantilla de Permisos**: Un conjunto versionado de permisos estrictamente limitado a una Suite.
3.  **Rol**: Un esquema específico del Sistema derivado de una Plantilla.
4.  **Perfil**: El nexo contextual donde se persisten las autorizaciones efectivas.

## 3. Reglas de Negocio

### 3.1 Tipos de Herencia
*   **Estática (Clonación)**: Los permisos se copian de la Plantilla al Rol en el momento de la creación. Los cambios posteriores en la Plantilla no afectan al Rol.
*   **Dinámica (Vinculada)**: El Rol mantiene una referencia a una versión específica de la Plantilla. El sistema puede activar una "Sincronización" para actualizar los permisos efectivos del Rol cuando cambia la versión de la Plantilla.

### 3.2 Anulaciones (Overrides)
Los inquilinos pueden agregar o eliminar permisos específicos a un Rol que fue creado a partir de una Plantilla, creando un "Delta" con respecto al esquema base.

### 3.3 Alcance y Multi-tenancy
*   **Alcance del Sistema**: Cada plantilla está vinculada a un Sistema/Suite específico.
*   **Aislamiento del Inquilino**: Las plantillas están aisladas por `TenantId`. Las plantillas globales se gestionan como valores predeterminados del sistema pero siempre dentro del contexto de un Sistema.

## 4. Ciclo de Vida y Versionado
*   Las plantillas admiten los estados **Borrador**, **Publicada** y **Depreciada**.
*   El versionado sigue SemVer (ej. v1.2.0).
*   Se mantiene la trazabilidad automática para cada cambio en una plantilla.

## 5. Flujo de Creación
1.  **Seleccionar Suite**: El usuario selecciona el área funcional objetivo.
2.  **Seleccionar Plantilla**: El usuario selecciona una plantilla base (Global o Local).
3.  **Configurar Rol**: El sistema completa previamente los permisos basados en la plantilla.
4.  **Personalizar**: El usuario agrega anulaciones específicas si es necesario.
5.  **Auditar**: El sistema registra el `SourceTemplateId` y la `Version` para fines de cumplimiento.
