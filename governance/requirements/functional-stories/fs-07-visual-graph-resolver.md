# Functional Story 7: Diagnose Permissions via Graph Visualizer

## 1. Business Purpose

Support and security teams need to understand why a user can or cannot perform an action. UMS must provide a clear visual explanation of effective permissions, including allowed paths, denied paths, and decision reasons.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **SRE / Support Engineer** | Investigates permission issues. |
| **Security Administrator** | Reviews authorization configuration and decisions. | ## 3. Business Preconditions

- The actor has diagnostic permissions.
- The target user exists.
- The target user has at least one profile.

## 4. Main Functional Flow

1. The actor searches for the target user.
2. The actor selects the tenant, branch, and system context for diagnosis.
3. The system resolves the user's effective permissions for that context.
4. The system displays a visual graph of allowed and denied paths.
5. The actor can inspect each decision reason.
6. The actor uses the explanation to resolve support or configuration issues.

## 5. Alternative Flows and Exceptions

### A. User Has No Profiles

If the user has no active profiles, the system shows that no permissions are available by assignment.

### B. Conflicting Rules

If both allow and deny rules apply, the system explains that explicit deny takes precedence.

## 6. Business Rules

1. Diagnostics must explain the reason behind each decision.
2. Denied permissions must be visually distinguishable from allowed permissions.
3. Diagnostic access must be restricted to authorized support or security roles.
4. Diagnostics must not grant additional permissions.

## 7. Acceptance Criteria

1. Authorized users can diagnose effective permissions for a selected user and context.
2. The graph clearly shows allowed and denied paths.
3. Decision reasons are visible.
4. Users without profiles show an understandable no-permissions state.

## 8. Technical Requirements

- Resolve the diagnostic authorization graph without mutating permissions.
- Include source rules and decision reasons in the diagnostic response.
- Bypass or refresh cache when diagnostic accuracy requires current source data.
- Emit audit events for diagnostic access.

## 9. Traceability

- Entities: `PROFILE`, `PROFILE_PERMISSION`, `PERMISSION_TEMPLATE`, `ACTION`
- ADRs: ADR-0021, ADR-0039
- Technical Enabler: TE-01
