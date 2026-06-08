# ADR Template

Use this template for UMS-specific architecture decisions. Keep the English and Spanish versions synchronized in the same change.

## Header

```markdown
# ADR-NNNN: Decision Title

**Status:** Proposed | Accepted | Deprecated | Superseded
**Date:** YYYY-MM-DD
**Decision Owner:** Architecture
**Related:** ADR links, functional stories, technical enablers
```

## Context

Describe the business and technical forces that make the decision necessary. Include constraints, affected bounded contexts, tenancy impact, security impact, and documentation impact.

## Decision

State the decision directly. Prefer one clear paragraph followed by a small table when the decision changes stack, runtime, persistence, contracts, or ownership.

## Consequences

Document positive outcomes, trade-offs, risks, and rejected alternatives.

## Compliance

List the checks required to prove the decision is implemented: code, tests, documentation, diagrams, migrations, seed data, and runtime configuration.

## Follow-Up

List required work that remains outside the ADR itself.
