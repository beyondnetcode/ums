# Enterprise-Grade Centralized IAM & UMS Core Specification

This document details the enterprise-grade functional specification, architectural interaction flow, and API contracts for the **User Management System (UMS) Centralized Authorization Engine** shared across the enterprise product suite.

---

## 1. Business Context Narrative
The modern enterprise software landscape demands a unified, highly scalable, and context-aware Identity and Access Management (IAM) posture. The **User Management System (UMS)** acts as a centralized, extensible authorization core (or "authorization kernel") shared across all enterprise platforms (ERP, CRM, HCM, etc.). Rather than siloing access control logic inside individual applications, the UMS decouples identity verification (Authentication) from granular permission resolution (Authorization). This allows the system to enforce centralized technical governance, support multi-tenant SaaS isolation, and dynamically adapt to different B2B customer identity directories.

In this reference scenario, a **Business Analyst** initiates a session via the centralized **Client Portal**. The authentication request carries not only credentials but also explicit organization (tenant) and branch/site context (e.g., *Callao Port Terminal*). By separating authentication concerns (delegated to federated Identity Providers like Zitadel, Okta, or Azure AD) from authorization compilation, the UMS can resolve and deliver a high-performance, cached **Hierarchical Authorization Graph** that shapes the analyst's entire workspace (menus, options, fleet dispatch actions) in under **5 milliseconds**, preventing vendor lock-in and guaranteeing absolute horizontal scalability.

---

## 2. Functional User Flow
*   **Step 1: Portal Ingress**: Business Analyst opens the Client Login Portal.
*   **Step 2: Context Input**: Analyst provides email, selects corporate organization (tenant), branch/site context (Callao Terminal), and preferred authentication method.
*   **Step 3: Identity Handshake**: Portal forwards details to the pluggable Authentication Gateway, which routes to either the local directory or a federated corporate IdP (SAML2/OIDC).
*   **Step 4: MFA Execution**: If enforced by tenant policy, the analyst completes Multi-Factor Authentication (WebAuthn/TOTP).
*   **Step 5: Authorization Call**: Gateway receives the confirmed identity and invokes the UMS Core (`GET /v1/authorization/graph`) passing user, tenant, branch, and target system IDs.
*   **Step 6: Graph Compilation**: UMS evaluates active profiles, resolves Explicit-Deny Precedence, caches the resulting JSON Authorization Graph in Redis, and returns the payload.
*   **Step 7: Dashboard Assembly**: Client Portal sets secure, HTTP-Only session cookies and uses the hierarchical JSON to dynamically construct and render application menus on-the-fly.

---

## 3. Enterprise Use Case Description

### Use Case: Contextual Authentication & Multi-Tenant Graph Synthesis
* **Primary Actor**: Business Analyst.
* **Target System**: Route Planner Application.
*   **Preconditions**: User is registered in UMS and holds a valid identity reference; Route Planner Application and Callao Terminal are registered in the UMS.
*   **Postconditions**: Session established with dual cryptographically rotated tokens and a contextual, branch-restricted JSON authorization graph.

### Variations and Exception Paths
*   **Variation A: Federated Identity Provider (Azure AD/Okta)**: Gateway delegates credential verification to the external corporate IdP via SAML2/OIDC. local database is bypassed for credential verification, and user is mapped using `identity_reference` claims.
*   **Variation B: Multi-Factor Authentication (MFA)**: If the tenant enforces adaptive MFA based on IP range, the analyst is prompted for WebAuthn (Passkeys) verification before the UMS authorization graph call is triggered.
*   **Exception 1: Missing Tenant Context**: If the request lacks `tenant_id` or `branch_id`, the API returns a `400 Bad Request` with error code `ERR_MISSING_CONTEXT`, aborting graph resolution.
*   **Exception 2: Authorization Graph Not Found**: If the user authenticates successfully but holds no active profile assignments for the Callao branch, the API returns a `403 Forbidden` with error code `ERR_ZERO_PERMISSIONS`.
*   **Exception 3: UMS Service Offline (Fallback)**: If the live UMS API is unreachable, the API Gateway activates a circuit breaker, falling back to a locally cached read-only permission graph or enforcing a deny-by-default posture.

---

## 4. Sequence Flow Description
The following sequence flow illustrates the end-to-end, decoupled transaction path between portal clients, federated directories, and the centralized UMS authorization core:

```mermaid
sequenceDiagram
    autonumber
    actor Analyst as Business Analyst
    participant Portal as Client Login Portal
    participant Gateway as Authentication Gateway
    participant IdP as Federated Identity Provider
    participant UMS as Centralized UMS Service
    participant Cache as Redis Cache Store

    Analyst->>Portal: Access Portal and Select Context
    Portal->>Gateway: Initiate Authentication Request
    alt Authentication Mode: External Identity Provider
        Gateway->>IdP: Delegate Credential Verification
        IdP-->>Gateway: Confirm Identity and Return Profile
    else Authentication Mode: Internal Store
        Gateway->>UMS: Validate Internal Credentials
        UMS-->>Gateway: Confirm Identity
    end
    Gateway->>UMS: Fetch Compiled Authorization Graph
    UMS->>Cache: Query Active Graph Cache
    alt Cache Hit
        Cache-->>UMS: Return Pre-Compiled JSON Graph
    else Cache Miss
        UMS-->>UMS: Query PostgreSQL and Apply Explicit Deny
        UMS->>Cache: Write Compiled JSON Graph
    end
    UMS-->>Gateway: Return Contextual Authorization Graph
    Gateway-->>Portal: Deliver Tokens and Authorization Graph
    Portal-->>Analyst: Render Dashboard and Dynamic Application Menus
```

---

## 5. Technical Architecture Explanation
The UMS is structured around standard **XACML Architectural Reference Model** patterns to ensure a clean Separation of Concerns (SoC) and high scalability:

```
               
                              Authentication Gateway                 
                           (Policy Enforcement Point - PEP)            
               
                                          
                                           Invoke Graph Resolution
               
                              UMS Authorization Core                 
                            (Policy Decision Point - PDP)              
               
                                                         
                           Read Policies                  Query Context
 
         Policy Repository                    Core Database System         
  (Policy Administration Point - PAP)      (Policy Information Point - PIP)    
 
```

1.  **Policy Enforcement Point (PEP)**: Implemented in the **Authentication Gateway / NestJS Guard**. It intercepts incoming requests, enforces adaptive security controls, and applies the returned authorization graph.
2.  **Policy Decision Point (PDP)**: The **UMS Authorization Core**. It processes authorization requests, evaluates user contexts, and compiles the dynamic hierarchical permission graph.
3.  **Policy Administration Point (PAP)**: The administrative modules where security administrators define templates, profiles, and baseline authorization rules.
4.  **Policy Information Point (PIP)**: The relational PostgreSQL database and user registries that supply the active user attributes, tenant constraints, and branch hierarchies during evaluation.

---

## 6. API Interaction Example

### Endpoint: `POST /v1/authorization/graph`

#### Request Headers
```http
POST /v1/authorization/graph HTTP/1.1
Host: iam.enterprise.com
Content-Type: application/json
X-Correlation-ID: tx_corr_987654321
Authorization: Bearer m2m_token_gateway_abc123
```

#### Request Body
```json
{
  "principal": {
    "user_id": "usr_analyst_callao_098",
    "email": "analyst@logisticscorp.com",
    "identity_reference": "EMP-PERU-998"
  },
  "context": {
    "tenant_id": "tenant_logistics_corp",
    "branch_id": "branch_callao_terminal"
  },
  "resource": {
    "system_id": "route_planner"
  },
  "projection_format": "hierarchical_json"
}
```

#### Response (Success - 200 OK)
*Returns the pre-compiled, hierarchical authorization graph optimized for frontend menu rendering and local backend guard validation.* (See Section 7 for the rich JSON payload).

#### Response (Error - 403 Forbidden)
```json
{
  "status": 403,
  "error_code": "ERR_ZERO_PERMISSIONS",
  "message": "The authenticated principal does not hold any active permission profiles for the specified branch context.",
  "correlation_id": "tx_corr_987654321",
  "timestaamp": "2026-05-08T15:46:02Z"
}
```

---

## 7. Example JSON Authorization Graph Response

```json
{
  "session": {
    "user_id": "usr_analyst_callao_098",
    "email": "analyst@logisticscorp.com",
    "organization_id": "tenant_logistics_corp",
    "branch_context_id": "branch_callao_terminal"
  },
  "authorization_graph": {
    "system_id": "route_planner",
    "active_roles": ["TransportationAnalyst"],
    "permissions": {
      "modules": [
        {
          "module_name": "Fleet Management",
          "module_code": "fleet_mgmt",
          "menus": [
            {
              "menu_name": "Fleet Dispatch Management",
              "options": [
                {
                  "id": "opt_view_routes",
                  "label": "View Active Routes",
                  "actions": ["read", "search"],
                  "scopes": {
                    "geofencing_restriction": "callao_port_radius_10km"
                  }
                },
                {
                  "id": "opt_modify_routes",
                  "label": "Dispatch Route Approval",
                  "actions": ["create", "update"],
                  "policies": {
                    "max_route_cost_authorization": 5000.00
                  }
                }
              ],
              "actions": ["view_dispatch", "assign_route"]
            }
          ],
          "actions": ["access_module"]
        }
      ]
    },
    "policy_metadata": {
      "policy_id": "pol_baseline_v1",
      "compiled_at": "2026-05-08T20:43:44Z",
      "precedence_applied": "explicit_deny_precedence",
      "cache_ttl_seconds": 3600
    }
  }
}
```

---

## 8. Extensibility Considerations
*   **Pluggable Output Formatters**: Output adapters use the **Strategy Pattern** to project compiled authorization graphs into custom targets, such as signed, context-aware JWT claims, GraphQL nodes, or minimal flat arrays optimized for legacy resource servers.
*   **Pluggable Policy Engines**: The UMS is engine-agnostic. The Policy Decision Point can interface with external engines (such as Open Policy Agent - OPA utilizing Rego policies) or custom rules engines without modifying core database models.
*   **Dynamic Resource Expansion**: Adding a new client module or action requires zero database schema evolution; resource types, action taxonomies, and hierarchical menus are registered as polymorphic dynamic data nodes.

---

## 9. Non-Functional Requirements (NFRs)

| Metric / Attribute | Specification Target | Verification Method |
| :--- | :--- | :--- |
| **Response Latency (Cache Hit)** | p95 < 5ms, p99 < 15ms | Locust load testing & Grafana dashboard metrics |
| **Response Latency (Cache Miss)**| p95 < 100ms, p99 < 150ms | Database query telemetry spans |
| **System Throughput** | >10,000 requests per second per region | Kubernetes horizontal pod autoscaling verification |
| **Availability SLA** | 99.99% operational uptime | Active-active multi-region deployment configurations |
| **Consistency SLA** | Cache eviction completed in <1000ms | Outbox event-driven cache invalidation hooks
## 10. Security Considerations
*   **Explicit Deny Precedence**: During graph compilation, the engine evaluates all applicable permissions. Any explicit `DENY` record immediately overrides all inherited `ALLOW` permissions.
*   **mTLS and Secure Transport**: All inter-service communications between the Authentication Gateway and UMS utilize mutual TLS (mTLS) with strong TLS 1.3 ciphers.
*   **Audit Trail Ledger**: Every graph compilation request and cache miss is logged to an immutable audit trail using database Outbox patterns to ensure non-repudiation.

---

## 11. ADR-Ready Architectural Decision Statement
*(Suitable for: `03-adrs/0023-centralized-ums-vs-decentralized-access.md`)*

### Architectural Decision Record: ADR-0023

*   **Status**: Accepted
*   **Deciders**: Enterprise IAM Architect, Lead Designer, Product Owner

#### Context
SaaS enterprise platforms (ERP, CRM, etc.) suffer from fragmented identity directories and siloed access control logic. Hardcoding roles and permission checks inside individual applications leads to severe security vulnerabilities, administrative overhead, and poor auditability.

#### Decision
We will establish a **Centralized User Management System (UMS) Core** to act as a shared, highly extensible "authorization kernel" across all enterprise portals. The system will decouple identity verification (delegated to external federated IdPs via the Strategy Pattern) from fine-grained authorization graph compilation. UMS will resolve context-aware branch permissions and return a cached, hierarchical JSON authorization graph in under **5ms**, utilizing pluggable output formatters to cater to frontends and microservices alike.

#### Consequences
*   **Positive**:
    *   **Absolute SoC**: Core business applications are entirely decoupled from Identity Provider schemas and SDKs.
    *   **Unified Auditing**: Complete visibility over global access control changes from a single ledger.
    *   **Sub-millisecond Resolution**: Redis read-aside caching delivers ultra-low latency.
*   **Negative**:
    *   Slightly higher initial network overhead, mitigated by Redis cache optimization and low-latency gRPC internal protocols.

---

## 12. Concise Executive Summary

> **Centralized IAM Authorization Core**: The UMS acts as the shared, extensible access governance core for all client systems. Using a **Corporate User** at the **Callao Branch (Sedes)** as the reference model, the architecture cleanly decouples identity verification (supporting pluggable internal credentials, Okta, Zitadel, and SAML) from authorization graph resolution. Upon successful login, the system utilizes the Strategy Pattern to deliver a dynamic, Redis-cached hierarchical **JSON Authorization Graph** in under **5ms**, mapping contextual modules, menus, options, and actions while enforcing strict Explicit-Deny precedence—delivering enterprise-grade performance, total vendor-neutrality, and absolute SaaS multi-tenant scalability.
