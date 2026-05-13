> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versión original (Inglés) y está programado para traducción oficial en la hoja de ruta.

# ADR 0011: Fault Tolerance and Resiliency Patterns (Circuit Breakers & Retries)

## Status
Approved

## Date
2026-05-08

## Context
In a mission-critical logistics and customs environment, the UMS and downstream systems must communicate with volatile external third-party APIs (e.g., SUNAT transmission services, external OCR APIs). Synchronous network failures, high latency, or timeouts in these external dependencies can cascade, causing our internal NestJS threads to hang and potentially crashing the entire service. 

We need a standardized fault-tolerance mechanism to gracefully handle external system outages without degrading the core user experience or losing transactional consistency.

## Decision
We will implement strict Resiliency Patterns for all outbound HTTP communications:

1. **Circuit Breaker Pattern (Infrastructure Adapter)**: We will utilize a Circuit Breaker library (such as `opossum`) strictly wrapped within infrastructure-level Adapters that implement Core Interfaces (Ports). The core domain remains 100% unaware of `opossum`. If an external service fails continuously, the circuit will "open," failing fast and preventing resource exhaustion.
2. **Exponential Backoff & Retries (Transparent to Core)**: For transient network errors, HTTP clients (e.g., Axios Interceptors) will be configured at the infrastructure layer to automatically retry the request with exponential backoff before failing completely. The Application/Use Case layers merely call `port.transmitData()` and receive success or an expected custom `DomainException`.
3. **Fallback Mechanisms**: In the event of an open circuit, the infrastructure adapter must provide a fallback mechanism (e.g., enqueueing the payload in a Dead Letter Queue or local database for delayed asynchronous transmission).

## Consequences
* **Pros**: Prevents cascading failures, ensures 24/7 internal operability even when third-party services are down, and guarantees no data loss for critical transmissions.
* **Cons**: Adds complexity to the outbound API adapters and requires careful tuning of timeout and retry thresholds to avoid overwhelming recovering services.

