# Technical Analysis: Ums.Shell Libraries Integration for UMS API

**Date**: 2026-05-23
**Author**: Principal Architecture Review
**Scope**: `Ums.Shell.Factory`, `Ums.Shell.Bootstrapper`, `Ums.Shell.Aop`

---

## Executive Summary

Three libraries have been integrated into the UMS monorepo as local source under `src/libs/shell/`. This document analyzes their fit within the existing Clean Architecture + DDD + CQRS pattern, identifies risks, and provides an incremental adoption plan.

**Verdict**: Adopt selectively. AOP for cross-cutting concerns is valuable; Factory and Bootstrapper should be used sparingly and only where they add clear value over native .NET patterns.

---

## 1. Architecture Fit Analysis

### 1.1 Clean Architecture Alignment

| Layer | Factory | Bootstrapper | AOP | Fit |
|-------|---------|--------------|-----|-----|
| **Domain** | No | No | No | Domain must remain pure POCOs |
| **Application** | Limited | Limited | Yes (validation, logging) | Cross-cutting only |
| **Infrastructure** | Yes | Yes | Yes (transactions, retry) | Best fit |
| **Presentation** | No | Yes (composition root) | Limited | Startup only |

**Rule**: Domain layer must NEVER reference any Shell library. Application layer may reference AOP aspects only through attributes (not direct types).

### 1.2 DDD Bounded Contexts

AOP attributes on Application services (Commands/Queries) are acceptable:

```csharp
// ACCEPTABLE: Application layer
[LoggerAspect]
[TenantValidationAspect]
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result> {
    public async Task<Result> Handle(...) { ... }
}
```

```csharp
// UNACCEPTABLE: Domain layer
[AopAspect]  // NEVER do this
public class User : AggregateRoot { ... }
```

### 1.3 DI Compatibility

All three libraries use `Microsoft.Extensions.DependencyInjection` natively:
- `Ums.Shell.Bootstrapper.DependencyInjection` wraps `IServiceCollection`
- `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` provides `AddAop()` and `AddAopProxy<TService, TImplementation>()` extensions
- `Ums.Shell.Factory`'s `ServiceLocator` is a parallel registry (use with caution)

---

## 2. Recommended Use Cases

### 2.1 AOP - HIGH VALUE

| Use Case | Aspect | Priority |
|----------|--------|----------|
| **Audit logging** | `LoggerAspect` + custom Serilog logger | HIGH |
| **Tenant isolation validation** | Custom `TenantAspect` | HIGH |
| **Retry policies** | `RetryAspect` | MEDIUM |
| **Transaction boundaries** | Custom `TransactionAspect` | HIGH |
| **Performance metrics** | Custom `MetricsAspect` | MEDIUM |
| **Authorization checks** | Custom `AuthorizationAspect` | HIGH |

### 2.2 Bootstrapper - MEDIUM VALUE

| Use Case | Bootstrapper | Priority |
|----------|--------------|----------|
| **Startup composition** | `CompositeBootstrapper` | MEDIUM |
| **DI registration grouping** | `DependencyInjectionBootstrapper` | LOW (use extension methods instead) |
| **AutoMapper setup** | `AutoMapperBootstrapper` | LOW (use `AddAutoMapper()` instead) |

### 2.3 Factory - LOW VALUE

| Use Case | Pattern | Priority |
|----------|---------|----------|
| **Strategy pattern** | Keyed resolution | LOW (use `IKeyedServiceProvider` instead) |
| **Plugin architecture** | Runtime component loading | MEDIUM |
| **Multi-tenant service selection** | Tenant-keyed services | LOW (use tenant middleware instead) |

---

## 3. Architecture Risks

### 3.1 CRITICAL: ServiceLocator Anti-Pattern

`Ums.Shell.Factory.ServiceLocator` implements the Service Locator pattern, which is widely considered an anti-pattern:

```csharp
// ANTI-PATTERN: Service Locator
var service = ServiceLocator.Current.Resolve<IService>("key");

// PREFERRED: Constructor Injection
public class MyHandler {
    private readonly IService _service;
    public MyHandler(IService service) { _service = service; }
}
```

**Risk**: Hidden dependencies, untestable code, runtime resolution failures.

**Mitigation**: If Factory must be used, inject `IServiceLocator` via constructor, never use `ServiceLocator.Current`.

### 3.2 HIGH: AOP Proxy Performance Overhead

`DispatchProxy` uses reflection for method invocation:

```csharp
// Each proxied call:
// 1. MethodInfo cache lookup (ConcurrentDictionary)
// 2. Reflection-based method invocation: targetMethod.Invoke(_target, args)
// 3. Aspect chain execution
```

**Impact**: ~2-5x slower than direct calls for hot paths.

**Mitigation**:
- Only proxy services with cross-cutting needs
- Never proxy high-frequency methods (e.g., in tight loops)
- Use `MethodCache` (already implemented) for MethodInfo caching

### 3.3 HIGH: Aspect Execution Order

Aspects execute in registration order via chain of responsibility. Incorrect ordering causes:

```csharp
// WRONG: Transaction starts AFTER validation fails
services.AddAop(aop => {
    aop.AddAspect<TransactionAspect>();     // Registered first = outer
    aop.AddAspect<ValidationAspect>();       // Inner
});
// Execution: Transaction -> Validation -> Method
// If validation fails, transaction is already started!

// CORRECT: Validation runs first
services.AddAop(aop => {
    aop.AddAspect<ValidationAspect>();       // Outer
    aop.AddAspect<TransactionAspect>();      // Inner
});
// Execution: Validation -> Transaction -> Method
```

**Rule**: Register aspects from outermost to innermost (first registered = outermost).

### 3.4 MEDIUM: Singleton Proxy Limitation

```csharp
// THROWS: "Singleton AOP proxies are not supported"
services.AddAopProxy<ICacheService, CacheService>(ServiceLifetime.Singleton);
```

**Reason**: Aspects may depend on scoped services (DbContext, HttpContext).

**Mitigation**: Use Scoped or Transient for all proxied services.

### 3.5 MEDIUM: AutoMapper 16.x API Change

The `AutoMapperBootstrapper` was adapted for AutoMapper 16.x. The result type changed from `MapperConfiguration` to `MapperConfigurationExpression`:

```csharp
// Old (pre-16):
var config = bootstrapper.Result; // MapperConfiguration

// New (16+):
var expression = bootstrapper.Result; // MapperConfigurationExpression
var config = new MapperConfiguration(expression); // Manual creation
```

---

## 4. Impact on Cross-Cutting Concerns

### 4.1 Logging

**Current**: Manual `ILogger<T>` injection in each class.

**With AOP**:
```csharp
[LoggerAspect]
public class CreateUserCommandHandler : IRequestHandler<...> { }
```

**Pros**: Zero boilerplate, consistent format.
**Cons**: Less control over what gets logged; may log sensitive data.

**Recommendation**: Use AOP logging for entry/exit only. Keep manual logging for business events.

### 4.2 Validation

**Current**: FluentValidation + MediatR pipeline behavior.

**With AOP**:
```csharp
[ValidationAspect]
public class CreateUserCommandHandler : IRequestHandler<...> { }
```

**Recommendation**: Keep MediatR pipeline behavior for validation. AOP validation is redundant and harder to test.

### 4.3 Transactions

**Current**: Manual `IDbContextTransaction` or unit-of-work pattern.

**With AOP**:
```csharp
[TransactionAspect]
public class CreateUserCommandHandler : IRequestHandler<...> { }
```

**Recommendation**: HIGH VALUE. AOP transaction management eliminates boilerplate and ensures consistency.

### 4.4 Tenant Filtering

**Current**: Application-layer tenant filtering in queries.

**With AOP**:
```csharp
[TenantAspect]
public class GetUserQueryHandler : IRequestHandler<...> { }
```

**Recommendation**: Use AOP to validate tenant context on entry. Keep query-level filtering in EF Core configuration.

---

## 5. Anti-Patterns to Avoid

### 5.1 NEVER: AOP on Domain Entities

```csharp
// NEVER DO THIS
[AopAspect]
public class User : AggregateRoot { }
```

Domain entities must be pure POCOs with zero framework dependencies.

### 5.2 NEVER: ServiceLocator.Current

```csharp
// NEVER DO THIS
var service = ServiceLocator.Current.Resolve<IService>("key");
```

Always use constructor injection.

### 5.3 NEVER: AOP for Business Logic

```csharp
// NEVER DO THIS - business logic in aspects
public class DiscountAspect : OnMethodBoundaryAspect<...> {
    protected override void OnEntry(IJoinPoint jp) {
        // Business logic here = BAD
        if (jp.Arguments[0] is Order order) {
            order.ApplyDiscount(); // WRONG
        }
    }
}
```

Aspects should only handle cross-cutting concerns: logging, transactions, retries, metrics.

### 5.4 NEVER: Circular Aspect Dependencies

```csharp
// NEVER: Aspect A depends on service B, which is proxied with Aspect A
services.AddAopProxy<IServiceA, ServiceA>(); // Has LoggingAspect
// ServiceA constructor depends on IServiceB
services.AddAopProxy<IServiceB, ServiceB>(); // Has LoggingAspect
// ServiceB constructor depends on IServiceA = STACK OVERFLOW
```

### 5.5 AVOID: Bootstrapper for Simple DI

```csharp
// AVOID: Over-engineered
var bootstrapper = new DependencyInjectionBootstrapper(services => {
    services.AddScoped<IMyService, MyService>();
});
bootstrapper.Run();

// PREFER: Native extension method
public static class ServiceCollectionExtensions {
    public static IServiceCollection AddMyServices(this IServiceCollection services) {
        services.AddScoped<IMyService, MyService>();
        return services;
    }
}
// In Program.cs:
builder.Services.AddMyServices();
```

---

## 6. Incremental Adoption Plan

### Phase 1: Foundation (Week 1-2)

1. Add project references to `Ums.Presentation` (startup project)
2. Register AOP infrastructure in `Program.cs`:
   ```csharp
   builder.Services.AddAop(aop => {
       aop.AddLogger<SerilogLogger>();
   });
   ```
3. Proxy 1-2 non-critical services to validate setup

### Phase 2: Logging (Week 3-4)

1. Create `SerilogLogger` implementing `Ums.Shell.Aop.Aspects.ILogger`
2. Add `[LoggerAspect]` to all Command/Query handlers
3. Verify log output format and performance

### Phase 3: Transactions (Week 5-6)

1. Create `TransactionAspect` using `OnMethodBoundaryAspect`
2. Apply to write operations (Commands)
3. Test rollback scenarios

### Phase 4: Tenant Validation (Week 7-8)

1. Create `TenantValidationAspect`
2. Apply to all handlers that access tenant-scoped data
3. Verify tenant isolation

### Phase 5: Metrics & Retry (Week 9-10)

1. Create `MetricsAspect` for OpenTelemetry
2. Apply `[RetryAspect]` to external service calls
3. Performance benchmarking

### Phase 6: Review & Refine (Week 11-12)

1. Architecture review
2. Performance profiling
3. Documentation update
4. Remove unused patterns

---

## 7. Final Recommendation

### Adopt:
- **AOP** for logging, transactions, tenant validation, and retry policies
- **Bootstrapper** only for complex startup composition (if needed)

### Use Sparingly:
- **Factory** only for plugin/strategy patterns where keyed resolution is necessary

### Avoid:
- ServiceLocator pattern
- AOP on domain entities
- AOP for business logic
- Bootstrapper for simple DI registration

### Key Principle:
> The Shell libraries are infrastructure tools. They belong in the outermost layers (Presentation, Infrastructure) and must never penetrate the Domain or Application core. Clean Architecture boundaries remain absolute.

---

## Appendix: Library Dependency Graph

```
Ums.Presentation (Web API)
├── Ums.Shell.Bootstrapper.DependencyInjection
│   └── Ums.Shell.Bootstrapper
├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer
│   ├── Ums.Shell.Aop.Aspects
│   │   └── Ums.Shell.Aop
│   ├── Ums.Shell.Aop.DispatchProxy
│   │   └── Ums.Shell.Aop
│   └── Ums.Shell.Aop.Aspects.Logger.Serilog
│       └── Ums.Shell.Aop.Aspects
└── Ums.Shell.Factory (optional)

Ums.Application
└── (AOP attributes only, no direct library references)

Ums.Domain
└── (NO Shell references - pure POCOs)

Ums.Infrastructure
├── Ums.Shell.Factory (if needed for strategy patterns)
└── Ums.Shell.Ddd (existing)
```
