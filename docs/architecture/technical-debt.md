# Technical Debt Registry

> Tracked items that are acknowledged deviations from ideal architecture but are deferred for strategic reasons.
> Each item includes a rationale, impact, and target resolution window.

---

## [TD-001] Monolithic `UmsPlatformDbContext`

- **Status**: Acknowledged
- **Severity**: Low
- **Component**: `Ums.Infrastructure/Persistence/UmsPlatformDbContext.cs`
- **Description**: All bounded contexts (Identity, Authorization, Configuration, Approvals, IGA, Audit) share a single `DbContext` instance (`UmsPlatformDbContext`). This violates strict bounded-context physical isolation and introduces coupling risks during migrations, schema evolution, and team parallelism.
- **Rationale**: Pragmatic choice for a modular monolith prototype. Single DbContext simplifies transaction management, outbox pattern implementation, and dev-data seeding across contexts.
- **Impact**:
  - Migration conflicts when multiple teams modify entity configurations simultaneously.
  - Single point of failure: a misconfigured entity in one context can break the entire persistence layer.
  - Harder to extract contexts into separate services or databases in the future.
- **Mitigation**:
  - Logical schema separation via `ToTable(name, schema)` per context.
  - Strict `IEntityTypeConfiguration<T>` isolation per bounded context.
  - No cross-context entity references in configurations.
- **Target Resolution**: Phase 3 (modular monolith → distributed persistence) or when team size exceeds 4 developers working concurrently on different contexts.
- **Related ADRs**: ADR-0066 (Database Schema per Module), ADR-0067 (Modular Monolith Schema per Domain).
