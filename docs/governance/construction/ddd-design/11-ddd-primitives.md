# Primitivas DDD del Proyecto

**Tipo:** DDD — C# Domain Primitives  
**Version:** 2.0 | **Fecha:** 2026-05-15  
**ADR de referencia:** ADR-0029 — C# native DDD primitives; sin dependencia de biblioteca externa

---

## Clases Base

```csharp
// Entity base con identidad y domain events
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void Raise(IDomainEvent e) => _events.Add(e);
    public void ClearDomainEvents() => _events.Clear();
}

// Aggregate Root marca el limite de consistencia
public abstract class AggregateRoot<TId> : Entity<TId> { }

// Value Object: igualdad estructural, inmutabilidad
public abstract record ValueObject;

// Result pattern: sin excepciones en flujos de negocio
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    public static Result<T> Ok(T value) => new(true, value, null!);
    public static Result<T> Fail(string error) => new(false, default!, error);
}
```

---

## Contrato de Evento de Dominio

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
    string EventType { get; }
}
```

---

## Estructura del Monorepo

```
src/
  Ums.Domain/
    Common/
      Entity.cs
      AggregateRoot.cs
      ValueObject.cs
      Result.cs
      IDomainEvent.cs
    Identity/
      Aggregates/    <- Tenant.cs, UserAccount.cs, Branch.cs
      ValueObjects/  <- Email.cs, UserCategory.cs, TenantType.cs ...
      Events/        <- UserRegisteredEvent.cs, UserActivatedEvent.cs ...
      Repositories/  <- ITenantRepository.cs, IUserAccountRepository.cs
    Authorization/
      Aggregates/    <- SystemSuite.cs, Role.cs, PermissionTemplate.cs, Profile.cs
      ValueObjects/  <- ExclusiveArcTarget.cs, PermissionEffect.cs ...
      Events/        <- ProfileCreatedEvent.cs, PermissionMutatedEvent.cs ...
      Repositories/  <- IProfileRepository.cs, IRoleRepository.cs
    Configuration/
      Aggregates/    <- IdpConfiguration.cs, AppConfiguration.cs, FeatureFlag.cs
      ValueObjects/
      Events/
      Repositories/
    Approvals/
      Aggregates/    <- ApprovalWorkflow.cs, ApprovalRequest.cs
      ValueObjects/
      Events/
      Repositories/
    IGA/
      Aggregates/    <- RolePromotionCriteria.cs, UserPromotionProcess.cs, UserManagementDelegation.cs
      ValueObjects/
      Events/
      Repositories/
    Compliance/
      Aggregates/    <- DocumentType.cs, UserDocument.cs
      ValueObjects/
      Events/
      Repositories/
    Audit/
      Aggregates/    <- AuditRecord.cs
      ValueObjects/
      Repositories/  <- IAuditRepository.cs
```

---

## Reglas de Implementacion (ADR-0029)

| Regla | Detalle |
|-------|---------|
| Primitivas C# nativas | Records para VOs, Entity base class, Result pattern — sin MediatR, Ardalis, ni ErrorOr en dominio |
| Cero dependencias externas | `Ums.Domain` no puede referenciar ningun paquete NuGet excepto los del runtime .NET |
| Value Objects son records | Igualdad estructural automatica via `record`; inmutabilidad via propiedades `init` |
| IDs como Guid | Todos los IDs son `Guid`; nunca `int` ni `string` en Aggregate Roots |
| Composite PK en persistencia | `(Id, RootTenantId)` en todas las tablas; mandatorio para RLS + particionado |

---

**[Anterior: Cross-Context Flows](./10-cross-context-flows.md)** | **[Indice DDD](./index.md)** | **[Siguiente: Design Decisions](./12-design-decisions.md)**
