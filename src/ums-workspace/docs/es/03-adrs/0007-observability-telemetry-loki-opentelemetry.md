> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versión original (Inglés) y está programado para traducción oficial en la hoja de ruta.

# ADR 0007: Observability Telemetry with Grafana Loki and OpenTelemetry

## Status
Approved

## Date
2026-05-08

## Context
As the UMS applications move to production, we need an advanced observability system to monitor performance, log errors, and trace database executions in real-time. Traditional enterprise stacks like Elastic (ELK) consume massive RAM and CPU, which violates our zero-cost and lightweight architecture goals.

## Proposed Decision
We propose to adopt the **Grafana LGTM Stack (Loki + Grafana + Tempo)** combined with **OpenTelemetry (OTel)**:
1. Instrument our NestJS API natively using **OpenTelemetry SDKs** to emit standard logs, metrics, and traces without coupling to any specific APM vendor.
2. Ship logs to **Grafana Loki** due to its lightweight, metadata-only indexing model that uses 10x less RAM than Elasticsearch.
3. Use **Grafana Tempo** for distributed tracing, allowing developers to see exact database execution paths and pinpoint latencies across use cases.

## Consequences

### Positive (Pros)
* **Lightweight and Free**: The complete LGTM stack can run easily with under 500MB of RAM, making it extremely cost-effective.
* **Aspect-Oriented Logs**: Real-time logging is handled transparently via our shared `libs-aop` library without polluting core use cases.
* **Unified Dashboard**: Grafana combines metrics, logs, and traces into a single glass-morphic pane of glass.

### Negative (Cons)
* Requires running a local or cloud Grafana Loki collector daemon.

