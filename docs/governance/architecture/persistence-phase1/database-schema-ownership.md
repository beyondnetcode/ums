# Database Schema Ownership

## Objective
Implement a Progressive Modular Monolith where the database is physically shared but logically partitioned by functional modules.

## Schema Strategy
The database will be separated into the following schemas corresponding to Bounded Contexts:
- `[identity]`
- `[authorization]`
- `[configuration]`
- `[approvals]`
- `[audit]`
- `[outbox]` (for messaging)

## Ownership Rules
1. **Module Ownership:** Each schema and its tables belong exclusively to one Bounded Context.
2. **No Cross-Schema Joins:** Modules cannot perform direct SQL joins or EF Core Include/Join operations against tables in another schema.
3. **Data Fetching:** If Module A needs data from Module B, it must query Module B's Application Services (Queries) via MediatR or request it via an Integration Event.
4. **Migrations:** Migrations are applied globally but structured so each schema's objects are isolated.

## Compatibility
- SQL Server supports `CREATE SCHEMA [name]`.
- PostgreSQL supports `CREATE SCHEMA "name"`.
- EF Core supports setting the default schema via `ToTable("TableName", "SchemaName")`.
