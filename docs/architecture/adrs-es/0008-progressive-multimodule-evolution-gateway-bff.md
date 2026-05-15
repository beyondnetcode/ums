# ADR 0008: Evolución Progresiva Multi-Módulo con API Gateway y Patrón BFF

* **Status:** Accepted
* **Basado en:** [arc32-8](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0008-progressive-multimodule-evolution-gateway-bff.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. BFF Gateway implementado en .NET 8 Minimal APIs. Misma arquitectura, mismo Kong Gateway. Solo cambia el runtime del BFF.
2. Diferido: Mobile BFF y gRPC hasta Fase 2.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
