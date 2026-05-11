# ADR 0030: API Gateway Implementation: Kong Open Source vs NestJS

## Status
Proposed

## Date
2026-05-10

## Context
During architectural reviews of the UMS (User Management System) gateway design, a question was raised regarding the choice of technology for the API Gateway layer. The current architectural documentation mentions using NestJS for gateway/BFF (Backend For Frontend) responsibilities. However, there is a proposal to use an open-source version of Kong (an NGINX-based API Gateway) instead. 

We need to clarify the distinction between these two technologies, determine if they serve the exact same purpose, and establish whether adopting Kong is a sound architectural decision for our specific use case.

## Analysis

### 1. Are Kong and NestJS the "same thing" in the context of a Gateway?
**No. They serve fundamentally different architectural purposes within the gateway tier.**

*   **Kong (API Gateway):** Kong is a highly performant, infrastructure-level API Gateway built on top of NGINX (via OpenResty). It is designed to operate at the network edge. Its primary concerns are cross-cutting infrastructure policies:
    *   Reverse Proxying and Load Balancing
    *   SSL/TLS Termination
    *   Rate Limiting & Traffic Control (Spike Arrests)
    *   Authentication verification (e.g., JWT validation, OAuth2 introspection) at the edge
    *   Security (WAF, CORS, IP Allow/Deny lists)
    *   Metrics and Logging (Prometheus, Datadog integrations)
    *   Protocol translation (e.g., HTTP to gRPC)

*   **NestJS (BFF / Orchestrator):** NestJS is a Node.js application framework. When used as a "Gateway" in microservices, it typically acts as a **Backend For Frontend (BFF)** or an **API Composition / Orchestration Layer**. Its primary concerns are business-logic-adjacent:
    *   Data Aggregation (calling UMS, TMS, and WMS to build a single composite response)
    *   Data Transformation & Filtering (removing sensitive fields before sending to the client, mapping data structures)
    *   GraphQL federation or REST endpoint orchestration
    *   Client-specific logic (e.g., one NestJS BFF for Mobile, one for Web)

### 2. Is it "okay" to use Kong?
**Yes, it is highly recommended and represents an architectural maturation.** 

Using NestJS to handle pure API Gateway concerns (like high-throughput rate limiting or basic proxying) is an anti-pattern. Node.js (and by extension NestJS) runs on a single-threaded event loop. While it handles asynchronous I/O well, it cannot match the raw throughput, low latency, and memory efficiency of an NGINX-based proxy like Kong for simply moving bytes from point A to point B and applying infrastructure policies.

Conversely, using Kong to do complex data aggregation (e.g., writing custom Lua plugins to fetch a user profile from UMS, then fetch their orders from TMS, merge the JSON, and return it) is also an anti-pattern. Kong is not an orchestration engine.

## Decision
We will adopt a **Two-Tiered Gateway Pattern**, differentiating between the *Edge API Gateway* and the *Application/BFF Gateway*:

1.  **Tier 1: Edge API Gateway (Kong Open Source)**
    *   Deployed at the edge of the network (Ingress).
    *   Handles all north-south traffic entering the cluster.
    *   Responsible for strictly non-business cross-cutting concerns: Rate Limiting, SSL termination, Bot detection, JWT validation (verifying signatures before traffic hits our apps), and routing traffic to the appropriate downstream service or BFF.
2.  **Tier 2: Application/BFF Gateway (NestJS)**
    *   Deployed behind Kong within the protected network.
    *   Handles API composition, GraphQL federation, data formatting, and client-specific (Web/Mobile) orchestration.
    *   Does *not* need to worry about basic rate limiting or SSL termination, as Kong has already handled these.

### 🏛️ Updated Two-Tier Gateway Architecture Diagram

```text
+------------------+     +--------------------+
|  React Web App   |     | Mobile Client App  |
+--------+---------+     +---------+----------+
         |                         |
         | HTTP / HTTPS            | HTTP / HTTPS
         v                         v
+---------------------------------------------+
|               Tier 1: Kong OSS              |
|               (Edge API Gateway)            |
|  [Rate Limiting, JWT Validation, Routing]   |
+--------+-------------------------+----------+
         |                         |
         v                         v
+--------+---------+     +---------+----------+
|  Tier 2: NestJS  |     |  Tier 2: NestJS    |
| (Web BFF Gateway)|     | (Mobile BFF Gateway|
| [Aggregation]    |     | [Transformation]   |
+--------+---------+     +---------+----------+
         |                         |
         +-------------+-----------+
                       |
                       v
         +-------------+-------------+
         |                           |
         v                           v
+--------+--------+           +------+------+
|   UMS Service   |           | TMS Service |
+-----------------+           +-------------+
```

## Consequences

### Positive (Pros)
*   **Separation of Concerns:** Developers can focus on business logic and composition in NestJS without writing custom middleware for rate limiting or security policies.
*   **Performance:** Kong acts as a highly efficient shield, dropping bad requests (invalid JWTs, rate-limit exceeders) at the edge, freeing up Node.js event loop cycles in the NestJS tier.
*   **Scalability:** The Edge Gateway (Kong) and the Application Gateways (NestJS) can scale independently based on bottleneck type (network I/O vs CPU/Memory).

### Negative (Cons)
*   **Operational Complexity:** Introduces a new infrastructure component (Kong, and potentially its backing datastore like PostgreSQL if not using DB-less mode) that must be deployed, monitored, and maintained.
*   **Network Hop:** Adds an additional network hop (Client -> Kong -> NestJS -> Microservice) which adds marginal latency, though usually offset by Kong's performance and caching capabilities.
