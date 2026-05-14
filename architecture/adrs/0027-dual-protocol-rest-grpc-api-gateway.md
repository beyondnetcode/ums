# ADR-0027: Dual-Protocol REST & gRPC API Structure with Kong Gateway

* **Status:** Accepted
* **Based on:** [arc32-27](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0027-dual-protocol-rest-grpc-api-gateway.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. UMS uses .NET gRPC (Grpc.AspNetCore) instead of NestJS @nestjs/microservices gRPC transport.
2. Same Kong Gateway, same dual-protocol architecture (REST for public, gRPC for internal). .proto contracts are shared.
3. Deferred: gRPC internal services until Phase 2. Kong is deployed per docker-compose.yml.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
