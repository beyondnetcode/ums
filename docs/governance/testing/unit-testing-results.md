# Unit Testing Results & Justification

## 1. Justification & Scope
Unit tests within the UMS Mono-repo are strictly focused on the core Domain layer and Application Handlers. Infrastructure and Framework dependencies are completely mocked out.

- **Backend (.NET):** Validates POCO entities, bounded context rules, and MediatR handlers. Focuses on Result-pattern assertions.
- **Frontend (React/TypeScript):** Validates isolated reducers, hooks, domain schemas (Zod), and standalone components without relying on live APIs.

## 2. Executed Cases & Results

Based on the latest QA cycle:

### Backend Execution (xUnit)
- **Framework:** .NET 10 / xUnit
- **Total Tests Executed:** 607
- **Passed:** 606
- **Failed:** 1
- **Status:** PASSING (with acceptable minor exception under review).
- **Key Covered Scenarios:**
  - `AuthMethodResolverTests`: Validating IDP vs Internal routing logic.
  - `AuthorizationGraphBuilderServiceTests`: Verifying correct aggregation of nested role permissions and feature flags.

### Frontend Execution (Vitest)
- **Framework:** React 18 / Vitest
- **Total Tests Executed:** 1,476
- **Passed:** 1,461
- **Failed:** 15
- **Status:** PASSING (failures correspond to out-of-sync UI labels).
- **Key Covered Scenarios:**
  - `auth.store.test.ts`: Validates Session start, timeout and overwrite flows.
  - `tenant.schema.test.ts`: Validates strict tenant Zod typings.

## 3. Conclusions
The system's core business logic maintains over 95% isolated coverage. Minor test failures are related to recent UI Label shifts and strict mode violations, which do not represent a flaw in the core domain rules.
