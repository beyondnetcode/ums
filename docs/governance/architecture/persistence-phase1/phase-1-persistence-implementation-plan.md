# Phase 1 Persistence Implementation Plan

## Overview
This plan dictates the actions required to fully align the current UMS Monolith with the Phase 1 persistence and messaging requirements outlined in the governance documents.

## Step 1: Database Provider Configuration
- Ensure `Ums.Infrastructure` remains ORM agnostic at the repository interface level.
- Ensure the connection string or application settings strictly specify `SQL Server` as the `ActiveProvider`.
- (Future) Create an issue in the backlog to create the `Ums.Infrastructure.PostgreSql` project, which will contain PostgreSQL specific EF Core Migrations and the `pg_advisory_lock` distributed lock implementation.

## Step 2: Schema Enforcement
- Validate all `ToTable("TableName", "SchemaName")` definitions in EF Core configurations.
- Verify that `[identity]`, `[authorization]`, `[approvals]`, `[configuration]`, and `[audit]` are strictly adhered to.

## Step 3: Outbox and Distributed Locks
- Create an `IDistributedLockProvider` interface.
- Implement `SqlServerDistributedLockProvider` wrapping `sp_getapplock` and `sp_releaseapplock`.
- Update `SqlServerSchemaBootstrapper.cs` to use this provider instead of hardcoded strings, allowing the bootstrapper to be easily adaptable for PostgreSQL.

## Step 4: Transational Boundaries
- Confirm all Repositories defer to the `IUnitOfWork` for `.SaveChangesAsync()`.
- Confirm that Domain Events are strictly translated into Outbox Messages within the same transaction scope as the Aggregate persistence.
- Implement idempotency checking in the MediatR `INotificationHandler` logic so that duplicate dispatches of the same Outbox Message do not fail or duplicate business effects.

## Step 5: Testing
- Run existing Testcontainers E2E tests for SQL Server.
- Create unit tests for the Outbox interceptor.
- Create integration tests simulating failures before and during commit to ensure no partial data or ghost messages exist.
