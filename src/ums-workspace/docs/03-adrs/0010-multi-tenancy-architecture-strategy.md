# ADR 0010: Multi-Tenancy Architecture Strategy for SaaS Evolution

## Status
Accepted

## Date
2026-05-08

## Context
As the platform evolves, there is a strategic requirement to transition the UMS and future downstream services (TMS, WMS) into a SaaS (Software as a Service) model. The system must be capable of serving multiple independent companies (Tenants) while ensuring strict data isolation, privacy, and security during execution. 

We must decide on a multi-tenancy data partitioning strategy for our PostgreSQL databases and NestJS applications that balances infrastructure costs, maintenance complexity, and security.

There are three standard industry approaches to multi-tenancy:

1. **Database-per-Tenant (Isolated)**: Each tenant has a completely separate physical PostgreSQL database.
2. **Schema-per-Tenant (Logical)**: A single database is used, but each tenant is assigned their own PostgreSQL Schema (e.g., `tenant_a.users`, `tenant_b.users`).
3. **Shared Database, Shared Schema (Pooled)**: All tenants share the same database and tables. A `tenant_id` column acts as the discriminator for every table.

## Decision
We will adopt a **Hybrid "Pooled" Multi-Tenancy Strategy utilizing Shared Schemas with PostgreSQL Row-Level Security (RLS)** as our default model, retaining the ability to deploy Isolated Databases for VIP/Enterprise clients.

1. **Application-Level Context**: The API Gateway will identify the tenant (via subdomains `tenantA.app.com` or JWT Claims) and pass an `x-tenant-id` header to downstream services. NestJS will intercept this header and inject it into an asynchronous execution context (e.g., `AsyncLocalStorage` via NestJS Request Scoping).
2. **Database-Level Isolation (RLS)**: To prevent catastrophic data leakage (e.g., a developer forgetting to append `WHERE tenant_id = X` in a query), we will leverage **PostgreSQL Row-Level Security (RLS)**. Before executing any query, NestJS will set a local transaction variable (e.g., `SET LOCAL app.current_tenant = 'tenantA'`). The PostgreSQL engine will automatically filter all rows at the lowest physical level, making it impossible for a tenant to read another tenant's data, regardless of the ORM query structure.
3. **Hybrid Scalability**: While 90% of tenants will use the Shared Database (Pooled) to minimize infrastructure costs, our routing logic will be abstracted enough to route specific `tenant_id`s to completely separate databases if required for strict legal/compliance reasons (HIPAA, GDPR) for Enterprise clients.

## Consequences

### Positive (Pros)
* **Cost Efficiency**: Sharing a database drastically reduces cloud infrastructure and hosting costs, especially for hundreds of small-to-medium tenants.
* **Simplified Operations**: Schema migrations (e.g., adding a new column to the `Users` table) only need to be run *once* across the shared schema, rather than looping through thousands of isolated tenant databases.
* **Bulletproof Security (RLS)**: Row-Level Security guarantees data isolation at the database engine level, removing the burden of security from the application's ORM queries and eliminating human error.

### Negative (Cons)
* **Noisy Neighbor Problem**: A sudden spike in heavy queries from Tenant A could consume CPU/RAM resources, slowing down performance for Tenant B. (Mitigation: Implement strict rate-limiting per tenant).
* **Backup & Restore Complexity**: Restoring the data for just a *single* tenant is harder in a shared database, requiring custom extraction scripts rather than a simple automated database dump.
* **Implementation Complexity**: Setting up TypeORM to properly pass the tenant context to PostgreSQL RLS policies requires advanced transactional configuration and interceptors in NestJS.
