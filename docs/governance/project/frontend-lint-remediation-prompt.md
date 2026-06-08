# Frontend Lint Remediation Prompt

Use this prompt to start a focused remediation thread for the current `app-web` lint gate.

```text
You are working in /Users/beyondnet/Source/ums. Follow AGENTS.md, BMAD rules, and the frontend audit playbook.

Goal:
Make `npm run lint --workspace app-web` pass without disabling rules, deleting tests, weakening TypeScript, or bypassing React hook checks.

Context from the audit:
- The frontend currently builds successfully.
- The lint gate fails with 315 errors and 29 warnings.
- The error categories include no-explicit-any, unused imports/vars, no-console, Prettier formatting, React hook rules, refs during render, synchronous setState in effects, and conditional hook calls.
- High-risk files called out by the audit include:
  - src/apps/ums.web-app/src/application/hooks/use-notified-mutation.ts
  - src/apps/ums.web-app/src/application/hooks/use-focus-trap.ts
  - src/apps/ums.web-app/src/application/hooks/use-local-overrides.ts
  - src/apps/ums.web-app/src/application/authorization/hooks/use-system-suite-dashboard.ts
  - src/apps/ums.web-app/src/application/identity/hooks/use-tenant-dashboard.ts
  - src/apps/ums.web-app/src/application/identity/hooks/use-delegation-dashboard.ts

Execution rules:
1. Run all technical commands from /Users/beyondnet/Source/ums/src.
2. Do not disable ESLint rules globally or locally unless an existing ADR explicitly allows it.
3. Replace `any` with precise local types, `unknown`, or generics.
4. Keep shared components domain-agnostic.
5. Fix React hook violations by changing control flow, derived state, or effects, not by suppressing rules.
6. Keep UI behavior unchanged unless a test proves the existing behavior is broken.
7. After each batch, run `npm run lint --workspace app-web`.
8. When lint passes, run `npm run build --workspace app-web` and `npm run test --workspace app-web`.

Deliverable:
Provide a concise summary of changed files, remaining risk, and verification output.
```
