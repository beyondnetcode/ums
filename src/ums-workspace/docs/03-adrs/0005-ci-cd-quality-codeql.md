# ADR 0005: Zero-Cost Security and CI Pipeline with CodeQL

## Status
Accepted

## Date
2026-05-08

## Context
To achieve total quality control and prevent vulnerable, uncompiled, or non-compliant code from entering `main`, we need an automated cloud gatekeeper (CI/CD pipeline). This pipeline must perform strict security analysis (identifying SQL injections, XSS, or hardcoded secrets) without incurring additional software subscription costs.

## Decision
We decided to implement a highly secure, $0 cost integration pipeline:
1. Create a **GitHub Actions** workflow (`ci.yml`) triggered automatically on every Pull Request to `main`.
2. Enforce strict build steps:
   * Perform exact version locking via `npm ci`.
   * Run global Nx linter checks (`npx nx run-many --target=lint`) to catch ESLint, SonarJS, and layer boundary errors.
   * Run concurrent unit tests with coverage enabled (`npx nx run-many --target=test --code-coverage`).
   * Compile both backend and frontend (`npx nx run-many --target=build`) to ensure no TypeScript compilation regressions.
3. Integrate **GitHub CodeQL Analysis** natively inside the workflow, running advanced semantic security scanning for TypeScript and JavaScript at $0 cost (free-tier for public repos and built-in GitHub features).

## Consequences

### Positive (Pros)
* **Zero Escaped Bugs**: Non-compiling or untested code cannot be merged into `main`.
* **Proactive Security**: CodeQL automatically scans and flags vulnerabilities (like SQL injections or weak cryptos) before code enters production, ensuring OWASP Top 10 compliance.
* **$0 Infrastructure Cost**: Leverages GitHub's native runners and free security scans for modern monorepos.

### Negative (Cons)
* CI builds can take 2-4 minutes to run complete static analysis and compilation (highly optimized by caching dependencies).
