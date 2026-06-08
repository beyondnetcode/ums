# Performance Testing Strategy (High-Availability SaaS)

## 1. Justification & Scope
UMS is designed as a High-Availability (HA) enterprise SaaS platform. Given the absence of a fixed baseline SLA, we must anticipate highly complex and stressful scenarios. The performance testing scope comprehensively covers all tiers:

- **Web (Frontend):** Ensuring the SPA remains responsive, correctly handles `429 Too Many Requests` scenarios, and seamlessly manages cache validation during high concurrency.
- **API (Backend):** Stressing the .NET 10 Modular Monolith, evaluating thread pool exhaustion, GraphQL resolution under deep hierarchical graphs, and massive concurrent JWT processing.
- **DB (Database):** Evaluating PostgreSQL limits, specifically targeting row-level security overhead, lock contention, and failover resilience.

## 2. Complex Scenario Definitions
We will design tests targeting the following complex edge-cases:

- **Massive Concurrency & Hierarchical Depth:** 500,000 active sessions across 1,500 distinct tenants. Tenants will have deep organizational hierarchies (up to 10 levels) requiring complex graph resolution for permissions.
- **The "Cache Stampede" Event:** A global permissions update forces the expiration of cached ACLs (Access Control Lists) for 50,000 concurrent users. The system must prevent a thundering herd on PostgreSQL.
- **Data Leakage Stress Test:** Pushing CPU and Memory to 95% utilization while executing concurrent cross-tenant queries to guarantee that the RLS (Row-Level Security) and Application filters do not misroute or leak data under extreme thread starvation.
- **Burst Reporting Traffic:** The top 10 largest tenants simultaneously execute heavy read-model reporting queries (via GraphQL) during peak transactional hours.

## 3. Testing Phases

### Phase 1: Distributed Load Testing (K6 Cluster)
- **Objective:** Establish the baseline throughput of the .NET API under complex payload resolution.
- **Execution:** K6 running from distributed geographic nodes simulating token issuance and GraphQL hierarchy fetching.
- **Metric Target:** Ensure graceful queuing (`429 Too Many Requests`) before the database thread pool is exhausted. 

### Phase 2: RLS and Database Stress Testing
- **Objective:** Identify the breaking point of PostgreSQL row-level security policies under heavy locking.
- **Execution:** JMeter generating parallel high-volume WRITE operations (Approvals, Profile Updates) alongside massive READ operations in the same Tenant DB.
- **Metric Target:** No deadlocks and maximum 15% execution overhead from the RLS layer.

### Phase 3: Chaos Engineering & Resilience
- **Objective:** Verify High-Availability capabilities during catastrophe.
- **Execution:** Randomly terminating Redis nodes (causing Cache Stampede) and forcing PostgreSQL failover while 10,000 requests are mid-flight.
- **Metric Target:** Application must automatically recover within 30 seconds, maintaining 99.99% availability without compromising tenant data boundaries.

## 4. Pending Actions
- Provision a Staging environment mirroring Production resources (Redis Cluster, PostgreSQL HA replica).
- Script K6 load scenarios incorporating dynamic JWTs and `X-Tenant-Id` headers.
- Establish baseline monitoring dashboards in Grafana/Prometheus specifically tracking RLS overhead.
