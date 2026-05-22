# Contributing to UMS

Thank you for contributing to the User Management System (UMS). This guide covers the standards and workflows expected for all contributors.

---

## Development Workflow

### 1. Branch Strategy

```
main (protected)
  └── feature/your-feature-name
  └── fix/issue-description
  └── docs/update-description
```

### 2. Before You Code

1. Read the relevant [ADR](./docs/architecture/adrs/index.md) for the area you're working on.
2. Check the [AGENTS.md](./AGENTS.md) for project conventions.
3. Run `npm install` from `src/` to ensure dependencies are current.

### 3. Making Changes

- **Frontend**: Work in `src/apps/ums.web-app/`
- **Backend**: Work in `src/apps/ums.api-dotnet/`
- **Documentation**: Work in `docs/`

### 4. Before Committing

Run all quality gates:

```bash
cd src/apps/ums.web-app

# TypeScript type check
npx tsc --noEmit

# ESLint (zero errors required)
npx eslint "src/**/*.{ts,tsx}"

# Tests (all must pass)
npx vitest run
```

---

## Code Standards

### TypeScript

- **Strict mode enabled**: `noUnusedLocals`, `noUnusedParameters`, `noFallthroughCasesInSwitch`
- **No `any`**: Use proper types or `unknown` with narrowing
- **Explicit return types** for public APIs
- **No non-null assertions** (`!`) — use optional chaining or guards

### React

- **Functional components only**: No class components
- **Custom hooks** for reusable logic (prefix with `use`)
- **`React.memo`** for pure components that receive stable props
- **No business logic in components** — move to hooks

### Zustand Stores

- **Single responsibility**: One store per concern
- **No DOM manipulation** in stores — handle in components
- **Use `persist` middleware** only when state should survive refresh
- **Dev tools isolated**: `devTools.store` is development-only

### Clean Architecture

```
domain/          → Pure business rules (no external deps)
application/     → Use cases, hooks, stores
infrastructure/  → HTTP clients, external services
presentation/    → Components, screens, layouts
```

**Dependency rule**: Presentation → Application → Domain. Infrastructure is injected.

---

## Testing

### Test Types

| Type | Tool | Location | Coverage Target |
|---|---|---|---|
| Unit | Vitest | `*.test.ts` / `*.test.tsx` | 80%+ |
| Component | Testing Library | `*.test.tsx` | Key interactions |
| Integration | Vitest + msw | `*.test.ts` | API contracts |
| E2E | Playwright (future) | `*.e2e.ts` | Critical paths |

### Writing Tests

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useMyHook } from './my-hook';

describe('useMyHook', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('does what it says', () => {
    const { result } = renderHook(() => useMyHook());
    expect(result.current.value).toBe('expected');
  });
});
```

### Test File Naming

- Store tests: `*.store.test.ts`
- Hook tests: `use-*.test.ts`
- Component tests: `*.test.tsx`
- Integration tests: `*.test.ts` (in `infrastructure/`)

---

## Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add tenant branding panel
fix: resolve stale closure in useFormFields
docs: expand README with architecture overview
test: add notification.store unit tests
refactor: extract useFocusTrap hook
chore: update ESLint rules
```

---

## Pull Requests

### Checklist

- [ ] TypeScript compiles with zero errors
- [ ] ESLint passes with zero errors
- [ ] All tests pass
- [ ] Code follows Clean Architecture boundaries
- [ ] No `console.log` in production code (use `logger.ts`)
- [ ] No `any` types
- [ ] No non-null assertions (`!`)
- [ ] ADRs updated if architecture changed
- [ ] README/docs updated if user-facing change

### Review Criteria

1. **Correctness**: Does it work as intended?
2. **Architecture**: Does it follow Clean Architecture?
3. **Testing**: Are there tests for new logic?
4. **Performance**: Any unnecessary re-renders or memory leaks?
5. **Accessibility**: ARIA attributes, keyboard navigation, screen reader support?

---

## Bilingual Documentation

All documentation must be maintained in **English** and **Spanish**:

| English | Spanish |
|---|---|
| `README.md` | `README.es.md` |
| `docs/governance/...` | `docs/governance/...-es/` |
| `docs/architecture/...` | `docs/architecture/...-es/` |

When updating documentation, ensure both versions are synchronized in content, technical precision, and clarity.

---

## Getting Help

- **Architecture questions**: Check [ADRs](./docs/architecture/adrs/index.md) or the [Architecture Portal](./docs/architecture/index.md)
- **Product questions**: Check the [Product Vision](./docs/governance/product/product-vision.md)
- **Setup issues**: See [Quick Start](./README.md#quick-start-engine-room)
- **BMAD Method**: See [AGENTS.md](./AGENTS.md)
- **BMAD Project Skills**: Use the local project playbooks in [`.harness/playbooks/`](./.harness/playbooks/README.md) for documentation audits, API audits, frontend audits, and modular-monolith evolution reviews.
