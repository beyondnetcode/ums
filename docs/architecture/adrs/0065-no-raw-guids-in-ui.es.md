# ADR 0065: Prohibición de Mostrar GUIDs Crudos en la Interfaz de Usuario

## Contexto y Planteamiento del Problema
En arquitecturas distribuidas, los GUIDs (Identificadores Globalmente Únicos) se utilizan extensamente para la identidad técnica, llaves foráneas y referencias en bases de datos (p. ej., Tenant IDs, User IDs, Delegation IDs). A menudo, los desarrolladores exponen inadvertidamente estos identificadores técnicos crudos a los usuarios finales en vistas de UI, tablas y paneles de detalle.

Esta práctica degrada drásticamente la Experiencia del Usuario (UX), viola la representación del lenguaje de negocio (Lenguaje Ubicuo de DDD) y crea la sensación de un "prototipo técnico" sin terminar en lugar de un producto pulido. Los GUIDs crudos no tienen significado para los usuarios finales y saturan la interfaz.

## Decisión
Establecemos una estricta regla arquitectónica y de UX: **Los GUIDs crudos NUNCA deben exponerse o renderizarse en la Interfaz de Usuario (UI), a menos que sea explícitamente solicitado por un requerimiento de negocio específico y justificado.**
Esta regla es adoptada a lo largo de toda la arquitectura de referencia de **Evolith** y es estrictamente aplicada en el monorepo de **UMS**.

### Lineamientos de Implementación
1. **Representación Semántica:** Cualquier ID técnico debe mapearse a un alias legible por humanos, rol, nombre de usuario, correo electrónico o código amigable para el negocio antes de ser renderizado.
2. **Uso Interno Exclusivo:** Los GUIDs solo deben usarse internamente para peticiones de API, enrutamiento, llaves de React, manejo de estado y procesamiento de payloads.
3. **Mecanismos de Respaldo (Fallback):** Si un nombre amigable no está disponible, el sistema debe mostrar un texto genérico localizado (p. ej., "Usuario", "Delegación", "Desconocido") en lugar de imprimir el hash o ID crudo.
4. **Revisiones de Código (Code Reviews):** Los Pull Requests que contengan exposición de IDs crudos en la capa de presentación (p. ej., componentes de React) sin una excepción explícita serán rechazados.

## Consecuencias

### Positivas
* Mejora significativamente la UX y el profesionalismo del sistema.
* Refuerza la alineación con el Lenguaje Ubicuo del Diseño Guiado por el Dominio (DDD) al presentar conceptos de negocio en lugar de tecnicismos de base de datos.
* Previene posibles fugas de enumeración o seguridad (aunque los GUIDs son seguros, ofuscar las llaves de la base de datos es una buena práctica de defensa en profundidad).

### Negativas
* Requiere mapeos adicionales en el frontend/backend o cruces de API para obtener etiquetas semánticas de entidades relacionadas en lugar de simplemente pasar llaves foráneas.
* Los desarrolladores deben ser diligentes al escribir formateadores de UI para casos límite donde los datos semánticos no estén disponibles inmediatamente en el contexto.
