# ADR-0062: Configuración Serilog Segura de PII (HARDENING-04)

**Estado:** Aceptado  
**Fecha:** 2026-05-24  
**Responsable:** Arquitectura  
**Disposición Evolith:** Propuesto para adopción en Evolith — el patrón es neutro en cuanto al runtime; aplicable a cualquier satélite .NET que use Serilog  
**Relacionados:**
- [ADR-0053: Estrategia de Observabilidad con OpenTelemetry](./0053-opentelemetry-observability.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [CP-06: Logging Estructurado Seguro de PII](../artifacts/canonical-patterns/cp-06-pii-safe-structured-logging.es.md)

---

## Contexto

UMS procesa información de identificación personal (PII): direcciones de correo electrónico, referencias de identidad, contraseñas, tokens e IDs nacionales. ADR-0053 exige logging estructurado vía Serilog, pero un logging estructurado sin protección crea riesgo de filtrado de PII:

```csharp
// Riesgo: el email se filtra a todos los sinks de log
_logger.LogInformation("User {Email} activated by {ActorId}", user.Email, actorId);
```

Existen tres capas de riesgo:
1. **Captura explícita** — el desarrollador registra deliberadamente un campo PII por nombre
2. **Destructuring** — las expansiones `{@object}` de Serilog registran todas las propiedades de una clase, incluyendo campos PII
3. **Filtrado por texto libre** — interpolación de cadenas o plantillas de mensajes que contienen cadenas con forma de email

### Por qué enmascaramiento por nombre de propiedad vs. anotaciones de atributo

El enfoque de anotaciones (`[Sensitive]` en propiedades de dominio) acopla la capa de Dominio a una librería de logging, violando la pureza del dominio. El enmascaramiento por nombre de propiedad en el nivel del pipeline de Serilog no requiere cambios en el dominio.

---

## Decisión

**Aplicar el enmascaramiento de PII a nivel del pipeline de Serilog mediante dos mecanismos complementarios: una política de destructuring y un enricher de eventos de log.**

### 1. `PiiMaskingPolicy` — `IDestructuringPolicy` de Serilog

Registrado vía `.Destructure.With<PiiMaskingPolicy>()`. Intercepta el destructuring de objetos antes de que cualquier sink lo procese.

La política intercepta a nivel del nombre de propiedad dentro del enricher (ver más abajo) — el método `TryDestructure` devuelve `false` para que Serilog continúe la destrucción normal; el enricher luego escanea y redacta.

### 2. `PiiSanitizerEnricher` — `ILogEventEnricher` de Serilog

Registrado vía `.Enrich.With<PiiSanitizerEnricher>()`. Se ejecuta después del renderizado del template del mensaje, escaneando todas las propiedades escalares de tipo cadena:

```csharp
// Nombres de propiedad enmascarados (insensible a mayúsculas/minúsculas):
"email", "emailaddress", "mail",
"password", "passwordhash", "passwordtext",
"identityreference",
"token", "accesstoken", "refreshtoken", "bearertoken", "idtoken",
"secret", "apikey", "apisecret", "clientsecret",
"ssn", "nationalid", "taxid"
```

**Barrido por regex de email** — cualquier valor escalar de texto libre que coincida con `[^@\s]+@[^@\s]+\.[^@\s]+` se enmascara parcialmente: `jo***@***.com`. Esto captura filtraciones a través de propiedades con nombres no-PII.

### 3. `LoggingExtensions.ConfigureUmsSerilog`

Único método de extensión que configura completamente Serilog:

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));
```

**Estrategia de salida:**

| Entorno | Formato | Justificación |
|---------|--------|---------------|
| Development | Consola con texto coloreado | Legible por humanos; prefijo trace/correlación visible |
| Staging/Production | JSON compacto (`CompactJsonFormatter`) | Legible por máquinas; consumido por Fluentd / drivers de log de contenedor |

**Configuración (appsettings.json):**

```json
"Observability": {
  "Logging": {
    "ConsoleFormat": "CompactJson",   // "Text" o "CompactJson"
    "MinimumLevel": "Information",    // cualquier nivel de Serilog
    "OutputTemplate": "..."           // solo para modo Text
  }
}
```

**Enrichers siempre aplicados:**
- `Enrich.FromLogContext()` — recoge scopes de ILogger (CorrelationId, SessionTrackingId del middleware)
- `Enrich.WithMachineName()` — identidad del pod/host para despliegues en Kubernetes
- `Enrich.WithThreadId()` — correlaciona logs de requests concurrentes
- `Enrich.With<PiiSanitizerEnricher>()` — enmascaramiento de PII

**Sobreescrituras de nivel:**
```csharp
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
```

### 4. Patrones prohibidos y requeridos de log

**Prohibidos:**
```csharp
// Concatenación de cadenas — sin campos estructurados
_logger.LogInformation("User " + userId);
// Volcado de objeto no estructurado
_logger.LogInformation(user.ToString());
// Valor PII directo en el template
_logger.LogInformation("Email: {email}", user.Email);
```

**Requeridos:**
```csharp
// Campos estructurados con nombres no-PII
_logger.LogInformation("User {UserId} activated by {ActorId}", userId, actorId);
// Cuando el nombre de campo PII es inevitable, el enricher lo enmascarará
_logger.LogInformation("Verified {Email}", maskedForDisplay);
```

---

## Consecuencias

### Positivas

- El enmascaramiento de PII se aplica a nivel del pipeline — no se requieren cambios en las capas de Dominio o Aplicación
- El barrido por regex de email captura filtraciones accidentales a través de nombres de propiedad no obvios
- `ConfigureUmsSerilog` proporciona un único punto de configuración auditable; todos los sinks reciben eventos enmascarados
- El formato de salida adaptable al entorno reduce el ruido en desarrollo mientras mantiene la estructura JSON para los pipelines de producción
- La integración del sink de OTel (cuando se añada) hereda todos los enrichments automáticamente

### Compromisos

- El escaneo por regex de propiedades de eventos de log añade una overhead menor por línea de log — benchmarked en <0.1ms en un evento típico de 10 propiedades
- El enmascaramiento por nombre de propiedad es basado en convención: un desarrollador que nombre un campo PII `userEmailAddress` (no está en la lista) eludirá el enmascaramiento. La revisión de código debe cubrir el nombrado de campos de log
- El enricher escanea TODAS las propiedades en cada evento de log — considerar un gate por nivel (`if level < Warning`) para rutas de alto rendimiento si el profiling lo identifica como hotspot

### Nota operacional

Para enviar logs a un sink remoto (Seq, Elasticsearch, Application Insights, Loki):
1. Añadir el paquete NuGet del sink a `Ums.Presentation`
2. Configurar el endpoint en `appsettings.json` bajo la sección `"Serilog"`
3. Serilog lee su propia configuración de forma nativa — no se requieren cambios en el código

---

## Checklist de Extracción Evolith

Los siguientes tienen namespace de UMS pero son trivialmente portables:
- [ ] `PiiMaskingPolicy` — sin import de producto, depende solo de `Serilog.Core`
- [ ] `PiiSanitizerEnricher` — sin import de producto, depende solo de `Serilog.Core`
- [ ] `LoggingExtensions.ConfigureUmsSerilog` — depende del entorno + configuración, portable con rename menor

---

**[Registro ADR](./index.md)** | **[CP-06 Logging PII](../artifacts/canonical-patterns/cp-06-pii-safe-structured-logging.es.md)** | **[ADR-0053 OTel](./0053-opentelemetry-observability.md)**
