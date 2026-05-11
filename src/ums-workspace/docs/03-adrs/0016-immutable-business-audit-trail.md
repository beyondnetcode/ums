# ADR 0016: Immutable Business Audit Trail and Change Tracking

## Status
Approved

## Date
2026-05-09

## Context
In regulated logistics and customs systems, absolute traceability is required by law. It is not enough to know the current state of a container's weight or seal; auditors must be able to trace exactly who changed it, when, from what IP, and what the previous values were. While initially considered at the ORM level, delegating compliance and auditing to infrastructure (database triggers or ORM subscribers) hides the business intent, loses request context (like user ID or IP), and tightly couples the architecture to the database technology.

## Decision
We will implement a **Hybrid Audit Strategy** combining row-level metadata with an Application-Level immutable ledger:

1. **Row-Level Metadata (Current State)**: Every business entity in the database will inherit from a common `BaseEntity` that strictly enforces the physical columns: `created_at`, `created_by`, `updated_at`, `updated_by`, and a `version` (timestamp/concurrency token). This allows instant, localized querying of who made the last modification and when.
2. **Application-Level Historical Ledger (Deltas)**: To prevent history loss when rows are overwritten, the application layer (Mappers/Use Cases) will explicitly dispatch an audit request (e.g., via a Domain Event using `@nestjs/event-emitter` or an `AuditService` helper) capturing `old_values` and `new_values`.
3. **Rich Context & Immutability**: The audit events will be written to a dedicated, append-only centralized Audit Table (or isolated document store) capturing the `user_id`, Use Case name, `tenant_id`, and network context. This table will use database-level triggers to block `UPDATE` and `DELETE` operations.

## Consequences
* **Pros**: Provides the "best of both worlds". Extremely fast localized queries for current state via row-metadata, combined with a 100% legally compliant, ORM-agnostic historical ledger for deep forensic analysis. 
* **Cons**: Requires strict developer discipline to ensure all domain modifications emit the corresponding audit event. Database storage will grow significantly over time due to the append-only ledger.
