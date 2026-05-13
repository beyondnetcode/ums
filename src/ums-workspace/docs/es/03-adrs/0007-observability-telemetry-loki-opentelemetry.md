# ADR 0007: Telemetría de Observabilidad con Grafana Loki y OpenTelemetry

* **Status:** Accepted
* **Basado en:** [arc32-7](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0007-observability-telemetry-loki-opentelemetry.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. UMS usa SDK .NET OpenTelemetry en lugar de NestJS @opentelemetry. Mismo protocolo OTLP, mismo backend.
2. Logging: .NET ILogger con exportador OTel en lugar de NestJS Logger.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
