# ADR 0047: Hierarchical Configuration Management

## Status
Approved

## Context
A multi-tenant system with complex functional modules requires a flexible way to manage settings, feature flags, and business parameters. Hardcoding these values or storing them in disconnected tables is unscalable. We need a centralized configuration engine that supports inheritance and overrides across different organizational scopes.

## Decision
We will implement a **Hierarchical Configuration Engine** (`APP_CONFIGURATION`):

1.  **Scoped Resolution (Inheritance)**:
    *   Settings can be defined at four levels of granularity:
        *   **Global**: `TenantId`, `SuiteId`, `ModuleId` are all NULL.
        *   **Tenant**: `TenantId` is populated; others are NULL.
        *   **Suite/System**: `SuiteId` is populated.
        *   **Module**: `ModuleId` is populated.
    *   **Resolution Strategy**: "Closest Scope Wins". The system resolves parameters by looking from the most specific scope (Module) upwards to the Global level.

2.  **Inheritance Control**:
    *   `IsInheritable`: If FALSE at a higher level (e.g., Global), lower levels (e.g., Tenant) cannot override the value. This ensures compliance with platform-wide mandates.

3.  **Encrypted Values**:
    *   `IsEncrypted`: Sensitive configuration values (API keys, secrets) will be stored in an encrypted format at rest.

4.  **Feature Flag Support**:
    *   The engine will be used to manage Feature Flags (e.g., `ENABLE_ROLE_EVOLUTION`) using boolean values (1/0) or complex JSON structures.

## Consequences
*   **Positive**: Extreme flexibility for multi-tenant and multi-system customization. Centralized management of system behavior. Secure storage of sensitive parameters.
*   **Negative**: Requires a robust caching strategy (e.g., Redis) to avoid high database latency during parameter resolution for every request.
