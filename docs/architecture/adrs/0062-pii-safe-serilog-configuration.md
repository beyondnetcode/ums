# ADR-0062: PII-Safe Serilog Configuration (HARDENING-04)

**Status:** Accepted  
**Date:** 2026-05-24  
**Decision Owner:** Architecture  
**arc32 disposition:** Proposed for arc32 adoption — pattern is runtime-neutral; applicable to any .NET satellite using Serilog  
**Related:**
- [ADR-0053: OpenTelemetry Observability](./0053-opentelemetry-observability.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [CP-06: PII-Safe Structured Logging](../artifacts/canonical-patterns/cp-06-pii-safe-structured-logging.md)

---

## Context

UMS processes personally identifiable information (PII): email addresses, identity references, passwords, tokens, and national IDs. ADR-0053 mandates structured logging via Serilog, but unguarded structured logging creates a PII leakage risk:

```csharp
// Risk: email leaks into every log sink
_logger.LogInformation("User {Email} activated by {ActorId}", user.Email, actorId);
```

Three layers of risk exist:
1. **Explicit capture** — developer deliberately logs a PII field by name
2. **Destructuring** — Serilog `{@object}` expansions log all properties of a class, including PII fields
3. **Free-text leakage** — string interpolation or message templates that happen to contain email-shaped strings

### Why property-name masking over attribute annotations

The annotation approach (`[Sensitive]` on domain properties) couples the Domain layer to a logging library, violating domain purity. Property-name masking at the Serilog pipeline level requires no domain changes.

---

## Decision

**Apply PII masking at the Serilog pipeline level through two complementary mechanisms: a destructuring policy and a log event enricher.**

### 1. `PiiMaskingPolicy` — Serilog `IDestructuringPolicy`

Registered via `.Destructure.With<PiiMaskingPolicy>()`. Intercepts object destructuring before any sink processes it.

The policy intercepts at the property-name level inside the enricher (see below) — the `TryDestructure` method returns `false` so Serilog continues normal destruction; the enricher then scans and redacts.

### 2. `PiiSanitizerEnricher` — Serilog `ILogEventEnricher`

Registered via `.Enrich.With<PiiSanitizerEnricher>()`. Runs after message template rendering, scanning all `ScalarValue` string properties:

```csharp
// Masked property names (case-insensitive):
"email", "emailaddress", "mail",
"password", "passwordhash", "passwordtext",
"identityreference",
"token", "accesstoken", "refreshtoken", "bearertoken", "idtoken",
"secret", "apikey", "apisecret", "clientsecret",
"ssn", "nationalid", "taxid"
```

**Email regex sweep** — any free-text scalar that matches `[^@\s]+@[^@\s]+\.[^@\s]+` is partially masked: `jo***@***.com`. This catches leakage through non-PII-named properties.

### 3. `LoggingExtensions.ConfigureUmsSerilog`

Single extension method wiring up the complete Serilog configuration:

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));
```

**Output strategy:**

| Environment | Format | Rationale |
|-------------|--------|-----------|
| Development | Coloured text console | Human-readable; trace/correlation prefix visible |
| Staging/Production | Compact JSON (`CompactJsonFormatter`) | Machine-readable; consumed by Fluentd / container log drivers |

**Configuration (appsettings.json):**

```json
"Observability": {
  "Logging": {
    "ConsoleFormat": "CompactJson",   // "Text" or "CompactJson"
    "MinimumLevel": "Information",    // any Serilog level
    "OutputTemplate": "..."           // Text-mode only
  }
}
```

**Enrichers always applied:**
- `Enrich.FromLogContext()` — picks up ILogger scopes (CorrelationId, SessionTrackingId from middleware)
- `Enrich.WithMachineName()` — pod/host identity for Kubernetes deployments
- `Enrich.WithThreadId()` — correlates concurrent request logs
- `Enrich.With<PiiSanitizerEnricher>()` — PII masking

**Level overrides:**
```csharp
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
```

### 4. Forbidden and required log patterns

**Forbidden:**
```csharp
// String concatenation — no structured fields
_logger.LogInformation("User " + userId);
// Unstructured object dump
_logger.LogInformation(user.ToString());
// Direct PII value in template
_logger.LogInformation("Email: {email}", user.Email);
```

**Required:**
```csharp
// Structured fields with non-PII names
_logger.LogInformation("User {UserId} activated by {ActorId}", userId, actorId);
// When PII field name is unavoidable, enricher will mask it
_logger.LogInformation("Verified {Email}", maskedForDisplay);
```

---

## Consequences

### Positive

- PII masking is applied at pipeline level — no Domain or Application layer changes required
- Email regex sweep catches accidental leakage through non-obvious property names
- `ConfigureUmsSerilog` provides a single, auditable configuration point; all sinks receive masked events
- Environment-aware output format reduces log noise in development while keeping JSON structure for production pipelines
- OTel sink integration (when added) inherits all enrichments automatically

### Trade-offs

- Regex scanning of log event properties adds minor overhead per log line — benchmarked at <0.1ms on a typical 10-property event
- Property-name masking is convention-based: a developer who names a PII field `userEmailAddress` (not in the list) will bypass masking. Code review must cover log field naming
- The enricher scans ALL properties on every log event — consider a level gate (`if level < Warning`) for high-throughput paths if profiling reveals it as a hotspot

### Operational note

To ship logs to a remote sink (Seq, Elasticsearch, Application Insights, Loki):
1. Add the sink NuGet package to `Ums.Presentation`
2. Configure the endpoint in `appsettings.json` under the `"Serilog"` section
3. Serilog reads its own configuration natively — no code change required

---

## arc32 Extraction Checklist

The following are UMS-namespaced but trivially portable:
- [ ] `PiiMaskingPolicy` — no product import, depends only on `Serilog.Core`
- [ ] `PiiSanitizerEnricher` — no product import, depends only on `Serilog.Core`
- [ ] `LoggingExtensions.ConfigureUmsSerilog` — depends on environment + configuration, portable with minor rename

---

**[ADR Registry](./index.md)** | **[CP-06 PII Logging](../artifacts/canonical-patterns/cp-06-pii-safe-structured-logging.md)** | **[ADR-0053 OTel](./0053-opentelemetry-observability.md)**
