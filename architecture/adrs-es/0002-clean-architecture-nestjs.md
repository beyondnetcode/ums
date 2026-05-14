# ADR 0002: Clean Architecture y Límites Hexagonales en NestJS

* **Status:** Accepted
* **Basado en:** [arc32-2](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/0002-clean-architecture-hexagonal.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. Runtime .NET 8 en lugar de NestJS/Node.js — Capa de dominio usa POCOs C#, capa de aplicación usa interfaces de servicio sin MediatR.
2. Patrón Puerto/Adaptador idéntico. Validación con FluentValidation en lugar de class-validator. DI nativa de Microsoft.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
