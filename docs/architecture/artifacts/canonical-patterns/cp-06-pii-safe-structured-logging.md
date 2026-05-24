# CP-06: PII-Safe Structured Logging with Serilog

**Type:** Canonical Pattern  
**Status:** Accepted  
**Evolith disposition:** Proposed for Evolith — no product-specific dependencies; portable to any Serilog + ASP.NET Core satellite  
**Related ADR:** [ADR-0062: PII-Safe Serilog Configuration](../../adrs/0062-pii-safe-serilog-configuration.md)

---

## Problem

Structured logging with Serilog risks leaking PII (email, token, password, national ID) through:
1. Developers explicitly logging PII field values by name
2. Serilog `{@object}` destructuring expanding domain objects that contain PII properties
3. Free-text string values that happen to contain email-shaped content

The Domain layer must remain free of any logging-library annotation (`[Sensitive]` attributes would couple Domain to Serilog).

---

## Pattern

Apply PII masking at the Serilog pipeline level through two complementary components that run before any sink receives the log event.

```
Application code                    Serilog pipeline
──────────────────                  ──────────────────────────────────────────────
_logger.LogXxx(...)   ──────────►  Destructure.With<PiiMaskingPolicy>()
                                     │
                                     ▼
                                   Enrich.With<PiiSanitizerEnricher>()
                                     │  - scan all scalar string properties
                                     │  - mask by property name (case-insensitive list)
                                     │  - mask by regex for email-shaped strings
                                     ▼
                                   WriteTo.Console / WriteTo.OpenTelemetry / WriteTo.*
                                   (PII already scrubbed — sinks receive clean events)
```

---

## Components

### 1. PiiSanitizerEnricher

```csharp
public sealed class PiiSanitizerEnricher : ILogEventEnricher
{
    private static readonly HashSet<string> MaskedNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "email", "emailaddress", "mail",
            "password", "passwordhash", "passwordtext",
            "identityreference",
            "token", "accesstoken", "refreshtoken", "bearertoken", "idtoken",
            "secret", "apikey", "apisecret", "clientsecret",
            "ssn", "nationalid", "taxid",
        };

    private static readonly Regex EmailRegex =
        new(@"[^@\s]+@[^@\s]+\.[^@\s]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(100));

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        foreach (var prop in logEvent.Properties.ToList())
        {
            if (MaskedNames.Contains(prop.Key))
            {
                logEvent.AddOrUpdateProperty(factory.CreateProperty(prop.Key, "[REDACTED]"));
            }
            else if (prop.Value is ScalarValue { Value: string s } && EmailRegex.IsMatch(s))
            {
                logEvent.AddOrUpdateProperty(factory.CreateProperty(prop.Key, MaskEmail(s)));
            }
        }
    }

    private static string MaskEmail(string email)
    {
        var at    = email.IndexOf('@');
        if (at <= 0) return "***@***.***";
        var local  = email[..Math.Min(at, 2)];
        var domain = email[(at + 1)..];
        var dot    = domain.LastIndexOf('.');
        var tld    = dot > 0 ? domain[(dot + 1)..] : "***";
        return $"{local}***@***.{tld}";
    }
}
```

### 2. PiiMaskingPolicy (destructuring hook)

```csharp
// Registered to participate in destructuring chain;
// actual masking happens in PiiSanitizerEnricher (event level)
public sealed class PiiMaskingPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory _,
        out LogEventPropertyValue? result)
    {
        result = null;
        return false; // pass through to enricher
    }
}
```

### 3. ConfigureUmsSerilog — complete wiring

```csharp
public static LoggerConfiguration ConfigureUmsSerilog(
    this LoggerConfiguration cfg,
    HostBuilderContext context)
{
    var env           = context.HostingEnvironment;
    var loggingSection = context.Configuration.GetSection("Observability:Logging");
    var consoleFormat  = loggingSection["ConsoleFormat"]
                         ?? (env.IsDevelopment() ? "Text" : "CompactJson");
    var minimumLevel   = loggingSection["MinimumLevel"]
                         ?? (env.IsDevelopment() ? "Debug" : "Information");

    cfg
        .ReadFrom.Configuration(context.Configuration) // honour appsettings Serilog section
        .Enrich.FromLogContext()                        // picks up ILogger scopes
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.With<PiiSanitizerEnricher>()            // ← PII masking
        .Destructure.With<PiiMaskingPolicy>()
        .MinimumLevel.Is(ParseLevel(minimumLevel))
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
            LogEventLevel.Warning);

    if (consoleFormat.Equals("CompactJson", StringComparison.OrdinalIgnoreCase))
        cfg.WriteTo.Console(new CompactJsonFormatter());
    else
        cfg.WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SessionTrackingId} "
            + "{SourceContext} {Message:lj}{NewLine}{Exception}");

    return cfg;
}
```

### 4. Program.cs wiring

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));

// Optional: structured access log per HTTP request
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, ctx) =>
    {
        diag.Set("CorrelationId",     ctx.TraceIdentifier);
        diag.Set("SessionTrackingId", ctx.Request.Headers[ObservabilityHeaders.SessionTrackingId]
                                          .FirstOrDefault());
    };
});
```

---

## Configuration

```json
"Observability": {
  "Logging": {
    "ConsoleFormat":    "CompactJson",   // "Text" (dev) or "CompactJson" (prod)
    "MinimumLevel":     "Information",   // Debug | Information | Warning | Error
    "OutputTemplate":   "[{Timestamp:HH:mm:ss} ...]"  // Text-mode only
  }
}
```

Remote sinks (Seq, Elasticsearch, Loki, Application Insights):
- Add sink NuGet package to Presentation
- Configure under `"Serilog"` section in appsettings.json
- Serilog reads its own configuration natively — no code change required

---

## Masking Reference

| Property name pattern | Replacement |
|----------------------|-------------|
| `email`, `emailAddress`, `mail` | `jo***@***.com` (partial) |
| `password`, `passwordHash`, `passwordText` | `[REDACTED]` |
| `identityReference` | `[REDACTED]` |
| `token`, `accessToken`, `refreshToken`, `bearerToken`, `idToken` | `[REDACTED]` |
| `secret`, `apiKey`, `apiSecret`, `clientSecret` | `[REDACTED]` |
| `ssn`, `nationalId`, `taxId` | `[REDACTED]` |
| Any scalar string value matching `email@domain.tld` | `jo***@***.tld` |

---

## Forbidden / Required Patterns

```csharp
// ✗ FORBIDDEN — string concatenation, no structured fields
_logger.LogInformation("User " + userId);

// ✗ FORBIDDEN — unstructured object dump
_logger.LogInformation(user.ToString());

// ✗ FORBIDDEN — PII in property value (enricher will catch it, but avoid)
_logger.LogInformation("Created user {Email}", user.Email);

// ✓ REQUIRED — structured fields with non-PII names
_logger.LogInformation("User {UserId} created by {ActorId}", userId, actorId);
```

---

## Related Patterns

- [CP-05: Execution Context Propagation](./cp-05-execution-context-propagation.md) — enriches log lines with CorrelationId and SessionTrackingId via ILogger scope
- [CP-08: AOP Logging Decorator](./cp-08-aop-logging-decorator.md) — uses this pattern for structured AOP entry/exit logs
- [ADR-0062](../../adrs/0062-pii-safe-serilog-configuration.md)
