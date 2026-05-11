# ADR 0014: Distributed Caching Strategy with Redis

## Status
Approved

## Date
2026-05-08

## Context
High-concurrency portals (like appointment scheduling or inventory checking) can overwhelm the primary PostgreSQL database with redundant read operations during peak traffic hours, drastically degrading response times.

## Decision
We will introduce a distributed caching layer using **Redis**:

1. **Cache Adapter (Tool Transparency)**: We will integrate the `@nestjs/cache-manager` module configured with a Redis store, but it will be strictly implemented as an Infrastructure Adapter. The Core domain will only interact with a pure `ICachePort` interface, remaining completely unaware of Redis or `@nestjs/cache-manager`.
2. **Read-Aside Caching**: High-frequency, low-mutation queries (e.g., master data catalogs, active operational statuses) will query the `ICachePort` first. If a cache miss occurs, the database is queried, and the result is stored in the cache with an appropriate TTL (Time To Live).
3. **Cache Invalidation**: Mutations to cached entities must synchronously trigger cache invalidation events to ensure data consistency.

## Consequences
* **Pros**: Drastically reduces database CPU load, lowers API latency to <150ms for cached routes, and improves overall system elasticity.
* **Cons**: Introduces a new infrastructure component (Redis) to maintain. Cache invalidation logic is notoriously prone to edge-case bugs leading to stale data.
