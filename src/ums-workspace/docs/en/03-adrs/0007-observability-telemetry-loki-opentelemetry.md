# ADR 0007: Observability Telemetry with Grafana Loki and OpenTelemetry

* **Status:** Accepted
* **Based on:** [arc32-7](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0007-observability-telemetry-loki-opentelemetry.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. UMS uses .NET OpenTelemetry SDK (OpenTelemetry.Instrumentation.AspNetCore) instead of NestJS @opentelemetry/instrumentation-http.
2. Same protocol (OTLP), same backend (Loki + Tempo + Grafana). Configuration differs per SDK.
3. Logging: .NET ILogger with OpenTelemetry exporter vs NestJS Logger with OTel transport.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
