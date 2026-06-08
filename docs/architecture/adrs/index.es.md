# Indice ADR — Registros de Decisiones Arquitectonicas UMS

> **Estandar padre:** [Evolith — ADR Registry](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/README.md)

Todas las decisiones arquitectonicas del User Management System (UMS) se registran aqui. UMS hereda la linea base obligatoria de la arquitectura de referencia Evolith y la extiende o especializa para el contexto del producto UMS.

UMS es un repositorio satelite de `evolith_arch32`. El repositorio padre define la linea base corporativa; los ADRs de UMS deben adoptarla por referencia o documentar una adaptacion explicita PostgreSQL / .NET cuando el ejemplo padre sea especifico de runtime o base de datos.

## Decisiones UMS

| ADR | Titulo | Estado |
|-----|--------|--------|
| [ADR-0050](./0050-naming-taxonomy-standard.es.md) | Estandar de nomenclatura y taxonomia | Aceptado |
| [ADR-0051](./0051-event-bus-injectable-port.es.md) | Event Bus como puerto inyectable | Aceptado |
| [ADR-0052](./0052-immutable-audit-trail-enforcement.es.md) | Enforcement de audit trail inmutable | Aceptado |
| [ADR-0053](./0053-opentelemetry-observability.es.md) | Estrategia de observabilidad OpenTelemetry | Aceptado |
| [ADR-0054](./0054-shell-library-isolation.es.md) | Aislamiento de shell libraries | Aceptado |
| [ADR-0055](./0055-graphql-rest-hybrid-api.es.md) | Patron API hibrido GraphQL/REST | Aceptado |
| [ADR-0056](./0056-clean-architecture-frontend.es.md) | Limites de Clean Architecture en frontend | Aceptado |
| [ADR-0057](./0057-zustand-tanstack-query-state.es.md) | Estado con Zustand y TanStack Query | Aceptado |
| [ADR-0058](./0058-api-gateway-yarp-evolution.es.md) | Evolucion API Gateway con YARP | Propuesto |
| [ADR-0059](./0059-single-api-tier-decision.es.md) | Decision de API tier unico | Aceptado |
| [ADR-0060](./0060-aop-cross-cutting-concern-strategy.es.md) | Estrategia AOP para concerns transversales | Aceptado |
| [ADR-0061](./0061-execution-context-accessor.es.md) | Patron Execution Context Accessor | Aceptado |
| [ADR-0062](./0062-pii-safe-serilog-configuration.es.md) | Configuracion Serilog segura para PII | Aceptado |
| [ADR-0063](./0063-idempotency-middleware.es.md) | Middleware de idempotencia | Aceptado |
| [ADR-0064](./0064-lean-root-repository-taxonomy.es.md) | Taxonomia lean del repositorio raiz | Aceptado |
| [ADR-0065](./0065-no-raw-guids-in-ui.es.md) | Prohibicion de GUIDs crudos en UI | Aceptado |
| [ADR-0066](./0066-actionable-user-error-contract.es.md) | Contrato de error accionable | Aceptado |
| [ADR-0068](./0068-feature-flag-system-scope.es.md) | Alcance SystemSuite para feature flags | Aceptado |
| [ADR-0069](./0069-domain-inheritance-strategy.es.md) | Estrategia de herencia en dominio | Aceptado |
| [ADR-0070](./0070-database-schema-strategy-decision.es.md) | Estrategia de esquema por modulo | Aceptado |
| [ADR-0071](./0071-auth-graph-engine.es.md) | Motor de grafo de autorizacion | Aceptado |
| [ADR-0072](./0072-dynamic-auth-method-resolution.es.md) | Resolucion dinamica de metodo de autenticacion | Aceptado |
| [ADR-0073](./0073-ums-sdk-multi-runtime.es.md) | SDK UMS multi-runtime | Aceptado |
| [ADR-0074](./0074-auth-graph-schema-versioning.es.md) | Politica de versionado del auth graph | Aceptado |
| [ADR-0075](./0075-onboarding-approval-inbox-and-scope-based-authorization.es.md) | Bandeja de aprobacion de onboarding | Aceptado |
| [ADR-0076](./0076-utc-dates-timezone-language-resolution.es.md) | Resolucion de UTC, timezone e idioma | Aceptado |
| [ADR-0077](./0077-tenant-portal-management-authorization-boundary.es.md) | Frontera de autorizacion del portal tenant | Aceptado |
| [ADR-0078](./0078-ddd-domain-resource-hierarchy.es.md) | Jerarquia DDD de recursos de dominio | Aceptado |
| [ADR-0079](./0079-dependency-guard-policy.es.md) | Politica de guardas de dependencia | Aceptado |
| [ADR-0080](./0080-auth-graph-preview-internal-pipeline.es.md) | Preview de auth graph interno | Aceptado |
| [ADR-0081](./0081-semantic-auth-graph-client-contract.es.md) | Contrato semantico del auth graph cliente | Propuesto |
| [ADR-0082](./0082-postgresql-authoritative-persistence-baseline.es.md) | Linea base autoritativa de persistencia PostgreSQL | Aceptado |

---

**[Portal de Arquitectura](../index.es.md)** | **[Indice Maestro](../../MASTER_INDEX.es.md)**
