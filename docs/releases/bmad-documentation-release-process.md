# BMAD Documentation Release Process Guide

**Purpose**: Guide for BMAD agents to understand, validate, and manage documentation releases in UMS.
**Applies To**: All AI coding agents, internal teams, and external providers working in this repository.
**BMAD Rules Compliance**: All 14 rules apply, especially R-03 (Encoding) and R-14 (No Emoji/Decoration).

---

## Overview

UMS (User Management System) is the applied reference implementation of Evolith enterprise standards. Documentation releases follow a structured process with automated validation gates to ensure quality, consistency, and traceability.

### Documentation Release Types

| Type | Versioning | Trigger | Example |
|------|------------|---------|---------|
| Initial Release | docs-vX.Y.Z | First documentation governance | docs-v1.0.0 |
| Feature Doc | docs-vX.Y.Z | New feature documentation | docs-v1.1.0 |
| Hotfix | docs-vX.Y.Z-patch | Critical documentation fix | docs-v1.0.1 |

---

## Section 1: Understanding the Documentation Process

### 1.1 Documentation Release Flow

```
Idea/Requirement
    ↓
Draft Documentation (feature/docs-* branch)
    ↓
Documentation Quality Pipeline (docs-quality.yml)
    ↓
PR Review & Approval
    ↓
Merge to main/develop
    ↓
Documentation Version Log Updated
    ↓
Git Tag Created (docs-vX.Y.Z)
    ↓
GitHub Release Published
```

### 1.2 Key Documentation Files

| File | Purpose | Location |
|------|---------|----------|
| README.md | Main entry point | root/ |
| MASTER_INDEX.md | Navigation hub | docs/ |
| GOVERNANCE.md | Complete governance policy | .github/ |
| CONTRIBUTING.md | Contributor guide | .github/ |
| VERSION_LOG.md | Release history | docs/releases/ |

### 1.3 Evolith-UMS Relationship

```
Evolith (Reusable Enterprise Standards) ← External reference
    ↓  Inherited via ADR and documentation
UMS  (Concrete Applied Implementation)
    ↓  Proposed patterns that are reusable
Evolith ← Via PR to beyondnetcode/evolith
```

**Critical**: UMS-specific rules stay local unless reusable and proposed back to Evolith.

---

## Section 2: BMAD Agent Documentation Review Protocol

### 2.1 Before Making Documentation Changes

BMAD agents MUST:

1. **Read GOVERNANCE.md** - Understand Git Flow, quality gates, and release process
2. **Check AGENTS.md** - Follow 14 BMAD rules (especially R-03, R-14)
3. **Review MASTER_INDEX.md** - Understand documentation structure
4. **Check ADR registry** - Understand existing architectural decisions

### 2.2 Documentation Change Requirements

Every documentation change must include:

| Requirement | Description | Verification |
|-------------|-------------|--------------|
| Bilingual Sync | English and Spanish versions | Automated check |
| Link Validation | All internal/external links work | docs-quality.yml |
| Diagram Syntax | Mermaid/PlantUML valid | Automated check |
| Evolith Alignment | UMS vs Evolith separation clear | Manual review |
| No Sensitive Data | No passwords/keys/tokens | Automated scan |
| No Obsolete Refs | No TODO/FIXME/DEMO | Automated check |

### 2.2.1 Complex Change Documentation Gate

When a code or architecture change is complex, cross-cutting, or evolutionary, the change must not be considered complete until the documentation impact has been checked and the affected artifacts are updated in both languages.

Examples of complex or evolutionary changes:
- Auth or authorization flow changes
- Tenant scope or multi-tenant isolation changes
- Public contract or SDK changes
- Configuration catalog or feature flag changes
- ADR-impacting architectural changes
- Sequence diagram or ERD changes

Required documentation output:
- Updated English and Spanish artifacts
- Updated diagrams, if the flow changed
- Updated ADR or functional story when the decision or business rule changed
- Updated traceability references and indexes
- A short rationale when no documentation change was needed

### 2.3 Checklist Before Committing Documentation

```bash
# 1. Bilingual check
python3 .github/scripts/check-bilingual-sync.py docs/

# 2. ADR validation
python3 .github/scripts/validate-adrs.py docs/architecture/adrs/

# 3. Mermaid syntax
python3 .github/scripts/validate-mermaid.py docs/

# 4. Link check (manual or automated)
# Verify all links resolve correctly

# 5. No sensitive data
# Ensure no secrets in documentation
```

---

## Section 3: Consistency Validation Rules

### 3.1 Internal Documentation Consistency

| Check | Method | Pass Criteria |
|-------|--------|---------------|
| Cross-references | Link checker | Zero broken links |
| MASTER_INDEX navigation | Manual/Automated | All links functional |
| ADR references | ADR validator | All references resolve |
| Diagram links | Mermaid validator | All diagrams valid |

### 3.2 External Documentation Consistency

| Check | Method | Pass Criteria |
|-------|--------|---------------|
| Evolith references | Manual check | Point to external Evolith GitHub |
| External URLs | Link checker | All resolve (HTTP 200) |
| API documentation | Manual check | Matches implementation |

### 3.3 Bilingual Consistency

| Check | Method | Pass Criteria |
|-------|--------|---------------|
| File existence | Script check | Both en/es versions exist |
| Content sync | Manual review | Same technical content |
| Navigation sync | Manual check | Same links in both |
| Section headers | Script check | Consistent structure |

### 3.4 Architecture Consistency

| Check | Method | Pass Criteria |
|-------|--------|---------------|
| Evolith inheritance | ADR review | UMS-specific vs Evolith clear |
| DDD boundaries | Architecture docs | No cross-domain joins |
| Tenant isolation | Code/docs review | Enforced consistently |

---

## Section 4: How to Detect and Report Issues

### 4.1 Broken Link Detection

**Automated (Preferred)**:
```bash
# Use markdown-link-check or docs-quality.yml
```

**Manual**:
1. Open documentation file
2. Click each link
3. Verify resolution

**Report Format**:
```markdown
## Broken Link Report

**File**: docs/path/file.md
**Line**: 42
**Link**: [Text](./target.md)
**Issue**: File does not exist
**Suggested Fix**: Update to correct path
```

### 4.2 Broken Diagram Detection

**Mermaid Syntax Check**:
```bash
python3 .github/scripts/validate-mermaid.py docs/
```

**Common Issues**:
- Unbalanced braces `{ }`
- Unbalanced parentheses `( )`
- Invalid diagram type
- Missing node IDs

**Report Format**:
```markdown
## Diagram Issue Report

**File**: docs/path/file.md
**Diagram**: Line 42-58
**Issue**: Unbalanced braces (3 open, 2 close)
**Suggested Fix**: Add closing brace after node B
```

### 4.3 ADR Consistency Issues

**Check for**:
- Duplicate ADR numbers
- Missing ADR numbers (gaps)
- Invalid status values
- Missing required sections

**Report Format**:
```markdown
## ADR Issue Report

**ADR**: ADR-0066
**Issue**: Number conflict with ADR-0042
**Resolution**: Renamed to ADR-0070 (see ADR-0070)
**Files Affected**: docs/architecture/adrs/
```

### 4.4 Bilingual Sync Issues

**Check for**:
- Missing Spanish translation
- Outdated translation
- Different technical content
- Broken navigation in Spanish version

**Report Format**:
```markdown
## Bilingual Sync Issue

**English File**: docs/architecture/overview.md
**Spanish File**: docs/architecture/overview.es.md
**Issue**: Missing section on authentication flow
**Impact**: Incomplete Spanish documentation
```

### 4.5 Blocking Issues (Must Fix)

| Issue Type | Example | Action |
|------------|---------|--------|
| Broken internal link | `./nonexistent.md` | Fix or remove |
| ADR number conflict | ADR-0066 exists twice | Renumber |
| Missing required section | ADR without Status | Add |
| Secret in documentation | `password=secret123` | Remove immediately |
| Broken Mermaid | Syntax error | Fix syntax |

### 4.6 Warning Issues (Should Fix)

| Issue Type | Example | Action |
|------------|---------|--------|
| External link timeout | Link > 5s | Replace or remove |
| Minor markdown lint | Trailing spaces | Fix |
| Bilingual wording | Slight variation | Align |
| Optional diagram | Could be clearer | Improve |

---

## Section 5: Release Readiness Decision

### 5.1 Pre-Release Checklist

Before creating a documentation release, verify:

**Documentation Quality**:
- [ ] All internal links work
- [ ] All external links verified
- [ ] All diagrams valid
- [ ] Bilingual sync complete
- [ ] No sensitive data
- [ ] No TODO/FIXME/DEMO
- [ ] MASTER_INDEX updated

**Architecture Consistency**:
- [ ] Evolith references correct
- [ ] UMS vs Evolith clear
- [ ] ADRs consistent
- [ ] DDD boundaries respected

**Pipeline Validation**:
- [ ] docs-quality.yml passes
- [ ] security.yml passes (if source changed)
- [ ] Release workflow documented

### 5.2 Decision Matrix

| Condition | Decision | Action |
|-----------|----------|--------|
| All blocking checks pass | ✅ Ready for release | Proceed |
| Any blocking check fails | ❌ NOT Ready | Fix issues first |
| Only warnings present | ⚠️ Conditionally Ready | Document known issues |

### 5.3 Release Approval Process

1. **Automated Validation**: All CI checks pass
2. **Manual Review**: Architecture lead reviews
3. **Security Review**: Security lead reviews (if applicable)
4. **Documentation Sign-off**: Documentation lead approves
5. **Release Creation**: Tag + GitHub Release

---

## Section 6: Version Log Update Process

### 6.1 When to Update Version Log

- Every documentation release (major/minor/patch)
- Major documentation restructure
- ADR additions or significant updates

### 6.2 Version Log Entry Structure

```markdown
## Release docs-vX.Y.Z (YYYY-MM-DD)

| Field | Value |
|-------|-------|
| Version | X.Y.Z |
| Date | YYYY-MM-DD |
| Commit SHA | abc1234 |
| Author | name@company.com |
| Summary | Brief description of changes |

### Files Changed
- `path/to/file1.md`
- `path/to/file2.md`

### ADRs Changed
- `ADR-NNNN-title.md`
- ...

### Validation Results
- Internal links: PASS/FAIL
- External links: PASS/FAIL
- Mermaid: PASS/FAIL
- Bilingual sync: PASS/FAIL

### Known Issues
- None / List issues

### Approval Status
- [ ] Architecture Team
- [ ] Security Team
- [ ] Product Owner
```

### 6.3 Semantic Versioning for Docs

```
docs-vMAJOR.MINOR.PATCH
        │      │      └── Patch: Hotfix, critical correction
        │      └───────── Minor: New documentation, feature docs
        └─────────────── Major: Governance restructure, major refactor
```

---

## Section 7: Git Tag and GitHub Release Process

### 7.1 Tag Creation

```bash
# 1. Ensure on main branch and up to date
git checkout main
git pull origin main

# 2. Create tag with annotation
git tag -a docs-v1.0.0 -m "Documentation Release 1.0.0 - Governance Framework"

# 3. Push tag to remote
git push origin docs-v1.0.0
```

### 7.2 GitHub Release Creation

1. **Navigate**: GitHub repo → Releases → "Draft a new release"
2. **Tag**: Select `docs-v1.0.0`
3. **Title**: "Documentation Release 1.0.0"
4. **Description**: Use version log summary
5. **Files**: Attach any release artifacts
6. **Publish**: Click "Publish release"

### 7.3 Release Documentation Content

```markdown
# Documentation Release 1.0.0

## Overview
Initial comprehensive documentation governance for UMS.

## What's New
- Complete Git Flow strategy
- 4 new GitHub Actions workflows
- Documentation quality pipeline
- Security pipeline
- BMAD agent guidance
- 21 ADRs with bilingual support

## Validation
- All internal links: PASS
- All external links: PASS
- Mermaid diagrams: PASS (12 validated)
- Bilingual sync: PASS

## Known Issues
- 38 integration test failures (source code, not docs)
- 8 high-severity npm vulnerabilities (deferred)

## Next Steps
- Fix Role seeder bug
- Update npm packages
- Add E2E tests

## Links
- [Version Log](./docs/releases/documentation-version-log.md)
- [Governance](./.github/GOVERNANCE.md)
- [CONTRIBUTING](./.github/CONTRIBUTING.md)
```

---

## Section 8: Quick Reference for BMAD Agents

### 8.1 Common Tasks

**Check documentation quality**:
```bash
python3 .github/scripts/validate-adrs.py docs/architecture/adrs/
python3 .github/scripts/validate-mermaid.py docs/
python3 .github/scripts/check-bilingual-sync.py docs/
```

**Create documentation branch**:
```bash
git checkout develop
git pull origin develop
git checkout -b docs/update-something
```

**Update version log**:
```bash
# Edit docs/releases/documentation-version-log.md
# Add new entry at top of Release History
```

### 8.2 Evolith Alignment Quick Check

1. **Is this UMS-specific?** → Document in UMS
2. **Is this reusable across projects?** → Propose to Evolith
3. **Does this reference Evolith?** → Ensure external GitHub link
4. **Is it a pattern?** → Evaluate for Evolith proposal

### 8.3 Quality Gate Severity Reference

**BLOCKING (Stop immediately)**:
- ❌ Build fails
- ❌ Tests fail
- ❌ Critical/High vulnerability found
- ❌ Secrets detected
- ❌ Broken internal documentation link
- ❌ ADR numbering conflict

**WARNING (Should fix but can proceed)**:
- ⚠️ Code coverage < 80%
- ⚠️ Minor markdown style issue
- ⚠️ External link timeout
- ⚠️ Low-risk dependency warning
- ⚠️ Bilingual wording mismatch

### 8.4 Release Decision Flow

```
Start
  ↓
Any blocking issues?
  ↓ Yes → ❌ Fix before release
  ↓ No
All warning issues documented?
  ↓ No → ⚠️ Document known issues
  ↓ Yes
Automated CI passes?
  ↓ No → ❌ Check CI logs
  ↓ Yes
Manual reviews complete?
  ↓ No → ⚠️ Complete reviews
  ↓ Yes
✅ Ready for release
```

---

## Section 9: Escalation and Issue Reporting

### 9.1 When to Escalate

| Situation | Escalate To |
|-----------|-------------|
| ADR numbering conflict | Architecture Lead |
| Broken Evolith reference | Architecture Lead |
| Security finding in docs | Security Team |
| Bilingual sync failure | Documentation Lead |
| GitHub Actions failure | DevSecOps |
| Blocking issue unclear | Architecture Lead |

### 9.2 Issue Report Template

```markdown
## Documentation Issue Report

**Date**: YYYY-MM-DD
**Reporter**: Name/Agent ID
**Severity**: Blocking/Warning

### Issue
Description of the issue.

### Impact
What is broken or at risk.

### Suggested Fix
If known, how to resolve.

### Evidence
Screenshots, logs, or references.
```

---

## Section 10: Related Documents

| Document | Location | Purpose |
|----------|----------|---------|
| GOVERNANCE.md | .github/ | Full governance policy |
| CONTRIBUTING.md | .github/ | Contributor guide |
| Version Log | docs/releases/ | Release history |
| Release Checklist | docs/releases/ | Pre-release validation |
| AGENTS.md | root/ | BMAD agent rules |

---

**Document Version**: 1.0.0
**Created**: 2026-05-29
**For**: BMAD agents, internal teams, external providers
**BMAD Rules**: R-03 (Encoding), R-14 (No Emoji) enforced
