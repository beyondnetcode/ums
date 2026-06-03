# Canonical Patterns Index

Canonical Patterns are reference implementations that demonstrate how UMS applies architectural decisions in code. Each pattern is a complete, copy-paste-ready blueprint for the corresponding concern.

| CP ID | Title | Type | ADR | Evolith |
|-------|-------|------|-----|-------|
| [CP-01](./cp-01-hexagonal-port-adapter.md) | Hexagonal Architecture — Port & Adapter | Structural | ADR-0002 | — |
| [CP-02](./cp-02-aggregate-root-domain-event.md) | Aggregate Root & Domain Event | Tactical DDD | ADR-0029 | — |
| [CP-03](./cp-03-result-pattern.md) | Result Pattern (Either Monad) | Error Handling | ADR-0038 | — |
| [CP-04](./cp-04-multitenant-repository-rls.md) | Multi-Tenant Repository with RLS | Data Access / Security | ADR-0010 | — |
| [CP-05](./cp-05-execution-context-propagation.md) | Execution Context Propagation | Observability | ADR-0061 | Proposed |
| [CP-06](./cp-06-pii-safe-structured-logging.md) | PII-Safe Structured Logging with Serilog | Security / Logging | ADR-0062 | Proposed |
| [CP-07](./cp-07-idempotency-middleware.md) | Idempotency Key Middleware | Reliability | ADR-0063 | Proposed |
| [CP-08](./cp-08-aop-logging-decorator.md) | AOP Logging Decorator with Observability Envelope | Cross-Cutting | ADR-0060 / ADR-0061 | Proposed | > **Proposed** — marked for extraction to the Evolith parent architecture; these patterns have zero UMS-specific dependencies and are applicable to any .NET satellite.

---

**[Back to Architecture Portal](../../index.md)** | **[Back to Master Index](../../../MASTER_INDEX.md)**
