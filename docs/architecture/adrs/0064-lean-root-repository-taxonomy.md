# ADR-0064: Lean Root Repository Taxonomy

**Status:** Accepted  
**Date:** 2026-05-24  
**Decision Owner:** Architecture  

---

## Context

Enterprise monorepos frequently suffer from root directory bloat. Over time, test directories (`tests/`), scripts (`scripts/`), configuration files (`NuGet.Config`), and scattered knowledge articles (`knowledge/`) accumulate in the root workspace. This creates cognitive overload for new engineers and obscures the primary entry points (`README.md`, `docs/MASTER_INDEX.md`). 

In UMS, the lack of a strict root taxonomy allowed technical and governance concerns to intermingle at the top level, creating a suboptimal developer experience and pushing critical documentation below the fold in GitHub's web interface.

## Decision

**Adopt the "Lean Root" (or Clean Root) architectural pattern, enforcing a strict binary dichotomy at the repository root: the Technical Engine (`src/`) versus the Knowledge Hub (`docs/`).**

1. **`src/` (Technical Engine):** All runnable code, tests, load testing scripts, database migrations, CI/CD utility scripts, and language-specific configurations (e.g., `NuGet.Config`) MUST reside within `src/` or its subdirectories.
2. **`docs/` (Knowledge Hub):** All enterprise documentation, architectural blueprints, requirements, and translated READMES (`README.es.md`) MUST reside within `docs/`.
3. **BMAD Exceptions:** AI-agent instructions (`AGENTS.md`) and standard open-source files (`CHANGELOG.md`, `LICENSE`, `README.md`) are the ONLY exceptions permitted to remain at the root, complying strictly with the BMAD methodology structural standards.

## Consequences

### Positive

- **Reduced Cognitive Load:** The root directory is instantly scannable. Engineers know exactly where to go for code (`src/`) vs. theory (`docs/`).
- **Improved Discoverability:** The repository's `README.md` and navigation links are prominently displayed "above the fold" on GitHub without requiring scrolling past dozens of folders.
- **Architectural Clarity:** Reinforces bounded contexts not just in code, but in repository management.

### Trade-offs

- Developers executing scripts or tests that were previously run from the root must now change their working directory to `src/` or update their command paths.
- Certain configuration files (like `NuGet.Config`) must be explicitly targeted or relied upon via standard inheritance mechanisms from within `src/`.

## Compliance

- The CI pipeline's structural linter and the BMAD AI-Agents will enforce this dichotomy by flagging any new top-level directories that violate the `src/` vs `docs/` separation.
