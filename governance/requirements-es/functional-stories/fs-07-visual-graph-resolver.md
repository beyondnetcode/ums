# Functional Story 7: Diagnosticar Permisos vía Visualizador de Grafos

## 1. Propósito de Negocio

Los equipos de soporte y seguridad necesitan entender por qué un usuario puede o no puede realizar una acción. UMS debe ofrecer una explicación visual clara de permisos efectivos, rutas permitidas, rutas denegadas y razones de decisión.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **SRE / Ingeniero de Soporte** | Investiga problemas de permisos. |
| **Administrador de Seguridad** | Revisa configuración y decisiones de autorización. |

## 3. Precondiciones de Negocio

- El actor tiene permisos de diagnóstico.
- El usuario objetivo existe.
- El usuario objetivo tiene al menos un perfil.

## 4. Flujo Funcional Principal

1. El actor busca al usuario objetivo.
2. El actor selecciona tenant, sede y sistema para el diagnóstico.
3. El sistema resuelve los permisos efectivos del usuario para ese contexto.
4. El sistema muestra un grafo visual de rutas permitidas y denegadas.
5. El actor puede inspeccionar la razón de cada decisión.
6. El actor usa la explicación para resolver problemas de soporte o configuración.

## 5. Flujos Alternativos y Excepciones

### A. Usuario Sin Perfiles

Si el usuario no tiene perfiles activos, el sistema muestra que no existen permisos disponibles por asignación.

### B. Reglas en Conflicto

Si aplican reglas de permitir y denegar, el sistema explica que la denegación explícita tiene prioridad.

## 6. Reglas de Negocio

1. El diagnóstico debe explicar la razón detrás de cada decisión.
2. Los permisos denegados deben distinguirse visualmente de los permitidos.
3. El acceso diagnóstico debe restringirse a roles autorizados de soporte o seguridad.
4. El diagnóstico no debe otorgar permisos adicionales.

## 7. Criterios de Aceptación

1. Usuarios autorizados pueden diagnosticar permisos efectivos para un usuario y contexto.
2. El grafo muestra claramente rutas permitidas y denegadas.
3. Las razones de decisión son visibles.
4. Los usuarios sin perfiles muestran un estado comprensible sin permisos.

## 8. Requisitos Técnicos

- Resolver el grafo diagnóstico de autorización sin mutar permisos.
- Incluir reglas fuente y razones de decisión en la respuesta diagnóstica.
- Omitir o refrescar caché cuando la precisión diagnóstica requiera datos fuente actuales.
- Emitir eventos de auditoría por acceso diagnóstico.

## 9. Trazabilidad

- Entidades: `PROFILE`, `PROFILE_PERMISSION`, `PERMISSION_TEMPLATE`, `ACTION`
- ADRs: ADR-0021, ADR-0039
- Technical Enabler: TE-01
