# Documentation Audit Playbook

## Purpose

Use this playbook when reviewing or updating functional requirements, functional stories, ADRs, blueprints, backlog items, or bilingual documentation.

## Mandatory Checks

1. Functional stories stay business-readable.
2. Technical detail is isolated in `Technical Requirements`.
3. English and Spanish artifacts remain synchronized.
4. Diagrams match the language of the document.
5. Stack references match the authoritative stack: `.NET 10`, `React 18`, `PostgreSQL`, `EF Core through Npgsql`, modular monolith.
6. Multi-tenancy is described with application-layer filtering as primary and PostgreSQL row-level security, schema ownership, constraints, and database policies as secondary failsafes.
7. Configuration catalogs use the mandatory `code`, `value`, `description` contract.

## Review Output

Every audit should report:

- artifact
- location
- issue type
- severity
- recommended correction

## Business-First Writing Rules

- Prefer operational language over implementation language.
- Explain what the business user, approver, sponsor, administrator, or auditor sees and decides.
- Move protocols, endpoints, tables, ORM details, and infrastructure behavior into technical sections.
