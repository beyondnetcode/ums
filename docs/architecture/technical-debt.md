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

---

## [TD-002] GraphQL Queries Return Empty Data for AppConfigurations

- **Status**: Acknowledged
- **Severity**: Medium
- **Component**: `Ums.Presentation/GraphQL/Configuration/AppConfigurationQueries.cs`, `Ums.Application/Configuration/AppConfiguration/Queries/GetAllAppConfigurationsQueryHandler.cs`
- **Description**: GraphQL endpoint for AppConfigurations (`appConfigurations` query) returns empty items array with `totalItems: 0` despite REST endpoint returning correct data (7 items). The handler executes successfully (~25ms) without errors, but the result is empty. Other GraphQL queries (e.g., `tenants`) work correctly.
- **Rationale**: Investigation did not reveal root cause. Same handler logic works for REST, same repository returns data for REST but not for GraphQL. Possible HotChocolate DataLoader or resolver scope issue.
- **Impact**:
  - Frontend falls back to REST due to `FRONTEND_CONFIG_TRANSPORT = "rest"` flag.
  - GraphQL remains non-functional for AppConfiguration bounded context.
  - Reduced flexibility for frontend data fetching choices.
- **Workaround**: Use `FRONTEND_CONFIG_TRANSPORT = "rest"` flag in BD (current default).
- **Target Resolution**: Investigate HotChocolate resolver execution context, DataLoader caching, or any middleware that could affect GraphQL resolution differently from REST.
- **Related Files**:
- `src/apps/ums.api/Ums.Presentation/GraphQL/Configuration/AppConfigurationQueries.cs`
- `src/apps/ums.api/Ums.Application/Configuration/AppConfiguration/Queries/GetAllAppConfigurationsQueryHandler.cs`

---

## [TD-003] Prepare Configuration System for Redis Migration

- **Status**: Proposed
- **Severity**: Low
- **Component**: `Ums.Infrastructure/Configuration/`, `Ums.Application/Configuration/`
- **Description**: The parameterization system already runs on an in-memory cache abstraction. The remaining debt is to migrate the cache implementation to Redis without changing the business-facing `IConfigurationProvider` or the typed `ConfigurationValues` consumers.
- **Rationale**: Initial implementation uses in-memory storage for simplicity and fast iteration. Redis will be needed when UMS scales to multiple API instances requiring shared configuration state and shared cache invalidation.
- **Impact**:
  - Current in-memory implementation may show stale data if multiple API instances have different cache states.
  - No distributed cache invalidation across pods.
  - Parameter changes still depend on the current reload path rather than a distributed cache event.
- **Mitigation**:
  - Introduce `IConfigurationCache` abstraction from day one.
  - Implement `InMemoryConfigurationCache` as the initial concrete implementation.
  - Design `ConfigurationProvider` to depend on `IConfigurationCache`, not on concrete implementation.
  - Use `ConfigurationValues` for strongly-typed consumers so the migration does not leak cache concerns into handlers or validators.
  - Document the abstraction interface and future migration steps.
- **Target Resolution**: Phase 2 (when scaling to multiple API instances or when Redis infrastructure is available).
- **Related Documents**:
  - [Parameterization System Specification](../governance/construction/ddd-design/parameterization-system-spec.md)
  - [TODO-003](../governance/project/TODO.md#todo-003-implement-parameterization-system-with-loader-and-provider)
