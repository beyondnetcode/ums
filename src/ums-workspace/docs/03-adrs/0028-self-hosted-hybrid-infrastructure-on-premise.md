# 📜 ADR-0028 — Self-Hosted, Open-Source Infrastructure for Hybrid & On-Premise Deployments

**Status:** Accepted  
**Date:** 2026-05-09  
**Deciders:** Solutions Architect, SRE/DevOps Engineer, Principal Architect  
**ADR Type:** Infrastructure & Cloud-Agnostic Sovereignty  
**Related Specs:** [`stack.md`](../../02-architecture/stack.md), [`stack-summary.md`](../../02-architecture/stack-summary.md)

---

## 📋 Context

The User Management System (UMS) must be capable of operating under a hybrid deployment model: functioning as a high-density, multi-tenant SaaS platform in public clouds (AWS, GCP) while simultaneously supporting standalone, air-gapped, on-premise installations inside secure client corporate networks.

Relying on proprietary, managed cloud infrastructure services (e.g., AWS S3 for storage, AWS SQS for event queuing, or AWS Secrets Manager for secret storage) creates severe architectural problems:
1. **Vendor Lock-in:** The domain and infrastructure code becomes coupled to cloud-specific SDKs and APIs.
2. **On-Premise Incompatibility:** Prevents the system from being deployed in localized, private, or air-gapped Kubernetes clusters where public internet or public cloud access is prohibited.
3. **Variable Operational Cost:** Managed cloud services generate unpredictable transaction-based costs that can scale exponentially.

To guarantee true cloud-agnostic sovereignty and support all on-premise clients, every infrastructure component in our technology stack must have a fully compliant, high-performance, and completely self-hostable open-source alternative.

---

## ⚖️ Decision

We will mandate and enforce the use of **100% Self-Hostable, Open-Source Infrastructure Components** across all environments:

1. **MinIO (Object Store):** Replaces proprietary cloud stores (e.g., AWS S3). MinIO is deployed inside our Kubernetes clusters, implementing the exact AWS S3 API contract.
2. **RabbitMQ (Message Broker):** Replaces proprietary cloud message buses (e.g., AWS SQS or Azure Service Bus). RabbitMQ handles asynchronous, intra-domain events via AMQP v0.9.1.
3. **HashiCorp Vault (Secrets Store):** Replaces proprietary cloud secret managers. Vault is self-hosted to provide secure secrets injection and dynamic key management.
4. **PostgreSQL & Redis (Database & Cache):** Both relational storage and caching are handled via self-hostable, open-source community editions of PostgreSQL v16 and Redis v7.2, deployed with native replication engines (Postgres streaming replication & Redis Sentinel).

---

## 📐 Architecture Constraints

- **S3 API Standard Compliance:** Any application logic interacting with MinIO must use generic S3-compliant clients, ensuring that a simple configuration change can redirect the storage target to a managed AWS S3 bucket if requested by a SaaS client, with zero code modifications.
- **AMQP Protocol Decoupling:** Event bus interactions are abstracted behind the generic `IEventBusPort` interface, preventing any domain-level dependency on RabbitMQ libraries.
- **K8s Secret Injection abstraction:** Secrets are resolved uniformly via environment variables injected at runtime by Kubernetes, completely decoupling the running container from HashiCorp Vault APIs.

---

## ✅ Consequences

### Positive
- **100% On-Premise & Air-Gap Ready:** The complete UMS platform can be deployed anywhere, from a local developer's laptop to an air-gapped corporate data center, with zero cloud dependencies.
- **Absolute Vendor Independence:** Zero risk of vendor lock-in or sudden pricing spikes from public cloud providers.
- **Consistent Local Developer Experience:** Developers run the exact same infrastructure locally via Docker Compose as is run in production on-premise Kubernetes clusters, eliminating "it works on my machine" bugs.

### Negative
- **Increased SRE Maintenance Overhead:** The SRE/DevOps team is responsible for managing, backing up, and scaling the stateful infrastructure components (PostgreSQL, Redis, RabbitMQ) instead of delegating this to a cloud provider.
- **Slightly Higher Initial Infrastructure Footprint:** Running self-hosted services inside local clusters consumes baseline CPU, RAM, and storage that would otherwise be offloaded to serverless cloud offerings.

---

## 🔗 ADR Impact Cross-References

| ADR | Impact |
| :--- | :--- |
| ADR-0010 (Multi-Tenancy)| Row-Level Security policies are implemented directly inside our self-hosted PostgreSQL v16 database engines, ensuring identical isolation enforcement across all deployment models. |
| ADR-0014 (Redis Caching)| Redis clusters are deployed natively on-premise inside the Kubernetes environment using Helm, ensuring low latency and total sovereignty. |
