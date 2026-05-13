# 🧪 Use Case 5: Register System and Define Menu Topology

This use case specifies the flow for registering a new client application (System) into the UMS and defining its navigational resource hierarchy (Menus, Submenus, Options, and Actions).

---

## 🏛️ 1. Use Case Definition

| Attribute | Specification |
| :--- | :--- |
| **Name** | Register System and Define Menu Topology |
| **Primary Actor** | Global Security Administrator (SuperAdmin) |
| **Preconditions** | Actor is authenticated as SuperAdmin in the UMS Admin Console. |
| **Postconditions** | System is registered with a secure M2M API credential. Menu topology is defined and available for template assignment. |

---

## 🔄 2. Transaction Flow

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Global Security Administrator
    participant Console as UMS Admin Console
    participant API as UMS .NET 8 API
    participant DB as PostgreSQL
    participant Audit as Audit Ledger

    Admin->>Console: Navigate to Systems > Register New System
    Admin->>Console: Fill system name, code, base URL
    Console->>API: POST /api/v1/systems { name, system_code, base_url }
    API->>DB: Insert SYSTEM record, generate api_credential_hash
    API->>Audit: Log SystemRegisteredEvent
    API-->>Console: 201 Created { systemId, apiCredential }
    Console-->>Admin: Show system credential (once only, copy prompt)
    Admin->>Console: Navigate to Systems > Topology > Add Menu
    loop Build Menu Tree
        Admin->>Console: Add Menu → Submenus → Options → Actions
        Console->>API: POST /api/v1/systems/{id}/menus (recursive batch)
        API->>DB: Insert MENU / SUBMENU / OPTION / ACTION records
    end
    API-->>Console: Topology saved confirmation
```

### A. Main Flow
1. SuperAdmin navigates to **Systems** and clicks **Register New System**.
2. Fills in system name (`SCM Route Planner`), machine code (`scm_route_planner`), and base URL.
3. The API generates a unique, hashed M2M API credential that client applications use in `Authorization: Bearer` headers when calling `POST /v1/authorization/graph`. This credential is shown **once** and must be saved.
4. Admin navigates to the **Topology Builder** for the registered system and constructs the navigation tree: `Menus → Submenus → Options → Actions`.
5. Each node specifies a label, order index, and (for Actions) an API endpoint mapping and action code (`create`, `read`, `update`, `delete`, `export`, `approve`).

---

## 🛡️ 3. Alternative Flows & Exception Handling

### Alternative Flow A: Duplicate System Code
- If `system_code` already exists, the API returns a `409 Conflict` with error code `ERR_DUPLICATE_SYSTEM_CODE`.

### Alternative Flow B: Topology Incomplete
- If an Option has no Actions defined, the topology is saved as a draft but cannot be referenced in Authorization Templates until at least one Action is linked.
