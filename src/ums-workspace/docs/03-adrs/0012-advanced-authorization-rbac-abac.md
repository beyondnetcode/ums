# ADR 0012: Advanced Authorization (RBAC/ABAC) and Security Auditing Strategy

## Status
Approved

## Date
2026-05-08

## Context
While the system currently relies on JWT for authentication, enterprise SaaS applications require robust access control matrices. Users within a specific tenant must have restricted access based on their operational roles (e.g., Admin, Operator, Auditor) and the state of the attributes they are trying to modify.

## Decision
We will implement a hybrid Role-Based Access Control (RBAC) and Attribute-Based Access Control (ABAC) architecture:

1. **NestJS Guards & Decorators**: Implement custom `@Roles()` and `@Permissions()` decorators at the controller/resolver level. A global `RolesGuard` will intercept requests and validate the JWT claims against the required endpoint permissions.
2. **Tenant-Aware Scoping**: Roles are not global; they are strictly bound to a `tenant_id`. A user might be an 'Admin' in Tenant A, but only a 'Viewer' in Tenant B.
3. **Proactive Security Auditing**: Security-critical actions (e.g., elevating privileges, modifying roles) will be automatically flagged and logged into a dedicated security audit stream, preparing the ground for future AI anomaly detection models.

## Consequences
* **Pros**: Highly granular, secure, and compliant access control mechanism suitable for strict enterprise and government audits.
* **Cons**: Role and permission matrices can become complex to manage and test. Requires careful design of the JWT payload to avoid token bloat.
