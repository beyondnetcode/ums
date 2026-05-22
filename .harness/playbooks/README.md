# BMAD Project Skills and Playbooks

These local playbooks convert audit findings, design standards, and implementation lessons into reusable BMAD execution skills for UMS agents.

All agents working in this repository must use the relevant playbook together with:

- `AGENTS.md`
- `.bmad-core/rules/global-rules.md`
- `.harness/rules/project-rules.yaml`

## Available Playbooks

1. `documentation-audit-playbook.md`
   - Functional story readability
   - Bilingual synchronization
   - ADR and stack coherence
   - Diagram traceability

2. `api-audit-playbook.md`
   - REST maturity
   - GraphQL query governance
   - Error mapping
   - Pagination, filtering, sorting, validation

3. `frontend-audit-playbook.md`
   - Reusable components
   - Layout and page-shell consistency
   - A11y
   - Modular-monolith frontend boundaries

4. `modular-monolith-evolution-playbook.md`
   - Bounded-context ownership
   - Extraction readiness
   - Shared libraries and shell boundaries
   - SQL Server tenancy and outbox alignment

## How To Use Them

- `PO` and `Analyst`: start with documentation and backlog playbooks.
- `Architect`: start with modular-monolith and API playbooks.
- `Developer`: use API, frontend, and modular-monolith playbooks depending on the change.
- `QA`: use documentation and API playbooks to verify compliance and generate audit reports.
