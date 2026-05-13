# 🏛️ Global Engineering Standards & Developer Guidelines (BMAD Manifesto)

## 1. 🌟 Core Engineering Principles (Mandatory)
All code, wrappers, and architectural designs within this monorepo **MUST** strictly adhere to the following principles. Code reviews will reject any Pull Request violating these foundations:

*   **SOLID**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion.
*   **DRY (Don't Repeat Yourself)**: Eliminate unnecessary duplication. Consolidate shared logic into utilities or shared-kernel libraries.
*   **KISS (Keep It Simple, Stupid)**: Avoid over-engineering. Write code that is easy to read, understand, and debug.
*   **YAGNI (You Aren't Gonna Need It)**: Do not add functionality, abstractions, or tools until they are strictly necessary.
*   **SoC (Separation of Concerns)**: Keep layers completely isolated. A controller must not write business logic; a use case must not execute raw SQL.
*   **Clean Code & Clean Architecture**: Maintain strict boundaries (Adapters vs. Core). Ensure code readability and intent-revealing names.
*   **Secure by Design & OWASP**: Validate all inputs (DTOs), sanitize outputs, enforce RBAC natively, and prevent SQL/NoSQL injections by default.

---

## 2. 🛡️ Domain-Driven Design (DDD): Optional & Pragmatic
While our architecture supports tactical and strategic DDD:
**DDD is strictly OPTIONAL**. 
It shall only be used when it adds tangible value to a complex business domain. It must **not** be considered a mandatory or restrictive straitjacket for the architecture. For simple CRUD (Create, Read, Update, Delete) operations, standard Hexagonal Use Cases and Data Mappers are more than sufficient. Over-applying DDD to simple entities is considered an anti-pattern (Over-engineering).

---

## 3. 🚫 Architectural & Code Anti-Patterns (Strictly Forbidden)
To guarantee high maintainability and low technical debt, the following practices are explicitly banned:
*   **High Coupling**: Direct dependencies on concrete third-party tools within the Core. (Violates DIP).
*   **God Classes / Magic Modules**: Classes that handle routing, validation, business logic, and database saving simultaneously.
*   **Vendor Lock-In without Adapters**: Hardcoding SDKs (e.g., AWS SDK, Unleash, Redis) outside of isolated Infrastructure Ports/Adapters.
*   **Spaghetti Code & Callback Hells**: Lack of structured async/await or functional monads (like the Result Pattern).
*   **Ignored Exceptions**: Catching errors without properly logging them or returning generic 500s without trace IDs.

---

## 4. ⚙️ Technical Governance & Enforcement Mechanisms
Human review is flawed. We rely on **Automated Enforcement** to ensure these principles are sustainable over time within the BMAD framework:

1.  **Linters & Architectural Rules**: 
    *   `eslint-plugin-boundaries` will automatically fail the build if a developer imports an outer layer (infrastructure) into an inner layer (core).
2.  **Static Code Analysis**: 
    *   `eslint-plugin-sonarjs` runs locally to detect cognitive complexity, cognitive debt, and code smells before a commit is even created.
3.  **Quality Gates & CI/CD Validations**: 
    *   GitHub Actions will block merging if tests fail or if the build breaks.
4.  **Automated Testing & Coverage Thresholds**: 
    *   Unit and E2E tests are mandatory. SonarQube/Jest will enforce a hard threshold of **>70% Code Coverage**.
5.  **Dependency & Security Scanning**: 
    *   Mandatory `npm audit --audit-level=high` in CI/CD pipelines to block vulnerable dependencies. GitHub CodeQL runs asynchronously to detect OWASP vulnerabilities.
6.  **Coding Standards Enforcement**: 
    *   Prettier and ESLint are enforced via Husky `pre-commit` hooks. Code that is not formatted correctly cannot be committed.

---

## 5. 🎯 Decision Priority Matrix
Whenever a technical decision is made (e.g., writing a new ADR, choosing a library, or designing a module), the architect and developers must prioritize the following attributes, in order:
1.  **Mantenibilidad** (Maintainability)
2.  **Escalabilidad** (Scalability)
3.  **Extensibilidad** (Extensibility)
4.  **Desacoplamiento** (Decoupling)
5.  **Observabilidad** (Observability)
6.  **Seguridad** (Security)
7.  **Resiliencia** (Resilience)
8.  **Testabilidad** (Testability)
9.  **Performance** (Performance)
10. **Claridad Arquitectónica** (Architectural Clarity)

---

## 6. 📝 Pull Request Quality Checklist
Before submitting a PR, developers must verify:
- [ ] No outer-layer logic is leaked into the Domain.
- [ ] Cross-cutting concerns (Logging, Caching) use Decorators or Ports (No hardcoded tool logic in the core).
- [ ] DDD was only used if the domain complexity justified it; otherwise, standard Clean Architecture was used.
- [ ] Test coverage for the new feature is >70%.
- [ ] Local `npm run lint` and `npm run test` pass successfully.
