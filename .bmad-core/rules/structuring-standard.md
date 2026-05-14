# R-13: Enterprise Structuring Standard

* **Scope:** `po`, `architect`, `dev`, `qa`, `analyst`, `sm`
* **Trigger Condition:** Repository initialization, new module creation, documentation generation, or structural refactoring.
* **Severity:** Critical
* **Instruction:** All directory creation, module scaffolding, documentation placement, and file naming MUST conform to this standard. Any deviation requires an explicit ADR.

---

## 1. Foundation Principles

| Principle | Description |
|-----------|-------------|
| **Convention over Configuration** | Directory layout and naming follow predictable patterns. No project-specific overrides without ADR. |
| **Separation of Concerns** | Each directory has exactly one purpose. Cross-cutting concerns live at the lowestá shared parent. |
| **Bounded Context Isolation** | Domain boundaries are physical directory boundaries. No-context file sharing without explicit interfaces. |
| **Docs-as-Code** | Documentation follows code lifecycle: versioned, reviewed, linted, and co-located with its domain. Must maintain enterprise visual standard (no emojis/icons). |
| **Progressive Disclosure** | Top-level directories aggregate; lower levels reveal detail. MASTER_INDEX.md provides the entry point. |
| **Bilingual Mirror** | All user-facing documentation is maintained in `en/` and `es/` with identical directory trees.
## 2. Repository Tree (Canonical)

```
<repo-root>/
|
|-- .bmad/                          # [CONFIG] BMAD project rules & schemas
|
|-- rules/
|
| |-- project-rules.yaml
|
| +-- project-rules.json
| +-- schemas/
| +-- project-rules.schema.json
|
|-- bmad-core/                      # [CORE] BMAD framework (immutable, do not edit)
|
|-- rules/
|
| |-- global-rules.md
|
| +-- structuring-standard.md     # THIS FILE
| +-- scripts/
| +-- cleanup_markdown_encoding.py
|
|-- docs/                           # [DOCS] Enterprise documentation (bilingual)
|
|-- en/
|
| |-- index.md                    # Entry point for English docs
|
| |-- 00-product/
|
| |-- 01-requirements/
|
| |-- 02-architecture/
|
| |-- 03-adrs/
|
| |-- 04-artifacts/
|
| +-- 05-roadmap/
| +-- es/                             # Spanish mirror (identical tree)
|
|-- index.md
|
|-- 00-product/
| +-- ...
|
|-- src/                            # [SOURCE] All runnable code
| +-- <workspace>/
|
|-- apps/                       # Deployable applications
|
| |-- <app-name>/             # One directory per app
|
| |
|-- src/
|
| |
|-- tests/
|
| | +-- <app-config-files>
|
| +-- ...
|
|-- libs/                       # Shared libraries
|
| |-- <lib-name>/
|
| |
|-- src/
|
| | +-- tests/
|
| +-- ...
|
|-- tools/                      # Build scripts, codegen, migration scripts
|
|-- ops/                        # Infrastructure as Code
|
|-- poc/                        # Proofs of concept (one subdir per POC)
| +-- docs/                       # Workspace-specific dev documentation
|
|-- dev-guide/
|
|-- runbooks/
|
|-- templates/
| +-- decisions/              # Workspace-level decisions (if not in ADRs)
|
|-- AGENTS.md                       # [ROOT] AI agent instructions
|-- README.md                       # [ROOT] Project overview (English)
|-- README.es.md                    # [ROOT] Project overview (Spanish)
|-- MASTER_INDEX.md                 # [ROOT] Navigation hub (English)
|-- MASTER_INDEX.es.md              # [ROOT] Navigation hub (Spanish)
|-- CONTRIBUTING.md                 # [ROOT] Contribution guidelines
|-- GOVERNANCE.md                   # [ROOT] Decision-making, review, and ownership
|-- SUPPORT.md                      # [ROOT] Support channels and SLAs
|-- SECURITY.md                     # [ROOT] Vulnerability reporting and policies
|-- LICENSE
+-- context7.json
```

### 2.1 Directory Purpose Taxonomy

| Path Pattern | Content Type | Modifiable by Agent? |
|---|---|---|
| `.bmad/` | Project-level BMAD rules | Yes (via ADR) |
| `bmad-core/` | BMAD framework (template) | No — immutable |
| `docs/en/`, `docs/es/` | Enterprise documentation | Yes |
| `src/<workspace>/apps/` | Runnable applications | Yes |
| `src/<workspace>/libs/` | Shared libraries | Yes |
| `src/<workspace>/tools/` | Scripts, code generators | Yes |
| `src/<workspace>/ops/` | IaC, Docker, Kubernetes | Yes |
| `src/<workspace>/poc/` | Research/spikes | Yes (delete when done) |
| `src/<workspace>/docs/` | Developer documentation | Yes
## 3. Naming Conventions

### 3.1 Directory Naming

| Scope | Convention | Example |
|---|---|---|
| Apps | `kebab-case` | `api-dotnet`, `web`, `mobile-bff` |
| Libraries | `kebab-case` | `shared-ui`, `domain-types`, `api-client` |
| Bounded contexts (code) | `PascalCase.DotNotation` | `Identity.Domain`, `Authorization.Application` |
| Phases in `docs/` | `NN-<kebab-case>` | `00-product`, `01-requirements` |
| ADRs in `docs/` | `NNNN-<kebab-case>` | `0001-monorepo-orchestration-nx` |
| Tools | `kebab-case` | `codegen`, `db-migrate`, `seed-data` |
| Ops | `kebab-case` | `grafana`, `otel`, `kong` |
| POCs | `kebab-case` | `kong-gateway`, `graphql-bench` | ### 3.2 File Naming

| Category | Convention | Example |
|---|---|---|
| C# classes | `PascalCase.cs` | `Organization.cs`, `Result.cs` |
| C# interfaces | `I` + `PascalCase.cs` | `IAuthenticationPort.cs` |
| TypeScript/React components | `PascalCase.tsx` | `UserProfile.tsx`, `PermissionGrid.tsx` |
| TypeScript utilities | `camelCase.ts` | `formatDate.ts`, `parseToken.ts` |
| TypeScript types/interfaces | `PascalCase.ts` | `UserTypes.ts`, `ApiResponse.ts` |
| Config files | `kebab-case.ext` | `vite.config.ts`, `tailwind.config.js` |
| Documentation | `kebab-case.md` | `business-context.md`, `architecture-spec.md` |
| ADR files | `NNNN-kebab-case.md` | `0041-bounded-context-split.md` |
| Functional stories | `fs-NN-kebab-case.md` | `fs-01-user-authentication.md` |
| Technical enablers | `te-NN-kebab-case.md` | `te-01-build-authorization-graph.md` |
| CI/CD workflows | `kebab-case.yml` | `ci.yml`, `deploy-production.yml` |
| Dockerfiles | `Dockerfile` or `Dockerfile.<role>` | `Dockerfile`, `Dockerfile.dev` |
| Root docs | `KEBAB_CASE.ext` | `README.md`, `CONTRIBUTING.md`, `GOVERNANCE.md` | ### 3.3 Backend (.NET) Project Naming

```
{Product}.{BoundedContext}.{Layer}
      ^          ^            ^
      |
| +-- Domain | Application | Infrastructure | Presentation
      | +-- Identity | Authorization | Configuration | Audit
      +-- Ums
```

Examples:
- `Ums.Identity.Domain`
- `Ums.Identity.Application`
- `Ums.Authorization.Domain`
- `Ums.Configuration.Infrastructure`
- `Ums.Audit.Infrastructure`

### 3.4 Frontend Directory Naming (within `apps/web/src/`)

```
src/
|-- app/            # App root: providers, layouts, routing
|-- pages/          # Route-level page components
|-- features/       # Feature modules (one per bounded context / capability)
|
|-- auth/
|
|-- authorization/
|
|-- organizations/
| +-- audit/
|-- shared/         # Reusable UI, hooks, utils, types
|
|-- ui/
|
|-- hooks/
|
|-- utils/
| +-- types/
+-- lib/            # External integrations, API clients, third-party wrappers
```

---

## 4. Domain / Bounded Context Separation

### 4.1 Code-Level Isolation

Each bounded context gets its own project/library:

```
src/ums-workspace/
  apps/api-dotnet/
    src/
      Ums.Identity.Domain/           # Pure POCOs: User, Organization, Branch
      Ums.Identity.Application/      # Use cases: RegisterOrg, AuthenticateUser
      Ums.Identity.Infrastructure/   # IdP adapters, Identity persistence
      Ums.Authorization.Domain/      # Pure POCOs: Profile, Permission, Template
      Ums.Authorization.Application/ # Use cases: AssignProfile, ResolveGraph
      Ums.Authorization.Infrastructure/ # Policy engine, AuthZ persistence
      Ums.Configuration.Domain/      # Pure POCOs: SystemConfig, FeatureFlag
      Ums.Configuration.Infrastructure/ # Config providers
      Ums.Audit.Domain/              # Pure POCOs: AuditRecord, AccessLog
      Ums.Audit.Infrastructure/      # Audit persistence, event bus
      Ums.Presentation/              # API layer (controllers, middleware, Program.cs)
    tests/
      Ums.Identity.Domain.Tests/
      Ums.Identity.Application.Tests/
      Ums.Authorization.Domain.Tests/
      Ums.Authorization.Application.Tests/
      Ums.Presentation.Tests/
```

### 4.2 Inter-BC Communication Rules

| Direction | Mechanism | Allowed? |
|---|---|---|
| Domain → Domain | Direct reference | NO |
| Domain → Application | Direct reference | NO |
| Application → Domain | Direct reference | YES |
| Application → Application | Interface/Port only | YES (via DI) |
| Infrastructure → Domain | Direct reference | YES |
| Infrastructure → Application | Interface/Port only | YES |
| Presentation → Application | Direct reference | YES |
| Presentation → Domain | Direct reference | NO (must go through Application) | ### 4.3 Library (libs/) Categorization

```
libs/
|-- shared-ui/              # React components, design system
|-- shared-utils/           # Pure functions, validators, formatters
|-- domain-types/           # TypeScript types mirroring C# DTOs
|-- api-client/             # Axios/HTTP client wrappers
|-- test-utils/             # Testá helpers, mocks, fixtures
+-- config/                 # Runtime config readers, feature flags
```

**Rule:** A library in `libs/` must NOT depend on any `apps/` module. Libraries may depend only on other `libs/` or external packages.

---

## 5. Documentation Architecture

### 5.1 Documentation Tree

```
docs/
|-- en/
|
|-- index.md                         # Phase overview, reading guide
|
|-- 00-product/                      # Product vision, stakeholders, scope
|
| |-- product-vision.md
|
| |-- business-context.md
|
| |-- objectives.md
|
| |-- scope.md
|
| +-- stakeholders.md
|
|-- 01-requirements/                 # Domain analysis, stories, glossary
|
| |-- glossary.md
|
| |-- conceptual-data-model.md
|
| |-- permission-matrix-example.md
|
| +-- functional-stories/
|
| |-- index.md
|
| |-- fs-01-user-authentication.md
|
| +-- ...
|
|-- 02-architecture/                 # C4, DDD maps, tech stack, NFRs
|
| |-- architecture-spec.md
|
| |-- bounded-context-map.md
|
| |-- stack.md
|
| +-- technical-enablers/
|
| |-- index.md
|
| |-- te-01-build-authorization-graph.md
|
| +-- ...
|
|-- 03-adrs/                         # ADRs (flat directory, no sub-phases)
|
| |-- index.md                     # ADR log with status table
|
| |-- 0001-monorepo-orchestration-nx.md
|
| +-- ...
|
|-- 04-artifacts/                    # Maturity models, audits, specs
|
| |-- engineering-standards.md
|
| |-- architecture-maturity-model.md
|
| +-- ...
| +-- 05-roadmap/                      # Release plans, versioning
|
|-- versioning-and-audit-strategy.md
| +-- ...
+-- es/                                  # Spanish mirror (identical tree)
```

### 5.2 Document Types

| Type | Location | Prefix | Status Tracking |
|---|---|---|---|
| Product/Requirements | `docs/*/00-product/`, `docs/*/01-requirements/` | `fs-NN-` | Phase-based |
| Architecture Specs | `docs/*/02-architecture/` | `te-NN-` | Phase-based |
| ADRs | `docs/*/03-adrs/` | `NNNN-` | Index with status table |
| Artifacts | `docs/*/04-artifacts/` | (none) | Versioned |
| Roadmap | `docs/*/05-roadmap/` | (none) | Versioned | ### 5.3 ADR Format

Every ADR MUST follow this template:

```markdown
# ADR NNNN: Title

* **Status:** [Proposed | Accepted | Deprecated | Superseded]
* **Date:** YYYY-MM-DD
* **Deciders:** [list of decision-makers]
* **Corporate Source:** [link if applicable]

## Context
[Problem statement and background]

## Decision
[What was decided]

## Consequences
[Pros, cons, trade-offs]

## Compliance
[How to verify this decision is followed]
```

---

## 6. Modularity & Reuse Rules

### 6.1 Dependency Inversion

All cross-boundary communication MUST use interfaces/ports. Concrete implementations must be injectable. The `Ums.{BoundedContext}.Domain` project MUST have zero external NuGet references.

### 6.2 Library Promotion Lifecycle

```
Utility identified as reusable
  → Placed in nearestá consumer's scope (local)
  → Second consumer appears
  → Promoted to src/<workspace>/libs/<lib-name>/
  → Third consumer from different app/BC
  → Promoted to dedicated package/ NuGet feed
```

### 6.3 POC Lifecycle

```
New POC → src/<workspace>/poc/<poc-name>/
   Accepted → migrate code to apps/ or libs/, delete poc/
   Rejected → delete poc/
```

POCs must NOT be referenced by any production code. POCs must NOT be deployed.

---

## 7. Navigation & Index Strategy

### 7.1 Index File Requirements

| Location | Required File | Content |
|---|---|---|
| `<repo-root>/` | `MASTER_INDEX.md` | Cross-reference of all major directories, docs phases, apps |
| `docs/en/`, `docs/es/` | `index.md` | Phase overview with links to each subdirectory |
| `docs/*/NN-<phase>/` | `index.md` | List of documents in phase with descriptions |
| `docs/*/03-adrs/` | `index.md` | ADR log with status table (number, title, status, date) |
| `docs/*/NN-<phase>/<subcategory>/` | `index.md` | List of items (e.g., functional stories index) |
| `src/<workspace>/` | `README.md` | Workspace overview, build commands, architecture diagram |
| `src/<workspace>/apps/<app>/` | `README.md` | App-specific description, run instructions |
| `src/<workspace>/libs/<lib>/` | `README.md` | Library API, usage examples, dependency diagram | ### 7.2 README Content Standard

Every README MUST contain:
1. Title and one-paragraph description
2. Language switcher (en/es) for bilingual repos
3. Quick-start / build commands
4. Links to related documentation
5. Ownership / maintenance contact

### 7.3 Cross-Referencing Rules

- All links MUST be relative (never absolute `/`) to support fork/mirror portability
- Links to `docs/` from code MUST go through `docs/en/` or `docs/es/`
- ADRs MUST be referenced by number: `[ADR-0041](docs/en/03-adrs/0041-bounded-context-split.md)`
- Functional stories MUST be referenced by ID: `FS-01`

---

## 8. Document Governance

### 8.1 Document Lifecycle

```
Draft → Review → Approved → Published → Reviewed (quarterly) → Archived or Superseded
```

| State | Visibility | Immutable? |
|---|---|---|
| Draft | Branch only | No |
| Review | PR with reviewers | No |
| Approved | `main` branch | Yes (changes require new PR) |
| Published | `main` branch + indexed | Yes |
| Archived | `docs/archive/` | Yes | ### 8.2 Ownership Matrix

| Document Type | Owner | Review Cadence |
|---|---|---|
| Product docs (Phase 00-01) | Product Manager | Quarterly |
| Architecture docs (Phase 02) | Architect | Quarterly |
| ADRs (Phase 03) | Architect + Tech Leads | Per decision |
| Artifacts (Phase 04) | Engineering | Per release |
| Roadmap (Phase 05) | Product Manager | Per release |
| README files | Tech Leads | Per release |
| AGENTS.md | Platform/DevEx | Per tooling change | ### 8.3 Deprecation

To deprecate a document:
1. Mark the old document header with `* **Status:** Deprecated`
2. Create a new document or update the relevant replacement
3. Add a redirect note in the old document: `* **Superseded by:** [link to new document]`
4. Update all cross-references in MASTER_INDEX.md and affected indexes

---

## 9. Versioning & Evolution

### 9.1 ADR Versioning

ADRs are immutable once approved. To revise a decision:
1. Create a new ADR that supersedes the old one
2. The new ADR header MUST include: `* **Supersedes:** ADR-NNNN`
3. The old ADR gets: `* **Status:** Superseded by ADR-NNNN`

### 9.2 Documentation Versioning

The `docs/` tree follows the BMAD-METHOD phase model (00-05). Phase directories are NOT versioned individually. Instead:
- Git tags mark release points
- Release notes reference document state at tag
- ADR index includes a "Last Reviewed" column

### 9.3 Changelog

`CHANGELOG.md` lives at `<workspace>/` and follows [Keep a Changelog](https://keepachangelog.com/) format with [SemVer](https://semver.org/).

---

## 10. Templates & Examples

### 10.1 Template Directory

All templates live in `src/<workspace>/docs/templates/`:

```
templates/
|-- adr-template.md                  # ADR markdown template
|-- functional-story-template.md     # FS template
|-- technical-enabler-template.md    # TE template
|-- decision-log-template.md         # Lightweight decision record
|-- README-template.md               # Module README template
+-- api-controller-template.cs       # .NET controller scaffold
```

### 10.2 Template Requirements

Every template MUST:
- Include clear `{{PLACEHOLDER}}` sections
- Have a comment header explaining purpose
- Include a `# Usage` section with example
- Be registered in the workspace `docs/templates/index.md`

---

## 11. GitHub Enterprise Bestá Practices

### 11.1 Repository Configuration

| Setting | Standard |
|---|---|
| Default branch | `main` |
| Branch protection | Require PR, require status checks, require up-to-date |
| PR merge strategy | Squash merge (linear history) |
| Issue templates | Bug report, Feature requestá, Technical spike |
| PR templates | Summary, Changes, Testing, ADR references |
| CODEOWNERS | Per directory ownership (.github/CODEOWNERS) |
| Labels | `area/*`, `type/*`, `priority/*`, `status/*` taxonomy | ### 11.2 Branch Naming

| Pattern | Example |
|---|---|
| `feat/<issue>-<kebab-desc>` | `feat/42-add-oidc-provider` |
| `fix/<issue>-<kebab-desc>` | `fix/87-fix-tenant-resolution` |
| `docs/<kebab-desc>` | `docs/add-adr-0041` |
| `refactor/<kebab-desc>` | `refactor/extract-identity-context` |
| `chore/<kebab-desc>` | `chore/upgrade-dotnet-8` | ### 11.3 Commit Message Format

```
<type>(<scope>): <imperative description>

[optional body with ADR references]

[optional footer with breaking/issue references]
```

Types enforced by commitlint: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`, `build`, `perf`, `style`, `revert`.

### 11.4 CI/CD Requirements

- Every PR MUST pass lint, type-check, and test gates before merge
- ADR compliance check: PR description MUST reference affected ADRs
- Documentation diff check: changes to code MUST include doc updates or explicitly exempt (e.g., `docs: exempt`)
- SonarJS quality gate must pass

---

## 12. Anti-Patterns

### 12.1 Directory Structure Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|---|---|---|
| `src/` with no sub-workspace | Monolith trap, no isolation | Use `src/<workspace>/apps/`, `src/<workspace>/libs/` |
| `docs/` with no phase separation | Information overload, no progressive disclosure | Use `docs/en/NN-<phase>/` structure |
| Nested deeper than 5 levels | Discovery becomes impossible | Flatten at 5. Create a new top-level dir. |
| Mixed `docs/en/` and `docs/es/` out of sync | Bilingual integrity broken | Enforce R-01: both mirrors updated atomically | ### 12.2 Naming Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|---|---|---|
| `my-service`, `MyService`, `my_service` in same tree | Inconsistent, confuses search | Pick one convention per category (Sec 3) |
| ADR `final-version-v2.md` | Unversioned, no traceability | Use `0042-title.md` with supersedes chain |
| `utils/`, `helpers/`, `misc/` | Dumping ground, no semantic meaning | Name by domain: `string-utils`, `date-utils` |
| `index.ts` with 500+ re-exports | Barrel file explosion | Group by feature, one barrel per module |
| `README.md` with no language switcher | Bilingual requirement violated | Add `[EN](README.md) | [ES](README.es.md)` | ### 12.3 Governance Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|---|---|---|
| Orphaned docs (no owner) | No accountability, stale information | Every doc has owner in header or CODEOWNERS |
| ADRs that are never reviewed | Decisions without validation | ADR review step in PR template |
| Docs in personal branches for months | Knowledge silo, no collaboration | Docs are code: PR, review, merge, iterate |
| No index file in directory | Hidden content, poor DX | Every directory gets an index/README | ### 12.4 Modularity Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|---|---|---|
| Domain project references EF Core | Couples domain to infrastructure | Domain is pure POCOs, zero dependencies |
| Shared kernel becomes "everything" | Ball of mud, no bounded context integrity | Narrow shared kernel, document in context map |
| Circular dependencies between libs | Build breaks, runtime errors | Enforce with eslint-plugin-boundaries or MSBuild rules |
| POCs merged into main | Dead code, no cleanup commitment | POCs on branches or dedicated poc/ dir with expiry
## 13. Migration Strategy

### 13.1 Current → Target State Mapping

| Current Location | Target Location | Migration Action |
|---|---|---|
| `src/ums-workspace/apps/api-dotnet/Ums.Domain/` | `src/ums-workspace/apps/api-dotnet/src/Ums.Identity.Domain/` | Restructure into bounded contexts |
| `src/ums-workspace/apps/api-dotnet/Ums.Application/` | `src/ums-workspace/apps/api-dotnet/src/Ums.Identity.Application/` | Restructure into bounded contexts |
| `src/ums-workspace/apps/api-dotnet/Ums.Infrastructure/` | `src/ums-workspace/apps/api-dotnet/src/Ums.Identity.Infrastructure/` | Restructure into bounded contexts |
| `src/ums-workspace/libs/` (empty) | `src/ums-workspace/libs/<lib-name>/` | Populate on first reuse opportunity |
| `src/ums-workspace/apps/web/src/` (flat) | `src/ums-workspace/apps/web/src/{app,pages,features,shared}/` | Structural refactor |
| No `CONTRIBUTING.md` | `<repo-root>/CONTRIBUTING.md` | Create from template |
| No `GOVERNANCE.md` | `<repo-root>/GOVERNANCE.md` | Create from template |
| `.bmad-core/` inside workspace | Merge into root `.bmad/` or `bmad-core/` | Consolidate BMAD config |
| `docs/rules-summary.md` | `docs/en/04-artifacts/rules-summary.md` | Move under phase structure | ### 13.2 Migration Phases

```
Phase 1 — Foundation (Week 1)
   Create CONTRIBUTING.md, GOVERNANCE.md, SUPPORT.md, SECURITY.md
   Consolidate .bmad-core/ into .bmad/ configuration
   Create templates/ directory with ADR, FS, TE templates
   Add CODEOWNERS file

Phase 2 — Source Restáructuring (Week 2-3)
   Restructure .NET solution into bounded context projects
   Create src/ layout for frontend (app/, pages/, features/, shared/)
   Add tests/ directories matching src/ structure
   Create initial libs/ with one shared library to validate pattern

Phase 3 — Documentation Alignment (Week 3-4)
   Move docs/rules-summary.md to docs/en/04-artifacts/
   Add index.md to every docs/ subdirectory
   Update MASTER_INDEX.md with new structure
   Audit all cross-references for correctness

Phase 4 — Automation (Week 4)
   Add directory structure validation to CI pipeline
   Add stale-doc detection workflow
   Create PR template with structural compliance checklist
   Add lint rule for naming conventions where tooling supports it
```

### 13.3 Migration Rules

1. **One PR per change.** Do not mix structural migration with feature work.
2. **All existing tests must pass** before and after migration. No test deletion or bypass.
3. **Preserve git history.** Use `git mv` for file moves, never copy-delete.
4. **Update all cross-references** in the same PR as the file move.
5. **Bilingual sync is mandatory.** Both `en/` and `es/` must be updated atomically (R-01).
6. **ADR for significant structural changes.** Any change that alters the canonical tree (Section 2) requires an ADR or ADR amendment.

---

## 14. Compliance Verification

### 14.1 Automated Checks

- **Directory lint:** CI pipeline validates canonical tree structure on every PR
- **Naming lint:** Shell script or custom ESLint rule checks naming conventions
- **ADR index check:** Verifies every ADR file is listed in `docs/*/03-adrs/index.md`
- **No orphaned docs:** Every `.md` file must be referenced in at least one index or README
- **Bilingual sync check:** Every file in `docs/en/` must have a corresponding file in `docs/es/` (R-01)

### 14.2 Manual Review Checklist

For every PR:
- [ ] Does the PR follow the branch naming convention?
- [ ] Are new files in the correct location per the canonical tree?
- [ ] Do file and directory names follow the naming conventions (Section 3)?
- [ ] Is every new directory accompanied by an index/README?
- [ ] Are cross-references relative and correct?
- [ ] Is bilingual sync maintained?
- [ ] Are ADR references included for structural decisions?

---

*This standard is R-13 of the BMAD-METHOD global rules and MUST be read in conjunction with R-01 through R-12. Conflicts with earlier rules are resolved in favor of this document for structural decisions.*
