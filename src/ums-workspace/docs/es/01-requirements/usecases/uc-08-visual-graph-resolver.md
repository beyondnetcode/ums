> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versiÛn original (InglÈs) y est· programado para traducciÛn oficial en la hoja de ruta.

# üß™ Use Case 8: Diagnose User Permissions via Visual Graph Resolver

This use case specifies the flow for SRE engineers and security administrators to diagnose and visualize the compiled authorization graph for a specific user within a target organization, branch, and system context.

---

## üèõÔ∏è 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Diagnose User Permissions via Visual Graph Resolver |
| **Primary Actor** | SRE / Support Engineer |
| **Preconditions** | Actor is authenticated with SRE or SuperAdmin role in the UMS Admin Console. Target user exists and has at least one profile assignment. |
| **Postconditions** | The compiled authorization graph is rendered visually. The actor can identify allowed paths (green), denied paths (red), and the reason for each decision. |

---

## üîÑ 2. Transaction Flow

```mermaid
sequenceDiagram
    autonumber
    actor SRE as SRE / Support Engineer
    participant Console as UMS Admin Console
    participant API as UMS NestJS API
    participant Engine as Auth Engine (PDP)
    participant Cache as Redis Cache
    participant DB as PostgreSQL

    SRE->>Console: Navigate to Graph Resolver module
    SRE->>Console: Search user by email or user_id
    Console->>API: GET /api/v1/users?email=analyst@logisticscorp.com
    API-->>Console: Return matching user record
    SRE->>Console: Select Organization, Branch, and Target System
    Console->>API: POST /v1/authorization/graph/diagnostic { user_id, tenant_id, branch_id, system_id }
    Note over API: Bypass cache for diagnostic - always recompiles from DB
    API->>Engine: Trigger fresh graph compilation
    Engine->>DB: Query all profiles and templates for user in context
    Engine->>Engine: Apply Explicit-Deny Precedence rules
    Engine-->>API: Return annotated graph with decision reasons per node
    API-->>Console: Return diagnostic payload { graph, decisions, source_rules }
    Console->>SRE: Render interactive Visual Tree
    Note over Console: Green = ALLOW, Red = DENY, Grey = Not Assigned
    SRE->>Console: Click any node to view decision rationale
    Console->>SRE: Show rule source (template_id, profile_id, effect, reason)
```

### A. Main Flow
1. SRE navigates to the **Graph Resolver** module in the Admin Console.
2. Types the user's email or `user_id` in the search input. The system returns matching user records.
3. Selects the **Organization**, **Branch**, and **System** context from cascading dropdowns.
4. Clicks **Resolve Graph**. The API calls the Authorization Engine's **diagnostic endpoint**, which **always bypasses the Redis cache** and recompiles directly from PostgreSQL to show the current ground-truth state.
5. The engine returns an annotated graph that includes, for each Menu/Submenu/Option/Action node:
    - The `effect` (`ALLOW`, `DENY`, or `NOT_ASSIGNED`)
    - The `source_rule` (template_id or profile_id that produced the effect)
    - The `reason` (e.g., `Granted by Template_SCM_Analyst_Baseline_v1`, `Blocked by Explicit DENY in profile PortSupervisor_Callao`)
6. The Console renders the tree interactively: **green nodes** for ALLOW, **red nodes** for DENY, **grey nodes** for NOT_ASSIGNED.
7. SRE can click any node to expand its decision rationale panel.

---

## üõ°Ô∏è 3. Alternative Flows & Exception Handling

### Alternative Flow A: User Has No Profile Assignments
- If the user has no active profiles for the selected context, the tree renders as completely grey with the message: *"No active profile assignments found for this user in the selected context. Assign a profile or template to grant access."*

### Alternative Flow B: No Branch Selected (Org-Wide Scope)
- If branch is left empty, the diagnostic resolves org-wide permissions only, excluding branch-scoped profile overrides.

### Alternative Flow C: Geofencing Policy Present
- If the compiled graph contains ABAC geofencing metadata on any node, the resolver displays the geofencing constraint inline (e.g., `callao_port_radius_10km`) as an informational annotation without evaluating runtime location.

