# Database and Messaging Testing Strategy

## Philosophy
Testing persistence and transactionality cannot rely solely on `Microsoft.EntityFrameworkCore.InMemory` or SQLite in memory, as these do not enforce schema isolation, true transactions, row locks, or concurrency exactly like a real relational database.

## Test Types

1. **Integration Tests (Testcontainers)**
   - Use `Testcontainers.MsSql` for SQL Server integration testing.
   - (Future) Use `Testcontainers.PostgreSql` for PostgreSQL certification.
   - Run tests using the real DbContext, migrations, and bootstrapper.

2. **Repository Contract Tests**
   - Verify that repositories can Save, Retrieve, Update, and Delete.
   - Verify that `RowVersion` concurrency exceptions are thrown correctly when simulating concurrent updates.

3. **Transaction Boundary Tests**
   - Write tests that instantiate an Aggregate, mutate it, and commit.
   - Assert that Outbox messages are correctly written to the database.

4. **Service Bus Integration Tests**
   - Inject MediatR and verify that publishing an event routes it to the expected handler.
   - Assert idempotency: sending the same event twice should not corrupt data or throw unhandled exceptions.

5. **Migration Upgrade Tests**
   - Run migrations from empty to current state.
   - Assert no syntax errors or locking issues occur during parallel boots.
