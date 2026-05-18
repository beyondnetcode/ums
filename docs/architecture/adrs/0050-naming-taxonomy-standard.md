# ADR-0050: Naming & Taxonomy Standard — Adoption of arc32 ADR-0056

## Status

Accepted

## Date

2026-05-15

## Context

UMS is a satellite product of the arc32 Progressive Monolith Architecture Reference. The parent reference defines a binding, automated-enforcement naming standard in [ADR-0056: Enterprise Naming & Design Conventions — Multi-Language, Multi-Platform](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/reference/architecture/adrs/core/0056-enterprise-naming-design-conventions.md).

UMS is a polyglot system spanning C# / .NET 8, SQL Server 2022, REST/OpenAPI, and CloudEvents. Without a formally adopted naming policy, integration bugs, onboarding friction, and security misconfigurations (field-name leaks) are predictable outcomes.

A conformance audit conducted on 2026-05-15 identified a critical SQL naming violation in the current UMS schema design: all tables use `UPPER_SNAKE_CASE` singular identifiers (e.g., `USER_ACCOUNT`, `SYSTEM_SUITE`) which violates the `snake_case` plural noun mandate of arc32 ADR-0056 §5.5.

---

## Decision

**Formally adopt arc32 ADR-0056 as the binding naming and taxonomy standard for all UMS artifacts** — code, SQL, API, events, files, and documentation.

The full policy is defined in the parent document. This ADR records:
1. Formal adoption with UMS-specific bindings
2. Known violations at time of adoption with migration path
3. UMS-specific tooling configuration

### 1. Canonical Name Derivation (inherited from ADR-0056 §3.1)

```
Ubiquitous Language Term (English noun/verb phrase from UMS Glossary)
    │
    ├─ C# class / record     → PascalCase
    ├─ C# property           → PascalCase
    ├─ C# private field      → _camelCase
    ├─ C# constant           → PascalCase  (NOT UPPER_SNAKE — C# convention per Microsoft guidelines)
    ├─ C# async method       → VerbNounAsync()
    ├─ REST URL segment      → kebab-case plural
    ├─ JSON property         → camelCase
    ├─ SQL schema            → snake_case (e.g. ums_identity)
    ├─ SQL table             → snake_case plural (e.g. user_accounts, system_suites)
    ├─ SQL column            → snake_case
    └─ CloudEvent type       → ums.{context}.{entity}.{past-participle}
```

### 2. UMS DDD Naming — Confirmed Compliant Patterns

| Construct | Rule | UMS Example | Status |
|-----------|------|-------------|--------|
| Aggregate Root | PascalCase, no `Aggregate` suffix | `UserAccount`, `Tenant` | Compliant |
| Value Object | `sealed record`, PascalCase | `Email`, `TenantType` | Compliant |
| Domain Event | `{Aggregate}{PastParticiple}Event` | `UserRegisteredEvent`, `DocumentExpiredEvent` | Compliant |
| Command | `{ImperativeVerb}{Noun}Command` | `RegisterUserCommand`, `BlockUserCommand` | Compliant |
| Repository Port | `I{Entity}Repository` | `IUserAccountRepository`, `ITenantRepository` | Compliant |
| Query | `Get{Noun}By{Criteria}Query` | `GetUserByEmailQuery` | Compliant |
| Test class | `{Subject}Tests` | `UserAccountTests` | Compliant |
| Test method | `{Method}_When{Condition}_Should{Outcome}` | `Block_WhenUserActive_ShouldTransitionToBlocked` | Compliant |

### 3. UMS CloudEvents Type Format

All domain events published to the event bus must follow:

```
ums.{bounded-context}.{entity}.{past-participle}
```

| Domain Event Class | CloudEvent type |
|-------------------|----------------|
| `UserRegisteredEvent` | `ums.identity.user.registered` |
| `UserActivatedEvent` | `ums.identity.user.activated` |
| `UserBlockedEvent` | `ums.identity.user.blocked` |
| `DocumentExpiredEvent` | `ums.compliance.document.expired` |
| `PromotionRequestApprovedEvent` | `ums.iga.promotion-request.approved` |
| `ApprovalRequestApprovedEvent` | `ums.approvals.approval-request.approved` |
| `PermissionMutatedEvent` | `ums.authorization.permission.mutated` |
| `ProfileAssignedToUserEvent` | `ums.authorization.profile.assigned` |

### 4. SQL Naming — Migration Path (Exception EX-UMS-001)

Current violation: all tables in `database-design-er.md` and `service-entity-map.md` use `UPPER_SNAKE_CASE` singular (e.g., `USER_ACCOUNT`). Required: `snake_case` plural (e.g., `user_accounts`).

This migration is time-boxed per ADR-0056 §12 (Exception Policy):

| ID | Context | Excepted Rule | Current Name | Target Name | Sunset Date | Status |
|----|---------|--------------|--------------|-------------|-------------|--------|
| EX-UMS-001 | All schemas | SQL table `snake_case` plural | `USER_ACCOUNT` | `user_accounts` | 2026-08-01 | Active |
| EX-UMS-001 | All schemas | SQL table `snake_case` plural | `SYSTEM_SUITE` | `system_suites` | 2026-08-01 | Active |
| EX-UMS-001 | All schemas | SQL table `snake_case` plural | `PERMISSION_TEMPLATE` | `permission_templates` | 2026-08-01 | Active |
| EX-UMS-001 | All schemas | SQL table `snake_case` plural | `TENANT` | `tenants` | 2026-08-01 | Active |
| EX-UMS-001 | All schemas | SQL table `snake_case` plural | `(all UPPER tables)` | `(snake_case plural)` | 2026-08-01 | Active |

**Migration action required before 2026-08-01:**
1. Rename all tables in DDL migrations to `snake_case` plural
2. Update `database-design-er.md`, `service-entity-map.md`, and all ER diagrams
3. Update EF Core entity configurations (`ToTable("user_accounts", "ums_identity")`)
4. Run `sqlfluff lint` on all migration scripts with zero violations

### 5. UMS Tooling Configuration

**.editorconfig (C#)**
```ini
[*.cs]
dotnet_naming_rule.private_fields_should_be_camel_case.severity = error
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_underscore_prefix_style

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.camel_case_underscore_prefix_style.capitalization = camel_case
dotnet_naming_style.camel_case_underscore_prefix_style.required_prefix = _
```

**sqlfluff (.sqlfluff)**
```ini
[sqlfluff]
dialect = tsql
max_line_length = 120

[sqlfluff:rules:capitalisation.keywords]
capitalisation_policy = upper

[sqlfluff:rules:capitalisation.identifiers]
extended_capitalisation_policy = lower
```

---

## Consequences

### Positive
- Full alignment with arc32 ADR-0056: single naming standard across the org
- Eliminates integration mapping bugs between API, domain, and database layers
- Automated enforcement via Roslyn Analyzers + sqlfluff from Sprint 1
- CloudEvent type format enables reliable event routing and consumer contracts

### Negative
- SQL migration required before Sprint 3 (EX-UMS-001 sunset: 2026-08-01)
- All ER diagrams and service-entity-map must be updated as part of the migration

---

**[ADR Registry](./index.md)** | **[arc32 ADR-0056](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/reference/architecture/adrs/core/0056-enterprise-naming-design-conventions.md)**
