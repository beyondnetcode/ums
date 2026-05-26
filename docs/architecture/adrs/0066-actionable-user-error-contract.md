# ADR-0066: Actionable User Error Contract and Correlated Diagnostics

**Status:** Accepted  
**Date:** 2026-05-26  
**Decision Owner:** Architecture  
**Evolith disposition:** Proposed for adoption as a universal Evolith standard  
**Related:**
- [ADR-0053: OpenTelemetry Observability](./0053-opentelemetry-observability.md)
- [ADR-0055: GraphQL/REST Hybrid API Pattern](./0055-graphql-rest-hybrid-api.md)
- [ADR-0057: Zustand and TanStack Query State Management](./0057-zustand-tanstack-query-state.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [ADR-0062: PII-Safe Serilog Configuration](./0062-pii-safe-serilog-configuration.md)
- [Notification and Feedback Architecture](../blueprints/notification-feedback-architecture.md)

---

## Context

A quality review of the System Suite module registration workflow exposed two conflicting failure modes:

1. Technical API messages, exception details, and stack-related data could reach the browser.
2. Once all backend messages were suppressed, expected validation failures became too generic for a user to correct the input.

For example, a user submitting a module description longer than the allowed limit should be told how to fix the data. That user must not see a namespace, class name, database detail, exception message, or stack trace. Support engineers still need a reference that locates the complete technical event in Serilog and Grafana Loki.

The decision applies across all user-initiated commands in UMS, not only module registration.

## Decision

UMS adopts a two-channel error contract:

1. **User feedback channel:** exposes only approved, actionable business or validation information.
2. **Diagnostic channel:** retains technical details in structured logs and telemetry correlated through a server-generated error identifier.

### 1. Error Classification

| Error class | Example | User-visible content | Technical logging |
|---|---|---|---|
| Validation failure | Maximum length exceeded | Actionable correction and tracking code | Optional structured event |
| Business conflict | Duplicate module code | Business-safe reason and tracking code | Structured event when operationally useful |
| Authorization or authentication | Access rejected | Safe access statement and tracking code | Security-aware structured event |
| Infrastructure or unexpected failure | Database, resolver, unhandled exception | Generic retry guidance and tracking code | Full sanitized exception with correlation |

### 2. Public Error Payload

REST command failures use RFC 7807 Problem Details. The public payload may include:

```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Validation Error",
  "status": 422,
  "detail": "No se pudo completar la operacion porque existen campos invalidos.",
  "userMessage": "No se pudo registrar el modulo porque el campo Codigo tiene un formato invalido. Use solo letras, numeros y guion bajo, por ejemplo REPORTS_01. Valor ingresado: DDDD-!.",
  "errorCode": "validation.invalid_format",
  "messageKey": "system_suite.module.code_invalid_format",
  "messageParameters": { "invalidValue": "DDDD-!" },
  "errorId": "0cd26dd6-d50e-4b3c-a662-8098a87569a4",
  "traceId": "<distributed-trace-id>"
}
```

Rules:

- `errorId` is a server-generated GUID created once for each failed request. It is required in the response, response header (`X-Error-Id`), and corresponding Serilog event.
- `traceId` and `X-Correlation-Id` remain distributed-tracing identifiers and must not replace `errorId`.
- `userMessage`, when present, is explicitly safe for presentation.
- `errorCode`, `messageKey`, and `messageParameters` are the target contract for localized clients.
- `detail` must never contain stack traces, exception type names, namespaces, source paths, SQL statements, tokens, secrets, or PII.
- Clients must not display arbitrary `detail`, raw GraphQL messages, or native exception messages. They display only approved fields or a local fallback.

The current UMS implementation produces `userMessage` through API localization resources selected by `X-Language` or `Accept-Language`. Stable `messageKey` support remains the evolution path for richer clients.

### 3. GraphQL Boundary

GraphQL is used for reads according to ADR-0055. GraphQL errors exposed to clients use safe localized generic messages and carry `errorId`. Resolver exceptions are logged through Serilog with that same `errorId`, but are not serialized to the browser.

### 4. Observability and Logging

- Every failed REST or GraphQL response obtains a new server-generated `errorId` GUID.
- Serilog records `ErrorId` for expected failures and records full sanitized exceptions with `ErrorId` for unexpected failures; Loki queries use that property.
- `CorrelationId` and `TraceId` continue to link distributed operations independently from the support-facing `ErrorId`.
- PII-safe logging rules from ADR-0062 apply before any log sink.
- A support engineer uses the visible tracking code to locate the detailed backend event.

### 5. Notification Lifecycle

User feedback is delivered through the centralized notification mechanism:

- Validation and business feedback is actionable and may include the tracking code.
- Automatic toast expiry removes only the ephemeral presentation; history may remain available.
- Manual dismissal through the close control is an explicit user action and removes that notification entry from the active notification state.

### 6. Required Message Examples

| Category | Safe user-facing example |
|---|---|
| Validation | `No se pudo registrar el modulo porque el campo Codigo tiene un formato invalido. Use solo letras, numeros y guion bajo, por ejemplo REPORTS_01. Valor ingresado: DDDD-!.` |
| Business rule | `No se pudo registrar el modulo porque ya existe otro modulo con el mismo Codigo. Ingrese un codigo diferente.` |
| Authorization | `No se pudo completar la operacion porque no tiene permisos suficientes. Solicite acceso al administrador.` |
| Unexpected | `No se pudo completar la operacion debido a un error inesperado. Intente nuevamente mas tarde.` |

Each displayed failure appends: `Si necesitas mas detalles, consulta con el administrador e indica este ID de error: {errorId}.`

## Rejected Alternatives

### Display all backend `detail` values

Rejected. It is convenient but allows implementation details or PII to escape when an endpoint is misconfigured.

### Display only generic messages

Rejected. It prevents users from correcting expected business or validation conditions, increasing retries and support requests.

### Show stack details only in development or QA

Rejected. QA and shared environments are still user-facing surfaces and may contain production-like data.

## Consequences

### Positive

- Users receive enough information to correct safe, expected failures.
- Technical diagnostics remain available to support without being exposed in the interface.
- REST, GraphQL, logging, and frontend notification behavior share one auditable contract.
- The contract can evolve from server-rendered `userMessage` text to fully localized client message keys.

### Trade-offs

- Backend boundaries must classify which failures are safe to publish.
- New validation rules require safe display text or localization metadata.
- Tests must cover both actionable output and absence of technical leakage.

## Implementation Mapping

| Concern | UMS implementation |
|---|---|
| REST safe mapping | `Ums.Presentation/Extensions/ResultExtensions.cs` |
| Error ID generation | `Ums.Presentation/Extensions/UserFacingErrorContext.cs` |
| Unhandled exceptions | `Ums.Presentation/Middleware/GlobalExceptionHandler.cs` |
| GraphQL sanitization | `Ums.Presentation/GraphQL/SafeGraphQlErrorFilter.cs` |
| Localization resources | `Ums.Globalization/Resources/{language}/domain-errors.json` |
| Safe web extraction | `src/application/errors/http-error.ts` |
| Notification composition | `src/application/hooks/use-notified-mutation.ts` |
| Manual dismissal | `src/application/stores/notification.store.ts` and `src/presentation/shared/components/ToastQueue.tsx` |

## Verification Requirements

- Integration tests assert actionable validation fields, server-generated `errorId`, the matching `X-Error-Id` header, and `traceId`.
- Security tests assert that stack traces, exception names, and raw internal messages are absent.
- Frontend tests assert that only approved messages and tracking codes are rendered.
- Notification tests assert that manual close removes the selected notification.

---

**[ADR Registry](./index.md)** | **[Notification and Feedback Blueprint](../blueprints/notification-feedback-architecture.md)**
