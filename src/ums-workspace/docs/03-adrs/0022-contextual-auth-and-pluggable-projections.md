# ADR 0022: Contextual Authentication and Pluggable Output Projections

## Status
Accepted

## Context
SaaS platforms require both robust external/internal identity verification and fine-grained, context-aware authorization (incorporating corporate branch contexts). Hardcoding permission models or forcing a single token output format limits integration with diverse microservices and high-fidelity frontends.

Under the **bMAD Method**, all high-concurrency systems must remain decoupled, highly extensible, and future-proof.

## Decision
We will establish a centralized UMS Core capable of decoupling identity validation from hierarchical authorization graph compilation. 

The system will:
*   **Decouple Concerns**: Swap identity verification strategies dynamically using the Strategy Pattern wrapped behind a Hexagonal Port (`IAuthenticationPort`).
*   **Support Pluggable Projections**: Project compiled authorizations into multiple formats (Hierarchical JSON, JWT compressed claims, Graph structures) using pluggable output adapters.
*   **Resolve Contextual Access**: Support hierarchical, tenant-level, and branch-level (sedes) multi-tenant authorization routing.
*   **Optimize Resolution**: Utilize a high-performance Read-Aside Redis Cache to resolve contextual permission graphs in under **5ms**.

## Consequences

### Positive
*   **Total Decoupling**: Swapping identity providers is a zero-impact configuration change on core SCM business logic.
*   **Extensible Projections**: Simultaneously supports frontend-optimized dynamic menu generation and downstream microservices lightweight JWT validation.
*   **Context-Aware**: Flawless support for branch-specific (sedes) multi-tenant authorization routing.

### Negative
*   **Cache Management Overhead**: Requires implementing proactive Redis eviction hooks when administrative permission mutations occur.
