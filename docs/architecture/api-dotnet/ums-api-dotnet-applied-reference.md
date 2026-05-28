# UMS API .NET Applied Reference

> Language: [English](./ums-api-dotnet-applied-reference.md) | [Espanol](./ums-api-dotnet-applied-reference.es.md)

## 1. Purpose

This document maps the UMS .NET API implementation to the Evolith API Standard - .NET. It is an applied product reference, not a universal standard.

Reusable backend rules belong in Evolith. UMS-specific details remain here unless promoted through an Evolith ADR, governance standard, or canonical pattern.

## 2. Source scope

The applied reference covers:

```text
src/apps/ums.api
```

Observed profile:

| Concern | UMS implementation |
|---|---|
| Host | ASP.NET Core minimal host |
| Composition | Presentation bootstrappers |
| Application boundary | MediatR and FluentValidation |
| API surface | REST commands and GraphQL queries |
| Versioning | URL segment API versioning |
| Persistence | EF Core with SQL Server baseline and local SQLite support |
| Operations | Structured logs, telemetry registration, health checks, rate limits, background workers |
| Cross-cutting policies | Aspects for audit, transactions, tenant validation, and logging |

## 3. Mapping

| Evolith topic | UMS source evidence | Classification |
|---|---|---|
| Host bootstrap | `Ums.Presentation/Program.cs` | Applied evidence |
| Modular service composition | `Ums.Presentation/Bootstrapping/UmsApiServiceBootstrappers.cs` | Reusable pattern |
| Application boundary | `Ums.Application/DependencyInjection.cs` | Reusable pattern |
| Middleware pipeline | `UseUmsApiPipeline` | Applied evidence |
| API surface mapping | `MapUmsApiSurface` and bounded-context route groups | Applied evidence; concrete modules local |
| GraphQL query surface | `MapGraphQlSurface` | Candidate Evolith governance standard |
| Health model | `MapHealthSurface` and infrastructure health checks | Reusable operations pattern |
| Persistence provider setup | `Ums.Infrastructure/DependencyInjection.cs` | Pattern reusable; provider matrix local |
| SQL Server EF Core profile | DbContext setup, retries, interceptors, migration history table | Candidate Evolith persistence profile |
| Background workers | Audit persistence and outbox dispatcher hosted services | Reusable reliability pattern |
| Cross-cutting aspects | AOP setup and MediatR proxy registration | Candidate canonical pattern |

## 4. Items that should remain UMS-local

| Item | Reason |
|---|---|
| Bounded contexts and aggregate modules | Product domain model. |
| Concrete endpoint groups and route names | UMS API surface. |
| Product documentation metadata | UMS product identity. |
| Concrete header and claim names | Local API contract. |
| Development-only helpers | Local development support. |
| Runtime values for limits and retries | Product operations policy. |
| Schema names and repository class names | UMS persistence implementation. |
| Product-specific strategy classes | UMS capability implementation. |
| Development seed data | Local environment behavior. |

## 5. Items to promote to Evolith

| Candidate | Promotion target |
|---|---|
| Minimal host plus named bootstrapper composition | Evolith API bootstrap standard |
| REST commands plus GraphQL query governance | Evolith API surface standard or ADR |
| MediatR plus validation pipeline | Evolith application-layer profile |
| Problem Details and user-safe error context | Evolith API error standard |
| SQL Server EF Core profile | Evolith persistence standard |
| Request and execution context abstractions | Evolith tenancy and observability standard |
| Liveness, readiness, and backlog health model | Evolith operations standard |
| Outbox and audit background processing | Evolith reliability standard |
| Aspect or decorator based cross-cutting policies | Evolith canonical pattern or ADR |

## 6. Items requiring ADR or formal promotion

| Item | Required action |
|---|---|
| .NET 10 backend runtime baseline | Evolith ADR or authoritative stack update |
| REST commands and GraphQL queries in one API tier | Evolith API governance ADR |
| MediatR as default application boundary | Evolith ADR or optional backend profile |
| FluentValidation as validation pipeline standard | Evolith engineering standard |
| EF Core plus SQL Server as default persistence profile | Evolith persistence ADR or stack standard |
| AOP shell library as cross-cutting implementation mechanism | Evolith canonical pattern and ADR |
| Retry and circuit breaker profile for persistence | Evolith resilience standard |
| Idempotency requirement for commands | Evolith API reliability standard |

## 7. Validation checklist

Before changing UMS API architecture, validate:

- Classify the change as Evolith standard, UMS local decision, or promotion candidate.
- Keep shared backend patterns independent from UMS product language.
- Keep product routes, headers, seed data, schemas, and strategy classes in UMS.
- Propose reusable rules in Evolith first.
- Update English and Spanish documentation together.
- Keep Markdown UTF-8 clean and free from decorative icons.
- Do not modify workflows or hooks without explicit authorization.

---
[Back to API .NET Portal](./README.md)
