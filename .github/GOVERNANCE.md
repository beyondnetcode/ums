# UMS Governance

**Version**: 1.0.0
**Effective Date**: 2026-05-29
**Owner**: UMS Architecture Team
**Review Cycle**: Quarterly

---

## Overview

UMS (User Management System) is the applied reference implementation that inherits standards from [Evolith](https://github.com/beyondnetcode/evolith). This governance document defines the complete strategy for Git Flow, quality gates, security, documentation, and release management.

### Evolith-UMS Relationship

```
Evolith (Reusable Enterprise Standards)
    ↓  ← Inherited via reference
UMS  (Concrete Applied Implementation)
    ↓  ← Patterns proposed back via PR
Evolith
```

**Key Principle**: UMS-specific rules must remain local unless they are reusable and should be proposed back to Evolith.

---

## 1. Git Flow Model

### 1.1 Branch Structure

```
main (protected)
  └── develop (protected)
        ├── feature/*           (e.g., feature/user-consent)
        ├── feature/docs-*      (e.g., feature/docs-api-contracts)
        ├── bugfix/*            (e.g., bugfix/tenant-isolation)
        ├── docs/*              (e.g., docs/update-adrs)
        └── release/vX.Y.Z      (e.g., release/v1.2.0)
  └── hotfix/*                  (e.g., hotfix/security-patch)
```

### 1.2 Branch Definitions

| Branch | Purpose | Lifetime | Protection |
|--------|---------|----------|------------|
| `main` | Production-ready code | Permanent | Required + Admin bypass |
| `develop` | Validated integration | Permanent | Required |
| `feature/*` | New features | Until merged | Optional |
| `feature/docs-*` | Documentation changes | Until merged | Optional |
| `bugfix/*` | Non-production fixes | Until merged | Optional |
| `release/vX.Y.Z` | Release candidates | Until tagged | Required |
| `hotfix/*` | Urgent production fixes | Until merged | Required |

### 1.3 Branch Naming Conventions

```bash
# Feature branches
feature/<context>-<short-description>
feature/identity-user-consent
feature/authorization-rbac-fine-tuning
feature/docs-api-contract-updates

# Bugfix branches
bugfix/<context>-<issue-description>
bugfix/tenant-isolation-query-filter
bugfix/audit-trail-null-reference

# Documentation branches
docs/<area>-<description>
docs/update-adrs-0070-alignment
docs/add-runbook-rb-05

# Release branches
release/vX.Y.Z
release/v1.2.0

# Hotfix branches
hotfix/<description>
hotfix/security-csp-header-fix
```

### 1.4 PR Policy

**Mandatory for all changes:**
- PRs must target `develop` for features/bugfixes/docs
- PRs must target `main` for releases and hotfixes
- PRs require minimum **1 approval** for features
- PRs require minimum **2 approvals** for security-sensitive changes
- PRs require **all checks passing** before merge
- PRs must include meaningful description with context
- PRs must reference linked issues

### 1.5 Merge Policy

| Source | Target | Merge Type | Validation |
|--------|--------|------------|------------|
| `feature/*` | `develop` | Squash/Rebase | CI + Reviews |
| `bugfix/*` | `develop` | Squash/Rebase | CI + Reviews |
| `docs/*` | `develop` | Squash | Docs QA |
| `release/vX.Y.Z` | `main` | Fast-forward | Release checks |
| `release/vX.Y.Z` | `develop` | Merge commit | Post-release sync |
| `hotfix/*` | `main` | Fast-forward | Hotfix validation |
| `hotfix/*` | `develop` | Merge commit | Hotfix sync |

### 1.6 Release Policy

1. **Release Branch Creation**:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b release/vX.Y.Z
   ```

2. **Release Process**:
   - Update version numbers (semantic versioning)
   - Update CHANGELOG.md
   - Update documentation version log
   - Run release checklist
   - Create Git tag `vX.Y.Z`
   - Create GitHub Release with release notes
   - Merge to `main` and `develop`

3. **Release Frequency**: Target 1 release per month (X.Y.0)

### 1.7 Hotfix Policy

1. **Hotfix Creation**:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b hotfix/<description>
   ```

2. **Hotfix Process**:
   - Fix must be minimal and targeted
   - CI must pass with all tests
   - Requires expedited review
   - Merged to `main` via fast-forward
   - Synced to `develop` via merge commit

3. **Emergency Approval**: CTO or security team can approve fast-track

---

## 2. Quality Gates

### 2.1 Blocking Gates (Must Pass)

| Gate | Tool | Threshold | Applies To |
|------|------|-----------|------------|
| Build | dotnet build / npm build | Zero errors | All PRs |
| Unit Tests | xUnit / Vitest | 100% pass | All PRs |
| Security Scan | CodeQL / npm audit | Zero critical/high | All PRs |
| Secrets Detection | GitLeaks / TruffleHog | Zero findings | All PRs |
| Tenant Isolation | Custom tests | 100% pass | Auth/Identity PRs |
| Documentation Links | markdown-link-check | Zero broken | Docs PRs |
| ADR Numbering | Custom validator | Zero conflicts | ADR changes |
| Required Checks | GitHub status | All passing | All PRs |

### 2.2 Warning Gates (Non-Blocking)

| Gate | Tool | Threshold | Applies To |
|------|------|-----------|------------|
| Code Coverage | Coverage.py / Coverlet | < 80% | All PRs |
| Lint Warnings | ESLint / Roslyn | > 0 warnings | All PRs |
| External Links | linkchecker | Timeout > 5s | All PRs |
| Dependency Age | npm outdated | Major version behind | All PRs |
| Documentation Style | markdownlint | > 0 errors | Docs PRs |

### 2.3 Quality Metrics

| Metric | Target | Current |
|--------|--------|---------|
| Build Time (Backend) | < 2 min | ~1 min |
| Build Time (Frontend) | < 3 min | ~2.5 min |
| Test Execution | < 5 min | ~4 min |
| Code Coverage | > 80% | 76% |
| Critical Vulnerabilities | 0 | 0 |
| High Vulnerabilities | 0 | 8* |

*8 high vulnerabilities in npm packages (deferred - require breaking changes)

---

## 3. Documentation Standards

### 3.1 Bilingual Documentation Policy

All documentation must be maintained in **English** and **Spanish**:

| English | Spanish |
|---------|---------|
| `README.md` | `README.es.md` |
| `docs/governance/...` | `docs/governance/...-es/...` |
| `docs/architecture/...` | `docs/architecture/...-es/...` |
| `docs/blueprints/...` | `docs/blueprints-es/...` |

**Rule**: Both versions must be synchronized in content, technical precision, and clarity.

### 3.2 Documentation Structure

```
docs/
├── MASTER_INDEX.md              # Main navigation
├── README.md                    # Documentation home
├── README.es.md                 # Spanish home
├── STANDARDS.md                 # Core standards reference
├── STANDARDS.es.md              # Spanish standards
├── architecture/
│   ├── overview.md             # Architecture overview
│   ├── overview.es.md          # Spanish overview
│   ├── adrs/                   # Architecture Decision Records
│   │   ├── index.md            # ADR registry
│   │   ├── ADR-0001-*.md       # Individual ADRs
│   │   └── ADR-0066*.md        # ADR numbering
│   ├── blueprints/             # Technical blueprints
│   ├── blueprints-es/          # Spanish blueprints
│   ├── api-dotnet/             # .NET API reference
│   └── web-frontend/           # React frontend reference
├── domain/                     # Domain documentation
├── governance/                 # Governance documents
│   ├── construction/           # Construction guides
│   ├── product/                # Product documentation
│   └── project/                # Project management
└── qa/                         # QA reports and evidences
```

### 3.3 ADR Standards

- **Naming**: `ADR-NNNN-title-slug.md`
- **Status Values**: `Proposed`, `Accepted`, `Deprecated`, `Superseded`
- **Required Sections**:
  - Title
  - Status
  - Context
  - Decision
  - Consequences
  - Related ADRs
  - Notes (Spanish translation note if applicable)

### 3.4 Documentation Version Log

Every release must update `docs/VERSION_LOG.md`:

```markdown
## Release v1.2.0 (2026-06-15)

| Field | Value |
|-------|-------|
| Version | 1.2.0 |
| Date | 2026-06-15 |
| Commit SHA | abc1234 |
| Author | architect@company.com |
| Summary | Tenant isolation improvements, new RBAC features |

### Files Changed
- `docs/architecture/adrs/ADR-0071.md`
- `docs/governance/project/api-contracts.md`

### Diagrams Changed
- `docs/architecture/blueprints/auth-flow.md`

### Validation Result
- All internal links: PASS
- All external links: PASS (7 verified)
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

## 4. Security Gates

### 4.1 Backend (.NET)

| Check | Tool | Frequency | Blocking |
|-------|------|-----------|----------|
| Restore | dotnet restore | Every PR | Yes |
| Build | dotnet build | Every PR | Yes |
| Test | dotnet test | Every PR | Yes |
| NuGet Audit | dotnet list --vulnerable | Every PR | Yes |
| CodeQL | github/codeql-action | Every PR | Yes |
| Dependency Review | actions/dependency-review-action | Every PR | Yes |
| Secrets Scanning | GitLeaks/TruffleHog | Every PR | Yes |

### 4.2 Frontend (React)

| Check | Tool | Frequency | Blocking |
|-------|------|-----------|----------|
| Install | npm ci | Every PR | Yes |
| TypeScript | tsc --noEmit | Every PR | Yes |
| Lint | eslint | Every PR | Yes |
| Test | vitest run | Every PR | Yes |
| npm Audit | npm audit | Every PR | Yes |
| Build | npm run build | Every PR | Yes |

### 4.3 Container & Configuration

| Check | Tool | Frequency | Blocking |
|-------|------|-----------|----------|
| Dockerfile Scan | Hadolint/Trivy | Every PR | Yes |
| License Scan | FOSSA/license检查 | Every PR | Yes |
| Hardcoded Secrets | GitLeaks | Every PR | Yes |
| CORS Config | Custom validator | Every PR | Yes |

---

## 5. Architecture Compliance

### 5.1 Evolith Alignment

UMS must maintain alignment with Evolith standards:

1. **Reference**: All ADRs must reference Evolith standards where applicable
2. **Proposal**: UMS-specific patterns that are reusable must be proposed to Evolith via PR
3. **Separation**: UMS-specific implementation must not be confused with Evolith standards

### 5.2 Domain Boundaries

| Context | Entities | Isolation |
|---------|----------|-----------|
| Identity | Tenant, UserAccount, Branch | Tenant-scoped |
| Authorization | SystemSuite, Role, Profile, Permission | Tenant-scoped |
| Configuration | FeatureFlag, TenantParameter | Tenant-scoped |
| Audit | AuditRecord | Tenant-scoped |

### 5.3 Tenant Enforcement

- **Application-layer**: Primary tenant filtering via `ITenantContext`
- **Infrastructure**: SQL Server RLS as secondary failsafe
- **Query Filters**: EF Core global query filters per entity

### 5.4 Permission Model

Follows three-tier structure:
- **Modules**: High-level functional areas
- **Domain Resources**: Entity types within modules
- **System Actions**: CRUD + custom operations

### 5.5 Logging Requirements

All logs must include where applicable:
- `TenantId`
- `UserId`
- `SessionTrackingId`
- `CorrelationId`

---

## 6. Release Governance

### 6.1 Semantic Versioning

```
vMAJOR.MINOR.PATCH
   │      │      └── Patch: Bug fixes, hotfixes
   │      └───────── Minor: New features, backward compatible
   └─────────────── Major: Breaking changes
```

### 6.2 Version Artifacts

| Artifact | Format | Description |
|----------|--------|-------------|
| Application | `vX.Y.Z` | Source code tag |
| Documentation | `docs-vX.Y.Z` | Docs-only tag (if released separately) |
| Shell Libraries | `shell-vX.Y.Z` | Library version tag |

### 6.3 Release Checklist

Every release must complete `RELEASE_CHECKLIST.md`:

```markdown
## Release v1.2.0 - Checklist

### Pre-Release
- [ ] All tests passing
- [ ] Security scan clean
- [ ] Code coverage > 80%
- [ ] Documentation updated
- [ ] Changelog updated
- [ ] Version numbers bumped

### Validation
- [ ] Integration tests pass
- [ ] E2E tests pass (if applicable)
- [ ] Performance benchmarks pass
- [ ] Migration scripts tested

### Approval
- [ ] Architecture team review
- [ ] Security team review
- [ ] Product owner sign-off

### Release
- [ ] Git tag created
- [ ] GitHub Release created
- [ ] Documentation version log updated
- [ ] Announcement sent
```

### 6.4 Rollback Process

1. Identify issue in production
2. Assess fix vs rollback decision
3. If rollback: `git revert` or `git checkout main~1`
4. Create hotfix branch if needed
5. Test rollback in staging
6. Deploy rollback
7. Document incident

---

## 7. GitHub Actions Workflows

### 7.1 Workflow Overview

| Workflow | Trigger | Purpose | Blocking |
|----------|---------|---------|----------|
| `ci.yml` | PR/Push to main,develop | Build, lint, test | Yes |
| `docs-quality.yml` | PR to docs/**,**.md | Doc validation | Yes |
| `security.yml` | PR/Push | Security scanning | Yes |
| `release-candidate.yml` | PR to release/* | Release validation | Yes |
| `hotfix.yml` | Push to hotfix/* | Hotfix validation | Yes |
| `dependency-review.yml` | PR | Dependency analysis | Yes |

### 7.2 Required Status Checks

All PRs must have these status checks passing:

```
✅ build (dotnet/npm build)
✅ test (all tests passing)
✅ lint (zero errors)
✅ security (CodeQL/no vulnerabilities)
✅ docs-quality (for doc changes)
```

---

## 8. Severity Classification

### Blocking (Must Fix Before Merge)

- Build failure
- Test failure
- Critical/High security vulnerability
- Secrets detected in code
- Broken internal documentation links
- Broken critical diagrams (Mermaid/PlantUML)
- ADR numbering conflicts
- Tenant isolation tests failing
- Authorization tests failing
- Required documentation missing in release

### Warning (Should Fix But Non-Blocking)

- Minor markdown lint errors
- Non-critical external link timeout
- Optional diagram improvements
- Low-risk dependency warnings
- Minor bilingual wording mismatch
- Code coverage below 80% (but > 60%)

---

## 9. Evolith Interaction

### 9.1 What Belongs in UMS

- UMS-specific implementation details
- UMS-specific configuration
- UMS-specific documentation
- Integration tests for UMS features

### 9.2 What Should Be Proposed to Evolith

- Reusable patterns discovered in UMS
- Cross-project standards
- Common architectural approaches
- Shared library improvements

### 9.3 Proposal Process

1. Document pattern in UMS with ADR
2. Evaluate reusability
3. If reusable: Create proposal PR to Evolith
4. Reference Evolith PR in UMS ADR
5. Upon Evolith acceptance: Update UMS to reference Evolith

---

## 10. Definition of Done

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

## Appendix A: File Templates

- [RELEASE_CHECKLIST.md](./templates/RELEASE_CHECKLIST.md)
- [DOCUMENTATION_VERSION_LOG.md](./templates/DOCUMENTATION_VERSION_LOG.md)
- [HOTFIX_TEMPLATE.md](./templates/HOTFIX_TEMPLATE.md)

## Appendix B: Related Documents

- [CONTRIBUTING.md](./CONTRIBUTING.md)
- [AGENTS.md](../AGENTS.md)
- [docs/architecture/adrs/index.md](../docs/architecture/adrs/index.md)