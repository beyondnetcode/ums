# UMS API .NET Applied Reference

> Language: [English](./README.md) | [Espanol](./README.es.md)

This section documents how the UMS .NET API implementation applies the Evolith API Standard - .NET.

UMS is an applied reference, not the source of universal backend standards. Reusable backend rules belong in Evolith. UMS keeps concrete product modules, endpoints, headers, persistence choices, bounded contexts, seeds, and local implementation decisions.

## Documents

| Document | Purpose |
|---|---|
| [UMS API .NET Applied Reference](./ums-api-dotnet-applied-reference.md) | Evidence-based mapping between Evolith API topics and UMS source files. |

## Authority boundary

| Concern | Evolith | UMS |
|---|---|---|
| Reusable API standards | Owns principles, boilerplate rules, quality gates, and promotion criteria | Consumes standards |
| Product implementation | References examples only | Owns concrete source code and local decisions |
| Persistence | Owns provider governance and data-quality rules | Owns concrete DbContext, repositories, interceptors, migrations, and provider switches |
| API surface | Owns REST/GraphQL responsibility rules | Owns concrete endpoints, route groups, schemas, and bounded-context modules |
| Operations | Owns required capabilities | Owns concrete Serilog, OpenTelemetry, health checks, rate limits, and runtime values |

## Current evidence scope

This reference is based on the current .NET API under:

```text
src/apps/ums.api
```

Key observed sources include host bootstrap, service bootstrappers, middleware pipeline, API surface mapping, application DI, infrastructure DI, persistence provider configuration, health checks, AOP, background services, and operational controls.

---
[Back to Architecture Portal](../index.md)
