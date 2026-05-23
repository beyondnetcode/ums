# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Repository Layout

```
/
├── src/
│   ├── apps/
│   │   ├── ums.api/          ← .NET 10 backend (primary codebase)
│   │   └── ums.web-app/      ← React 18 + TypeScript frontend
│   └── libs/shell/ddd/       ← Internal DDD shell libraries (shared; no NuGet)
├── docs/                     ← Architecture docs, ADRs, governance
└── AGENTS.md                 ← Agent rules (read this first for conventions)
```

All backend commands run from `src/apps/ums.api/` unless noted.

---

## Backend Commands (from `src/apps/ums.api/`)

```bash
# Build
dotnet build Ums.sln

# Run all tests
dotnet test Ums.sln

# Run a single test class
dotnet test Ums.Application.Test --filter "FullyQualifiedName~CreateTenantCommandHandlerTests"

# Run a single test method
dotnet test Ums.Application.Test --filter "FullyQualifiedName~T01_Create_Success"

# Run the API
dotnet run --project Ums.Presentation

# Coverage (macOS arm64 — uses coverlet.console local tool)
dotnet tool restore          # first time only
bash coverage.sh             # generates coverage/report/index.html
bash coverage.sh --ci        # exits non-zero if combined line coverage < 80%
```

---

## Frontend Commands (from `src/`)

```bash
npm install
npx nx run app-web:dev       # start dev server
npx tsc --noEmit             # type check
npx eslint "src/**/*.{ts,tsx}"
npx vitest run
```

---

## Backend Architecture

### Layer Boundaries (enforced, do not cross)

```
Ums.Domain          ← pure POCOs, zero NuGet references
Ums.Application     ← use cases, interfaces (ICommandHandler, IQueryHandler)
Ums.Infrastructure  ← EF Core, SQL Server, background services, outbox
Ums.Presentation    ← Minimal API endpoints + GraphQL (HotChocolate)
```

**Shell libraries** (`src/libs/shell/ddd/`) provide base types used across all layers:
- `Ums.Shell.Ddd` — `IAggregateRoot`, `Result<T>`, `ValueObject<T>`, `IDomainEvent`
- `Ums.Shell.Ddd.ValueObjects` — `AuditValueObject`, `IdValueObject`, common VOs

### Bounded Contexts

Eight contexts, each isolated in Domain + Application + Infrastructure subfolders:

| Context | Key aggregates |
|---|---|
| Identity | `Tenant`, `UserAccount`, `UserManagementDelegation` |
| Authorization | `Profile`, `SystemSuite`, `PermissionTemplate` |
| Configuration | `AppConfiguration`, `FeatureFlag`, `IdpConfiguration` |
| Audit | `AuditRecord` |
| Approvals | `ApprovalWorkflow`, `ApprovalRequest`, `DocumentType`, `UserDocument`, `AccessEnforcementPolicy`, `NotificationRule` |
| IGA | `PromotionRequest`, `RoleMaturityStatus` |

### CQRS Split

- **Commands** → REST Minimal API endpoints (`MapXxxEndpoints()` in Presentation)
- **Queries** → GraphQL via HotChocolate (`MapXxxQueryEndpoints()`) **or** REST query endpoints
- Handler registration: MediatR pipeline with `ValidationBehavior` (FluentValidation) → handler

### Result Pattern

All handlers return `Result` or `Result<T>`. Never throw for flow control.

```csharp
// Domain operation
var result = aggregate.Activate(actorId);
if (result.IsFailure) return Result.Failure(result.Error);

// HTTP mapping in Presentation
return result.ToHttpResult(); // uses DomainErrorStatusMapper
```

`DomainErrorStatusMapper` maps error prefixes to HTTP status:
- `"Validation.Failed:"` → 422
- `"not found"` → 404
- `"already exists"` / `"conflict"` → 409
- `"Authenticated user is required"` → 401

### Persistence Options (appsettings.json)

```json
"Persistence": {
  "Provider": "SqlServer",           // or "InMemory"
  "UseSqlServerIdentityStores": true,
  "UseSqlServerAuthorizationStores": true,
  "UseSqlServerConfigurationStores": true,
  "SeedDevData": true,
  "EnableOutbox": true,
  "InitializePlatformStoreOnStartup": true
}
```

**Dev default** (`appsettings.Development.json`): All `UseSqlServer*` = true, `InitializePlatformStoreOnStartup` = true.  
**Approvals / IGA / SystemSuite / PermissionTemplate** are still InMemory — SQL repos are TODO (see `DependencyInjection.cs` TODOs).

### Server-Side Pagination (REC-12)

All list queries use `GetPagedAsync(page, pageSize, search?, status?, sortBy, sortOrder, ...)` returning `(IReadOnlyList<T> Items, int TotalCount)`. SQL repos use EF Core `Skip/Take` at the DB level. InMemory repos apply equivalent filtering in memory. Handlers receive a `PagedQueryParameters` record from the query object.

### Soft-Delete + GDPR Anonymization (REC-16)

Entities with soft-delete carry `IsDeleted`, `DeletedAtUtc`, `DeletedBy`. `UserAccountRecord` additionally has `AnonymizedAtUtc`.

- **EF global filter**: `!x.IsDeleted` is combined into every `HasQueryFilter` in `UmsPlatformDbContext`.
- **Handler pattern**: call `aggregate.Delete(actorId)` → `UpdateAsync(aggregate)` → `SoftDeleteAsync(id, deletedBy)` → `SaveEntitiesAsync()`. This order is required so the aggregate is in `_trackedAggregates` for outbox message creation before the soft-delete fields are stamped.
- **GDPR**: `SoftDeleteAsync` for `UserAccountRecord` replaces `Email` with `gdpr_del_{sha256_16}@anonymized.invalid` and nulls `IdentityReference`. The SHA-256 is deterministic (keyed on the row GUID) so it is irreversible but auditable.
- **Migration**: `20260523_soft_delete_gdpr.sql` — idempotent `ALTER TABLE` + filtered index `WHERE [IsDeleted] = 0`.
- **IgnoreQueryFilters**: use in `SoftDeleteAsync` implementations to locate already-deleted rows for idempotence checks.

### Tenant Isolation (two-layer)

1. **Primary**: EF Core `HasQueryFilter` on every tenant-scoped entity in `UmsPlatformDbContext`. Filter: `!tenantContext.OrganizationId.HasValue || x.TenantId == tenantContext.OrganizationId.Value`.
2. **Failsafe**: SQL Server RLS predicates set via `OrganizationDbContextInterceptor` (`sp_set_session_context`).

`AppConfigurationRecord` additionally allows `TenantId IS NULL` (global configs visible to all tenants).

### Idempotency (REC-10)

POST/PUT/PATCH endpoints accept an `Idempotency-Key` header (UUID). The middleware stores the first response in `IdempotencyStore` (InMemory / SQL) and replays it verbatim for duplicates within the TTL window. Keys must be client-generated UUIDs. The store is registered in DI via `AddIdempotency()`.

### ETag / Optimistic Concurrency on HTTP (REC-10)

`GET` responses for individual aggregates include an `ETag` header (the aggregate's `RowVersion` as a base64 string). Mutating requests (`PUT`/`PATCH`) should send `If-Match: <etag>`. A mismatch returns HTTP 409 via `ConcurrencyConflictException`.

### Correlation ID / OpenTelemetry (REC-17)

`UseCorrelationId` middleware reads or generates an `X-Correlation-Id` header and injects it into the OTel activity bag. All log entries carry `CorrelationId` via Serilog enrichment. The same ID is forwarded to downstream HTTP calls through `DelegatingHandler`.

### Dead-Letter Queue (REC-13)

Outbox messages that exhaust all retries (`MaxRetries = 5`) are moved to `DeadLetterMessages` table instead of being dropped. The outbox dispatcher updates `FailedAt` / `FailureReason` and marks the message as dead-lettered. `DbSet<DeadLetterMessage> OutboxDeadLetters` is exposed on `UmsPlatformDbContext` for ops inspection.

### Outbox Pattern

All aggregate saves go through `SaveEntitiesAsync()` which:
1. Materializes domain events into `OutboxMessage` rows (same transaction via `OutboxMessageFactory`).
2. Sets `MarkChangesAsCommitted()` **after** `SaveChangesAsync()` succeeds.
3. `OutboxDispatcherBackgroundService` polls every 5 seconds (batch 50, max 5 retries) to publish via MediatR.

### Optimistic Concurrency

All SQL-backed aggregate entities carry `byte[] RowVersion { get; set; }` configured with `.IsRowVersion()`. `DbUpdateConcurrencyException` is caught in every `SaveEntitiesAsync()` and re-thrown as `ConcurrencyConflictException` → HTTP 409.

### Cross-Aggregate Transactions

Use `IUnitOfWorkScope` (registered in DI) when a handler must modify two or more repositories atomically:

```csharp
await using var tx = await _scope.BeginAsync(cancellationToken);
await _repoA.UnitOfWork.SaveEntitiesAsync(cancellationToken);
await _repoB.UnitOfWork.SaveEntitiesAsync(cancellationToken);
await tx.CommitAsync(cancellationToken);
```

### Middleware Pipeline Order (Program.cs)

```
UseCorrelationId → UseGlobalExceptionHandler → UseIdempotency → UseRateLimiter
→ UseCors → UseSecurityHeaders → UseCulture → UseDevAuth → UseHttpsRedirection
→ Routes (MapGraphQL, MapHealthChecks, MapXxxEndpoints)
```

`UseDevAuth` reads `X-User-Id` / `X-User-Name` headers (development only).

### Health Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /health/live` | Liveness — always 200 if process is alive |
| `GET /health/ready` | Readiness — SQL Server connectivity + outbox backlog |
| `GET /health` | Full report (all checks, JSON) |

### OpenTelemetry

Configured via `AddUmsObservability()`. Set `OpenTelemetry:Endpoint` in appsettings to enable OTLP export (Jaeger/Tempo/Datadog/Azure Monitor). Without an endpoint, instrumentation runs in-process but does not export.

---

## Testing

### Test Projects

- `Ums.Domain.Test` — domain aggregate unit tests (~382 tests)
- `Ums.Application.Test` — command/query handler unit tests with Moq (~331 tests)
- `Ums.Presentation.IntegrationTest` — HTTP integration tests via `WebApplicationFactory` (not in `Ums.sln`; run directly with `dotnet test Ums.Presentation.IntegrationTest`)

### Test Conventions

```csharp
// Application tests: mock repo + UoW + context
private readonly Mock<IXxxRepository> _repo = new();
private readonly Mock<IUnitOfWork>    _uow  = new();
private readonly Mock<IUserContext>   _ctx  = new();

public XxxTests()
{
    _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
    _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _ctx.Setup(u => u.UserId).Returns("user-001");
}
```

- Tests tagged `[documents gap]` in `TransactionalAtomicityTests` document KNOWN risks — do not delete them.
- `ValidationBehavior` produces errors prefixed with `"Validation.Failed:"` — assert accordingly.
- **SQL Server integration tests** (`SqlServerTenantTests`) require Docker. They skip automatically via `Assert.Skip(...)` when Docker is unavailable — never `Skip.If()` which doesn't exist in xUnit v3.
- **Moq value-tuple returns**: `.ReturnsAsync(((IReadOnlyList<T>)list, list.Count))` — explicit cast required for the tuple type to resolve.

### Testcontainers (SQL Server)

`Ums.Presentation.IntegrationTest/Infrastructure/SqlServerContainerFixture` starts a SQL Server 2022 container on demand and runs all migration scripts via `SqlServerSchemaBootstrapper.InitializeAsync`. Use `[Collection("SqlServer")]` on test classes that need a real DB.

---

## Agent Rules (from AGENTS.md — enforce always)

- **NEVER delete or bypass existing tests** to make a fix pass.
- **Domain purity**: `Ums.Domain` project must have zero NuGet references.
- **Tenancy enforcement**: Application-layer tenant filter is primary. SQL Server RLS is secondary failsafe — never replace the application layer with RLS-only logic.
- **Configuration Catalog Standard**: Any new parameter, feature flag, or policy must follow `code / value / description` model and update ORM, migrations, and docs together.
- **ADR traceability**: Before modifying core logic, verify there is an approved ADR for the area. Check `docs/architecture/adrs/`.
- **Bilingual docs**: When updating documentation, synchronize both English and Spanish versions.
- **Do NOT modify** CI/CD workflows (`.github/workflows/`) or Git hooks.

---

## Key Files to Know

| File | Purpose |
|---|---|
| `Ums.Infrastructure/DependencyInjection.cs` | All DI wiring — persistence provider switching, health checks, circuit breaker |
| `Ums.Infrastructure/Persistence/UmsPlatformDbContext.cs` | DbSets + global query filters for tenant isolation |
| `Ums.Infrastructure/Persistence/Options/PersistenceOptions.cs` | Feature flags for which stores use SQL vs InMemory |
| `Ums.Presentation/Program.cs` | App startup, middleware pipeline, endpoint registration |
| `Ums.Presentation/Extensions/DomainErrorStatusMapper.cs` | Result error → HTTP status mapping |
| `Ums.Application/Common/Behaviors/ValidationBehavior.cs` | MediatR pipeline — FluentValidation integration |
| `Ums.Infrastructure/Hosting/OutboxDispatcherBackgroundService.cs` | Domain event dispatch from outbox |
| `Ums.Infrastructure/Persistence/SqlServerSchemaBootstrapper.cs` | Ordered migration script runner — add new `.sql` files here |
| `Ums.Infrastructure/Persistence/Migrations/` | Idempotent SQL migration scripts (embedded resources, run in `ScriptOrder`) |
| `Ums.Presentation.IntegrationTest/Infrastructure/SqlServerContainerFixture.cs` | Testcontainers SQL Server fixture — starts lazily, graceful skip if Docker absent |
| `src/apps/ums.api/coverage.sh` | Coverage runner (run from `src/apps/ums.api/`) |
