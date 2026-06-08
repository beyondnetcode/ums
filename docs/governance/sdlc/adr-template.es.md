# Plantilla ADR

Usa esta plantilla para decisiones arquitectonicas especificas de UMS. Mantén sincronizadas las versiones en ingles y espanol dentro del mismo cambio.

## Encabezado

```markdown
# ADR-NNNN: Titulo de la Decision

**Estado:** Propuesto | Aceptado | Deprecado | Reemplazado
**Fecha:** YYYY-MM-DD
**Responsable de Decision:** Arquitectura
**Relacionado:** links ADR, historias funcionales, habilitadores tecnicos
```

## Contexto

Describe las fuerzas de negocio y tecnicas que hacen necesaria la decision. Incluye restricciones, bounded contexts afectados, impacto en tenancy, impacto de seguridad e impacto documental.

## Decision

Declara la decision de forma directa. Prefiere un parrafo claro seguido de una tabla pequena cuando la decision cambia stack, runtime, persistencia, contratos o propiedad.

## Consecuencias

Documenta resultados positivos, trade-offs, riesgos y alternativas rechazadas.

## Cumplimiento

Lista los checks requeridos para demostrar que la decision esta implementada: codigo, pruebas, documentacion, diagramas, migraciones, seed data y configuracion runtime.

## Seguimiento

Lista el trabajo requerido que queda fuera del ADR.
