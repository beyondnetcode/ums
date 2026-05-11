# ADR 0021: High-Performance Authentication and Authorization Graph Compilation

## Status
Accepted

## Context
In a federated B2B SaaS ecosystem, user login is the highest-concurrency entry point. Generating complex dynamic role-resolution trees and querying PostgreSQL relational tables on every HTTP request to build custom menus and permission structures is highly resource-intensive, resulting in high database load and poor latency profiles.

Under the **bMAD Method**, all high-concurrency gateways must be stateless, horizontally scalable, and optimized for sub-millisecond response profiles.

## Decision
We will expose a unified, stateless `/api/v1/auth/login` endpoint that abstracts internal/external identity providers (using the Strategy Pattern) and returns a pre-compiled, Redis-cached **Hierarchical Authorization Graph** mapping:
`Organization ➔ System ➔ Role ➔ Menu ➔ Submenu ➔ Option ➔ Action`

*   **Stateless Handshake**: Session validity is cryptographically verified on-the-fly using RS256-signed Access Tokens coupled with cryptographically rotated Refresh Tokens (RTR).
*   **Read-Aside Cache**: The compiled authorization graph is cached inside Redis utilizing `user_id:target_system_id:org_id` as the composite key, keeping resolution latency under **5ms**.
*   **Explicit-Deny Precedence**: The graph compilation engine enforces that any explicit `DENY` rule overrides all other inherited `ALLOW` permissions.

## Consequences

### Positive
*   **Sub-millisecond Latency**: Redis caching reduces graph resolution to <5ms (Cache Hit).
*   **Stateless Scalability**: Authentication servers can scale horizontally without session synchronization bottlenecks.
*   **Frontend-Optimized**: A single network call returns both session tokens and UI-rendering configurations.

### Negative
*   **Cache Invalidation Overhead**: Requires implementing proactive Redis eviction hooks when administrative permission mutations occur.
