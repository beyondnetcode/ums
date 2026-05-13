> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versión original (Inglés) y está programado para traducción oficial en la hoja de ruta.

# ADR 0018: Testing Pyramid and Automated Quality Gates Strategy

## Status
Approved

## Date
2026-05-08

## Context
While the CI/CD pipeline runs existing tests, ensuring long-term software resilience requires a formalized and uncompromising testing strategy. Critical logistics operations cannot afford regressions deployed into production due to lack of test coverage.

## Decision
We will enforce a strict Testing Pyramid coupled with automated CI Quality Gates:

1. **Unit Testing (Domain & Use Cases)**: The vast majority of tests will be ultra-fast Unit Tests targeting pure Hexagonal Domain entities and Application Use Cases using Jest.
2. **Integration Testing (Adapters)**: Secondary focus on testing database repositories (TypeORM) and external HTTP client adapters against test-containers or mocked APIs.
3. **E2E Testing (HTTP Entry Points)**: A dedicated suite of End-to-End tests using `Supertest` to validate full HTTP Request -> Controller -> UseCase -> DB -> Response flows.
4. **Hard Quality Gates**: SonarQube and CI configurations will be set to explicitly reject any Pull Request that drops overall code coverage below **70%** or introduces new code with 0% coverage.

## Consequences
* **Pros**: Guarantees long-term maintainability. Drastically reduces production bugs and provides a safety net for massive refactorings.
* **Cons**: Slows down initial feature development speed as developers must allocate time for writing comprehensive tests.

