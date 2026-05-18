# Functional Story 4: Register System and Define Menu Topology

## 1. Business Purpose

UMS must let administrators register client systems and describe their navigation structure so permissions can be assigned against real business capabilities.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Global Security Administrator** | Registers client systems and defines their menu topology. |
| **Client System Owner** | Provides the system structure and access actions. | ## 3. Business Preconditions

- The administrator is authorized to register systems.
- The system owner has provided the expected modules, menus, options, and actions.

## 4. Main Functional Flow

1. The administrator starts the registration of a new client system.
2. The administrator enters the system name, business code, and routing information.
3. The system is registered and marked available for topology configuration.
4. The administrator defines modules, menus, options, and actions.
5. The system validates that the topology is complete enough to support authorization templates.
6. The topology becomes available for permission assignment and diagnostics.

## 5. Alternative Flows and Exceptions

### A. Duplicate System Code

If another system already uses the same business code, UMS prevents registration and asks the administrator to choose a unique code.

### B. Incomplete Topology

If a topology node is incomplete, UMS can save it as draft but prevents its use in authorization templates until required actions are defined.

## 6. Business Rules

1. System codes must be unique.
2. Menus and options must belong to a registered system hierarchy.
3. Actions must be explicit before they can be assigned through templates.
4. Draft topology must not grant permissions.

## 7. Acceptance Criteria

1. An administrator can register a new system with a unique code.
2. Duplicate system codes are rejected.
3. Menu topology can be built and reviewed.
4. Incomplete topology cannot be used for authorization assignment.

## 8. Technical Requirements

> [!WARNING]
> **ESTADO DE IMPLEMENTACIÓN: DIFERIDO**  
> En la fase actual, la gestión activa de la topología de recursos de sistemas (`SystemSuite`, `FunctionalModule`, etc.) está **diferida** en el dominio principal de C# y se maneja mediante referencias externas a nivel de Value Object ID (`SystemSuiteId`).

- Enforce system identifier and metadata persistence.
- Enforce uniqueness for system codes.
- Emit domain events when system metadata is registered.

## 9. Traceability

- Entities: `SystemSuite` (Deferred ID Reference), `FunctionalModule` (Deferred ID Reference)
- ADRs: ADR-0032, ADR-0034, ADR-0047
- Related Stories: FS-02, FS-07
