# Contributing to UMS

**User Management System (UMS)** - Enterprise Monorepo for authorization and identity management.

Thank you for contributing to UMS. This guide covers the complete standards and workflows expected for all contributors, including internal teams, external providers, and AI coding agents.

---

## Table of Contents

1. [Git Flow and Branch Strategy](#1-git-flow-and-branch-strategy)
2. [Pull Request Process](#2-pull-request-process)
3. [Code Standards](#3-code-standards)
4. [Testing Requirements](#4-testing-requirements)
5. [Documentation Standards](#5-documentation-standards)
6. [Security Requirements](#6-security-requirements)
7. [Architecture Compliance](#7-architecture-compliance)
8. [How to Update ADRs](#8-how-to-update-adrs)
9. [How to Update Documentation Version Logs](#9-how-to-update-documentation-version-logs)
10. [Release Process](#10-release-process)
11. [How to Handle Hotfixes](#11-how-to-handle-hotfixes)
12. [Reporting Issues](#12-reporting-issues)
13. [Evolith Alignment](#13-evolith-alignment)
14. [Definition of Done](#14-definition-of-done)
15. [Commit Message Guidelines](#15-commit-message-guidelines)
16. [PR Checklist](#16-pr-checklist)

---

## 1. Git Flow and Branch Strategy

UMS follows a structured Git Flow model with the following branches:

```
main (protected)
  └── develop (protected)
        ├── feature/*           (e.g., feature/identity-user-consent)
        ├── feature/docs-*      (e.g., feature/docs-api-contracts)
        ├── bugfix/*            (e.g., bugfix/tenant-isolation)
        ├── docs/*              (e.g., docs/update-adrs)
        └── release/vX.Y.Z      (e.g., release/v1.2.0)
  └── hotfix/*                  (e.g., hotfix/security-patch)
```

### Branch Naming Conventions

| Branch Type | Pattern | Example |
|-------------|---------|---------|
| Feature | `feature/<context>-<description>` | `feature/identity-user-consent` |
| Documentation | `docs/<area>-<description>` | `docs/update-adrs-0070` |
| Bugfix | `bugfix/<context>-<issue>` | `bugfix/tenant-isolation-query` |
| Release | `release/vX.Y.Z` | `release/v1.2.0` |
| Hotfix | `hotfix/<description>` | `hotfix/security-csp-fix` |

### Branch Protection Rules

| Branch | Protection Level |
|--------|-----------------|
| `main` | Required: PR + 1 approval + all checks passing |
| `develop` | Required: PR + 1 approval + all checks passing |
| `release/*` | Required: All release checks passing |
| `hotfix/*` | Required: Hotfix validation + expedited review |

---

## 2. Pull Request Process

### PR Requirements

**All PRs must:**
1. Target `develop` for features, bugfixes, and documentation
2. Target `main` for releases and hotfixes
3. Include a meaningful description with context
4. Reference linked issues
5. Pass all required status checks
6. Receive minimum 1 approval (2 for security-sensitive changes)

### PR Workflow

```bash
# 1. Create feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/my-feature

# 2. Make changes and commit
git add .
git commit -m "feat: add new feature"

# 3. Push and create PR
git push origin feature/my-feature
# Create PR via GitHub UI

# 4. Address reviews
# Make fixes, amend commits if needed
git push origin feature/my-feature --force-with-lease

# 5. Merge after approval
# Squash and merge via GitHub UI
```

### Required Status Checks

All PRs must have these checks passing before merge:

```
✅ build          - Build succeeds
✅ test           - All tests passing
✅ lint           - Zero lint errors
✅ security       - Security scan clean
✅ docs-quality   - Documentation validation (for doc changes)
```

---

## 3. Code Standards

### TypeScript (Frontend)

- **Strict mode enabled**: `noUnusedLocals`, `noUnusedParameters`, `noFallthroughCasesInSwitch`
- **No `any`**: Use proper types or `unknown` with narrowing
- **Explicit return types** for public APIs
- **No non-null assertions** (`!`) — use optional chaining or guards
- **No `console.log`** in production code — use logger.ts

### React (Frontend)

- **Functional components only**: No class components
- **Custom hooks** for reusable logic (prefix with `use`)
- **`React.memo`** for pure components that receive stable props
- **No business logic in components** — move to hooks
- **Material Design 3** must be respected in user-facing components

### .NET (Backend)

- **Domain project**: Pure POCOs with zero NuGet references
- **Result Pattern**: For domain flow control (not exceptions)
- **Tenant isolation**: All data access must be tenant-scoped
- **CQRS**: Query side via read models, command side via handlers

### Clean Architecture

```
src/
├── domain/          → Pure business rules (no external deps)
├── application/     → Use cases, handlers, interfaces
├── infrastructure/  → Persistence, external services
└── presentation/    → API controllers, endpoints
```

**Dependency rule**: Presentation → Application → Domain. Infrastructure is injected.

---

## 4. Testing Requirements

### Test Types and Targets

| Type | Tool | Location | Coverage Target |
|------|------|----------|-----------------|
| Unit | xUnit / Vitest | `*.test.cs` / `*.test.ts` | 80%+ |
| Integration | xUnit / Vitest + InMemory | `*.IntegrationTest.cs` | Key scenarios |
| Component | Testing Library | `*.test.tsx` | Key interactions |
| API Contract | WebAppFactory | `*.IntegrationTest/` | Endpoint validation |

### Test File Naming

- .NET: `*.test.cs` for unit, `*RepositoryTests.cs` for integration
- TypeScript: `*.test.ts` / `*.test.tsx`

### Running Tests

```bash
# Backend
cd src/apps/ums.api
dotnet test --configuration Release

# Frontend
cd src
npx nx run-many --target=test
```

### Tenant Isolation Testing

For authorization and identity changes, tenant isolation tests must pass:

```bash
dotnet test --filter "FullyQualifiedName~TenantIsolation"
```

---

## 5. Documentation Standards

### Bilingual Documentation Policy

All documentation must be maintained in **English** and **Spanish**:

| English | Spanish |
|---------|---------|
| `README.md` | `README.es.md` |
| `docs/governance/...` | `docs/governance/...-es/...` |
| `docs/architecture/...` | `docs/architecture/...-es/...` |
| `docs/blueprints/...` | `docs/blueprints-es/...` |

**Rule**: Both versions must be synchronized in content, technical precision, and clarity.

### Documentation Structure

```
docs/
├── MASTER_INDEX.md              # Main navigation
├── README.md                    # Documentation home
├── STANDARDS.md                 # Core standards
├── architecture/
│   ├── adrs/                    # Architecture Decision Records
│   ├── blueprints/              # Technical blueprints
│   └── blueprints-es/           # Spanish blueprints
└── ...
```

### ADR Standards

- **Naming**: `ADR-NNNN-title-slug.md`
- **Status Values**: `Proposed`, `Accepted`, `Deprecated`, `Superseded`
- **Required Sections**: Title, Status, Context, Decision, Consequences, Related ADRs

### Required GitHub Actions Checks

Documentation PRs must pass:
- Markdown lint (no style errors)
- Internal link validation (zero broken)
- External link validation (verified)
- Mermaid diagram syntax (valid)
- Bilingual sync (both versions aligned)
- ADR numbering validation (no conflicts)
- No sensitive information exposed

---

## 6. Security Requirements

### Blocking Security Gates

All PRs must pass these security checks:

| Check | Tool | Threshold |
|-------|------|-----------|
| Secrets Detection | GitLeaks/TruffleHog | Zero findings |
| Dependency Scan | actions/dependency-review-action | Zero critical/high |
| CodeQL | github/codeql-action | Zero critical/high |
| NuGet Audit | dotnet list --vulnerable | Zero vulnerabilities |
| npm Audit | npm audit | Zero critical/high |

### Security-Sensitive Changes

Changes to these areas require 2 approvals:
- Authentication/authorization code
- Tenant isolation logic
- Encryption/hashing code
- Session management
- API security configuration

### Tenant Context Enforcement

All data access must be tenant-scoped via `ITenantContext`:
- Application-layer filtering is primary
- SQL Server RLS is secondary failsafe
- Never bypass tenant context for convenience

---

## 7. Architecture Compliance

### Evolith Alignment

UMS inherits standards from [Evolith](https://github.com/beyondnetcode/evolith). When making architectural decisions:

1. **Reference Evolith** standards where applicable
2. **Document UMS-specific** patterns with ADRs
3. **Propose reusable** patterns back to Evolith via PR

### DDD Domain Boundaries

| Context | Entities | Boundary |
|---------|----------|----------|
| Identity | Tenant, UserAccount, Branch | Tenant-scoped |
| Authorization | SystemSuite, Role, Profile, Permission | Tenant-scoped |
| Configuration | FeatureFlag, TenantParameter | Tenant-scoped |
| Audit | AuditRecord | Tenant-scoped |

**Rule**: No cross-domain joins unless explicitly justified in ADR.

### Permission Model

Follows three-tier structure:
- **Modules**: High-level functional areas
- **Domain Resources**: Entity types within modules
- **System Actions**: CRUD + custom operations

### Logging Requirements

All logs must include where applicable:
- `TenantId`
- `UserId`
- `SessionTrackingId`
- `CorrelationId`

---

## 8. How to Update ADRs

### Creating a New ADR

1. **Create file**: `docs/architecture/adrs/ADR-NNNN-title-slug.md`
2. **Include required sections**:

```markdown
# ADR-NNNN: Title

## Status
Proposed | Accepted | Deprecated | Superseded

## Context
What is the issue we're seeing?

## Decision
What is the decision we've made?

## Consequences
What becomes easier or more difficult?

## Related ADRs
- [ADR-0001](./ADR-0001-title-slug.md)

## Notes
_Include Spanish translation note if applicable_
```

3. **Update registry**: Add to `docs/architecture/adrs/index.md`
4. **Update version log**: Include in documentation version log

### ADR Rules
- Numbering must be sequential (check existing ADRs)
- Status changes must be documented
- Superseded ADRs must reference replacement

---

## 9. How to Update Documentation Version Logs

For every release, update `docs/VERSION_LOG.md`:

```markdown
## Release v1.2.0 (2026-06-15)

| Field | Value |
|-------|-------|
| Version | 1.2.0 |
| Date | 2026-06-15 |
| Commit SHA | abc1234 |
| Author | architect@company.com |
| Summary | Tenant isolation improvements |

### Files Changed
- `docs/architecture/adrs/ADR-0071.md`
- `docs/governance/project/api-contracts.md`

### Validation Results
- Internal links: PASS
- External links: PASS (7 verified)
- Mermaid syntax: PASS (12 diagrams)
- Bilingual sync: PASS

### Known Issues
- None

### Approval Status
- [x] Architecture Team
- [x] Security Team
- [x] Product Owner
```

---

## 10. Release Process

### Release Branch Creation

```bash
# 1. Create release branch from develop
git checkout develop
git pull origin develop
git checkout -b release/v1.2.0

# 2. Update version numbers (semantic versioning)
# Update CHANGELOG.md
# Update documentation version log

# 3. Run release checklist
# See .github/templates/RELEASE_CHECKLIST.md

# 4. Create Git tag
git tag -a v1.2.0 -m "Release v1.2.0"

# 5. Merge to main
git checkout main
git merge release/v1.2.0 --ff-only
git push origin main --tags

# 6. Merge to develop
git checkout develop
git merge release/v1.2.0
git push origin develop
```

### Release Frequency
- Target: 1 release per month (X.Y.0)
- Patch releases for critical fixes as needed

---

## 11. How to Handle Hotfixes

### Hotfix Workflow

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/security-patch

# 2. Implement minimal fix
# Write tests
# Ensure CI passes

# 3. Get expedited review
# Security team + Tech lead approval

# 4. Merge to main (fast-forward)
git checkout main
git merge hotfix/security-patch --ff-only
git push origin main

# 5. Sync to develop
git checkout develop
git merge main
git push origin develop

# 6. Create patch tag if needed
git tag -a v1.2.1 -m "Hotfix: security patch"
git push origin v1.2.1
```

### Hotfix Rules
- Must be minimal and targeted
- Must include rollback plan
- CI must pass with all tests
- Requires expedited review

---

## 12. Reporting Issues

### Broken Links or Diagrams

1. **Check** if issue is already reported in GitHub Issues
2. **Create issue** with:
   - URL of broken link/diagram
   - Location in documentation
   - Suggested fix (if known)

### Security Vulnerabilities

1. **Do not** create public GitHub issue
2. **Email**: security@company.com with details
3. **Wait** for guidance before disclosure

### General Bugs

1. Create GitHub Issue with:
   - Description of bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details

---

## 13. Evolith Alignment

### What Belongs in UMS

- UMS-specific implementation details
- UMS-specific configuration
- UMS-specific documentation
- Integration tests for UMS features

### What Should Be Proposed to Evolith

- Reusable patterns discovered in UMS
- Cross-project standards
- Common architectural approaches
- Shared library improvements

### Proposal Process

1. Document pattern in UMS with ADR
2. Evaluate reusability
3. If reusable: Create proposal PR to Evolith
4. Reference Evolith PR in UMS ADR
5. Upon Evolith acceptance: Update UMS to reference Evolith

---

## 14. Definition of Done

### Documentation Changes

- [ ] English version complete and accurate
- [ ] Spanish version synchronized
- [ ] All links verified
- [ ] Diagrams render correctly
- [ ] No sensitive information exposed
- [ ] MASTER_INDEX updated if applicable
- [ ] Documentation version log updated

### Source Code Changes

- [ ] Code compiles with zero errors
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Security scan clean
- [ ] Code coverage maintained or improved
- [ ] Tenant isolation validated (if applicable)
- [ ] ADR updated if architecture changed
- [ ] CHANGELOG updated

### Production Release

- [ ] All pre-release checks passed
- [ ] Release checklist completed
- [ ] Git tag created
- [ ] GitHub Release created
- [ ] Documentation version log updated
- [ ] All required approvals obtained
- [ ] Rollback plan documented

---

## 15. Commit Message Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

| Type | Use For |
|------|---------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation changes |
| `style` | Formatting, no code change |
| `refactor` | Code change, no feature/fix |
| `test` | Adding or updating tests |
| `chore` | Maintenance, dependencies |
| `perf` | Performance improvement |
| `ci` | CI/CD changes |

### Examples

```
feat(identity): add user consent feature
fix(auth): resolve tenant isolation query filter
docs(adr): add ADR-0071 for new authorization model
test(api): add integration tests for user endpoint
refactor(core): extract tenant context interface
chore(deps): update React to v18.3
```

---

## 16. PR Checklist

### Before Submitting

- [ ] Code compiles with zero errors
- [ ] All tests passing locally
- [ ] Lint checks passing
- [ ] No `TODO` or `FIXME` left in code (unless documented)
- [ ] No hardcoded secrets
- [ ] ADRs updated if architecture changed
- [ ] CHANGELOG updated if user-facing change
- [ ] Documentation updated if needed
- [ ] Bilingual docs synchronized if changed

### PR Description

- [ ] Clear title following conventional commits
- [ ] Meaningful description with context
- [ ] Reference to linked issue
- [ ] List of changes made
- [ ] List of files changed
- [ ] Testing notes

### Review Readiness

- [ ] Self-review completed
- [ ] Code follows architecture boundaries
- [ ] No unnecessary dependencies added
- [ ] Tenant isolation enforced (if applicable)
- [ ] Security implications considered

---

## Getting Help

- **Architecture questions**: Check [ADRs](./docs/architecture/adrs/index.md)
- **Product questions**: Check [Product Vision](./docs/governance/product/product-vision.md)
- **Setup issues**: See [Quick Start](./README.md#quick-start)
- **BMAD Method**: See [AGENTS.md](./AGENTS.md)
- **Governance**: See [GOVERNANCE.md](./.github/GOVERNANCE.md)

---

## Related Documents

- [.github/GOVERNANCE.md](./.github/GOVERNANCE.md) - Full governance document
- [.github/templates/RELEASE_CHECKLIST.md](./.github/templates/RELEASE_CHECKLIST.md)
- [.github/templates/DOCUMENTATION_VERSION_LOG.md](./.github/templates/DOCUMENTATION_VERSION_LOG.md)
- [AGENTS.md](./AGENTS.md) - AI agent conventions

---

**Version**: 2.0.0
**Last Updated**: 2026-05-29
**Review Cycle**: Quarterly