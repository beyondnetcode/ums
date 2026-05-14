# ADR-0029: Adopción de Primitivas TDD Tácticas Opcionales

* **Status:** Accepted
* **Basado en:** [arc32-29](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0029-tactical-ddd-primitives-library.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. UMS usa primitivas DDD nativas de C# (records para ValueObjects, Entity base class, Result pattern) en lugar de @nestájslatam/ddd.
2. Mismo enfoque DDD: dominio sin dependencias, ValueObject con igualdad estructural, AggregateRoot con eventos de dominio.
3. Primitivas definidas en Ums.Domain/Common/. Sin dependencia externa.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
