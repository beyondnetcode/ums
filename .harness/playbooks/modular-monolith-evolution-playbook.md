# Modular Monolith Evolution Playbook

## Purpose

Use this playbook when making architectural decisions that affect bounded contexts, shared libraries, persistence, integration patterns, or future extraction into microservices or micro-frontends.

## Mandatory Checks

1. Each bounded context keeps clear ownership of aggregates and rules.
2. Shared code belongs in shell or common infrastructure only when it is truly cross-context.
3. Domain code stays pure and framework-agnostic.
4. Cross-context collaboration prefers contracts, events, ACLs, and outbox over direct internal shortcuts.
5. PostgreSQL remains the authoritative persistence target.
6. Outbox and integration events are favored when preparing extraction paths.
7. Shared frontend logic is separated from domain features to support future extraction.

## Extraction Readiness Questions

- Can this module be isolated without copying hidden logic?
- Are contracts explicit enough to split later?
- Are we leaking one bounded context into another?
- Is this shared code actually cross-context, or just temporarily convenient?
