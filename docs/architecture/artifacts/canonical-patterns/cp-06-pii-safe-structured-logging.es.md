# CP-06: Logging Estructurado Seguro de PII con Serilog

**Tipo:** Patrón Canónico  
**Estado:** Aceptado  
**Disposición Evolith:** Propuesto para Evolith — sin dependencias específicas del producto; portable a cualquier satélite Serilog + ASP.NET Core  
**ADR relacionado:** [ADR-0062: Configuración Serilog Segura de PII](../../adrs/0062-pii-safe-serilog-configuration.md)

---

## Problema

El logging estructurado con Serilog arriesga filtrar PII (email, token, contraseña, ID nacional) a través de:
1. Desarrolladores que registran explícitamente valores de campos PII por nombre
2. Destructuring `{@object}` de Serilog que expande objetos de dominio con propiedades PII
3. Valores de cadena en texto libre que contienen contenido con forma de email

La capa de Dominio debe permanecer libre de cualquier anotación de librería de logging (los atributos `[Sensitive]` acoplarían el Dominio a Serilog).

---

## Patrón

Aplicar el enmascaramiento de PII a nivel del pipeline de Serilog a través de dos componentes complementarios que se ejecutan antes de que cualquier sink reciba el evento de log.

```
Código de aplicación                Serilog pipeline
──────────────────                  ──────────────────────────────────────────────
_logger.LogXxx(...)   ──────────►  Destructure.With<PiiMaskingPolicy>()
                                     │
                                     ▼
                                   Enrich.With<PiiSanitizerEnricher>()
                                     │  - escanea todas las propiedades escalares de cadena
                                     │  - enmascara por nombre de propiedad (lista insensible a mayúsculas)
                                     │  - enmascara por regex para cadenas con forma de email
                                     ▼
                                   WriteTo.Console / WriteTo.OpenTelemetry / WriteTo.*
                                   (PII ya depurado — los sinks reciben eventos limpios)
```

---

## Componentes

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

### 2. PiiMaskingPolicy (hook de destructuring)

```csharp
// Registrada para participar en la cadena de destructuring;
// el enmascaramiento real ocurre en PiiSanitizerEnricher (nivel de evento)
public sealed class PiiMaskingPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory _,
        out LogEventPropertyValue? result)
    {
        result = null;
        return false; // pasar al enricher
    }
}
```

### 3. ConfigureUmsSerilog — configuración completa

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
        .ReadFrom.Configuration(context.Configuration) // respetar sección Serilog de appsettings
        .Enrich.FromLogContext()                        // recoge scopes de ILogger
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.With<PiiSanitizerEnricher>()            // ← enmascaramiento PII
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

### 4. Configuración en Program.cs

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));

// Opcional: log de acceso estructurado por request HTTP
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

## Configuración

```json
"Observability": {
  "Logging": {
    "ConsoleFormat":    "CompactJson",   // "Text" (dev) o "CompactJson" (prod)
    "MinimumLevel":     "Information",   // Debug | Information | Warning | Error
    "OutputTemplate":   "[{Timestamp:HH:mm:ss} ...]"  // solo modo Text
  }
}
```

Sinks remotos (Seq, Elasticsearch, Loki, Application Insights):
- Añadir paquete NuGet del sink a Presentation
- Configurar bajo la sección `"Serilog"` en appsettings.json
- Serilog lee su propia configuración de forma nativa — no se requieren cambios en el código

---

## Referencia de Enmascaramiento

| Patrón de nombre de propiedad | Reemplazo |
|-------------------------------|-----------|
| `email`, `emailAddress`, `mail` | `jo***@***.com` (parcial) |
| `password`, `passwordHash`, `passwordText` | `[REDACTED]` |
| `identityReference` | `[REDACTED]` |
| `token`, `accessToken`, `refreshToken`, `bearerToken`, `idToken` | `[REDACTED]` |
| `secret`, `apiKey`, `apiSecret`, `clientSecret` | `[REDACTED]` |
| `ssn`, `nationalId`, `taxId` | `[REDACTED]` |
| Cualquier valor escalar de cadena que coincida con `email@domain.tld` | `jo***@***.tld` |

---

## Patrones Prohibidos / Requeridos

```csharp
// ✗ PROHIBIDO — concatenación de cadenas, sin campos estructurados
_logger.LogInformation("User " + userId);

// ✗ PROHIBIDO — volcado de objeto no estructurado
_logger.LogInformation(user.ToString());

// ✗ PROHIBIDO — PII en valor de propiedad (el enricher lo capturaría, pero evitar)
_logger.LogInformation("Created user {Email}", user.Email);

// ✓ REQUERIDO — campos estructurados con nombres no-PII
_logger.LogInformation("User {UserId} created by {ActorId}", userId, actorId);
```

---

## Patrones Relacionados

- [CP-05: Propagación del Contexto de Ejecución](./cp-05-execution-context-propagation.es.md) — enriquece las líneas de log con CorrelationId y SessionTrackingId vía scope de ILogger
- [CP-08: Decorator de Logging AOP](./cp-08-aop-logging-decorator.es.md) — usa este patrón para logs estructurados de entrada/salida AOP
- [ADR-0062](../../adrs/0062-pii-safe-serilog-configuration.md)
