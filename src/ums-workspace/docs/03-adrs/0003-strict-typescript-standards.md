# ADR 0003: Strict TypeScript Standards and SonarJS Static Analysis

## Status
Accepted

## Date
2026-05-08

## Context
Code quality, security (OWASP Top 10), and type safety are critical pillars for UMS. Permitting the `any` type leads to runtime exceptions, makes code reviews difficult, and degrades our compiler's safety nets. Additionally, we need a way to detect cognitive complexity, bugs, and security hotspots early, without incurring external license costs (such as SonarCloud's paid tier for private repositories).

## Decision
We decided to enforce strict TypeScript and static analysis rules:
1. Ban the use of `any` across all workspaces by enforcing `@typescript-eslint/no-explicit-any` and its safety sub-rules.
2. Force the use of `interface` over `type` aliases for object contracts via `@typescript-eslint/consistent-type-definitions`, optimizing compiler execution speed and enabling declaration merging.
3. Integrate **`eslint-plugin-sonarjs`** directly into the root `.eslintrc.js` to run Sonar's official code quality, security hotspot, and cognitive complexity analysis rules at $0 cost, both locally and in CI, without requiring cloud secrets or paid accounts.

## Consequences

### Positive (Pros)
* **Robust Type Safety**: Eliminating `any` prevents silent runtime crashes and guarantees reliable data shapes across layers.
* **$0 Cost Sonar Analysis**: Developers receive real-time feedback on code complexity, duplicate code blocks, and bugs directly inside VS Code and pre-commit hooks, with zero infrastructure costs.
* **Better Performance**: Interfaces are indexed and cached more efficiently by the TypeScript compiler (`tsc`) compared to intersections of `type` aliases.

### Negative (Cons)
* Developers must write explicit type casts (e.g., using `unknown` and custom typings) for external libraries or catch blocks, slightly increasing initial development time.
* Minor execution overhead during `eslint` commands (fully neutralized by using `lint-staged`).
