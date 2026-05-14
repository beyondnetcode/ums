# Estándar de Redacción de Historias Funcionales

> **Fuente corporativa:** Este estándar local de UMS implementa el estándar base ARC32/BMAD-METHOD definido en `arc32_progresive_monolith/governance/sdlc-es/03-documentation/functional-story-writing-standard.md`.

Este estándar define cómo deben redactarse las Historias Funcionales de UMS para que Product Owners, Analistas de Negocio, QA y Desarrolladores puedan usar el mismo documento sin mezclar intención de negocio con detalle de implementación.

---

## 1. Estructura Obligatoria

Toda Historia Funcional DEBE seguir está estructura:

1. **Propósito de Negocio**: qué problema resuelve y por qué importa.
2. **Actores**: participantes principales y secundarios, descritos por responsabilidad de negocio.
3. **Precondiciones de Negocio**: condiciones que deben cumplirse antes de iniciar el flujo.
4. **Flujo Funcional Principal**: narrativa de negocio orientada al usuario, sin detalles de implementación.
5. **Flujos Alternativos y Excepciones**: resultados de negocio ante rechazo, duplicidad, información faltante, servicio no disponible o estado inválido.
6. **Reglas de Negocio**: reglas de dominio validables por el Product Owner.
7. **Criterios de Aceptación**: condiciones verificables por PO/QA.
8. **Requisitos Técnicos**: restricciónes de implementación para desarrollo.
9. **Trazabilidad**: entidades, ADRs, Technical Enablers y artefactos operativos relacionados.

---

## 2. Reglas de Narrativa Funcional

Las secciones funcionales DEBEN usar lenguaje entendible para Product Owner o Analista de Negocio.

Las secciones funcionales NO DEBEN iniciar con:

- rutas de API o métodos HTTP,
- nombres de protocolos,
- detalles de motor de base de datos,
- detalles de caché,
- ejemplos de payload,
- nombres de excepciones,
- frameworks o librerías,
- comportamiento específico de infraestructura.

Esos detalles pertenecen a **Requisitos Técnicos**.

---

## 3. Reglas de Requisitos Técnicos

La sección de Requisitos Técnicos DEBE capturar:

- APIs/endpoints,
- entidades y tablas,
- persistencia, caché e invalidación,
- controles de seguridad,
- eventos de auditoría,
- códigos de error,
- requisitos de protocolos o tokens,
- contratos de integración,
- restricciónes derivadas de ADRs o Technical Enablers.

Esta sección permite que desarrollo tenga precisión sin hacer más difícil la lectura funcional.

---

## 4. Reglas de Criterios de Aceptación

Los criterios de aceptación DEBEN ser observables y validables desde negocio. Deben describir resultados esperados, no pasos de implementación.

Correcto:

- "El patrocinador puede ver si la solicitud fue aprobada o rechazada."
- "El sistema evita que usuarios externos reciban perfiles administrativos internos."

Evitar en criterios funcionales:

- "La API retorna `403 Forbidden`."
- "Se invalidan llaves de Redis."
- "La base de datos escribe en `APPROVAL_REQUEST`."

Mover esos detalles a Requisitos Técnicos.
