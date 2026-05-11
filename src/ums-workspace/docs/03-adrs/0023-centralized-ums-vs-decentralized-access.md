# ADR 0023: Centralized UMS Core vs Decentralized Access Control

## Status
Accepted

## Context
SaaS enterprise platforms (SCM, ERP, etc.) suffer from fragmented identity directories and siloed access control logic. Hardcoding roles and permission checks inside individual applications leads to severe security vulnerabilities, administrative overhead, and poor auditability.

Under the **bMAD Method**, all critical platform capabilities must remain highly decoupled, extensible, and future-proof.

## Decision
We will establish a **Centralized User Management System (UMS) Core** to act as a shared, highly extensible "authorization kernel" across all enterprise portals. 

The system will:
*   **Decouple Concerns**: Separate identity verification (delegated to external federated IdPs via the Strategy Pattern) from fine-grained authorization graph compilation.
*   **Support Multi-Tenant Contexts**: Resolve context-aware branch/site permissions seamlessly.
*   **Deliver Pluggable Projections**: Project compiled authorizations into multiple formats (Hierarchical JSON, JWT compressed claims, Graph structures) using the Strategy Pattern.
*   **Optimize Performance**: Use a high-performance Read-Aside Redis Cache to resolve contextual permission graphs in under **5ms**.

## Consequences

### Positive
*   **Absolute SoC**: Core business applications are entirely decoupled from Identity Provider schemas and SDKs.
*   **Unified Auditing**: Complete visibility over global access control changes from a single, immutable ledger.
*   **Sub-millisecond Resolution**: Redis read-aside caching delivers ultra-low latency.

### Negative
*   **Network Dependency**: Introduces an internal network dependency, mitigated by Redis cache optimization and low-latency internal protocols (e.g., gRPC).
