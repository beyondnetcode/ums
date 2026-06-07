# ADR-0069: Domain Layer Inheritance Strategy - AggregateRoot Base Class

> **Promoted to Evolith:** This ADR has been elevated to [Evolith ADR-0071 — Domain Layer Base Class and Inheritance Strategy](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0071-domain-layer-base-class-inheritance-strategy.md). UMS retains this document as implementation reference.**Status:**Accepted**Date:**2026-05-29**Deciders:**Architecture Team**Supersedes:**ADR-0054 (Shell Library Isolation)

---

## Context

The UMS Domain layer (`Ums.Domain`) currently inherits from `AggregateRoot<T>` and `Entity<T>` base classes provided by `BeyondNetCode.Shell.Ddd`. This creates a transitive dependency on**MediatR** (v12.3.0) through the shell library.

### Current Architecture

```
Ums.Domain (Project)
 └── ProjectReference → BeyondNetCode.Shell.Ddd
 └── PackageReference → MediatR (12.3.0)
```

### Relevant Code**Domain Entity Example** (`src/apps/ums.api/Ums.Domain/Authorization/Profile/Profile.cs`):
```csharp
public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
 // Inherits: Id, BrokenRules, IsValid(), DomainEvents
}
```

**Shell Ddd Base Class** (`libs/shell/ddd/src/BeyondNetCode.Shell.Ddd/`):
```csharp
public abstract class AggregateRoot<T, TProps> : Entity<T, TProps>
 where TProps : Props
{
 public IDomainEvents DomainEvents { get; }
 // ... MediatR integration
}
```

---

## Decision**Option A (CURRENT):**Continue using inheritance from `BeyondNetCode.Shell.Ddd` base classes.

**Option B:**Refactor to composition-based design, removing inheritance from shell.

**Option C:**Create a "Domain.Abstractions" project with pure interfaces, removing MediatR from shell.

### Decision Made: **Option A (Maintain Current)**with documented justification.

---

## Pros and Cons Analysis

### Option A: Continue Inheritance from Shell.Ddd

#### Pros
| # | Pro | Rationale |
|---|-----|-----------|
| 1 | **Development Velocity** | Teams can focus on business logic, not infrastructure boilerplate |
| 2 | **Consistent Patterns** | All aggregates share identical base behavior (ID, broken rules, events) |
| 3 | **MediatR as Infrastructure** | MediatR is at the shell level, not in Domain directly - it's a framework concern |
| 4 | **Battle-Tested** | Shell libraries are shared across multiple projects (csdevlib) |
| 5 | **Less Code to Maintain** | No duplicate `AggregateRoot<T>` implementations needed | #### Cons
| # | Con | Rationale |
|---|-----|-----------|
| 1 | **Violation of Strict Domain Purity** | Domain references external NuGet transitively |
| 2 | **Framework Leakage Risk** | If shell evolves, Domain may be forced to change |
| 3 | **Testing Complexity** | Domain tests require MediatR assembly resolution |
| 4 | **Conceptual Contamination** | Domain concept of "Aggregate" knows about MediatR |
| 5 | **Portability Loss** | Domain cannot be extracted to separate package | ---

### Option B: Composition-Based Refactor

#### Pros
| # | Pro | Rationale |
|---|-----|-----------|
| 1 | **True Domain Purity** | Domain has zero external dependencies |
| 2 | **Extractable** | Domain could be published as standalone NuGet |
| 3 | **Testability** | Pure POCOs easier to unit test |
| 4 | **No Conceptual Coupling** | Domain doesn't know about MediatR | #### Cons
| # | Con | Rationale |
|---|-----|-----------|
| 1 | **Massive Refactoring** | 100+ entities need modification |
| 2 | **Lost Consistency** | Each aggregate may implement patterns differently |
| 3 | **Boilerplate Proliferation** | Common behavior duplicated across aggregates |
| 4 | **Breaking Change** | Allbounded contexts affected |
| 5 | **Time Investment** | Estimated 3-4 weeks of work | ---

### Option C: Domain.Abstractions (Hybrid)

Create `Ums.Domain.Abstractions` with pure interfaces:
- `IAggregateRoot<T>`
- `IEntity<T, TProps>`
- `IDomainEvents`

Shell.Ddd implements these interfaces. Domain references only Abstractions.

#### Pros
| # | Pro | Rationale |
|---|-----|-----------|
| 1 | **Architectural Clarity** | Clear separation between contracts and implementation |
| 2 | **Domain Purity** | Domain only depends on Abstractions (no NuGets) |
| 3 | **Flexibility** | Can swap shell implementation if needed |
| 4 | **Maintainable** | Changes to MediatR don't ripple to Domain | #### Cons
| # | Con | Rationale |
|---|-----|-----------|
| 1 | **Another Layer** | Adds indirection without immediate benefit |
| 2 | **Over-Engineering Risk** | 3 projects where 2 might suffice |
| 3 | **Migration Effort** | Requires creating Abstractions project and updating references |
| 4 | **Build Complexity** | More projects = longer build times | ---

## Selected Approach: Option A (Current)

### Justification

1. **BMAD Rule R-10**states: "Domain must be pure POCOs with zero NuGet references" - but this rule is**aspirational**in strict interpretation. The transitive nature of shell dependencies is a**controlled compromise**.

2. **Pragmatic Architecture**: The shell library is a**shared kernel** (DDD concept). MediatR is not a Domain concern - it's infrastructure for handling commands/queries. The Domain doesn't invoke MediatR directly; it merely uses base classes that happen to include it.

3. **Risk Assessment**:
- If MediatR changes major version → Shell.Ddd updates, Domain doesn't change
- If Shell.Ddd changes → We control both, can migrate together
- Risk of MediatR coupling in Domain is**contained within shell**4. **Historical Context**: This architecture was deliberately designed in ADR-0054 as a trade-off between purity and productivity.

---

## Implementation Notes

### If We Later Choose Option C (Domain.Abstractions)

Migration path:
1. Create `Ums.Domain.Abstractions` project
2. Define `IAggregateRoot<T>`, `IEntity<T>`, `IDomainEvents` interfaces
3. Make `AggregateRoot<T>` in shell implement these interfaces
4. Update Domain project references from `Shell.Ddd` to `Shell.Ddd.Abstractions`

### Validation

To verify Domain purity violation severity, run:
```bash
dotnet list src/apps/ums.api/Ums.Domain/Ums.Domain.csproj package
# Should show NO direct package references
```

---

## Consequences

### Positive
- Development velocity maintained
- Consistent aggregate implementation across all bounded contexts
- Shell libraries can evolve independently

### Negative
- Strict interpretation of BMAD R-10 is violated
- Domain cannot be published as standalone package
- Teams must understand the transitive dependency model

### Mitigations
- Document this architectural decision clearly
- Ensure shell.Ddd has stability guarantees (versioning policy)
- Consider Option C if portability becomes a requirement

---

## References

- [ADR-0054: Shell Library Isolation](0054-shell-library-isolation.md)
- [BMAD Rule R-10: Domain Purity](../../../.bmad-core/rules/global-rules.md)
- [MediatR Documentation](https://github.com/mattroberts297/MediatR)
- [DDD Shared Kernel Pattern](https://martinfowler.com/articles/refactoring-the-keynote/index.html#SharedKernel)

---

## Spanish Translation / Traducción al Español

Ver: [0069-domain-inheritance-strategy.es.md](./0069-domain-inheritance-strategy.es.md)
